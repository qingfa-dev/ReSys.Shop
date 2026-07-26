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
    [*Goal*], [Create, update, or remove shipping and billing addresses; set a default address per type.],
    [*Trigger*], [Customer navigates to the address book in their account settings.],
    [*Preconditions*], [
      - Customer is authenticated.
    ],
    [*Postconditions*], [
      - Addresses available for selection during checkout.
      - Default address pre-selected in checkout address step.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Navigates to the address book page in their account settings.
      2. System -- Displays the list of saved addresses with type labels (shipping, billing) and default indicators.
      3. Customer -- Creates a new address: enters name, street, city, postal code, country, and state.
      4. Customer -- Assigns an address type (shipping, billing, or both) and optionally sets it as default.
      5. Customer -- Saves the address.
      6. System -- Validates required fields and address format.
      7. System -- Persists the new address.
      8. System -- Confirms the addition and refreshes the address list.
    ],
    [*Alternative Flows*], [
      A1. Customer edits an existing address -- System pre-fills the form with current values; validation and save proceed as per the main flow.
      A2. Customer removes an address used in active orders -- System warns that the address is referenced by past orders (retained for history) and allows deletion; the address is soft-deleted.
      A3. Customer sets a new default address -- System removes the default flag from the previous default of the same type.
    ],
    [*Exception Flows*], [
      E1. Address validation fails for an invalid country/state combination -- System highlights the mismatch and prevents save.
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
    [*Goal*], [Create named wishlists; add product variants with optional notes; remove items; rename or delete lists.],
    [*Trigger*], [Customer navigates to the wishlist section or clicks Add to Wishlist on a product page.],
    [*Preconditions*], [
      - Customer is authenticated.
    ],
    [*Postconditions*], [
      - Wishlist updated.
      - Items retained for future reference.
    ],
    [*Main Success Scenario*], [
      1. Customer -- On a product detail page, clicks Add to Wishlist.
      2. System -- Displays a dialog to select which wishlist to add to, or create a new one.
      3. Customer -- Selects an existing wishlist or creates a new named wishlist.
      4. Customer -- Optionally adds a note about the item.
      5. System -- Adds the product variant to the selected wishlist.
      6. System -- Confirms the addition.
      7. Customer -- Navigates to the wishlists section to view all wishlists.
      8. System -- Displays all wishlists with item counts and preview thumbnails.
      9. Customer -- Selects a wishlist to view its items, remove items, rename the list, or delete the list.
    ],
    [*Alternative Flows*], [
      A1. Same variant already exists in the selected wishlist -- System notifies the customer and suggests adding a note to the existing entry instead.
      A2. Customer removes an item -- System removes it immediately with an option to undo.
      A3. Customer deletes a wishlist -- System asks for confirmation; once deleted, the list and its items are permanently removed.
      A4. Customer moves an item between wishlists -- System transfers the item from the source to the destination list.
    ],
    [*Exception Flows*], [
      E1. Product variant was archived since being added to the wishlist -- System displays the item with an Unavailable label and a remove suggestion.
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
    [*Goal*], [Configure per-channel notification settings for each notification category; opt in or out per category.],
    [*Trigger*], [Customer navigates to the notification preferences page in account settings.],
    [*Preconditions*], [
      - Customer is authenticated.
    ],
    [*Postconditions*], [
      - Notification preferences saved.
      - Future notifications respect the configured settings.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Navigates to the notification preferences page.
      2. System -- Displays all notification categories (e.g. order updates, promotions, product alerts) with current opt-in/out status for each channel (email, SMS).
      3. Customer -- Toggles individual notification categories on or off per channel.
      4. Customer -- Saves the preferences.
      5. System -- Persists the updated preferences.
      6. System -- Confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Customer opts out of all notifications -- System warns that important order and account notifications will also be suppressed; asks for confirmation.
      A2. Customer opts in to SMS notifications without a verified phone number -- System prompts the customer to add and verify a phone number first.
      A3. Customer resets preferences to defaults -- System restores the default settings (all enabled for transactional notifications, optional for marketing).
    ],
    [*Exception Flows*], [
      E1. System fails to persist preferences -- System reports the failure and retains the unsaved changes for retry.
    ],
    [*Related Requirements*], [PRF-FR-03],
  ),
  caption: [UC-STR-PRF-03 -- Manage Notification Preferences.],
)
