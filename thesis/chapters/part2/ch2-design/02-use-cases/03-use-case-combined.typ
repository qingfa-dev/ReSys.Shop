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
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Module*], [*Related FR*]),

    [UC-ADM-PROD], [Manage products], [Admin], [Catalog], [CAT-FR-01, CAT-FR-02, CAT-FR-03, CAT-FR-11, CAT-FR-12, CAT-FR-13],
    [UC-ADM-VAR], [Manage variants], [Admin], [Catalog], [CAT-FR-03, CAT-FR-10, CAT-FR-21, CAT-FR-22],
    [UC-ADM-IMG], [Manage images and embeddings], [Admin], [Catalog], [CAT-FR-04, CAT-FR-05, CAT-FR-14, CAT-FR-15],
    [UC-ADM-TAX], [Manage taxonomies and classification], [Admin], [Catalog], [CAT-FR-09, CAT-FR-18, CAT-FR-19],
    [UC-ADM-OPT], [Manage option types], [Admin], [Catalog], [CAT-FR-10, CAT-FR-20],

    [UC-ADM-ORD], [Manage orders], [Admin], [Ordering], [ORD-FR-04, ORD-FR-05, ORD-FR-06, ORD-FR-07, ORD-FR-09, ORD-FR-13],
    [UC-ADM-ORD-ITEMS], [Manage order details], [Admin], [Ordering], [ORD-FR-13],

    [UC-ADM-PAY], [Manage payments], [Admin], [Payment], [PAY-FR-03, PAY-FR-05, PAY-FR-07, PAY-FR-09],
    [UC-ADM-PAY-METHOD], [Manage payment methods], [Admin], [Payment], [PAY-FR-10],

    [UC-ADM-STK], [Manage stock], [Admin], [Inventory], [INV-FR-02, INV-FR-05, INV-FR-06, INV-FR-08, INV-FR-09, INV-FR-10, INV-FR-12],
    [UC-ADM-LOC], [Manage stock locations], [Admin], [Inventory], [INV-FR-01],

    [UC-ADM-USR], [Manage users], [Admin], [Identity], [IDN-FR-09, IDN-FR-13],
    [UC-ADM-ROL], [Manage roles and permissions], [Admin], [Identity], [IDN-FR-11, IDN-FR-12],

    [UC-ADM-SHP], [Manage shipping], [Admin], [Shipping], [SHP-FR-01, SHP-FR-04, SHP-FR-05],
    [UC-ADM-REF], [Manage reference data], [Admin], [Location], [LOC-FR-01, LOC-FR-02, LOC-FR-03, LOC-FR-04],

    [UC-STR-BRW], [Browse and search catalog], [Customer], [Catalog], [CAT-FR-01, CAT-FR-02, CAT-FR-03, CAT-FR-09, CAT-FR-16, CAT-FR-22],
    [UC-STR-SRC], [Visual search], [Customer], [Catalog], [CAT-FR-06, CAT-FR-07, CAT-FR-08, CAT-FR-17],

    [UC-STR-CRT], [Manage cart], [Customer], [Ordering], [ORD-FR-01, ORD-FR-02, ORD-FR-10],
    [UC-STR-CHK], [Checkout], [Customer], [Ordering], [ORD-FR-04, ORD-FR-05, ORD-FR-08, ORD-FR-11, ORD-FR-12],
    [UC-STR-OHI], [Order history], [Customer], [Ordering], [ORD-FR-07, ORD-FR-14],

    [UC-STR-PAY], [Payment processing], [Customer], [Payment], [PAY-FR-01, PAY-FR-02],
    [UC-STR-AUT], [Authentication], [Customer], [Identity], [IDN-FR-01, IDN-FR-02, IDN-FR-08, IDN-FR-14],
    [UC-STR-SES], [Session management], [Customer], [Identity], [IDN-FR-04, IDN-FR-05, IDN-FR-16],

    [UC-STR-PRF], [Profile management], [Customer], [Profile], [PRF-FR-01, PRF-FR-02, PRF-FR-03],

    [UC-SYS-EMB], [Embedding operations], [System], [Embedding], [CAT-FR-05, CAT-FR-15],
    [UC-SYS-MNT], [System maintenance], [System], [Infrastructure], [CAT-FR-06, CAT-FR-08, ORD-FR-03, INV-FR-07, PAY-FR-04],
  ),
  kind: table,
  caption: [
    All 26 use cases with actor, module, and requirement traceability.
  ],
) <tbl-uc-summary>
