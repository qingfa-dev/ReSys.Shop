// ============================================================================ 
// CTU THESIS TEMPLATE - REFINED 
// template/ctu-thesis.typ 
// ============================================================================ 

#import "i18n.typ": term 
#import "ctu-styles.typ": * 

// ---------------------------------------------------------------------------- 
// Cover Page Component 
// ---------------------------------------------------------------------------- 
#let cover-page( 
  info: (:), 
  lang: "en", 
  has-department: false, 
) = { 
  
  set page(numbering: none, margin: 2.5cm) 
  set align(center) 
  
  v(0.5cm) 
  
  text(size: 13pt)[#term(lang, "university")] 
  linebreak() 
  text(size: 13pt, weight: "bold")[#upper(info.college)] 
  
  if has-department and info.department != none { 
    linebreak() 
    text(size: 13pt, weight: "bold")[#upper(info.department)] 
  } 
  
  v(1cm) 
  
  circle( 
    radius: 1.5cm, 
    stroke: 2pt + black, 
    fill: rgb("#003366"), 
  )[ 
    #text(fill: white, size: 16pt, weight: "bold")[CTU] 
  ] 
  
  v(1cm) 
  
    text(size: 13pt, weight: "bold")[#term(lang, "thesis_type")]
  
    linebreak()
  
    v(0.3cm)
  
    text(size: 13pt, weight: "bold")[#upper(info.degree) #term(lang, "in_major")]
  
    linebreak()
  
    text(size: 13pt, weight: "bold")[#upper(info.major)]
  
   
  linebreak() 
  text(size: 12pt, weight: "bold")[(#upper(info.program))] 
  
  v(1.5cm) 
  
  text(size: 14pt, weight: "bold")[ 
    #upper(info.title) 
  ] 
  
  v(1fr) 
  
  // Localized labels 
  let l-student = term(lang, "student") 
  let l-id = term(lang, "student_id") 
  let l-class = term(lang, "class") 
  let l-advisor = term(lang, "advisor") 

  grid( 
    columns: (1fr, 1fr), 
    gutter: 1cm, 
    align: (left + horizon, left + horizon), 
    [ 
      #text(size: 12pt)[#l-student #info.student-name] \ 
      #text(size: 12pt)[#l-id #info.student-id] \ 
      #text(size: 12pt)[#l-class #info.student-class] 
    ], 
    [ 
      #text(size: 12pt)[#l-advisor #info.advisor-name] 
    ], 
  ) 
  
  v(1cm) 
  
  text(size: 13pt)[#info.submission-date] 
  
  pagebreak() 
} 

// ---------------------------------------------------------------------------- 
// Main Template Function 
// ---------------------------------------------------------------------------- 
#let ctu-thesis( 
  // Information dictionary or individual fields 
  title-main: "", 
  student-name: "", 
  student-id: "", 
  student-class: "", 
  advisor-name: "", 
  degree: "", 
  major: "", 
  program: "", 
  college: "", 
  department: "", 
  submission-date: "", 
  
  // Settings 
  lang: "en", 
  bibliography-file: none, 
  
  body, 
) = { 
  
  // Construct the info object 
  let info = ( 
    title: title-main, 
    student-name: student-name, 
    student-id: student-id, 
    student-class: student-class, 
    advisor-name: advisor-name, 
    degree: degree, 
    major: major, 
    program: program, 
    college: college, 
    department: department, 
    submission-date: submission-date, 
  ) 

  // Document Metadata 
  set document(
    title: title-main, 
    author: student-name, 
  ) 

  // Apply Styles 
  // Now ctu-styles takes `doc` and wraps it. 
  show: doc => ctu-styles(doc, lang: lang) 

  // Cover Pages 
  cover-page(info: info, lang: lang, has-department: false) 
  cover-page(info: info, lang: lang, has-department: true) 
  
  body 
} 

// ---------------------------------------------------------------------------- 
// Utilities (Exported for main.typ usage) 
// ---------------------------------------------------------------------------- 
#let part-heading(body) = { 
  pagebreak() 
  set align(center) 
  set text(size: 14pt, weight: "bold") 
  v(2cm) 
  upper(body) 
  v(1cm) 
}

