using Microsoft.EntityFrameworkCore;
using MyProject.Core.Entities;

namespace MyProject.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Car> Cars => Set<Car>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Rental> Rentals => Set<Rental>();
        public DbSet<Feature> Features => Set<Feature>();
        public DbSet<User> Users => Set<User>();

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        // concurrency token מנוהל בקוד (הדרך שמתאימה ל-PostgreSQL, שאין בו rowversion):
        // בכל שמירה של רכב שנוסף או השתנה מחליפים את ה-Version. EF שולח את הערך הישן בתנאי
        // ה-WHERE, ולכן שמירה שמבוססת על נתון שהתיישן לא מוצאת שורה וזורקת DbUpdateConcurrencyException
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            RenewCarVersions();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            RenewCarVersions();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void RenewCarVersions()
        {
            foreach (var entry in ChangeTracker.Entries<Car>())
            {
                if (entry.State is EntityState.Added or EntityState.Modified)
                {
                    entry.Entity.Version = Guid.NewGuid();
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>(car =>
            {
                car.Property(c => c.LicenseNumber).HasMaxLength(20).IsRequired();
                car.HasIndex(c => c.LicenseNumber).IsUnique();
                car.Property(c => c.Model).HasMaxLength(50).IsRequired();
                car.Property(c => c.Version).IsConcurrencyToken();
                // LastBookedAt נשמר ב-UTC
                car.Property(c => c.LastBookedAt).HasColumnType("timestamp with time zone");

                // many-to-many: טבלת קישור CarFeatures עם מפתח מורכב
                car.HasMany(c => c.Features)
                    .WithMany(f => f.Cars)
                    .UsingEntity<Dictionary<string, object>>(
                        "CarFeatures",
                        join => join.HasOne<Feature>().WithMany().HasForeignKey("FeatureId"),
                        join => join.HasOne<Car>().WithMany().HasForeignKey("CarId"),
                        join =>
                        {
                            join.HasKey("CarId", "FeatureId");
                            join.HasData(CarFeatureSeed());
                        });
            });

            modelBuilder.Entity<Feature>(feature =>
            {
                feature.Property(f => f.Name).HasMaxLength(30).IsRequired();
                feature.HasIndex(f => f.Name).IsUnique();
            });

            modelBuilder.Entity<User>(user =>
            {
                user.Property(u => u.Email).HasMaxLength(100).IsRequired();
                user.HasIndex(u => u.Email).IsUnique();
                user.Property(u => u.PasswordHash).HasMaxLength(200).IsRequired();
                user.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

                // one-to-one אופציונלי: משתמש מסוג Customer מקושר ללקוח אחד
                user.HasOne(u => u.Customer)
                    .WithOne()
                    .HasForeignKey<User>(u => u.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Customer>(customer =>
            {
                customer.Property(c => c.Name).HasMaxLength(100).IsRequired();
                customer.Property(c => c.PhoneNumber).HasMaxLength(15).IsRequired();
                customer.Property(c => c.DriverLicenseId).HasMaxLength(9).IsRequired();
                customer.HasIndex(c => c.DriverLicenseId).IsUnique();
            });

            modelBuilder.Entity<Rental>(rental =>
            {
                // Restrict: אי אפשר למחוק רכב או לקוח שיש להם השכרות. ה-Service מחזיר על זה 409
                rental.HasOne(r => r.Car)
                    .WithMany(c => c.Rentals)
                    .HasForeignKey(r => r.CarId)
                    .OnDelete(DeleteBehavior.Restrict);

                rental.HasOne(r => r.Customer)
                    .WithMany(c => c.Rentals)
                    .HasForeignKey(r => r.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // לבדיקת חפיפה מהירה של תאריכים לרכב
                rental.HasIndex(r => new { r.CarId, r.DateStart, r.DateEnd });

                // תאריך ושעה מקומיים, בלי אזור זמן - כפי שהם נבחרים במסך ההזמנה
                rental.Property(r => r.DateStart).HasColumnType("timestamp without time zone");
                rental.Property(r => r.DateEnd).HasColumnType("timestamp without time zone");
            });

            // ב-PostgreSQL הכנסה עם Id מפורש (נתוני ה-Seed) לא מקדמת את מונה ה-identity.
            // לכן המונה מתחיל ב-1000, מעל כל ה-Id-ים של ה-Seed, ורשומות חדשות לא יתנגשו בהם
            modelBuilder.Entity<Car>().Property(c => c.Id).HasIdentityOptions(startValue: 1000);
            modelBuilder.Entity<Customer>().Property(c => c.Id).HasIdentityOptions(startValue: 1000);
            modelBuilder.Entity<Rental>().Property(r => r.Id).HasIdentityOptions(startValue: 1000);
            modelBuilder.Entity<Feature>().Property(f => f.Id).HasIdentityOptions(startValue: 1000);

            Seed(modelBuilder);
        }

        // משתמשי הדמו לא נוצרים כאן אלא בהפעלה (AuthService.SeedDemoUsersAsync),
        // כי הסיסמאות שלהם נטענות מהקונפיגורציה ועוברות hash
        private static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Feature>().HasData(
                new Feature { Id = 1, Name = "אוטומטי" },
                new Feature { Id = 2, Name = "חשמלי" },
                new Feature { Id = 3, Name = "היברידי" },
                new Feature { Id = 4, Name = "7 מקומות" },
                new Feature { Id = 5, Name = "GPS" },
                new Feature { Id = 6, Name = "מושב בטיחות לילד" },
                new Feature { Id = 7, Name = "גג נפתח" },
                new Feature { Id = 8, Name = "מצלמה אחורית" });

            var cars = new[]
            {
                // מחירים לשעה. כפולות של 10, כדי שגם המחיר אחרי 10% הנחה יהיה שקל שלם
                new Car { Id = 1, LicenseNumber = "123-45-678", Model = "Mazda 3", PricePerHour = 50 },
                new Car { Id = 2, LicenseNumber = "987-65-432", Model = "Toyota Corolla", PricePerHour = 50 },
                // נתוני ההדגמה הנוספים מתחילים ב-Id 101, כדי לא להתנגש ברשומות שנוצרו דרך האפליקציה
                new Car { Id = 101, LicenseNumber = "234-56-781", Model = "Hyundai i20", PricePerHour = 40 },
                new Car { Id = 102, LicenseNumber = "345-67-812", Model = "Kia Picanto", PricePerHour = 30 },
                new Car { Id = 103, LicenseNumber = "456-78-123", Model = "Skoda Octavia", PricePerHour = 60 },
                new Car { Id = 104, LicenseNumber = "567-81-234", Model = "Tesla Model 3", PricePerHour = 100 },
                new Car { Id = 105, LicenseNumber = "678-12-345", Model = "Toyota RAV4 Hybrid", PricePerHour = 80 },
                new Car { Id = 106, LicenseNumber = "789-23-456", Model = "Kia Sorento", PricePerHour = 80 },
                new Car { Id = 107, LicenseNumber = "891-34-567", Model = "Mitsubishi Outlander", PricePerHour = 70 },
                new Car { Id = 108, LicenseNumber = "12-345-67", Model = "Suzuki Swift", PricePerHour = 30 },
                new Car { Id = 109, LicenseNumber = "23-456-78", Model = "Hyundai Ioniq 5", PricePerHour = 90 },
                new Car { Id = 110, LicenseNumber = "34-567-89", Model = "Volkswagen Golf", PricePerHour = 60 },
                new Car { Id = 111, LicenseNumber = "45-678-91", Model = "Chevrolet Spark", PricePerHour = 30 },
                new Car { Id = 112, LicenseNumber = "56-789-12", Model = "Seat Ibiza", PricePerHour = 40 }
            };
            // ערך Version קבוע לנתוני ה-Seed (ולא Guid אקראי), אחרת כל migration חדש היה "משנה" אותם
            foreach (var car in cars)
            {
                car.Version = new Guid($"00000000-0000-0000-0000-{car.Id:D12}");
            }
            modelBuilder.Entity<Car>().HasData(cars);

            modelBuilder.Entity<Customer>().HasData(
                new Customer { Id = 1, Name = "ישראל ישראלי", PhoneNumber = "050-1234567", DriverLicenseId = "99887766" },
                new Customer { Id = 2, Name = "דנה כהן", PhoneNumber = "052-7654321", DriverLicenseId = "11223344" },
                new Customer { Id = 101, Name = "יוסף לוי", PhoneNumber = "053-2345678", DriverLicenseId = "22334455" },
                new Customer { Id = 102, Name = "רחל אברהם", PhoneNumber = "054-3456789", DriverLicenseId = "33445566" },
                new Customer { Id = 103, Name = "משה פרידמן", PhoneNumber = "050-4567890", DriverLicenseId = "44556677" },
                new Customer { Id = 104, Name = "שרה גולדברג", PhoneNumber = "052-5678901", DriverLicenseId = "55667788" },
                new Customer { Id = 105, Name = "אברהם שפירא", PhoneNumber = "058-6789012", DriverLicenseId = "66778899" },
                new Customer { Id = 106, Name = "מרים כץ", PhoneNumber = "055-7890123", DriverLicenseId = "77889900" });

            // השכרות בעבר, פעילות ועתידיות. אין חפיפה בין השכרות של אותו רכב
            modelBuilder.Entity<Rental>().HasData(
                SeedRental(1, carId: 1, customerId: 1, 2026, 10, 1, days: 5),
                SeedRental(2, carId: 2, customerId: 2, 2026, 10, 3, days: 6),
                SeedRental(101, carId: 1, customerId: 101, 2026, 10, 20, days: 4),
                SeedRental(102, carId: 101, customerId: 102, 2026, 9, 15, days: 5),
                SeedRental(103, carId: 101, customerId: 103, 2026, 10, 10, days: 4),
                SeedRental(104, carId: 103, customerId: 104, 2026, 10, 11, days: 7),
                SeedRental(105, carId: 104, customerId: 1, 2026, 10, 25, days: 5),
                SeedRental(106, carId: 105, customerId: 105, 2026, 9, 1, days: 7),
                SeedRental(107, carId: 105, customerId: 2, 2026, 11, 1, days: 4),
                SeedRental(108, carId: 106, customerId: 106, 2026, 10, 14, days: 7),
                SeedRental(109, carId: 109, customerId: 101, 2026, 10, 8, days: 7),
                SeedRental(110, carId: 110, customerId: 102, 2026, 11, 10, days: 3),
                SeedRental(111, carId: 2, customerId: 103, 2026, 10, 15, days: 4),
                SeedRental(112, carId: 107, customerId: 104, 2026, 8, 20, days: 7),
                SeedRental(113, carId: 104, customerId: 105, 2026, 10, 5, days: 4),
                SeedRental(114, carId: 1, customerId: 2, 2026, 11, 20, days: 5));
        }

        private static Rental SeedRental(int id, int carId, int customerId, int year, int month, int day, int days)
        {
            var start = new DateTime(year, month, day);
            return new Rental { Id = id, CarId = carId, CustomerId = customerId, DateStart = start, DateEnd = start.AddDays(days) };
        }

        // מפתחות תכונה: 1 אוטומטי, 2 חשמלי, 3 היברידי, 4 7 מקומות, 5 GPS, 6 מושב לילד, 7 גג נפתח, 8 מצלמה אחורית
        private static object[] CarFeatureSeed()
        {
            var featuresByCar = new Dictionary<int, int[]>
            {
                [1] = new[] { 1, 5 },
                [2] = new[] { 1, 3 },
                [101] = new[] { 1 },
                [103] = new[] { 1, 5, 8 },
                [104] = new[] { 1, 2, 5, 7, 8 },
                [105] = new[] { 1, 3, 5, 8 },
                [106] = new[] { 1, 4, 5, 6, 8 },
                [107] = new[] { 1, 4, 6 },
                [109] = new[] { 1, 2, 5, 8 },
                [110] = new[] { 1, 5, 7 },
                [112] = new[] { 1 }
            };

            return featuresByCar
                .SelectMany(pair => pair.Value.Select(featureId => (object)new { CarId = pair.Key, FeatureId = featureId }))
                .ToArray();
        }
    }
}
