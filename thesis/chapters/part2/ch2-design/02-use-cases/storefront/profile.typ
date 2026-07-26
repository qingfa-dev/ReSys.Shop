==== Profile and Preferences

// Diagram placeholder: Profile and Preferences use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-STR-PRF-01], [Manage addresses], [Customer], [Create, update, or remove shipping and billing addresses; set a default address per type.], [Customer is authenticated.], [Addresses available for selection during checkout. Default address pre-selected.],
  [UC-STR-PRF-02], [Manage wishlists], [Customer], [Create named wishlists; add product variants with optional notes; remove items; rename or delete lists.], [Customer is authenticated.], [Wishlist updated. Items retained for future reference.],
  [UC-STR-PRF-03], [Manage notification preferences], [Customer], [Configure per-channel notification settings for each notification category; opt in or out per category.], [Customer is authenticated.], [Notification preferences saved. Future notifications respect the configured settings.],
)
