using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveConnect.infrastructure.Migrations
{
    public partial class AddSubscriptionBillingFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillingCycle",
                table: "Subscriptions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Monthly");

            migrationBuilder.AddColumn<decimal>(
                name: "BillingAmount",
                table: "Subscriptions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                "UPDATE [Subscriptions] SET [BillingAmount] = [MonthlyFee];");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingAmount",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "BillingCycle",
                table: "Subscriptions");
        }
    }
}
