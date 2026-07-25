#import "../info.typ": *
#import "../template/i18n.typ": term

#let lang = settings.primary_lang

#heading(level: 1, numbering: none, outlined: true)[#term(lang, "abbreviations_title")]
#v(1cm)
#set align(left)
#set par(first-line-indent: 0cm)

#table(
  columns: (auto, 1fr),
  stroke: none,
  align: (left, left),
  inset: (x: 8pt, y: 12pt),
  [*#term(lang, "abbreviations_term")*], [*#term(lang, "abbreviations_desc")*],
  [API], [Application Programming Interface],
  [ANN], [Approximate Nearest Neighbor],
  [CBIR], [Content-Based Image Retrieval],
  [CNN], [Convolutional Neural Network],
  [CQRS], [Command Query Responsibility Segregation],
  [DDD], [Domain-Driven Design],
  [DSR], [Design Science Research],
  [EF Core], [Entity Framework Core],
  [HNSW], [Hierarchical Navigable Small World],
  [JWT], [JSON Web Token],
  [mAP], [Mean Average Precision],
  [P\@K], [Precision at K],
  [R\@K], [Recall at K],
  [REST], [Representational State Transfer],
  [SPA], [Single Page Application],
  [ViT], [Vision Transformer],
  [VSA], [Vertical Slice Architecture],
)
#pagebreak()
