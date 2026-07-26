==== User Management
// Diagram placeholder for User Management

#figure(
  table(
    columns: (auto, auto, auto, 1fr, auto, 1fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Flow*], [*Postcondition*], [*Related FR*]),
    [UC-ADM-IDN-01], [Manage users], [Admin],
    [Create user accounts with email and initial role. Update profile fields. Enable or disable accounts. View user detail including assigned roles and permissions.],
    [User account created or modified. Status changes take effect on next authentication attempt.],
    [IDN-FR-01, IDN-FR-09, IDN-FR-13],
    [UC-ADM-IDN-06], [Toggle user status], [Admin],
    [Enable or disable individual user accounts. Disabled accounts cannot authenticate or receive new tokens. Existing sessions invalidated upon disable.],
    [User account status updated; disabled users blocked from system access.],
    [IDN-FR-09, IDN-FR-13],
  ),
  caption: [Administrator use cases — User Management.],
)

==== Role and Permission Governance
// Diagram placeholder for Role and Permission Governance

#figure(
  table(
    columns: (auto, auto, auto, 1fr, auto, 1fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Flow*], [*Postcondition*], [*Related FR*]),
    [UC-ADM-IDN-02], [Manage roles], [Admin],
    [Create, update, or delete roles. Assign, synchronise, or revoke permissions per role. List roles with paging.],
    [Role configuration updated. Users assigned to modified roles receive updated permission sets.],
    [IDN-FR-07, IDN-FR-11],
    [UC-ADM-IDN-03], [Assign user roles], [Admin],
    [Assign, synchronise, or revoke roles for individual users. View current role assignments.],
    [User role assignments updated. Effective permissions recalculated on next token issuance.],
    [IDN-FR-07, IDN-FR-12],
    [UC-ADM-IDN-04], [Grant user permissions], [Admin],
    [Grant or revoke granular permissions directly on a user account, bypassing role inheritance for exceptional cases.],
    [User's effective permissions updated to include direct grants plus role-derived permissions.],
    [IDN-FR-07, IDN-FR-12],
    [UC-ADM-IDN-05], [View permissions catalog], [Admin],
    [Browse the complete list of available permission claims across all modules, organised by domain and category.],
    [Full permission matrix visible for audit and role design.],
    [IDN-FR-07],
  ),
  caption: [Administrator use cases — Role and Permission Governance.],
)
