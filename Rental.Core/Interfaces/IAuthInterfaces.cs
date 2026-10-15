using MyProject.Core.DTOs;
using MyProject.Core.Entities;
using MyProject.Core.Results;

namespace MyProject.Core.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken ct);
        Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct);
        Task SeedDemoUsersAsync(IEnumerable<DemoUserOptions> demoUsers, CancellationToken ct);
    }

    // יצירת JWT. המימוש יושב ב-API, כי הוא תלוי בספריות ה-JWT ובהגדרות
    public interface ITokenService
    {
        AccessToken CreateToken(User user, string displayName);
    }

    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string passwordHash);
    }

    // המשתמש של הבקשה הנוכחית, מתוך ה-claims של ה-token
    public interface ICurrentUser
    {
        int? UserId { get; }
        int? CustomerId { get; }
        bool IsAdmin { get; }
    }
}
