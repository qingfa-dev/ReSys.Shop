#import "../info.typ": *
#import "../template/i18n.typ": term

#let lang = settings.primary_lang

#heading(level: 1, numbering: none, outlined: true)[ABSTRACT]
#v(1cm)
#set align(left)
#set par(first-line-indent: 1cm, justify: true)

E-commerce platforms rely primarily on text-based search, yet fashion products are inherently visual \u2014 patterns, silhouettes, and textures resist keyword description. This thesis presents a fashion e-commerce platform with integrated Content-Based Image Retrieval (CBIR) that enables customers to search for products by uploading images rather than typing keywords. The system implements a modular architecture with a .NET backend, Vue.js frontend, and a Python machine learning sidecar for embedding generation.

A systematic benchmark evaluates four pre-trained deep learning models \u2014 Fashion-CLIP, ResNet-50, EfficientNet-B0, and CLIP ViT-B/32 \u2014 across both retrieval accuracy and operational efficiency on a curated fashion product dataset. Fashion-CLIP, a CLIP variant fine-tuned on over 700,000 fashion images, achieved the highest mean Average Precision at 0.7455, outperforming ResNet-50 (mAP 0.7150) by 4.3% and the generic CLIP (mAP 0.7026) by 6.1%. At the opposite end of the speed\u2013accuracy spectrum, EfficientNet-B0 delivered inference at 21.6 ms per image while maintaining competitive retrieval quality (mAP 0.7196), representing a 3.9\u00d7 throughput advantage over Fashion-CLIP (84.4 ms) for a 3.5% accuracy trade-off.

Results demonstrate that domain-specific pre-training provides measurable advantages for visual fashion retrieval. The pluggable model architecture \u2014 switchable via a single environment variable \u2014 allows production deployments to select the optimal accuracy\u2013speed profile for their infrastructure. The thesis provides a reference implementation for systematic comparison of embedding models in the fashion domain.

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

Các nền tảng thương mại điện tử hiện nay chủ yếu phụ thuộc vào tìm kiếm bằng văn bản, tuy nhiên sản phẩm thời trang mang tính trực quan cao \u2014 hoa văn, kiểu dáng, và chất liệu vải rất khó diễn đạt chính xác bằng từ khóa. Luận văn này phát triển một hệ thống thương mại điện tử thời trang tích hợp truy xuất hình ảnh dựa trên nội dung (CBIR), cho phép khách hàng tìm kiếm sản phẩm bằng cách tải lên hình ảnh thay vì gõ từ khóa. Hệ thống được xây dựng theo kiến trúc module với backend .NET, frontend Vue.js, và dịch vụ máy học Python đảm nhận việc tạo embedding.

Thực nghiệm so sánh có hệ thống đánh giá bốn mô hình học sâu tiền huấn luyện \u2014 Fashion-CLIP, ResNet-50, EfficientNet-B0, và CLIP ViT-B/32 \u2014 trên cả hai tiêu chí: chất lượng truy xuất và hiệu năng vận hành, sử dụng bộ dữ liệu ảnh sản phẩm thời trang đã được gán nhãn thủ công. Fashion-CLIP \u2014 biến thể CLIP được tinh chỉnh trên hơn 700.000 ảnh thời trang \u2014 đạt độ chính xác trung bình (mAP) cao nhất ở mức 0,7455, vượt qua ResNet-50 (mAP 0,7150) và EfficientNet-B0 (mAP 0,7196). Ở chiều ngược lại, EfficientNet-B0 đạt tốc độ suy luận 21,6 ms trên mỗi ảnh \u2014 nhanh gấp 3,9 lần Fashion-CLIP (84,4 ms) \u2014 với mức đánh đổi về độ chính xác chỉ 3,5%.

Các kết quả cho thấy mô hình được huấn luyện chuyên biệt cho lĩnh vực thời trang mang lại lợi thế rõ rệt trong truy xuất hình ảnh sản phẩm. Kiến trúc mô hình có thể hoán đổi linh hoạt \u2014 chỉ cần thay đổi một biến môi trường \u2014 cho phép hệ thống triển khai trong môi trường sản xuất lựa chọn cấu hình chính xác\u2013tốc độ tối ưu theo hạ tầng sẵn có. Luận văn cung cấp một cài đặt tham chiếu phục vụ so sánh có hệ thống các mô hình embedding trong lĩnh vực thời trang.

#v(1cm)
#set text(style: "normal")
#par(first-line-indent: 0cm)[
  *Từ khóa:* #info.vi.keywords.join(", ")
]

#pagebreak()
