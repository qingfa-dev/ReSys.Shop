#import "../info.typ": *
#import "../template/i18n.typ": term

#let lang = settings.primary_lang
#let data = info.at(lang, default: info.en)

#heading(level: 1, numbering: none, outlined: true)[#term(lang, "abstract_title")]
#v(1cm)
#set align(left)
#set par(first-line-indent: 1cm, justify: true)

E-commerce platforms rely primarily on text-based search, yet fashion products are inherently visual — patterns, silhouettes, and textures resist keyword description. Consumers can easily recognize a specific garment by its visual appearance but struggle to articulate it using standardized metadata terms. This thesis presents a fashion e-commerce platform with integrated Content-Based Image Retrieval that enables customers to search for products by uploading images rather than typing keywords. The system implements a modular architecture with a .NET backend, Vue.js frontend, and a Python machine learning sidecar for embedding generation, connected through a service-oriented design that bridges enterprise-grade web application reliability with access to the Python artificial intelligence ecosystem.

A systematic benchmark evaluates eleven pre-trained deep learning models spanning convolutional neural networks — ResNet and EfficientNet variants — and vision transformers — DINOv2 and CLIP-based architectures including a fashion-specific fine-tuned variant — across both retrieval accuracy and operational efficiency. The evaluation framework measures mean Average Precision, Precision at K, Recall at K, inference latency, throughput, and resource consumption across a curated fashion product dataset.

Results demonstrate that fashion-specific models achieve measurable advantages for visual fashion retrieval, with domain-trained embeddings consistently outperforming general-purpose counterparts in retrieval quality while maintaining inference speeds viable for real-time deployment on commodity hardware. The work shows that open-source tools combined with a pluggable model architecture can deliver production-grade visual search capabilities comparable to commercial solutions, while providing a reference implementation for systematic comparison of embedding models in the fashion domain.

#v(1cm)
#set text(style: "normal")
#par(first-line-indent: 0cm)[
  *#term(lang, "keywords_label")* #data.keywords.join(", ")
]

#pagebreak()
