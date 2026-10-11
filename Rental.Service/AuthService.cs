using Microsoft.Extensions.Logging;
using MyProject.Core.DTOs;
using MyProject.Core.Entities;
using MyProject.Core.Interfaces;
using MyProject.Core.Results;

namespace MyProject.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly ICustomerRepository _customers;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository users,
            ICustomerRepository customers,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            ILogger<AuthService> logger)
        {
            _users = users;
            _customers = customers;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken ct)
        {
            var email = NormalizeEmail(dto.Email);
            if (await _users.EmailExistsAsync(email, ct))
            {
                return Result<AuthResponseDto>.Conflict("כבר קיים משתמש עם האימייל הזה");
            }
            if (await _customers.DriverLicenseExistsAsync(dto.DriverLicenseId, null, ct))
            {
                return Result<AuthResponseDto>.Conflict("כבר קיים לקוח עם מספר רישיון הנהיגה הזה");
            }

            var customer = new Customer
            {
                Name = dto.Name,
                PhoneNumber = dto.PhoneNumber,
                DriverLicenseId = dto.DriverLicenseId
            };
            // הלקוח והמשתמש נשמרים יחד, באותה טרנזקציה
            var user = new User
            {
                Email = email,
                PasswordHash = _passwordHasher.Hash(dto.Password),
                Role = UserRole.Customer,
                Customer = customer
            };
            _users.Add(user);
            await _users.SaveChangesAsync(ct);

            // לא כותבים ללוג את פרטי ההרשמה, רק את המזהה
            _logger.LogInformation("User {UserId} registered", user.Id);
            return Result<AuthResponseDto>.Success(CreateResponse(user, customer.Name));
        }

        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct)
        {
            var user = await _users.GetByEmailAsync(NormalizeEmail(dto.Email), ct);
            if (user == null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt");
                // אותה הודעה בשני המקרים, כדי לא לחשוף אילו אימיילים רשומים
                return Result<AuthResponseDto>.Unauthorized("אימייל או סיסמה שגויים");
            }

            _logger.LogInformation("User {UserId} logged in", user.Id);
            return Result<AuthResponseDto>.Success(CreateResponse(user, user.Customer?.Name ?? "מנהל"));
        }

        public async Task SeedDemoUsersAsync(IEnumerable<DemoUserOptions> demoUsers, CancellationToken ct)
        {
            var added = 0;
            foreach (var demo in demoUsers)
            {
                var email = NormalizeEmail(demo.Email);
                if (await _users.EmailExistsAsync(email, ct))
                {
                    continue;
                }

                _users.Add(new User
                {
                    Email = email,
                    PasswordHash = _passwordHasher.Hash(demo.Password),
                    Role = Enum.Parse<UserRole>(demo.Role, ignoreCase: true),
                    CustomerId = demo.CustomerId
                });
                added++;
            }

            if (added > 0)
            {
                await _users.SaveChangesAsync(ct);
                _logger.LogInformation("Seeded {Count} demo users", added);
            }
        }

        private AuthResponseDto CreateResponse(User user, string name)
        {
            var token = _tokenService.CreateToken(user, name);
            return new AuthResponseDto
            {
                Token = token.Token,
                ExpiresAt = token.ExpiresAt,
                Email = user.Email,
                Name = name,
                Role = user.Role.ToString(),
                CustomerId = user.CustomerId
            };
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    }
}
