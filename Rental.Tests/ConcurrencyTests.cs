using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MyProject.Core.DTOs;
using MyProject.Core.Exceptions;
using MyProject.Core.Interfaces;
using MyProject.Core.Results;
using MyProject.Data;
using MyProject.Services;
using Moq;
using Xunit;

namespace MyProject.Tests
{
    public class ConcurrencyTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _db;

        public ConcurrencyTests(TestDatabaseFixture db)
        {
            _db = db;
        }

        // שני DbContext נפרדים קוראים את אותו רכב, שניהם משנים אותו.
        // הראשון שומר בהצלחה, והשני מקבל DbUpdateConcurrencyException
        [Fact]
        public async Task TwoContexts_UpdateSameCar_SecondSaveThrows()
        {
            var carId = await _db.CreateCarAsync();
            await using var first = _db.CreateContext();
            await using var second = _db.CreateContext();

            var carInFirst = await first.Cars.SingleAsync(c => c.Id == carId);
            var carInSecond = await second.Cars.SingleAsync(c => c.Id == carId);

            carInFirst.LastBookedAt = DateTime.UtcNow;
            carInSecond.LastBookedAt = DateTime.UtcNow;

            await first.SaveChangesAsync();
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
        }

        // תרחיש ה-race המלא, שלב אחר שלב: שתי בקשות קוראות את הרכב, שתיהן רואות שהוא פנוי,
        // ושתיהן מנסות לשמור הזמנה לאותם תאריכים. רק אחת נשמרת.
        [Fact]
        public async Task TwoBookings_InterleavedOnSameCar_OnlyFirstIsSaved()
        {
            var carId = await _db.CreateCarAsync();
            var start = DateTime.Today.AddDays(10);
            var end = start.AddDays(3);

            await using var contextA = _db.CreateContext();
            await using var contextB = _db.CreateContext();
            var carsA = new CarRepository(contextA);
            var carsB = new CarRepository(contextB);
            var rentalsA = new RentalRepository(contextA);
            var rentalsB = new RentalRepository(contextB);

            // שתי הבקשות טוענות את הרכב ובודקות זמינות לפני שמישהו שמר
            var carA = (await carsA.GetForUpdateAsync(carId, default))!;
            var carB = (await carsB.GetForUpdateAsync(carId, default))!;
            Assert.False(await rentalsA.HasOverlapAsync(carId, start, end, null, default));
            Assert.False(await rentalsB.HasOverlapAsync(carId, start, end, null, default));

            rentalsA.Add(new MyProject.Core.Entities.Rental { CarId = carId, CustomerId = 1, DateStart = start, DateEnd = end });
            carA.LastBookedAt = DateTime.UtcNow;
            rentalsB.Add(new MyProject.Core.Entities.Rental { CarId = carId, CustomerId = 2, DateStart = start, DateEnd = end });
            carB.LastBookedAt = DateTime.UtcNow;

            await rentalsA.SaveChangesAsync(default);
            await Assert.ThrowsAsync<ConcurrencyConflictException>(() => rentalsB.SaveChangesAsync(default));

            // ה-INSERT של ההזמנה השנייה בוטל יחד עם ה-UPDATE של הרכב
            await using var verify = _db.CreateContext();
            Assert.Equal(1, await verify.Rentals.CountAsync(r => r.CarId == carId));
        }

        // שתי בקשות הזמנה אמיתיות במקביל דרך ה-Service, כל אחת עם DbContext משלה
        [Fact]
        public async Task ParallelBookings_ThroughService_ExactlyOneSucceedsAndOtherGets409()
        {
            var mapper = new MapperConfiguration(c => c.AddProfile<MappingProfile>()).CreateMapper();

            for (var round = 0; round < 5; round++)
            {
                var carId = await _db.CreateCarAsync();
                // מנהל שמזמין עבור לקוח 1
                var admin = new Moq.Mock<ICurrentUser>();
                admin.Setup(u => u.IsAdmin).Returns(true);
                // חברת אשראי שמאשרת כל חיוב. בודקים בסוף שהמפסיד במרוץ קיבל זיכוי
                var payments = new Moq.Mock<IPaymentGateway>();
                payments.Setup(p => p.ChargeAsync(It.IsAny<PaymentDto>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => PaymentResult.Success(Guid.NewGuid().ToString("N")));

                var request = new RentalRequestDto
                {
                    CarId = carId,
                    CustomerId = 1,
                    DateStart = DateTime.Today.AddDays(5),
                    DateEnd = DateTime.Today.AddDays(8),
                    Payment = new PaymentDto
                    {
                        CardHolder = "Test",
                        CardNumber = "4242424242424242",
                        ExpiryMonth = 12,
                        ExpiryYear = DateTime.Today.Year + 2,
                        Cvv = "123"
                    }
                };

                async Task<Result<RentalDto>> BookAsync()
                {
                    await using var context = _db.CreateContext();
                    var service = new RentalService(
                        new RentalRepository(context),
                        new CarRepository(context),
                        new CustomerRepository(context),
                        admin.Object,
                        payments.Object,
                        mapper,
                        NullLogger<RentalService>.Instance);
                    return await service.CreateAsync(request, default);
                }

                var results = await Task.WhenAll(Task.Run(BookAsync), Task.Run(BookAsync));

                Assert.Single(results, r => r.IsSuccess);
                Assert.Single(results, r => r.Status == ResultStatus.Conflict);

                await using var verify = _db.CreateContext();
                Assert.Equal(1, await verify.Rentals.CountAsync(r => r.CarId == carId));

                // המפסיד נדחה בבדיקת החפיפה (בלי חיוב) או בשמירה (חיוב וזיכוי).
                // בשני המקרים נשאר בדיוק חיוב אחד בפועל - של ההזמנה שנשמרה
                var charges = payments.Invocations.Count(i => i.Method.Name == nameof(IPaymentGateway.ChargeAsync));
                var refunds = payments.Invocations.Count(i => i.Method.Name == nameof(IPaymentGateway.RefundAsync));
                Assert.Equal(1, charges - refunds);
            }
        }
    }
}
