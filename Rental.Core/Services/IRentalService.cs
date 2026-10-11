using MyProject.Core.DTOs;
using MyProject.Core.Results;

namespace MyProject.Core.Interfaces
{
    public interface IRentalService
    {
        Task<PagedResult<RentalDto>> GetAllAsync(RentalQueryParameters query, CancellationToken ct);
        Task<Result<RentalDto>> GetByIdAsync(int id, CancellationToken ct);
        Task<Result<RentalDto>> CreateAsync(RentalRequestDto dto, CancellationToken ct);
        Task<Result> UpdateAsync(int id, RentalRequestDto dto, CancellationToken ct);
        Task<Result> DeleteAsync(int id, CancellationToken ct);
    }
}
