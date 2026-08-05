==== User Management

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-user-management.png",
    width: 60%
  ),
  caption: [Use case diagram for User Management (UC-ADM-USR).],
) <fig-uc-adm-usr-d>

==== UC-ADM-USR: Manage Users

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-ADM-USR — Manage Users],
    [*Actor*], [Administrator],
    [*Goal*], [Create, update, enable, and disable user accounts.],
    [*Pre/Post*], [
      Pre: authenticated with user management permissions.
      Post: user account created or modified; status changes take effect on next authentication.
    ],
    [*Scenario*], [
      + Navigates to user management.
      + System displays user list with default sorting and pagination.
      + Applies optional filters (email, name, role, account status).
      + Creates new user account by entering email, name, assigning roles.
      + Alternatively selects existing user to view detail or edit.
      + Modifies profile fields, enables/disables account, or adjusts role assignments.
      + Saves; system validates email uniqueness, persists, confirms; if account was disabled, all active sessions are revoked.
    ],
    [*Alternatives*], [
      + A1. Disable account → system revokes all active sessions immediately.
      + A2. Re-enable disabled account → system restores active status.
      + A3. Email already registered → system rejects, prompts for different email.
    ],
    [*Exceptions*], [
      + E1. Persistence failure → system reports, retains form data for retry.
    ],
    [*Requirements*], [IDN-FR-09, IDN-FR-13],
  ),
    kind: table,
  caption: [Manage Users.],
)

==== Role and Permission Governance

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-role-permission.png",
    height: 50%
  ),
  caption: [Use case diagram for Role and Permission Governance (UC-ADM-ROL).],
) <fig-uc-adm-rol-d>

==== UC-ADM-ROL: Manage Roles and Permissions

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-ADM-ROL — Manage Roles and Permissions],
    [*Actor*], [Administrator],
    [*Goal*], [Create and manage roles, assign permissions to roles, and grant roles to users.],
    [*Pre/Post*], [
      Pre: authenticated with role and permission management rights.
      Post: role and permission configuration updated; affected users receive updated permissions on next token.
    ],
    [*Scenario*], [
      *Manage Roles*
      + Navigates to role management.
      + System displays list of roles with name, description, permission count.
      + Creates new role with name and description.
      + Assigns permissions from permissions catalogue.
      + Optionally edits existing role's name, description, or permission set.
      + Saves; system validates role name uniqueness, persists, confirms.
      ,
      *Assign User Roles*
      + Opens user's detail page.
      + Opens role assignment panel.
      + System displays all available roles with checkboxes for current assignments.
      + Selects roles to assign and deselects to revoke.
      + Saves; system persists, displays updated effective permissions.
      ,
      *Grant Direct Permissions*
      + Opens user's detail page.
      + Opens direct permission panel.
      + System displays permissions catalogue with role-inherited and direct-grant indicators.
      + Selects permissions to grant directly and deselects to revoke.
      + Saves; system persists, recalculates effective permissions.
      ,
      *View Permissions Catalogue*
      + Navigates to permissions catalogue.
      + System displays all permission claims grouped by domain and module.
      + Applies optional filters (domain, category, keyword search).
      + Reviews permission matrix to plan role designs or audit assignments.
      ,
    ],
    [*Alternatives*], [
      + A1. Remove role assigned to users → system warns affected users lose permissions.
      + A2. Revoke permission from role → system warns all users with this role lose it on next token.
      + A3. Revoke last role from user → system warns user loses all role-derived permissions.
      + A4. Grant already inherited from role → system accepts; effective permission unchanged but direct grant recorded.
    ],
    [*Exceptions*], [
      + E1. Concurrent modification or role deleted → system refreshes, asks to retry.
    ],
    [*Requirements*], [IDN-FR-11, IDN-FR-12],
  ),
    kind: table,
  caption: [Manage Roles and Permissions.],
)
