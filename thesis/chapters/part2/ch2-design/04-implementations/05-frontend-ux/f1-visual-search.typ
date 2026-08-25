===== Visual Search: UC-STR-SRC

The visual search interface implements a four-state UI model:

#figure(
  table(
    columns: (auto, 1.5fr, 2.5fr),
    stroke: 0.5pt,
    align: (left + horizon, left, left),
    inset: 5pt,
    table.header([*State*], [*Display*], [*Trigger*]),
    [Empty], [Upload prompt with dashed-border drop zone, cloud-upload icon, and format guidance text], [Page load or after clearing previous search],
    [Upload], [Selected image preview with a "Search Similar Products" button below], [User drops or selects an image file],
    [Loading], [Skeleton grid of animated placeholder cards (4 columns desktop, 2 mobile)], [Search API request in flight],
    [Results], [Product card grid with thumbnails, names, prices, and colour-coded similarity badges], [API returns non-empty results array],
  ),
  kind: table,
  caption: [Visual search UI state model.],
) <tbl-search-states>

Two input methods are supported: drag-and-drop onto the drop zone and file browser selection. Client-side validation rejects non-image MIME types and files exceeding 10 MB:

```json
{
  "results": [{
    "productId": "a1b2c3d4-...",
    "title": "Floral Summer Midi Dress",
    "price": 750000, "currency": "VND",
    "thumbnailUrl": "/images/products/a1b2c3d4_thumb.webp",
    "similarityScore": 0.9328
  }],
  "searchDurationMs": 287,
  "model": "Fashion-CLIP"
}
```

Each product card renders a thumbnail, name, formatted price, and colour-coded similarity badge (green for $>= 90%$, amber for $>= 80%$, grey below). The query image persists in a sidebar panel while scrolling results.

The four visual search states are illustrated below.

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-cbir-empty.png", width: 100%),
  caption: [Visual search empty state: drop zone with format note (max 10 MB).],
) <fig-cbir-empty>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-cbir-upload.png", width: 100%),
  caption: [Upload state: selected-image preview with Search Similar Products button.],
) <fig-cbir-upload>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-cbir-params.png", width: 100%),
  caption: [Search parameters panel: Fashion-CLIP model selector and three sliders.],
) <fig-cbir-params>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-cbir-results.png", width: 100%),
  caption: [Results state: product grid with similarity badges; query image in sidebar.],
) <fig-cbir-results>
