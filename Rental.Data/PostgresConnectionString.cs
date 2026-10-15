using Npgsql;

namespace MyProject.Data
{
    // Render (ושירותי ענן אחרים) נותנים את פרטי החיבור ככתובת URL:
    //   postgres://user:password@host:5432/database
    // Npgsql מצפה לפורמט של מפתח=ערך. המחלקה מקבלת את שני הפורמטים
    public static class PostgresConnectionString
    {
        public static string Normalize(string connectionString)
        {
            if (!connectionString.StartsWith("postgres://") && !connectionString.StartsWith("postgresql://"))
            {
                return connectionString;
            }

            var uri = new Uri(connectionString);
            var credentials = uri.UserInfo.Split(':', 2);
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Database = uri.AbsolutePath.TrimStart('/'),
                Username = Uri.UnescapeDataString(credentials[0]),
                Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : null
            };

            // כתובת חיצונית (עם נקודה בשם השרת) עוברת באינטרנט, ולכן מוצפנת
            if (uri.Host.Contains('.'))
            {
                builder.SslMode = SslMode.Require;
                builder.TrustServerCertificate = true;
            }

            return builder.ConnectionString;
        }
    }
}
