using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileAndIdentityModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "profiles");

            migrationBuilder.EnsureSchema(
                name: "location");

            migrationBuilder.RenameTable(
                name: "state",
                schema: "locations",
                newName: "state",
                newSchema: "location");

            migrationBuilder.RenameTable(
                name: "country",
                schema: "locations",
                newName: "country",
                newSchema: "location");

            migrationBuilder.AlterColumn<string>(
                name: "phone_number",
                schema: "identity",
                table: "users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                schema: "identity",
                table: "user_tokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "login_provider",
                schema: "identity",
                table: "user_tokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "provider_key",
                schema: "identity",
                table: "user_logins",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "login_provider",
                schema: "identity",
                table: "user_logins",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.CreateTable(
                name: "wishlists",
                schema: "profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_private = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("pk_wishlists", x => x.id);
                    table.ForeignKey(
                        name: "fk_wishlists_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wished_items",
                schema: "profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wishlist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wished_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_wished_items_wishlist_wishlist_id",
                        column: x => x.wishlist_id,
                        principalSchema: "profiles",
                        principalTable: "wishlists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "addresses",
                schema: "profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    address_type = table.Column<string>(type: "text", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    zip_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_default_billing = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_default_shipping = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    country_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state_province = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    country_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    state_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    user_profile_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addresses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                schema: "profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    date_of_birth = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    preferences_preferred_style = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    preferences_preferred_fit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    preferences_favorite_colors = table.Column<List<string>>(type: "text[]", nullable: false),
                    preferences_favorite_categories = table.Column<List<string>>(type: "text[]", nullable: false),
                    preferences_preferred_brands = table.Column<List<string>>(type: "text[]", nullable: false),
                    preferences_size_top = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    preferences_size_bottom = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    preferences_shoe_size = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    notifications_enable_sms = table.Column<bool>(type: "boolean", nullable: false),
                    notifications_enable_email = table.Column<bool>(type: "boolean", nullable: false),
                    notifications_enable_newsfeeds = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    accepts_email_marketing = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    internal_note_html = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    default_billing_address_id = table.Column<Guid>(type: "uuid", nullable: true),
                    default_shipping_address_id = table.Column<Guid>(type: "uuid", nullable: true),
                    orders_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_spent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    last_order_completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_profiles_addresses_default_billing_address_id",
                        column: x => x.default_billing_address_id,
                        principalSchema: "profiles",
                        principalTable: "addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_user_profiles_addresses_default_shipping_address_id",
                        column: x => x.default_shipping_address_id,
                        principalSchema: "profiles",
                        principalTable: "addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_user_profiles_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_addresses_address_type_user_profile_id",
                schema: "profiles",
                table: "addresses",
                columns: new[] { "address_type", "user_profile_id" });

            migrationBuilder.CreateIndex(
                name: "ix_addresses_country_code",
                schema: "profiles",
                table: "addresses",
                column: "country_code");

            migrationBuilder.CreateIndex(
                name: "ix_addresses_user_profile_id",
                schema: "profiles",
                table: "addresses",
                column: "user_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_default_billing_address_id",
                schema: "profiles",
                table: "user_profiles",
                column: "default_billing_address_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_default_shipping_address_id",
                schema: "profiles",
                table: "user_profiles",
                column: "default_shipping_address_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_user_id",
                schema: "profiles",
                table: "user_profiles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_wished_items_wishlist_id_variant_id",
                schema: "profiles",
                table: "wished_items",
                columns: new[] { "wishlist_id", "variant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wishlists_token",
                schema: "profiles",
                table: "wishlists",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wishlists_user_id_is_default",
                schema: "profiles",
                table: "wishlists",
                columns: new[] { "user_id", "is_default" });

            migrationBuilder.AddForeignKey(
                name: "fk_addresses_user_profiles_user_profile_id",
                schema: "profiles",
                table: "addresses",
                column: "user_profile_id",
                principalSchema: "profiles",
                principalTable: "user_profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_addresses_user_profiles_user_profile_id",
                schema: "profiles",
                table: "addresses");

            migrationBuilder.DropTable(
                name: "wished_items",
                schema: "profiles");

            migrationBuilder.DropTable(
                name: "wishlists",
                schema: "profiles");

            migrationBuilder.DropTable(
                name: "user_profiles",
                schema: "profiles");

            migrationBuilder.DropTable(
                name: "addresses",
                schema: "profiles");

            migrationBuilder.EnsureSchema(
                name: "locations");

            migrationBuilder.RenameTable(
                name: "state",
                schema: "location",
                newName: "state",
                newSchema: "locations");

            migrationBuilder.RenameTable(
                name: "country",
                schema: "location",
                newName: "country",
                newSchema: "locations");

            migrationBuilder.AlterColumn<string>(
                name: "phone_number",
                schema: "identity",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                schema: "identity",
                table: "user_tokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "login_provider",
                schema: "identity",
                table: "user_tokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "provider_key",
                schema: "identity",
                table: "user_logins",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "login_provider",
                schema: "identity",
                table: "user_logins",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
