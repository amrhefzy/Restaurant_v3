using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase8_InventoryPurchasing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryMovements_Branches_BranchId",
                table: "InventoryMovements");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryMovements_BranchId",
                table: "InventoryMovements");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PurchaseOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedOnUtc",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReceivedQuantity",
                table: "PurchaseOrderItems",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "InventoryMovements",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_BranchId_Status_CreatedOn",
                table: "PurchaseOrders",
                columns: new[] { "BranchId", "Status", "CreatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId_ProductId",
                table: "PurchaseOrderItems",
                columns: new[] { "PurchaseOrderId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_BranchId_MovementType_CreatedOn",
                table: "InventoryMovements",
                columns: new[] { "BranchId", "MovementType", "CreatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_BranchId_ProductId_CreatedOn",
                table: "InventoryMovements",
                columns: new[] { "BranchId", "ProductId", "CreatedOn" });

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryMovements_Branches_BranchId",
                table: "InventoryMovements",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryMovements_Branches_BranchId",
                table: "InventoryMovements");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_BranchId_Status_CreatedOn",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId_ProductId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryMovements_BranchId_MovementType_CreatedOn",
                table: "InventoryMovements");

            migrationBuilder.DropIndex(
                name: "IX_InventoryMovements_BranchId_ProductId_CreatedOn",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "SubmittedOnUtc",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ReceivedQuantity",
                table: "PurchaseOrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "InventoryMovements",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId",
                table: "PurchaseOrderItems",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_BranchId",
                table: "InventoryMovements",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryMovements_Branches_BranchId",
                table: "InventoryMovements",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
