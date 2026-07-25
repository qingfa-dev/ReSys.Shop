#import "../info.typ": *
#import "../template/i18n.typ": term

#let lang = settings.primary_lang
#let data = info.at(lang, default: info.en)

#heading(level: 1, numbering: none, outlined: true)[#term(lang, "acknowledgments_title")]
#v(1cm)
#set align(left)
#set par(first-line-indent: 1cm, justify: true)

I would like to express my sincere gratitude to Dr. Tran Cong An, my thesis advisor, for his guidance, continuous feedback, and support throughout this research. His input on the Design Science Research methodology, benchmark system architecture, and experimental design was instrumental.

I also thank the Chair of the Thesis Defense Committee, Dr. Pham The Phi, and the Committee Reviewer, Dr. Thai Minh Tuan, for their time, thorough evaluation, and constructive comments that strengthened the statistical analysis and discussion in this thesis.

My sincere appreciation goes to the Faculty of Information Technology, Can Tho University, for providing the academic foundation that supported this work, and to the High-Quality Program for fostering the engineering discipline applied throughout this thesis.

Finally, I am deeply grateful to my family for their constant encouragement, patience, and unwavering support throughout my studies and the completion of this work.

#v(1cm)
#set align(right)
#set par(first-line-indent: 0cm)

Sincerely,

#v(0.3cm)
#data.thesis.location, #data.thesis.date

#v(1cm)
#data.student.name
#pagebreak()
