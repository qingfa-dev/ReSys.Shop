==== Shipping Method Configuration

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-shipping-method.png",
    width: 70%
  ),
  caption: [Use case diagram for Shipping Method Configuration (UC-ADM-SHP).],
) <fig-uc-adm-shp-d>

==== UC-ADM-SHP: Manage Shipping

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-ADM-SHP — Manage Shipping],
    [*Actor*], [Administrator],
    [*Goal*], [Configure delivery methods and their associated shipping rates.],
    [*Pre/Post*], [
      Pre: authenticated with shipping management permissions.
      Post: shipping methods and rates configured; active methods available for checkout if zone-applicable.
    ],
    [*Scenario*], [
      *Manage Shipping Methods*
      + Navigates to shipping method management.
      + System displays configured shipping methods with activation status.
      + Creates new method with name, description, carrier identifier, and applicable zones.
      + Optionally edits, activates, deactivates, or removes existing methods.
      + Saves; system validates method name uniqueness, persists, confirms.
      ,
      *Manage Shipping Rates*
      + Selects shipping method from method listing.
      + System displays current rate tiers for method.
      + Creates new rate tier: selects zone, defines weight and cart value ranges, enters rate amount.
      + Optionally edits or removes existing rate tiers.
      + Saves; system validates rate tier ranges do not overlap for same zone, persists, confirms.
      ,
    ],
    [*Alternatives*], [
      + A1. Deactivate method in active checkouts → system warns; existing selections unaffected.
      + A2. Remove method with active rates → system prompts to remove rates or reassign.
      + A3. No zones assigned → system warns method will not be selectable at checkout.
      + A4. Ranges overlap with existing tiers → system rejects, highlights conflicting tiers.
    ],
    [*Exceptions*], [
      + E1. Concurrent modification → system refreshes, asks to retry.
    ],
    [*Requirements*], [SHP-FR-01, SHP-FR-04, SHP-FR-05],
  ),
    kind: table,
  caption: [Manage Shipping.],
)

==== Reference Data Management

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-reference-data.png",
    width: 70%
  ),
  caption: [Use case diagram for Reference Data Management (UC-ADM-REF).],
) <fig-uc-adm-ref-d>

==== UC-ADM-REF: Manage Reference Data

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-ADM-REF — Manage Reference Data],
    [*Actor*], [Administrator],
    [*Goal*], [Create and update country and state reference data.],
    [*Pre/Post*], [
      Pre: authenticated with reference data management permissions.
      Post: country and state data updated; active records available in address forms and shipping zones.
    ],
    [*Scenario*], [
      *Manage Countries*
      + Navigates to country management.
      + System displays list of countries with name, ISO code, and active status.
      + Creates new country record with display name and ISO 3166-1 code.
      + Sets active status flag; optionally edits or removes existing country records.
      + Saves; system validates ISO code uniqueness and format, persists, confirms.
      ,
      *Manage States*
      + Navigates to state management.
      + System displays list of states with name, ISO code, parent country, and active status.
      + Creates new state record with display name, ISO 3166-2 code, selects parent country.
      + Sets active status flag; optionally edits or removes existing state records.
      + Saves; system validates ISO code uniqueness within parent country, persists, confirms.
      ,
    ],
    [*Alternatives*], [
      + A1. Deactivate country → system warns addresses retained but country not in new forms.
      + A2. Delete country with associated states → system warns states will be orphaned.
      + A3. Delete country in shipping zones → system warns, suggests deactivating instead.
      + A4. Deactivate parent country → system prompts whether to cascade-deactivate associated states.
    ],
    [*Exceptions*], [
      + E1. ISO code already exists → system rejects, prompts for different code.
    ],
    [*Requirements*], [LOC-FR-01, LOC-FR-02, LOC-FR-03, LOC-FR-04],
  ),
    kind: table,
  caption: [Manage Reference Data.],
)
