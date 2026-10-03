// Inner Cover Page
#import "../info.typ": *
#import "../template/i18n.typ": term

#let inner-cover-page(lang: "en") = {
  let data = info.at(lang)

  page(
    margin: (left: 3cm, right: 3cm, top: 2cm, bottom: 2cm),
    numbering: none,
    header: none,
    footer: none,
  )[
    // Blue border box
    #rect(
      width: 100%,
      height: 100%,
      stroke: settings.border_color + 3pt,
      inset: 1.5cm,
    )[
      #set align(center)

      // 1. HEADER (Includes Ministry and Department on Inner Cover)
      #block(width: 100%)[
        #set par(leading: 0.3cm, justify: false)
        #set text(size: 13pt, weight: "bold", tracking: 0.2pt)

        #upper(term(lang, "ministry")) \
        #upper(term(lang, "university")) \
        #upper(term(lang, "college")) \
        #upper(term(lang, "department"))
      ]

      #v(1fr)

      // 2. LOGO
      #image("/images/logos/logo.png", width: 3.5cm)

      #v(1fr)

      // 3. THESIS TYPE
      #text(size: 14pt, weight: "bold")[
        #upper(term(lang, "thesis_type")) \
        #upper(data.thesis.degree) #upper(term(lang, "in_major")) \
        #upper(data.student.major) \
        #v(0.3cm)
        (#upper(data.student.program))
      ]

      #v(1fr)

      // 4. TITLE
      #block(width: 90%)[
        #set par(leading: 0.4cm)
        #text(size: 16pt, weight: "bold")[
          #data.thesis.title
        ]
      ]

      #v(2fr)

      // 5. STUDENT INFO
      #align(center)[
        #grid(
          columns: (auto, auto),
          column-gutter: 1cm,
          row-gutter: 0.5cm,
          align: (right, left),

          text(size: 13pt, weight: "bold")[#term(lang, "student_label")], text(size: 13pt)[#data.student.name],
          text(size: 13pt, weight: "bold")[#term(lang, "student_id_label")], text(size: 13pt)[#data.student.id],
          text(size: 13pt, weight: "bold")[#term(lang, "class_label")], text(size: 13pt)[#data.student.class],
          text(size: 13pt, weight: "bold")[#term(lang, "advisor_label")], text(size: 13pt)[#data.advisor.name],
        )
      ]

      #v(1fr)

      // 6. DATE
      #set align(center)
      #text(size: 13pt)[#data.thesis.location, #data.thesis.date]

      #v(0.5cm)
    ]
  ]
}

