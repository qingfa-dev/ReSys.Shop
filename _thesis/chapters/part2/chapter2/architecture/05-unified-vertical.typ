==== Unified Vertical Architecture <sec:architecture-vsa>

Unlike traditional layered architectures that separate concerns horizontally (Presentation, Logic, Data), ReSys.Shop adopts a *Vertical Slice Architecture (VSA)*. This approach groups all concerns related to a single business capability (e.g., "Place Order") into a cohesive, self-contained unit.

#figure(
  placement: none,
  image("../../../../images/diagrams/02-system-architecture/sys-02-vertical-slice-architecture.png", width: 65%),
  caption: [Vertical Slice Architecture: Organizing code by Features rather than Technical Layers],
) <fig:sys-02-vsa>

#figure(
  placement: none,
  table(
    stroke: 0.5pt,
    align: (center, left),
    [*Concept*],
    [*Implementation in ReSys.Shop*],
    [*Slice*],
    [A distinct feature (e.g., `SearchProducts`). Contains its own input (DTO), logic (Handler), and specific data access queries.],

    [*Coupling*],
    [Low coupling between slices. A change in the "Ordering" slice does not affect the "Catalog" slice, preventing regression bugs.],

    [*Shared Kernel*],
    [Common infrastructure like Middleware, Auth, and Validation Behaviors (`MediatR`) are shared across all slices.],
  ),
  caption: [Vertical Slice Architecture Concepts],
)
