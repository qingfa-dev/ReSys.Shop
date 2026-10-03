=== Qualitative Observations

In addition to the numerical metrics, search results were manually reviewed to get a sense of how well the system worked in practice.

==== Additional Metrics

To evaluate the quality of recommendations, a few additional metrics were calculated:

- *Category Match Rate:* How often the recommended products are in the same category as the query. For example, if searching for a dress, how many results are also dresses?

- *Diversity:* Whether the results show a variety of products or just very similar items. Some diversity is good because it helps users discover options they might not have considered.

- *Semantic Distance:* How "close" the recommendations are to the query in the vector space. Lower distance means more similar results.

#figure(
  table(
    columns: (auto, auto),
    stroke: 0.5pt,
    align: center,
    [*Metric*], [*Value*],
    [Category Match Rate], [71.4%],
    [Diversity Score], [0.202],
    [Average Distance], [0.127],
  ),
  caption: [Performance of the recommendation feature using CLIP],
  kind: table,
) <tab-recommendation>

The category match rate of 71.4% means that about 7 out of 10 recommended products were in the same category as the query product. This is reasonable, though there is room for improvement.

=== Comparative Model Performance Analysis

Based on both the numerical tests and manual observation, each model seemed to have different strengths:

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (left, left),
    [*Model*], [*Worked Best For:*],
    [Fashion-CLIP], [Finding products with similar style or category (formal vs casual)],
    [DINOv2], [Matching the shape and silhouette of garments, even in different colors],
    [EfficientNet], [Finding products with similar colors or simple patterns],
  ),
  caption: [Observed strengths of each model],
  kind: table,
)

For example, when searching with an image of a floral dress:
- Fashion-CLIP found other dresses with similar styles
- DINOv2 found dresses with similar shapes regardless of pattern
- EfficientNet found items with similar colors, even if they were not dresses

This suggests that different models might be better suited for different use cases. A future improvement could be to let users choose which type of similarity they care about.

=== Visualizing the Search Space

To better understand how the models organize products, a visualization technique called *t-SNE* was used. This technique takes the high-dimensional vectors (512 numbers) and projects them onto a 2D plot that humans can look at.

When viewing the t-SNE plot of Fashion-CLIP embeddings, products in similar categories tended to cluster together. For example, women's footwear appeared in one region while sportswear appeared in another. This suggests the model is capturing meaningful fashion-related features.

However, the clusters were not perfectly separated, which explains why some search results occasionally included items from different categories. This is a limitation that could potentially be improved with model fine-tuning, which was not attempted in this project.

