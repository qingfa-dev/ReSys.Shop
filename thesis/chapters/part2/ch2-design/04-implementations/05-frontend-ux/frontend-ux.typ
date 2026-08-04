=== Frontend Applications

The user-facing components of ReSys.Shop consist of two Vue 3 single-page applications: a customer storefront built with PrimeVue's Aura theme and an administration dashboard using the Sakai theme. This section presents the frontend architecture, the shared API client pattern, and the implemented user interfaces organised by the 26 use cases defined in Section 2.2.2.

==== Frontend Architecture

===== Dual-SPA Structure

Both applications share a common technology foundation: Vue 3.5 with the Composition API, TypeScript ~6.0, Vite 8, Pinia for state management, and Axios for HTTP communication: but differ in their UI component presets. The storefront uses PrimeVue 4 (Aura theme) optimised for consumer browsing, image upload, and product discovery. The administration dashboard uses PrimeVue 5 (Sakai theme) with Chart.js 4 for data-dense views, inline editing, and aggregate dashboards.

Each application is organised by feature module, grouping views, types, services, and API repositories together. Vite provides hot module replacement with sub-second feedback during development; the dev server proxies `/api/` requests to the .NET backend running on a separate Aspire-managed port.

===== API Client Pattern

All communication with the backend follows a typed repository pattern that mirrors the C\# `Result\<T\>` convention. The TypeScript `Result\<T\>` interface carries `isSuccess`, `statusCode`, `data`, and `errors[]` fields, enabling type-safe error handling in the UI without try-catch blocks:

```typescript
export interface Result<T> {
  isSuccess: boolean
  isFailure: boolean
  statusCode: number
  message?: string
  data?: T
  errors?: Array<{
    code: string
    description: string
    field?: string
  }>
}
```

The `BaseRepository` class wraps an Axios instance with typed CRUD methods (`get\<T\>()`, `post\<T\>()`, `uploadFile\<T\>()`) and unified error handling. Axios error responses are unwrapped to preserve the backend's structured error codes and field-level metadata. Each feature module defines its own repository extending this base class.

For the visual search feature, `RecommendationsApiRepository` extends the base repository with a multipart upload method:

```typescript
async searchByImage(file: File): Promise<Result<Product[]>> {
  const formData = new FormData()
  formData.append('image', file)
  const response = await this.client.post<Result<Product[]>>(
    '/api/storefront/search-by-image',
    formData,
    { headers: { 'Content-Type': 'multipart/form-data' } }
  )
  return response.data
}
```

#line(length: 100%, stroke: 0.3pt + luma(200))

==== Storefront Interfaces

The customer storefront implements eight use cases covering product discovery, purchasing, and account management. Each subsection below presents the implemented interface and the screenshot evidence for the corresponding use case.

===== Visual Search: UC-STR-SRC

The visual search interface is the primary research-facing feature. It implements a four-state UI model managed through Vue reactive refs and computed properties:

#figure(
  table(
    columns: (auto, 1.5fr, 2.5fr),
    stroke: 0.5pt,
    align: (left + horizon, left, left),
    inset: 5pt,
    table.header([*State*], [*UI Displayed*], [*Transition Trigger*]),
    [Empty], [Upload prompt with dashed-border drop zone, cloud-upload icon, format description (JPEG, PNG, WebP up to 10 MB)], [Initial page load \| after clearing a previous search],
    [Upload], [Selected image preview displayed at full width with a "Search Similar Products" action button below], [User drops a file into the zone or selects one via file browser],
    [Loading], [Skeleton grid of animated placeholder cards (4 columns desktop, 2 mobile) with pulsing backgrounds], [Search API request in flight (`POST /api/storefront/search-by-image`)],
    [Results], [Responsive product card grid (4/2 columns) with thumbnail, name, price, and a coloured similarity badge (e.g., "93% match")], [API returns a non-empty `results` array],
  ),
  kind: table,
  caption: [Visual search UI state model.],
) <tbl-search-states>

Two input methods are supported: drag-and-drop onto the highlight-responsive upload zone and file browser selection via a styled button labelled "Choose an image". Client-side validation rejects non-image MIME types and files exceeding 10 MB before any network request, providing immediate inline feedback. The `DragEvent` handlers toggle an `isDragging` ref that applies a visual highlight CSS class to the drop zone.

// [SCREENSHOT: cbir-empty-state.png] Visual search page in the empty state: large drop zone with dashed border, cloud-upload icon, description text "Drop an image here or click to browse", format note "JPEG, PNG, or WebP up to 10 MB", and a "Choose an image" button centered below.

