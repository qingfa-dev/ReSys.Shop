// Table of Contents
#import "../template/i18n.typ": term

#heading(level: 1, numbering: none, outlined: true)[TABLE OF CONTENTS]
#v(1cm)

#show outline.entry.where(level: 1): it => {
  let el = it.element
  
  if el.numbering == none {
    // For unnumbered entries (Abstract, List of Figures, etc.)
    // We rely on 'it' to handle the page number formatting correctly
    v(12pt, weak: true)
    strong(upper(it))
  } else {
    // For numbered entries (Chapters, Appendices)
    // We reconstruct the line to add "CHAPTER" prefix and ensure single-line layout
    v(12pt, weak: true)
    set text(weight: "bold")
    
    let prefix = if el.numbering == "A.1" { "APPENDIX " } else { "CHAPTER " }
    let num = numbering(el.numbering, ..counter(heading).at(el.location()))
    
    // Get page number from counter since 'it.page' might be missing
    // We assume Arabic numbering for chapters/appendices
    let page-num = counter(page).at(el.location()).first()
    
    link(el.location())[
      #upper[#prefix #num. #el.body]
      #box(width: 1fr, repeat[.])
      #page-num
    ]
  }
}

#outline(
  title: none, 
  indent: auto, 
  depth: 3
)

