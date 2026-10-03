=== Frontend Implementation

#import "../../../../template/ctu-styles.typ": figure-placeholder

The Storefront provides the primary user interface for customers to browse the catalog, perform visual searches, and manage their shopping experience. Built as a Single Page Application (SPA), it emphasizes performance, interactivity, and a cohesive design system.

- *Framework:* *Vue 3* (Composition API) with *Vite 7* for next-generation build tooling.
- *Styling:* *Tailwind CSS 4* (Utility-first) combined with *PrimeVue 4* component library (Aura theme) for accessible, high-quality UI elements.
- *State Management:* *Pinia* for reactive, centralized state management (cart, user session, search results).
- *Key Dependencies:*
  - `vue-router` for client-side navigation.
  - `vee-validate` + `zod` for robust form validation.
  - `@stripe/stripe-js` for payment integration.
- *Distinctive Features:*
  - *Visual Search UI:* Integration with the camera/upload interfaces to capture images for similarity search.
  - *Real-time Notifications:* Dynamic updates for order status and cart actions.

The frontend is a *Single Page Application (SPA)* built with *Vue 3* and *TypeScript*, designed to deliver a native-like experience. It strictly separates *Presentation* (Components) from *Business Logic* (Composables/Stores).

#include "03-frontend/01-technical-foundation.typ"
#include "03-frontend/02-design-system.typ"
#include "03-frontend/03-ml-ai-features-intro.typ"
#include "03-frontend/04-visual-search-module.typ"
#include "03-frontend/05-contextual-recommendations.typ"
#include "03-frontend/06-core-features-intro.typ"
#include "03-frontend/07-catalog-synchronization.typ"
#include "03-frontend/08-cart-checkout-orchestration.typ"
#include "03-frontend/09-ui-flows-intro.typ"
#include "03-frontend/10-discovery-flow.typ"
#include "03-frontend/11-decision-cart-flow.typ"
#include "03-frontend/12-secure-checkout-flow.typ"
#include "03-frontend/13-retention-order-history.typ"
