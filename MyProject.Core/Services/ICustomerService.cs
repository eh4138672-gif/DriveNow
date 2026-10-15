using MyProject.Core.DTOs;
using MyProject.Core.Results;

namespace MyProject.Core.Interfaces
{
    public interface ICustomerService
    {
        Task<PagedResult<CustomerDto>> GetAllAsync(CustomerQueryParameters query, CancellationToken ct);
        Task<Result<CustomerDto>> GetByIdAsync(int id, CancellationToken ct);
        Task<Result<CustomerDto>> CreateAsync(CustomerRequestDto dto, CancellationToken ct);
        Task<Result> UpdateAsync(int id, CustomerRequestDto dto, CancellationToken ct);
        Task<Result> DeleteAsync(int id, CancellationToken ct);
    }
}
