using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class FixPaymentMethodSettingsColumnType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_line_items_variants_variant_id",
                schema: "ordering",
                table: "line_items");

            migrationBuilder.DropForeignKey(
                name: "fk_payment_capture_order_order_id",
                schema: "payment",
                table: "payment_capture");

            migrationBuilder.DropForeignKey(
                name: "fk_payment_capture_orders_order_id1",
                schema: "payment",
                table: "payment_capture");

            migrationBuilder.DropForeignKey(
                name: "fk_stock_item_variant_variant_id",
                schema: "inventory",
                table: "stock_item");

            migrationBuilder.DropIndex(
                name: "ix_stock_item_variant_id",
                schema: "inventory",
                table: "stock_item");

            migrationBuilder.DropIndex(
                name: "ix_payment_capture_order_id1",
                schema: "payment",
                table: "payment_capture");

            migrationBuilder.DropColumn(
                name: "order_id1",
                schema: "payment",
                table: "payment_capture");

            migrationBuilder.AlterColumn<string>(
                name: "settings",
                schema: "payment",
                table: "payment_method",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AddForeignKey(
                name: "fk_line_items_variants_variant_id",
                schema: "ordering",
                table: "line_items",
                column: "variant_id",
                principalSchema: "catalog",
                principalTable: "variants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_payment_capture_orders_order_id",
                schema: "payment",
                table: "payment_capture",
                column: "order_id",
                principalSchema: "ordering",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_line_items_variants_variant_id",
                schema: "ordering",
                table: "line_items");

            migrationBuilder.DropForeignKey(
                name: "fk_payment_capture_orders_order_id",
                schema: "payment",
                table: "payment_capture");

            migrationBuilder.AlterColumn<string>(
                name: "settings",
                schema: "payment",
                table: "payment_method",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "order_id1",
                schema: "payment",
                table: "payment_capture",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_stock_item_variant_id",
                schema: "inventory",
                table: "stock_item",
                column: "variant_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_capture_order_id1",
                schema: "payment",
                table: "payment_capture",
                column: "order_id1");

            migrationBuilder.AddForeignKey(
                name: "fk_line_items_variants_variant_id",
                schema: "ordering",
                table: "line_items",
                column: "variant_id",
                principalSchema: "catalog",
                principalTable: "variants",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_payment_capture_order_order_id",
                schema: "payment",
                table: "payment_capture",
                column: "order_id",
                principalSchema: "ordering",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_payment_capture_orders_order_id1",
                schema: "payment",
                table: "payment_capture",
                column: "order_id1",
                principalSchema: "ordering",
                principalTable: "orders",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_stock_item_variant_variant_id",
                schema: "inventory",
                table: "stock_item",
                column: "variant_id",
                principalSchema: "catalog",
                principalTable: "variants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
