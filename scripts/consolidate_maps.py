"""Machine-readable migration maps for scripts/consolidate-shared.py.
Generated deterministically from plan/refactor-shared-consolidation-1.md Appendices A & B.
Excludes the Shipping module, which was pre-consolidated in-flight. Do not hand-edit.
"""

MODULES = ["Billing", "Catalog", "Customer", "Dashboard", "Identity", "Inventory", "Ordering"]

# List of dicts: {module, area ('admin'|'storefront'), target (rel to module), sources (rel to module Features/Area)}
TARGETS = [
 {
  "module": "Billing",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/Payment.Mapping.cs",
  "sources": [
   "Payments/Shared/Mappings/Payment.Mapping.Domain.cs",
   "Payments/Shared/Mappings/Payment.Mapping.Model.cs"
  ]
 },
 {
  "module": "Billing",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/Payment.Model.cs",
  "sources": [
   "Payments/Shared/Models/Payment.Model.Parameters.cs",
   "Payments/Shared/Models/Payment.Model.Request.cs",
   "Payments/Shared/Models/Payment.Model.Response.cs"
  ]
 },
 {
  "module": "Billing",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/Payment.Validator.cs",
  "sources": [
   "Payments/Shared/Validators/Payment.Validator.cs"
  ]
 },
 {
  "module": "Billing",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/PaymentMethod.Mapping.cs",
  "sources": [
   "PaymentMethods/Shared/Mappings/PaymentMethod.Mapping.Domain.cs",
   "PaymentMethods/Shared/Mappings/PaymentMethod.Mapping.Model.cs"
  ]
 },
 {
  "module": "Billing",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/PaymentMethod.Model.cs",
  "sources": [
   "PaymentMethods/Shared/Models/PaymentMethod.Model.Parameters.cs",
   "PaymentMethods/Shared/Models/PaymentMethod.Model.Request.cs",
   "PaymentMethods/Shared/Models/PaymentMethod.Model.Response.cs"
  ]
 },
 {
  "module": "Billing",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/PaymentMethod.Validator.cs",
  "sources": [
   "PaymentMethods/Shared/Validators/PaymentMethod.Validator.cs"
  ]
 },
 {
  "module": "Billing",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/PaymentMethodUpdate.Model.cs",
  "sources": [
   "PaymentMethods/Shared/Models/PaymentMethodUpdateParameters.cs",
   "PaymentMethods/Shared/Models/PaymentMethodUpdateRequest.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/CatalogDashboard.Model.cs",
  "sources": [
   "Dashboard/Get/Shared/Models/CatalogDashboard.Model.Parameters.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/ImageEmbedding.Model.cs",
  "sources": [
   "Variants/Images/Embeddings/Shared/Models/ImageEmbedding.Model.Parameters.cs",
   "Variants/Images/Embeddings/Shared/Models/ImageEmbedding.Model.Request.cs",
   "Variants/Images/Embeddings/Shared/Models/ImageEmbedding.Model.Response.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/OptionType.Mapping.cs",
  "sources": [
   "OptionTypes/Shared/Mappings/OptionType.Mapping.Domain.cs",
   "OptionTypes/Shared/Mappings/OptionType.Mapping.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/OptionType.Model.cs",
  "sources": [
   "OptionTypes/Shared/Models/OptionType.Model.Parameters.cs",
   "OptionTypes/Shared/Models/OptionType.Model.Request.cs",
   "OptionTypes/Shared/Models/OptionType.Model.Response.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/OptionType.Validator.cs",
  "sources": [
   "OptionTypes/Shared/Validators/OptionType.Validator.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/OptionValue.Mapping.cs",
  "sources": [
   "OptionTypes/Values/Shared/Mappings/OptionValue.Mapping.Domain.cs",
   "OptionTypes/Values/Shared/Mappings/OptionValue.Mapping.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/OptionValue.Model.cs",
  "sources": [
   "OptionTypes/Values/Shared/Models/OptionValue.Model.Parameters.cs",
   "OptionTypes/Values/Shared/Models/OptionValue.Model.Request.cs",
   "OptionTypes/Values/Shared/Models/OptionValue.Model.Response.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/OptionValue.Validator.cs",
  "sources": [
   "OptionTypes/Values/Shared/Validators/OptionValue.Validator.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/Price.Mapping.cs",
  "sources": [
   "Variants/Prices/Shared/Mappings/Price.Mapping.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/Price.Model.cs",
  "sources": [
   "Variants/Prices/Shared/Models/Price.Model.Parameters.cs",
   "Variants/Prices/Shared/Models/Price.Model.Request.cs",
   "Variants/Prices/Shared/Models/Price.Model.Response.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/Product.Mapping.cs",
  "sources": [
   "Products/Shared/Mappings/Product.Mapping.Domain.cs",
   "Products/Shared/Mappings/Product.Mapping.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/Product.Model.cs",
  "sources": [
   "Products/Shared/Models/Product.Model.Parameters.cs",
   "Products/Shared/Models/Product.Model.Request.cs",
   "Products/Shared/Models/Product.Model.Response.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/Product.Validator.cs",
  "sources": [
   "Products/Shared/Validation/Product.Validator.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/ProductClassification.Mapping.cs",
  "sources": [
   "Products/Classifications/Shared/Mappings/ProductClassification.Mapping.Domain.cs",
   "Products/Classifications/Shared/Mappings/ProductClassification.Mapping.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/ProductClassification.Model.cs",
  "sources": [
   "Products/Classifications/Shared/Models/ProductClassification.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/ProductClassification.Validator.cs",
  "sources": [
   "Products/Classifications/Shared/Validations/ProductClassification.Validator.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/ProductOptionType.Mapping.cs",
  "sources": [
   "Products/Options/Shared/Mappings/ProductOptionType.Mapping.Domain.cs",
   "Products/Options/Shared/Mappings/ProductOptionType.Mapping.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/ProductOptionType.Model.cs",
  "sources": [
   "Products/Options/Shared/Models/ProductOptionType.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/ProductOptionType.Validator.cs",
  "sources": [
   "Products/Options/Shared/Validations/ProductOptionType.Validator.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/Taxon.Mapping.cs",
  "sources": [
   "Taxonomies/Taxons/Shared/Mappings/Taxon.Mapping.Domain.cs",
   "Taxonomies/Taxons/Shared/Mappings/Taxon.Mapping.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/Taxon.Model.cs",
  "sources": [
   "Taxonomies/Taxons/Shared/Models/Taxon.Model.Parameters.cs",
   "Taxonomies/Taxons/Shared/Models/Taxon.Model.Request.cs",
   "Taxonomies/Taxons/Shared/Models/Taxon.Model.Response.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/Taxon.Validator.cs",
  "sources": [
   "Taxonomies/Taxons/Shared/Validators/Taxon.Validator.Request.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/TaxonRule.Mapping.cs",
  "sources": [
   "Taxonomies/Taxons/Rules/Shared/Mappings/TaxonRule.Mapping.Domain.cs",
   "Taxonomies/Taxons/Rules/Shared/Mappings/TaxonRule.Mapping.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/TaxonRule.Model.cs",
  "sources": [
   "Taxonomies/Taxons/Rules/Shared/Models/TaxonRule.Model.Action.cs",
   "Taxonomies/Taxons/Rules/Shared/Models/TaxonRule.Model.Collection.cs",
   "Taxonomies/Taxons/Rules/Shared/Models/TaxonRule.Model.Parameters.cs",
   "Taxonomies/Taxons/Rules/Shared/Models/TaxonRule.Model.Request.cs",
   "Taxonomies/Taxons/Rules/Shared/Models/TaxonRule.Model.Response.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/TaxonRule.Validator.cs",
  "sources": [
   "Taxonomies/Taxons/Rules/Shared/Validations/TaxonRuleValidationExtension.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/Taxonomy.Mapping.cs",
  "sources": [
   "Taxonomies/Shared/Mappings/Taxonomy.Mapping.Domain.cs",
   "Taxonomies/Shared/Mappings/Taxonomy.Mapping.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/Taxonomy.Model.cs",
  "sources": [
   "Taxonomies/Shared/Models/Taxonomy.Model.Parameters.cs",
   "Taxonomies/Shared/Models/Taxonomy.Model.Request.cs",
   "Taxonomies/Shared/Models/Taxonomy.Model.Response.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/Taxonomy.Validator.cs",
  "sources": [
   "Taxonomies/Shared/Validators/Taxonomy.Validator.Request.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/Variant.Mapping.cs",
  "sources": [
   "Variants/Shared/Mappings/Variant.Mapping.Domain.cs",
   "Variants/Shared/Mappings/Variant.Mapping.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/Variant.Model.cs",
  "sources": [
   "Variants/Shared/Models/Variant.Model.Parameters.cs",
   "Variants/Shared/Models/Variant.Model.Request.cs",
   "Variants/Shared/Models/Variant.Model.Response.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/Variant.Validator.cs",
  "sources": [
   "Variants/Shared/Validators/Variant.Validator.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/VariantImage.Mapping.cs",
  "sources": [
   "Variants/Images/Shared/Mappings/VariantImage.Mapping.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/VariantImage.Model.cs",
  "sources": [
   "Variants/Images/Shared/Models/VariantImage.Model.Parameters.cs",
   "Variants/Images/Shared/Models/VariantImage.Model.Request.cs",
   "Variants/Images/Shared/Models/VariantImage.Model.Response.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/VariantImage.Validator.cs",
  "sources": [
   "Variants/Images/Shared/Validators/VariantImage.Validator.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/VariantOptionValue.Model.cs",
  "sources": [
   "Variants/Values/Shared/Models/VariantOptionValue.Model.Parameters.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/VariantPrice.Model.cs",
  "sources": [
   "Variants/Prices/Shared/Models/VariantPrice.Model.Action.cs",
   "Variants/Prices/Shared/Models/VariantPrice.Model.Collection.cs"
  ]
 },
 {
  "module": "Dashboard",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/Dashboard.Model.cs",
  "sources": [
   "Get/Shared/Models/Dashboard.Model.Parameters.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/Permission.Mapping.cs",
  "sources": [
   "Permissions/Shared/Mappings/Permission.Mapping.Response.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/Permission.Model.cs",
  "sources": [
   "Permissions/Shared/Models/Permission.Model.Category.cs",
   "Permissions/Shared/Models/Permission.Model.Group.cs",
   "Permissions/Shared/Models/Permission.Model.Resouce.cs",
   "Permissions/Shared/Models/Permission.Model.Response.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/PermissionCollection.Model.cs",
  "sources": [
   "Permissions/Shared/Models/PermissionCollection.Model.Parameters.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/PermissionComposite.Mapping.cs",
  "sources": [
   "Permissions/Shared/Mappings/PermissionComposite.Mapping.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/Role.Mapping.cs",
  "sources": [
   "Roles/Shared/Mappings/Role.Mapping.Domain.cs",
   "Roles/Shared/Mappings/Role.Mapping.Model.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/Role.Model.cs",
  "sources": [
   "Roles/Shared/Models/Role.Model.Parameters.cs",
   "Roles/Shared/Models/Role.Model.Request.cs",
   "Roles/Shared/Models/Role.Model.Response.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/Role.Validator.cs",
  "sources": [
   "Roles/Shared/Validators/Role.Validator.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/User.Mapping.cs",
  "sources": [
   "Users/Shared/Mappings/User.Mapping.Domain.cs",
   "Users/Shared/Mappings/User.Mapping.Model.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/User.Model.cs",
  "sources": [
   "Users/Shared/Models/User.Model.Parameters.cs",
   "Users/Shared/Models/User.Model.Request.cs",
   "Users/Shared/Models/User.Model.Response.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/User.Validator.cs",
  "sources": [
   "Users/Shared/Validators/User.Validator.RoleName.cs",
   "Users/Shared/Validators/User.Validator.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/UserRoles.Model.cs",
  "sources": [
   "Users/Roles/Shared/Models/UserRoles.Model.Parameters.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/ImportStockItems.Mapping.cs",
  "sources": [
   "StockItems/Shared/Mappings/ImportStockItems.Mapping.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/InventoryDashboard.Model.cs",
  "sources": [
   "Dashboard/Shared/Models/InventoryDashboard.Model.Parameters.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/StockItem.Mapping.cs",
  "sources": [
   "StockItems/Shared/Mappings/StockItem.Mapping.Domain.cs",
   "StockItems/Shared/Mappings/StockItem.Mapping.Model.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/StockItem.Model.cs",
  "sources": [
   "StockItems/Shared/Models/StockItem.Model.Parameters.cs",
   "StockItems/Shared/Models/StockItem.Model.Request.cs",
   "StockItems/Shared/Models/StockItem.Model.Response.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/StockItem.Validator.cs",
  "sources": [
   "StockItems/Shared/Validators/StockItem.Validator.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/StockLocation.Mapping.cs",
  "sources": [
   "StockLocations/Shared/Mappings/StockLocation.Mapping.Domain.cs",
   "StockLocations/Shared/Mappings/StockLocation.Mapping.Model.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/StockLocation.Model.cs",
  "sources": [
   "StockLocations/Shared/Models/StockLocation.Model.Parameters.cs",
   "StockLocations/Shared/Models/StockLocation.Model.Request.cs",
   "StockLocations/Shared/Models/StockLocation.Model.Response.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/StockLocation.Validator.cs",
  "sources": [
   "StockLocations/Shared/Validators/StockLocation.Validator.Address.cs",
   "StockLocations/Shared/Validators/StockLocation.Validator.City.cs",
   "StockLocations/Shared/Validators/StockLocation.Validator.Code.cs",
   "StockLocations/Shared/Validators/StockLocation.Validator.Name.cs",
   "StockLocations/Shared/Validators/StockLocation.Validator.Parameters.cs",
   "StockLocations/Shared/Validators/StockLocation.Validator.Phone.cs",
   "StockLocations/Shared/Validators/StockLocation.Validator.PostalCode.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/StockMovement.Mapping.cs",
  "sources": [
   "StockMovements/Shared/Mappings/StockMovement.Mapping.Domain.cs",
   "StockMovements/Shared/Mappings/StockMovement.Mapping.Model.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/StockMovement.Model.cs",
  "sources": [
   "StockMovements/Shared/Models/StockMovement.Model.Parameters.cs",
   "StockMovements/Shared/Models/StockMovement.Model.Response.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/StockMovement.Validator.cs",
  "sources": [
   "StockMovements/Shared/Validators/StockMovement.Validator.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/StockReservation.Mapping.cs",
  "sources": [
   "StockReservations/Shared/Mappings/StockReservation.Mapping.Domain.cs",
   "StockReservations/Shared/Mappings/StockReservation.Mapping.Model.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/StockReservation.Model.cs",
  "sources": [
   "StockReservations/Shared/Models/StockReservation.Model.Parameters.cs",
   "StockReservations/Shared/Models/StockReservation.Model.Request.cs",
   "StockReservations/Shared/Models/StockReservation.Model.Response.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/StockReservation.Validator.cs",
  "sources": [
   "StockReservations/Shared/Validators/StockReservation.Validator.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/StockTransfer.Mapping.cs",
  "sources": [
   "StockTransfers/Shared/Mappings/StockTransfer.Mapping.Domain.cs",
   "StockTransfers/Shared/Mappings/StockTransfer.Mapping.Model.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/StockTransfer.Model.cs",
  "sources": [
   "StockTransfers/Shared/Models/StockTransfer.Model.Parameters.cs",
   "StockTransfers/Shared/Models/StockTransfer.Model.ReceiveRequest.cs",
   "StockTransfers/Shared/Models/StockTransfer.Model.Request.cs",
   "StockTransfers/Shared/Models/StockTransfer.Model.Response.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/StockTransfer.Validator.cs",
  "sources": [
   "StockTransfers/Shared/Validators/StockTransfer.Validator.cs"
  ]
 },
 {
  "module": "Ordering",
  "area": "admin",
  "target": "Features/Admin/Shared/Mappings/Order.Mapping.cs",
  "sources": [
   "Orders/Shared/Mappings/Order.Mapping.Domain.cs",
   "Orders/Shared/Mappings/Order.Mapping.Model.cs"
  ]
 },
 {
  "module": "Ordering",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/Order.Model.cs",
  "sources": [
   "Orders/Shared/Models/Order.Model.Action.cs",
   "Orders/Shared/Models/Order.Model.AddressAction.cs",
   "Orders/Shared/Models/Order.Model.Parameters.cs",
   "Orders/Shared/Models/Order.Model.QuantityAction.cs",
   "Orders/Shared/Models/Order.Model.Request.cs",
   "Orders/Shared/Models/Order.Model.Response.cs",
   "Orders/Shared/Models/Order.Model.ShippingMethodAction.cs"
  ]
 },
 {
  "module": "Ordering",
  "area": "admin",
  "target": "Features/Admin/Shared/Validators/Order.Validator.cs",
  "sources": [
   "Orders/Shared/Validators/Order.Validator.cs"
  ]
 },
 {
  "module": "Ordering",
  "area": "admin",
  "target": "Features/Admin/Shared/Models/OrderingDashboard.Model.cs",
  "sources": [
   "Dashboard/Get/Shared/Models/OrderingDashboard.Model.Parameters.cs"
  ]
 },
 {
  "module": "Billing",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Mappings/Storefront.Payment.Mapping.cs",
  "sources": [
   "Payment/Shared/Mappings/PaymentStore.Mapping.cs"
  ]
 },
 {
  "module": "Billing",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.Payment.Model.cs",
  "sources": [
   "Payment/Shared/Models/PaymentStore.Model.Request.cs",
   "Payment/Shared/Models/PaymentStore.Model.Response.cs"
  ]
 },
 {
  "module": "Billing",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Validators/Storefront.Payment.Validator.cs",
  "sources": [
   "Payment/Shared/Validators/PaymentStore.Validator.cs"
  ]
 },
 {
  "module": "Billing",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Mappings/Storefront.PaymentMethod.Mapping.cs",
  "sources": [
   "PaymentMethods/Shared/Mappings/PaymentMethodStore.Mapping.cs"
  ]
 },
 {
  "module": "Billing",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.PaymentMethod.Model.cs",
  "sources": [
   "PaymentMethods/Shared/Models/PaymentMethodStore.Model.Response.cs"
  ]
 },
 {
  "module": "Billing",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Validators/Storefront.PaymentMethod.Validator.cs",
  "sources": [
   "PaymentMethods/Shared/Validators/PaymentMethodStore.Validator.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.ImageSearch.Model.cs",
  "sources": [
   "Products/Images/Search/Shared/Models/ImageSearch.Model.Parameters.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Mappings/Storefront.OptionType.Mapping.cs",
  "sources": [
   "OptionTypes/Shared/Mappings/Store.OptionType.Mapping.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.OptionType.Model.cs",
  "sources": [
   "OptionTypes/Shared/Models/Store.OptionType.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Mappings/Storefront.OptionValue.Mapping.cs",
  "sources": [
   "OptionTypes/Shared/Mappings/Store.OptionValue.Mapping.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.OptionValue.Model.cs",
  "sources": [
   "OptionTypes/Shared/Models/Store.OptionValue.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Mappings/Storefront.Product.Mapping.cs",
  "sources": [
   "Products/Shared/Mappings/Store.Product.Mapping.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.Product.Model.cs",
  "sources": [
   "Products/Shared/Models/Store.Product.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Mappings/Storefront.Taxon.Mapping.cs",
  "sources": [
   "Taxonomies/Shared/Mappings/Store.Taxon.Mapping.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.Taxon.Model.cs",
  "sources": [
   "Taxonomies/Shared/Models/Store.Taxon.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Mappings/Storefront.Taxonomy.Mapping.cs",
  "sources": [
   "Taxonomies/Shared/Mappings/Store.Taxonomy.Mapping.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.Taxonomy.Model.cs",
  "sources": [
   "Taxonomies/Shared/Models/Store.Taxonomy.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Mappings/Storefront.Variant.Mapping.cs",
  "sources": [
   "Products/Shared/Mappings/Store.Variant.Mapping.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.Variant.Model.cs",
  "sources": [
   "Products/Shared/Models/Store.Variant.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Mappings/Storefront.VariantImage.Mapping.cs",
  "sources": [
   "Products/Shared/Mappings/Store.VariantImage.Mapping.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.VariantImage.Model.cs",
  "sources": [
   "Products/Shared/Models/Store.VariantImage.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Mappings/Storefront.VariantPrice.Mapping.cs",
  "sources": [
   "Products/Shared/Mappings/Store.VariantPrice.Mapping.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.VariantPrice.Model.cs",
  "sources": [
   "Products/Shared/Models/Store.VariantPrice.Model.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Mappings/Storefront.VisualSearchModel.Mapping.cs",
  "sources": [
   "Products/Images/Inferences/Shared/Mappings/VisualSearchModel.Mapping.cs"
  ]
 },
 {
  "module": "Catalog",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.VisualSearchModel.Model.cs",
  "sources": [
   "Products/Images/Inferences/Shared/Models/VisualSearchModel.Response.cs"
  ]
 },
 {
  "module": "Customer",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.WishedItem.Model.cs",
  "sources": [
   "Wishlists/Shared/Models/WishedItem.Model.Response.cs"
  ]
 },
 {
  "module": "Customer",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Mappings/Storefront.Wishlist.Mapping.cs",
  "sources": [
   "Wishlists/Shared/Mappings/Wishlist.Mapping.Domain.cs",
   "Wishlists/Shared/Mappings/Wishlist.Mapping.Model.cs"
  ]
 },
 {
  "module": "Customer",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.Wishlist.Model.cs",
  "sources": [
   "Wishlists/Shared/Models/Wishlist.Model.Parameters.cs",
   "Wishlists/Shared/Models/Wishlist.Model.Request.cs",
   "Wishlists/Shared/Models/Wishlist.Model.Response.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.Auth.Model.cs",
  "sources": [
   "Shared/Models/Auth.Request.Model.cs",
   "Shared/Models/Auth.Response.Model.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Mappings/Storefront.AuthToken.Mapping.cs",
  "sources": [
   "Shared/Mappings/AuthToken.Mapping.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.Email.Model.cs",
  "sources": [
   "Emails/Shared/Models/Email.Model.Parameters.cs",
   "Emails/Shared/Models/Email.Model.Request.cs",
   "Emails/Shared/Models/Email.Model.Response.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.External.Model.cs",
  "sources": [
   "Auth/Login/External/Shared/Models/External.Model.Request.cs",
   "Auth/Login/External/Shared/Models/External.Model.Response.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.Password.Model.cs",
  "sources": [
   "Passwords/Shared/Models/Password.Model.Parameters.cs",
   "Passwords/Shared/Models/Password.Model.Request.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.Register.Model.cs",
  "sources": [
   "Auth/Shared/Models/Register.Model.Request.cs"
  ]
 },
 {
  "module": "Identity",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Mappings/Storefront.Session.Mapping.cs",
  "sources": [
   "Shared/Mappings/Session.Mapping.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.Inventory.Model.cs",
  "sources": [
   "Shared/Models/Inventory.Storefront.Model.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Validators/Storefront.Inventory.Validator.cs",
  "sources": [
   "Shared/Validators/Inventory.Storefront.Validator.cs"
  ]
 },
 {
  "module": "Inventory",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.StockReservationReserve.Model.cs",
  "sources": [
   "StockReservations/Shared/Models/StockReservationReserve.Model.Parameters.cs"
  ]
 },
 {
  "module": "Ordering",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Mappings/Storefront.Cart.Mapping.cs",
  "sources": [
   "Cart/Shared/Mappings/Cart.Mapping.Domain.cs",
   "Cart/Shared/Mappings/Cart.Mapping.Model.cs"
  ]
 },
 {
  "module": "Ordering",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.Cart.Model.cs",
  "sources": [
   "Cart/Shared/Models/Cart.Model.Parameters.cs",
   "Cart/Shared/Models/Cart.Model.Request.cs",
   "Cart/Shared/Models/Cart.Model.Response.Base.cs",
   "Cart/Shared/Models/Cart.Model.Response.cs"
  ]
 },
 {
  "module": "Ordering",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Validators/Storefront.Cart.Validator.cs",
  "sources": [
   "Cart/Shared/Validators/Cart.Validator.cs"
  ]
 },
 {
  "module": "Ordering",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Mappings/Storefront.Order.Mapping.cs",
  "sources": [
   "Orders/Shared/Mappings/OrderStore.Mapping.cs"
  ]
 },
 {
  "module": "Ordering",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.Order.Model.cs",
  "sources": [
   "Orders/Shared/Models/Order.Model.Response.cs"
  ]
 },
 {
  "module": "Ordering",
  "area": "storefront",
  "target": "Features/Storefront/Shared/Models/Storefront.OrderTracking.Model.cs",
  "sources": [
   "Orders/GetTracking/Shared/Models/OrderTracking.Model.Parameters.cs"
  ]
 }
]

# dict: module -> {old_namespace: new_namespace}
NAMESPACES = {
 "Billing": {
  "Module.Billing.Features.Admin.Payments.Shared.Mappings": "Module.Billing.Features.Admin.Shared.Mappings",
  "Module.Billing.Features.Admin.Payments.Shared.Models": "Module.Billing.Features.Admin.Shared.Models",
  "Module.Billing.Features.Admin.Payments.Shared.Validators": "Module.Billing.Features.Admin.Shared.Validators",
  "Module.Billing.Features.Admin.PaymentMethods.Shared.Mappings": "Module.Billing.Features.Admin.Shared.Mappings",
  "Module.Billing.Features.Admin.PaymentMethods.Shared.Models": "Module.Billing.Features.Admin.Shared.Models",
  "Module.Billing.Features.Admin.PaymentMethods.Shared.Validators": "Module.Billing.Features.Admin.Shared.Validators",
  "Module.Billing.Features.Storefront.Payment.Shared.Mappings": "Module.Billing.Features.Storefront.Shared.Mappings",
  "Module.Billing.Features.Storefront.Payment.Shared.Models": "Module.Billing.Features.Storefront.Shared.Models",
  "Module.Billing.Features.Storefront.Payment.Shared.Validators": "Module.Billing.Features.Storefront.Shared.Validators",
  "Module.Billing.Features.Storefront.PaymentMethods.Shared.Mappings": "Module.Billing.Features.Storefront.Shared.Mappings",
  "Module.Billing.Features.Storefront.PaymentMethods.Shared.Models": "Module.Billing.Features.Storefront.Shared.Models",
  "Module.Billing.Features.Storefront.PaymentMethods.Shared.Validators": "Module.Billing.Features.Storefront.Shared.Validators"
 },
 "Catalog": {
  "Module.Catalog.Features.Admin.Dashboard.Get.Shared.Models": "Module.Catalog.Features.Admin.Shared.Models",
  "Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Models": "Module.Catalog.Features.Admin.Shared.Models",
  "Module.Catalog.Features.Admin.OptionTypes.Shared.Mappings": "Module.Catalog.Features.Admin.Shared.Mappings",
  "Module.Catalog.Features.Admin.OptionTypes.Shared.Models": "Module.Catalog.Features.Admin.Shared.Models",
  "Module.Catalog.Features.Admin.OptionTypes.Shared.Validators": "Module.Catalog.Features.Admin.Shared.Validators",
  "Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Mappings": "Module.Catalog.Features.Admin.Shared.Mappings",
  "Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Models": "Module.Catalog.Features.Admin.Shared.Models",
  "Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Validators": "Module.Catalog.Features.Admin.Shared.Validators",
  "Module.Catalog.Features.Admin.Variants.Prices.Shared.Mappings": "Module.Catalog.Features.Admin.Shared.Mappings",
  "Module.Catalog.Features.Admin.Variants.Prices.Shared.Models": "Module.Catalog.Features.Admin.Shared.Models",
  "Module.Catalog.Features.Admin.Products.Shared.Mappings": "Module.Catalog.Features.Admin.Shared.Mappings",
  "Module.Catalog.Features.Admin.Products.Shared.Models": "Module.Catalog.Features.Admin.Shared.Models",
  "Module.Catalog.Features.Admin.Products.Shared.Validation": "Module.Catalog.Features.Admin.Shared.Validators",
  "Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Mappings": "Module.Catalog.Features.Admin.Shared.Mappings",
  "Module.Catalog.Features.Admin.Products.Classifications.Shared.Models": "Module.Catalog.Features.Admin.Shared.Models",
  "Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Validations": "Module.Catalog.Features.Admin.Shared.Validators",
  "Module.Catalog.Features.Admin.Products.Options.Shared.Mappings": "Module.Catalog.Features.Admin.Shared.Mappings",
  "Module.Catalog.Features.Admin.Products.Options.Shared.Models": "Module.Catalog.Features.Admin.Shared.Models",
  "Module.Catalog.Features.Admin.Products.Options.Shared.Validations": "Module.Catalog.Features.Admin.Shared.Validators",
  "Module.Catalog.Features.Admin.Taxons.Shared.Mappings": "Module.Catalog.Features.Admin.Shared.Mappings",
  "Module.Catalog.Features.Admin.Taxons.Shared.Models": "Module.Catalog.Features.Admin.Shared.Models",
  "Module.Catalog.Features.Admin.Taxons.Shared.Validators": "Module.Catalog.Features.Admin.Shared.Validators",
  "Module.Catalog.Features.Admin.Taxons.Rules.Shared.Mappings": "Module.Catalog.Features.Admin.Shared.Mappings",
  "Module.Catalog.Features.Admin.Taxons.Rules.Shared.Models": "Module.Catalog.Features.Admin.Shared.Models",
  "Module.Catalog.Features.Admin.Taxons.Rules.Shared.Validations": "Module.Catalog.Features.Admin.Shared.Validators",
  "Module.Catalog.Features.Admin.Taxonomies.Shared.Mappings": "Module.Catalog.Features.Admin.Shared.Mappings",
  "Module.Catalog.Features.Admin.Taxonomies.Shared.Models": "Module.Catalog.Features.Admin.Shared.Models",
  "Module.Catalog.Features.Admin.Taxonomies.Shared.Validators": "Module.Catalog.Features.Admin.Shared.Validators",
  "Module.Catalog.Features.Admin.Variants.Shared.Mappings": "Module.Catalog.Features.Admin.Shared.Mappings",
  "Module.Catalog.Features.Admin.Variants.Shared.Models": "Module.Catalog.Features.Admin.Shared.Models",
  "Module.Catalog.Features.Admin.Variants.Shared.Validators": "Module.Catalog.Features.Admin.Shared.Validators",
  "Module.Catalog.Features.Admin.Variants.Images.Shared.Mappings": "Module.Catalog.Features.Admin.Shared.Mappings",
  "Module.Catalog.Features.Admin.Variants.Images.Shared.Models": "Module.Catalog.Features.Admin.Shared.Models",
  "Module.Catalog.Features.Admin.Variants.Images.Shared.Validators": "Module.Catalog.Features.Admin.Shared.Validators",
  "Module.Catalog.Features.Admin.Variants.Values.Shared.Models": "Module.Catalog.Features.Admin.Shared.Models",
  "Module.Catalog.Features.Storefront.Products.Images.Search.Shared.Models": "Module.Catalog.Features.Storefront.Shared.Models",
  "Module.Catalog.Features.Storefront.OptionTypes.Shared.Mappings": "Module.Catalog.Features.Storefront.Shared.Mappings",
  "Module.Catalog.Features.Storefront.OptionTypes.Shared.Models": "Module.Catalog.Features.Storefront.Shared.Models",
  "Module.Catalog.Features.Storefront.Products.Shared.Mappings": "Module.Catalog.Features.Storefront.Shared.Mappings",
  "Module.Catalog.Features.Storefront.Products.Shared.Models": "Module.Catalog.Features.Storefront.Shared.Models",
  "Module.Catalog.Features.Storefront.Classifications.Shared.Mappings": "Module.Catalog.Features.Storefront.Shared.Mappings",
  "Module.Catalog.Features.Storefront.Classifications.Shared.Models": "Module.Catalog.Features.Storefront.Shared.Models",
  "Module.Catalog.Features.Storefront.Products.Images.Inferences.Shared.Mappings": "Module.Catalog.Features.Storefront.Shared.Mappings",
  "Module.Catalog.Features.Storefront.Products.Images.Inferences.Shared.Models": "Module.Catalog.Features.Storefront.Shared.Models"
 },
 "Dashboard": {
  "Module.Dashboard.Features.Admin.Get.Shared.Models": "Module.Dashboard.Features.Admin.Shared.Models"
 },
 "Identity": {
  "Module.Identity.Features.Shared.Admin.Permissions.Shared.Mappings": "Module.Identity.Features.Admin.Shared.Mappings",
  "Module.Identity.Features.Shared.Admin.Permissions.Shared.Models": "Module.Identity.Features.Admin.Shared.Models",
  "Module.Identity.Features.Shared.Admin.Roles.Shared.Mappings": "Module.Identity.Features.Admin.Shared.Mappings",
  "Module.Identity.Features.Shared.Admin.Roles.Shared.Models": "Module.Identity.Features.Admin.Shared.Models",
  "Module.Identity.Features.Shared.Admin.Roles.Shared.Validators": "Module.Identity.Features.Admin.Shared.Validators",
  "Module.Identity.Features.Shared.Admin.Users.Shared.Mappings": "Module.Identity.Features.Admin.Shared.Mappings",
  "Module.Identity.Features.Shared.Admin.Users.Shared.Models": "Module.Identity.Features.Admin.Shared.Models",
  "Module.Identity.Features.Shared.Admin.Users.Shared.Validators": "Module.Identity.Features.Admin.Shared.Validators",
  "Module.Identity.Features.Shared.Admin.Users.Roles.Shared.Models": "Module.Identity.Features.Admin.Shared.Models",
  "Module.Identity.Features.Shared.Storefront.Shared.Models": "Module.Identity.Features.Storefront.Shared.Models",
  "Module.Identity.Features.Shared.Storefront.Shared.Mappings": "Module.Identity.Features.Storefront.Shared.Mappings",
  "Module.Identity.Features.Shared.Storefront.Emails.Shared.Models": "Module.Identity.Features.Storefront.Shared.Models",
  "Module.Identity.Features.Shared.Storefront.Auth.Login.External.Shared.Models": "Module.Identity.Features.Storefront.Shared.Models",
  "Module.Identity.Features.Shared.Storefront.Passwords.Shared.Models": "Module.Identity.Features.Storefront.Shared.Models",
  "Module.Identity.Features.Shared.Storefront.Auth.Shared.Models": "Module.Identity.Features.Storefront.Shared.Models"
 },
 "Inventory": {
  "Module.Inventory.Features.Admin.StockItems.Shared.Mappings": "Module.Inventory.Features.Admin.Shared.Mappings",
  "Module.Inventory.Features.Admin.Dashboard.Shared.Models": "Module.Inventory.Features.Admin.Shared.Models",
  "Module.Inventory.Features.Admin.StockItems.Shared.Models": "Module.Inventory.Features.Admin.Shared.Models",
  "Module.Inventory.Features.Admin.StockItems.Shared.Validators": "Module.Inventory.Features.Admin.Shared.Validators",
  "Module.Inventory.Features.Admin.StockLocations.Shared.Mappings": "Module.Inventory.Features.Admin.Shared.Mappings",
  "Module.Inventory.Features.Admin.StockLocations.Shared.Models": "Module.Inventory.Features.Admin.Shared.Models",
  "Module.Inventory.Features.Admin.StockLocations.Shared.Validators": "Module.Inventory.Features.Admin.Shared.Validators",
  "Module.Inventory.Features.Admin.StockMovements.Shared.Mappings": "Module.Inventory.Features.Admin.Shared.Mappings",
  "Module.Inventory.Features.Admin.StockMovements.Shared.Models": "Module.Inventory.Features.Admin.Shared.Models",
  "Module.Inventory.Features.Admin.StockMovements.Shared.Validators": "Module.Inventory.Features.Admin.Shared.Validators",
  "Module.Inventory.Features.Admin.StockReservations.Shared.Mappings": "Module.Inventory.Features.Admin.Shared.Mappings",
  "Module.Inventory.Features.Admin.StockReservations.Shared.Models": "Module.Inventory.Features.Admin.Shared.Models",
  "Module.Inventory.Features.Admin.StockReservations.Shared.Validators": "Module.Inventory.Features.Admin.Shared.Validators",
  "Module.Inventory.Features.Admin.StockTransfers.Shared.Mappings": "Module.Inventory.Features.Admin.Shared.Mappings",
  "Module.Inventory.Features.Admin.StockTransfers.Shared.Models": "Module.Inventory.Features.Admin.Shared.Models",
  "Module.Inventory.Features.Admin.StockTransfers.Shared.Validators": "Module.Inventory.Features.Admin.Shared.Validators",
  "Module.Inventory.Features.Storefront.Shared.Models": "Module.Inventory.Features.Storefront.Shared.Models",
  "Module.Inventory.Features.Storefront.Shared.Validators": "Module.Inventory.Features.Storefront.Shared.Validators",
  "Module.Inventory.Features.Storefront.StockReservations.Shared.Models": "Module.Inventory.Features.Storefront.Shared.Models"
 },
 "Ordering": {
  "Module.Ordering.Features.Admin.Orders.Shared.Mappings": "Module.Ordering.Features.Admin.Shared.Mappings",
  "Module.Ordering.Features.Admin.Orders.Shared.Models": "Module.Ordering.Features.Admin.Shared.Models",
  "Module.Ordering.Features.Admin.Orders.Shared.Validators": "Module.Ordering.Features.Admin.Shared.Validators",
  "Module.Ordering.Features.Admin.Dashboard.Get.Shared.Models": "Module.Ordering.Features.Admin.Shared.Models",
  "Module.Ordering.Features.Storefront.Cart.Shared.Mappings": "Module.Ordering.Features.Storefront.Shared.Mappings",
  "Module.Ordering.Features.Storefront.Cart.Shared.Models": "Module.Ordering.Features.Storefront.Shared.Models",
  "Module.Ordering.Features.Storefront.Cart.Shared.Validators": "Module.Ordering.Features.Storefront.Shared.Validators",
  "Module.Ordering.Features.Storefront.Orders.Shared.Mappings": "Module.Ordering.Features.Storefront.Shared.Mappings",
  "Module.Ordering.Features.Storefront.Orders.Shared.Models": "Module.Ordering.Features.Storefront.Shared.Models",
  "Module.Ordering.Features.Storefront.Orders.GetTracking.Shared.Models": "Module.Ordering.Features.Storefront.Shared.Models"
 },
 "Customer": {
  "Module.Customer.Features.Storefront.Wishlists.Shared.Models": "Module.Customer.Features.Storefront.Shared.Models",
  "Module.Customer.Features.Storefront.Wishlists.Shared.Mappings": "Module.Customer.Features.Storefront.Shared.Mappings"
 }
}