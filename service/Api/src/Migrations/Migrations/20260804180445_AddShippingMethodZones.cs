using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingMethodZones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shipping_method_zones",
                schema: "shipping",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shipping_method_id = table.Column<Guid>(type: "uuid", nullable: false),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    state_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shipping_method_zones", x => x.id);
                    table.ForeignKey(
                        name: "fk_shipping_method_zones_shipping_methods_shipping_method_id",
                        column: x => x.shipping_method_id,
                        principalSchema: "shipping",
                        principalTable: "shipping_methods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shipping_method_zones_shipping_method_id_country_code_state",
                schema: "shipping",
                table: "shipping_method_zones",
                columns: new[] { "shipping_method_id", "country_code", "state_code" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shipping_method_zones",
                schema: "shipping");
        }
    }
}
