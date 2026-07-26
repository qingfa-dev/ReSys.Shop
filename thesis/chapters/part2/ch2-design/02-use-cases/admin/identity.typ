==== User Management

// Diagram placeholder: User Management use case diagram

==== UC-ADM-USR — Manage Users

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-USR],
    [*Use Case Name*], [Manage Users],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create, update, enable, and disable user accounts.],
    [*Trigger*], [Administrator navigates to user management.],
    [*Preconditions*], [
      - Authenticated with user management permissions.
    ],
    [*Postconditions*], [
      - User account created or modified. Status changes take effect on next authentication.
    ],
    [*Main Success Scenario*], [
      1. Navigates to user management.
      2. System displays user list with default sorting and pagination.
      3. Applies optional filters: email, name, role, account status.
      4. Creates a new user account by entering email, name, and assigning roles.
      5. Alternatively selects an existing user to view detail or edit.
      6. Modifies profile fields, enables/disables account, or adjusts role assignments.
      7. Saves. System validates email uniqueness, persists, and confirms. If account was disabled, all active sessions are revoked.
    ],
    [*Alternative Flows*], [
      A1. Disable account: system revokes all active sessions immediately.
      A2. Re-enable disabled account: system restores active status.
      A3. Email already registered: system rejects and prompts for a different email.
    ],
    [*Exception Flows*], [
      E1. Persistence failure: system reports and retains form data for retry.
    ],
    [*Related Requirements*], [IDN-FR-09, IDN-FR-13],
  ),
  caption: [UC-ADM-USR -- Manage Users.],
)

==== Role and Permission Governance

// Diagram placeholder: Role and Permission use case diagram

==== UC-ADM-ROL — Manage Roles and Permissions

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-ROL],
    [*Use Case Name*], [Manage Roles and Permissions],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create and manage roles, assign permissions to roles, and grant roles to users.],
    [*Trigger*], [Administrator navigates to role management or user detail page.],
    [*Preconditions*], [
      - Authenticated with role and permission management rights.
    ],
    [*Postconditions*], [
      - Role and permission configuration updated. Affected users receive updated permissions on next token.
    ],
    [*Main Success Scenario*], [
      *Manage Roles*
      1. Navigates to role management.
      2. System displays list of roles with name, description, and permission count.
      3. Creates a new role with name and description.
      4. Assigns permissions from the permissions catalogue.
      5. Optionally edits an existing role's name, description, or permission set.
      6. Saves. System validates role name uniqueness, persists, and confirms.
      ,
      *Assign User Roles*
      1. Opens a user's detail page.
      2. Opens the role assignment panel.
      3. System displays all available roles with checkboxes for current assignments.
      4. Selects roles to assign and deselects to revoke.
      5. Saves. System persists and displays updated effective permissions.
      ,
      *Grant Direct Permissions*
      1. Opens a user's detail page.
      2. Opens the direct permission panel.
      3. System displays permissions catalogue with role-inherited and direct-grant indicators.
      4. Selects permissions to grant directly and deselects to revoke.
      5. Saves. System persists and recalculates effective permissions.
      ,
      *View Permissions Catalogue*
      1. Navigates to the permissions catalogue.
      2. System displays all permission claims grouped by domain and module.
      3. Applies optional filters: domain, category, keyword search.
      4. Reviews the permission matrix to plan role designs or audit assignments.
    ],
    [*Alternative Flows*], [
      A1. Remove role assigned to users: system warns affected users lose permissions.
      A2. Revoke permission from role: system warns all users with this role lose it on next token.
      A3. Revoke last role from user: system warns user loses all role-derived permissions.
      A4. Grant already inherited from role: system accepts; effective permission unchanged but direct grant recorded.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification or role deleted: system refreshes and asks to retry.
    ],
    [*Related Requirements*], [IDN-FR-11, IDN-FR-12],
  ),
  caption: [UC-ADM-ROL -- Manage Roles and Permissions.],
)
