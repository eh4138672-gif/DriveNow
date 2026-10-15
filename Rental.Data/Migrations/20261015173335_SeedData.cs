using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.Data.Migrations
{
    public partial class SeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Cars",
                columns: new[] { "Id", "IsActive", "LastBookedAt", "LicenseNumber", "Model", "PricePerHour", "Version" },
                values: new object[,]
                {
                    { 1, true, null, "123-45-678", "Mazda 3", 50, new Guid("00000000-0000-0000-0000-000000000001") },
                    { 2, true, null, "987-65-432", "Toyota Corolla", 50, new Guid("00000000-0000-0000-0000-000000000002") },
                    { 101, true, null, "234-56-781", "Hyundai i20", 40, new Guid("00000000-0000-0000-0000-000000000101") },
                    { 102, true, null, "345-67-812", "Kia Picanto", 30, new Guid("00000000-0000-0000-0000-000000000102") },
                    { 103, true, null, "456-78-123", "Skoda Octavia", 60, new Guid("00000000-0000-0000-0000-000000000103") },
                    { 104, true, null, "567-81-234", "Tesla Model 3", 100, new Guid("00000000-0000-0000-0000-000000000104") },
                    { 105, true, null, "678-12-345", "Toyota RAV4 Hybrid", 80, new Guid("00000000-0000-0000-0000-000000000105") },
                    { 106, true, null, "789-23-456", "Kia Sorento", 80, new Guid("00000000-0000-0000-0000-000000000106") },
                    { 107, true, null, "891-34-567", "Mitsubishi Outlander", 70, new Guid("00000000-0000-0000-0000-000000000107") },
                    { 108, true, null, "12-345-67", "Suzuki Swift", 30, new Guid("00000000-0000-0000-0000-000000000108") },
                    { 109, true, null, "23-456-78", "Hyundai Ioniq 5", 90, new Guid("00000000-0000-0000-0000-000000000109") },
                    { 110, true, null, "34-567-89", "Volkswagen Golf", 60, new Guid("00000000-0000-0000-0000-000000000110") },
                    { 111, true, null, "45-678-91", "Chevrolet Spark", 30, new Guid("00000000-0000-0000-0000-000000000111") },
                    { 112, true, null, "56-789-12", "Seat Ibiza", 40, new Guid("00000000-0000-0000-0000-000000000112") }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "DriverLicenseId", "Name", "PhoneNumber" },
                values: new object[,]
                {
                    { 1, "99887766", "ישראל ישראלי", "050-1234567" },
                    { 2, "11223344", "דנה כהן", "052-7654321" },
                    { 101, "22334455", "יוסף לוי", "053-2345678" },
                    { 102, "33445566", "רחל אברהם", "054-3456789" },
                    { 103, "44556677", "משה פרידמן", "050-4567890" },
                    { 104, "55667788", "שרה גולדברג", "052-5678901" },
                    { 105, "66778899", "אברהם שפירא", "058-6789012" },
                    { 106, "77889900", "מרים כץ", "055-7890123" }
                });

            migrationBuilder.InsertData(
                table: "Features",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "אוטומטי" },
                    { 2, "חשמלי" },
                    { 3, "היברידי" },
                    { 4, "7 מקומות" },
                    { 5, "GPS" },
                    { 6, "מושב בטיחות לילד" },
                    { 7, "גג נפתח" },
                    { 8, "מצלמה אחורית" }
                });

            migrationBuilder.InsertData(
                table: "CarFeatures",
                columns: new[] { "CarId", "FeatureId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 5 },
                    { 2, 1 },
                    { 2, 3 },
                    { 101, 1 },
                    { 103, 1 },
                    { 103, 5 },
                    { 103, 8 },
                    { 104, 1 },
                    { 104, 2 },
                    { 104, 5 },
                    { 104, 7 },
                    { 104, 8 },
                    { 105, 1 },
                    { 105, 3 },
                    { 105, 5 },
                    { 105, 8 },
                    { 106, 1 },
                    { 106, 4 },
                    { 106, 5 },
                    { 106, 6 },
                    { 106, 8 },
                    { 107, 1 },
                    { 107, 4 },
                    { 107, 6 },
                    { 109, 1 },
                    { 109, 2 },
                    { 109, 5 },
                    { 109, 8 },
                    { 110, 1 },
                    { 110, 5 },
                    { 110, 7 },
                    { 112, 1 }
                });

            migrationBuilder.InsertData(
                table: "Rentals",
                columns: new[] { "Id", "CarId", "CustomerId", "DateEnd", "DateStart" },
                values: new object[,]
                {
                    { 1, 1, 1, new DateTime(2026, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 2, 2, new DateTime(2026, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 101, 1, 101, new DateTime(2026, 10, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 102, 101, 102, new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 103, 101, 103, new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 104, 103, 104, new DateTime(2026, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 105, 104, 1, new DateTime(2026, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 106, 105, 105, new DateTime(2026, 9, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 107, 105, 2, new DateTime(2026, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 108, 106, 106, new DateTime(2026, 10, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 109, 109, 101, new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 110, 110, 102, new DateTime(2026, 11, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 11, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 111, 2, 103, new DateTime(2026, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 112, 107, 104, new DateTime(2026, 8, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 113, 104, 105, new DateTime(2026, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 114, 1, 2, new DateTime(2026, 11, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 11, 20, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 1, 5 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 2, 3 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 101, 1 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 103, 1 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 103, 5 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 103, 8 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 104, 1 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 104, 2 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 104, 5 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 104, 7 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 104, 8 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 105, 1 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 105, 3 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 105, 5 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 105, 8 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 106, 1 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 106, 4 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 106, 5 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 106, 6 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 106, 8 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 107, 1 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 107, 4 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 107, 6 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 109, 1 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 109, 2 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 109, 5 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 109, 8 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 110, 1 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 110, 5 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 110, 7 });

            migrationBuilder.DeleteData(
                table: "CarFeatures",
                keyColumns: new[] { "CarId", "FeatureId" },
                keyValues: new object[] { 112, 1 });

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "Rentals",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Rentals",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Rentals",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Rentals",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Rentals",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Rentals",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Rentals",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "Rentals",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "Rentals",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "Rentals",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "Rentals",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "Rentals",
                keyColumn: "Id",
                keyValue: 110);

            migrationBuilder.DeleteData(
                table: "Rentals",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "Rentals",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "Rentals",
                keyColumn: "Id",
                keyValue: 113);

            migrationBuilder.DeleteData(
                table: "Rentals",
                keyColumn: "Id",
                keyValue: 114);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 110);

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
