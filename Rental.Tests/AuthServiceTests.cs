using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MyProject.Core.DTOs;
using MyProject.Core.Entities;
using MyProject.Core.Interfaces;
using MyProject.Core.Results;
using MyProject.Services;
using Xunit;

namespace Rental.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _users = new();
        private readonly Mock<ICustomerRepository> _customers = new();
        private readonly Mock<ITokenService> _tokens = new();
        private readonly PasswordHasher _hasher = new();

        public AuthServiceTests()
        {
            _tokens.Setup(t => t.CreateToken(It.IsAny<User>(), It.IsAny<string>()))
                .Returns(new AccessToken("token", DateTime.UtcNow.AddHours(1)));
        }

        private AuthService CreateService() =>
            new(_users.Object, _customers.Object, _hasher, _tokens.Object, NullLogger<AuthService>.Instance);

        private void SetupUser(string password) =>
            _users.Setup(r => r.GetByEmailAsync("israel@rental.local", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User
                {
                    Id = 1,
                    Email = "israel@rental.local",
                    PasswordHash = _hasher.Hash(password),
                    Role = UserRole.Customer,
                    CustomerId = 1,
                    Customer = new Customer { Id = 1, Name = "ישראל ישראלי" }
                });

        [Fact]
        public async Task Login_CorrectPassword_ReturnsToken()
        {
            SetupUser("Customer123!");

            var result = await CreateService().LoginAsync(
                new LoginDto { Email = " Israel@Rental.local ", Password = "Customer123!" }, default);

            Assert.True(result.IsSuccess);
            Assert.Equal("token", result.Value!.Token);
            Assert.Equal("Customer", result.Value.Role);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            SetupUser("Customer123!");

            var result = await CreateService().LoginAsync(
                new LoginDto { Email = "israel@rental.local", Password = "wrong-password" }, default);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
        }

        [Fact]
        public async Task Register_ExistingEmail_ReturnsConflict()
        {
            _users.Setup(r => r.EmailExistsAsync("israel@rental.local", It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateService().RegisterAsync(new RegisterDto
            {
                Email = "israel@rental.local",
                Password = "Password123",
                Name = "ישראל",
                PhoneNumber = "050-1234567",
                DriverLicenseId = "12345678"
            }, default);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            _users.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public void PasswordHasher_SamePasswordTwice_ProducesDifferentHashesThatBothVerify()
        {
            var first = _hasher.Hash("Secret123!");
            var second = _hasher.Hash("Secret123!");

            Assert.NotEqual(first, second);
            Assert.True(_hasher.Verify("Secret123!", first));
            Assert.True(_hasher.Verify("Secret123!", second));
            Assert.False(_hasher.Verify("secret123!", first));
        }
    }
}
