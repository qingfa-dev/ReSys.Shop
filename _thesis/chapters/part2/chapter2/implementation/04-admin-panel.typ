=== Admin Panel Implementation

#import "../../../../template/ctu-styles.typ": figure-placeholder
#import "../../../../template/ctu-styles.typ": context-callout

The Admin Dashboard empowers administrators to manage the platform's resources, including product catalogs, inventory levels, order fulfillment, and system analytics. It shares the same technological foundation as the Storefront but focuses on data density and operational control.

- *Role:* Back-office management interface.
- *Framework:* *Vue 3* + *Vite 7*.
- *Styling:* *Tailwind CSS 4* + *PrimeVue 4* (with extensive use of DataTables and Charts).
- *Integration:*
  - *Chart.js:* For visualizing sales trends, traffic, and inventory metrics.
  - *JWT Decode:* For handling secure administrative authentication.
- *Key Modules:*
  - *Product Management:* CRUD operations for products, variants, and image uploads.
  - *Order Processing:* Workflow for reviewing, approving, and shipping orders.
  - *Analytics:* Visual dashboards for low-stock alerts and revenue tracking.

The Admin Panel serves as the *Operational Interface* for the entire platform. Unlike the Storefront, which focuses on conversion, this application is engineered for *Data Density* and *integrity enforcement*, acting as the primary User Interface for the backend's Domain-Driven Design (DDD) Bounded Contexts.

#include "04-admin-panel/01-technical-foundation.typ"
#include "04-admin-panel/02-bounded-contexts-intro.typ"
#include "04-admin-panel/03-business-intelligence.typ"
#include "04-admin-panel/04-fulfillment-management.typ"
#include "04-admin-panel/05-catalog-definition.typ"
#include "04-admin-panel/06-identity-governance.typ"
