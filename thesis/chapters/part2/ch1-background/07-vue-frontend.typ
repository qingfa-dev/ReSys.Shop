== Vue.js Frontend

The frontend uses Vue 3 with TypeScript and the Vite build tool. Two surfaces share a common component library and state management:

- *Storefront.* Product catalog browsing with category trees, faceted filters (price, size, colour), and paginated result grids. Visual search: drag-and-drop image upload with client-side preview, similarity-ranked results with thumbnail, price, and score display. Cart management with session-based guest support and checkout flow spanning address entry, delivery selection, payment confirmation, and order completion.

- *Admin panel.* Full CRUD for products, variants, taxonomies, and option types. Order management with fulfilment status tracking. User and role administration. Dashboard with sales and inventory summary charts.

- *Pinia* manages client-side state through typed stores. Each bounded context has its own store (catalog, cart, auth, orders), mirroring the backend module boundaries.
