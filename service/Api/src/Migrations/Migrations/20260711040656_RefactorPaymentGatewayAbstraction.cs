using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPaymentGatewayAbstraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_record",
                schema: "payment");

            migrationBuilder.DropColumn(
                name: "provider_type",
                schema: "payment",
                table: "payment_method");

            migrationBuilder.DropColumn(
                name: "webhook_secret",
                schema: "payment",
                table: "payment_method");

            migrationBuilder.DropColumn(
                name: "webhook_url",
                schema: "payment",
                table: "payment_method");

            migrationBuilder.AlterColumn<bool>(
                name: "webhook_enabled",
                schema: "payment",
                table: "payment_method",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "preferences",
                schema: "payment",
                table: "payment_method",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "provider_key",
                schema: "payment",
                table: "payment_method",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "settings",
                schema: "payment",
                table: "payment_method",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "payment_capture",
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
                    refunded_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    provider_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payment_method_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    order_id1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_capture", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_capture_order_order_id",
                        column: x => x.order_id,
                        principalSchema: "ordering",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_payment_capture_orders_order_id1",
                        column: x => x.order_id1,
                        principalSchema: "ordering",
                        principalTable: "orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_payment_capture_payment_method_payment_method_id",
                        column: x => x.payment_method_id,
                        principalSchema: "payment",
                        principalTable: "payment_method",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payment_capture_order_id",
                schema: "payment",
                table: "payment_capture",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_capture_order_id1",
                schema: "payment",
                table: "payment_capture",
                column: "order_id1");

            migrationBuilder.CreateIndex(
                name: "ix_payment_capture_payment_method_id",
                schema: "payment",
                table: "payment_capture",
                column: "payment_method_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_capture",
                schema: "payment");

            migrationBuilder.DropColumn(
                name: "provider_key",
                schema: "payment",
                table: "payment_method");

            migrationBuilder.DropColumn(
                name: "settings",
                schema: "payment",
                table: "payment_method");

            migrationBuilder.AlterColumn<bool>(
                name: "webhook_enabled",
                schema: "payment",
                table: "payment_method",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "preferences",
                schema: "payment",
                table: "payment_method",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AddColumn<string>(
                name: "provider_type",
                schema: "payment",
                table: "payment_method",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "webhook_secret",
                schema: "payment",
                table: "payment_method",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "webhook_url",
                schema: "payment",
                table: "payment_method",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "payment_record",
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
    }
}
