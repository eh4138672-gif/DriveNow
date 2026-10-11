using MyProject.Core.DTOs;
using MyProject.Core.Entities;

namespace MyProject.Core.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<PagedResult<Customer>> GetPagedAsync(CustomerQueryParameters query, CancellationToken ct);
        Task<bool> DriverLicenseExistsAsync(string driverLicenseId, int? excludeCustomerId, CancellationToken ct);
    }
}
