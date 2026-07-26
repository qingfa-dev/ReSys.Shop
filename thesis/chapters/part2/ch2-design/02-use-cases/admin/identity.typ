==== User Management

// Diagram placeholder: User Management use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-ADM-IDN-01], [Manage users], [Administrator],
    [Create user accounts. Update profile fields. Enable or disable accounts. View user detail including assigned roles and permissions.],
    [User account created or modified. Status changes take effect on next authentication.],
    [IDN-FR-01, IDN-FR-09, IDN-FR-13],
  ),
  caption: [Administrator use cases — User Management.],
)

==== Role and Permission Governance

// Diagram placeholder: Role and Permission use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-ADM-IDN-02], [Manage roles], [Administrator],
    [Create, update, or remove roles. Assign or revoke permissions per role. List roles with paging.],
    [Role configuration updated. Users assigned to modified roles receive updated permission sets.],
    [IDN-FR-07, IDN-FR-11],
    [UC-ADM-IDN-03], [Assign user roles], [Administrator],
    [Assign or revoke roles for individual users. View current role assignments.],
    [User role assignments updated. Effective permissions recalculated on next token issuance.],
    [IDN-FR-07, IDN-FR-12],
    [UC-ADM-IDN-04], [Grant user permissions], [Administrator],
    [Grant or revoke granular permissions directly on a user account.],
    [User's effective permissions updated to include direct grants plus role-derived permissions.],
    [IDN-FR-07, IDN-FR-12],
    [UC-ADM-IDN-05], [View permissions catalogue], [Administrator],
    [Browse the complete list of available permission claims across all modules.],
    [Full permission matrix visible for audit and role design.],
    [IDN-FR-07],
  ),
  caption: [Administrator use cases — Role and Permission Governance.],
)
