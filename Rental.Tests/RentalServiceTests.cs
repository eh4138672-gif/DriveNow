using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MyProject.Core.DTOs;
using MyProject.Core.Entities;
using MyProject.Core.Exceptions;
using MyProject.Core.Interfaces;
using MyProject.Core.Results;
using MyProject.Services;
using Xunit;

namespace Rental.Tests
{
    public class RentalServiceTests
    {
        private readonly Mock<IRentalRepository> _rentals = new();
        private readonly Mock<ICarRepository> _cars = new();
        private readonly Mock<ICustomerRepository> _customers = new();
        private readonly Mock<ICurrentUser> _currentUser = new();
        private readonly Mock<ITransaction> _transaction = new();
        private readonly IMapper _mapper = new MapperConfiguration(c => c.AddProfile<MappingProfile>()).CreateMapper();
        private readonly Car _car = new() { Id = 1, Model = "Mazda 3", LicenseNumber = "123-45-678", Price = 100 };

        public RentalServiceTests()
        {
            // ברירת המחדל: לקוח מחובר עם CustomerId = 1
            _currentUser.Setup(u => u.CustomerId).Returns(1);
            _currentUser.Setup(u => u.IsAdmin).Returns(false);

            _cars.Setup(r => r.GetForUpdateAsync(_car.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_car);
            _customers.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _rentals.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_transaction.Object);
            _rentals.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) => new MyProject.Core.Entities.Rental
                {
                    Id = id,
                    CarId = _car.Id,
                    Car = _car,
                    CustomerId = 1,
                    Customer = new Customer { Id = 1, Name = "ישראל ישראלי" },
                    DateStart = Request().DateStart,
                    DateEnd = Request().DateEnd
                });
        }

        private RentalService CreateService() =>
            new(_rentals.Object, _cars.Object, _customers.Object, _currentUser.Object, _mapper, NullLogger<RentalService>.Instance);

        private static RentalRequestDto Request(int carId = 1) => new()
        {
            CarId = carId,
            DateStart = DateTime.Today.AddDays(1),
            DateEnd = DateTime.Today.AddDays(4)
        };

        private void SetupOverlap(bool overlap) =>
            _rentals.Setup(r => r.HasOverlapAsync(_car.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(overlap);

        [Fact]
        public async Task Create_CarAvailable_SavesRentalAndTouchesCar()
        {
            SetupOverlap(false);

            var result = await CreateService().CreateAsync(Request(), default);

            Assert.True(result.IsSuccess);
            Assert.Equal(300, result.Value!.TotalPrice);
            Assert.Equal("Mazda 3", result.Value.CarModel);
            // הרכב חייב להשתנות, אחרת ה-RowVersion שלו לא נבדק בשמירה
            Assert.NotNull(_car.LastBookedAt);
            _rentals.Verify(r => r.Add(It.Is<MyProject.Core.Entities.Rental>(x => x.CarId == _car.Id)), Times.Once);
            _rentals.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Create_CarAlreadyBooked_ReturnsConflictWithoutSaving()
        {
            SetupOverlap(true);

            var result = await CreateService().CreateAsync(Request(), default);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            _rentals.Verify(r => r.Add(It.IsAny<MyProject.Core.Entities.Rental>()), Times.Never);
            _rentals.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Create_AnotherBookingSavedFirst_ReturnsConflict()
        {
            SetupOverlap(false);
            _rentals.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ConcurrencyConflictException("conflict", new Exception()));

            var result = await CreateService().CreateAsync(Request(), default);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            // הטרנזקציה לא נסגרת ב-Commit, ולכן היא מתבטלת
            _transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _transaction.Verify(t => t.DisposeAsync(), Times.Once);
        }

        [Fact]
        public async Task Delete_RentalRemovedConcurrently_ReturnsConflict()
        {
            var rental = new MyProject.Core.Entities.Rental { Id = 5, CarId = 1, CustomerId = 1 };
            _rentals.Setup(r => r.GetForUpdateAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(rental);
            _rentals.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ConcurrencyConflictException("conflict", new Exception()));

            var result = await CreateService().DeleteAsync(5, default);

            Assert.Equal(ResultStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task GetAll_NonAdminWithoutCustomer_ReturnsEmptyPage()
        {
            _currentUser.Setup(u => u.CustomerId).Returns((int?)null);

            var page = await CreateService().GetAllAsync(new RentalQueryParameters(), default);

            Assert.Empty(page.Items);
            _rentals.Verify(r => r.GetPagedAsync(It.IsAny<RentalQueryParameters>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Create_CarNotFound_ReturnsNotFound()
        {
            var result = await CreateService().CreateAsync(Request(carId: 99), default);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            _rentals.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Create_CustomerSendsOtherCustomerId_BooksForThemselves()
        {
            SetupOverlap(false);
            var request = Request();
            request.CustomerId = 2;

            var result = await CreateService().CreateAsync(request, default);

            Assert.True(result.IsSuccess);
            _rentals.Verify(r => r.Add(It.Is<MyProject.Core.Entities.Rental>(x => x.CustomerId == 1)), Times.Once);
        }

        [Fact]
        public async Task Create_AdminWithoutCustomer_ReturnsInvalid()
        {
            _currentUser.Setup(u => u.IsAdmin).Returns(true);
            _currentUser.Setup(u => u.CustomerId).Returns((int?)null);

            var result = await CreateService().CreateAsync(Request(), default);

            Assert.Equal(ResultStatus.Invalid, result.Status);
        }

        [Fact]
        public async Task Delete_RentalOfAnotherCustomer_ReturnsForbidden()
        {
            var othersRental = new MyProject.Core.Entities.Rental { Id = 7, CarId = 1, CustomerId = 2 };
            _rentals.Setup(r => r.GetForUpdateAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(othersRental);

            var result = await CreateService().DeleteAsync(7, default);

            Assert.Equal(ResultStatus.Forbidden, result.Status);
            _rentals.Verify(r => r.Remove(It.IsAny<MyProject.Core.Entities.Rental>()), Times.Never);
        }

        [Fact]
        public async Task Create_DateInPast_ReturnsInvalid()
        {
            var request = Request();
            request.DateStart = DateTime.Today.AddDays(-1);

            var result = await CreateService().CreateAsync(request, default);

            Assert.Equal(ResultStatus.Invalid, result.Status);
        }
    }
}
