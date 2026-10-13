namespace MyProject.Core.Interfaces
{
    // טרנזקציה של בסיס הנתונים, בלי תלות ב-EF Core.
    // Dispose בלי Commit מבטל את כל מה שנעשה בתוכה
    public interface ITransaction : IAsyncDisposable
    {
        Task CommitAsync(CancellationToken ct);
    }
}
