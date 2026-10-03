// Acknowledgements
#import "../info.typ": *
#import "../template/i18n.typ": term

#let lang = settings.primary_lang
#let data = info.at(lang)

#heading(level: 1, numbering: none, outlined: true)[#term(lang, "acknowledgments_title")]
#v(1cm)
#set align(left)
#set par(first-line-indent: 1cm, justify: true)

#if lang == "vi" [
  Em xin bày tỏ lòng biết ơn sâu sắc nhất và lời cảm ơn chân thành đến cán bộ hướng dẫn, #data.advisor.name, người đã không chỉ tận tình chỉ dẫn mà còn dành rất nhiều thời gian, tâm huyết để hỗ trợ và dẫn dắt em vượt qua những giai đoạn thử thách nhất của luận văn này. Em cũng xin gửi lời tri ân đến quý thầy cô Trường Công nghệ Thông tin và Truyền thông - Đại học Cần Thơ đã truyền thụ những kiến thức quý báu trong suốt những năm học qua.

  Đặc biệt, con xin gửi lời cảm ơn vô hạn đến gia đình, những người đã luôn kề vai sát cánh, đồng hành và là chỗ dựa vững chắc nhất để con có thể hoàn thành tốt chặng đường này. Em cũng xin cảm ơn những người bạn đã luôn khích lệ và đồng hành cùng em trong suốt thời gian học tập và thực hiện đề tài.
] else [
  I wish to express my deepest gratitude and sincere thanks to my advisor, #data.advisor.name, who dedicated immense time, patience, and effort to guide me through every challenge of this thesis. His invaluable support was the cornerstone of this work. I would also like to thank the lecturers at Can Tho University for their invaluable knowledge and guidance throughout my studies.

  Most importantly, I am profoundly grateful to my family for being my constant companions, offering their unwavering love, care, and support throughout this journey. I am also thankful to my friends for their continuous encouragement and companionship.
]

#v(1cm)
#set align(right)
#set par(first-line-indent: 0cm)

#if lang == "vi" [
  Trân trọng,
] else [
  Sincerely,
]

#data.thesis.location, #data.thesis.date

#v(1cm)
#data.student.name
#pagebreak()
