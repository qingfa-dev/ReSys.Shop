==== Profile and Preferences

==== UC-STR-PRF — Profile Management

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-PRF],
    [*Use Case Name*], [Profile Management],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [Manage shipping addresses, wishlists, and notification preferences.],
    [*Trigger*], [Customer navigates to account settings.],
    [*Preconditions*], [
      - Customer is authenticated.
    ],
    [*Postconditions*], [
      - Profile data and preferences updated.
    ],
    [*Main Success Scenario*], [
      *Manage Addresses*
      1. Navigates to address book page.
      2. System displays saved addresses with type labels and default indicators.
      3. Creates a new address: enters name, street, city, postal code, country, and state.
      4. Assigns address type (shipping, billing, or both) and optionally sets as default.
      5. Optionally edits or removes existing addresses.
      6. Saves. System validates required fields and address format, persists, and confirms.
      ,
      *Manage Wishlists*
      1. On a product detail page, clicks Add to Wishlist.
      2. System displays dialog to select wishlist or create new.
      3. Selects an existing wishlist or creates a new named wishlist; optionally adds a note.
      4. System adds the product variant to the selected wishlist and confirms.
      5. Navigates to wishlists section to view all with item counts and preview thumbnails.
      6. Selects a wishlist to view items, remove items, rename, or delete the list.
      ,
      *Manage Notification Preferences*
      1. Navigates to notification preferences page.
      2. System displays all notification categories with current per-channel opt-in/out status.
      3. Toggles individual notification categories on or off per channel.
      4. Saves. System persists and confirms.
    ],
    [*Alternative Flows*], [
      A1. Same variant already in wishlist: system notifies and suggests adding note to existing entry.
      A2. Remove address used in active orders: system warns it is referenced by past orders (retained); allows soft-delete.
      A3. Opts out of all notifications: system warns important notifications also suppressed; asks confirmation.
      A4. Deletes wishlist: system asks for confirmation; list and items permanently removed.
    ],
    [*Exception Flows*], [
      E1. Address validation fails for invalid country/state: system highlights mismatch and prevents save.
      E2. Product variant archived since added to wishlist: system displays with Unavailable label and remove suggestion.
      E3. Persistence failure: system reports failure and retains unsaved changes for retry.
    ],
    [*Related Requirements*], [PRF-FR-01, PRF-FR-02, PRF-FR-03],
  ),
    kind: table,
  caption: [Profile Management.],
)

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-profile-preferences.png",
    width: 100%
  ),
  caption: [Use case diagram for Profile and Preferences (UC-STR-PRF).],
) <fig-uc-str-prf-d>
