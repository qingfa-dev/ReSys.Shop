==== Cross-Context Patterns

The domain model implements several advanced patterns to ensure consistency and extensibility:

*Shared Entity Pattern:* `InventoryUnit` is a child entity owned by both `Order` (for tracking customer promises) and `StockItem` (for physical inventory). This dual ownership ensures that reservation logic remains synchronized across the Ordering and Inventories contexts without introducing circular dependencies.

*Profile Segregation:* The `User` aggregate supports two distinct profile types (`CustomerProfile` for e-commerce features, `StaffProfile` for administrative capabilities), allowing role-specific data to be isolated while maintaining a unified identity model.

*Soft Delete (ISoftDeletable):* Critical aggregates like `Product`, `StockItem`, and `User` implement soft deletion to preserve referential integrity and audit trails. Deleted entities remain in the database with `IsDeleted = true` and can be restored if needed.

*Metadata Extensibility (IHasMetadata):* All major aggregates expose `PublicMetadata` and `PrivateMetadata` dictionaries, enabling runtime schema evolution without database migrations. This is particularly useful for A/B testing and feature flags.
