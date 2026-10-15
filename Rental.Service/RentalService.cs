using AutoMapper;
using Microsoft.Extensions.Logging;
using MyProject.Core.DTOs;
using MyProject.Core.Entities;
using MyProject.Core.Exceptions;
using MyProject.Core.Interfaces;
using MyProject.Core.Results;

namespace MyProject.Services
{
    // המשאב המוגבל במערכת: רכב בטווח תאריכים. אסור ששתי השכרות של אותו רכב יחפפו.
    //
    // איך נמנעת התנגשות בין שתי הזמנות מקבילות (optimistic concurrency):
    // 1. טוענים את הרכב עם ה-RowVersion שלו - לפני בדיקת החפיפה.
    // 2. בודקים שאין השכרה חופפת.
    // 3. מוסיפים את ההשכרה ומעדכנים את LastBookedAt של הרכב.
    // 4. SaveChanges מריץ INSERT של ההשכרה ו-UPDATE של הרכב, וה-UPDATE כולל
    //    WHERE RowVersion = הערך שקראנו בשלב 1. שלבים 2-4 רצים בטרנזקציה מפורשת אחת.
    // אם הזמנה אחרת נשמרה אחרי שלב 1, היא שינתה את ה-RowVersion של הרכב, ה-UPDATE שלנו
    // לא מוצא שורה, וכל הטרנזקציה (כולל ה-INSERT) מתבטלת. אם היא נשמרה לפני שלב 1,
    // בדיקת החפיפה בשלב 2 כבר רואה אותה. בשני המקרים רק הזמנה אחת מצליחה.
    //
    // הרשאות: מנהל רואה ומנהל את כל ההשכרות. לקוח רואה ומבטל רק את ההשכרות שעל שמו.
    // הזמנה נרשמת על שם המשתמש המחובר, או על שם לקוח אחר לפי מספר רישיון הנהיגה שלו
    // (בלי לחשוף רשימת לקוחות).
    public class RentalService : IRentalService
    {
        private readonly IRentalRepository _rentals;
        private readonly ICarRepository _cars;
        private readonly ICustomerRepository _customers;
        private readonly ICurrentUser _currentUser;
        private readonly IMapper _mapper;
        private readonly ILogger<RentalService> _logger;

        public RentalService(
            IRentalRepository rentals,
            ICarRepository cars,
            ICustomerRepository customers,
            ICurrentUser currentUser,
            IMapper mapper,
            ILogger<RentalService> logger)
        {
            _rentals = rentals;
            _cars = cars;
            _customers = customers;
            _currentUser = currentUser;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<RentalDto>> GetAllAsync(RentalQueryParameters query, CancellationToken ct)
        {
            if (!_currentUser.IsAdmin)
            {
                // משתמש שאינו מנהל ואינו מקושר ללקוח לא רואה אף השכרה
                if (_currentUser.CustomerId == null)
                {
                    return new PagedResult<RentalDto>(new List<RentalDto>(), query.Page, query.PageSize, 0);
                }
                query.CustomerId = _currentUser.CustomerId;
            }

            var page = await _rentals.GetPagedAsync(query, ct);
            return new PagedResult<RentalDto>(_mapper.Map<List<RentalDto>>(page.Items), page.Page, page.PageSize, page.TotalCount);
        }

        public async Task<Result<RentalDto>> GetByIdAsync(int id, CancellationToken ct)
        {
            var rental = await _rentals.GetByIdAsync(id, ct);
            if (rental == null)
            {
                return Result<RentalDto>.NotFound($"השכרה {id} לא נמצאה");
            }
            if (!CanAccess(rental))
            {
                return Result<RentalDto>.Forbidden("אין לך הרשאה לצפות בהשכרה הזאת");
            }
            return Result<RentalDto>.Success(_mapper.Map<RentalDto>(rental));
        }

        public async Task<Result<RentalDto>> CreateAsync(RentalRequestDto dto, CancellationToken ct)
        {
            var customer = await ResolveCustomerAsync(dto, ct);
            if (!customer.IsSuccess)
            {
                return Result<RentalDto>.FromFailure(customer);
            }
            dto.CustomerId = customer.Value;

            if (IsInPast(dto.DateStart))
            {
                return Result<RentalDto>.Invalid("לא ניתן להזמין רכב למועד שכבר עבר");
            }

            var booking = await TryBookAsync(dto, null, ct);
            if (!booking.IsSuccess)
            {
                return Result<RentalDto>.FromFailure(booking);
            }

            var rental = booking.Value!;
            _logger.LogInformation("Rental {RentalId} created for car {CarId}", rental.Id, rental.CarId);

            var created = await _rentals.GetByIdAsync(rental.Id, ct);
            return Result<RentalDto>.Success(_mapper.Map<RentalDto>(created));
        }

        public async Task<Result> UpdateAsync(int id, RentalRequestDto dto, CancellationToken ct)
        {
            var existing = await _rentals.GetByIdAsync(id, ct);
            if (existing == null)
            {
                return Result.NotFound($"השכרה {id} לא נמצאה");
            }
            if (!CanAccess(existing))
            {
                return Result.Forbidden("אין לך הרשאה לשנות את ההשכרה הזאת");
            }

            // לקוח לא יכול להעביר השכרה ללקוח אחר
            dto.CustomerId = _currentUser.IsAdmin ? dto.CustomerId ?? existing.CustomerId : existing.CustomerId;

            if (dto.DateStart != existing.DateStart && IsInPast(dto.DateStart))
            {
                return Result.Invalid("לא ניתן להזיז השכרה למועד שכבר עבר");
            }

            var booking = await TryBookAsync(dto, id, ct);
            return booking.IsSuccess ? Result.Success() : booking;
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken ct)
        {
            var rental = await _rentals.GetForUpdateAsync(id, ct);
            if (rental == null)
            {
                return Result.NotFound($"השכרה {id} לא נמצאה");
            }
            if (!CanAccess(rental))
            {
                return Result.Forbidden("אין לך הרשאה לבטל את ההשכרה הזאת");
            }

            _rentals.Remove(rental);
            try
            {
                await _rentals.SaveChangesAsync(ct);
            }
            catch (ConcurrencyConflictException)
            {
                _logger.LogWarning("Cancel conflict on rental {RentalId}: it was changed or removed by another user", id);
                return Result.Conflict("ההשכרה שונתה או בוטלה על ידי משתמש אחר. רענן ונסה שוב.");
            }
            _logger.LogInformation("Rental {RentalId} cancelled", id);
            return Result.Success();
        }

        // דקה של סבולת, כדי שהזמנה ל"עכשיו" לא תידחה בגלל הזמן שלקח לבקשה להגיע
        private static bool IsInPast(DateTime start) => start < DateTime.Now.AddMinutes(-1);

        private bool CanAccess(Rental rental) =>
            _currentUser.IsAdmin || rental.CustomerId == _currentUser.CustomerId;

        // על שם מי ההזמנה:
        // 1. נשלח מספר רישיון נהיגה - הלקוח שזה הרישיון שלו (גם לקוח וגם מנהל).
        // 2. מנהל ששלח CustomerId (דרך ה-API) - הלקוח הזה.
        // 3. אחרת - המשתמש המחובר. לקוח ששלח CustomerId של מישהו אחר מזמין לעצמו.
        private async Task<Result<int>> ResolveCustomerAsync(RentalRequestDto dto, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(dto.DriverLicenseId))
            {
                var customer = await _customers.GetByDriverLicenseAsync(dto.DriverLicenseId.Trim(), ct);
                return customer == null
                    ? Result<int>.NotFound("לא נמצא לקוח רשום עם מספר רישיון הנהיגה הזה")
                    : Result<int>.Success(customer.Id);
            }

            var customerId = _currentUser.IsAdmin ? dto.CustomerId : _currentUser.CustomerId;
            return customerId == null
                ? Result<int>.Invalid("יש להזין את מספר רישיון הנהיגה של הלקוח שעל שמו ההזמנה")
                : Result<int>.Success(customerId.Value);
        }

