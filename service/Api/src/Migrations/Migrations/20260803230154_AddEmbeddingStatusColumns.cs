using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbeddingStatusColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "completed_at_utc",
                schema: "catalog",
                table: "product_image_embeddings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "error",
                schema: "catalog",
                table: "product_image_embeddings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hangfire_job_id",
                schema: "catalog",
                table: "product_image_embeddings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "catalog",
                table: "product_image_embeddings",
                type: "text",
                nullable: false,
                defaultValue: "Completed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "completed_at_utc",
                schema: "catalog",
                table: "product_image_embeddings");

            migrationBuilder.DropColumn(
                name: "error",
                schema: "catalog",
                table: "product_image_embeddings");

            migrationBuilder.DropColumn(
                name: "hangfire_job_id",
                schema: "catalog",
                table: "product_image_embeddings");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "catalog",
                table: "product_image_embeddings");
        }
    }
}
