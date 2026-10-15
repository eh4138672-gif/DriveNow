using Microsoft.EntityFrameworkCore;
using MyProject.Core.Entities;
using MyProject.Core.Interfaces;

namespace MyProject.Data
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(DataContext context) : base(context)
        {
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct) =>
            ReadQuery.Include(u => u.Customer).FirstOrDefaultAsync(u => u.Email == email, ct);

        public Task<bool> EmailExistsAsync(string email, CancellationToken ct) =>
            Set.AnyAsync(u => u.Email == email, ct);
    }
}
