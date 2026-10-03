#import "../info.typ": *
#import "../template/i18n.typ": term

#let lang = settings.primary_lang

#heading(level: 1, numbering: none, outlined: true)[ABSTRACT]
#v(1cm)
#set align(left)
#set par(first-line-indent: 1cm, justify: true)

E-commerce platforms rely primarily on text-based search, yet fashion products are inherently visual. Patterns, silhouettes, and textures resist keyword description. This thesis presents a fashion e-commerce platform with integrated Content-Based Image Retrieval (CBIR) that enables customers to search by uploading images. The system implements a modular architecture with a .NET backend, Vue.js frontend, and a Python ML sidecar for embedding generation.

A systematic benchmark compares six pre-trained models spanning CNN, vision-transformer, and CLIP architectures on both retrieval accuracy and operational efficiency. Fashion-CLIP, fine-tuned on over 700,000 fashion images, achieved the highest observed mean Average Precision at 0.9336 (SD 0.0060); the nearest competitor, DINOv2 ViT-S/14, scored 0.9299 (SD 0.0058). With only three folds, the 0.40% gap falls within measurement uncertainty. EfficientNet-B0 delivered inference at 42.6 ms (SD 5.6), a 2.67x speed advantage over Fashion-CLIP (113.6 ms) for a 2.86% accuracy trade-off.

Domain-specific pre-training may provide retrieval advantages, though the margin is small and within measurement uncertainty. The pluggable model architecture, switchable via a single environment variable, allows deployments to select the optimal accuracy-speed profile for their infrastructure.

#v(1cm)
#set text(style: "normal")
#par(first-line-indent: 0cm)[
  *Keywords:* #info.en.keywords.join(", ")
]

#pagebreak()

#heading(level: 1, numbering: none, outlined: true)[TÓM TẮT]
#v(1cm)
#set align(left)
#set par(first-line-indent: 1cm, justify: true)

Các sàn thương mại điện tử phụ thuộc nhiều vào tìm kiếm văn bản, một cơ chế kém hiệu quả trong lĩnh vực thời trang, nơi hoa văn, chất liệu và kiểu dáng khó diễn đạt qua từ khóa. Hệ thống được xây dựng theo hướng module: backend .NET, frontend Vue.js, và dịch vụ Python trích xuất đặc trưng hình ảnh từ mô hình học sâu tiền huấn luyện.

Thực nghiệm đánh giá chéo ba lần, so sánh sáu mô hình embedding trên chất lượng truy xuất (mAP, P\@K, R\@K) và hiệu năng vận hành (độ trễ, thông lượng, dung lượng). Fashion-CLIP, tinh chỉnh trên hơn 700.000 ảnh thời trang, đạt mAP quan sát cao nhất 0,9336 (SD 0,0060); mô hình gần nhất, DINOv2 ViT-S/14, đạt 0,9299 (SD 0,0058). Với ba lần đánh giá chéo, biên 0,40% nằm trong phạm vi không chắc chắn đo lường. EfficientNet-B0 đạt tốc độ suy luận nhanh nhất 42,6 ms (SD 5,6), nhanh gấp 2,67 lần Fashion-CLIP (113,6 ms) với mức đánh đổi 2,86% về chất lượng.

Huấn luyện chuyên biệt theo lĩnh vực có thể mang lại lợi thế truy xuất, tuy nhiên biên lợi thế nhỏ và nằm trong phạm vi không chắc chắn đo lường. Kiến trúc mô hình hoán đổi linh hoạt qua biến môi trường cho phép hệ thống thích ứng với nhiều cấu hình triển khai.

#v(1cm)
#set text(style: "normal")
#par(first-line-indent: 0cm)[
  *Từ khóa:* #info.vi.keywords.join(", ")
]

#pagebreak()
