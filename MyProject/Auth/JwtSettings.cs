namespace MyProject.Auth
{
    // נטען מהסעיף Jwt בקונפיגורציה. Key לא נמצא ב-appsettings אלא ב-User Secrets
    public class JwtSettings
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; } = 120;
    }
}
