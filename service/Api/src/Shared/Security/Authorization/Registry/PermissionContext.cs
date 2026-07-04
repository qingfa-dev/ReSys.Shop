using Shared.Security.Authorization.Features;
using Shared.Security.Identity.Domain.Permissions;

namespace Shared.Security.Authorization.Registry;

public static class PermissionContext
{
    public static class Domains
    {
        public static readonly OptionDescriptor<string> Admin = OptionDescriptor<string>.Option(
            "admin", "Admin", "Administration domain for permission identifiers.");
        public static readonly OptionDescriptor<string> Catalog = OptionDescriptor<string>.Option(
            "catalog", "Catalog", "Product catalog domain for permission identifiers.");
        public static readonly OptionDescriptor<string> Identity = OptionDescriptor<string>.Option(
            "identity", "Identity", "Identity and authentication domain for permission identifiers.");
        public static readonly OptionDescriptor<string> Ordering = OptionDescriptor<string>.Option(
            "ordering", "Ordering", "Order management domain for permission identifiers.");
        public static readonly OptionDescriptor<string> Inventory = OptionDescriptor<string>.Option(
            "inventory", "Inventory", "Inventory management domain for permission identifiers.");
        public static readonly OptionDescriptor<string> Configuration = OptionDescriptor<string>.Option(
            "configuration", "Configuration", "System configuration domain for permission identifiers.");
        public static readonly OptionDescriptor<string> Promotions = OptionDescriptor<string>.Option(
            "promotions", "Promotions", "Promotions and discounts domain for permission identifiers.");
        public static readonly OptionDescriptor<string> Dashboard = OptionDescriptor<string>.Option(
            "dashboard", "Dashboard", "Dashboard and reporting domain for permission identifiers.");
    }

    public static class Categories
    {
        public static readonly OptionDescriptor<string> Location = OptionDescriptor<string>.Option(
            "location", "Location", "Location-related permission category.");
        public static readonly OptionDescriptor<string> Identity = OptionDescriptor<string>.Option(
            "identity", "Identity", "Identity-related permission category.");
        public static readonly OptionDescriptor<string> Profile = OptionDescriptor<string>.Option(
            "profile", "Profile", "User profile permission category.");
        public static readonly OptionDescriptor<string> Catalog = OptionDescriptor<string>.Option(
            "catalog", "Catalog", "Catalog permission category.");
        public static readonly OptionDescriptor<string> Ordering = OptionDescriptor<string>.Option(
            "ordering", "Ordering", "Ordering permission category.");
        public static readonly OptionDescriptor<string> Inventory = OptionDescriptor<string>.Option(
            "inventory", "Inventory", "Inventory permission category.");
        public static readonly OptionDescriptor<string> Configuration = OptionDescriptor<string>.Option(
            "configuration", "Configuration", "Configuration permission category.");
        public static readonly OptionDescriptor<string> Merchandising = OptionDescriptor<string>.Option(
            "merchandising", "Merchandising", "Merchandising permission category.");
        public static readonly OptionDescriptor<string> Dashboard = OptionDescriptor<string>.Option(
            "dashboard", "Dashboard", "Dashboard permission category.");
    }

