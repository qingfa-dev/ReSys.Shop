===== 2. Decision & Cart Management
Instead of navigating away to a separate page, the application uses a *Slide-Over Drawer* mechanism. This allows users to view and manage their basket contents while maintaining visual context of their current shopping journey, reducing abandonment rates.

// TODO: [Implementation] Add Cart Drawer screenshot.
// #figure(
//   figure-placeholder("Cart Drawer UI"),
//   caption: [Slide-over Cart Drawer allowing "Add to Cart" interactions without page navigation.],
// )

To maintain the user's immersion in the browsing experience, the application adopts a "Slide-Over" interaction pattern. As illustrated in the *Cart Management Sequence* (see `sq-0005-cart` in *Cart Orchestration*), this UI component acts as the visual terminal for the background synchronization logic:

- *State 1: Context Preservation:* The drawer overlays the current catalog view, preventing the "Context Loss" associated with full-page redirects.
- *State 2: Interaction Zones:*
  - *Header:* Immediate status feedback (item count).
  - *Body:* Reactive `CartItem` components. Key user actions (Increment/Remove) trigger optimistic updates to the local Pinia store before the API request completes (@fig:sq-0005-cart), ensuring a perceived zero-latency experience.
  - *Footer:* A sticky "Proceed to Checkout" action ensures the conversion path is never obstructed by long item lists.

