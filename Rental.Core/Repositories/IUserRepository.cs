using MyProject.Core.Entities;

namespace MyProject.Core.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        // כולל את הלקוח המקושר, לצורך השם שב-token
        Task<User?> GetByEmailAsync(string email, CancellationToken ct);
        Task<bool> EmailExistsAsync(string email, CancellationToken ct);
    }

    public interface IFeatureRepository : IRepository<Feature>
    {
        Task<List<Feature>> GetAllAsync(CancellationToken ct);

        // עם מעקב שינויים, כדי לשייך אותן לרכב
        Task<List<Feature>> GetByIdsForUpdateAsync(IReadOnlyCollection<int> ids, CancellationToken ct);
    }
}
