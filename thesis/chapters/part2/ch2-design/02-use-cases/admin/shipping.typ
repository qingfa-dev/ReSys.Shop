==== Shipping Method Configuration

// Diagram placeholder: Shipping Configuration use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-ADM-SHP-01], [Manage shipping methods], [Administrator],
    [Create, update, activate, deactivate, or remove shipping methods. Configure carrier and applicable zones per method.],
    [Shipping method available for customer selection at checkout if active and zone-applicable.],
    [SHP-FR-01, SHP-FR-04],
    [UC-ADM-SHP-02], [Manage shipping rates], [Administrator],
    [Create, update, or remove shipping rates per method. Define rate tiers by weight, cart value, and geographic zone.],
    [Shipping rates applied during storefront checkout calculation for matching carts.],
    [SHP-FR-02, SHP-FR-05],
  ),
  caption: [Administrator use cases — Shipping Method Configuration.],
)

==== Reference Data Management

// Diagram placeholder: Reference Data use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-ADM-LOC-01], [Manage countries], [Administrator],
    [Create, update, or remove country records with ISO codes. Set active status to control availability.],
    [Country data updated. Active countries available in address forms and shipping zone configuration.],
    [LOC-FR-01, LOC-FR-03],
    [UC-ADM-LOC-02], [Manage states], [Administrator],
    [Create, update, or remove state records with ISO codes, linked to parent country. Set active status per state.],
    [State data updated. Active states available for address validation within their parent country.],
    [LOC-FR-02, LOC-FR-04],
  ),
  caption: [Administrator use cases — Reference Data Management (Location module).],
)
