==== Catalog Browsing

// Diagram placeholder: Catalog Browsing use case diagram

==== UC-STR-BRW-01 — Browse Catalog

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-BRW-01],
    [*Use Case Name*], [Browse Catalog],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [Navigate the taxonomy tree to browse products by category with faceted filtering and paginated product grids.],
    [*Trigger*], [Customer visits the storefront and selects a category from the taxonomy navigation.],
    [*Preconditions*], [
      - Catalog and taxonomy data are published and available.
    ],
    [*Postconditions*], [
      - Filtered product listing displayed with thumbnails, prices, and availability indicators.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Visits the storefront home page or a category landing page.
      2. System -- Displays the taxonomy tree with top-level categories.
      3. Customer -- Selects a category or subcategory from the taxonomy navigation.
      4. System -- Retrieves products associated with the selected taxon and its descendants.
      5. System -- Displays a paginated grid of product cards showing thumbnail, name, price, and availability status.
      6. Customer -- Applies optional facets to filter by option types (e.g. size, colour) and other attributes.
      7. System -- Refreshes the listing with filtered results.
      8. Customer -- Browses additional pages using pagination controls.
    ],
    [*Alternative Flows*], [
      A1. No products in the selected category -- System displays an empty state message suggesting the customer browse other categories.
      A2. Customer sorts products by price, name, or relevance -- System reorders the listing accordingly.
      A3. Customer clears all applied facets -- System returns to the unfiltered listing for the current category.
    ],
    [*Exception Flows*], [
      E1. System fails to retrieve catalog data -- System displays an error message and offers a retry option.
    ],
    [*Related Requirements*], [CAT-FR-09, CAT-FR-16],
  ),
  caption: [UC-STR-BRW-01 -- Browse Catalog.],
)

==== UC-STR-BRW-02 — View Product Detail

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-BRW-02],
    [*Use Case Name*], [View Product Detail],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [View complete product information: description, fashion metadata, variant options with availability, images, pricing, and taxonomy path.],
    [*Trigger*], [Customer selects a product from a catalog listing or search result.],
    [*Preconditions*], [
      - The product exists and is published.
    ],
    [*Postconditions*], [
      - Product detail page displayed with all variant configurations and real-time availability.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Clicks on a product card from a catalog listing, search result, or recommendation.
      2. System -- Displays the product detail page with primary image, product name, description, and price range.
      3. System -- Shows fashion-specific metadata: style code, season, material, department, gender target.
      4. System -- Displays the taxonomy breadcrumb showing the category path.
      5. Customer -- Browses product images in the gallery.
      6. Customer -- Selects variant option values (e.g. Size M, Colour Red) from available options.
      7. System -- Updates the displayed price, availability status, and variant-specific images based on the selection.
      8. Customer -- Views the product detail including care instructions, fit notes, and shipping information.
    ],
    [*Alternative Flows*], [
      A1. Selected variant is out of stock -- System displays out-of-stock status and disables the add-to-cart button; the customer can browse other variants.
      A2. Product has only one variant -- System displays the single set of options and does not require a selection step.
      A3. Customer navigates to a similar product -- System links to visually similar products (see UC-STR-BRW-03).
    ],
    [*Exception Flows*], [
      E1. Product data is temporarily unavailable -- System displays an error message and suggests the customer browse other products or retry.
    ],
    [*Related Requirements*], [CAT-FR-02, CAT-FR-03, CAT-FR-16, CAT-FR-22],
  ),
  caption: [UC-STR-BRW-02 -- View Product Detail.],
)

==== UC-STR-BRW-03 — View Similar Products

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-BRW-03],
    [*Use Case Name*], [View Similar Products],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [ML Service],
    [*Goal*], [On a product detail page, view products visually similar to the current product for passive discovery.],
    [*Trigger*], [Customer views a product detail page that has image embeddings available.],
    [*Preconditions*], [
      - The product has images with available embeddings.
      - The ML service is operational.
    ],
    [*Postconditions*], [
      - Similar products displayed for passive discovery without requiring an upload.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Opens a product detail page.
      2. System -- Retrieves the product's primary image embedding from the embedding index.
      3. System -- Performs a similarity search against other product embeddings.
      4. System -- Retrieves matching products ranked by visual similarity.
      5. System -- Displays a horizontal carousel or grid of visually similar product cards below the main product information.
      6. Customer -- Clicks on a similar product card to navigate to its detail page.
    ],
    [*Alternative Flows*], [
      A1. Product has no images or embeddings -- System hides the similar products section entirely.
      A2. No other products meet the similarity threshold -- System hides the similar products section.
      A3. ML service is unavailable -- System gracefully hides the similar products section without affecting the rest of the product detail page.
    ],
    [*Exception Flows*], [
      E1. Similarity search fails due to an unexpected error -- System hides the section and logs the error; the product detail page remains fully functional.
    ],
    [*Related Requirements*], [CAT-FR-17],
  ),
  caption: [UC-STR-BRW-03 -- View Similar Products.],
)

