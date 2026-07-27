=== Functional Validation Scenarios

This section details the "Happy Flow" validation steps for the core user journeys, verifying that the implementation meets the functional requirements defined in Chapter 2.

==== Customer Storefront (Frontend)

// TC-001: Browse & Filter
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-001*],
    [*Feature*], [*Catalog Browsing & Filtering*],
    [*Source Context*], [`src/apps/ReSys.Shop/features/catalog`],
    [*Objective*], [Verify that users can filter products and load subsequent pages via Infinite Scroll.],
    [*Preconditions*], [Catalog contains > 50 items. User is on Homepage.],

    [*Step*], [*Action*],
    [1], [Click "Catalog" in navigation menu.],
    [2], [Select Category "Electronics" from Sidebar.],
    [3], [Scroll to the bottom of the viewport.],

    [*Expected Result*],
    [
      - URL updates to `/catalog/electronics`.
      - Product grid renders first 20 items.
      - "Loading" spinner appears briefly.
      - Next 20 items are appended (Total: 40).
    ],

    [*Actual Result*], [Pagination triggered at 90% scroll depth. 0.4s latency. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-001: Browse Product Happy Flow],
  kind: table,
)

// TC-003: Search
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-003*],
    [*Feature*], [*Full-Text Search with Debounce*],
    [*Source Context*], [`src/apps/ReSys.Shop/features/discovery`],
    [*Objective*], [Verify input debounce and relevance of text search results.],
    [*Preconditions*], [Index is populated. Service is reachable.],

    [*Step*], [*Action*],
    [1], [Type "blue smar" into Search Bar.],
    [2], [Pause typing for > 300ms.],
    [3], [Click on the first result.],

    [*Expected Result*],
    [
      - No API call during typing.
      - API call triggers only after pause.
      - Results include "Blue Smartphone".
      - Directs to PDP of selected item.
    ],

    [*Actual Result*], [Debounce confirmed (300ms). Accuracy 100%. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-003: Keyword Search Happy Flow],
  kind: table,
)

// TC-004: Visual Search
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-004*],
    [*Feature*], [*Visual Search (Drag-and-Drop)*],
    [*Source Context*], [`src/services/ReSys.ML`],
    [*Objective*], [Verify image upload pipeline and vector similarity Retrieval.],
    [*Preconditions*], [ML Service is running (Fashion-CLIP).],

    [*Step*], [*Action*],
    [1], [Drag `red_dress.jpg` into Drop Zone.],
    [2], [Wait for inference indicators.],

    [*Expected Result*],
    [
      - Optimistic Preview renders immediately.
      - Skeleton Loader displays during inference.
      - Results grid populates with visually similar items (Red Dresses).
    ],

    [*Actual Result*], [Client validation OK. Inference < 800ms. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-004: Visual Search Happy Flow],
  kind: table,
)

// TC-005: Shopping Cart
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-005*],
    [*Feature*], [*Cart Management (Optimistic UI)*],
    [*Source Context*], [`src/apps/ReSys.Shop/stores/cart`],
    [*Objective*], [Verify instant UI feedback and background synchronization.],
    [*Preconditions*], [Product ID `123` is in stock.],

    [*Step*], [*Action*],
    [1], [Click "Add to Cart" on PDP.],
    [2], [Navigate to Cart View.],

    [*Expected Result*],
    [
      - Cart Badge updates instantly (0ms latency).
      - "Item Added" toast appears.
      - Item is present in Cart View.
    ],

    [*Actual Result*], [UI updated before network ack. Sync successful. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-005: Add to Cart Happy Flow],
  kind: table,
)

// TC-002: Secure Checkout
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-002*],
    [*Feature*], [*Checkout Wizard Flow*],
    [*Source Context*], [`src/apps/ReSys.Shop/features/checkout`],
    [*Objective*], [Verify the multi-step transaction funnel (Ship $\to$ Pay $\to$ Order).],
    [*Preconditions*], [Cart has items. User is authenticated.],

    [*Step*], [*Action*],
    [1], [Enter Valid Address and Click "Next".],
    [2], [Select "Express Shipping" and Click "Next".],
    [3], [Enter Valid Card (Stripe Test) and Pay.],

    [*Expected Result*],
    [
      - Step 1: Transitions to Shipping Method.
      - Step 2: Transitions to Payment.
      - Step 3: Payment success webhook received.
      - Order Confirmation Page displayed.
    ],

    [*Actual Result*], [State machine enforced. Order created. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-002: Full Checkout Happy Flow],
  kind: table,
)

// TC-006: Order History (Retention)
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-006*],
    [*Feature*], [*Order Tracking Timeline*],
    [*Source Context*], [`src/apps/ReSys.Shop/features/account`],
    [*Objective*], [Verify parsing of domain events into a visual timeline.],
    [*Preconditions*], [Order `ORD-001` exists with multiple status events.],

    [*Step*], [*Action*],
    [1], [Go to "My Orders".],
    [2], [Select Order `ORD-001`.],

    [*Expected Result*],
    [
      - Timeline shows: Placed $\to$ Paid $\to$ Shipped.
      - Current status is highlighted.
      - Tracking number is visible.
    ],

    [*Actual Result*], [Event stream aggregated correctly. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-006: Order Tracking Happy Flow],
  kind: table,
)

