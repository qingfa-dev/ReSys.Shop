using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class FixPluralTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_payment_capture_orders_order_id",
                schema: "payment",
                table: "payment_capture");

            migrationBuilder.DropForeignKey(
                name: "fk_payment_capture_payment_method_payment_method_id",
                schema: "payment",
                table: "payment_capture");

            migrationBuilder.DropForeignKey(
                name: "fk_state_country_country_id",
                schema: "location",
                table: "state");

            migrationBuilder.DropForeignKey(
                name: "fk_stock_item_stock_location_stock_location_id",
                schema: "inventory",
                table: "stock_item");

            migrationBuilder.DropForeignKey(
                name: "fk_stock_movement_stock_item_stock_item_id",
                schema: "inventory",
                table: "stock_movement");

            migrationBuilder.DropForeignKey(
                name: "fk_stock_movement_stock_location_stock_location_id",
                schema: "inventory",
                table: "stock_movement");

            migrationBuilder.DropForeignKey(
                name: "fk_stock_transfer_stock_location_destination_location_id",
                schema: "inventory",
                table: "stock_transfer");

            migrationBuilder.DropForeignKey(
                name: "fk_stock_transfer_stock_location_source_location_id",
                schema: "inventory",
                table: "stock_transfer");

            migrationBuilder.DropForeignKey(
                name: "fk_transfer_item_stock_transfer_stock_transfer_id",
                schema: "inventory",
                table: "transfer_item");

            migrationBuilder.DropPrimaryKey(
                name: "pk_transfer_item",
                schema: "inventory",
                table: "transfer_item");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stock_transfer",
                schema: "inventory",
                table: "stock_transfer");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stock_reservation",
                schema: "inventory",
                table: "stock_reservation");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stock_movement",
                schema: "inventory",
                table: "stock_movement");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stock_location",
                schema: "inventory",
                table: "stock_location");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stock_item",
                schema: "inventory",
                table: "stock_item");

            migrationBuilder.DropPrimaryKey(
                name: "pk_state",
                schema: "location",
                table: "state");

            migrationBuilder.DropPrimaryKey(
                name: "pk_shipping_rate",
                schema: "shipping",
                table: "shipping_rate");

            migrationBuilder.DropPrimaryKey(
                name: "pk_shipping_method",
                schema: "shipping",
                table: "shipping_method");

            migrationBuilder.DropPrimaryKey(
                name: "pk_payment_method",
                schema: "payment",
                table: "payment_method");

            migrationBuilder.DropPrimaryKey(
                name: "pk_payment_capture",
                schema: "payment",
                table: "payment_capture");

            migrationBuilder.DropPrimaryKey(
                name: "pk_country",
                schema: "location",
                table: "country");

            migrationBuilder.RenameTable(
                name: "transfer_item",
                schema: "inventory",
                newName: "transfer_items",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "stock_transfer",
                schema: "inventory",
                newName: "stock_transfers",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "stock_reservation",
                schema: "inventory",
                newName: "stock_reservations",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "stock_movement",
                schema: "inventory",
                newName: "stock_movements",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "stock_location",
                schema: "inventory",
                newName: "stock_locations",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "stock_item",
                schema: "inventory",
                newName: "stock_items",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "state",
                schema: "location",
                newName: "states",
                newSchema: "location");

            migrationBuilder.RenameTable(
                name: "shipping_rate",
                schema: "shipping",
                newName: "shipping_rates",
                newSchema: "shipping");

            migrationBuilder.RenameTable(
                name: "shipping_method",
                schema: "shipping",
                newName: "shipping_methods",
                newSchema: "shipping");

            migrationBuilder.RenameTable(
                name: "payment_method",
                schema: "payment",
                newName: "payment_methods",
                newSchema: "payment");

            migrationBuilder.RenameTable(
                name: "payment_capture",
                schema: "payment",
                newName: "payment_captures",
                newSchema: "payment");

            migrationBuilder.RenameTable(
                name: "country",
                schema: "location",
                newName: "countries",
                newSchema: "location");

            migrationBuilder.RenameIndex(
                name: "ix_transfer_item_stock_transfer_id",
                schema: "inventory",
                table: "transfer_items",
                newName: "ix_transfer_items_stock_transfer_id");

            migrationBuilder.RenameIndex(
                name: "ix_stock_transfer_state",
                schema: "inventory",
                table: "stock_transfers",
                newName: "ix_stock_transfers_state");

            migrationBuilder.RenameIndex(
                name: "ix_stock_transfer_source_location_id",
                schema: "inventory",
                table: "stock_transfers",
                newName: "ix_stock_transfers_source_location_id");

            migrationBuilder.RenameIndex(
                name: "ix_stock_transfer_destination_location_id",
                schema: "inventory",
                table: "stock_transfers",
                newName: "ix_stock_transfers_destination_location_id");

            migrationBuilder.RenameIndex(
                name: "ix_stock_movement_stock_location_id",
                schema: "inventory",
                table: "stock_movements",
                newName: "ix_stock_movements_stock_location_id");

            migrationBuilder.RenameIndex(
                name: "ix_stock_movement_stock_item_id",
                schema: "inventory",
                table: "stock_movements",
                newName: "ix_stock_movements_stock_item_id");

            migrationBuilder.RenameIndex(
                name: "ix_stock_item_stock_location_id_variant_id",
                schema: "inventory",
                table: "stock_items",
                newName: "ix_stock_items_stock_location_id_variant_id");

            migrationBuilder.RenameIndex(
                name: "ix_state_country_id",
                schema: "location",
                table: "states",
                newName: "ix_states_country_id");

            migrationBuilder.RenameIndex(
                name: "ix_shipping_method_code",
                schema: "shipping",
                table: "shipping_methods",
                newName: "ix_shipping_methods_code");

            migrationBuilder.RenameIndex(
                name: "ix_payment_capture_payment_method_id",
                schema: "payment",
                table: "payment_captures",
                newName: "ix_payment_captures_payment_method_id");

            migrationBuilder.RenameIndex(
                name: "ix_payment_capture_order_id",
                schema: "payment",
                table: "payment_captures",
                newName: "ix_payment_captures_order_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_transfer_items",
                schema: "inventory",
                table: "transfer_items",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stock_transfers",
                schema: "inventory",
                table: "stock_transfers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stock_reservations",
                schema: "inventory",
                table: "stock_reservations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stock_movements",
                schema: "inventory",
                table: "stock_movements",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stock_locations",
                schema: "inventory",
                table: "stock_locations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stock_items",
                schema: "inventory",
                table: "stock_items",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_states",
                schema: "location",
                table: "states",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_shipping_rates",
                schema: "shipping",
                table: "shipping_rates",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_shipping_methods",
                schema: "shipping",
                table: "shipping_methods",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_payment_methods",
                schema: "payment",
                table: "payment_methods",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_payment_captures",
                schema: "payment",
                table: "payment_captures",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_countries",
                schema: "location",
                table: "countries",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_payment_captures_orders_order_id",
                schema: "payment",
                table: "payment_captures",
                column: "order_id",
                principalSchema: "ordering",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_payment_captures_payment_methods_payment_method_id",
                schema: "payment",
                table: "payment_captures",
                column: "payment_method_id",
                principalSchema: "payment",
                principalTable: "payment_methods",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_states_countries_country_id",
                schema: "location",
                table: "states",
                column: "country_id",
                principalSchema: "location",
                principalTable: "countries",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_stock_items_stock_location_stock_location_id",
                schema: "inventory",
                table: "stock_items",
                column: "stock_location_id",
                principalSchema: "inventory",
                principalTable: "stock_locations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_stock_movements_stock_items_stock_item_id",
                schema: "inventory",
                table: "stock_movements",
                column: "stock_item_id",
                principalSchema: "inventory",
                principalTable: "stock_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_stock_movements_stock_locations_stock_location_id",
                schema: "inventory",
                table: "stock_movements",
                column: "stock_location_id",
                principalSchema: "inventory",
                principalTable: "stock_locations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_stock_transfers_stock_locations_destination_location_id",
                schema: "inventory",
                table: "stock_transfers",
                column: "destination_location_id",
                principalSchema: "inventory",
                principalTable: "stock_locations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_stock_transfers_stock_locations_source_location_id",
                schema: "inventory",
                table: "stock_transfers",
                column: "source_location_id",
                principalSchema: "inventory",
                principalTable: "stock_locations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_transfer_items_stock_transfers_stock_transfer_id",
                schema: "inventory",
                table: "transfer_items",
                column: "stock_transfer_id",
                principalSchema: "inventory",
                principalTable: "stock_transfers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_payment_captures_orders_order_id",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.DropForeignKey(
                name: "fk_payment_captures_payment_methods_payment_method_id",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.DropForeignKey(
                name: "fk_states_countries_country_id",
                schema: "location",
                table: "states");

            migrationBuilder.DropForeignKey(
                name: "fk_stock_items_stock_location_stock_location_id",
                schema: "inventory",
                table: "stock_items");

            migrationBuilder.DropForeignKey(
                name: "fk_stock_movements_stock_items_stock_item_id",
                schema: "inventory",
                table: "stock_movements");

            migrationBuilder.DropForeignKey(
                name: "fk_stock_movements_stock_locations_stock_location_id",
                schema: "inventory",
                table: "stock_movements");

            migrationBuilder.DropForeignKey(
                name: "fk_stock_transfers_stock_locations_destination_location_id",
                schema: "inventory",
                table: "stock_transfers");

            migrationBuilder.DropForeignKey(
                name: "fk_stock_transfers_stock_locations_source_location_id",
                schema: "inventory",
                table: "stock_transfers");

            migrationBuilder.DropForeignKey(
                name: "fk_transfer_items_stock_transfers_stock_transfer_id",
                schema: "inventory",
                table: "transfer_items");

            migrationBuilder.DropPrimaryKey(
                name: "pk_transfer_items",
                schema: "inventory",
                table: "transfer_items");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stock_transfers",
                schema: "inventory",
                table: "stock_transfers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stock_reservations",
                schema: "inventory",
                table: "stock_reservations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stock_movements",
                schema: "inventory",
                table: "stock_movements");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stock_locations",
                schema: "inventory",
                table: "stock_locations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_stock_items",
                schema: "inventory",
                table: "stock_items");

            migrationBuilder.DropPrimaryKey(
                name: "pk_states",
                schema: "location",
                table: "states");

            migrationBuilder.DropPrimaryKey(
                name: "pk_shipping_rates",
                schema: "shipping",
                table: "shipping_rates");

            migrationBuilder.DropPrimaryKey(
                name: "pk_shipping_methods",
                schema: "shipping",
                table: "shipping_methods");

            migrationBuilder.DropPrimaryKey(
                name: "pk_payment_methods",
                schema: "payment",
                table: "payment_methods");

            migrationBuilder.DropPrimaryKey(
                name: "pk_payment_captures",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.DropPrimaryKey(
                name: "pk_countries",
                schema: "location",
                table: "countries");

            migrationBuilder.RenameTable(
                name: "transfer_items",
                schema: "inventory",
                newName: "transfer_item",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "stock_transfers",
                schema: "inventory",
                newName: "stock_transfer",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "stock_reservations",
                schema: "inventory",
                newName: "stock_reservation",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "stock_movements",
                schema: "inventory",
                newName: "stock_movement",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "stock_locations",
                schema: "inventory",
                newName: "stock_location",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "stock_items",
                schema: "inventory",
                newName: "stock_item",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "states",
                schema: "location",
                newName: "state",
                newSchema: "location");

            migrationBuilder.RenameTable(
                name: "shipping_rates",
                schema: "shipping",
                newName: "shipping_rate",
                newSchema: "shipping");

            migrationBuilder.RenameTable(
                name: "shipping_methods",
                schema: "shipping",
                newName: "shipping_method",
                newSchema: "shipping");

            migrationBuilder.RenameTable(
                name: "payment_methods",
                schema: "payment",
                newName: "payment_method",
                newSchema: "payment");

            migrationBuilder.RenameTable(
                name: "payment_captures",
                schema: "payment",
                newName: "payment_capture",
                newSchema: "payment");

            migrationBuilder.RenameTable(
                name: "countries",
                schema: "location",
                newName: "country",
                newSchema: "location");

            migrationBuilder.RenameIndex(
                name: "ix_transfer_items_stock_transfer_id",
                schema: "inventory",
                table: "transfer_item",
                newName: "ix_transfer_item_stock_transfer_id");

            migrationBuilder.RenameIndex(
                name: "ix_stock_transfers_state",
                schema: "inventory",
                table: "stock_transfer",
                newName: "ix_stock_transfer_state");

            migrationBuilder.RenameIndex(
                name: "ix_stock_transfers_source_location_id",
                schema: "inventory",
                table: "stock_transfer",
                newName: "ix_stock_transfer_source_location_id");

            migrationBuilder.RenameIndex(
                name: "ix_stock_transfers_destination_location_id",
                schema: "inventory",
                table: "stock_transfer",
                newName: "ix_stock_transfer_destination_location_id");

            migrationBuilder.RenameIndex(
                name: "ix_stock_movements_stock_location_id",
                schema: "inventory",
                table: "stock_movement",
                newName: "ix_stock_movement_stock_location_id");

            migrationBuilder.RenameIndex(
                name: "ix_stock_movements_stock_item_id",
                schema: "inventory",
                table: "stock_movement",
                newName: "ix_stock_movement_stock_item_id");

            migrationBuilder.RenameIndex(
                name: "ix_stock_items_stock_location_id_variant_id",
                schema: "inventory",
                table: "stock_item",
                newName: "ix_stock_item_stock_location_id_variant_id");

            migrationBuilder.RenameIndex(
                name: "ix_states_country_id",
                schema: "location",
                table: "state",
                newName: "ix_state_country_id");

            migrationBuilder.RenameIndex(
                name: "ix_shipping_methods_code",
                schema: "shipping",
                table: "shipping_method",
                newName: "ix_shipping_method_code");

            migrationBuilder.RenameIndex(
                name: "ix_payment_captures_payment_method_id",
                schema: "payment",
                table: "payment_capture",
                newName: "ix_payment_capture_payment_method_id");

            migrationBuilder.RenameIndex(
                name: "ix_payment_captures_order_id",
                schema: "payment",
                table: "payment_capture",
                newName: "ix_payment_capture_order_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_transfer_item",
                schema: "inventory",
                table: "transfer_item",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stock_transfer",
                schema: "inventory",
                table: "stock_transfer",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stock_reservation",
                schema: "inventory",
                table: "stock_reservation",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stock_movement",
                schema: "inventory",
                table: "stock_movement",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stock_location",
                schema: "inventory",
                table: "stock_location",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_stock_item",
                schema: "inventory",
                table: "stock_item",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_state",
                schema: "location",
                table: "state",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_shipping_rate",
                schema: "shipping",
                table: "shipping_rate",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_shipping_method",
                schema: "shipping",
                table: "shipping_method",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_payment_method",
                schema: "payment",
                table: "payment_method",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_payment_capture",
                schema: "payment",
                table: "payment_capture",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_country",
                schema: "location",
                table: "country",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_payment_capture_orders_order_id",
                schema: "payment",
                table: "payment_capture",
                column: "order_id",
                principalSchema: "ordering",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_payment_capture_payment_method_payment_method_id",
                schema: "payment",
                table: "payment_capture",
                column: "payment_method_id",
                principalSchema: "payment",
                principalTable: "payment_method",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_state_country_country_id",
                schema: "location",
                table: "state",
                column: "country_id",
                principalSchema: "location",
                principalTable: "country",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_stock_item_stock_location_stock_location_id",
                schema: "inventory",
                table: "stock_item",
                column: "stock_location_id",
                principalSchema: "inventory",
                principalTable: "stock_location",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_stock_movement_stock_item_stock_item_id",
                schema: "inventory",
                table: "stock_movement",
                column: "stock_item_id",
                principalSchema: "inventory",
                principalTable: "stock_item",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_stock_movement_stock_location_stock_location_id",
                schema: "inventory",
                table: "stock_movement",
                column: "stock_location_id",
                principalSchema: "inventory",
                principalTable: "stock_location",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_stock_transfer_stock_location_destination_location_id",
                schema: "inventory",
                table: "stock_transfer",
                column: "destination_location_id",
                principalSchema: "inventory",
                principalTable: "stock_location",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_stock_transfer_stock_location_source_location_id",
                schema: "inventory",
                table: "stock_transfer",
                column: "source_location_id",
                principalSchema: "inventory",
                principalTable: "stock_location",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_transfer_item_stock_transfer_stock_transfer_id",
                schema: "inventory",
                table: "transfer_item",
                column: "stock_transfer_id",
                principalSchema: "inventory",
                principalTable: "stock_transfer",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
