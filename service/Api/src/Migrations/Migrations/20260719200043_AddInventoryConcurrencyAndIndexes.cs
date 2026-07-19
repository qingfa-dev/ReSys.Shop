using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryConcurrencyAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "inventory",
                table: "stock_transfers",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "inventory",
                table: "stock_reservations",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateIndex(
                name: "ix_stock_reservations_cart_token_state",
                schema: "inventory",
                table: "stock_reservations",
                columns: new[] { "cart_token", "state" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_reservations_order_id_state",
                schema: "inventory",
                table: "stock_reservations",
                columns: new[] { "order_id", "state" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_stock_reservations_cart_token_state",
                schema: "inventory",
                table: "stock_reservations");

            migrationBuilder.DropIndex(
                name: "ix_stock_reservations_order_id_state",
                schema: "inventory",
                table: "stock_reservations");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "inventory",
                table: "stock_transfers");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "inventory",
                table: "stock_reservations");
        }
    }
}
