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
    [*Goal*], [Create, update, activate, deactivate, or remove shipping methods; configure carrier and applicable zones per method.],
    [*Trigger*], [Administrator navigates to the shipping method management interface.],
    [*Preconditions*], [
      - Administrator is authenticated with shipping management permissions.
    ],
    [*Postconditions*], [
      - Shipping method configuration updated.
      - Active methods available for customer selection at checkout if zone-applicable.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the shipping method management interface.
      2. System -- Displays the list of configured shipping methods with activation status.
      3. Administrator -- Creates a new shipping method with a name, description, carrier identifier, and applicable geographic zones.
      4. Administrator -- Optionally edits, activates, deactivates, or removes existing shipping methods.
      5. Administrator -- Saves the changes.
      6. System -- Validates that the method name is unique.
      7. System -- Persists the shipping method configuration.
      8. System -- Confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Administrator deactivates a shipping method currently the default in active checkouts -- System warns that new checkouts cannot use this method; existing selections remain unaffected.
      A2. Administrator removes a shipping method that has active rates -- System prompts to also remove the associated rates or reassign them to another method.
      A3. No geographic zones are assigned to a method -- System warns that the method will not be selectable at checkout because no zones match any delivery address.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification detected -- System detects the shipping method was modified by another session, refreshes the data, and asks the administrator to retry.
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
    [*Goal*], [Create, update, or remove shipping rates per method; define rate tiers by weight, cart value, and geographic zone.],
    [*Trigger*], [Administrator navigates to the shipping rate configuration interface.],
    [*Preconditions*], [
      - Administrator is authenticated with shipping management permissions.
      - The shipping method exists.
    ],
    [*Postconditions*], [
      - Shipping rates applied during storefront checkout calculation for matching carts.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Selects a shipping method from the method listing.
      2. System -- Displays the current rate tiers for the selected method.
      3. Administrator -- Creates a new rate tier: selects applicable geographic zone, defines weight range (min-max), defines cart value range (min-max), and enters the rate amount.
      4. Administrator -- Optionally edits or removes existing rate tiers.
      5. Administrator -- Saves the changes.
      6. System -- Validates that rate tier ranges do not overlap for the same zone.
      7. System -- Persists the rate configuration.
      8. System -- Confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Rate tier ranges overlap with existing tiers -- System rejects and highlights the conflicting tiers.
      A2. Administrator leaves a rate range unbounded (e.g. no maximum weight) -- System accepts and treats the tier as a catch-all for values above the minimum.
      A3. Multiple rates exist for the same method and zone with different weight tiers -- System selects the matching tier at checkout based on cart weight.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification detected -- System detects the rate was modified by another session, refreshes the data, and asks the administrator to retry.
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
    [*Goal*], [Create, update, or remove country records with ISO codes; set active status to control availability.],
    [*Trigger*], [Administrator navigates to the country management interface.],
    [*Preconditions*], [
      - Administrator is authenticated with reference data management permissions.
    ],
    [*Postconditions*], [
      - Country data updated.
      - Active countries available in address forms and shipping zone configuration.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the country management interface.
      2. System -- Displays the list of countries with paging, showing name, ISO code, and active status.
      3. Administrator -- Creates a new country record with display name and ISO 3166-1 code.
      4. Administrator -- Sets the active status flag.
      5. Administrator -- Optionally edits or removes existing country records.
      6. Administrator -- Saves the changes.
      7. System -- Validates that the ISO code is unique and in valid format.
      8. System -- Persists the country data.
      9. System -- Confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Administrator deactivates a country -- System warns that addresses using this country will still be retained but the country will not appear in new address forms.
      A2. Administrator attempts to delete a country with associated states -- System warns that associated states will be orphaned and asks for confirmation.
      A3. Administrator attempts to delete a country referenced by active shipping zones -- System warns and suggests deactivating instead.
    ],
    [*Exception Flows*], [
      E1. ISO code already exists -- System rejects and prompts the administrator to use a different code.
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
    [*Goal*], [Create, update, or remove state records with ISO codes, linked to a parent country; set active status per state.],
    [*Trigger*], [Administrator navigates to the state management interface.],
    [*Preconditions*], [
      - Administrator is authenticated with reference data management permissions.
      - Parent country exists.
    ],
    [*Postconditions*], [
      - State data updated.
      - Active states available for address validation within their parent country.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the state management interface.
      2. System -- Displays the list of states with paging, showing name, ISO code, parent country, and active status.
      3. Administrator -- Creates a new state record with display name, ISO 3166-2 code, and selects the parent country.
      4. Administrator -- Sets the active status flag.
      5. Administrator -- Optionally edits or removes existing state records.
      6. Administrator -- Saves the changes.
      7. System -- Validates that the ISO code is unique within the parent country.
      8. System -- Persists the state data.
      9. System -- Confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Administrator deactivates a state -- System warns that addresses using this state will still be retained but the state will not appear in new address forms for that country.
      A2. Administrator deactivates the parent country -- System prompts whether to cascade-deactivate all associated states.
      A3. Administrator attempts to delete a state referenced by shipping zones -- System warns and suggests deactivating instead.
    ],
    [*Exception Flows*], [
      E1. ISO code already exists for the same parent country -- System rejects and prompts the administrator to use a different code.
    ],
    [*Related Requirements*], [LOC-FR-02, LOC-FR-04],
  ),
  caption: [UC-ADM-REF-02 -- Manage States.],
)
