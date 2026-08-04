// Styles for CTU thesis, extracted from `ctu-thesis.typ`
// Usage: show: doc => ctu-styles(doc, lang: "en")

#import "i18n.typ": term
#import "../info.typ": info

#let ctu-styles(
  doc,
  lang: "en",
) = {
  // Localized header content directly from info.typ
  let header-title = info.at(lang).thesis.short_title

  // Page geometry and defaults
  set page(
    paper: "a4",
    margin: (top: 2.5cm, bottom: 2.5cm, left: 3cm, right: 2cm),

    // Header: Title (Left) + Line
    header: context {
      // Show header only for main content (Arabic numbering "1")
      if page.numbering == "1" {
        set text(size: 9pt, style: "italic")
        grid(
          columns: (1fr, auto),
          gutter: 1em,
          align: (left + bottom, right + bottom),
          block(width: 100%, clip: true, height: 1.2em)[#upper(header-title)],
        )
        v(-5pt)
        line(length: 100%, stroke: 0.5pt)
      }
    },

    // Footer: Line + 0.1cm + Numbering
    footer: context {
      if page.numbering != none {
        set align(center)
        line(length: 100%, stroke: 0.5pt)
        v(0.1cm)
        text(size: 11pt)[#counter(page).display(page.numbering)]
      }
    },
  )

  // Main text settings
  set text(
    font: "Times New Roman",
    size: 13pt,
    lang: lang,
  )

  // Paragraph settings
  set par(
    justify: true,
    leading: 0.25cm,
    first-line-indent: 1cm,
  )

  // Headings
  show heading.where(level: 1): it => {
    set align(center)
    set text(size: 14pt, weight: "bold")

    // Check if it's a numbered chapter (not Part heading)
    if it.numbering != none {
      // Get the chapter number
      let num = counter(heading).display(it.numbering)
      upper(term(lang, "chapter"))
      [ ]
      upper(num)
      linebreak()
    }

    upper(it.body)
    v(12pt)
  }

  show heading.where(level: 2): it => {
    set text(size: 13pt, weight: "bold")
    {
      show text: upper
      it
    }
    v(0.3cm)
  }

  show heading.where(level: 3): it => {
    set text(size: 13pt, weight: "bold")
    v(0.15cm)
    it
    v(0.15cm)
  }

  // Figures & tables
  show figure.where(kind: image): it => {
    block(breakable: false)[
      #set align(center)
      #it.body
      #v(0.1cm)
      #set text(size: 12pt)
      #if it.caption != none {
        [#term(lang, "figure") #it.counter.display(it.numbering). #it.caption.body]
      }
      #v(0.1cm)
    ]
  }

  show figure.where(kind: table): it => {
    block(breakable: false)[
      #set align(center)
      #v(0.1cm)
      #set text(size: 11pt)
      #if it.caption != none {
        [#term(lang, "table") #it.counter.display(it.numbering). #it.caption.body]
      }
      // #v(0.1cm)
      #it.body
      #v(0.1cm)
    ]
  }

  // Raw blocks
  show raw.where(block: true): it => {
    set text(size: 10pt, font: "Courier New")
    block(
      fill: rgb("#f5f5f5"),
      inset: 0.3cm,
      radius: 0.1cm,
      width: 100%,
      it,
    )
  }

  doc
}

// Helper for missing figures
#let figure-placeholder(title) = {
  rect(
    width: 100%,
    height: 150pt,
    fill: luma(245),
    stroke: (paint: gray, thickness: 1pt, dash: "dashed"),
    radius: 5pt,
  )[
    #set align(center + horizon)
    #set text(fill: gray, size: 11pt)
    [PLACEHOLDER: #upper(title)] \
    #v(5pt)
    (Replace this with the actual diagram or screenshot)
  ]
}


