using System.ComponentModel.DataAnnotations;

namespace MyProject.Core.DTOs
{
    // הרשמה יוצרת משתמש מסוג Customer יחד עם פרטי הלקוח שלו
    public class RegisterDto : CustomerRequestDto
    {
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
    }

    public record AccessToken(string Token, DateTime ExpiresAt);

    // משתמשי דמו שנוצרים בהפעלה. הפרטים נטענים מהקונפיגורציה, לא כתובים בקוד
    public class DemoUserOptions
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
    }
}
