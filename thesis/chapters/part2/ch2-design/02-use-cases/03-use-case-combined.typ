=== Use Case Overview

The platform's 26 use cases are organised around three actors and grouped by business module, as summarised in @tbl-uc-summary and structured by the work breakdown in @fig-uc-overview. The Administrator interacts with seven modules (Catalog, Ordering, Payment, Inventory, Identity, Shipping, Location); the Customer interacts with five modules (Catalog, Ordering, Payment, Identity, Profile); and the System performs background operations through the System Services module.

These modules map onto the feature classification of Section 2.1.3. The Core Research contributions (Visual Search, the ML embedding pipeline, and the model benchmark system) are embodied by UC-STR-SRC (Visual Search), UC-SYS-EMB (Embedding Operations), and UC-ADM-IMG (Image and Embedding Management). The Supporting infrastructure areas (Product Catalog, Order System, Inventory, Authentication) retain full detail across their catalog, ordering, inventory, and identity use cases. Peripheral modules absent from that classification (Payment, Shipping, Profile, Location, and System Maintenance) are specified in condensed form in the following sections.

#figure(
  image(
    "../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-overview-wbs.png",
    width: 100%
  ),
  caption: [Work breakdown of the 26 use cases into the Administration, Storefront, and Background Services work areas.]
) <fig-uc-overview>

The work breakdown above decomposes the 26 use cases into the three actor work areas (Administration, Storefront, Background Services); @tbl-uc-summary below provides the traceability from every use case to the functional requirements defined in Section 2.1.

#figure(
  table(
    columns: (auto, 1.2fr, auto, auto, 1.6fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Module*], [*Related FR group*]),

    [UC-ADM-PROD], [Manage products], [Admin], [Catalog], [CAT-GRP-01, CAT-GRP-02],
    [UC-ADM-VAR], [Manage variants], [Admin], [Catalog], [CAT-GRP-02],
    [UC-ADM-IMG], [Manage images and embeddings], [Admin], [Catalog], [CAT-GRP-03],
    [UC-ADM-TAX], [Manage taxonomies and classification], [Admin], [Catalog], [CAT-GRP-05],
    [UC-ADM-OPT], [Manage option types], [Admin], [Catalog], [CAT-GRP-02],

    [UC-ADM-ORD], [Manage orders], [Admin], [Ordering], [ORD-GRP-02, ORD-GRP-03, ORD-GRP-04],
    [UC-ADM-ORD-ITEMS], [Manage order details], [Admin], [Ordering], [ORD-GRP-04],

    [UC-ADM-PAY], [Manage payments], [Admin], [Payment], [PAY-GRP-02, PAY-GRP-03],
    [UC-ADM-PAY-METHOD], [Manage payment methods], [Admin], [Payment], [PAY-GRP-04],

    [UC-ADM-STK], [Manage stock], [Admin], [Inventory], [INV-GRP-01, INV-GRP-03, INV-GRP-04],
    [UC-ADM-LOC], [Manage stock locations], [Admin], [Inventory], [INV-GRP-01],

    [UC-ADM-USR], [Manage users], [Admin], [Identity], [IDN-GRP-04],
    [UC-ADM-ROL], [Manage roles and permissions], [Admin], [Identity], [IDN-GRP-03],

    [UC-ADM-SHP], [Manage shipping], [Admin], [Shipping], [SHP-GRP-01],
    [UC-ADM-REF], [Manage reference data], [Admin], [Location], [LOC-GRP-01],

    [UC-STR-BRW], [Browse and search catalog], [Customer], [Catalog], [CAT-GRP-01, CAT-GRP-02, CAT-GRP-04, CAT-GRP-05],
    [UC-STR-SRC], [Visual search], [Customer], [Catalog], [CAT-GRP-04],

    [UC-STR-CRT], [Manage cart], [Customer], [Ordering], [ORD-GRP-01],
    [UC-STR-CHK], [Checkout], [Customer], [Ordering], [ORD-GRP-02, ORD-GRP-03],
    [UC-STR-OHI], [Order history], [Customer], [Ordering], [ORD-GRP-04],

    [UC-STR-PAY], [Payment processing], [Customer], [Payment], [PAY-GRP-01],
    [UC-STR-AUT], [Authentication], [Customer], [Identity], [IDN-GRP-01, IDN-GRP-02],
    [UC-STR-SES], [Session management], [Customer], [Identity], [IDN-GRP-01],

    [UC-STR-PRF], [Profile management], [Customer], [Profile], [PRF-GRP-01, PRF-GRP-02],

    [UC-SYS-EMB], [Embedding operations], [System], [Embedding], [CAT-GRP-03],
    [UC-SYS-MNT], [System maintenance], [System], [Infrastructure], [CAT-GRP-04, ORD-GRP-01, INV-GRP-02, PAY-GRP-03],
  ),
  kind: table,
  caption: [
    All 26 use cases with actor, module, and requirement traceability.
  ],
) <tbl-uc-summary>
