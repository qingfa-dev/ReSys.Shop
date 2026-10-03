==== Shipping Method Configuration

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-shipping-method.png",
    width: 60%
  ),
  caption: [Use case diagram for Shipping Method Configuration (UC-ADM-SHP).],
) <fig-uc-adm-shp-d>

==== UC-ADM-SHP: Manage Shipping

*Goal:* Configure delivery methods and their associated shipping rates. *Trigger:* the administrator opens shipping method management. *Related requirements:* SHP-GRP-01. The flow creates, edits, activates, deactivates, or removes shipping methods and their rate tiers (with zone, weight, and cart-value ranges); alternatives warn about methods in active checkouts, methods with no zone, and overlapping rate tiers, and an exception handles concurrent modification.

==== Reference Data Management

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-reference-data.png",
    width: 50%
  ),
  caption: [Use case diagram for Reference Data Management (UC-ADM-REF).],
) <fig-uc-adm-ref-d>

==== UC-ADM-REF: Manage Reference Data

*Goal:* Create and update country and state reference data. *Trigger:* the administrator opens country or state management. *Related requirements:* LOC-GRP-01. The flow creates and edits ISO-coded country and state records with active-status flags; alternatives warn about deactivating or deleting records referenced elsewhere (states, shipping zones, addresses), and an exception handles duplicate ISO codes.
