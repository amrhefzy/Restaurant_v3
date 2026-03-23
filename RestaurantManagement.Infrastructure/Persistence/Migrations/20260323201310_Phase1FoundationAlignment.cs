using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase1FoundationAlignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModifiedOnUtc",
                table: "Suppliers",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUtc",
                table: "Suppliers",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "ModifiedOnUtc",
                table: "SalesReturns",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUtc",
                table: "SalesReturns",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "ModifiedOnUtc",
                table: "SalesOrders",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUtc",
                table: "SalesOrders",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "ModifiedOnUtc",
                table: "RestaurantTables",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUtc",
                table: "RestaurantTables",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "ModifiedOnUtc",
                table: "Reservations",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUtc",
                table: "Reservations",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "ModifiedOnUtc",
                table: "PurchaseReturns",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUtc",
                table: "PurchaseReturns",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "ModifiedOnUtc",
                table: "PurchaseOrders",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUtc",
                table: "PurchaseOrders",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "ModifiedOnUtc",
                table: "Products",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUtc",
                table: "Products",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "ModifiedOnUtc",
                table: "InventoryMovements",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUtc",
                table: "InventoryMovements",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "ModifiedOnUtc",
                table: "Customers",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUtc",
                table: "Customers",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "ModifiedOnUtc",
                table: "Categories",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUtc",
                table: "Categories",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "ModifiedOnUtc",
                table: "Branches",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUtc",
                table: "Branches",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "ModifiedOnUtc",
                table: "AppSettings",
                newName: "ModifiedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUtc",
                table: "AppSettings",
                newName: "CreatedOn");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "SalesOrders",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "SalesOrderItems",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "SalesOrderItems");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "Suppliers",
                newName: "ModifiedOnUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Suppliers",
                newName: "CreatedOnUtc");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "SalesReturns",
                newName: "ModifiedOnUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "SalesReturns",
                newName: "CreatedOnUtc");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "SalesOrders",
                newName: "ModifiedOnUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "SalesOrders",
                newName: "CreatedOnUtc");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "RestaurantTables",
                newName: "ModifiedOnUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "RestaurantTables",
                newName: "CreatedOnUtc");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "Reservations",
                newName: "ModifiedOnUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Reservations",
                newName: "CreatedOnUtc");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "PurchaseReturns",
                newName: "ModifiedOnUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "PurchaseReturns",
                newName: "CreatedOnUtc");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "PurchaseOrders",
                newName: "ModifiedOnUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "PurchaseOrders",
                newName: "CreatedOnUtc");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "Products",
                newName: "ModifiedOnUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Products",
                newName: "CreatedOnUtc");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "InventoryMovements",
                newName: "ModifiedOnUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "InventoryMovements",
                newName: "CreatedOnUtc");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "Customers",
                newName: "ModifiedOnUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Customers",
                newName: "CreatedOnUtc");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "Categories",
                newName: "ModifiedOnUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Categories",
                newName: "CreatedOnUtc");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "Branches",
                newName: "ModifiedOnUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Branches",
                newName: "CreatedOnUtc");

            migrationBuilder.RenameColumn(
                name: "ModifiedOn",
                table: "AppSettings",
                newName: "ModifiedOnUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "AppSettings",
                newName: "CreatedOnUtc");
        }
    }
}
