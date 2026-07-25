#import "../info.typ": *
#import "../template/i18n.typ": term

#let lang = settings.primary_lang
#let data = info.at(lang, default: info.en)

#heading(level: 1, numbering: none, outlined: true)[#term(lang, "acknowledgments_title")]
#v(1cm)
#set align(left)
#set par(first-line-indent: 1cm, justify: true)

Dr. Tran Cong An guided this thesis through the Design Science Research (DSR) methodology, from problem identification and artifact design through systematic evaluation, and provided concrete, actionable feedback on the benchmark system architecture, model selection strategy, and experimental protocol. The committee, Dr. Pham The Phi as chairman and Dr. Thai Minh Tuan as reviewer, asked probing questions during the defense that directly sharpened the statistical analysis and trade-off discussion in Chapter 6. The Information Technology faculty at Can Tho University delivered foundational coursework in algorithms, database systems, and software engineering that underpins the implementation. The High-Quality Program's project-oriented curriculum shaped the engineering discipline applied throughout this work. To my family: your patience across the late nights and weekends made every milestone achievable.

#v(1cm)

Em xin gửi lời cảm ơn chân thành đến TS. Trần Công Án, người đã trực tiếp hướng dẫn em từ những bước đầu tiên của đề tài. Thầy đã định hướng phương pháp Nghiên cứu Khoa học Thiết kế (DSR) cho toàn bộ quy trình thực hiện luận văn và đưa ra những góp ý cụ thể, sát thực tế về kiến trúc hệ thống đánh giá hiệu năng mô hình, chiến lược lựa chọn mô hình, và giao thức thực nghiệm. Em cũng xin cảm ơn TS. Phạm Thế Phi, Chủ tịch Hội đồng bảo vệ, và TS. Thái Minh Tuấn, Ủy viên phản biện, đã đặt những câu hỏi sâu sắc trong buổi bảo vệ, giúp em nhìn nhận lại và hoàn thiện phần phân tích ở Chương 6. Những kiến thức nền tảng từ Khoa Công nghệ Thông tin, Trường Công nghệ Thông tin và Truyền thông, Trường Đại học Cần Thơ, đặc biệt trong chương trình Chất lượng cao, đã là hành trang quý giá để em có thể triển khai một hệ thống hoàn chỉnh từ backend .NET, frontend Vue.js đến dịch vụ máy học Python. Cảm ơn gia đình, những người đã âm thầm ủng hộ và tạo mọi điều kiện để em tập trung hoàn thành công việc này.

#v(1cm)
#set align(right)
#set par(first-line-indent: 0cm)

Sincerely,

#v(0.3cm)
#data.thesis.location, #data.thesis.date

#v(1cm)
#data.student.name
#pagebreak()
