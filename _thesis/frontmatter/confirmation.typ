// Confirmation of Revision Page (Vietnamese Template)
#import "../info.typ": *

#let confirmation-page(lang: "vi") = {
  page(
    margin: (left: 3cm, right: 2cm, top: 2cm, bottom: 2cm),
    header: none,
    footer: none,
  )[
    // --- 1. Header (Administrative Layout) ---
    #grid(
      columns: (1fr, 1.2fr),
      align: (center, center),
      gutter: 1em,
      [
        #text(size: 11pt)[TRƯỜNG ĐẠI HỌC CẦN THƠ] \
        #text(size: 11pt, weight: "bold")[TRƯỜNG CÔNG NGHỆ THÔNG TIN \ VÀ TRUYỀN THÔNG] \
        #text(size: 11pt, weight: "bold")[KHOA CÔNG NGHỆ THÔNG TIN]
        #v(-0.8em)
        #line(length: 40%, stroke: 0.5pt)
      ],
      [
        #text(size: 11pt, weight: "bold")[CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM] \
        #text(size: 12pt, weight: "bold")[Độc lập – Tự do – Hạnh phúc]
        #v(-0.8em)
        #line(length: 40%, stroke: 0.5pt)
      ],
    )

    #v(1.0cm)

    // --- 2. Title ---
    #align(center)[
      #text(size: 15pt, weight: "bold")[XÁC NHẬN CHỈNH SỬA LUẬN VĂN] \
      #text(size: 15pt, weight: "bold")[THEO YÊU CẦU CỦA HỘI ĐỒNG]
    ]

    #v(0.8cm)

    // --- 3. Student & Thesis Info ---
    #text(size: 13pt)[
      Tên luận văn (tiếng Việt và tiếng Anh): \
      #v(0.5em)
      #par(justify: true)[Building a Fashion Ecommerce Application with Recommendation and Image-based Product Search]
      #v(0.3em)
      #par(
        justify: true,
      )[Phát triển ứng dụng thương mại điện tử bán thời trang tích hợp gợi ý và tìm kiếm sản phẩm bằng hình ảnh]

      #v(1em)

      #grid(
        columns: (auto, 1fr, auto, auto),
        gutter: 0.5em,
        [Họ tên sinh viên:], [Nguyễn Thanh Phát], [MASV:], [B2005853],
      )

      #v(0.5em)
      Mã lớp: DI20V7F1

      #v(0.5em)
      Đã báo cáo tại hội đồng ngành: Công nghệ Thông tin

      #v(0.5em)
      // Hardcoded date format per user request/example if needed, or use info
      Ngày báo cáo: 24/12/2025

      #v(0.5em)
      Hội đồng báo cáo gồm:
      #v(0.5em)
      #grid(
        columns: (30pt, 2fr, 1fr),
        row-gutter: 1em,
        [1.], [TS. Phạm Thế Phi], [],
        [2.], [TS. Thái Minh Tuấn], [],
        [3.], [TS. Trần Công Án], [],
      )
    ]

    #v(1cm)

    // --- 4. Confirmation Statement ---
    #text(size: 13pt)[
      Luận văn đã được chỉnh sửa theo góp ý của Hội đồng.
    ]

    #v(1.5cm)

    // --- 5. Footer / Signature ---
    #align(right)[
      #text(size: 13pt, style: "italic")[
        Cần Thơ, ngày ..... tháng ..... năm 2026
      ]

      #v(0.5em)

      #text(size: 13pt, weight: "bold")[Giáo viên hướng dẫn] \
      #text(size: 12pt, style: "italic")[(Ký và ghi họ tên)]

      #v(3cm) // Space for signature

      // #text(size: 13pt, weight: "bold")[TS. Trần Công Án]
    ]
  ]
}