    public static class Actions
    {
        public static readonly OptionDescriptor<string> List = OptionDescriptor<string>.Option(
            "view", "View", "Allows viewing resources without modification.");
        public static readonly OptionDescriptor<string> Detail = OptionDescriptor<string>.Option(
            "read", "Read", "Allows reading resource details.");
        public static readonly OptionDescriptor<string> Create = OptionDescriptor<string>.Option(
            "create", "Create", "Allows creating new resources.");
        public static readonly OptionDescriptor<string> Update = OptionDescriptor<string>.Option(
            "update", "Update", "Allows updating existing resources.");
        public static readonly OptionDescriptor<string> Delete = OptionDescriptor<string>.Option(
            "delete", "Delete", "Allows deleting resources.");
        public static readonly OptionDescriptor<string> Assign = OptionDescriptor<string>.Option(
            "assign", "Assign", "Allows assigning resources or roles.");
        public static readonly OptionDescriptor<string> Revoke = OptionDescriptor<string>.Option(
            "revoke", "Revoke", "Allows revoking assignments.");
        public static readonly OptionDescriptor<string> Manage = OptionDescriptor<string>.Option(
            "manage", "Manage", "Allows full management access.");
        public static readonly OptionDescriptor<string> Sync = OptionDescriptor<string>.Option(
            "sync", "Sync", "Allows synchronizing data.");
        public static readonly OptionDescriptor<string> Restore = OptionDescriptor<string>.Option(
            "restore", "Restore", "Allows restoring deleted resources.");
        public static readonly OptionDescriptor<string> Cancel = OptionDescriptor<string>.Option(
            "cancel", "Cancel", "Allows cancelling orders or operations.");
        public static readonly OptionDescriptor<string> Refund = OptionDescriptor<string>.Option(
            "refund", "Refund", "Allows processing refunds.");
        public static readonly OptionDescriptor<string> Fulfill = OptionDescriptor<string>.Option(
            "fulfill", "Fulfill", "Allows fulfilling orders.");
        public static readonly OptionDescriptor<string> Ship = OptionDescriptor<string>.Option(
            "ship", "Ship", "Allows shipping orders.");
        public static readonly OptionDescriptor<string> Capture = OptionDescriptor<string>.Option(
            "capture", "Capture", "Allows capturing payments.");
        public static readonly OptionDescriptor<string> Void = OptionDescriptor<string>.Option(
            "void", "Void", "Allows voiding transactions.");
        public static readonly OptionDescriptor<string> Activate = OptionDescriptor<string>.Option(
            "activate", "Activate", "Allows activating resources.");
        public static readonly OptionDescriptor<string> Deactivate = OptionDescriptor<string>.Option(
            "deactivate", "Deactivate", "Allows deactivating resources.");
        public static readonly OptionDescriptor<string> Adjust = OptionDescriptor<string>.Option(
            "adjust", "Adjust", "Allows adjusting quantities or values.");
        public static readonly OptionDescriptor<string> Audit = OptionDescriptor<string>.Option(
            "audit", "Audit", "Allows auditing resource changes.");
        public static readonly OptionDescriptor<string> ManageAssets = OptionDescriptor<string>.Option(
            "manage_assets", "Manage Assets", "Allows managing resource assets.");
        public static readonly OptionDescriptor<string> ManageMetadata = OptionDescriptor<string>.Option(
            "manage_metadata", "Manage Metadata", "Allows managing resource metadata.");
        public static readonly OptionDescriptor<string> ManagePrice = OptionDescriptor<string>.Option(
            "manage_price", "Manage Price", "Allows managing pricing information.");
        public static readonly OptionDescriptor<string> Rebuild = OptionDescriptor<string>.Option(
            "rebuild", "Rebuild", "Allows rebuilding indexes or structures.");
        public static readonly OptionDescriptor<string> ManageRules = OptionDescriptor<string>.Option(
            "manage_rules", "Manage Rules", "Allows managing business rules.");
        public static readonly OptionDescriptor<string> ManageItems = OptionDescriptor<string>.Option(
            "manage_items", "Manage Items", "Allows managing line items.");
    }

