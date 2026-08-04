// Abstract
#import "../info.typ": *
#import "../template/i18n.typ": term

#let lang = settings.primary_lang
#let other_lang = if lang == "en" { "vi" } else { "en" }
#let data = info.at(lang)
#let other_data = info.at(other_lang)

// English Abstract (Vietnamese Student Style)
#heading(level: 1, numbering: none, outlined: true)[ABSTRACT]
#v(1cm)
#set align(left)
#set par(first-line-indent: 1cm, justify: true)

Traditional keyword search in fashion e-commerce is limited in identifying the visual nuances of user intent, creating a "semantic gap" between what users see and how they search. This thesis introduces *ReSys.Shop*, an e-commerce ecosystem that engineers a replacement for metadata-reliant search using high-performance computer vision. The system is built on a hybrid microservices architecture: a *.NET 10* backend manages core retail logic, while a *Python/FastAPI* service handles deep learning inference.

This research conducts a comparative analysis of Convolutional Neural Networks (CNN) and Vision Transformers (ViT) for product feature extraction. Experimental results establish that the domain-specific *Fashion-CLIP* model achieved a Mean Average Precision (*mAP\@10*) of *0.725*, significantly outperforming general-purpose baselines. To ensure real-time performance without expensive GPU infrastructure, the system leverages *HNSW indexing* within a *PostgreSQL/pgvector* database. This configuration achieves search latencies under *100ms* on standard CPU hardware. *ReSys.Shop* demonstrates that small and medium-sized retailers can deploy advanced, visually intuitive discovery tools without incurring prohibitive infrastructure costs.

#v(1cm)
#set text(style: "normal")
#par(first-line-indent: 0cm)[
  *Keywords:* #info.en.keywords.join(", ")
]

#pagebreak()

// Vietnamese Abstract (Standard University Style)
#heading(level: 1, numbering: none, outlined: true)[TÓM TẮT]
#v(1cm)
#set align(left)
#set par(first-line-indent: 1cm, justify: true)

Tìm kiếm dựa trên từ khóa trong thương mại điện tử thời trang gặp nhiều hạn chế trong việc nắm bắt các đặc điểm trực quan của sản phẩm, tạo ra "khoảng cách ngữ nghĩa" giữa những gì người dùng thấy và cách họ tìm kiếm. Luận văn này giới thiệu *ReSys.Shop*, một hệ sinh thái thương mại điện tử triển khai giải pháp thay thế tìm kiếm dựa trên siêu dữ liệu bằng các kỹ thuật thị giác máy tính hiệu suất cao. Hệ thống được xây dựng trên kiến trúc vi dịch vụ lai: backend *.NET 10* quản lý logic bán lẻ cốt lõi, trong khi dịch vụ *Python/FastAPI* đảm nhiệm việc suy luận học sâu.

Nghiên cứu thực hiện phân tích so sánh hiệu quả giữa Mạng nơ-ron tích chập (CNN) và Vision Transformer (ViT) trong trích xuất đặc trưng. Kết quả thực nghiệm khẳng định mô hình chuyên biệt *Fashion-CLIP* đạt độ chính xác trung bình trung (*mAP\@10*) là *0,725*. Để đảm bảo hiệu năng thời gian thực mà không cần hạ tầng GPU đắt đỏ, hệ thống sử dụng chỉ mục *HNSW* tích hợp trong cơ sở dữ liệu *PostgreSQL/pgvector*. Cấu hình này cho phép thời gian phản hồi tìm kiếm dưới *100ms* trên phần cứng CPU tiêu chuẩn. *ReSys.Shop* chứng minh rằng các doanh nghiệp vừa và nhỏ hoàn toàn có thể triển khai các công cụ khám phá hình ảnh tiên tiến mà không phải gánh chịu chi phí hạ tầng quá lớn.

#v(1cm)
#set text(style: "normal")
#par(first-line-indent: 0cm)[
  *Từ khóa:* #info.vi.keywords.join(", ")
]

#pagebreak()

