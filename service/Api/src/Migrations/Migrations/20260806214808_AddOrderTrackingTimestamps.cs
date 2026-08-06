using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderTrackingTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "delivered_at",
                schema: "ordering",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "delivery_exception_at",
                schema: "ordering",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "estimated_delivery_at",
                schema: "ordering",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "payment_completed_at",
                schema: "ordering",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "payment_failed_at",
                schema: "ordering",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "payment_processing_at",
                schema: "ordering",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "shipped_at",
                schema: "ordering",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "delivered_at",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "delivery_exception_at",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "estimated_delivery_at",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "payment_completed_at",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "payment_failed_at",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "payment_processing_at",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipped_at",
                schema: "ordering",
                table: "orders");
        }
    }
}