    public static class Resources
    {
        public static readonly OptionDescriptor<string> Countries = OptionDescriptor<string>.Option(
            "countries", "Countries", "Country resource for location-based permissions.");
        public static readonly OptionDescriptor<string> States = OptionDescriptor<string>.Option(
            "states", "States", "State or province resource for location-based permissions.");
        public static readonly OptionDescriptor<string> Users = OptionDescriptor<string>.Option(
            "users", "Users", "User account resource.");
        public static readonly OptionDescriptor<string> UsersPermissions = OptionDescriptor<string>.Option(
            "users_permissions", "User Permissions", "User permission assignment resource.");
        public static readonly OptionDescriptor<string> UsersRoles = OptionDescriptor<string>.Option(
            "users_roles", "User Roles", "User role assignment resource.");
        public static readonly OptionDescriptor<string> Roles = OptionDescriptor<string>.Option(
            "roles", "Roles", "Role definition resource.");
        public static readonly OptionDescriptor<string> RolesPermissions = OptionDescriptor<string>.Option(
            "roles_permissions", "Role Permissions", "Role-to-permission mapping resource.");
        public static readonly OptionDescriptor<string> Permissions = OptionDescriptor<string>.Option(
            "permissions", "Permissions", "Permission definition resource.");
        public static readonly OptionDescriptor<string> UserProfile = OptionDescriptor<string>.Option(
            "userprofile", "User Profile", "User profile resource.");
        public static readonly OptionDescriptor<string> Products = OptionDescriptor<string>.Option(
            "products", "Products", "Product catalog resource.");
        public static readonly OptionDescriptor<string> ProductsOptionTypes = OptionDescriptor<string>.Option(
            "products_optiontypes", "Product Option Types", "Product-to-option-type assignment resource.");
        public static readonly OptionDescriptor<string> ProductsClassifications = OptionDescriptor<string>.Option(
            "products_classifications", "Product Classifications", "Product classification assignment resource.");
        public static readonly OptionDescriptor<string> ProductsVariants = OptionDescriptor<string>.Option(
            "products_variants", "Product Variants", "Product variant resource.");
        public static readonly OptionDescriptor<string> ProductsVariantsOptionValues = OptionDescriptor<string>.Option(
            "products_variants_optionvalues", "Product Variant Option Values", "Product variant option value resource.");
        public static readonly OptionDescriptor<string> ProductsVariantsImages = OptionDescriptor<string>.Option(
            "products_variants_images", "Product Variant Images", "Product variant image resource.");
        public static readonly OptionDescriptor<string> Taxonomies = OptionDescriptor<string>.Option(
            "taxonomies", "Taxonomies", "Taxonomy definition resource.");
        public static readonly OptionDescriptor<string> Taxons = OptionDescriptor<string>.Option(
            "taxons", "Taxons", "Taxon (category node) resource.");
        public static readonly OptionDescriptor<string> OptionTypes = OptionDescriptor<string>.Option(
            "optiontypes", "Option Types", "Option type definition resource.");
        public static readonly OptionDescriptor<string> OptionTypesOptionValues = OptionDescriptor<string>.Option(
            "optiontypes_optionvalues", "Option Type Values", "Option type value resource.");
        public static readonly OptionDescriptor<string> PropertyTypes = OptionDescriptor<string>.Option(
            "propertytypes", "Property Types", "Property type definition resource.");
        public static readonly OptionDescriptor<string> Orders = OptionDescriptor<string>.Option(
            "orders", "Orders", "Order resource.");
        public static readonly OptionDescriptor<string> Fulfillment = OptionDescriptor<string>.Option(
            "fulfillment", "Fulfillment", "Fulfillment resource for order shipping.");
        public static readonly OptionDescriptor<string> Payments = OptionDescriptor<string>.Option(
            "payments", "Payments", "Payment transaction resource.");
        public static readonly OptionDescriptor<string> PaymentMethods = OptionDescriptor<string>.Option(
            "payment_methods", "Payment Methods", "Payment method configuration resource.");
        public static readonly OptionDescriptor<string> StockItems = OptionDescriptor<string>.Option(
            "stockitems", "Stock Items", "Stock item (inventory unit) resource.");
        public static readonly OptionDescriptor<string> StockLocation = OptionDescriptor<string>.Option(
            "stocklocations", "Stock Location", "Stock location (warehouse) resource.");
        public static readonly OptionDescriptor<string> StockReservations = OptionDescriptor<string>.Option(
            "stock_reservations", "Stock Reservations", "Stock reservation resource.");
        public static readonly OptionDescriptor<string> Settings = OptionDescriptor<string>.Option(
            "settings", "Settings", "Application settings resource.");
        public static readonly OptionDescriptor<string> Promotions = OptionDescriptor<string>.Option(
            "promotions", "Promotions", "Promotion definition resource.");
        public static readonly OptionDescriptor<string> PromotionRules = OptionDescriptor<string>.Option(
            "promotions_rules", "Promotion Rules", "Promotion rule resource.");
        public static readonly OptionDescriptor<string> PromotionActions = OptionDescriptor<string>.Option(
            "promotions_actions", "Promotion Actions", "Promotion action resource.");
        public static readonly OptionDescriptor<string> Sales = OptionDescriptor<string>.Option(
            "sales", "Sales", "Sales data resource for reporting.");
        public static readonly OptionDescriptor<string> InventoryDb = OptionDescriptor<string>.Option(
            "inventory", "Inventory Database", "Inventory database resource.");
        public static readonly OptionDescriptor<string> CatalogDb = OptionDescriptor<string>.Option(
            "catalog", "Catalog Database", "Catalog database resource.");
        public static readonly OptionDescriptor<string> Activity = OptionDescriptor<string>.Option(
            "activity", "Activity", "Activity log resource.");
        public static readonly OptionDescriptor<string> Logs = OptionDescriptor<string>.Option(
            "logs", "Logs", "System log resource.");
        public static readonly OptionDescriptor<string> StoreBranding = OptionDescriptor<string>.Option(
            "store_branding", "Store Branding", "Store branding settings resource.");
        public static readonly OptionDescriptor<string> StoreContact = OptionDescriptor<string>.Option(
            "store_contact", "Store Contact", "Store contact information resource.");
        public static readonly OptionDescriptor<string> StoreSeo = OptionDescriptor<string>.Option(
            "store_seo", "Store SEO", "Store SEO settings resource.");
        public static readonly OptionDescriptor<string> StoreCurrency = OptionDescriptor<string>.Option(
            "store_currency", "Store Currency", "Store currency configuration resource.");
        public static readonly OptionDescriptor<string> StoreCheckout = OptionDescriptor<string>.Option(
            "store_checkout", "Store Checkout", "Store checkout settings resource.");
        public static readonly OptionDescriptor<string> TaxCategories = OptionDescriptor<string>.Option(
            "tax_categories", "Tax Categories", "Tax category definition resource.");
        public static readonly OptionDescriptor<string> TaxRates = OptionDescriptor<string>.Option(
            "tax_rates", "Tax Rates", "Tax rate definition resource.");
        public static readonly OptionDescriptor<string> CustomFieldDefinitions = OptionDescriptor<string>.Option(
            "custom_field_definitions", "Custom Field Definitions", "Custom field definition resource.");
        public static readonly OptionDescriptor<string> CustomFields = OptionDescriptor<string>.Option(
            "custom_fields", "Custom Fields", "Custom field value resource.");
        public static readonly OptionDescriptor<string> Stores = OptionDescriptor<string>.Option(
            "stores", "Stores", "Multi-store configuration resource.");
    }

