==== Profile and Preferences

// Diagram placeholder: Profile and Preferences use case diagram

==== UC-STR-PRF-01 — Manage Addresses

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-PRF-01],
    [*Use Case Name*], [Manage Addresses],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [Create, update, or remove shipping and billing addresses; set defaults per type.],
    [*Trigger*], [Customer navigates to the address book in account settings.],
    [*Preconditions*], [
      - Customer is authenticated.
    ],
    [*Postconditions*], [
      - Addresses available for checkout. Default pre-selected.
    ],
    [*Main Success Scenario*], [
      1. Navigates to address book page.
      2. System displays saved addresses with type labels and default indicators.
      3. Creates a new address: enters name, street, city, postal code, country, and state.
      4. Assigns address type (shipping, billing, or both) and optionally sets as default.
      5. Saves the address.
      6. System validates required fields and address format.
      7. System persists the new address.
      8. System confirms and refreshes the address list.
    ],
    [*Alternative Flows*], [
      A1. Edits existing address: system pre-fills form with current values.
      A2. Remove address used in active orders: system warns it is referenced by past orders (retained); allows soft-delete.
      A3. Set new default: system removes default flag from previous default of same type.
    ],
    [*Exception Flows*], [
      E1. Address validation fails for invalid country/state: system highlights mismatch and prevents save.
    ],
    [*Related Requirements*], [PRF-FR-01],
  ),
  caption: [UC-STR-PRF-01 -- Manage Addresses.],
)

==== UC-STR-PRF-02 — Manage Wishlists

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-PRF-02],
    [*Use Case Name*], [Manage Wishlists],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [Create named wishlists; add product variants with notes; remove items; rename or delete lists.],
    [*Trigger*], [Customer navigates to wishlist section or clicks Add to Wishlist on a product page.],
    [*Preconditions*], [
      - Customer is authenticated.
    ],
    [*Postconditions*], [
      - Wishlist updated. Items retained for future reference.
    ],
    [*Main Success Scenario*], [
      1. On a product detail page, clicks Add to Wishlist.
      2. System displays dialog to select wishlist or create new.
      3. Selects an existing wishlist or creates a new named wishlist.
      4. Optionally adds a note about the item.
      5. System adds the product variant to the selected wishlist.
      6. System confirms the addition.
      7. Navigates to wishlists section to view all.
      8. System displays all wishlists with item counts and preview thumbnails.
      9. Selects a wishlist to view items, remove items, rename, or delete the list.
    ],
    [*Alternative Flows*], [
      A1. Same variant already in wishlist: system notifies and suggests adding note to existing entry.
      A2. Removes item: system removes immediately with undo option.
      A3. Deletes wishlist: system asks for confirmation; list and items permanently removed.
      A4. Moves item between wishlists: system transfers item from source to destination.
    ],
    [*Exception Flows*], [
      E1. Product variant archived since added: system displays with Unavailable label and remove suggestion.
    ],
    [*Related Requirements*], [PRF-FR-02],
  ),
  caption: [UC-STR-PRF-02 -- Manage Wishlists.],
)

==== UC-STR-PRF-03 — Manage Notification Preferences

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-PRF-03],
    [*Use Case Name*], [Manage Notification Preferences],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [Configure per-channel notification settings for each notification category.],
    [*Trigger*], [Customer navigates to notification preferences in account settings.],
    [*Preconditions*], [
      - Customer is authenticated.
    ],
    [*Postconditions*], [
      - Notification preferences saved. Future notifications respect settings.
    ],
    [*Main Success Scenario*], [
      1. Navigates to notification preferences page.
      2. System displays all notification categories with current per-channel opt-in/out status.
      3. Toggles individual notification categories on or off per channel.
      4. Saves the preferences.
      5. System persists the updated preferences.
      6. System confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Opts out of all notifications: system warns important notifications also suppressed; asks confirmation.
      A2. Opts in to SMS without verified phone: system prompts to add and verify phone number.
      A3. Resets to defaults: system restores default settings.
    ],
    [*Exception Flows*], [
      E1. Persistence failure: system reports failure and retains unsaved changes for retry.
    ],
    [*Related Requirements*], [PRF-FR-03],
  ),
  caption: [UC-STR-PRF-03 -- Manage Notification Preferences.],
)
