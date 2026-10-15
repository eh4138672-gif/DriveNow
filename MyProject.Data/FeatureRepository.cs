using Microsoft.EntityFrameworkCore;
using MyProject.Core.Entities;
using MyProject.Core.Interfaces;

namespace MyProject.Data
{
    public class FeatureRepository : Repository<Feature>, IFeatureRepository
    {
        public FeatureRepository(DataContext context) : base(context)
        {
        }

        public Task<List<Feature>> GetAllAsync(CancellationToken ct) =>
            ReadQuery.OrderBy(f => f.Name).ToListAsync(ct);

        public Task<List<Feature>> GetByIdsForUpdateAsync(IReadOnlyCollection<int> ids, CancellationToken ct) =>
            Set.Where(f => ids.Contains(f.Id)).ToListAsync(ct);
    }
}
