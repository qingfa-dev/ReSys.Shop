===== 4. Cart & Checkout Orchestration
*Guest Cart Persistence:*
To minimize friction, anonymous users can build a cart. Upon authentication, a *Merge Strategy* unifies the local session with the user's persistent database record.

```typescript
// Cart Merge Flow (Post-Login)
FUNCTION OnLoginSuccess:
    Check LocalStorage for temporary items
    IF items exist:
        CALL API "Merge Cart" Endpoint
        AWAIT success response
        REFRESH global cart state from Server
    ELSE:
        FETCH user's existing cart
```

*Checkout State Machine:*
The checkout process is managed by a *Strict State Machine* that enforces a linear progression, preventing invalid states (e.g., paying before shipping is selected).

```typescript
// Checkout Stepper Logic
STATE currentStep = Address;

FUNCTION NextStep:
    SWITCH currentStep:
        CASE Address:
            IF no address selected: THROW Validation Error
            TRANSITION to Shipping
        CASE Shipping:
            IF no method selected: THROW Validation Error
            TRANSITION to Payment
```

The cart management logic follows an asynchronous reconciliation pattern. As shown in Figure 5, item additions are reflected immediately in the local Pinia store for responsiveness, while a background synchronization process ensures the server-side session remains accurate, especially when transitioning from guest to authenticated status.

// #figure(
//   image("/images/diagrams/flow/checkout-state.png", width: 100%),
//   caption: [Checkout State Machine: Enforcing linear state transitions and data validation gates throughout the purchase funnel.],
// )

*Synchronization Strategy:*
- *UI Optimism:* The Pinia Store acts as the "Single Source of Truth" for the DOM. When a user clicks "Add," the item appears instantly (0ms latency). The network request happens in the background.
- *Sequence Flow:* @fig:sq-0005-cart illustrates the eventual consistency. If the backend API validates and accepts the `AddToCartCommand`, the local state is confirmed. If it rejects (e.g., *Out of Stock* race condition), the UI silently "rolls back" the item and displays a Toast notification, maintaining data integrity.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/customer/sq-0005-cart.png", width: 100%),
  caption: [Shopping Cart Management Sequence: Client-side optimistic updates synchronized with server-side session state.],
) <fig:sq-0005-cart>
