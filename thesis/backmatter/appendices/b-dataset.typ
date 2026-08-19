= Dataset Composition <appendix-b>

This appendix details the composition of the benchmark dataset used in the evaluation presented in Chapter 3.

== Source and Selection

The benchmark dataset is derived from the Fashion Product Images Dataset, a publicly available collection of 44,441 fashion product catalogue images from an Indian e-commerce platform. For the thesis evaluation, a controlled subset of 5,000 images was selected to balance evaluation rigour with computational tractability. The subset was sampled via stratified random sampling to preserve the natural category distribution.

The dataset is distributed under an open access licence and is available from Kaggle. Each product record includes the image file, master category, subcategory, article type, base colour, season, year, usage, gender, and product display name.

== Category Distribution

The 5,000-image subset is stratified across five master categories. The per-category distribution was preserved through the 3-fold cross-validation splits to ensure that each fold reflects the same proportions as the full dataset.

#figure(
  caption: [Dataset Category Distribution],
  table(
    columns: (auto, auto, auto, 1fr),
    align: (left, center, center, center),
    stroke: 0.5pt,
    table.header(
      [*Category*], [*Images*], [*Percentage*], [*Fold Size (Train / Test)*],
    ),
    [Apparel], [2,500], [50.0%], [1,667 / 833],
    [Accessories], [1,250], [25.0%], [833 / 417],
    [Footwear], [750], [15.0%], [500 / 250],
    [Personal Care], [350], [7.0%], [233 / 117],
    [Sporting Goods], [150], [3.0%], [100 / 50],
    [*Total*], [*5,000*], [*100.0%*], [*3,333 / 1,667*],
  ),
  kind: table,
)

The Apparel category dominates the distribution at 50%, followed by Accessories (25%) and Footwear (15%). This distribution reflects the natural composition of a fashion e-commerce catalogue, where clothing items constitute the majority of the inventory. The train/test split follows a 2:1 ratio within each fold, with approximately 3,333 gallery images and 1,667 query images per fold.

== Ground-Truth Label Schemes

Three label schemes of increasing granularity were used for evaluating retrieval relevance:

*Category-only* labels use the master category field (e.g., Apparel, Accessories, Footwear). Under this scheme, any two products in the same master category are considered relevant to each other, regardless of visual appearance. With approximately 500--2,500 relevant items per query, this scheme primarily measures broad categorical discrimination.

*Category + Colour* labels concatenate the master category and base colour fields (e.g., "Apparel/Blue"). This is the primary relevance criterion used in Chapter 6. With an average of 8.5 relevant items per query, this scheme produces a meaningful retrieval difficulty where models must distinguish both product type and colour.

*Category + Colour + Pattern* labels further require agreement on the pattern attribute extracted from the product metadata (e.g., "Apparel/Blue/Striped"). With an average of 3.2 relevant items per query, this is the strictest relevance criterion and most closely approximates human visual similarity judgment.

== Image Preprocessing

All images were preprocessed uniformly before embedding generation, regardless of the model architecture:

- *Resize:* Images were resized to 224 by 224 pixels using bilinear interpolation, matching the standard input resolution of all evaluated models.
- *Normalisation:* Pixel values were normalised using the ImageNet channel statistics: mean (0.485, 0.456, 0.406) and standard deviation (0.229, 0.224, 0.225) for the RGB channels respectively. Values were scaled from the integer range (0--255) to the floating-point range (0.0--1.0) before normalisation.
- *Colour space:* Images were maintained in RGB colour space. No grayscale conversion or colour augmentation was applied.
- *Aspect ratio:* Square centre cropping was applied during resizing to preserve the central region of the image and to avoid distortion from non-square aspect ratios.
