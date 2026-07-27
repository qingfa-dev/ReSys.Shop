=== Data Management Strategy

The evaluation utilizes a hybrid dataset strategy. While *Functional Tests* use synthetically generated clean data to ensure deterministic outcomes, the *ML Validation* pipeline utilizes a *Controlled Subset* of the real-world *Fashion Product Images Dataset* to provide authentic visual complexity.

#figure(
  table(
    columns: (1fr, 1fr),
    align: center,
    stroke: 0.5pt,
    [*Category*], [*Count*],
    [Tops], [1,500],
    [Bottoms], [1,200],
    [Footwear], [1,000],
    [Accessories], [800],
    [Jewellery], [500],
    [*Total*], [*5,000*],
  ),
  caption: [Distribution of the controlled test dataset.],
  kind: table,
)
