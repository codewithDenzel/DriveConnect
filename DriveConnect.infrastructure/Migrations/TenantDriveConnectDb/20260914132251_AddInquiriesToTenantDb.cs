using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveConnect.infrastructure.Migrations.TenantDriveConnectDb
{
    /// <inheritdoc />
    public partial class AddInquiriesToTenantDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inquiries_Customers_CustomerId",
                table: "Inquiries");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Inquiries_CustomerId",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "Inquiries");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "Inquiries",
                newName: "ValidationNotes");

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
                oldDefaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "CarModel",
                table: "Inquiries",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Inquiries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Concern",
                table: "Inquiries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                table: "Inquiries",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "Inquiries",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Inquiries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HandledBy",
                table: "Inquiries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Inquiries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "Inquiries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Inquiries",
                type: "nvarchar(50)",
                maxLength: 50,
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarModel",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "Concern",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "EmailAddress",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "EstimatedCost",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "HandledBy",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "PickedUpAt",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "PickupStatus",
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

            migrationBuilder.RenameColumn(
                name: "ValidationNotes",
                table: "Inquiries",
                newName: "Message");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Inquiries",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Diagnose");

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "Inquiries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "Inquiries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    LeadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstimatedValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LeadName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "New")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.LeadId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_CustomerId",
                table: "Inquiries",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inquiries_Customers_CustomerId",
                table: "Inquiries",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
