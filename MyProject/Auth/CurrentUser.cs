using System.Security.Claims;
using MyProject.Core.Entities;
using MyProject.Core.Interfaces;

namespace MyProject.Auth
{
    // קורא את המשתמש מה-claims של ה-token שאומת ב-JwtBearer
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

        public int? UserId => ParseInt(Principal?.FindFirstValue(ClaimTypes.NameIdentifier));

        public int? CustomerId => ParseInt(Principal?.FindFirstValue(JwtTokenService.CustomerIdClaim));

        public bool IsAdmin => Principal?.IsInRole(nameof(UserRole.Admin)) == true;

        private static int? ParseInt(string? value) => int.TryParse(value, out var result) ? result : null;
    }
}
