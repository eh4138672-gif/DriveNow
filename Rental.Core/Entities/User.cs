namespace MyProject.Core.Entities
{
    public enum UserRole
    {
        Customer,
        Admin
    }

    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        // משתמש מסוג Customer מקושר ללקוח. למנהל אין לקוח
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
    }
}
