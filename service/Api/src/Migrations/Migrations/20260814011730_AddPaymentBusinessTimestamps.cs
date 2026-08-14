using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentBusinessTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "completed_at_utc",
                schema: "payment",
                table: "payment_captures",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "disputed_at_utc",
                schema: "payment",
                table: "payment_captures",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "failed_at_utc",
                schema: "payment",
                table: "payment_captures",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_stripe_event_created_at_utc",
                schema: "payment",
                table: "payment_captures",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_stripe_event_id",
                schema: "payment",
                table: "payment_captures",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "refunded_at_utc",
                schema: "payment",
                table: "payment_captures",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "voided_at_utc",
                schema: "payment",
                table: "payment_captures",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "completed_at_utc",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.DropColumn(
                name: "disputed_at_utc",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.DropColumn(
                name: "failed_at_utc",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.DropColumn(
                name: "last_stripe_event_created_at_utc",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.DropColumn(
                name: "last_stripe_event_id",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.DropColumn(
                name: "refunded_at_utc",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.DropColumn(
                name: "voided_at_utc",
                schema: "payment",
                table: "payment_captures");
        }
    }
}
