===== Catalogue Browsing: UC-STR-BRW

The storefront home page presents the brand hero, curated collections, and featured product carousel. The catalogue page presents products in a paginated, faceted grid with left-sidebar category tree navigation built from hierarchical taxonomy data. Selecting a category filters the grid; a keyword search bar sends full-text queries. Product cards show thumbnails, names, prices, and a hover overlay with quick-add-to-cart (see screenshots below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-home.png", width: 100%),
  caption: [Storefront home: hero banner, curated collection cards, and featured product carousel.],
) <fig-storefront-home>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-catalog-grid.png", width: 100%),
  caption: [Catalogue browsing: left sidebar with expandable category tree, centre 4-column product grid with thumbnail/name/price cards, pagination controls, and search bar.],
) <fig-storefront-catalog>

The product detail page displays complete product information with a variant image gallery, size and colour pickers with real-time stock indicators, current price with strikethrough original price, quantity selector, and Add to Cart button. Expandable sections provide description, material, care instructions, and size guide. A Similar Products carousel appears at bottom (see screenshot below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-product-detail.png", width: 100%),
  caption: [Product detail: left gallery with thumbnail dots and main image; right panel with product name, price, colour swatches, size selector with stock badges, quantity input, Add to Cart button. Expandable sections and Similar Products carousel below.],
) <fig-storefront-product-detail>

===== Shopping Cart: UC-STR-CRT

The cart page lists line items with thumbnail, title, variant details, unit price, quantity controls, per-line subtotal, and remove button. A sticky summary panel shows item count, subtotal, and Proceed to Checkout button. Cart data persists via backend API and synchronises across tabs through Pinia state. Guest carts merge into authenticated accounts on login (see screenshots below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-cart.png", width: 100%),
  caption: [Cart with line items: thumbnails, product names with variant info, unit prices, quantity selectors, line subtotals, trash icons. Right sticky panel: Order Summary with item count, subtotal, Proceed to Checkout button.],
) <fig-storefront-cart>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-cart-empty.png", width: 100%),
  caption: [Empty cart: shopping bag icon, "Your cart is empty" message, "Continue Shopping" button linking to catalogue.],
) <fig-storefront-cart-empty>
