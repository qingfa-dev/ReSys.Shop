==== Shipping Method Configuration

// Diagram placeholder: Shipping Configuration use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-ADM-SHP-01], [Manage shipping methods], [Admin], [Create, update, activate, deactivate, or remove shipping methods; configure carrier and applicable zones per method.], [Admin is authenticated with shipping management permissions.], [Shipping method available for customer selection at checkout if active and zone-applicable.],
  [UC-ADM-SHP-02], [Manage shipping rates], [Admin], [Create, update, or remove shipping rates per method; define rate tiers by weight, cart value, and geographic zone.], [Admin is authenticated. The shipping method exists.], [Shipping rates applied during storefront checkout calculation for matching carts.],
)

==== Reference Data Management

// Diagram placeholder: Reference Data use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-ADM-REF-01], [Manage countries], [Admin], [Create, update, or remove country records with ISO codes; set active status to control availability.], [Admin is authenticated with reference data management permissions.], [Country data updated. Active countries available in address forms and shipping zone configuration.],
  [UC-ADM-REF-02], [Manage states], [Admin], [Create, update, or remove state records with ISO codes, linked to parent country; set active status per state.], [Admin is authenticated. Parent country exists.], [State data updated. Active states available for address validation within their parent country.],
)
