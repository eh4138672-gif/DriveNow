using Microsoft.EntityFrameworkCore;
using MyProject.Core.Entities;
using MyProject.Data;

namespace MyProject.Tests
{
    // בסיס נתונים נפרד לבדיקות, על PostgreSQL אמיתי. בדיקות ה-concurrency צריכות שני חיבורים
    // נפרדים לאותה שורה, ולכן הן לא יכולות לרוץ על InMemory.
    // מחרוזת החיבור נקראת ממשתנה הסביבה RENTAL_TEST_DB (ראו README), כדי שהסיסמה לא תהיה בקוד
    public class TestDatabaseFixture : IDisposable
    {
        public const string ConnectionStringVariable = "RENTAL_TEST_DB";

        private readonly string _connectionString;

        public TestDatabaseFixture()
        {
            _connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable)
                ?? Environment.GetEnvironmentVariable(ConnectionStringVariable, EnvironmentVariableTarget.User)
                ?? throw new InvalidOperationException(
                    $"Set the {ConnectionStringVariable} environment variable to a PostgreSQL connection string " +
                    "for a test database, e.g. Host=localhost;Database=rentaldb_tests;Username=postgres;Password=...");

            using var context = CreateContext();
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }

        public DataContext CreateContext() =>
            new(new DbContextOptionsBuilder<DataContext>().UseNpgsql(_connectionString).Options);

        // יוצר רכב חדש לכל בדיקה, כדי שהבדיקות לא ישפיעו זו על זו
        public async Task<int> CreateCarAsync()
        {
            await using var context = CreateContext();
            var car = new Car
            {
                LicenseNumber = Guid.NewGuid().ToString("N")[..20],
                Model = "Test Car",
                PricePerHour = 50
            };
            context.Cars.Add(car);
            await context.SaveChangesAsync();
            return car.Id;
        }

        public void Dispose()
        {
            using var context = CreateContext();
            context.Database.EnsureDeleted();
        }
    }
}
