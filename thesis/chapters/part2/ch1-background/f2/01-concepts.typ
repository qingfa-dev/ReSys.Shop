=== Visual Search Concepts

At the heart of visual search is a simple idea: turning images into lists of numbers that a computer can compare. These lists are called *vector embeddings*, sometimes *feature vectors*. When an AI model processes an image, it outputs a fixed-length sequence of numbers representing the visual content. Visually similar products produce similar number sequences; dissimilar products produce different ones.

Content-Based Image Retrieval (CBIR) replaces text queries with image queries. Rather than requiring users to describe what they want in words, CBIR lets them search by example: upload a photo of a garment, and the system retrieves visually similar products from the catalog. This approach bypasses the need for consistent, complete textual labels. A photograph of a dress with a distinctive neckline retrieves visually similar products regardless of how the catalog describes them. The embedding becomes a universal description that captures shape, texture, colour, and pattern automatically.

=== The Semantic Gap

For fashion e-commerce, visual search addresses a fundamental limitation of text-based product discovery. Shoppers often know what they want visually but struggle to articulate it in search terms. A customer who sees a dress they like can search with the image itself, discovering products that share its visual characteristics without needing to name the style, cut, or pattern.

The gap between visual richness and linguistic expression, known as the semantic gap, is the central challenge in image retrieval. Global fashion e-commerce exceeded 770 billion U.S. dollars in 2024 @statista2024fashion, yet search abandonment rates hover around 30 percent when customers cannot find products through text queries @pinterest2023visual. CBIR bridges this gap by operating directly on visual content rather than on human-authored labels.