// TC-008: Recommendations
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-008*],
    [*Feature*], [*Contextual Recommendations*],
    [*Source Context*], [`src/apps/ReSys.Shop/features/recommendations`],
    [*Objective*], [Verify that "Similar Products" are fetched based on Vector Proximity.],
    [*Preconditions*], [User viewing "Blue Denim Jacket" (PDP). ML Service Active.],

    [*Step*], [*Action*],
    [1], [Scroll down to "You Might Also Like".],
    [2], [Verify lazy loading trigger.],

    [*Expected Result*],
    [
      - IntersectionObserver triggers API call.
      - Carousel populates with ~5 items.
      - Items are visually/semantically similar (e.g., Jeans, Jackets).
    ],

    [*Actual Result*], [Vector neighbors fetched. Semantic match high. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-008: Recommendations Happy Flow],
  kind: table,
)

// TC-007: Address Book
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-007*],
    [*Feature*], [*Address Book Management*],
    [*Source Context*], [`src/apps/ReSys.Shop/features/account`],
    [*Objective*], [Verify CRUD operations and "Set Default" logic.],
    [*Preconditions*], [User logged in. Profile exists.],

    [*Step*], [*Action*],
    [1], [Add New Address "Home 2".],
    [2], [Set as "Default Shipping".],

    [*Expected Result*],
    [
      - New address appears in Grid.
      - "Default" badge moves to "Home 2".
      - Checkout now pre-selects "Home 2".
    ],

    [*Actual Result*], [Profile updated. Default flag persisted. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-007: Address Book Happy Flow],
  kind: table,
)

==== Admin Panel (Operations)

// TC-009: Catalog Management
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-009*],
    [*Feature*], [*Product Creation Transaction*],
    [*Source Context*], [`src/services/ReSys.Api/Features/Catalog`],
    [*Objective*], [Verify atomic creation of Product Aggregate (Root + Variants).],
    [*Preconditions*], [Admin logged in. "Create Product" form open.],

    [*Step*], [*Action*],
    [1], [Fill "Title", "Price" ($>0$), "Description".],
    [2], [Add Variant "Size: L", "Stock: 10".],
    [3], [Click "Save Product".],

    [*Expected Result*],
    [
      - Validation passes (Form turns valid).
      - "Saving..." spinner appears.
      - Success Toast: "Product Created".
      - Redirects to Product List.
    ],

    [*Actual Result*], [Transaction committed. Entity ID generated. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-009: Create Product Happy Flow],
  kind: table,
)

// TC-010: Bulk Image Upload
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-010*],
    [*Feature*], [*Parallel Image Uploads*],
    [*Source Context*], [`src/apps/ReSys.Admin/features/catalog`],
    [*Objective*], [Verify drag-and-drop batch upload and progress tracking.],
    [*Preconditions*], [Editing active Product. 5 Images ready.],

    [*Step*], [*Action*],
    [1], [Drag 5 images into Upload Zone.],
    [2], [Monitor progress bars.],

    [*Expected Result*],
    [
      - 5 concurrent requests initiate.
      - Progress bars update independently (0% $\to$ 100%).
      - All 5 images appear in gallery on completion.
    ],

    [*Actual Result*], [Parallel execution confirmed. All saved. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-010: Image Upload Happy Flow],
  kind: table,
)

// TC-011: Taxonomy
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-011*],
    [*Feature*], [*Taxonomy Tree Mutation*],
    [*Source Context*], [`src/apps/ReSys.Admin/features/taxonomy`],
    [*Objective*], [Verify "Reparenting" of categories via Drag-and-Drop.],
    [*Preconditions*], [Category "Laptops" is under to "Electronics".],

    [*Step*], [*Action*],
    [1], [Drag "Laptops" to "Computers".],
    [2], [Confirm Move.],

    [*Expected Result*],
    [
      - UI updates tree structure immediately.
      - Backend updates Materialized Path (`/Computers/Laptops`).
      - Products remain linked.
    ],

    [*Actual Result*], [Graph mutation successful. Path updated. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-011: Taxonomy Reorganization Happy Flow],
  kind: table,
)

// TC-012: Inventory Monitor
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-012*],
    [*Feature*], [*Real-Time Inventory Updates*],
    [*Source Context*], [`src/apps/ReSys.Admin/features/inventory`],
    [*Objective*], [Verify WebSocket updates on the Inventory Grid.],
    [*Preconditions*], [Admin viewing Inventory Grid. Customer (Simulated) buys Item A.],

    [*Step*], [*Action*],
    [1], [Observe Row for Item A.],
    [2], [Trigger `StockReservedEvent` (via Backend Test Tool).],

    [*Expected Result*],
    [
      - Row for Item A flashes (Highlighter).
      - "Available" count decreases by 1.
      - "Reserved" count increases by 1.
    ],

    [*Actual Result*], [Socket message received < 100ms. Grid updated. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-012: Inventory WebSocket Test],
  kind: table,
)

