using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedStripeEventIdsToPaymentCaptures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "processed_stripe_event_ids",
                schema: "payment",
                table: "payment_captures",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "payment",
                table: "payment_captures",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "processed_stripe_event_ids",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "payment",
                table: "payment_captures");
        }
    }
}
