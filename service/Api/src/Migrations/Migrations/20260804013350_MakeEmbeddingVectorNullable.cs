using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MakeEmbeddingVectorNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Vector>(
                name: "vector",
                schema: "catalog",
                table: "product_image_embeddings",
                type: "vector(512)",
                nullable: true,
                oldClrType: typeof(Vector),
                oldType: "vector(512)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Vector>(
                name: "vector",
                schema: "catalog",
                table: "product_image_embeddings",
                type: "vector(512)",
                nullable: false,
                oldClrType: typeof(Vector),
                oldType: "vector(512)",
                oldNullable: true);
        }
    }
}
