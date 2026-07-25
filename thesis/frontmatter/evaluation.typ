#import "../info.typ": *
#import "../template/i18n.typ": term

#let lang = settings.primary_lang
#let data = info.at(lang, default: info.en)

#set page(numbering: "i")
#heading(level: 1, numbering: none, outlined: true)[#term(lang, "evaluation_title")]
#v(1cm)
#set align(left)

#line(length: 100%, stroke: (paint: black, thickness: 1pt, dash: "dotted"))
#v(0.8cm)

#line(length: 100%, stroke: (paint: black, thickness: 1pt, dash: "dotted"))
#v(0.8cm)

#line(length: 100%, stroke: (paint: black, thickness: 1pt, dash: "dotted"))
#v(0.8cm)

#line(length: 100%, stroke: (paint: black, thickness: 1pt, dash: "dotted"))
#v(0.8cm)

#line(length: 100%, stroke: (paint: black, thickness: 1pt, dash: "dotted"))
#v(0.8cm)

#line(length: 100%, stroke: (paint: black, thickness: 1pt, dash: "dotted"))
#v(0.8cm)

#line(length: 100%, stroke: (paint: black, thickness: 1pt, dash: "dotted"))

#v(2cm)
#set align(right)
#text(size: 13pt)[
  #term(lang, "advisor_label")
  #v(1.5cm)
  #line(length: 6cm) \
  #data.advisor.name
]
#pagebreak()
