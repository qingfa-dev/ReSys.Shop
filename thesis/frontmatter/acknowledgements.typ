#import "../info.typ": *
#import "../template/i18n.typ": term

#let lang = settings.primary_lang
#let data = info.at(lang, default: info.en)

#heading(level: 1, numbering: none, outlined: true)[#term(lang, "acknowledgments_title")]
#v(1cm)
#set align(left)
#set par(first-line-indent: 1cm, justify: true)

Dr. Tran Cong An guided this thesis through the Design Science Research (DSR) methodology \u2014 from problem identification and artifact design through systematic evaluation \u2014 and provided concrete, actionable feedback on the benchmark system architecture, model selection strategy, and experimental protocol. The committee \u2014 Dr. Pham The Phi as chairman and Dr. Thai Minh Tuan as reviewer \u2014 asked probing questions during the defense that directly sharpened the statistical analysis and trade-off discussion in Chapter 6. The Information Technology faculty at Can Tho University delivered foundational coursework \u2014 algorithms, database systems, and software engineering \u2014 that underpins the implementation. The High-Quality Program\u2019s project-oriented curriculum shaped the engineering discipline applied throughout this work. To my family: your patience across the late nights and weekends made every milestone achievable.

#v(1cm)

Em xin gửi lời cảm ơn chân thành đến TS. Trần Công Án \u2014 người đã trực tiếp hướng dẫn em từ những bước đầu tiên của đề tài. Thầy đã định hướng phương pháp Nghiên cứu Khoa học Thiết kế (DSR) cho toàn bộ quy trình thực hiện luận văn và đưa ra những góp ý cụ thể, sát thực tế về kiến trúc hệ thống đánh giá hiệu năng mô hình, chiến lược lựa chọn mô hình, và giao thức thực nghiệm. Em cũng xin cảm ơn TS. Phạm Thế Phi \u2014 Chủ tịch Hội đồng bảo vệ \u2014 và TS. Thái Minh Tuấn \u2014 Ủy viên phản biện \u2014 đã đặt những câu hỏi sâu sắc trong buổi bảo vệ, giúp em nhìn nhận lại và hoàn thiện phần phân tích ở Chương 6. Những kiến thức nền tảng từ Khoa Công nghệ Thông tin, Trường Công nghệ Thông tin và Truyền thông, Trường Đại học Cần Thơ \u2014 đặc biệt trong chương trình Chất lượng cao \u2014 đã là hành trang quý giá để em có thể triển khai một hệ thống hoàn chỉnh từ backend .NET, frontend Vue.js đến dịch vụ máy học Python. Cảm ơn gia đình \u2014 những người đã âm thầm ủng hộ và tạo mọi điều kiện để em tập trung hoàn thành công việc này.

#v(1cm)
#set align(right)
#set par(first-line-indent: 0cm)

Sincerely,

#v(0.3cm)
#data.thesis.location, #data.thesis.date

#v(1cm)
#data.student.name
#pagebreak()
