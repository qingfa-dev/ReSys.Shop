# Store SPA Rebuild — Cycle 2: Catalog Views

Date: 2026-08-08
Scope: All catalog views and components for the Store SPA
Tier: 2 of 3 (Layouts → Catalog → Identity/Ordering/Profile)

## Visual Direction (inherited from Cycle 1)

Minimal clean e-commerce. Neutral palette, generous white space, subtle borders.
Reference aesthetic: Everlane, Aesop, Muji.

## Brand Tokens (from Cycle 1)

| Token | Value | Usage |
|-------|-------|-------|
| Page bg | `neutral-50` | All page backgrounds |
| Surface | `white` | Cards, header, footer |
| Border | `neutral-200` | Dividers, card borders |
| Text primary | `neutral-900` | Headings |
| Text secondary | `neutral-500` | Metadata, hints |
| Accent | `#0d7377` | CTAs, links, selected states |
| Body font | Inter 400/500/600 | All UI text |
| Editorial font | Newsreader italic | Hero headline only |
| Price font | JetBrains Mono 500 | Currency/pricing |

## Views & Components

### 1. HomeView — Editorial Hero + Discovery

**Hero section:**
- Full-width, min-h-[60vh], bg-neutral-100 with subtle gradient
- Headline: Newsreader italic, text-4xl, text-neutral-900, centered
- Subtext: Inter, text-neutral-500, max-w-xl, centered
- CTA: teal Button "Shop New Arrivals", router-link to /shop
- Placeholder: no image — gradient-only background (future: fashion photography)

**Featured Categories:**
- Section eyebrow: "Shop by Category" — text-sm font-medium text-neutral-500 uppercase tracking-wide
- 4-col grid, cards: aspect-[3/4], rounded-lg, bg-neutral-100 placeholder
- Card content: category name (text-lg font-semibold) + item count (text-sm text-neutral-500)
- Cards are router-link to /shop?category=:id
- Hover: opacity-90 transition

**Featured Products:**
- Section eyebrow: "New Arrivals"
- 4-col product grid using shared ProductCard component
- "View All →" link at bottom right, router-link to /shop
- Data: productListStore with sortField=-createdAtUtc, pageSize=8

**Bottom CTA:**
- Full-width strip, py-16, bg-neutral-100
- "Join the waitlist for exclusive drops" + email input + Subscribe button
- Non-functional placeholder (no newsletter API yet)

**Data sources:** catalogStore (taxonomy), productListStore (featured products)

### 2. ShopView — Sidebar Filters + Product Grid

**Layout:** 2-column flex — sidebar (w-64, sticky top-20) + content area (flex-1)

**Sidebar:**
- **Taxonomy tree:** Indented, collapsible groups (chevron toggle). Clicking a taxon toggles it in `catalogStore.selectedTaxonIds`. Active items bold + checkbox.
- **Price range:** Min/max InputText inputs + native range slider. Updates `catalogStore.minPrice`/`maxPrice`.
- **Option values:** Checkbox list grouped by option type name. Each option shows option value name + product count badge. Uses `catalogStore.optionTypes[]`.
- **Clear All:** Appears when `activeFilterCount > 0`. Calls `catalogStore.clearFilters()`.
- **Mobile:** All filters in a slide-out panel triggered by "Filters" button in sort bar.

**Sort bar:**
- Left: dropdown (PrimeVue Select) with "Newest", "Price: Low to High", "Price: High to Low", "Name A-Z"
- Right: "Showing 1–20 of N" results count text
- Mobile: filter toggle button on left
- Updates `catalogStore.sortField`

**Product grid:**
- 3 columns default, 4 columns xl
- Shared ProductCard component
- Loading state: 12 skeleton cards (3×4 grid)
- Empty state: "No products found matching your filters" with "Clear all filters" button
- Error state: error message with retry button

**Pagination:**
- PrimeVue Paginator below grid
- Hidden when totalPages ≤ 1
- Uses productListStore page, totalPages, goToPage()

**Data sources:** catalogStore (filters, sort, search), productListStore (products, pagination)

### 3. ProductDetailView — Purchase Decision

**Layout:** 2-column side-by-side (desktop), stacked (mobile)

