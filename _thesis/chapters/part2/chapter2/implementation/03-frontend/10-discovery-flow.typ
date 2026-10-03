===== 1. Discovery (Search & Browser)
Users can explore the catalog via semantic text search or visual similarity.
- *Visual Search View:* A dedicated interface centered around the upload zone, offering real-time image previews.
- *Product List View:* A comprehensive grid layout featuring a faceted sidebar for filtering. It utilizes infinite scrolling to handle large datasets smoothly.
- *Product Detail View:* A high-fidelity view focusing on imagery, incorporating the "Related Products" AI carousel alongside standard variant selection.

#figure(
  placement: none,
  image("../../../../../images/ui/store/ui-store-catalog-product-detail.png", width: 100%),
  caption: [Discovery UI: Standard Product Detail Page illustrating the integration of variants and rich media.],
)

#figure(
  placement: none,
  table(
    columns: (120pt, 1fr),
    stroke: 0.5pt,
    align: (left, center),
    [*Filter Sidebar*], [*Product Grid*],
    [Categories (Tree) \ Price Range \ Color \ Size],
    [Search Bar (Camera Icon) $arrow$ Product Cards (Image, Title, Price, Similarity Badge) $arrow$ Infinite Scroll Loader],
  ),
  caption: [Storefront Discovery Layout: Combining faceted filtering with an AI-integrated search experience.],
)

*Search Interaction Strategy:*
- *UI Logic (Debounce):* To prevent API flooding, the Search Bar implements a strict *300ms Debounce*. Input events (`@input`) clear any pending `setTimeout` timers, ensuring the expensive `PerformedSearch` event is only fired once the user pauses typing. A "Loading Spinner" provides immediate visual feedback during this wait state.
- *Sequence Flow:* @fig:sq-0003-search illustrates the execution path. The frontend dispatches a `SearchProductsQuery`. The backend leverages *Azure AI Search* (or a `pg_trgm` fallback) to return ranked candidates based on lexical similarity.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/customer/sq-0003-keyword-search.png", width: 55%),
  caption: [Keyword Search Flow: The execution path for text-based queries against product indices (UC-0003).],
) <fig:sq-0003-search>

*Browsing & Pagination Strategy:*
- *UI Pattern (Infinite Scroll):* The Product Grid replaces traditional pagination with a *Virtual Scroller*. A scroll listener detects when the viewport approaches the bottom (threshold $\ge$ 90%), triggering a "Fetch More" action without user intervention.
- *Sequence Flow:* As defined in @fig:sq-0001-browse, this triggers a `GetProducts` query for Page $N+1$. The backend uses *Cursor-based Pagination* (rather than `OFFSET/LIMIT`) to ensure stable performance and prevent "skipped items" if new products are added during the session.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/customer/sq-0001-browse-products.png", width: 55%),
  caption: [Browse Products Sequence: Categories, filtering, and infinite scroll pagination (UC-0001).],
) <fig:sq-0001-browse>


