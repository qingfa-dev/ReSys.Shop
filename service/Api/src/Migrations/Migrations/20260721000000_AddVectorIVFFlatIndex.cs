using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddVectorIVFFlatIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS idx_embeddings_vector_ivfflat
                ON catalog.product_image_embeddings
                USING ivfflat (vector vector_cosine_ops)
                WITH (lists = 100)
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS catalog.idx_embeddings_vector_ivfflat
                """);
        }
    }
}
