# Use Case Diagrams - README

## Overview

This directory contains all PlantUML source files and generated diagrams for the ReSys.Shop thesis use case documentation.

## Directory Structure

```
usecases/
├── sources/               # PlantUML source files (.puml)
│   ├── customer/          # Customer-facing use cases (UC-0001 to UC-0008)
│   ├── admin/             # Administrator use cases (UC-0009 to UC-0022)
│   ├── system/            # System/background use cases (UC-0017 to UC-0020)
│   ├── _shared/           # Shared style and actor definitions
│   │   ├── styles.iuml    # Color palette and typography
│   │   └── actors.iuml    # Actor definitions
│   └── generate-all.ps1   # Batch generation script
├── generated/             # Generated PNG/SVG outputs
│   ├── customer/
│   ├── admin/
│   └── system/
└── specifications/        # Markdown specifications
    ├── customer/
    ├── admin/
    └── system/
```

## Style Standards

All diagrams follow UML 2.5 standards with consistent visual styling:

- **Color Palette**: Defined in `_shared/styles.iuml`
  - Core research features: Light green (#E8F5E9)
  - Supporting features: Light blue (#E3F2FD)
  - System features: Light gray (#F5F5F5)
  
- **Typography**: Segoe UI font family
- **Relationships**: Only «include» and «extend» stereotypes (UML 2.5 compliant)

## Generating Diagrams

### Prerequisites
- PlantUML installed and in PATH
- Java Runtime Environment (for PlantUML)

### Generate All Diagrams
```powershell
cd sources
.\generate-all.ps1
```

### Generate Specific Category
```powershell
.\generate-all.ps1 -Category customer
```

### Generate SVG Format
```powershell
.\generate-all.ps1 -OutputFormat svg
```

## Use Case Categories

### Customer Use Cases (8)
- UC-0001: Browse Products and Categories
- UC-0002: Multi-step Checkout ✓ Fixed «precondition» error
- UC-0003: Keyword Search ✓ Fixed «use» error
- UC-0004: Visual Search (Core Research)
- UC-0005: Manage Shopping Cart
- UC-0006: Track Order Status ✓ Fixed «trace» error
- UC-0007: Manage Address Book ✓ Fixed «use» error
- UC-0008: View Recommendations (Core Research)

### Admin Use Cases (8)
- UC-0009: Manage Product Catalog
- UC-0010: Upload Product Images
- UC-0011: Manage Taxonomy
- UC-0012: Manage Inventory
- UC-0013: View Analytics
- UC-0014: Order Fulfillment
- UC-0016: User Management
- UC-0022: System Configuration

### System Use Cases (4)
- UC-0017: Generate Embeddings (Core Research)
- UC-0018: Stock Reservations
- UC-0019: Update Vector Index (Core Research)
- UC-0020: Background Jobs

## UML Compliance

All diagrams comply with UML 2.5 standard:

✅ **Allowed Relationships:**
- Association (solid line): Actor to Use Case
- «include» (dashed arrow): Mandatory sub-functionality
- «extend» (dashed arrow): Optional extension
- Generalization (solid line with triangle): Inheritance

❌ **Prohibited (Non-standard):**
- «precondition» - Not a UML stereotype
- «use» - Deprecated (UML 1.x only)
- «trace» - For requirements traceability, not use cases
- «calls», «invokes» - Not standard

## Validation Checklist

Before finalizing diagrams, ensure:

- [ ] Only «include» and «extend» stereotypes used
- [ ] Correct relationship directionality
- [ ] Consistent color usage per feature type
- [ ] Proper actor-use case associations
- [ ] System boundary labeled correctly
- [ ] Notes provide necessary context
- [ ] PNG generated at correct resolution (1200x800 standard)
- [ ] Business logic matches codebase implementation

## References

- [UML 2.5 Specification](https://www.omg.org/spec/UML/2.5/)
- [PlantUML Use Case Guide](https://plantuml.com/use-case-diagram)
- Thesis Advisor Feedback (2026-02-03)

---

**Last Updated**: 2026-02-03  
**Status**: Phase 1 Complete - Infrastructure Ready
