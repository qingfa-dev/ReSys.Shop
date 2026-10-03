== FUNCTIONAL DECOMPOSITION <func_decomp>

The system's functionality is decomposed into hierarchical modules to ensure separation of concerns and clarify the scope of features. The functional decomposition diagram below illustrates the organization of features across the Customer (Storefront), Administrator (Back Office), and System (Background Services) domains.

#figure(
  image("/images/diagrams/usecases/00-functional-decomposition.png", width: 95%),
  caption: [System Functional Breakdown (WBS)],
)

== USE CASE SPECIFICATIONS <uc_specs>

This section provides detailed specifications for the 20 verified use cases of the system, including ERP-grade inventory auditing and financial tracking capabilities, split by functional area.

#include "use-cases/01-customer.typ"
#include "use-cases/02-admin.typ"
#include "use-cases/03-system.typ"

=== Use Case Dependencies

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    stroke: 0.5pt,
    align: (center, left, left),
    [*Use Case*], [*Depends On*], [*Triggers*],
    [UC-0001 (Visual Search)], [UC-0016 (Vectors)], [-],
    [UC-0004 (Recommendations)], [UC-0016 (Vectors)], [-],
    [UC-0002 (Checkout)], [Inventory (UC-0012)], [UC-0017 (Stock Res)],
    [UC-0013 (Fulfillment)], [UC-0002 (Order)], [-],
    [UC-0016 (Analytics)], [UC-0002, UC-0012], [-],
    [UC-0022 (Stock Audit)], [UC-0012, UC-0013], [-],
  ),
  caption: [System Use Case Dependencies],
)


