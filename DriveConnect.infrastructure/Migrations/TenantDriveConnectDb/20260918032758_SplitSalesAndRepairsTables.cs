using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveConnect.infrastructure.Migrations.TenantDriveConnectDb
{
    /// <inheritdoc />
    public partial class SplitSalesAndRepairsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Concern",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "PickedUpAt",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "PickupStatus",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "ValidationNotes",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "ValidationStatus",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "WhenRequested",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "WhenValidated",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "WhoRequested",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "WhoValidated",
                table: "Inquiries");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Inquiries",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "New Inquiry",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Diagnose");

            migrationBuilder.AlterColumn<string>(
                name: "MiddleName",
                table: "Inquiries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "HandledBy",
                table: "Inquiries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "EmailAddress",
                table: "Inquiries",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.CreateTable(
                name: "RepairTickets",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CarModel = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Concern = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Diagnose"),
                    EstimatedCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HandledBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PickupStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Pending"),
                    PickedUpAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairTickets", x => x.TicketId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepairTickets");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Inquiries",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Diagnose",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValue: "New Inquiry");

            migrationBuilder.AlterColumn<string>(
                name: "MiddleName",
                table: "Inquiries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HandledBy",
                table: "Inquiries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmailAddress",
                table: "Inquiries",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Concern",
                table: "Inquiries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PickedUpAt",
                table: "Inquiries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupStatus",
                table: "Inquiries",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "ValidationNotes",
                table: "Inquiries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ValidationStatus",
                table: "Inquiries",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "WhenRequested",
                table: "Inquiries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "WhenValidated",
                table: "Inquiries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhoRequested",
                table: "Inquiries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhoValidated",
                table: "Inquiries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
