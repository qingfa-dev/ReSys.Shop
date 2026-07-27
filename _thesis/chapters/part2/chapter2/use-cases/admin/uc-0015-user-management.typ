#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/admin/uc-0015-user-management.png", width: 100%),
  caption: [Use Case Diagram for UC-0015],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0015*], [*User Management*],
    [Actor], [Administrator],
    [Description], [Manage user accounts and roles.],
    [Preconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Admin has User Management permissions.],
    ),

    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [Admin views User List.],
      [2], [Selects User to Edit roles/permissions.],
      [3], [System updates User Claims.],
    ),

    [Postconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [User permissions updated.],
    ),

    [Related Use Cases], [],
  ),
  caption: [UC-0015: User Management],
)

This use case involves the governance of system access, allowing a Super Administrator to assign roles and permissions to other staff members. By controlling Claims (e.g., "CatalogEditor", "InventoryManager"), the system implements the Principle of Least Privilege, ensuring that employees can only access the features relevant to their specific job functions.
