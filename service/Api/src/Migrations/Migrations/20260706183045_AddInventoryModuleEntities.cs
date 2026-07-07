using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryModuleEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tax_category_id",
                schema: "catalog",
                table: "products");

            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.CreateTable(
                name: "stock_location",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    presentation = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    address1 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    address2 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    country_id = table.Column<Guid>(type: "uuid", nullable: true),
                    state_id = table.Column<Guid>(type: "uuid", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    @default = table.Column<bool>(name: "default", type: "boolean", nullable: false, defaultValue: false),
                    backorderable_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    propagate_all_variants = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    admin_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    position = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    low_stock_threshold = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    notify_on_low_stock = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    store_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_location", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stock_reservation",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_location_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    state = table.Column<string>(type: "text", nullable: false),
                    expires_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cart_token = table.Column<string>(type: "text", nullable: true),
                    line_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_reservation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stock_item",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    count_on_hand = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    backorderable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    stock_location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_stock_item_stock_location_stock_location_id",
                        column: x => x.stock_location_id,
                        principalSchema: "inventory",
                        principalTable: "stock_location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_stock_item_variant_variant_id",
                        column: x => x.variant_id,
                        principalSchema: "catalog",
                        principalTable: "variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_transfer",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    source_location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destination_location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_transfer", x => x.id);
                    table.ForeignKey(
                        name: "fk_stock_transfer_stock_location_destination_location_id",
                        column: x => x.destination_location_id,
                        principalSchema: "inventory",
                        principalTable: "stock_location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_stock_transfer_stock_location_source_location_id",
                        column: x => x.source_location_id,
                        principalSchema: "inventory",
                        principalTable: "stock_location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_movement",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    previous_count_on_hand = table.Column<int>(type: "integer", nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    stock_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_location_id = table.Column<Guid>(type: "uuid", nullable: true),
                    originator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    originator_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_movement", x => x.id);
                    table.ForeignKey(
                        name: "fk_stock_movement_stock_item_stock_item_id",
                        column: x => x.stock_item_id,
                        principalSchema: "inventory",
                        principalTable: "stock_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_stock_movement_stock_location_stock_location_id",
                        column: x => x.stock_location_id,
                        principalSchema: "inventory",
                        principalTable: "stock_location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transfer_item",
                schema: "inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_transfer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    received_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transfer_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_transfer_item_stock_transfer_stock_transfer_id",
                        column: x => x.stock_transfer_id,
                        principalSchema: "inventory",
                        principalTable: "stock_transfer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_stock_item_stock_location_id_variant_id",
                schema: "inventory",
                table: "stock_item",
                columns: new[] { "stock_location_id", "variant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stock_item_variant_id",
                schema: "inventory",
                table: "stock_item",
                column: "variant_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_movement_stock_item_id",
                schema: "inventory",
                table: "stock_movement",
                column: "stock_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_movement_stock_location_id",
                schema: "inventory",
                table: "stock_movement",
                column: "stock_location_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_transfer_destination_location_id",
                schema: "inventory",
                table: "stock_transfer",
                column: "destination_location_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_transfer_source_location_id",
                schema: "inventory",
                table: "stock_transfer",
                column: "source_location_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_transfer_state",
                schema: "inventory",
                table: "stock_transfer",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "ix_transfer_item_stock_transfer_id",
                schema: "inventory",
                table: "transfer_item",
                column: "stock_transfer_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_movement",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "stock_reservation",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "transfer_item",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "stock_item",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "stock_transfer",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "stock_location",
                schema: "inventory");

            migrationBuilder.AddColumn<Guid>(
                name: "tax_category_id",
                schema: "catalog",
                table: "products",
                type: "uuid",
                nullable: true);
        }
    }
}
