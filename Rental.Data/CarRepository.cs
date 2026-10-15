using Microsoft.EntityFrameworkCore;
using MyProject.Core.DTOs;
using MyProject.Core.Entities;
using MyProject.Core.Interfaces;

namespace MyProject.Data
{
    public class CarRepository : Repository<Car>, ICarRepository
    {
        public CarRepository(DataContext context) : base(context)
        {
        }

        protected override IQueryable<Car> ReadQuery =>
            Set.AsNoTracking().Include(c => c.Features);

        public Task<PagedResult<Car>> GetPagedAsync(CarQueryParameters query, CancellationToken ct)
        {
            var cars = ReadQuery;

            if (!query.IncludeInactive)
            {
                cars = cars.Where(c => c.IsActive);
            }
            if (query.FeatureId.HasValue)
            {
                cars = cars.Where(c => c.Features.Any(f => f.Id == query.FeatureId.Value));
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                cars = cars.Where(c => c.Model.Contains(query.Search) || c.LicenseNumber.Contains(query.Search));
            }
            if (query.MinPrice.HasValue)
            {
                cars = cars.Where(c => c.PricePerHour >= query.MinPrice.Value);
            }
            if (query.MaxPrice.HasValue)
            {
                cars = cars.Where(c => c.PricePerHour <= query.MaxPrice.Value);
            }

            var ordered = query.SortBy?.ToLowerInvariant() switch
            {
                "price" => query.Descending ? cars.OrderByDescending(c => c.PricePerHour) : cars.OrderBy(c => c.PricePerHour),
                "licensenumber" => query.Descending ? cars.OrderByDescending(c => c.LicenseNumber) : cars.OrderBy(c => c.LicenseNumber),
                _ => query.Descending ? cars.OrderByDescending(c => c.Model) : cars.OrderBy(c => c.Model)
            };

            // מיון משני לפי Id, כדי שהדפים יהיו יציבים כשיש ערכים זהים
            return ToPagedResultAsync(ordered.ThenBy(c => c.Id), query, ct);
        }

        public Task<Car?> GetWithFeaturesForUpdateAsync(int id, CancellationToken ct) =>
            Set.Include(c => c.Features).FirstOrDefaultAsync(c => c.Id == id, ct);

        public Task<bool> LicenseNumberExistsAsync(string licenseNumber, int? excludeCarId, CancellationToken ct) =>
            Set.AnyAsync(c => c.LicenseNumber == licenseNumber && c.Id != excludeCarId, ct);

        public void SetOriginalRowVersion(Car car, byte[] rowVersion) =>
            Context.Entry(car).Property(c => c.RowVersion).OriginalValue = rowVersion;
    }
}
