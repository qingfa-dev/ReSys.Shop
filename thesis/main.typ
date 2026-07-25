#import "info.typ": *
#import "template/ctu-styles.typ": ctu-styles
#import "template/i18n.typ": term

#let lang = settings.primary_lang

#show: doc => ctu-styles(doc, lang: lang)

#set document(
  title: info.at(lang).thesis.title,
  author: info.at(lang).student.name,
)

// Front matter (Roman numerals)
#set page(numbering: "i")
#counter(page).update(1)

#import "frontmatter/cover.typ": cover-page
#import "frontmatter/inner-cover.typ": inner-cover-page
#cover-page(lang: lang)
#inner-cover-page(lang: lang)

#include "frontmatter/evaluation.typ"
#include "frontmatter/acknowledgements.typ"
#include "frontmatter/table-of-contents.typ"
#pagebreak()
#include "frontmatter/list-of-figures.typ"
#pagebreak()
#include "frontmatter/list-of-tables.typ"
#pagebreak()
#include "frontmatter/abbreviations.typ"
#include "frontmatter/abstract.typ"

// Main content (Arabic numerals)
#set page(numbering: "1")
#counter(page).update(1)
#set heading(numbering: "1.1.1")

#let part-heading(body) = {
  pagebreak()
  v(2cm)
  heading(level: 1, numbering: none, outlined: true)[#body]
}

// Part 1: Introduction
#part-heading[#term(lang, "part") 1: INTRODUCTION]
#counter(heading).update(1)
#include "chapters/part1-introduction.typ"

// Part 2: Content
#part-heading[#term(lang, "part") 2: THESIS CONTENT]
#counter(heading).step()
#include "chapters/part2-content.typ"

// Part 3: Conclusion
#part-heading[#term(lang, "part") 3: CONCLUSION AND FUTURE WORK]
#counter(heading).step()
#include "chapters/part3-conclusion.typ"

// Back matter
#pagebreak()
#include "backmatter/references.typ"
#pagebreak()
#set page(numbering: none)
#counter(heading).update(0)
#set heading(numbering: "A.1")
#include "backmatter/appendices.typ"