// TC-013: Analytics
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-013*],
    [*Feature*], [*Dashboard Aggregation*],
    [*Source Context*], [`src/apps/ReSys.Admin/features/dashboard`],
    [*Objective*], [Verify distinct widget loading from Read Replicas.],
    [*Preconditions*], [Database has 10k orders.],

    [*Step*], [*Action*],
    [1], [Load Dashboard Homepage.],

    [*Expected Result*],
    [
      - "Revenue" widget shows Skeleton $\to$ Value.
      - "Top Products" widget shows Skeleton $\to$ Chart.
      - Queries do NOT block main thread.
    ],

    [*Actual Result*], [Parallel fetch confirmed. Render time < 1s. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-013: Analytics Dashboard Happy Flow],
  kind: table,
)

// TC-014: Fulfillment
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-014*],
    [*Feature*], [*Ship Order Command*],
    [*Source Context*], [`src/services/ReSys.Api/Features/Ordering`],
    [*Objective*], [Verify state transition and inventory reconciliation.],
    [*Preconditions*], [Order in `Paid` state. Stock is `Reserved`.],

    [*Step*], [*Action*],
    [1], [Open Order Details.],
    [2], [Enter "Tracking Number" and click "Ship".],

    [*Expected Result*],
    [
      - Status changes to `Shipped`.
      - "Ship" button becomes disabled.
      - Inventory `Reserved` count decreases (Deducted).
    ],

    [*Actual Result*], [State transition persisted. Stock reconciled. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-014: Ship Order Happy Flow],
  kind: table,
)

// TC-015: Identity Governance
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-015*],
    [*Feature*], [*Role Promotion & Security Side-Effect*],
    [*Source Context*], [`src/services/ReSys.Identity`],
    [*Objective*], [Verify that changing a role invalidates the user's session.],
    [*Preconditions*], [Target User A is "Customer". Admin is SuperAdmin.],

    [*Step*], [*Action*],
    [1], [Change User A Role to "Manager".],
    [2], [Confirm Modal.],
    [3], [Attempt API call as User A (using old Token).],

    [*Expected Result*],
    [
      - Database Role updated.
      - User A's `RefreshToken` revoked.
      - Step 3 returns `401 Unauthorized`.
    ],

    [*Actual Result*], [Token revocation successful. Access denied. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-015: RBAC Promotion Happy Flow],
  kind: table,
)

==== Backend & ML Services

// TC-016: ML Vectorization
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-016*],
    [*Feature*], [*Async Embedding Generation*],
    [*Source Context*], [`src/services/ReSys.ML`],
    [*Objective*], [Verify GPU offloading and callback mechanism.],
    [*Preconditions*], [Valid Image URL in `ProductImage`.],

    [*Step*], [*Action*],
    [1], [Publish `ProductImageCreated` event.],
    [2], [Monitor `ImageEmbedding` table.],

    [*Expected Result*],
    [
      - ML Service receives job.
      - Core API receives Callback (`POST`).
      - Table populated with 512-dim vector.
    ],

    [*Actual Result*], [Callback received. Vector stored. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-016: Vector Generation Integration Test],
  kind: table,
)

// TC-019: System Automation
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-019*],
    [*Feature*], [*Low Stock Alert Generation*],
    [*Source Context*], [`src/services/ReSys.worker`],
    [*Objective*], [Verify Materialized View refresh for alerts.],
    [*Preconditions*], [Item B stock set to 2 (Threshold = 5).],

    [*Step*], [*Action*],
    [1], [Trigger `RefreshLowStockJob` (Manual or Schedule).],
    [2], [Check `LowStockSnapshot` table.],

    [*Expected Result*],
    [
      - Job executes SQL Aggregation.
      - Item B appears in Snapshot.
      - Admin widget displays "+1 Alert".
    ],

    [*Actual Result*], [Snapshot updated. Alert visible. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-019: Background Job Happy Flow],
  kind: table,
)

// TC-017: Atomic Reservations
#figure(
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Test Case ID*], [*TC-017*],
    [*Feature*], [*Concurrent Stock Reservation*],
    [*Source Context*], [`src/services/ReSys.Api/Features/Inventory`],
    [*Objective*], [Verify Serializable transaction isolation prevents overselling.],
    [*Preconditions*], [Item Stock = 1.],

    [*Step*], [*Action*],
    [1], [Simulate 2 concurrent `ReserveStock(1)` requests.],

    [*Expected Result*],
    [
      - Transaction A succeeds.
      - Transaction B fails (DB Lock Wait or Conflict).
      - Final Stock = 0 (Not -1).
    ],

    [*Actual Result*], [Race condition prevented. Data consistent. $\to$ PASS],
    [*Status*], [*PASS*],
  ),
  caption: [TC-017: Concurrency Control Stress Test],
  kind: table,
)
