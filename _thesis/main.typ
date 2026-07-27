// ============================================================================
// CTU GRADUATION THESIS - MAIN FILE
// ============================================================================

// 1. CONFIGURATION & IMPORTS
#import "info.typ": *
#import "template/ctu-styles.typ": ctu-styles
#import "template/i18n.typ": term

// 2. GLOBAL SETTINGS
#let lang = settings.primary_lang

// 3. DOCUMENT SETUP
#show: doc => ctu-styles(doc, lang: lang)

// Document Metadata
#set document(
  title: info.at(lang).thesis.title,
  author: info.at(lang).student.name,
)

// 4. FRONT MATTER (Roman numerals)
#set page(numbering: "i")
#counter(page).update(1)

// Cover Pages
#import "frontmatter/cover.typ": cover-page
#import "frontmatter/inner-cover.typ": inner-cover-page

#cover-page(lang: lang)
#inner-cover-page(lang: lang)

#import "frontmatter/confirmation.typ": confirmation-page
#confirmation-page(lang: lang)

// Front matter pages
#include "frontmatter/evaluation.typ"
#include "frontmatter/acknowledgements.typ"

// Lists (TOC, LOF, LOT)
#include "frontmatter/table-of-contents.typ"
#pagebreak()

#include "frontmatter/list-of-figures.typ"
#pagebreak()

#include "frontmatter/list-of-tables.typ"
#pagebreak()

#include "frontmatter/abbreviations.typ"
#include "frontmatter/abstract.typ"

// 5. MAIN CONTENT (Arabic numerals)
#set page(numbering: "1")
#counter(page).update(1)
#set heading(numbering: "1.1.1.1") // Enable automatic numbering

// Helper for Part Headings (Visual separator, not in Outline)
#let part-heading(body) = {
  pagebreak()
  v(2cm)
  heading(level: 1, numbering: none, outlined: true)[#body]
}

// PART 1: INTRODUCTION
#part-heading[#term(lang, "part") 1: INTRODUCTION]
// Manually set counter to 1 so subsections become 1.1, 1.2...
#counter(heading).update(1)
#include "chapters/part1-introduction.typ"

// PART 2: THESIS CONTENT
#part-heading[#term(lang, "part") 2: THESIS CONTENT]
// Reset counter so that the first chapter in Part 2 starts at 1
#counter(heading).update(0)
#include "chapters/part2-content.typ"


// PART 3: CONCLUSION
#part-heading[#term(lang, "part") 3: CONCLUSION AND FUTURE WORK]
// Step counter for Conclusion to be the next chapter (e.g., 5)
#counter(heading).step()
#include "chapters/part3-conclusion.typ"

// 6. BACK MATTER
// REFERENCES
#include "backmatter/references.typ"

// APPENDICES
#pagebreak()
#set page(numbering: none)
#counter(heading).update(0) // Reset heading counter
#set heading(numbering: "A.1") // Switch to Alphabetical numbering
#include "backmatter/appendices.typ"

