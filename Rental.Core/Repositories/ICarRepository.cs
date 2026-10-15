using MyProject.Core.DTOs;
using MyProject.Core.Entities;

namespace MyProject.Core.Interfaces
{
    public interface ICarRepository : IRepository<Car>
    {
        Task<PagedResult<Car>> GetPagedAsync(CarQueryParameters query, CancellationToken ct);

        // עם מעקב שינויים וכולל התכונות, לצורך עדכון רשימת התכונות
        Task<Car?> GetWithFeaturesForUpdateAsync(int id, CancellationToken ct);
        Task<bool> LicenseNumberExistsAsync(string licenseNumber, int? excludeCarId, CancellationToken ct);

        // קובע את ה-Version שהלקוח קרא, כדי שהעדכון ייכשל אם הרכב השתנה מאז
        void SetOriginalVersion(Car car, Guid version);
    }
}
