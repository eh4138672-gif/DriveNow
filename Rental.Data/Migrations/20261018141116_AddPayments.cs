using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.Data.Migrations
{
    public partial class AddPayments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AmountPaid",
                table: "Rentals",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardLast4",
                table: "Rentals",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Rentals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "Rentals",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "CardLast4",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Rentals");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "Rentals");
        }
    }
}
