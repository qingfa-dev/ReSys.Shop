== Result of Testing

=== Visual Search (CBIR) -- UC-STR-SRC

#figure(
  table(
    columns: (auto, 2fr, 4fr, 3fr, auto),
    stroke: 0.5pt,
    align: (center, left, left, left, center),
    inset: 5pt,
    table.header([*No*], [*Description*], [*Step*], [*Expected Result*], [*Result*]),

    [1], [Upload query image], [Drop JPEG on drop zone; click "Search Similar Products".], [Grid with thumbnails and colour badges (green >= 90%, amber >= 80%).], [Pass],
    [2], [Loading state], [Upload image; observe during API request.], [Skeleton grid of animated placeholder cards displayed.], [Pass],
    [3], [No results], [Upload image with no visually similar products.], ["No similar products found" with "Try Again" button.], [Pass],
    [4], [Invalid file type], [Attempt to upload a PDF on the search page.], [Rejected client-side with unsupported format message.], [Pass],
    [5], [File exceeds 10 MB], [Attempt to upload an image over 10 MB.], [Rejected with size-limit warning before network request.], [Pass],
    [6], [Sidecar unavailable], [Stop ML sidecar; upload image and search.], [HTTP 503; catalogue and cart endpoints remain functional.], [Pass],
    [7], [Sidecar recovery], [Restart sidecar; wait for health check; search.], [Health probe OK; search returns correct results.], [Pass],
  ),
  kind: table,
  caption: [Visual search test cases: normal flow, edge cases, and fault recovery.],
) <tbl-result-cbir>

=== ML Embedding Pipeline

#figure(
  table(
    columns: (auto, 2fr, 4fr, 3fr, auto),
    stroke: 0.5pt,
    align: (center, left, left, left, center),
    inset: 5pt,
    table.header([*No*], [*Description*], [*Step*], [*Expected Result*], [*Result*]),

    [8], [Fashion-CLIP], [POST /embeddings/bytes with fashion image.], [512-dim vector; model "fashion_clip"; inference_ms present.], [Pass],
    [9], [EfficientNet-B0], [Set model to EfficientNet-B0; POST request.], [1280-dim vector; model "efficientnet_b0".], [Pass],
    [10], [ResNet-50], [Set model to ResNet-50; POST request.], [2048-dim vector; model "resnet50".], [Pass],
    [11], [CLIP-generic], [Set model to CLIP ViT-B/16; POST request.], [512-dim vector; model "clip_vit_b16".], [Pass],
    [12], [Model-name filtering], [Generate with two models; search and verify.], [Results filtered by model name; match active model only.], [Pass],
    [13], [Invalid API key], [POST /embeddings/bytes without API key.], [HTTP 401 Unauthorized.], [Pass],
  ),
  kind: table,
  caption: [ML embedding pipeline test cases across four model architectures.],
) <tbl-result-embedding>

=== Shopping Cart and Checkout -- UC-STR-CRT, UC-STR-CHK

#figure(
  table(
    columns: (auto, 2fr, 4fr, 3fr, auto),
    stroke: 0.5pt,
    align: (center, left, left, left, center),
    inset: 5pt,
    table.header([*No*], [*Description*], [*Step*], [*Expected Result*], [*Result*]),

    [14], [Add item to cart], [Select variant (size, colour); click "Add to Cart".], [Cart count updates in header; toast displayed.], [Pass],
    [15], [Update quantity], [Open cart; change quantity from 1 to 3.], [Line and cart subtotals recalculate correctly.], [Pass],
    [16], [Remove item], [Open cart; click remove on a line item.], [Item removed; subtotal updated; items displayed.], [Pass],
    [17], [Guest cart merge], [Add items as guest; register and log in.], [Guest cart merges; cookie invalidated; no items lost.], [Pass],
    [18], [Exceed stock], [Add item; increase quantity beyond available stock.], [Rejected with max-available notification; capped.], [Pass],
    [19], [Checkout pipeline], [Complete Address, Delivery, Payment, Confirm, Complete.], [Progress bar advances; order generated; inventory reserved.], [Pass],
    [20], [Empty cart checkout], [Navigate to checkout with empty cart.], [Redirected to cart; "Your cart is empty" displayed.], [Pass],
    [21], [Cancel order], [Open order in pre-completion state; cancel.], [Order Cancelled; inventory released; payment voided.], [Pass],
  ),
  kind: table,
  caption: [Shopping cart and checkout test cases.],
) <tbl-result-cart-checkout>

=== Admin Product Management -- UC-ADM-PROD, UC-ADM-VAR, UC-ADM-IMG

#figure(
  table(
    columns: (auto, 2fr, 4fr, 3fr, auto),
    stroke: 0.5pt,
    align: (center, left, left, left, center),
    inset: 5pt,
    table.header([*No*], [*Description*], [*Step*], [*Expected Result*], [*Result*]),

    [22], [Create product], [Fill name, slug, fashion metadata; click "Save".], [Product created with generated slug; notification.], [Pass],
    [23], [Duplicate slug], [Create product with existing slug.], [Error: slug exists; product not created.], [Pass],
    [24], [Add variant], [Select Size M and Colour Navy; enter SKU, price.], [Variant added with option combination in table.], [Pass],
    [25], [Upload images], [Upload 3 images; set display order; check status.], [Green checkmark with model name per image; order kept.], [Pass],
    [26], [Regenerate embeddings], [Switch model; click "Regenerate All Embeddings".], [Status reset to pending; then green with new model.], [Pass],
    [27], [Archive product], [Change status to Archived; verify storefront.], [Hidden from storefront; admin shows Archived badge.], [Pass],
    [28], [Taxonomy management], [Create taxon under "Dresses"; drag to reorder.], [New taxon with auto-slug; tree reordered; count updates.], [Pass],
  ),
  kind: table,
  caption: [Admin product, variant, image, and taxonomy management test cases.],
) <tbl-result-admin>

=== Summary

All 28 test cases passed across four functional areas: visual search (7), ML embedding pipeline (6), cart and checkout (8), and admin product management (7). Error states, edge cases (empty carts, duplicate slugs, stock exhaustion), and recovery scenarios (sidecar restart) were handled correctly.
