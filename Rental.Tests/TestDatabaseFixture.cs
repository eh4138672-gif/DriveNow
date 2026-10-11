using Microsoft.EntityFrameworkCore;
using MyProject.Core.Entities;
using MyProject.Data;

namespace Rental.Tests
{
    // בסיס נתונים נפרד לבדיקות, על LocalDB. rowversion קיים רק ב-SQL Server,
    // ולכן בדיקות ה-concurrency לא יכולות לרוץ על InMemory
    public class TestDatabaseFixture : IDisposable
    {
        private const string ConnectionString =
            @"Server=(localdb)\MSSQLLocalDB;Database=RentalDb_Tests;Trusted_Connection=True;TrustServerCertificate=True";

        public TestDatabaseFixture()
        {
            using var context = CreateContext();
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }

        public DataContext CreateContext() =>
            new(new DbContextOptionsBuilder<DataContext>().UseSqlServer(ConnectionString).Options);

        // יוצר רכב חדש לכל בדיקה, כדי שהבדיקות לא ישפיעו זו על זו
        public async Task<int> CreateCarAsync()
        {
            await using var context = CreateContext();
            var car = new Car
            {
                LicenseNumber = Guid.NewGuid().ToString("N")[..20],
                Model = "Test Car",
                Price = 100
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
