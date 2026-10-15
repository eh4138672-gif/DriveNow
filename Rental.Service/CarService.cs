using AutoMapper;
using Microsoft.Extensions.Logging;
using MyProject.Core.DTOs;
using MyProject.Core.Entities;
using MyProject.Core.Exceptions;
using MyProject.Core.Interfaces;
using MyProject.Core.Results;

namespace MyProject.Services
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _cars;
        private readonly IRentalRepository _rentals;
        private readonly IFeatureRepository _features;
        private readonly ICurrentUser _currentUser;
        private readonly IMapper _mapper;
        private readonly ILogger<CarService> _logger;

        public CarService(
            ICarRepository cars,
            IRentalRepository rentals,
            IFeatureRepository features,
            ICurrentUser currentUser,
            IMapper mapper,
            ILogger<CarService> logger)
        {
            _cars = cars;
            _rentals = rentals;
            _features = features;
            _currentUser = currentUser;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<CarDto>> GetAllAsync(CarQueryParameters query, CancellationToken ct)
        {
            // רק מנהל רואה רכבים שהוצאו מההשכרה
            query.IncludeInactive = query.IncludeInactive && _currentUser.IsAdmin;

            var page = await _cars.GetPagedAsync(query, ct);
            return new PagedResult<CarDto>(_mapper.Map<List<CarDto>>(page.Items), page.Page, page.PageSize, page.TotalCount);
        }

        public async Task<Result<CarDto>> GetByIdAsync(int id, CancellationToken ct)
        {
            var car = await _cars.GetByIdAsync(id, ct);
            return car == null || (!car.IsActive && !_currentUser.IsAdmin)
                ? Result<CarDto>.NotFound($"רכב {id} לא נמצא")
                : Result<CarDto>.Success(_mapper.Map<CarDto>(car));
        }

        public async Task<Result> SetAvailabilityAsync(int id, bool isActive, CancellationToken ct)
        {
            var car = await _cars.GetForUpdateAsync(id, ct);
            if (car == null)
            {
                return Result.NotFound($"רכב {id} לא נמצא");
            }
            if (car.IsActive == isActive)
            {
                return Result.Success();
            }

            // השינוי מעדכן את ה-Version, ולכן הזמנה שרצה במקביל ייכשל עם 409 במקום להיכנס לרכב שהוצא
            car.IsActive = isActive;
            try
            {
                await _cars.SaveChangesAsync(ct);
            }
            catch (ConcurrencyConflictException)
            {
                _logger.LogWarning("Concurrency conflict changing availability of car {CarId}", id);
                return Result.Conflict("הרכב השתנה בזמן העדכון. נסה שוב.");
            }

            _logger.LogInformation("Car {CarId} is now {State}", id, isActive ? "available for rent" : "out of rental");
            return Result.Success();
        }

        public async Task<List<FeatureDto>> GetFeaturesAsync(CancellationToken ct) =>
            _mapper.Map<List<FeatureDto>>(await _features.GetAllAsync(ct));

        public async Task<Result<CarDto>> CreateAsync(CarRequestDto dto, CancellationToken ct)
        {
            if (await _cars.LicenseNumberExistsAsync(dto.LicenseNumber, null, ct))
            {
                return Result<CarDto>.Conflict($"כבר קיים רכב עם מספר רישוי {dto.LicenseNumber}");
            }

            var features = await LoadFeaturesAsync(dto.FeatureIds, ct);
            if (features == null)
            {
                return Result<CarDto>.Invalid("אחת התכונות שנבחרו לא קיימת");
            }

            var car = _mapper.Map<Car>(dto);
            car.Features = features;
            _cars.Add(car);
            try
            {
                await _cars.SaveChangesAsync(ct);
            }
            catch (DuplicateKeyException)
            {
                _logger.LogWarning("Car create conflict: license number {LicenseNumber} was saved concurrently", dto.LicenseNumber);
                return Result<CarDto>.Conflict($"כבר קיים רכב עם מספר רישוי {dto.LicenseNumber}");
            }

            _logger.LogInformation("Car {CarId} created", car.Id);
            return Result<CarDto>.Success(_mapper.Map<CarDto>(car));
        }

        public async Task<Result> UpdateAsync(int id, CarUpdateDto dto, CancellationToken ct)
        {
            var car = await _cars.GetWithFeaturesForUpdateAsync(id, ct);
            if (car == null)
            {
                return Result.NotFound($"רכב {id} לא נמצא");
            }
            if (await _cars.LicenseNumberExistsAsync(dto.LicenseNumber, id, ct))
            {
                return Result.Conflict($"כבר קיים רכב עם מספר רישוי {dto.LicenseNumber}");
            }

            var features = await LoadFeaturesAsync(dto.FeatureIds, ct);
            if (features == null)
            {
                return Result.Invalid("אחת התכונות שנבחרו לא קיימת");
            }

            _mapper.Map(dto, car);
            car.Features.Clear();
            foreach (var feature in features)
            {
                car.Features.Add(feature);
            }
            _cars.SetOriginalVersion(car, dto.Version!.Value);

            try
            {
                await _cars.SaveChangesAsync(ct);
            }
            catch (ConcurrencyConflictException)
            {
                _logger.LogWarning("Concurrency conflict updating car {CarId}", id);
                return Result.Conflict("הרכב עודכן על ידי משתמש אחר מאז שנטען. טען אותו מחדש ונסה שוב.");
            }
            catch (DuplicateKeyException)
            {
                _logger.LogWarning("Car update conflict: license number {LicenseNumber} was saved concurrently", dto.LicenseNumber);
                return Result.Conflict($"כבר קיים רכב עם מספר רישוי {dto.LicenseNumber}");
            }

            return Result.Success();
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken ct)
        {
            var car = await _cars.GetForUpdateAsync(id, ct);
            if (car == null)
            {
                return Result.NotFound($"רכב {id} לא נמצא");
            }
            if (await _rentals.AnyForCarAsync(id, ct))
            {
                return Result.Conflict("לא ניתן למחוק רכב שיש לו השכרות. אפשר להוציא אותו מההשכרה במקום.");
            }

            _cars.Remove(car);

            try
            {
                await _cars.SaveChangesAsync(ct);
            }
            catch (ConcurrencyConflictException)
            {
                // הרכב הוזמן או עודכן בין הבדיקה למחיקה
                _logger.LogWarning("Concurrency conflict deleting car {CarId}", id);
                return Result.Conflict("הרכב השתנה בזמן המחיקה. נסה שוב.");
            }

            return Result.Success();
        }

        // מחזיר null אם אחד המזהים לא קיים
        private async Task<List<Feature>?> LoadFeaturesAsync(List<int> featureIds, CancellationToken ct)
        {
            var ids = featureIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return new List<Feature>();
            }

            var features = await _features.GetByIdsForUpdateAsync(ids, ct);
            return features.Count == ids.Count ? features : null;
        }
    }
}
