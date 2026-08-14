using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentCorrelationColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "processed_at_utc",
                schema: "payment",
                table: "payment_captures",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stripe_payment_intent_id",
                schema: "payment",
                table: "payment_captures",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stripe_session_id",
                schema: "payment",
                table: "payment_captures",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_captures_stripe_payment_intent_id",
                schema: "payment",
                table: "payment_captures",
                column: "stripe_payment_intent_id",
                filter: "stripe_payment_intent_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_payment_captures_stripe_session_id",
                schema: "payment",
                table: "payment_captures",
                column: "stripe_session_id",
                filter: "stripe_session_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_payment_captures_stripe_payment_intent_id",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.DropIndex(
                name: "ix_payment_captures_stripe_session_id",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.DropColumn(
                name: "processed_at_utc",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.DropColumn(
                name: "stripe_payment_intent_id",
                schema: "payment",
                table: "payment_captures");

            migrationBuilder.DropColumn(
                name: "stripe_session_id",
                schema: "payment",
                table: "payment_captures");
        }
    }
}
