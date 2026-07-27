# CTU Graduation Thesis - Typst Template

## Quick Start

1. Install Typst: https://github.com/typst/typst/releases
2. Edit `main.typ` with your information
3. Compile: `typst compile main.typ`
4. Or watch: `typst watch main.typ`

## Structure

```
├── main.typ                  # Main file - START HERE
├── template/                 # Core template (don't modify)
├── frontmatter/             # Front matter pages
├── chapters/                # Your thesis content
│   ├── part1/              # Introduction sections
│   ├── chapter1/           # Background sections
│   ├── chapter2/           # Design sections
│   ├── chapter3/           # Testing sections
│   └── part3/              # Conclusion sections
├── images/                  # All figures
├── appendices/             # Appendices
└── references.bib          # Bibliography

## Writing Guide

1. Edit your info in `main.typ` (lines 7-17)
2. Start writing from Part 1: `chapters/part1/01-context.typ`
3. Add images to `images/` folder
4. Add references to `references.bib`
5. Compile frequently to see progress

## Need Help?

- Typst Documentation: https://typst.app/docs
- CTU Thesis Guidelines: Check with your department

Good luck! 🎓
