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
            migrationBuilder.DropColumn(
                name: "primary_media_id",
                schema: "catalog",
                table: "variants");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "inventory",
                table: "stock_transfers",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<Guid>(
                name: "payment_method_id",
                schema: "payment",
                table: "payment_captures",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

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

            migrationBuilder.AddColumn<Guid>(
                name: "primary_media_id",
                schema: "catalog",
                table: "variants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "payment_method_id",
                schema: "payment",
                table: "payment_captures",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
