==== Profile and Preferences

#figure(
  table(
    columns: (auto, auto, auto, 1fr, auto, 1fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Flow*], [*Postcondition*], [*Related FR*]),
    [UC-STR-PRF-01], [Manage addresses], [Customer],
    [Create, update, or delete shipping and billing addresses. Set a default address per type. View all saved addresses.],
    [Addresses available for selection during checkout. Default address pre-selected.],
    [PRF-FR-01],
    [UC-STR-PRF-02], [Manage wishlists], [Customer],
    [Create named wishlists. Add product variants to a wishlist with optional notes. Remove items. View wishlist contents. Rename or delete lists.],
    [Wishlist updated. Items retained for future reference and potential cart addition.],
    [PRF-FR-02],
    [UC-STR-PRF-03], [Manage notification preferences], [Customer],
    [Configure per-channel notification settings (email, SMS) for each notification category (order updates, promotions, stock alerts). Opt in or out per category.],
    [Notification preferences saved. Future notifications respect the configured settings.],
    [PRF-FR-03],
  ),
  caption: [Customer use cases for the Profile module.],
)
