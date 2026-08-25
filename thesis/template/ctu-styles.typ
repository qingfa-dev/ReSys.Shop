// ============================================================================
// CTU THESIS STYLES
// Based on CTU Format Guidelines (2025-2026)
// Fonts: Times New Roman 13pt
// Margins: Left 4cm, Others 2.5cm
// Line Spacing: 1.2 for main text
// ============================================================================

#import "i18n.typ": term
#import "../info.typ": info, settings

#let ctu-styles(
  doc,
  lang: "en",
) = {
  
  // Localized header content
  let header-title = info.at(lang, default: info.en).thesis.short_title
  
  // Page geometry (CTU Standard)
  set page(
    paper: "a4",
    margin: settings.format.margins,
    
    // Header: Title + Line (only for main content)
    header: context {
      if page.numbering == "1" {
        set text(size: 9pt, style: "italic")
        grid(
          columns: (1fr),
          align: (left+bottom),
          block(width: 100%, clip: true, height: 1.2em)[#upper(header-title)],
        )
        v(-5pt) 
        line(length: 100%, stroke: 0.5pt)
      }
    },

    // Footer: Line + Page Number
    footer: context {
      if page.numbering != none {
        set align(center)
        line(length: 100%, stroke: 0.5pt)
        v(0.1cm)
        text(size: 11pt)[#counter(page).display(page.numbering)]
      }
    }
  )
  
  // Main text settings (CTU Standard: Times New Roman, 13pt)
  set text(
    font: settings.format.font,
    size: settings.format.font_size,
    lang: lang,
  )
  
  // Paragraph settings (CTU Standard: 1.2 spacing, 1cm indent)
  set par(
    justify: true,
    leading: 0.25cm,  // 1.2 line spacing ≈ 0.25cm
    first-line-indent: settings.format.paragraph_indent,
  )

  // Headings (CTU Format)
  show heading.where(level: 1): it => {
    set align(center)
    set text(size: 14pt, weight: "bold")
    {
      show text: upper
      it
    }
    v(12pt)
  }
  
  show heading.where(level: 2): it => {
    set text(size: 13pt, weight: "bold")
    {
      show text: upper
      it
    }
    v(0.4cm)
  }
  
  show heading.where(level: 3): it => {
    set text(size: 13pt, weight: "bold")
    v(0.2cm)
    it
    v(0.2cm)
  }
  
  // Figures (CTU Format: Single line spacing, centered)
  show figure.where(kind: image): it => {
    set align(center)
    it.body
    v(0.2cm)
    set text(size: 12pt)
    set par(leading: 0.15cm) // Single spacing for captions
    [#it.caption]
    v(0.4cm)
  }
  
  // Tables: caption above with Typst-managed numbering
  show figure.where(kind: table): it => {
    set align(center)
    set block(breakable: true)

    set text(size: 12pt)
    set par(leading: 0.15cm)

    v(0.15cm)
    
    // Caption
    it.caption

    v(0.15cm)

    // Table
    it.body

    v(0.4cm)
  }
    
  // Code blocks
  show raw.where(block: true): it => {
    set text(size: 9pt, font: "Courier New")
    set par(leading: 0.1cm) // Single spacing for code
    block(
      fill: rgb("#f5f5f5"),
      inset: 0.2cm,
      radius: 0.1cm,
      width: 100%,
      it
    )
  }

  // Inline code (Courier New, slightly smaller to sit in Times New Roman body)
  show raw.where(block: false): it => {
    set text(font: "Courier New", size: 0.9em)
    it
  }

  doc
}
