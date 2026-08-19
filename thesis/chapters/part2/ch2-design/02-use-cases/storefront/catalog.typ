==== Catalog Browsing
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-catalog-browsing.png",
    width: 50%
  ),
  caption: [Use case diagram for Catalog Browsing (UC-STR-BRW).],
) <fig-uc-str-brw-d>

==== UC-STR-BRW: Browse and Search Catalog

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-STR-BRW: Browse and Search Catalog],
    [*Actor*], [Customer],
    [*Goal*], [Browse the product catalog, view product details, and search by keyword.],
    [*Pre/Post*], [
      Pre: catalog data is published and searchable.
      Post: product listing or detail displayed with pricing and availability.
    ],
    [*Scenario*], [
      *Browse Catalog*
      + Visits storefront home page or category landing page.
      + System displays taxonomy tree with top-level categories.
      + Selects category or subcategory from taxonomy navigation.
      + System retrieves products associated with selected taxon and its descendants.
      + System displays paginated grid of product cards showing thumbnail, name, price, availability.
      + Applies optional facets to filter by option types (e.g. size, colour) and other attributes.
      + System refreshes listing with filtered results.
      ,
      *View Product Detail*
      + Clicks product card from catalog listing, search result, or recommendation.
      + System displays product detail page with primary image, name, description, price range.
      + System shows fashion metadata (style code, season, material, department, gender target).
      + System displays taxonomy breadcrumb showing category path.
      + Browses product images in gallery.
      + Selects variant option values from available options.
      + System updates displayed price, availability, variant-specific images based on selection.
      ,
      *Keyword Search*
      + Enters search keyword or phrase in storefront search bar.
      + System queries catalog for products matching name, description, or fashion attributes.
      + System ranks results by relevance, displays paginated grid of matching product cards.
      + Refines search with additional keywords or applies filters.
      ,
    ],
    [*Alternatives*], [
      + A1. No products in category → system displays empty state suggesting other categories.
      + A2. Selected variant out of stock → system shows out-of-stock status, disables add-to-cart.
      + A3. No products match keyword → system displays suggestions to check spelling or browse categories.
      + A4. Very short query (single character) → system prompts to enter at least two characters.
    ],
    [*Exceptions*], [
      + E1. Retrieval or search failure → system displays error, offers retry.
    ],
    [*Requirements*], [CAT-FR-01, CAT-FR-02, CAT-FR-03, CAT-FR-09, CAT-FR-16, CAT-FR-22],
  ),
    kind: table,
  caption: [Browse and Search Catalog.],
)

==== Search
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-search.png",
    width: 70%
  ),
  caption: [Use case diagram for Search (UC-STR-SRC).],
) <fig-uc-str-src-d>

==== UC-STR-SRC: Visual Search

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-STR-SRC: Visual Search],
    [*Actor*], [Customer],
    [*Support*], [ML Service],
    [*Goal*], [Search for products by uploading a reference image and discover visually similar items.],
    [*Pre/Post*], [
      Pre: catalog has products with embeddings; ML service is operational.
      Post: visually similar products displayed with similarity scores above configured threshold.
    ],
    [*Scenario*], [
      *Search by Image (CBIR)*
      + Navigates to visual search interface.
      + Selects or drags image file from local file system.
      + System validates image format (JPEG, PNG, WebP) and size constraints.
      + System sends image to ML service for embedding generation.
      + System performs similarity search against image embeddings index.
      + System retrieves matching products ranked by similarity score above minimum threshold.
      + System displays results as grid of product thumbnails with scores, linking to detail pages.
      ,
      *View Similar Products*
      + Opens product detail page with image embeddings available.
      + System retrieves product's primary image embedding.
      + System performs similarity search against other product embeddings.
      + System displays horizontal carousel or grid of visually similar product cards.
      + Clicks on similar product card to navigate to its detail page.
      ,
    ],
    [*Alternatives*], [
      + A1. Invalid image format → system rejects, lists accepted formats.
      + A2. No products above threshold → system displays empty result suggesting different image or keyword search.
      + A3. No images or embeddings on detail page → system hides similar products section.
      + A4. ML service unavailable → system gracefully hides section without affecting detail page.
    ],
    [*Exceptions*], [
      + E1. ML service unavailable → system displays error, suggests retrying later.
      + E2. Embedding index empty → system reports visual search not yet available.
    ],
    [*Requirements*], [CAT-FR-06, CAT-FR-07, CAT-FR-08, CAT-FR-17],
  ),
    kind: table,
  caption: [Visual Search.],
)
