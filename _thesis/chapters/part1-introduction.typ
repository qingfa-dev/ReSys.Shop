// Heading moved to main.typ (Part 1)
#set heading(numbering: (..nums) => {
  let values = nums.pos()
  if values.len() == 1 {
    // Level 1: I, II, III, ...
    numbering("I.", ..values)
  } else if values.len() == 2 {
    // Level 2: I, II, III, ...
    numbering("I.", values.at(1))
  } else {
    // Level 3+ (Subsections): No numbering
    none
  }
})

// Hide subsections (Level 3) from the Table of Contents for Part 1
#show heading.where(level: 3): set heading(outlined: false)

#include "part1/01-context.typ"
#include "part1/02-related-work.typ"
#include "part1/03-objectives.typ"
#include "part1/04-research-methods.typ"
#include "part1/05-thesis-outline.typ"

