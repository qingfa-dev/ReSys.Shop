== Functional Decomposition and Use Cases

The system requirements are presented through a functional decomposition diagram, a use case overview diagram, a use case summary matrix, and detailed scenario specifications for selected core use cases. This structure improves readability while preserving traceability between functional requirements, design decisions, and test cases.

=== Functional Decomposition

The functional decomposition of ReSys.Shop, shown in @fig-func-decomp, organises the platform into three top-level functional areas: Administration, Storefront, and Background Services. Each functional area decomposes into constituent modules, reflecting the responsibilities defined in the use case summary matrix (Section 2.2.3).

The Administration area encompasses seven modules accessible to the Administrator actor: Catalog Management, Order Management, Payment Management, Inventory Management, Identity Management, Shipping Management, and Location Management. The Storefront area covers Customer-facing functionality across three modules: Product Discovery (browse, search, visual search), Purchase Flow (cart, checkout, payment processing, order history), and Account Management (authentication, session management, profile management). Background Services include five automated processes performed by the System actor: Embedding Generation, Cart Expiry, Inventory Reservation, Payment Webhook Processing, and Index Optimisation.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/02-use-cases/P2S2.2.2_functional-decomposition.png", width: 100%),
  caption: [Functional decomposition of ReSys.Shop using a Work Breakdown Structure (WBS), showing the hierarchical breakdown into three functional areas and their constituent modules and sub-functions.]
) <fig-func-decomp>

=== Use Case Overview

@fig-uc-overview presents the system use case diagram, showing all 26 use cases from the summary matrix grouped by business module. Each package corresponds to a module from the matrix, containing the use cases accessible to the connected actors. The Administrator interacts with seven modules (Catalog, Ordering, Payment, Inventory, Identity, Shipping, Location), the Customer interacts with five modules (Catalog, Ordering, Payment, Identity, Profile), and the System performs background operations through the System Services module.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/02-use-cases/P2S2.2.2_use-case-overview.png", width: 100%),
  caption: [System use case diagram showing all 26 use cases from the summary matrix, grouped by business module with actor-to-module associations.]
) <fig-uc-overview>

=== Use Case Summary Matrix

@tbl-uc-summary consolidates all use cases across the three actors and provides traceability to the functional requirements defined in Section 2.1.

#figure(
  table(
    columns: (auto, auto, auto, auto, auto),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Module*], [*Related FR*]),

    // Administrator — Catalog
    [UC-ADM-PROD], [Manage products], [Admin], [Catalog], [CAT-FR-01, CAT-FR-02, CAT-FR-03, CAT-FR-11, CAT-FR-12, CAT-FR-13],
    [UC-ADM-VAR], [Manage variants], [Admin], [Catalog], [CAT-FR-03, CAT-FR-10, CAT-FR-21, CAT-FR-22],
    [UC-ADM-IMG], [Manage images and embeddings], [Admin], [Catalog], [CAT-FR-04, CAT-FR-05, CAT-FR-14, CAT-FR-15],
    [UC-ADM-TAX], [Manage taxonomies and classification], [Admin], [Catalog], [CAT-FR-09, CAT-FR-18, CAT-FR-19],
    [UC-ADM-OPT], [Manage option types], [Admin], [Catalog], [CAT-FR-10, CAT-FR-20],

    // Administrator — Ordering
    [UC-ADM-ORD], [Manage orders], [Admin], [Ordering], [ORD-FR-04, ORD-FR-05, ORD-FR-06, ORD-FR-07, ORD-FR-09, ORD-FR-13],
    [UC-ADM-ORD-ITEMS], [Manage order details], [Admin], [Ordering], [ORD-FR-13],

    // Administrator — Payment
    [UC-ADM-PAY], [Manage payments], [Admin], [Payment], [PAY-FR-03, PAY-FR-05, PAY-FR-07, PAY-FR-09],
    [UC-ADM-PAY-METHOD], [Manage payment methods], [Admin], [Payment], [PAY-FR-10],

    // Administrator — Inventory
    [UC-ADM-STK], [Manage stock], [Admin], [Inventory], [INV-FR-02, INV-FR-05, INV-FR-06, INV-FR-08, INV-FR-09, INV-FR-10, INV-FR-12],
    [UC-ADM-LOC], [Manage stock locations], [Admin], [Inventory], [INV-FR-01],

    // Administrator — Identity
    [UC-ADM-USR], [Manage users], [Admin], [Identity], [IDN-FR-09, IDN-FR-13],
    [UC-ADM-ROL], [Manage roles and permissions], [Admin], [Identity], [IDN-FR-11, IDN-FR-12],

    // Administrator — Shipping and Location
    [UC-ADM-SHP], [Manage shipping], [Admin], [Shipping], [SHP-FR-01, SHP-FR-04, SHP-FR-05],
    [UC-ADM-REF], [Manage reference data], [Admin], [Location], [LOC-FR-01, LOC-FR-02, LOC-FR-03, LOC-FR-04],

    // Customer — Catalog
    [UC-STR-BRW], [Browse and search catalog], [Customer], [Catalog], [CAT-FR-01, CAT-FR-02, CAT-FR-03, CAT-FR-09, CAT-FR-16, CAT-FR-22],
    [UC-STR-SRC], [Visual search], [Customer], [Catalog], [CAT-FR-06, CAT-FR-07, CAT-FR-08, CAT-FR-17],

    // Customer — Ordering
    [UC-STR-CRT], [Manage cart], [Customer], [Ordering], [ORD-FR-01, ORD-FR-02, ORD-FR-10],
    [UC-STR-CHK], [Checkout], [Customer], [Ordering], [ORD-FR-04, ORD-FR-05, ORD-FR-08, ORD-FR-11, ORD-FR-12],
    [UC-STR-OHI], [Order history], [Customer], [Ordering], [ORD-FR-07, ORD-FR-14],

    // Customer — Payment and Identity
    [UC-STR-PAY], [Payment processing], [Customer], [Payment], [PAY-FR-01, PAY-FR-02],
    [UC-STR-AUT], [Authentication], [Customer], [Identity], [IDN-FR-01, IDN-FR-02, IDN-FR-03, IDN-FR-08, IDN-FR-14],
    [UC-STR-SES], [Session management], [Customer], [Identity], [IDN-FR-04, IDN-FR-05, IDN-FR-16],

    // Customer — Profile
    [UC-STR-PRF], [Profile management], [Customer], [Profile], [PRF-FR-01, PRF-FR-02, PRF-FR-03],

    // System
    [UC-SYS-EMB], [Embedding operations], [System], [Embedding], [CAT-FR-05, CAT-FR-15],
    [UC-SYS-MNT], [System maintenance], [System], [Infrastructure], [CAT-FR-06, CAT-FR-08, ORD-FR-03, INV-FR-07, PAY-FR-04],
  ),
  kind: table,
  caption: [
    Use case summary matrix. All use cases are listed with their actor, associated business module, and traceability to functional requirements defined in Section 2.1.
  ],
) <tbl-uc-summary>

The following sections present detailed scenario specifications for each use case, organised by actor and business module. Each specification includes the actor's goal, trigger, preconditions, postconditions, a numbered main success scenario, alternative and exception flows, and related functional requirements.

=== Administrator Use Cases

The Administrator actor manages data and operational workflows across all eight business modules through a dedicated administration interface.

#include "admin/admin.typ"

=== Customer Use Cases

The Customer actor interacts through the browser-based storefront for product discovery, purchase, and account management.

#include "storefront/storefront.typ"

=== System Use Cases

The System actor represents automated background processes that maintain data consistency, generate embeddings, and perform scheduled operations.

#include "system/system.typ"
