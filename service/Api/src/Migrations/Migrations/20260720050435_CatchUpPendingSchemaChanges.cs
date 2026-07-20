using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class CatchUpPendingSchemaChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "capture_event_created",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.AddColumn<string>(
                name: "statement_descriptor_suffix",
                schema: "payment",
                table: "payment_methods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_status",
                schema: "payment",
                table: "payment_captures",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "statement_descriptor_suffix",
                schema: "payment",
                table: "payment_methods");

            migrationBuilder.DropColumn(
                name: "payment_status",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.AddColumn<bool>(
                name: "capture_event_created",
                schema: "payment",
                table: "payment_captures",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
