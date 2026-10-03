// Internationalization Dictionary
// Contains static strings and labels used in the template.

#let dict = (
  en: (
    // Organization
    ministry: "MINISTRY OF EDUCATION AND TRAINING",
    university: "CAN THO UNIVERSITY",
    college: "COLLEGE OF INFORMATION AND COMMUNICATION TECHNOLOGY",
    department: "DEPARTMENT OF SOFTWARE ENGINEERING",
    
    // Labels
    thesis_type: "GRADUATION THESIS",
    in_major: "IN",
    student_label: "Student:",
    student_id_label: "Student ID:",
    class_label: "Class:",
    advisor_label: "Advisor:",
    keywords_label: "Keywords:",
    
    // Headings
    figure: "Figure",
    table: "Table",
    toc: "TABLE OF CONTENTS",
    lof: "LIST OF FIGURES",
    lot: "LIST OF TABLES",
    ref: "REFERENCES",
    appendix: "APPENDICES",
    part: "PART",
    chapter: "CHAPTER",
    abbreviations_title: "LIST OF ABBREVIATIONS",
    abbreviations_term: "Abbreviations",
    abbreviations_desc: "Description",
    abstract_title: "ABSTRACT",
    acknowledgments_title: "ACKNOWLEDGEMENTS",
    evaluation_title: "EVALUATION OF ADVISOR",
  ),
  vi: (
    // Organization
    ministry: "BỘ GIÁO DỤC VÀ ĐÀO TẠO",
    university: "ĐẠI HỌC CẦN THƠ",
    college: "TRƯỜNG CÔNG NGHỆ THÔNG TIN VÀ TRUYỀN THÔNG",
    department: "KHOA CÔNG NGHỆ PHẦM MỀM",
    
    // Labels
    thesis_type: "LUẬN VĂN TỐT NGHIỆP",
    in_major: "NGÀNH",
    student_label: "Sinh viên thực hiện:",
    student_id_label: "MSSV:",
    class_label: "Lớp:",
    advisor_label: "Cán bộ hướng dẫn:",
    thesis_title_label: "Tên đề tài:",
    keywords_label: "Từ khóa:",
    
    // Headings
    figure: "Hình",
    table: "Bảng",
    toc: "MỤC LỤC",
    lof: "DANH MỤC HÌNH",
    lot: "DANH MỤC BẢNG",
    ref: "TÀI LIỆU THAM KHẢO",
    appendix: "PHỤ LỤC",
    part: "PHẦN",
    chapter: "CHƯƠNG",
    abbreviations_title: "DANH MỤC TỪ VIẾT TẮT",
    abbreviations_term: "Từ viết tắt",
    abbreviations_desc: "Diễn giải",
    abstract_title: "TÓM TẮT",
    acknowledgments_title: "LỜI CẢM ỞN",
    evaluation_title: "NHẬN XÉT CỦA CÁN BỘ HƯỚNG DẪN",
  ),
)

// Helper function to get a term
#let term(lang, key) = {
  let lang-dict = dict.at(lang, default: dict.en)
  lang-dict.at(key, default: key)
}

