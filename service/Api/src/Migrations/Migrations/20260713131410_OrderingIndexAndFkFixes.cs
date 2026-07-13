using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class OrderingIndexAndFkFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_line_items_variants_variant_id",
                schema: "ordering",
                table: "line_items");

            migrationBuilder.CreateIndex(
                name: "ix_orders_session_id_status",
                schema: "ordering",
                table: "orders",
                columns: new[] { "session_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_user_id_status",
                schema: "ordering",
                table: "orders",
                columns: new[] { "user_id", "status" });

            migrationBuilder.AddForeignKey(
                name: "fk_line_items_variants_variant_id",
                schema: "ordering",
                table: "line_items",
                column: "variant_id",
                principalSchema: "catalog",
                principalTable: "variants",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_line_items_variants_variant_id",
                schema: "ordering",
                table: "line_items");

            migrationBuilder.DropIndex(
                name: "ix_orders_session_id_status",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_orders_user_id_status",
                schema: "ordering",
                table: "orders");

            migrationBuilder.AddForeignKey(
                name: "fk_line_items_variants_variant_id",
                schema: "ordering",
                table: "line_items",
                column: "variant_id",
                principalSchema: "catalog",
                principalTable: "variants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
