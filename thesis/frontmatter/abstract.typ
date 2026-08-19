#import "../info.typ": *
#import "../template/i18n.typ": term

#let lang = settings.primary_lang

#heading(level: 1, numbering: none, outlined: true)[ABSTRACT]
#v(1cm)
#set align(left)
#set par(first-line-indent: 1cm, justify: true)

E-commerce platforms rely primarily on text-based search, yet fashion products are inherently visual. Patterns, silhouettes, and textures resist keyword description. This thesis presents a fashion e-commerce platform with integrated Content-Based Image Retrieval (CBIR) that enables customers to search for products by uploading images rather than typing keywords. The system implements a modular architecture with a .NET backend, Vue.js frontend, and a Python machine learning sidecar for embedding generation.

A systematic benchmark evaluates four pre-trained deep learning models across both retrieval accuracy and operational efficiency on a curated fashion product dataset. Fashion-CLIP, a CLIP variant fine-tuned on over 700,000 fashion images, achieved the highest mean Average Precision at 0.9309 (SD 0.0068), outperforming the generic CLIP (mAP 0.9115, SD 0.0077) by 2.13%, EfficientNet-B0 (mAP 0.8895, SD 0.0056) by 4.65%, and ResNet-50 (mAP 0.8857, SD 0.0114) by 5.10%. At the opposite end of the speed spectrum, EfficientNet-B0 delivered inference at 37.8 ms per image (SD 26.6) while maintaining competitive retrieval quality, representing a 2.56x speed advantage over Fashion-CLIP (96.8 ms, SD 6.8) for a 4.65% accuracy trade-off.

Results demonstrate that domain-specific pre-training provides measurable advantages for visual fashion retrieval. The pluggable model architecture, switchable via a single environment variable, allows production deployments to select the optimal accuracy-speed profile for their infrastructure. The thesis provides a reference implementation for systematic comparison of embedding models in the fashion domain.

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

Các sàn thương mại điện tử phụ thuộc nhiều vào tìm kiếm văn bản, một cơ chế kém hiệu quả trong lĩnh vực thời trang, nơi hoa văn, chất liệu và kiểu dáng khó diễn đạt chính xác qua từ khóa. Hệ thống được xây dựng theo hướng module, bao gồm ba thành phần chính: backend .NET xử lý logic nghiệp vụ, frontend Vue.js phục vụ giao diện người dùng, và dịch vụ Python chuyên biệt cho tác vụ trích xuất đặc trưng hình ảnh từ các mô hình học sâu tiền huấn luyện.

Thực nghiệm được thiết lập trên bộ dữ liệu ảnh sản phẩm thời trang với quy trình đánh giá chéo ba lần (3-fold cross-validation), so sánh bốn mô hình embedding trên hai chiều: chất lượng truy xuất (đo bằng mAP, Precision at K, Recall at K) và hiệu năng vận hành (đo bằng độ trễ suy luận, thông lượng, và dung lượng lưu trữ). Mô hình Fashion-CLIP, được tinh chỉnh trên tập dữ liệu hơn 700.000 ảnh thời trang, đạt mAP cao nhất 0,9309 với độ lệch chuẩn 0,0068. CLIP gốc đạt mAP 0,9115 (SD 0,0077), EfficientNet-B0 đạt 0,8895 (SD 0,0056) và ResNet-50 đạt 0,8857 (SD 0,0114). Ở chiều ngược lại, mô hình EfficientNet-B0 cho tốc độ suy luận nhanh nhất, 37,8 ms mỗi ảnh (SD 26,6), nhanh gấp 2,56 lần Fashion-CLIP (96,8 ms, SD 6,8) trong khi chỉ đánh đổi 4,65% về chất lượng truy xuất.

Kết quả thực nghiệm khẳng định giá trị của huấn luyện chuyên biệt theo lĩnh vực trong bài toán truy xuất hình ảnh thời trang, đồng thời cung cấp dữ liệu định lượng cho việc lựa chọn mô hình dựa trên ràng buộc hạ tầng thực tế. Kiến trúc mô hình hoán đổi linh hoạt, được điều khiển qua biến môi trường, cho phép hệ thống thích ứng với nhiều cấu hình triển khai khác nhau mà không cần thay đổi mã nguồn.

#v(1cm)
#set text(style: "normal")
#par(first-line-indent: 0cm)[
  *Từ khóa:* #info.vi.keywords.join(", ")
]

#pagebreak()
