# Store SPA Rebuild — Cycle 3b: Ordering

Date: 2026-08-08
Scope: Ordering domain — Cart, Checkout, Order List, Order Detail
Tier: 3b of 3 (Identity → Ordering → Profile)

## Visual Direction (inherited from Cycle 1)

Minimal clean e-commerce. Neutral palette, white cards, subtle borders.
CartView + CheckoutView use DefaultLayout. OrderListView + OrderDetailView use AccountLayout.

## Views

### 1. CartView — Full-page Shopping Cart

**Layout:** DefaultLayout. Two-column on desktop — line items (flex-1) + Order Summary sidebar (w-80, sticky top-24).

**Line items:**
- Product image (w-20 h-24), name, SKU + selected option values, quantity stepper [−] N [+], line total, remove button (pi-times, text, danger)
- Each item is a Card row with image + info + price
- Quantity stepper calls cartStore.updateQuantity(lineItemId, qty)
- Remove button calls cartStore.removeItem(lineItemId)
- "Clear Cart" link at bottom (text-xs, danger)

**Order summary sidebar:**
- Subtotal (from cartStore.subtotal)
- Shipping ("Calculated at checkout")
- Tax ("Calculated at checkout")
- Divider
- Total (= subtotal)
- "Checkout" Button (teal, full-width, router-link to /checkout)
- "Continue Shopping" link below (router-link to /shop)

**States:**
- Loading: 3 Skeleton rows + skeleton sidebar
- Empty: Shopping cart icon, "Your cart is empty", "Continue Shopping" Button
- Error: Error message with retry
- Populated: Line items list + summary sidebar

**Data:** cartStore (items, loading, error, isEmpty, subtotal, itemCount, updateQuantity, removeItem, clearCart, fetchCart)

### 2. CheckoutView — 5-Step Wizard

**Layout:** DefaultLayout. Two-column — stepper + form (flex-1) + Order Summary sidebar (w-80).

**Stepper:** Custom stepper bar at top showing 5 steps (Address → Delivery → Payment → Confirm → Complete). Active step highlighted (teal bg + white text). Completed steps show checkmark. Uses checkoutStore.steps computed array.

**Step 1 — Address:**
- Radio list of saved addresses from addressStore (if loaded)
- "Add New Address" button (opens modal or expands inline form)
- Email InputText field
- "Continue to Delivery" Button → calls checkoutStore.saveAddress(id, email)

**Step 2 — Delivery:**
- Radio list of shipping methods from shippingStore (Standard $5.99, Express $14.99)
- "Continue to Payment" Button → calls checkoutStore.selectShippingRate(methodId)

**Step 3 — Payment:**
- Stripe Card Element mounted via useStripe/usePayment composable (already built)
- "Pay $XX.XX" Button → creates payment intent, confirms card payment
- Order summary sidebar shows line items

**Step 4 — Confirm:**
- Read-only summary: address, delivery method, payment method (card last 4)
- "Place Order" Button → finalizes order

**Step 5 — Complete:**
- Success icon (pi-check-circle, green, text-4xl)
- "Order #XXXXX confirmed!"
- Order number + email confirmation notice
- "View Order" → /account/orders/:id
- "Continue Shopping" → /shop

**Navigation:** "← Back" button on each step to go to previous step. Not browser-back — uses checkoutStore stepper.

**Data:** checkoutStore (steps, currentStep, saveAddress, selectShippingRate, createPaymentIntent, placeOrder), addressStore, shippingStore, cartStore, useStripe composable

### 3. OrderListView — Order History

**Layout:** AccountLayout (sidebar active on Orders). Page title "Your Orders".

**Order cards:**
- Each order is a Card with: order number (#12345), placed date, status Tag, item count, total
- Click navigates to /account/orders/:id
- Status colors: Processing (info), Shipped (warn), Delivered (success), Canceled (danger)
- PrimeVue Tag for status badge

**Pagination:**
- PrimeVue Paginator below list
- Hidden when totalPages ≤ 1
- Uses orderStore page, totalPages, goToPage()

**States:**
- Loading: 3 Skeleton cards
- Empty: "No orders yet" + "Start Shopping" Button
- Error: Error message with retry
- Populated: Order cards + Paginator

**Data:** orderStore (items, loading, error, page, totalPages, totalCount, fetchOrders, nextPage, prevPage, goToPage)

### 4. OrderDetailView — Single Order

**Layout:** AccountLayout (sidebar). Back link "← Back to Orders" at top.

**Content sections:**
- Order header: #12345, placed date, status Tag
- Items: Line items with image (w-16 h-20), name, SKU, option values, quantity, line total
- Shipping: Name, full address, shipping method, tracking number (if available) + "Track" link
- Summary: Subtotal, Shipping, Tax, Total in a 2-column table
- Cancel button (if status = "Processing"): danger outlined button with ConfirmDialog

**States:** Loading (Skeleton), error, not found, loaded.

**Data:** orderStore (currentOrder, detailLoading, fetchOrder, cancelOrder)