==== Search

// Diagram placeholder: Search use case diagram

==== UC-STR-SRC-01 — Search by Image

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-SRC-01],
    [*Use Case Name*], [Search by Image],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [ML Service],
    [*Goal*], [Find visually similar products by uploading a reference image and receiving ranked results.],
    [*Trigger*], [Customer navigates to the visual search interface and uploads an image.],
    [*Preconditions*], [
      - Catalog contains products with available image embeddings.
      - The ML service is operational.
    ],
    [*Postconditions*], [
      - Visually similar products displayed with similarity scores.
      - Results filtered above the configured minimum similarity threshold.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Navigates to the visual search interface.
      2. Customer -- Selects or drags an image file from the local file system.
      3. System -- Validates the image format (JPEG, PNG, WebP) and size constraints.
      4. System -- Sends the image to the ML service for embedding generation.
      5. System -- Receives the visual embedding from the ML service.
      6. System -- Performs a similarity search against the image embeddings index.
      7. System -- Retrieves matching products ranked by similarity score, filtered above the configured minimum threshold.
      8. System -- Displays results as a grid of product thumbnails with similarity scores, linking each result to its product detail page.
    ],
    [*Alternative Flows*], [
      A1. Invalid image format -- System rejects the upload and displays a message listing the accepted formats.
      A2. Image exceeds the maximum file size -- System rejects the upload and displays the applicable size constraint.
      A3. No products found above the similarity threshold -- System displays an empty result message suggesting the customer try a different image or use keyword search.
      A4. Customer adjusts the similarity threshold -- System re-filters the existing results and reflects the updated threshold immediately.
    ],
    [*Exception Flows*], [
      E1. ML service is unavailable -- System displays an error message indicating the visual search service is temporarily unavailable and suggests retrying later.
      E2. Embedding index is empty -- System reports that visual search is not yet available because no product images have been processed.
    ],
    [*Related Requirements*], [CAT-FR-06, CAT-FR-07, CAT-FR-08],
  ),
  caption: [UC-STR-SRC-01 -- Search by Image.],
)

==== UC-STR-SRC-02 — Keyword Search

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-SRC-02],
    [*Use Case Name*], [Keyword Search],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [Enter a text query to search products by name, description, or fashion attributes.],
    [*Trigger*], [Customer enters a search term in the storefront search bar.],
    [*Preconditions*], [
      - Catalog data is indexed and searchable.
    ],
    [*Postconditions*], [
      - Matching products displayed ranked by relevance.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Enters a search keyword or phrase in the storefront search bar.
      2. System -- Queries the catalog for products whose name or description matches the search terms.
      3. System -- Ranks results by relevance based on match quality across product fields.
      4. System -- Displays a paginated grid of matching product cards with thumbnails, names, prices, and availability.
      5. Customer -- Refines the search with additional keywords or applies category and attribute filters.
    ],
    [*Alternative Flows*], [
      A1. No products match the search query -- System displays an empty result message with suggestions: check spelling, use different keywords, or browse categories.
      A2. Customer uses a very short query (single character) -- System prompts the customer to enter at least two characters.
      A3. Search query matches fashion attributes (e.g. "cotton dress") -- System returns products matching both the name/description and fashion metadata fields.
    ],
    [*Exception Flows*], [
      E1. Search service is temporarily degraded -- System displays a message indicating that search results may be incomplete and suggests retrying shortly.
    ],
    [*Related Requirements*], [CAT-FR-01],
  ),
  caption: [UC-STR-SRC-02 -- Keyword Search.],
)
