using Microsoft.EntityFrameworkCore;
using MyProject.Core.DTOs;
using MyProject.Core.Entities;
using MyProject.Core.Interfaces;

namespace MyProject.Data
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(DataContext context) : base(context)
        {
        }

        public Task<PagedResult<Customer>> GetPagedAsync(CustomerQueryParameters query, CancellationToken ct)
        {
            var customers = ReadQuery;

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                customers = customers.Where(c => c.Name.Contains(query.Search) || c.PhoneNumber.Contains(query.Search));
            }

            return ToPagedResultAsync(customers.OrderBy(c => c.Name).ThenBy(c => c.Id), query, ct);
        }

        public Task<bool> DriverLicenseExistsAsync(string driverLicenseId, int? excludeCustomerId, CancellationToken ct) =>
            Set.AnyAsync(c => c.DriverLicenseId == driverLicenseId && c.Id != excludeCustomerId, ct);

        public Task<Customer?> GetByDriverLicenseAsync(string driverLicenseId, CancellationToken ct) =>
            ReadQuery.FirstOrDefaultAsync(c => c.DriverLicenseId == driverLicenseId, ct);
    }
}
