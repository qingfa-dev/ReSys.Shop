using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddWebhookStoreShipmentsAndPaymentColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "shipment_state",
                schema: "ordering",
                table: "orders",
                newName: "fulfillment_state");

            migrationBuilder.AddColumn<decimal>(
                name: "captured_amount",
                schema: "payment",
                table: "payment_captures",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "payment_method_id",
                schema: "ordering",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "shipments",
                schema: "shipping",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shipping_method_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tracking_number = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    shipped_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    delivered_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    estimated_delivery_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shipments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "webhook_events",
                schema: "payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stripe_event_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    state = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    processed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webhook_events", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_orders_payment_method_id",
                schema: "ordering",
                table: "orders",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_order_id",
                schema: "shipping",
                table: "shipments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_order_id_status",
                schema: "shipping",
                table: "shipments",
                columns: new[] { "order_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_webhook_events_stripe_event_id",
                schema: "payment",
                table: "webhook_events",
                column: "stripe_event_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_orders_payment_methods_payment_method_id",
                schema: "ordering",
                table: "orders",
                column: "payment_method_id",
                principalSchema: "payment",
                principalTable: "payment_methods",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_orders_payment_methods_payment_method_id",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropTable(
                name: "shipments",
                schema: "shipping");

            migrationBuilder.DropTable(
                name: "webhook_events",
                schema: "payment");

            migrationBuilder.DropIndex(
                name: "ix_orders_payment_method_id",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "captured_amount",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.DropColumn(
                name: "payment_method_id",
                schema: "ordering",
                table: "orders");

            migrationBuilder.RenameColumn(
                name: "fulfillment_state",
                schema: "ordering",
                table: "orders",
                newName: "shipment_state");
        }
    }
}