**Gallery (left, w-7/12):**
- Main image: aspect-[3/4], bg-neutral-100, rounded-lg
- Thumbnail strip: vertical column (desktop) or horizontal row (mobile), w-20 h-24 each
- Active thumbnail: border-2 border-neutral-900 ring
- Click switches main image
- Works with single image — no thumbnails shown if only one

**Product info (right, w-5/12):**
- Brand name: text-xs text-neutral-500 uppercase tracking-wide
- Product name: text-2xl font-semibold text-neutral-900
- Price: text-xl font-medium (JetBrains Mono) text-neutral-900
- Short description: text-sm text-neutral-600, 2-3 lines
- **Variant selector:** Rendered dynamically per option type from `product.variants`:
  - Size/Material types: PrimeVue SelectButton group
  - Color types: Visual swatches (32px colored circles, border on selected)
  - Calls `detail.selectVariant(id)`, updates selectedVariant
- **Quantity:** Stepper [−] N [+] — disabled at 1, disabled at max stock
- **Add to Cart:** Full-width teal Button, label "Add to Cart — $XX.XX" (line total). Calls `detail.addToCart()`. On success: toast "Added to cart", CartDrawer badge increments.
- **Stock label:** `detail.stockLabel` — "Only X left" (amber), "Available for backorder" (neutral), "Out of stock" (red — button disabled + "Notify Me" text)
- **Trust signals:** Three ✓ bullet points below ATC: "Free shipping over $100", "30-day returns", "Secure checkout"
- **Wishlist:** Outlined secondary Button "Add to Wishlist", icon pi pi-heart

**Tabs (full-width below):**
- PrimeVue Tabs: Description, Details, Shipping
- Description: v-html rendered rich text with Tailwind prose-like classes
- Details: Specification table (material, care, country of origin)
- Shipping: Static shipping policy text

**Visually Similar (below tabs):**
- Section: "Visually Similar" + subtext "AI-powered recommendations based on visual style"
- Horizontal scroll strip, 5 items visible with overflow scroll
- Left/right arrow buttons at scroll edges
- Product cards with similarity badge (top-right, bg-teal-500/80 text-white text-xs rounded)
- Data: detail.similarProducts (loaded concurrently with main product)

**You May Also Like (below visually similar):**
- Section: "You May Also Like" + subtext "Customers who viewed this also bought"
- 4-col grid of product cards (no scroll)
- Data: detail.relatedProducts (loaded concurrently with main product)

**States:**
- Loading: Skeleton product page (placeholder image + skeleton text lines)
- Error: Error message with retry
- Product not found: "Product not found" message with "Back to Shop" link

**Data sources:** productDetailStore (product, similar, related, variant selection, cart add)

### 4. SearchOverlay — Global Search

**Trigger:** Ctrl+K keyboard shortcut or header search icon click
**Close:** Esc key, backdrop click, or navigate to result

**Layout:**
- Teleport to body, fixed inset-0, z-50
- Backdrop: bg-black/50, click to close
- Search box: centered, pt-[20vh], max-w-2xl, bg-white rounded-xl, shadow-2xl
- Input: text-lg, autofocus, border-0, outline-none, full-width within box
- Hint: "Search products..." as placeholder

**Results area:**
- Live results after 300ms debounce on input
- Product name + thumbnail (w-10 h-10) per result row
- Keyboard: ↑↓ arrows to navigate results, Enter to select → navigate to /products/:slug
- "View all results for 'query'" link at bottom → navigates to /shop?q=query
- Empty state: "No products match 'query'"
- Initial state: "Type to search products, collections, and more..."

**States:**
- Closed: v-if=false, no DOM rendered
- Open-empty: input + initial hint
- Open-loading: skeleton rows
- Open-results: result rows + view all link
- Open-no-results: no-match message

**Data sources:** useSearch composable (open state), catalogStore.searchQuery + productListStore for results

### 5. CollectionsView — Category Grid

- Breadcrumb: Home > Collections
- Page title: "Collections"
- 3-col grid of category cards (aspect-[2/3], rounded-lg, bg-neutral-100 placeholder)
- Card: category name (text-lg font-semibold) + item count (text-sm text-neutral-500)
- Cards link to /shop?taxon=:id (routes to Shop filtered by category)
- Data: catalogStore.taxonomyGroups (top-level only, not nested)

