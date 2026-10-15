using Microsoft.EntityFrameworkCore;
using Npgsql;
using MyProject.Core.DTOs;
using MyProject.Core.Exceptions;
using MyProject.Core.Interfaces;

namespace MyProject.Data
{
    public abstract class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DataContext Context;
        protected readonly DbSet<T> Set;

        protected Repository(DataContext context)
        {
            Context = context;
            Set = context.Set<T>();
        }

        // שאילתת הבסיס לקריאה בלבד. מחלקות יורשות מוסיפות Include לפי הצורך
        protected virtual IQueryable<T> ReadQuery => Set.AsNoTracking();

        public Task<T?> GetByIdAsync(int id, CancellationToken ct) =>
            ReadQuery.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, ct);

        public async Task<T?> GetForUpdateAsync(int id, CancellationToken ct) =>
            await Set.FindAsync(new object[] { id }, ct);

        public Task<bool> ExistsAsync(int id, CancellationToken ct) =>
            Set.AnyAsync(e => EF.Property<int>(e, "Id") == id, ct);

        public void Add(T entity) => Set.Add(entity);

        public void Remove(T entity) => Set.Remove(entity);

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            try
            {
                await Context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // ה-Version בשורה כבר לא תואם למה שקראנו - מישהו אחר שמר לפנינו
                throw new ConcurrencyConflictException("The row was modified by another user", ex);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // 23505 - הפרה של אינדקס ייחודי או של מפתח ראשי
                throw new DuplicateKeyException("A row with the same unique value already exists", ex);
            }
        }

        public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct) =>
            new EfTransaction(await Context.Database.BeginTransactionAsync(ct));

        protected static async Task<PagedResult<T>> ToPagedResultAsync(
            IQueryable<T> query, PagingParameters paging, CancellationToken ct)
        {
            // Skip ו-Take רצים בתוך שאילתת ה-SQL, לא בזיכרון
            var totalCount = await query.CountAsync(ct);
            var items = await query
                .Skip((paging.Page - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync(ct);

            return new PagedResult<T>(items, paging.Page, paging.PageSize, totalCount);
        }
    }
}
