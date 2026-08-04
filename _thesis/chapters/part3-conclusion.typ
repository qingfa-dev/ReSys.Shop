// Custom numbering for Part 3 (Roman numerals I, II...)
#set heading(numbering: (..nums) => {
  let values = nums.pos()
  if values.len() == 1 {
    // Level 1: I, II, III...
    numbering("I.", ..values)
  } else if values.len() == 2 {
    // Level 2: I, II, III... (Using the second digit)
    numbering("I.", values.at(1))
  } else {
    // Level 3+: No numbering
    none
  }
})

// Reset counter so Conclusion starts at "I"
#counter(heading).update(0)

#include "part3/01-conclusion.typ"
#include "part3/02-future-work.typ"
