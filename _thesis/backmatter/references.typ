// Reference to the bibliography file
#import "../info.typ": *
#import "../template/i18n.typ": term

#let lang = settings.primary_lang

#pagebreak()
#bibliography(
  "bibliography.bib", 
  title: term(lang, "ref"),
  style: "ieee"
)
