===== 2. Contextual Recommendations (More Like This)
On the Product Detail Page, the system proactively suggests similar items using a *Related Products Carousel*.
- *Trigger:* The component automatically initiates a background fetch when mounted or when the primary product changes.
- *Data Source:* It requests the "Recommendations" endpoint which performs a vector similarity search against the current product's embedding.
- *Presentation:* Items are displayed in a horizontal scroll view, prioritizing visual style matches over simple category matches.

#figure(
  placement: none,
  image("../../../../../images/ui/store/ui-store-catalog-product-detail-rec-fashionclip.png", width: 100%),
  caption: [Contextual Recommendations: 'You May Also Like' carousel powered by Fashion-CLIP vector similarity.],
)

*Contextual Suggestion Engine:*
- *UI Trigger (Lazy Load):* The "You May Also Like" carousel remains dormant until it enters the user's viewport. An `IntersectionObserver` detects this visibility event, ensuring that the heavy similarity query is only executed when the user actually engages with the bottom of the page ("Scroll Spy" pattern).
- *Sequence Flow:* @fig:sq-0008-rec depicts the query execution. The system extracts the vector of the currently viewed product and requests the `N` nearest neighbors from the `pgvector` index.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/customer/sq-0008-recommendations.png", width: 95%),
  caption: [Contextual Recommendations Sequence: Fetching similar products based on vector proximity (UC-0008).],
) <fig:sq-0008-rec>
