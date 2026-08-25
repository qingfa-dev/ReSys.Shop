=== Commercial Systems

Several platforms have deployed visual search at production scale.

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    stroke: 0.5pt,
    align: (center, left, left),
    [*Product*], [*Key Strength*], [*Limitation*],
    [Google Lens], [Massive scale, general-domain coverage], [Closed ecosystem, not customisable],
    [Pinterest Lens], [Over 600M monthly searches, style-aware], [Proprietary, requires Pinterest integration],
    [ASOS Style Match], [Fashion-specific accuracy], [Restricted to ASOS catalog only],
    [ViSenze], [API-based, good accuracy], [Paid service with recurring per-query costs],
  ),
    kind: table,
  caption: [Comparison of commercial visual search products],
) <tbl-commercial-comparison>

These products share some common limitations for independent projects:

- They are proprietary, so they cannot be studied or changed.
- API access costs money based on how many queries are made.
- Relying on external services creates dependency on that provider.

This thesis shows that similar functionality can be built using open-source tools. This provides both a reference implementation and a lower-cost alternative for smaller deployments.
