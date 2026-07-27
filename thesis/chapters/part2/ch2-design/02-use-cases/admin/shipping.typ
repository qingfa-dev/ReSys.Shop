==== Shipping Method Configuration

// Diagram placeholder: Shipping Configuration use case diagram

==== UC-ADM-SHP — Manage Shipping

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-SHP],
    [*Use Case Name*], [Manage Shipping],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Configure delivery methods and their associated shipping rates.],
    [*Trigger*], [Administrator navigates to shipping configuration.],
    [*Preconditions*], [
      - Authenticated with shipping management permissions.
    ],
    [*Postconditions*], [
      - Shipping methods and rates configured. Active methods available for checkout if zone-applicable.
    ],
    [*Main Success Scenario*], [
      *Manage Shipping Methods*
      1. Navigates to shipping method management.
      2. System displays configured shipping methods with activation status.
      3. Creates a new method with name, description, carrier identifier, and applicable zones.
      4. Optionally edits, activates, deactivates, or removes existing methods.
      5. Saves. System validates method name uniqueness, persists, and confirms.
      ,
      *Manage Shipping Rates*
      1. Selects a shipping method from the method listing.
      2. System displays current rate tiers for the method.
      3. Creates a new rate tier: selects zone, defines weight and cart value ranges, enters rate amount.
      4. Optionally edits or removes existing rate tiers.
      5. Saves. System validates rate tier ranges do not overlap for the same zone, persists, and confirms.
    ],
    [*Alternative Flows*], [
      A1. Deactivate method in active checkouts: system warns; existing selections unaffected.
      A2. Remove method with active rates: system prompts to remove rates or reassign.
      A3. No zones assigned: system warns method will not be selectable at checkout.
      A4. Ranges overlap with existing tiers: system rejects and highlights conflicting tiers.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification: system refreshes and asks to retry.
    ],
    [*Related Requirements*], [SHP-FR-01, SHP-FR-04, SHP-FR-05],
  ),
    kind: table,
  caption: [Manage Shipping.],
)

#figure(
  image(
    "../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-shipping-method.png",
    width: 100%
  ),
  caption: [Use case diagram for Shipping Method Configuration (UC-ADM-SHP).],
) <fig-uc-adm-shp-d>

==== Reference Data Management

// Diagram placeholder: Reference Data use case diagram

==== UC-ADM-REF — Manage Reference Data

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-REF],
    [*Use Case Name*], [Manage Reference Data],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create and update country and state reference data.],
    [*Trigger*], [Administrator navigates to reference data management.],
    [*Preconditions*], [
      - Authenticated with reference data management permissions.
    ],
    [*Postconditions*], [
      - Country and state data updated. Active records available in address forms and shipping zones.
    ],
    [*Main Success Scenario*], [
      *Manage Countries*
      1. Navigates to country management.
      2. System displays list of countries with name, ISO code, and active status.
      3. Creates a new country record with display name and ISO 3166-1 code.
      4. Sets the active status flag; optionally edits or removes existing country records.
      5. Saves. System validates ISO code uniqueness and format, persists, and confirms.
      ,
      *Manage States*
      1. Navigates to state management.
      2. System displays list of states with name, ISO code, parent country, and active status.
      3. Creates a new state record with display name, ISO 3166-2 code, and selects parent country.
      4. Sets the active status flag; optionally edits or removes existing state records.
      5. Saves. System validates ISO code uniqueness within parent country, persists, and confirms.
    ],
    [*Alternative Flows*], [
      A1. Deactivate country: system warns addresses retained but country not in new forms.
      A2. Delete country with associated states: system warns states will be orphaned.
      A3. Delete country in shipping zones: system warns and suggests deactivating instead.
      A4. Deactivate parent country: system prompts whether to cascade-deactivate associated states.
    ],
    [*Exception Flows*], [
      E1. ISO code already exists: system rejects and prompts for different code.
    ],
    [*Related Requirements*], [LOC-FR-01, LOC-FR-02, LOC-FR-03, LOC-FR-04],
  ),
    kind: table,
  caption: [Manage Reference Data.],
)

#figure(
  image(
    "../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-reference-data.png",
    width: 100%
  ),
  caption: [Use case diagram for Reference Data Management (UC-ADM-REF).],
) <fig-uc-adm-ref-d>
