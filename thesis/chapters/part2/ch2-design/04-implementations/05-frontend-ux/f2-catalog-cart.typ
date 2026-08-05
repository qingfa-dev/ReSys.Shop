===== Catalogue Browsing: UC-STR-BRW

The catalogue page presents products in a paginated, faceted grid with left-sidebar category tree navigation built from hierarchical taxonomy data. Selecting a category filters the grid; a keyword search bar sends full-text queries. Product cards show thumbnails, names, prices, and a hover overlay with quick-add-to-cart (see screenshot below).

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-catalog-grid.png", width: 100%),
//   caption: [Catalogue browsing: left sidebar with expandable category tree (Clothing > Dresses > Evening Dresses), centre 4-column product grid with thumbnail/name/price cards, pagination controls, and search bar.],
// ) <fig-storefront-catalog>

The product detail page displays complete product information with a variant image gallery, size and colour pickers with real-time stock indicators, current price with strikethrough original price, quantity selector, and Add to Cart button. Expandable sections provide description, material, care instructions, and size guide. A Similar Products carousel appears at bottom (see screenshot below).

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-product-detail.png", width: 100%),
//   caption: [Product detail: left gallery with 4 thumbnail dots and main image; right panel with product name, price, colour swatches (Black, Navy, Burgundy), size selector (S/M/L/XL with stock badges), quantity input, Add to Cart button. Expandable sections and Similar Products carousel below.],
// ) <fig-storefront-product-detail>

===== Shopping Cart: UC-STR-CRT

The cart page lists line items with thumbnail, title, variant details, unit price, quantity controls, per-line subtotal, and remove button. A sticky summary panel shows item count, subtotal, and Proceed to Checkout button. Cart data persists via backend API and synchronises across tabs through Pinia state. Guest carts merge into authenticated accounts on login (see screenshot below).

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-cart.png", width: 100%),
//   caption: [Cart with 3 line items: thumbnails (80x80px), product names with variant info ("Size: M / Colour: Navy"), unit prices, quantity selectors, line subtotals, trash icons. Right sticky panel: Order Summary with item count, subtotal (2,850,000 VND), Proceed to Checkout button.],
// ) <fig-storefront-cart>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-cart-empty.png", width: 100%),
//   caption: [Empty cart: shopping bag icon, "Your cart is empty" message, "Continue Shopping" button linking to catalogue.],
// ) <fig-storefront-cart-empty>
