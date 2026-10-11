using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.Data.Migrations
{
    public partial class AddMoreSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Cars",
                columns: new[] { "Id", "LastBookedAt", "LicenseNumber", "Model", "Price" },
                values: new object[,]
                {
                    { 101, null, "234-56-781", "Hyundai i20", 150 },
                    { 102, null, "345-67-812", "Kia Picanto", 130 },
                    { 103, null, "456-78-123", "Skoda Octavia", 240 },
                    { 104, null, "567-81-234", "Tesla Model 3", 420 },
                    { 105, null, "678-12-345", "Toyota RAV4 Hybrid", 350 },
                    { 106, null, "789-23-456", "Kia Sorento", 380 },
                    { 107, null, "891-34-567", "Mitsubishi Outlander", 330 },
                    { 108, null, "12-345-67", "Suzuki Swift", 140 },
                    { 109, null, "23-456-78", "Hyundai Ioniq 5", 400 },
                    { 110, null, "34-567-89", "Volkswagen Golf", 260 },
                    { 111, null, "45-678-91", "Chevrolet Spark", 120 },
                    { 112, null, "56-789-12", "Seat Ibiza", 170 }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "DriverLicenseId", "Name", "PhoneNumber" },
                values: new object[,]
                {
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
                    { 6, "מושב בטיחות לילד" },
                    { 7, "גג נפתח" },
                    { 8, "מצלמה אחורית" }
                });

            migrationBuilder.InsertData(
                table: "Rentals",
                columns: new[] { "Id", "CarId", "CustomerId", "DateEnd", "DateStart" },
                values: new object[] { 114, 1, 2, new DateTime(2026, 11, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 11, 20, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "CarFeatures",
                columns: new[] { "CarId", "FeatureId" },
                values: new object[,]
                {
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
                    { 113, 104, 105, new DateTime(2026, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
