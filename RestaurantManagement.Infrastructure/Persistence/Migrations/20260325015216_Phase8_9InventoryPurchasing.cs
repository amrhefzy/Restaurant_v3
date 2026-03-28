using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase8_9InventoryPurchasing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashierShifts_Branches_BranchId",
                table: "CashierShifts");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_Branches_BranchId",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_Branches_BranchId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseReturns");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_SalesOrders_Branches_BranchId1'
      AND parent_object_id = OBJECT_ID(N'[SalesOrders]'))
BEGIN
    ALTER TABLE [SalesOrders] DROP CONSTRAINT [FK_SalesOrders_Branches_BranchId1];
END
");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Customers_CustomerId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_RestaurantTables_TableId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_Branches_BranchId",
                table: "SalesReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_SalesOrders_SalesOrderId",
                table: "SalesReturns");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_SalesOrders_BranchId1'
      AND object_id = OBJECT_ID(N'[SalesOrders]'))
BEGIN
    DROP INDEX [IX_SalesOrders_BranchId1] ON [SalesOrders];
END
");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE Name = N'BranchId1'
      AND Object_ID = OBJECT_ID(N'[SalesOrders]'))
BEGIN
    ALTER TABLE [SalesOrders] DROP COLUMN [BranchId1];
END
");

            migrationBuilder.AddForeignKey(
                name: "FK_CashierShifts_Branches_BranchId",
                table: "CashierShifts",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_Branches_BranchId",
                table: "PaymentTransactions",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_Branches_BranchId",
                table: "PurchaseReturns",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseReturns",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Customers_CustomerId",
                table: "SalesOrders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_RestaurantTables_TableId",
                table: "SalesOrders",
                column: "TableId",
                principalTable: "RestaurantTables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_Branches_BranchId",
                table: "SalesReturns",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_SalesOrders_SalesOrderId",
                table: "SalesReturns",
                column: "SalesOrderId",
                principalTable: "SalesOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashierShifts_Branches_BranchId",
                table: "CashierShifts");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_Branches_BranchId",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_Branches_BranchId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Customers_CustomerId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_RestaurantTables_TableId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_Branches_BranchId",
                table: "SalesReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_SalesOrders_SalesOrderId",
                table: "SalesReturns");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId1",
                table: "SalesOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_BranchId1",
                table: "SalesOrders",
                column: "BranchId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CashierShifts_Branches_BranchId",
                table: "CashierShifts",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_Branches_BranchId",
                table: "PaymentTransactions",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_Branches_BranchId",
                table: "PurchaseReturns",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseReturns",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Branches_BranchId1",
                table: "SalesOrders",
                column: "BranchId1",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Customers_CustomerId",
                table: "SalesOrders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_RestaurantTables_TableId",
                table: "SalesOrders",
                column: "TableId",
                principalTable: "RestaurantTables",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_Branches_BranchId",
                table: "SalesReturns",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_SalesOrders_SalesOrderId",
                table: "SalesReturns",
                column: "SalesOrderId",
                principalTable: "SalesOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
