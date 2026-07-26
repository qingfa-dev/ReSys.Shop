==== Catalog Browsing

// Diagram placeholder: Catalog Browsing use case diagram

*UC-STR-CAT-01 — Browse catalog.*
*Primary Actor:* Customer. \
*Main Flow:* Navigate the taxonomy tree to browse products by category. Apply faceted filters to narrow results. Scroll through paginated product grids. \
*Postcondition:* Filtered product listing displayed with thumbnails, prices, and availability indicators. \
*Related FR:* CAT-FR-09, CAT-FR-10.

#v(0.5cm)
*UC-STR-CAT-02 — View product detail.*
*Primary Actor:* Customer. \
*Main Flow:* Select a product to view its complete information: description, fashion metadata, variant options with availability, images, pricing, and taxonomy path. \
*Postcondition:* Product detail page displayed with all variant configurations and real-time availability. \
*Related FR:* CAT-FR-01, CAT-FR-16.

#v(0.5cm)
*UC-STR-CAT-03 — Keyword search.*
*Primary Actor:* Customer. \
*Main Flow:* Enter a text query to search products by name, description, or fashion attributes. \
*Postcondition:* Matching products displayed ranked by relevance. \
*Related FR:* CAT-FR-01.

==== Visual Search

// Diagram placeholder: Visual Search use case diagram

*UC-STR-CAT-04 — Search by image.*
*Primary Actor:* Customer. \
*Main Flow:* Upload a reference image. The system finds and returns visually similar products ranked by similarity. \
*Postcondition:* Visually similar products displayed with similarity scores. Results filtered to items above the configured minimum threshold. \
*Related FR:* CAT-FR-06, CAT-FR-07, CAT-FR-08, NFR-01.

#v(0.5cm)
*UC-STR-CAT-05 — View similar products.*
*Primary Actor:* Customer. \
*Main Flow:* On a product detail page, view products visually similar to the current product. \
*Postcondition:* Similar products displayed for passive discovery without requiring an upload. \
*Related FR:* CAT-FR-17.
