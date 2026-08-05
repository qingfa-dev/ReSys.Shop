==== Profile and Preferences
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-profile-preferences.png",
    width: 70%
  ),
  caption: [Use case diagram for Profile and Preferences (UC-STR-PRF).],
) <fig-uc-str-prf-d>

==== UC-STR-PRF: Profile Management

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-STR-PRF — Profile Management],
    [*Actor*], [Customer],
    [*Goal*], [Manage shipping addresses, wishlists, and notification preferences.],
    [*Pre/Post*], [
      Pre: customer is authenticated.
      Post: profile data and preferences updated.
    ],
    [*Scenario*], [
      *Manage Addresses*
      + Navigates to address book page.
      + System displays saved addresses with type labels and default indicators.
      + Creates a new address: enters name, street, city, postal code, country, and state.
      + Assigns address type (shipping, billing, or both) and optionally sets as default.
      + Optionally edits or removes existing addresses.
      + Saves; system validates required fields and address format, persists, and confirms.
      ,
      *Manage Wishlists*
      + On a product detail page, clicks Add to Wishlist.
      + System displays dialog to select wishlist or create new.
      + Selects an existing wishlist or creates a new named wishlist; optionally adds a note.
      + System adds the product variant to the selected wishlist and confirms.
      + Navigates to wishlists section to view all with item counts and preview thumbnails.
      + Selects a wishlist to view items, remove items, rename, or delete the list.
      ,
      *Manage Notification Preferences*
      + Navigates to notification preferences page.
      + System displays all notification categories with current per-channel opt-in/out status.
      + Toggles individual notification categories on or off per channel.
      + Saves; system persists and confirms.
      ,
    ],
    [*Alternatives*], [
      + A1. Same variant already in wishlist → system notifies and suggests adding note to existing entry.
      + A2. Remove address used in active orders → system warns it is referenced by past orders (retained); allows soft-delete.
      + A3. Opts out of all notifications → system warns important notifications also suppressed; asks confirmation.
      + A4. Deletes wishlist → system asks for confirmation; list and items permanently removed.
    ],
    [*Exceptions*], [
      + E1. Address validation fails for invalid country/state → system highlights mismatch and prevents save.
      + E2. Product variant archived since added to wishlist → system displays with Unavailable label and remove suggestion.
      + E3. Persistence failure → system reports failure and retains unsaved changes for retry.
    ],
    [*Requirements*], [PRF-FR-01, PRF-FR-02, PRF-FR-03],
  ),
    kind: table,
  caption: [Profile Management.],
)