    private static readonly Dictionary<string, PermissionMetadata> SByIdentifier =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, List<PermissionMetadata>> SByCategory =
        new(StringComparer.OrdinalIgnoreCase);

    public static readonly IReadOnlyList<PermissionMetadata> All;

    static PermissionContext()
    {
        List<PermissionMetadata> all = new();
        all.AddRange(CatalogFeatureMetadata.All);
        all.AddRange(IdentityFeatureMetadata.All);
        all.AddRange(LocationFeatureMetadata.All);
        all.AddRange(ProfileFeatureMetadata.All);
        all.AddRange(OrderingFeatureMetadata.All);
        all.AddRange(InventoryFeatureMetadata.All);
        all.AddRange(ConfigurationFeatureMetadata.All);
        all.AddRange(PromotionsFeatureMetadata.All);
        all.AddRange(DashboardFeatureMetadata.All);

        foreach (PermissionMetadata perm in all)
        {
            SByIdentifier[perm.Identifier] = perm;

            if (!SByCategory.TryGetValue(perm.Category, out List<PermissionMetadata>? list))
            {
                list = [];
                SByCategory[perm.Category] = list;
            }
            list.Add(perm);
        }

        All = all.AsReadOnly();
    }

    public static bool IsKnown(string identifier) =>
        SByIdentifier.ContainsKey(identifier);

    public static PermissionMetadata? ByIdentifier(string identifier) =>
        SByIdentifier.TryGetValue(identifier, out PermissionMetadata? perm) ? perm : null;

    public static IReadOnlyList<PermissionMetadata> ByCategory(string category) =>
        SByCategory.TryGetValue(category, out List<PermissionMetadata>? list) ? list : [];
}
