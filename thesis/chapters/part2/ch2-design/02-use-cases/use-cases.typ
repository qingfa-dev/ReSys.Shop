== Functional Decomposition and Use Cases

The system requirements are presented using a use case diagram, a use case summary matrix, and detailed scenario specifications for selected core use cases. This structure improves readability while preserving traceability between functional requirements, design decisions, and test cases.

=== Use Case Diagram

// Diagram: Use case diagram showing all actors and their use cases
// #figure(image("images/diagrams/use-case-overview.png", width: 100%), caption: [System use case diagram showing the three actors and their primary use cases grouped by business domain.])

The three actors (Administrator, Customer, System) interact with the platform across eight business domains. The Administrator manages product catalogues, order fulfilment, payment processing, inventory, user governance, shipping configuration, and reference data. The Customer browses, searches, purchases, and manages account information. The System performs automated embedding generation, cart maintenance, inventory reservation, payment webhook processing, and index optimisation.

=== Use Case Summary Matrix

Table @tbl-uc-summary consolidates all use cases across the three actors and provides traceability to the functional requirements defined in Section 2.1.

#figure(
  table(
    columns: (auto, auto, auto, auto, auto),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Module*], [*Related FR*]),

    // Administrator — Catalog
    [UC-ADM-PROD-01], [Create product], [Admin], [Catalog], [CAT-FR-01, CAT-FR-02, CAT-FR-03, CAT-FR-13],
    [UC-ADM-PROD-02], [Update product], [Admin], [Catalog], [CAT-FR-01, CAT-FR-12],
    [UC-ADM-PROD-03], [Delete product], [Admin], [Catalog], [CAT-FR-01],
    [UC-ADM-VAR-01], [Add variant], [Admin], [Catalog], [CAT-FR-03, CAT-FR-21, CAT-FR-22],
    [UC-ADM-VAR-02], [Manage option values], [Admin], [Catalog], [CAT-FR-10, CAT-FR-20, CAT-FR-21],
    [UC-ADM-VAR-03], [Manage pricing], [Admin], [Catalog], [CAT-FR-03, CAT-FR-22],
    [UC-ADM-IMG-01], [Upload variant images], [Admin], [Catalog], [CAT-FR-04, CAT-FR-05, CAT-FR-15],
    [UC-ADM-IMG-02], [Regenerate embeddings], [Admin], [Catalog], [CAT-FR-05, CAT-FR-08, CAT-FR-15],
    [UC-ADM-TAX-01], [Manage taxonomies], [Admin], [Catalog], [CAT-FR-09],
    [UC-ADM-TAX-02], [Classify products], [Admin], [Catalog], [CAT-FR-09, CAT-FR-19],
    [UC-ADM-OPT-01], [Manage option types], [Admin], [Catalog], [CAT-FR-10, CAT-FR-20],

    // Administrator — Ordering
    [UC-ADM-ORD-01], [View orders], [Admin], [Ordering], [ORD-FR-05, ORD-FR-06, ORD-FR-13],
    [UC-ADM-ORD-02], [Update order], [Admin], [Ordering], [ORD-FR-05, ORD-FR-13],
    [UC-ADM-ORD-03], [Approve order], [Admin], [Ordering], [ORD-FR-04, ORD-FR-13],
    [UC-ADM-ORD-04], [Complete order], [Admin], [Ordering], [ORD-FR-09, ORD-FR-13],
    [UC-ADM-ORD-05], [Cancel order], [Admin], [Ordering], [ORD-FR-07],
    [UC-ADM-ORD-06], [Resume order], [Admin], [Ordering], [ORD-FR-13],

    // Administrator — Payment
    [UC-ADM-PAY-01], [Capture payment], [Admin], [Payment], [PAY-FR-03],
    [UC-ADM-PAY-02], [Refund payment], [Admin], [Payment], [PAY-FR-03, PAY-FR-05],
    [UC-ADM-PAY-03], [Void payment], [Admin], [Payment], [PAY-FR-09],
    [UC-ADM-PAY-04], [View payments], [Admin], [Payment], [PAY-FR-07],
    [UC-ADM-PAY-05], [Manage payment methods], [Admin], [Payment], [PAY-FR-10],

    // Administrator — Inventory
    [UC-ADM-LOC-01], [Manage stock locations], [Admin], [Inventory], [INV-FR-01],
    [UC-ADM-STK-01], [Manage stock items], [Admin], [Inventory], [INV-FR-02, INV-FR-06, INV-FR-08],
    [UC-ADM-STK-02], [Restock inventory], [Admin], [Inventory], [INV-FR-02, INV-FR-06, INV-FR-08],
    [UC-ADM-STK-03], [Transfer stock], [Admin], [Inventory], [INV-FR-05, INV-FR-10],
    [UC-ADM-STK-04], [Review stock movements], [Admin], [Inventory], [INV-FR-06, INV-FR-12],
    [UC-ADM-STK-05], [Monitor low stock], [Admin], [Inventory], [INV-FR-09],

    // Administrator — Identity
    [UC-ADM-USR-01], [Manage users], [Admin], [Identity], [IDN-FR-01, IDN-FR-09, IDN-FR-13],
    [UC-ADM-ROL-01], [Manage roles], [Admin], [Identity], [IDN-FR-07, IDN-FR-11],
    [UC-ADM-ROL-02], [Assign user roles], [Admin], [Identity], [IDN-FR-07, IDN-FR-12],
    [UC-ADM-ROL-03], [Grant user permissions], [Admin], [Identity], [IDN-FR-07, IDN-FR-12],
    [UC-ADM-ROL-04], [View permissions catalog], [Admin], [Identity], [IDN-FR-07],

    // Administrator — Shipping and Location
    [UC-ADM-SHP-01], [Manage shipping methods], [Admin], [Shipping], [SHP-FR-01, SHP-FR-04],
    [UC-ADM-SHP-02], [Manage shipping rates], [Admin], [Shipping], [SHP-FR-02, SHP-FR-05],
    [UC-ADM-REF-01], [Manage countries], [Admin], [Location], [LOC-FR-01, LOC-FR-03],
    [UC-ADM-REF-02], [Manage states], [Admin], [Location], [LOC-FR-02, LOC-FR-04],

    // Customer — Catalog
    [UC-STR-BRW-01], [Browse catalog], [Customer], [Catalog], [CAT-FR-09, CAT-FR-10],
    [UC-STR-BRW-02], [View product detail], [Customer], [Catalog], [CAT-FR-01, CAT-FR-16],
    [UC-STR-BRW-03], [Keyword search], [Customer], [Catalog], [CAT-FR-01],
    [UC-STR-SRC-01], [Search by image (CBIR)], [Customer], [Catalog], [CAT-FR-06, CAT-FR-07, CAT-FR-08],
    [UC-STR-SRC-02], [View similar products], [Customer], [Catalog], [CAT-FR-17],

    // Customer — Ordering
    [UC-STR-CRT-01], [Manage cart], [Customer], [Ordering], [ORD-FR-01, ORD-FR-10],
    [UC-STR-CRT-02], [Associate cart with account], [Customer], [Ordering], [ORD-FR-02],
    [UC-STR-CHK-01], [Select shipping address], [Customer], [Ordering], [ORD-FR-04],
    [UC-STR-CHK-02], [Select shipping method], [Customer], [Ordering], [ORD-FR-04, ORD-FR-12, SHP-FR-02, SHP-FR-06],
    [UC-STR-CHK-03], [Complete checkout], [Customer], [Ordering], [ORD-FR-04, ORD-FR-05, ORD-FR-08, ORD-FR-11],
    [UC-STR-OHI-01], [View order history], [Customer], [Ordering], [ORD-FR-14],
    [UC-STR-OHI-02], [Cancel order], [Customer], [Ordering], [ORD-FR-07],

    // Customer — Payment
    [UC-STR-PAY-01], [Create payment intent], [Customer], [Payment], [PAY-FR-01],
    [UC-STR-PAY-02], [Confirm payment], [Customer], [Payment], [PAY-FR-02],

    // Customer — Identity
    [UC-STR-AUT-01], [Register], [Customer], [Identity], [IDN-FR-01],
    [UC-STR-AUT-02], [Login with password], [Customer], [Identity], [IDN-FR-02, IDN-FR-04],
    [UC-STR-AUT-03], [Login with Google], [Customer], [Identity], [IDN-FR-03],
    [UC-STR-AUT-04], [Reset password], [Customer], [Identity], [IDN-FR-08, IDN-FR-14],
    [UC-STR-AUT-05], [Change password], [Customer], [Identity], [IDN-FR-14],
    [UC-STR-SES-01], [Refresh session], [Customer], [Identity], [IDN-FR-04, IDN-FR-05],
    [UC-STR-SES-02], [Logout], [Customer], [Identity], [IDN-FR-16],

    // Customer — Profile
    [UC-STR-PRF-01], [Manage addresses], [Customer], [Profile], [PRF-FR-01],
    [UC-STR-PRF-02], [Manage wishlists], [Customer], [Profile], [PRF-FR-02],
    [UC-STR-PRF-03], [Manage notification preferences], [Customer], [Profile], [PRF-FR-03],

    // System
    [UC-SYS-EMB-01], [Generate image embeddings], [System], [Embedding], [CAT-FR-05, CAT-FR-15],
    [UC-SYS-EMB-02], [Regenerate all embeddings], [System], [Embedding], [CAT-FR-08, CAT-FR-15],
    [UC-SYS-MNT-01], [Verify model health], [System], [Infrastructure], [NFR-04],
    [UC-SYS-MNT-02], [Expire abandoned carts], [System], [Ordering], [ORD-FR-03, NFR-05],
    [UC-SYS-MNT-03], [Manage inventory reservations], [System], [Inventory], [INV-FR-03, INV-FR-07, NFR-05],
    [UC-SYS-MNT-04], [Process payment webhooks], [System], [Payment], [PAY-FR-04, PAY-FR-07, NFR-05],
    [UC-SYS-MNT-05], [Maintain search index], [System], [Catalog], [CAT-FR-06],
  ),
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