        // יוצר השכרה חדשה (rentalId == null) או מעדכן קיימת, תוך שמירה על הרכב כמשאב מוגבל
        private async Task<Result<Rental>> TryBookAsync(RentalRequestDto dto, int? rentalId, CancellationToken ct)
        {
            // שלב 1: הרכב נטען עם מעקב, וה-RowVersion שלו נשמר כערך המקורי
            var car = await _cars.GetForUpdateAsync(dto.CarId, ct);
            if (car == null)
            {
                return Result<Rental>.NotFound($"רכב {dto.CarId} לא נמצא");
            }
            if (!car.IsActive)
            {
                return Result<Rental>.Conflict("הרכב הוצא מההשכרה ואינו זמין להזמנה");
            }
            var customerId = dto.CustomerId!.Value;
            if (!await _customers.ExistsAsync(customerId, ct))
            {
                return Result<Rental>.NotFound($"לקוח {customerId} לא נמצא");
            }

            // תאריך ושעה מדויקים. DateEnd הוא רגע ההחזרה, והשכרה אחרת יכולה להתחיל בו בדיוק
            var start = dto.DateStart;
            var end = dto.DateEnd;

            // בדיקת החפיפה והשמירה רצות באותה טרנזקציה. יציאה בלי Commit מבטלת הכול
            await using var transaction = await _rentals.BeginTransactionAsync(ct);

            // שלב 2: בדיקת זמינות
            _logger.LogDebug("Checking availability of car {CarId} for {Start:yyyy-MM-dd} - {End:yyyy-MM-dd}", car.Id, start, end);
            if (await _rentals.HasOverlapAsync(car.Id, start, end, rentalId, ct))
            {
                return Result<Rental>.Conflict("הרכב כבר מוזמן בחלק מהתאריכים האלה");
            }

            // שלב 3
            Rental rental;
            if (rentalId == null)
            {
                rental = _mapper.Map<Rental>(dto);
                _rentals.Add(rental);
            }
            else
            {
                rental = (await _rentals.GetForUpdateAsync(rentalId.Value, ct))!;
                _mapper.Map(dto, rental);
            }
            rental.CustomerId = customerId;
            rental.DateStart = start;
            rental.DateEnd = end;

            // נוגעים ברכב כדי שה-UPDATE שלו (עם בדיקת ה-RowVersion) ייכנס לאותה טרנזקציה
            car.LastBookedAt = DateTime.UtcNow;

            // שלב 4
            try
            {
                await _rentals.SaveChangesAsync(ct);
            }
            catch (ConcurrencyConflictException)
            {
                _logger.LogWarning(
                    "Booking conflict on car {CarId} for {Start:yyyy-MM-dd} - {End:yyyy-MM-dd}: another booking was saved first",
                    car.Id, start, end);
                return Result<Rental>.Conflict("הרכב נתפס על ידי משתמש אחר ממש עכשיו. בחר תאריכים אחרים או נסה שוב.");
            }
            await transaction.CommitAsync(ct);

            return Result<Rental>.Success(rental);
        }
    }
}
