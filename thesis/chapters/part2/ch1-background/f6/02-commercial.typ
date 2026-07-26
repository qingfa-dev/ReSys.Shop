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

These products share common limitations for independent projects: they are proprietary and cannot be studied or modified, API access incurs costs at query volume, and reliance on external services creates vendor lock-in. This thesis demonstrates that comparable functionality is achievable with open-source tools, providing both a reference implementation and a cost-effective alternative for smaller deployments.