// [SCREENSHOT: cbir-upload-state.png] Visual search page in the upload state: the selected image displayed as a large preview with a caption showing the original filename and file size. A "Search Similar Products" button is prominently positioned below the preview. The upload zone collapses to a smaller "Change image" control.

// [SCREENSHOT: cbir-loading-state.png] Visual search page in the loading state: a skeleton grid of 8 placeholder cards with pulsing grey backgrounds, each containing rectangular placeholders for thumbnail, title, and price. A progress indicator or spinner is visible above the grid.

After upload, the component constructs a `FormData` payload with the `image` field containing the selected `File` object. The backend orchestrates embedding extraction and pgvector similarity search as described in Section 2.4.3. The response payload carries product metadata with similarity scores computed server-side:

```json
{
  "results": [{
    "productId": "a1b2c3d4-...",
    "title": "Floral Summer Midi Dress",
    "price": 750000, "currency": "VND",
    "thumbnailUrl": "/images/products/a1b2c3d4_thumb.webp",
    "similarityScore": 0.9328,
    "categoryPath": "Clothing > Dresses > Midi Dresses"
  }],
  "searchDurationMs": 287,
  "model": "Fashion-CLIP"
}
```

Each product card renders a thumbnail image, product title, formatted price, and a colour-coded similarity confidence badge (green for >= 90%, amber for >= 80%, grey below 80%). The query image is displayed in a persistent sidebar panel for reference while scrolling through results. Clicking a product card navigates to the full product detail page.

// [SCREENSHOT: cbir-results-grid.png] Search results grid in the results state: 8 product cards in a 4-column layout, each showing thumbnail, product name, price in VND, and a similarity badge (e.g., "94% match" in green). The query image is displayed in a left sidebar. A "New Search" button is visible above the grid. The search duration (287 ms) and model name (Fashion-CLIP) are shown in a metadata bar below the sidebar.

// [SCREENSHOT: cbir-results-empty.png] Visual search results showing the "No similar products found" empty state: a friendly illustration or icon with the message "We couldn't find products similar to your image. Try a different image or adjust your search." and a "Try Again" button.

#line(length: 100%, stroke: 0.3pt + luma(200))

===== Catalogue Browsing and Product Detail: UC-STR-BRW

The catalogue browser presents products in a paginated, faceted grid with left-sidebar category tree navigation. The category tree is rendered from the hierarchical taxonomy data returned by `GET /api/storefront/taxonomies/tree`, with expandable/collapsible taxon nodes.

Selecting a category filters the product grid.

