==== Profile and Preferences
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-profile-preferences.png",
    width: 50%
  ),
  caption: [Use case diagram for Profile and Preferences (UC-STR-PRF).],
) <fig-uc-str-prf-d>

==== UC-STR-PRF: Profile Management

*Goal:* Manage shipping addresses, wishlists, and notification preferences. *Trigger:* the authenticated customer opens their profile. *Related requirements:* PRF-GRP-01, PRF-GRP-02. The flow manages an address book, named wishlists with variant entries, and per-channel notification opt-in/out; alternatives cover duplicate wishlist items and addresses referenced by orders, and exceptions handle address validation, archived variants, and persistence failure.
