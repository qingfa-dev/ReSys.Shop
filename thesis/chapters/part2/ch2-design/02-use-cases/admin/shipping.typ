==== Shipping Method Configuration
// Diagram placeholder for Shipping Method Configuration

#figure(
  table(
    columns: (auto, auto, auto, 1fr, auto, 1fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Flow*], [*Postcondition*], [*Related FR*]),
    [UC-ADM-SHP-01], [Manage shipping methods], [Admin],
    [Create, update, activate, deactivate, or delete shipping methods. Configure carrier name, pricing rules, and applicable geographic zones per method.],
    [Shipping method available for customer selection at checkout if active and zone-applicable.],
    [SHP-FR-01, SHP-FR-04],
    [UC-ADM-SHP-02], [Manage shipping rates], [Admin],
    [Create, update, or delete shipping rates per method. Define rate tiers by weight range, cart value range, and geographic zone.],
    [Shipping rates applied during storefront checkout calculation for matching carts.],
    [SHP-FR-02, SHP-FR-05],
  ),
  caption: [Administrator use cases — Shipping Method Configuration.],
)

==== Reference Data
// Diagram placeholder for Reference Data

#figure(
  table(
    columns: (auto, auto, auto, 1fr, auto, 1fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Flow*], [*Postcondition*], [*Related FR*]),
    [UC-ADM-LOC-01], [Manage countries], [Admin],
    [Create, update, or delete country records with ISO 3166-1 codes and display names. Set active status to control availability in address forms and shipping zones.],
    [Country data updated; active countries available in storefront address forms and shipping zone configuration.],
    [LOC-FR-01, LOC-FR-03],
    [UC-ADM-LOC-02], [Manage states], [Admin],
    [Create, update, or delete state records with ISO 3166-2 codes, linked to parent country. Set active status per state.],
    [State data updated; active states available for address validation within their parent country.],
    [LOC-FR-02, LOC-FR-04],
  ),
  caption: [Administrator use cases — Reference Data.],
)
