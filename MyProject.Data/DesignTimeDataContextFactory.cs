using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyProject.Data
{
    // משמש רק את dotnet ef ביצירת migrations, בלי להריץ את Program.cs.
    // יצירת migration לא מתחברת לבסיס הנתונים, ולכן מספיקה מחרוזת חיבור כללית.
    // את ה-migrations עצמם מריץ השרת בהפעלה (Database.MigrateAsync)
    public class DesignTimeDataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__RentalDb")
                ?? "Host=localhost;Database=rentaldb;Username=postgres";

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseNpgsql(PostgresConnectionString.Normalize(connectionString))
                .Options;
            return new DataContext(options);
        }
    }
}
