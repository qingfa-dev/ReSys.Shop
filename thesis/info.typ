// ============================================================================
// CTU THESIS INFORMATION CONFIGURATION
// Can Tho University - College of Information and Communication Technology
// ============================================================================

#let info = (
  en: (
    student: (
      name: "Nguyen Thanh Phat",
      id: "B220001",
      class: "DI2296A1",
      major: "INFORMATION TECHNOLOGY",
      program: "High-Quality Program",
    ),
    advisor: (
      name: "TS. Tran Thi B",
      title: "TS",
    ),
    thesis: (
      title: "He thong Thuong mai Dien tu voi Tim kiem Anh Cong nghe",
      short_title: "He thong TMĐT",
      date: "July 2026",
      location: "Can Tho",
      degree: "BACHELOR OF ENGINEERING",
    ),
    keywords: (
      "e-commerce",
      "content-based image retrieval",
      "modular monolith",
      "fashion retrieval",
      "embedding models",
    ),
    committee: (
      chairman: "Dr. Chairman Name",
      reviewer: "Dr. Reviewer Name",
      advisor: "TS. Tran Thi B",
    ),
    abbreviations: (
      ("API", "Application Programming Interface"),
      ("CTU", "Can Tho University"),
      ("ICT", "Information and Communication Technology"),
      ("UI/UX", "User Interface/User Experience"),
      ("HTTP", "Hypertext Transfer Protocol"),
      ("CBIR", "Content-Based Image Retrieval"),
      ("CQRS", "Command Query Responsibility Segregation"),
      ("MediatR", "Mediator library for .NET"),
      ("EF Core", "Entity Framework Core"),
      ("JWT", "JSON Web Token"),
      ("pgvector", "PostgreSQL vector extension"),
    ),
  ),
  vi: (
    student: (
      name: "Nguyen Thanh Phat",
      id: "B220001",
      class: "DI2296A1",
      major: "CONG NGHE THONG TIN",
      program: "Chat luong cao",
    ),
    advisor: (
      name: "TS. Tran Thi B",
      title: "TS",
    ),
    thesis: (
      title: "He thong Thuong mai Dien tu voi Tim kiem Anh Cong nghe",
      short_title: "He thong TMĐT",
      date: "Thang 07/2026",
      location: "Can Tho",
      degree: "KY SU",
    ),
    keywords: (
      "thuong mai dien tu",
      "tim kiem anh cong nghe",
      "kien truc don mo",
      "tim kiem anh thoi trang",
      "mo hinh embedding",
    ),
    committee: (
      chairman: "TS. Ten Chu Tich",
      reviewer: "TS. Ten Phan Bien",
      advisor: "TS. Tran Thi B",
    ),
    abbreviations: (
      ("API", "Giao dien lap trinh ung dung"),
      ("CTU", "Dai hoc Can Tho"),
      ("CNTT-TT", "Cong nghe Thong tin va Truyen thong"),
      ("UI/UX", "Giao dien/Trai nguoi dung"),
      ("HTTP", "Giao thuc truyen tai sieu van ban"),
      ("CBIR", "Tim kiem anh cong nghe"),
      ("CQRS", "Phan cach lenh truy van"),
      ("MediatR", "Thu vien trung gian cho .NET"),
      ("EF Core", "Entity Framework Core"),
      ("JWT", "JSON Web Token"),
      ("pgvector", "Phan mo rong vector cua PostgreSQL"),
    ),
  ),
)

// ============================================================================
// GLOBAL SETTINGS (CTU STANDARD — Decision 4125/QĐ-ĐHCT 2024)
// ============================================================================
#let settings = (
  primary_lang: "en",

  // CTU Official Colors
  border_color: rgb(0, 51, 153), // CTU Blue (#003399)
  accent_color: rgb(0, 83, 159), // CTU Accent (#00539F)

  // CTU Format Requirements (2025-2026)
  format: (
    font: "Times New Roman",
    font_size: 13pt,
    line_spacing: 1.2,
    margins: (
      left: 4cm,
      right: 2.5cm,
      top: 2.5cm,
      bottom: 2.5cm,
    ),
    paragraph_indent: 1cm,
    abstract_words: (200, 350),
  ),
)
