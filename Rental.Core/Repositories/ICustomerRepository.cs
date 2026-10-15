using MyProject.Core.DTOs;
using MyProject.Core.Entities;

namespace MyProject.Core.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<PagedResult<Customer>> GetPagedAsync(CustomerQueryParameters query, CancellationToken ct);
        Task<bool> DriverLicenseExistsAsync(string driverLicenseId, int? excludeCustomerId, CancellationToken ct);

        // חיפוש לקוח אחד לפי התאמה מדויקת של רישיון הנהיגה (קריאה בלבד)
        Task<Customer?> GetByDriverLicenseAsync(string driverLicenseId, CancellationToken ct);
    }
}
