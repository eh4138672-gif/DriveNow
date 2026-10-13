namespace MyProject.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // קריאה בלבד (AsNoTracking)
        Task<T?> GetByIdAsync(int id, CancellationToken ct);

        // טעינה עם מעקב שינויים, לצורך עדכון או מחיקה
        Task<T?> GetForUpdateAsync(int id, CancellationToken ct);

        Task<bool> ExistsAsync(int id, CancellationToken ct);
        void Add(T entity);
        void Remove(T entity);

        // שומר את כל השינויים של הבקשה בטרנזקציה אחת.
        // זורק ConcurrencyConflictException אם שורה שונתה בינתיים על ידי מישהו אחר,
        // ו-DuplicateKeyException אם נשמר ערך שכבר קיים באינדקס ייחודי
        Task SaveChangesAsync(CancellationToken ct);

        // פותח טרנזקציה מפורשת, כדי שבדיקה עסקית והשמירה שאחריה ירוצו באותה טרנזקציה
        Task<ITransaction> BeginTransactionAsync(CancellationToken ct);
    }
}
