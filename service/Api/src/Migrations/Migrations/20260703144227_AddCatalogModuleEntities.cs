using Microsoft.EntityFrameworkCore.Migrations;

using Pgvector;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogModuleEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateTable(
                name: "option_types",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    presentation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    filterable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_option_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                    slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    meta_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    meta_description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    meta_keywords = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    available_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    discontinue_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    make_active_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    style_code = table.Column<string>(type: "text", nullable: true),
                    season_name = table.Column<string>(type: "text", nullable: true),
                    material_composition = table.Column<string>(type: "text", nullable: true),
                    care_instructions = table.Column<string>(type: "text", nullable: true),
                    fit_notes = table.Column<string>(type: "text", nullable: true),
                    department = table.Column<string>(type: "text", nullable: true),
                    gender_target = table.Column<string>(type: "text", nullable: true),
                    master_variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tax_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    marked_for_regenerate_taxon_products = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "taxonomies",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    presentation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_taxonomies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "option_values",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    presentation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    option_type_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_option_values", x => x.id);
                    table.ForeignKey(
                        name: "fk_option_values_option_types_option_type_id",
                        column: x => x.option_type_id,
                        principalSchema: "catalog",
                        principalTable: "option_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_option_types",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    option_type_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_option_types", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_option_types_option_types_option_type_id",
                        column: x => x.option_type_id,
                        principalSchema: "catalog",
                        principalTable: "option_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_product_option_types_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "variants",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_master = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    sku = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    track_inventory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    barcode = table.Column<string>(type: "text", nullable: true),
                    hs_code = table.Column<string>(type: "text", nullable: true),
                    discontinued_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    weight = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true, defaultValue: 0m),
                    weight_unit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true, defaultValue: "Kg"),
                    height = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true, defaultValue: 0m),
                    width = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true, defaultValue: 0m),
                    depth = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true, defaultValue: 0m),
                    dimensions_unit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true, defaultValue: "Cm"),
                    primary_media_id = table.Column<Guid>(type: "uuid", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true, defaultValue: 0m),
                    cost_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true, defaultValue: 0m),
                    cost_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true, defaultValue: "USD"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_variants", x => x.id);
                    table.ForeignKey(
                        name: "fk_variants_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "taxa",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    presentation = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    description_html = table.Column<string>(type: "text", nullable: true),
                    position = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    children_count = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    lft = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    rgt = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    depth = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    automatic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    rules_match_policy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "All"),
                    sort_order = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Manual"),
                    hide_from_nav = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    marked_for_regenerate_taxon_products = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    permalink = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    pretty_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    image_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    square_image_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    meta_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    meta_description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    meta_keywords = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    taxonomy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_taxa", x => x.id);
                    table.ForeignKey(
                        name: "fk_taxa_taxa_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "catalog",
                        principalTable: "taxa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_taxa_taxonomy_taxonomy_id",
                        column: x => x.taxonomy_id,
                        principalSchema: "catalog",
                        principalTable: "taxonomies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "option_value_variants",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    option_value_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_option_value_variants", x => x.id);
                    table.ForeignKey(
                        name: "fk_option_value_variants_option_values_option_value_id",
                        column: x => x.option_value_id,
                        principalSchema: "catalog",
                        principalTable: "option_values",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_option_value_variants_variant_variant_id",
                        column: x => x.variant_id,
                        principalSchema: "catalog",
                        principalTable: "variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prices",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    compare_at_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    country_iso = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    price_list_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prices", x => x.id);
                    table.ForeignKey(
                        name: "fk_prices_variant_variant_id",
                        column: x => x.variant_id,
                        principalSchema: "catalog",
                        principalTable: "variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_images",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "image/jpeg"),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_size = table.Column<int>(type: "integer", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: true),
                    height = table.Column<int>(type: "integer", nullable: true),
                    dimensions_unit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true, defaultValue: "px"),
                    position = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    storage_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    alt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, defaultValue: "Product image"),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Default"),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_images_variants_variant_id",
                        column: x => x.variant_id,
                        principalSchema: "catalog",
                        principalTable: "variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "classifications",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_automatic = table.Column<bool>(type: "boolean", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    taxon_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_classifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_classifications_product_product_id",
                        column: x => x.product_id,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_classifications_taxon_taxon_id",
                        column: x => x.taxon_id,
                        principalSchema: "catalog",
                        principalTable: "taxa",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "taxon_rules",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    taxon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    match_policy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_taxon_rules", x => x.id);
                    table.ForeignKey(
                        name: "fk_taxon_rules_taxa_taxon_id",
                        column: x => x.taxon_id,
                        principalSchema: "catalog",
                        principalTable: "taxa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_image_embeddings",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    model_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    model_version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    vector = table.Column<Vector>(type: "vector", nullable: false),
                    dimensions = table.Column<int>(type: "integer", nullable: false),
                    variant_image_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_image_embeddings", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_image_embeddings_variant_image_variant_image_id",
                        column: x => x.variant_image_id,
                        principalSchema: "catalog",
                        principalTable: "product_images",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_classifications_product_id_taxon_id",
                schema: "catalog",
                table: "classifications",
                columns: new[] { "product_id", "taxon_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_classifications_taxon_id",
                schema: "catalog",
                table: "classifications",
                column: "taxon_id");

            migrationBuilder.CreateIndex(
                name: "ix_option_value_variants_option_value_id",
                schema: "catalog",
                table: "option_value_variants",
                column: "option_value_id");

            migrationBuilder.CreateIndex(
                name: "ix_option_value_variants_variant_id_option_value_id",
                schema: "catalog",
                table: "option_value_variants",
                columns: new[] { "variant_id", "option_value_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_option_values_option_type_id",
                schema: "catalog",
                table: "option_values",
                column: "option_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_prices_variant_id",
                schema: "catalog",
                table: "prices",
                column: "variant_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_image_embeddings_variant_image_id",
                schema: "catalog",
                table: "product_image_embeddings",
                column: "variant_image_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_images_variant_id",
                schema: "catalog",
                table: "product_images",
                column: "variant_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_option_types_option_type_id",
                schema: "catalog",
                table: "product_option_types",
                column: "option_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_option_types_product_id_option_type_id",
                schema: "catalog",
                table: "product_option_types",
                columns: new[] { "product_id", "option_type_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_slug",
                schema: "catalog",
                table: "products",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_taxa_parent_id",
                schema: "catalog",
                table: "taxa",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_taxa_slug",
                schema: "catalog",
                table: "taxa",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_taxa_taxonomy_id",
                schema: "catalog",
                table: "taxa",
                column: "taxonomy_id");

            migrationBuilder.CreateIndex(
                name: "ix_taxon_rules_taxon_id",
                schema: "catalog",
                table: "taxon_rules",
                column: "taxon_id");

            migrationBuilder.CreateIndex(
                name: "ix_variants_product_id",
                schema: "catalog",
                table: "variants",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_variants_sku",
                schema: "catalog",
                table: "variants",
                column: "sku",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "classifications",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "option_value_variants",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "prices",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "product_image_embeddings",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "product_option_types",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "taxon_rules",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "option_values",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "product_images",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "taxa",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "option_types",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "variants",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "taxonomies",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "products",
                schema: "catalog");
        }
    }
}
