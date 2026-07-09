using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class RefactorWebhookConstants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment",
                schema: "payment");

            migrationBuilder.DropColumn(
                name: "additional_tax_total",
                schema: "shipping",
                table: "shipping_rate");

            migrationBuilder.DropColumn(
                name: "included_tax_total",
                schema: "shipping",
                table: "shipping_rate");

            migrationBuilder.DropColumn(
                name: "tax_amount",
                schema: "shipping",
                table: "shipping_rate");

            migrationBuilder.DropColumn(
                name: "additional_tax_total",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "cart_promo_total",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "included_tax_total",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "promo_total",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "tax_total",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "included_tax_total",
                schema: "ordering",
                table: "line_items");

            migrationBuilder.DropColumn(
                name: "pre_tax_amount",
                schema: "ordering",
                table: "line_items");

            migrationBuilder.DropColumn(
                name: "promo_total",
                schema: "ordering",
                table: "line_items");

            migrationBuilder.DropColumn(
                name: "tax_total",
                schema: "ordering",
                table: "line_items");

            migrationBuilder.CreateTable(
                name: "payment_record",
                schema: "payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    state = table.Column<string>(type: "text", nullable: false, defaultValue: "Checkout"),
                    response_code = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    avs_response = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    cvv_response_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    cvv_response_message = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    intent_client_secret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    capture_event_created = table.Column<bool>(type: "boolean", nullable: false),
                    payment_method_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_record", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_record_order_order_id",
                        column: x => x.order_id,
                        principalSchema: "ordering",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_payment_record_payment_method_payment_method_id",
                        column: x => x.payment_method_id,
                        principalSchema: "payment",
                        principalTable: "payment_method",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payment_record_order_id",
                schema: "payment",
                table: "payment_record",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_record_payment_method_id",
                schema: "payment",
                table: "payment_record",
                column: "payment_method_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_record",
                schema: "payment");

            migrationBuilder.AddColumn<decimal>(
                name: "additional_tax_total",
                schema: "shipping",
                table: "shipping_rate",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "included_tax_total",
                schema: "shipping",
                table: "shipping_rate",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "tax_amount",
                schema: "shipping",
                table: "shipping_rate",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "additional_tax_total",
                schema: "ordering",
                table: "orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "cart_promo_total",
                schema: "ordering",
                table: "orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "included_tax_total",
                schema: "ordering",
                table: "orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "promo_total",
                schema: "ordering",
                table: "orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "tax_total",
                schema: "ordering",
                table: "orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "included_tax_total",
                schema: "ordering",
                table: "line_items",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "pre_tax_amount",
                schema: "ordering",
                table: "line_items",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "promo_total",
                schema: "ordering",
                table: "line_items",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "tax_total",
                schema: "ordering",
                table: "line_items",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "payment",
                schema: "payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_method_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    avs_response = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    capture_event_created = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cvv_response_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    cvv_response_message = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    intent_client_secret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    response_code = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    state = table.Column<string>(type: "text", nullable: false, defaultValue: "Checkout")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "ordering",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_payment_payment_method_payment_method_id",
                        column: x => x.payment_method_id,
                        principalSchema: "payment",
                        principalTable: "payment_method",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payment_order_id",
                schema: "payment",
                table: "payment",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_payment_method_id",
                schema: "payment",
                table: "payment",
                column: "payment_method_id");
        }
    }
}
