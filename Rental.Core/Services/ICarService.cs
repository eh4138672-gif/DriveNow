using MyProject.Core.DTOs;
using MyProject.Core.Results;

namespace MyProject.Core.Interfaces
{
    public interface ICarService
    {
        Task<PagedResult<CarDto>> GetAllAsync(CarQueryParameters query, CancellationToken ct);
        Task<Result<CarDto>> GetByIdAsync(int id, CancellationToken ct);
        Task<Result<CarDto>> CreateAsync(CarRequestDto dto, CancellationToken ct);
        Task<Result> UpdateAsync(int id, CarUpdateDto dto, CancellationToken ct);
        Task<Result> DeleteAsync(int id, CancellationToken ct);
        Task<Result> SetAvailabilityAsync(int id, bool isActive, CancellationToken ct);
        Task<List<FeatureDto>> GetFeaturesAsync(CancellationToken ct);
    }
}