### 6. VisualSearchView — Image-Based Product Search

**Upload state:**
- Drop zone: border-2 border-dashed border-neutral-300 rounded-xl, py-16, text-center
- Icon: cloud-upload (pi pi-cloud-upload, text-4xl, text-neutral-300)
- Text: "Upload an image" (text-lg font-medium) + "JPEG, PNG, WebP — Max 10 MB" (text-sm text-neutral-500)
- Click to open file picker, or drag and drop
- Validation: MIME type check, size check (<10MB) — error toast on invalid

**Preview state:**
- Selected image preview: w-40 h-40, object-cover, rounded-lg
- Model selector dropdown: lists available ML models from visualSearchStore.availableModels
- "Search" Button (teal), disabled when no image selected
- "Change image" link to return to upload state

**Results state:**
- Section header: "Results (N)"
- 4-col grid of product cards with similarity score badge
- Similarity badge: top-right of image, bg-teal-500/80 text-white text-xs rounded, shows "92.3%"
- Click navigates to /products/:slug

**States:**
- Empty: upload drop zone
- Upload selected: image preview + model selector + search button
- Loading: search button shows spinner, results area shows skeleton grid
- Results: product grid with similarity badges
- Error: toast notification + reset to upload state
- No results: "No visually similar products found. Try a different image."

**Data sources:** visualSearchStore (state machine, file validation, search, results, models)

### 7. NotFoundView — 404

- No breadcrumb, no skeleton
- Centered layout, py-24
- "404" in text-6xl font-light text-neutral-300
- "Page not found" in text-2xl font-semibold
- Descriptive message
- "Back to Home" Button (outlined) linking to /

### 8. AboutView — Static Content

- Breadcrumb: Home > About
- Title: "About ReSys.Shop"
- max-w-3xl centered prose layout, py-12
- Sections: Our Story, Our Technology, Contact
- Static text content — no API calls, no store wiring

### 9. TermsView — Static Content

- Breadcrumb: Home > Terms of Service
- Title: "Terms of Service"
- max-w-3xl centered prose layout, py-12
- Sections: Acceptance, Account Terms, Purchases, Returns, etc.
- Static text content — no API calls, no store wiring

### 10. PrivacyView — Static Content

- Breadcrumb: Home > Privacy Policy
- Title: "Privacy Policy"
- max-w-3xl centered prose layout, py-12
- Sections: Data Collection, Usage, Sharing, Cookies, Rights, etc.
- Static text content — no API calls, no store wiring

## Shared Components

### ProductCard.vue

A reusable product card used across Home, Shop, ProductDetail, Collections, VisualSearch.

**Props:**
- `product: ProductListItem | SimilarProduct` — the product data
- `showBrand: boolean` (default: true)
- `showSimilarityScore: boolean` (default: false)
- `aspectRatio: string` (default: "aspect-[3/4]")

**Template:**
- Image: aspect-[3/4], bg-neutral-100 placeholder (or actual image URL)
- Brand: text-xs text-neutral-500 uppercase tracking-wide
- Product name: text-sm font-medium, truncate
- Price: text-sm font-medium, JetBrains Mono
- Optional similarity badge: top-right corner
- Hover: shadow-sm + image opacity transition
- Link: router-link to /products/:slug

## Testing

**Smoke tests (each view renders):**
1. HomeView renders hero + category cards + featured products
2. ShopView renders sidebar + sort bar + product grid
3. ProductDetailView renders gallery + info + tabs
4. CollectionsView renders category cards
5. VisualSearchView renders upload drop zone
6. NotFoundView renders 404 content
7. AboutView renders static content
8. SearchOverlay opens/closes via keyboard + click

**Integration tests:**
9. ProductCard renders product name + price + links to detail
10. ShopView filter toggle updates product list (debounced)
11. ProductDetailView variant select updates add-to-cart button price

## Non-Scope

- Cart page full implementation (Cycle 3: Ordering)
- Checkout flow (Cycle 3)
- Login/register forms (Cycle 3)
- Account management pages (Cycle 3)
- Address management (Cycle 3)
- Actual product images (uses neutral-100 placeholders)
- Newsletter signup backend (not yet available)
- Product reviews (placeholder only)
- Stripe payment integration (Cycle 3)
