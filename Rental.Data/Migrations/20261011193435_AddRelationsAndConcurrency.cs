using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.Data.Migrations
{
    public partial class AddRelationsAndConcurrency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Cars_CarId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Customers_CustId",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "IX_Rentals_CarId",
                table: "Rentals");

            migrationBuilder.RenameColumn(
                name: "CustId",
                table: "Rentals",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Rentals_CustId",
                table: "Rentals",
                newName: "IX_Rentals_CustomerId");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Customers",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DriverLicenseId",
                table: "Customers",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "Cars",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Cars",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastBookedAt",
                table: "Cars",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Cars",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_CarId_DateStart_DateEnd",
                table: "Rentals",
                columns: new[] { "CarId", "DateStart", "DateEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_DriverLicenseId",
                table: "Customers",
                column: "DriverLicenseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cars_LicenseNumber",
                table: "Cars",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Cars_CarId",
                table: "Rentals",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Customers_CustomerId",
                table: "Rentals",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Cars_CarId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Customers_CustomerId",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "IX_Rentals_CarId_DateStart_DateEnd",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "IX_Customers_DriverLicenseId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Cars_LicenseNumber",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "LastBookedAt",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Cars");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Rentals",
                newName: "CustId");

            migrationBuilder.RenameIndex(
                name: "IX_Rentals_CustomerId",
                table: "Rentals",
                newName: "IX_Rentals_CustId");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "DriverLicenseId",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_CarId",
                table: "Rentals",
                column: "CarId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Cars_CarId",
                table: "Rentals",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Customers_CustId",
                table: "Rentals",
                column: "CustId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
