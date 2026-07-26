==== User Management

// Diagram placeholder: User Management use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-ADM-USR-01], [Manage users], [Admin], [Create user accounts, update profile fields, enable or disable accounts, and view user detail including assigned roles and permissions.], [Admin is authenticated with user management permissions.], [User account created or modified. Status changes take effect on next authentication.],
)

==== Role and Permission Governance

// Diagram placeholder: Role and Permission use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-ADM-ROL-01], [Manage roles], [Admin], [Create, update, or remove roles; assign or revoke permissions per role; list roles with paging.], [Admin is authenticated with role management permissions.], [Role configuration updated. Users assigned to modified roles receive updated permission sets.],
  [UC-ADM-ROL-02], [Assign user roles], [Admin], [Assign or revoke roles for individual users and view current role assignments.], [Admin is authenticated. The user and roles exist.], [User role assignments updated. Effective permissions recalculated on next token issuance.],
  [UC-ADM-ROL-03], [Grant user permissions], [Admin], [Grant or revoke granular permissions directly on a user account.], [Admin is authenticated with permission management rights. The user and permissions exist.], [User's effective permissions updated to include direct grants plus role-derived permissions.],
  [UC-ADM-ROL-04], [View permissions catalogue], [Admin], [Browse the complete list of available permission claims across all modules.], [Admin is authenticated with permission viewing rights.], [Full permission matrix visible for audit and role design.],
)