The product listing grid shows thumbnails, product names, prices (formatted with the master variant's current price), and a hover overlay revealing a quick-add-to-cart button. Pagination controls at the bottom of the grid navigate between result pages. A keyword search bar at the top of the page sends full-text queries to `GET /api/storefront/products?search=...`.

// [SCREENSHOT: storefront-catalog-grid.png] Catalogue browsing page: left sidebar showing the category tree with expandable nodes (Clothing > Dresses > Evening Dresses), centre content area displaying a 4-column product grid with thumbnail/name/price cards, pagination controls at the bottom, and a search bar in the top toolbar.

The product detail page displays the complete product information. A gallery of variant images provides thumbnail navigation. The selected variant's size and colour pickers show real-time stock availability indicators ("In Stock" / "Only 2 left" / "Out of Stock").

The current price is displayed alongside any promotional discount shown as a strikethrough original price. A quantity selector and "Add to Cart" button complete the purchase actions.

Expandable sections provide product description, material composition, care instructions, and size guide. A "Similar Products" section at the bottom displays visually similar items retrieved from `GET /api/storefront/products/{id}/similar`.

// [SCREENSHOT: storefront-product-detail.png] Product detail page: left side showing an image gallery with 4 thumbnail navigation dots and the main image, right side showing product name, price (750,000 VND), colour swatches (Black, Navy, Burgundy), size selector (S/M/L/XL) with stock badges, quantity input with +/- controls, "Add to Cart" button, and expandable sections below for Description, Material, Care Instructions, and Size Guide. A "Similar Products" horizontal scroll row is at the page bottom.

#line(length: 100%, stroke: 0.3pt + luma(200))

===== Shopping Cart: UC-STR-CRT

The cart page lists line items in a scrollable list, each showing the product thumbnail, title, selected size and colour, unit price, a quantity control with increment/decrement buttons and a current-quantity display (minimum 1, maximum available stock), a per-line subtotal, and a remove button (trash icon).

A sticky summary panel on the right displays the item count, subtotal, and a "Proceed to Checkout" button. An empty-cart state is shown when no items are present, with a "Continue Shopping" link to the catalogue.

Cart data is persisted via the backend API (`GET/POST/PUT/DELETE /api/storefront/cart`) and synchronised across browser tabs through Pinia reactive state. Guest users' carts are identified by a signed session cookie; upon authentication, the guest cart is automatically merged into the customer's existing cart via the `/api/storefront/cart/associate` endpoint.

// [SCREENSHOT: storefront-cart.png] Cart page with: left panel showing 3 line items, each with thumbnail (80x80px), "Floral Summer Midi Dress" title, "Size: M / Colour: Navy" variant info, unit price (750,000 VND), quantity selector [- 2 +], line subtotal (1,500,000 VND), and trash icon. Right sticky panel showing "Order Summary" with item count (3 items), subtotal (2,850,000 VND), and prominent "Proceed to Checkout" button.

// [SCREENSHOT: storefront-cart-empty.png] Empty cart state: a shopping bag icon with the message "Your cart is empty" and a "Continue Shopping" button linking to the catalogue.

#line(length: 100%, stroke: 0.3pt + luma(200))

===== Checkout: UC-STR-CHK

Checkout progresses through the five-state pipeline (Address, Delivery, Payment, Confirm, Complete) defined in the order state machine (Section 2.2.3). Each step is rendered as a single-page form with a visual progress indicator at the top showing all five stages, with the current stage highlighted and completed stages marked with a checkmark.

- *Address step.* Displays the customer's saved addresses from the profile with radio-button selection and an inline "Add new address" form. The selected address is validated server-side before proceeding.
- *Delivery step.* Lists available shipping methods with carrier name, estimated delivery time, and calculated rate. Rates are computed by the backend based on address zone, cart weight, and order value.
- *Payment step.* Presents available payment methods with provider icons. The selected method is confirmed via `POST /api/storefront/payment/create-intent`.
- *Confirm step.* Displays a read-only summary of the order: line items, shipping address, delivery method, payment method, and totals (item subtotal, shipping cost, grand total). A "Place Order" button finalises the checkout.
- *Complete step.* Shows an order confirmation with the generated order number, a summary, and a "Continue Shopping" link.

Each transition is validated server-side; attempts to skip steps or submit stale data are rejected.

// [SCREENSHOT: storefront-checkout-address.png] Checkout page: Address step: progress indicator showing [Address (current, highlighted)] → [Delivery] → [Payment] → [Confirm] → [Complete]. Below: list of saved addresses with radio buttons, an "Add New Address" collapsible form, and a "Continue to Delivery" button at the bottom.

// [SCREENSHOT: storefront-checkout-delivery.png] Checkout page: Delivery step: progress indicator with Address marked complete (checkmark), Delivery highlighted. List of 3 shipping methods (Standard, Express, Next-Day) with radio buttons, carrier names, estimated delivery dates, and calculated rates. "Continue to Payment" button.

// [SCREENSHOT: storefront-checkout-payment.png] Checkout page: Payment step: progress indicator with Address and Delivery marked complete, Payment highlighted. List of payment methods (Stripe Card Payment, Cash on Delivery, Bank Transfer) with provider icons, descriptions, and radio buttons. "Continue to Confirm" button.

// [SCREENSHOT: storefront-checkout-confirm.png] Checkout page: Confirm step: progress indicator with Address/Delivery/Payment marked complete, Confirm highlighted. Read-only order summary showing all line items, selected address, delivery method, payment method, and totals table (Item Total, Shipping, Grand Total). "Place Order" button prominently at the bottom.

// [SCREENSHOT: storefront-checkout-complete.png] Checkout page: Complete step: progress indicator with all five stages marked complete. Large success icon, "Order #ORD-2025-0042 Confirmed" heading, order summary, and "Continue Shopping" button.

#line(length: 100%, stroke: 0.3pt + luma(200))

===== Order History: UC-STR-OHI

The order history page lists all past orders in a vertically stacked card layout, with each card showing the order number, date, current status (with colour-coded badge), item count, and total amount. Expanding a card reveals line items with thumbnails, quantities, and prices, plus shipping and payment status detail.

Active orders show a "Cancel Order" button when the order is in a cancellable state. A "View Details" link navigates to the full order detail page with a timeline of checkout state transitions.

// [SCREENSHOT: storefront-order-history.png] Order history page: vertically stacked order cards (3 visible), each showing order number (ORD-2025-0042), date (Dec 15, 2025), status badge (Shipped in green), item count (3 items), and total (2,850,000 VND). The top card is expanded showing line items with thumbnails and prices.

// [SCREENSHOT: storefront-order-detail.png] Order detail page: order number and status at top, timeline showing each checkout state transition with timestamp (Address → Delivery → Payment → Confirm → Complete), line items table, shipping address block, payment method block, and totals table.

#line(length: 100%, stroke: 0.3pt + luma(200))

===== Authentication: UC-STR-AUT, UC-STR-SES

Authentication supports three pathways: email/password login, Google OAuth 2.0, and guest sessions.

- *Registration.* A form with email, password (with strength indicator), confirm password, and full name fields. On success, the user is redirected to the storefront with an authenticated session.
- *Login.* A form with email and password fields, a "Remember me" checkbox, and "Forgot password?" link. A "Sign in with Google" button triggers the OAuth flow.
- *Password reset.* A two-step flow: email entry form, then a new-password form accessed via a single-use token link sent to the registered email.
- *Session management.* A session page lists active sessions with device, IP address, and last-activity timestamp. A "Logout All Devices" button terminates all sessions.

Guest sessions use signed cookies to identify anonymous users for cart persistence. Upon authentication, the guest cart is merged into the customer's account without data loss.

// [SCREENSHOT: storefront-login.png] Login page: centered card with email input, password input with show/hide toggle, "Remember me" checkbox, "Sign In" button, "Forgot password?" link, horizontal "or" divider, and "Sign in with Google" button. Link to registration page at bottom.

// [SCREENSHOT: storefront-register.png] Registration page: centered card with full name, email, password (with strength bar showing "Strong"), confirm password fields, and "Create Account" button. Link to login page at bottom.

#line(length: 100%, stroke: 0.3pt + luma(200))

===== Payment Processing: UC-STR-PAY

During checkout, the payment step presents available methods retrieved from `GET /api/storefront/payment/methods`. Selecting a method and confirming triggers `POST /api/storefront/payment/create-intent` with the order amount and currency. For Stripe payments, the Stripe Elements embedded UI collects card details within an iframe without exposing sensitive data to the storefront's JavaScript context. The payment confirmation result updates the order's payment sub-state and advances to the Confirm step.

// [SCREENSHOT: storefront-payment-methods.png] Payment method selection page during checkout: list of 3 methods (Stripe Credit/Debit Card with card icon, Cash on Delivery with truck icon, Bank Transfer with bank icon), each with radio button and description. Stripe is selected (highlighted). "Continue" button below.

// [SCREENSHOT: storefront-payment-stripe.png] Stripe payment form: the Stripe Elements embedded card-number, expiry-date, and CVC fields inside the storefront's page layout with product order summary in the sidebar. "Pay 2,850,000 VND" confirmation button.

#line(length: 100%, stroke: 0.3pt + luma(200))

===== Profile Management: UC-STR-PRF

The profile section provides three management areas:

- *Address book.* A list of saved addresses with type labels (Home, Work), a default-address toggle, inline edit capability, and an "Add New Address" form. Addresses include recipient name, phone, street, ward, district, city, and country fields with cascading location selectors.
- *Wishlists.* Named collections of saved products. Each wishlist card shows the name, product count, and privacy setting. Expanding a wishlist shows product thumbnails with remove and add-to-cart actions.
- *Notification preferences.* Per-channel toggles (email, SMS) for order updates, promotions, and stock alerts.

// [SCREENSHOT: storefront-profile-addresses.png] Address book page: list of 2 saved addresses as expandable cards showing recipient name, phone, full address, and type label (Home badge in blue). Each card has Edit and Delete action icons. A "Add New Address" button at the top opens an inline form.

// [SCREENSHOT: storefront-profile-wishlists.png] Wishlists page: grid of wishlist cards ("Summer Collection": 12 items, "Work Attire": 5 items, "Gift Ideas": 3 items). Each card shows name, item count, privacy toggle (Public/Private), and a Delete icon.

// [SCREENSHOT: storefront-profile-notifications.png] Notification preferences page: toggles for Order Updates (Email ON, SMS OFF), Promotions (Email OFF, SMS OFF), Stock Alerts (Email ON, SMS ON) displayed in a two-column table layout.

#line(length: 100%, stroke: 0.3pt + luma(200))

==== Administration Interfaces

The administration dashboard implements the fifteen administrative use cases from the summary matrix. The interface uses PrimeVue's data-table components with server-side pagination, sorting, and filtering for all list views, and form dialogs with inline validation for create and edit operations.

===== Product Management: UC-ADM-PROD, UC-ADM-VAR, UC-ADM-IMG, UC-ADM-TAX, UC-ADM-OPT

The product management module is the most data-intensive admin area. It is organised into interlinked management surfaces:

*Product list (UC-ADM-PROD).* A paginated data table with columns for thumbnail, product name, SKU, category path (breadcrumb-style), status badge (Draft in grey, Active in green, Archived in red), base price (from master variant), and last modified date. The toolbar provides a keyword search input, a category filter dropdown, a status filter multi-select, and a "New Product" button.

Row actions include Edit, Activate/Discontinue, and Delete. Bulk actions support status changes and category assignment across selected rows.

// [SCREENSHOT: admin-product-list.png] Admin product listing: paginated data table with 25 rows. Columns: 60x60px thumbnail, Product Name (linked), SKU, Category (breadcrumb: Clothing > Dresses > Evening), Status badge (coloured pill), Price (VND), Last Modified (relative date). Toolbar above: search bar, category dropdown, status filter, "New Product" button. Pagination controls at bottom showing "Page 1 of 4 (100 products)".

*Product create/edit form (UC-ADM-PROD).* A multi-section form opened as a full-page view. The Basic Info section contains name, URL slug (auto-generated from name with manual override), description (rich text editor), and status dropdown.

The Fashion Metadata section contains style code, season (dropdown: Spring/Summer, Fall/Winter, All-Season), material composition (free text), department (dropdown: Women, Men, Kids, Unisex), and gender target (dropdown: Female, Male, Unisex).

The SEO section contains meta title and meta description fields. Form validation provides inline error messages below each field.

// [SCREENSHOT: admin-product-create.png] Admin product creation form: sections for Basic Info (name input "Floral Summer Midi Dress", auto-generated slug "floral-summer-midi-dress", description rich text area, status dropdown "Draft"), Fashion Metadata (style code "FSMD-2025", season "Spring/Summer", material "100% Cotton", department "Women", gender "Female"), and SEO (meta title, meta description textareas). "Save" and "Save & Publish" buttons at bottom.

*Variant management (UC-ADM-VAR).* Embedded within the product detail page, a table lists all variants with columns for SKU, barcode, master-variant radio selector, option combination (e.g., "M / Navy"), track-inventory toggle, price (current), and dimensions (weight, length, width, height).

Inline actions: Edit, Delete. An "Add Variant" button opens a modal dialog with option-value selectors, SKU auto-generation, pricing fields (current price, optional sale price with date range), and dimension fields.

// [SCREENSHOT: admin-product-variants.png] Product detail page: Variants tab: table with 5 variant rows. Columns: Master radio (one selected), SKU, Barcode, Options (e.g., "S / Black", "M / Navy"), Inventory toggle icons, Price (750,000 VND with optional sale price in red), Dimensions, Actions (Edit/Delete icons). "Add Variant" button above the table.

*Pricing management (UC-ADM-VAR).* Each variant supports time-bound pricing. A pricing tab within the variant edit dialog shows a table of price records: amount, currency, effective-from date, effective-to date (nullable for indefinite), and status (Active, Scheduled, Expired). "Add Price" opens inline fields for amount, currency, and date range.

// [SCREENSHOT: admin-variant-pricing.png] Variant pricing dialog: price history table with 3 entries (Current: 750,000 VND from Dec 1, Sale: 600,000 VND from Dec 20-31, Scheduled: 700,000 VND from Jan 15). "Add Price" button with inline amount/currency/date form.

*Image and embedding management (UC-ADM-IMG).* An image upload panel within the product detail page supports drag-and-drop and multi-file selection. Uploaded images appear in a sortable grid with drag handles for reordering, thumbnail previews, alt-text input, and a delete action.

Each image row shows an embedding status indicator: a green checkmark with "Embedded (Fashion-CLIP)" label for processed images, a yellow spinner for pending generation, and a red exclamation with "Regenerate" button for failed or missing embeddings.

A "Regenerate All Embeddings" button triggers re-embedding with the currently active model.

// [SCREENSHOT: admin-product-images.png] Product detail page: Images tab: sortable grid of 6 image cards, each with thumbnail (200x200px), drag handle, alt-text input ("Front view: Navy variant"), embedding status badge (green checkmark + "Fashion-CLIP"), and delete icon. Upload zone at top with drag-drop support. "Regenerate All Embeddings" button with confirmation dialog.

*Taxonomy management (UC-ADM-TAX, UC-ADM-OPT).* A tree editor for managing hierarchical category structures. The left panel shows the taxonomy tree with expand/collapse, drag-and-drop reordering, and context-menu actions (Add child, Edit, Delete). The right panel shows the selected taxon's details: name, slug, description, parent taxon, and associated business rules.

Taxons are assigned to products through a multi-select tree picker in the product edit form.

// [SCREENSHOT: admin-taxonomy-tree.png] Taxonomy management page: left panel showing tree (Clothing > Dresses > Evening Dresses, Casual Dresses; Clothing > Tops > T-Shirts, Blouses). Right panel showing "Evening Dresses" taxon details: name, slug, description, parent "Dresses", associated products count (45). "Add Child Taxon" and "Edit" buttons.

// [SCREENSHOT: admin-option-types.png] Option types management page: table listing Size (S, M, L, XL), Colour (Black, White, Navy, Burgundy, Olive), Material (Cotton, Silk, Linen, Polyester). Each row shows the option type name, value count, and action buttons. Clicking a row expands to show the ordered values with drag handles.

#line(length: 100%, stroke: 0.3pt + luma(200))

===== Order Management: UC-ADM-ORD, UC-ADM-ORD-ITEMS

The order management grid provides a filterable, paginated view of all orders. Columns display order number, customer name (linked to user detail), checkout state (colour-coded badge: Pending in blue, Confirmed in purple, Completed in green, Cancelled in red), payment status, shipment status, total amount, and creation date. Filters include status multi-select, date range picker, payment method, and keyword search across order numbers and customer names.

// [SCREENSHOT: admin-orders-grid.png] Admin order management page: data table with columns Order Number, Customer, Checkout State (coloured badges), Payment Status, Shipment Status, Total (VND), Created (date). Filter bar above with status multi-select, date range, and search input. Summary bar showing counts: "All (150) | Pending (12) | Confirmed (45) | Shipped (78) | Cancelled (15)".

Clicking an order opens the order detail page. The order header displays the order number, customer info, and current state, followed by a timeline of state transitions with timestamps.

A line items table shows product thumbnails, variant details (SKU, size, colour), quantity, unit price, and line total. Shipping and billing address blocks sit alongside a payment detail section with gateway transaction ID, payment state, and action buttons. Shipment tracking shows the carrier and tracking number.

State transition buttons (Approve, Complete, Cancel, Resume) appear conditionally based on the current checkout state per the order state machine.

// [SCREENSHOT: admin-order-detail.png] Admin order detail page: header with Order #ORD-2025-0042, customer name linked to profile, current state badge (Confirmed). Below: state timeline (Created → Address Set → Delivery Selected → Payment Confirmed). Line items table. Address blocks side-by-side (Shipping / Billing). Payment section with Stripe transaction ID, state (Succeeded), Capture/Refund buttons. Shipment section with tracking number. Action buttons: "Complete Order", "Cancel Order".

#line(length: 100%, stroke: 0.3pt + luma(200))

===== Payment Management: UC-ADM-PAY, UC-ADM-PAY-METHOD

The payment detail panel provides full visibility into payment intent lifecycles. It displays the payment intent ID, gateway provider (Stripe or Bogus), gateway transaction ID, amount, and currency.

The current state appears with a colour-coded badge and state transition timeline. A capture/refund/void action bar has buttons conditionally enabled based on state.

A payment log shows each gateway interaction with timestamp, and a webhook event log records Stripe-triggered state changes.

Payment method management (UC-ADM-PAY-METHOD) presents a table of configured gateways with provider name, display name, active toggle, and supported currencies. An "Add Method" dialog configures a new gateway with provider selection, display name, and supported-currency multi-select.

// [SCREENSHOT: admin-payment-detail.png] Payment detail page: header with Payment Intent ID, Stripe badge, amount (2,850,000 VND), state timeline (Created → Authorized → Captured). Action buttons: "Refund" (enabled), "Void" (disabled: already captured). Payment log table with gateway events and timestamps. Webhook event log below.

// [SCREENSHOT: admin-payment-methods.png] Payment methods management page: table with 4 configured methods (Stripe: active, Cash on Delivery: active, Bank Transfer: active, Bogus Test: inactive). Each row has provider icon, display name, active toggle, supported currencies, and Edit/Delete actions. "Add Payment Method" button.

#line(length: 100%, stroke: 0.3pt + luma(200))

===== Inventory Management: UC-ADM-STK, UC-ADM-LOC

The inventory panel provides real-time stock visibility and management across warehouse locations.

*Stock list.* A table grouped by product shows each variant with its SKU, size/colour, on-hand quantity, reserved quantity, available quantity (on-hand minus reserved, computed client-side), and a low-stock indicator (red badge when available drops below the configured threshold). Filtering by location, category, and low-stock-only is supported.

// [SCREENSHOT: admin-inventory-stock.png] Inventory stock list: table grouped by product, showing each variant row. Columns: Product Name, Variant SKU, Size/Colour, On Hand, Reserved, Available, Status. A product "Floral Summer Midi Dress" has 5 variant rows expanded. The "M / Navy" row shows On Hand: 3, Reserved: 2, Available: 1 with a red "Low Stock" badge. Filter bar above with location dropdown, category filter, and "Low Stock Only" toggle.

*Stock movements (UC-ADM-STK).* An append-only audit log table showing every stock change with columns for date/time (UTC), product/variant, movement type (Receiving, Selling, Returning, Stocktaking, Transferring), quantity delta (+/-), quantity before, quantity after, reason code, and operating user. The table is paginated and filterable by type, product, date range, and location.

// [SCREENSHOT: admin-inventory-movements.png] Stock movements audit log: paginated table with rows showing timestamp, product/variant identifier, movement type icon and label, +/- quantity, before/after quantities, reason, and user. Filter bar with type dropdown, product search, and date range picker.

*Stock locations (UC-ADM-LOC).* A management table listing warehouse locations with name, address, active toggle, and stock item count. "Add Location" and edit dialogs capture location name, address fields, and active status.

// [SCREENSHOT: admin-stock-locations.png] Stock locations management page: table with 3 locations (Main Warehouse: 1,250 items, Hanoi Hub: 340 items, HCMC Hub: 580 items). Each row shows name, address, active badge, item count, and Edit/Delete actions.

====== Restock and Transfer

- *Restock.* A form accepting product/variant selection, quantity, unit cost, and reason. Submitting creates a Receiving stock movement and increments the on-hand quantity.
- *Transfer.* A form with source location, destination location, product/variant, and quantity. Submitting creates a Transferring movement, decrements the source, and increments the destination. The transfer lifecycle progresses through Created, In-Transit, and Received states.

// [SCREENSHOT: admin-inventory-restock.png] Restock form dialog: Product/Variant search-and-select field, Quantity input (50), Unit Cost input (120,000 VND), Reason dropdown (New Shipment), and optional Notes textarea. "Confirm Restock" button.

// [SCREENSHOT: admin-inventory-transfer.png] Stock transfer form dialog: Source Location dropdown (Main Warehouse), Destination Location dropdown (HCMC Hub), Product/Variant selector, Quantity input (25), and Notes textarea. "Create Transfer" button.

#line(length: 100%, stroke: 0.3pt + luma(200))

===== User and Role Administration: UC-ADM-USR, UC-ADM-ROL

*User management (UC-ADM-USR).* A paginated table listing registered users with columns for avatar, full name, email, registration date, enabled/disabled status toggle, assigned roles (displayed as compact badges), and last login date. Filters include status (All, Enabled, Disabled), role, and keyword search.

The user create/edit form includes full name, email, password (create only), enabled toggle, and a role assignment multi-select.

// [SCREENSHOT: admin-user-list.png] User management page: data table with 50 users per page. Columns: avatar (32px circle with initials fallback), Name, Email, Registered (date), Status toggle (green/grey), Roles (badges: Admin in purple, Customer in blue, Manager in orange), Last Login (relative). Toolbar with search, status filter, role filter, and "Add User" button.

// [SCREENSHOT: admin-user-edit.png] User edit form dialog: Full Name input, Email input (disabled: read only), Enabled toggle, Roles multi-select with checkboxes (Admin, Manager, Support, Customer: Customer checked), "Save" and "Cancel" buttons.

*Role management (UC-ADM-ROL).* A table listing roles with role name, description, user count, and creation date. Expanding a role shows its permission assignments in an expandable tree grouped by domain (Catalog, Ordering, Payment, Inventory, Identity, Profile, Shipping, Location, Dashboard). Each permission is a `domain:category:action` claim with a checkbox toggle. The permissions catalogue is read from `GET /api/admin/identity/permissions`.

// [SCREENSHOT: admin-role-list.png] Role management page: table with 5 roles (Admin: 12 users, Manager: 5 users, Support: 8 users, Customer: 1,200 users, Guest). Each row shows name, description, user count badge, and Edit/Delete actions.

// [SCREENSHOT: admin-role-permissions.png] Role permission editor: left list of domains (Catalog expanded, others collapsed). Right panel showing Catalog permissions tree: Products (Create, Read, Update, Delete: all checked), Variants (Create, Read, Update, Delete: all checked), Images (Upload, Read, Delete: all checked), Taxonomies (Read, Update: checked; Create, Delete: unchecked). "Save Permissions" button.

#line(length: 100%, stroke: 0.3pt + luma(200))

===== Shipping Configuration: UC-ADM-SHP

The shipping management interface configures delivery methods and their associated rates and geographic zones.

*Shipping methods.* A table listing configured shipping methods (Standard Delivery, Express Delivery, Next-Day Delivery, International) with carrier name, estimated delivery time range, active toggle, and associated rates count. The create/edit dialog captures method name, carrier, description, delivery time estimate, and active status.

// [SCREENSHOT: admin-shipping-methods.png] Shipping methods management page: table with 4 methods, each showing name, carrier, estimated delivery (3-5 business days / 1-2 days / Next day / 7-14 days), active toggle, and rates count badge.

*Shipping rates.* Each method has a table of rates configured per geographic zone and weight/value tier. Rate rows show zone name, minimum/maximum weight, minimum/maximum order value, flat rate amount, and active status. The add-rate dialog provides zone dropdown, weight range, value range, and rate amount fields.

// [SCREENSHOT: admin-shipping-rates.png] Shipping rates page for Standard Delivery: table with 5 rate rows (Zone: Domestic: 0-2kg: 30,000 VND, 2-5kg: 50,000 VND; Zone: Southeast Asia: 0-2kg: 150,000 VND; etc.). Each row shows zone, weight range, value range, rate, and active toggle.

===== Reference Data: UC-ADM-REF

Country and state reference data management provides lookup tables used by addresses, shipping zones, and tax calculations. Countries are listed with ISO 3166-1 alpha-2 code, name, and active toggle. Selecting a country displays its associated states with ISO 3166-2 codes.

// [SCREENSHOT: admin-reference-data.png] Reference data page: left panel showing country list (Vietnam, USA, Japan, Korea, Singapore: all with ISO codes and active toggles). Right panel showing Vietnam's states/provinces: Ho Chi Minh City (SG), Hanoi (HN), Da Nang (DN), etc. with ISO 3166-2 codes.

#line(length: 100%, stroke: 0.3pt + luma(200))

===== System Processes: UC-SYS-EMB, UC-SYS-MNT

Background automation is monitored through the Hangfire dashboard, accessible at the `/hangfire` admin route.

The dashboard overview displays key metrics: total succeeded jobs, failed jobs (with red badge if non-zero), recurring job schedules, and current queue depths. The recurring jobs section lists each scheduled job with its cron expression or interval, last execution time and duration, next scheduled execution, and success/failure counts.

- *Cart expiry.* Runs every 20 minutes. Queries carts with no activity for 7 days, releases reserved inventory back to available stock, and deletes the cart record. The job log shows the count of expired carts per execution.
- *Embedding retries.* Processes failed embedding generation requests with exponential back-off (1 min, 2 min, 4 min, 8 min, max 3 retries). Permanently failed embeddings are flagged for manual review.
- *Index maintenance.* Runs nightly. Analyses the HNSW index state (vector count, dead tuples, query performance statistics) and rebuilds or reindexes when thresholds are exceeded.
- *Reservation expiry.* Runs every 15 minutes. Releases inventory reservations that have exceeded the 15-minute hold timeout, returning the reserved quantity to available stock.
- *Payment webhook processing.* Triggered by incoming Stripe webhook events. Validates HMAC signatures, checks idempotency keys, updates local payment state, and triggers order state transitions.

// [SCREENSHOT: hangfire-dashboard-overview.png] Hangfire dashboard overview: top metrics bar showing Succeeded (12,450), Failed (3: red badge), Recurring Jobs (5), Queues (3). Below: Recurring Jobs table with 5 rows (Cart Expiry, Embedding Retries, Index Maintenance, Reservation Expiry, Webhook Processing) showing cron/interval, last execution (2 min ago), next execution, and success count.

// [SCREENSHOT: hangfire-job-detail.png] Hangfire job detail page: showing the Cart Expiry recurring job. Job history table with columns for Job ID, Created, State (Succeeded/Failed), Duration. A "Last 100 executions" chart showing per-execution duration in milliseconds. Failed job entry highlighted with red row and "Retry" button.

// [SCREENSHOT: hangfire-queues.png] Hangfire queues page: 3 queue panels (default, embedding, maintenance) each showing enqueued, processing, and scheduled job counts. The embedding queue shows 2 enqueued (failed retries), 0 processing. The default queue shows 0 enqueued, 0 processing.
