using Microsoft.EntityFrameworkCore;
using MyProject.Core.DTOs;
using MyProject.Core.Entities;
using MyProject.Core.Interfaces;

namespace MyProject.Data
{
    public class RentalRepository : Repository<Rental>, IRentalRepository
    {
        public RentalRepository(DataContext context) : base(context)
        {
        }

        // טעינת הרכב, התכונות שלו והלקוח באותה שאילתה (JOIN), במקום שאילתה נפרדת לכל השכרה (N+1)
        protected override IQueryable<Rental> ReadQuery =>
            Set.AsNoTracking()
                .Include(r => r.Car)
                    .ThenInclude(c => c.Features)
                .Include(r => r.Customer);

        public Task<PagedResult<Rental>> GetPagedAsync(RentalQueryParameters query, CancellationToken ct)
        {
            var rentals = ReadQuery;

            if (query.CarId.HasValue)
            {
                rentals = rentals.Where(r => r.CarId == query.CarId.Value);
            }
            if (query.CustomerId.HasValue)
            {
                rentals = rentals.Where(r => r.CustomerId == query.CustomerId.Value);
            }
            if (query.From.HasValue)
            {
                rentals = rentals.Where(r => r.DateEnd > query.From.Value);
            }
            if (query.To.HasValue)
            {
                rentals = rentals.Where(r => r.DateStart < query.To.Value);
            }

            var ordered = query.Descending
                ? rentals.OrderByDescending(r => r.DateStart)
                : rentals.OrderBy(r => r.DateStart);

            return ToPagedResultAsync(ordered.ThenBy(r => r.Id), query, ct);
        }

        public Task<bool> HasOverlapAsync(int carId, DateTime start, DateTime end, int? excludeRentalId, CancellationToken ct) =>
            // שני טווחים חופפים אם כל אחד מתחיל לפני שהשני נגמר
            Set.AnyAsync(r =>
                r.CarId == carId &&
                r.Id != excludeRentalId &&
                r.DateStart < end &&
                start < r.DateEnd, ct);

        public Task<bool> AnyForCarAsync(int carId, CancellationToken ct) =>
            Set.AnyAsync(r => r.CarId == carId, ct);

        public Task<bool> AnyForCustomerAsync(int customerId, CancellationToken ct) =>
            Set.AnyAsync(r => r.CustomerId == customerId, ct);
    }
}
