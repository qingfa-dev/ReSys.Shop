==== Catalog Browsing

// Diagram placeholder: Catalog Browsing use case diagram

==== UC-STR-BRW — Browse and Search Catalog

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-BRW],
    [*Use Case Name*], [Browse and Search Catalog],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [Browse the product catalog, view product details, and search by keyword.],
    [*Trigger*], [Customer visits the storefront and selects a category, product, or enters a search term.],
    [*Preconditions*], [
      - Catalog data is published and searchable.
    ],
    [*Postconditions*], [
      - Product listing or detail displayed with pricing and availability.
    ],
    [*Main Success Scenario*], [
      *Browse Catalog*
      1. Visits the storefront home page or a category landing page.
      2. System displays the taxonomy tree with top-level categories.
      3. Selects a category or subcategory from the taxonomy navigation.
      4. System retrieves products associated with the selected taxon and its descendants.
      5. System displays a paginated grid of product cards showing thumbnail, name, price, and availability.
      6. Applies optional facets to filter by option types (e.g. size, colour) and other attributes.
      7. System refreshes the listing with filtered results.
      ,
      *View Product Detail*
      1. Clicks on a product card from a catalog listing, search result, or recommendation.
      2. System displays product detail page with primary image, name, description, and price range.
      3. System shows fashion metadata: style code, season, material, department, gender target.
      4. System displays taxonomy breadcrumb showing category path.
      5. Browses product images in the gallery.
      6. Selects variant option values from available options.
      7. System updates displayed price, availability, and variant-specific images based on selection.
      ,
      *Keyword Search*
      1. Enters a search keyword or phrase in the storefront search bar.
      2. System queries the catalog for products matching name, description, or fashion attributes.
      3. System ranks results by relevance and displays a paginated grid of matching product cards.
      4. Refines search with additional keywords or applies filters.
    ],
    [*Alternative Flows*], [
      A1. No products in category: system displays empty state suggesting other categories.
      A2. Selected variant out of stock: system shows out-of-stock status and disables add-to-cart.
      A3. No products match keyword: system displays suggestions to check spelling or browse categories.
      A4. Very short query (single character): system prompts to enter at least two characters.
    ],
    [*Exception Flows*], [
      E1. Retrieval or search failure: system displays error and offers retry.
    ],
    [*Related Requirements*], [CAT-FR-01, CAT-FR-02, CAT-FR-03, CAT-FR-09, CAT-FR-16, CAT-FR-22],
  ),
  caption: [UC-STR-BRW -- Browse and Search Catalog.],
)

==== Search

// Diagram placeholder: Search use case diagram

==== UC-STR-SRC — Visual Search

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-SRC],
    [*Use Case Name*], [Visual Search],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [ML Service],
    [*Goal*], [Search for products by uploading a reference image and discover visually similar items.],
    [*Trigger*], [Customer navigates to visual search or views a product detail page.],
    [*Preconditions*], [
      - Catalog has products with embeddings.
      - ML service is operational.
    ],
    [*Postconditions*], [
      - Visually similar products displayed with similarity scores above configured threshold.
    ],
    [*Main Success Scenario*], [
      *Search by Image (CBIR)*
      1. Navigates to the visual search interface.
      2. Selects or drags an image file from the local file system.
      3. System validates image format (JPEG, PNG, WebP) and size constraints.
      4. System sends the image to the ML service for embedding generation.
      5. System performs similarity search against the image embeddings index.
      6. System retrieves matching products ranked by similarity score above minimum threshold.
      7. System displays results as a grid of product thumbnails with scores, linking to detail pages.
      ,
      *View Similar Products*
      1. Opens a product detail page with image embeddings available.
      2. System retrieves the product's primary image embedding.
      3. System performs similarity search against other product embeddings.
      4. System displays a horizontal carousel or grid of visually similar product cards.
      5. Clicks on a similar product card to navigate to its detail page.
    ],
    [*Alternative Flows*], [
      A1. Invalid image format: system rejects and lists accepted formats.
      A2. No products above threshold: system displays empty result suggesting different image or keyword search.
      A3. No images or embeddings on detail page: system hides the similar products section.
      A4. ML service unavailable: system gracefully hides section without affecting detail page.
    ],
    [*Exception Flows*], [
      E1. ML service unavailable: system displays error and suggests retrying later.
      E2. Embedding index empty: system reports visual search not yet available.
    ],
    [*Related Requirements*], [CAT-FR-06, CAT-FR-07, CAT-FR-08, CAT-FR-17],
  ),
  caption: [UC-STR-SRC -- Visual Search.],
)
