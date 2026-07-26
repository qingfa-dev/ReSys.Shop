==== Profile and Preferences

// Diagram placeholder: Profile and Preferences use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-STR-PRF-01], [Manage addresses], [Customer],
    [Create, update, or remove shipping and billing addresses. Set a default address per type.],
    [Addresses available for selection during checkout. Default address pre-selected.],
    [PRF-FR-01],
    [UC-STR-PRF-02], [Manage wishlists], [Customer],
    [Create named wishlists. Add product variants with optional notes. Remove items. Rename or delete lists.],
    [Wishlist updated. Items retained for future reference.],
    [PRF-FR-02],
    [UC-STR-PRF-03], [Manage notification preferences], [Customer],
    [Configure per-channel notification settings for each notification category. Opt in or out per category.],
    [Notification preferences saved. Future notifications respect the configured settings.],
    [PRF-FR-03],
  ),
  caption: [Customer use cases — Profile and Preferences.],
)
