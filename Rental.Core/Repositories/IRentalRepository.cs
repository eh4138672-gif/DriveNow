using MyProject.Core.DTOs;
using MyProject.Core.Entities;

namespace MyProject.Core.Interfaces
{
    public interface IRentalRepository : IRepository<Rental>
    {
        Task<PagedResult<Rental>> GetPagedAsync(RentalQueryParameters query, CancellationToken ct);

        // האם יש לרכב השכרה שחופפת לטווח. excludeRentalId - להתעלם מההשכרה שמעדכנים
        Task<bool> HasOverlapAsync(int carId, DateTime start, DateTime end, int? excludeRentalId, CancellationToken ct);

        Task<bool> AnyForCarAsync(int carId, CancellationToken ct);
        Task<bool> AnyForCustomerAsync(int customerId, CancellationToken ct);
    }
}
