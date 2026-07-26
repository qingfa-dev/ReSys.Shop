==== Shipping Method Configuration

// Diagram placeholder: Shipping Configuration use case diagram

==== UC-ADM-SHP-01 — Manage Shipping Methods

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-SHP-01],
    [*Use Case Name*], [Manage Shipping Methods],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create, update, activate, deactivate, or remove shipping methods with carrier and zone configuration.],
    [*Trigger*], [Administrator navigates to shipping method management.],
    [*Preconditions*], [
      - Authenticated with shipping management permissions.
    ],
    [*Postconditions*], [
      - Shipping method configuration updated. Active methods available for checkout if zone-applicable.
    ],
    [*Main Success Scenario*], [
      1. Navigates to shipping method management.
      2. System displays configured shipping methods with activation status.
      3. Creates a new method with name, description, carrier identifier, and applicable zones.
      4. Optionally edits, activates, deactivates, or removes existing methods.
      5. Saves the changes.
      6. System validates method name uniqueness.
      7. System persists the configuration.
      8. System confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Deactivate method in active checkouts: system warns; existing selections unaffected.
      A2. Remove method with active rates: system prompts to remove rates or reassign.
      A3. No zones assigned: system warns method will not be selectable at checkout.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification: system refreshes and asks to retry.
    ],
    [*Related Requirements*], [SHP-FR-01, SHP-FR-04],
  ),
  caption: [UC-ADM-SHP-01 -- Manage Shipping Methods.],
)

==== UC-ADM-SHP-02 — Manage Shipping Rates

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-SHP-02],
    [*Use Case Name*], [Manage Shipping Rates],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create, update, or remove shipping rates per method with weight, cart value, and zone tiers.],
    [*Trigger*], [Administrator navigates to shipping rate configuration.],
    [*Preconditions*], [
      - Authenticated with shipping management permissions.
      - Shipping method exists.
    ],
    [*Postconditions*], [
      - Shipping rates applied during checkout calculation for matching carts.
    ],
    [*Main Success Scenario*], [
      1. Selects a shipping method from the method listing.
      2. System displays current rate tiers for the method.
      3. Creates a new rate tier: selects zone, defines weight and cart value ranges, enters rate amount.
      4. Optionally edits or removes existing rate tiers.
      5. Saves the changes.
      6. System validates rate tier ranges do not overlap for the same zone.
      7. System persists the rate configuration.
      8. System confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Ranges overlap with existing tiers: system rejects and highlights conflicting tiers.
      A2. Unbounded range (no maximum): system treats as catch-all above minimum.
      A3. Multiple tiers per method and zone: system selects matching tier at checkout based on cart weight.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification: system refreshes and asks to retry.
    ],
    [*Related Requirements*], [SHP-FR-05],
  ),
  caption: [UC-ADM-SHP-02 -- Manage Shipping Rates.],
)

==== Reference Data Management

// Diagram placeholder: Reference Data use case diagram

==== UC-ADM-REF-01 — Manage Countries

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-REF-01],
    [*Use Case Name*], [Manage Countries],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create, update, or remove country records with ISO codes and active status.],
    [*Trigger*], [Administrator navigates to country management.],
    [*Preconditions*], [
      - Authenticated with reference data management permissions.
    ],
    [*Postconditions*], [
      - Country data updated. Active countries available in address forms and shipping zones.
    ],
    [*Main Success Scenario*], [
      1. Navigates to country management.
      2. System displays list of countries with name, ISO code, and active status.
      3. Creates a new country record with display name and ISO 3166-1 code.
      4. Sets the active status flag.
      5. Optionally edits or removes existing country records.
      6. Saves the changes.
      7. System validates ISO code uniqueness and format.
      8. System persists the country data.
      9. System confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Deactivate country: system warns addresses retained but country not in new forms.
      A2. Delete country with associated states: system warns states will be orphaned.
      A3. Delete country in shipping zones: system warns and suggests deactivating instead.
    ],
    [*Exception Flows*], [
      E1. ISO code already exists: system rejects and prompts for different code.
    ],
    [*Related Requirements*], [LOC-FR-01, LOC-FR-03],
  ),
  caption: [UC-ADM-REF-01 -- Manage Countries.],
)

==== UC-ADM-REF-02 — Manage States

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-REF-02],
    [*Use Case Name*], [Manage States],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create, update, or remove state records linked to a parent country, with active status.],
    [*Trigger*], [Administrator navigates to state management.],
    [*Preconditions*], [
      - Authenticated with reference data management permissions.
      - Parent country exists.
    ],
    [*Postconditions*], [
      - State data updated. Active states available for address validation.
    ],
    [*Main Success Scenario*], [
      1. Navigates to state management.
      2. System displays list of states with name, ISO code, parent country, and active status.
      3. Creates a new state record with display name, ISO 3166-2 code, and selects parent country.
      4. Sets the active status flag.
      5. Optionally edits or removes existing state records.
      6. Saves the changes.
      7. System validates ISO code uniqueness within parent country.
      8. System persists the state data.
      9. System confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Deactivate state: system warns addresses retained but state not in new forms.
      A2. Deactivate parent country: system prompts whether to cascade-deactivate associated states.
      A3. Delete state in shipping zones: system warns and suggests deactivating instead.
    ],
    [*Exception Flows*], [
      E1. ISO code already exists for parent country: system rejects and prompts for different code.
    ],
    [*Related Requirements*], [LOC-FR-02, LOC-FR-04],
  ),
  caption: [UC-ADM-REF-02 -- Manage States.],
)
