==== User Management

// Diagram placeholder: User Management use case diagram

==== UC-ADM-USR-01 — Manage Users

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-USR-01],
    [*Use Case Name*], [Manage Users],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create, update, enable, or disable user accounts and manage role assignments.],
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
      7. Saves the changes.
      8. System validates email uniqueness.
      9. System persists the user account changes.
      10. System confirms; if account was disabled, all active sessions are revoked.
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
  caption: [UC-ADM-USR-01 -- Manage Users.],
)

==== Role and Permission Governance

// Diagram placeholder: Role and Permission use case diagram

==== UC-ADM-ROL-01 — Manage Roles

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-ROL-01],
    [*Use Case Name*], [Manage Roles],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create, update, or remove roles; assign or revoke permissions per role.],
    [*Trigger*], [Administrator navigates to role management.],
    [*Preconditions*], [
      - Authenticated with role management permissions.
    ],
    [*Postconditions*], [
      - Role configuration updated. Affected users receive updated permissions on next token.
    ],
    [*Main Success Scenario*], [
      1. Navigates to role management.
      2. System displays list of roles with name, description, and permission count.
      3. Creates a new role with name and description.
      4. Assigns permissions from the permissions catalogue.
      5. Optionally edits an existing role's name, description, or permission set.
      6. Saves the changes.
      7. System validates role name uniqueness.
      8. System persists the role configuration.
      9. System confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Remove role assigned to users: system warns affected users lose permissions.
      A2. Revoke permission from role: system warns all users with this role lose it on next token.
      A3. Role name already exists: system rejects and prompts for a different name.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification: system refreshes and asks to retry.
    ],
    [*Related Requirements*], [IDN-FR-11],
  ),
  caption: [UC-ADM-ROL-01 -- Manage Roles.],
)

==== UC-ADM-ROL-02 — Assign User Roles

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-ROL-02],
    [*Use Case Name*], [Assign User Roles],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Assign or revoke roles for individual users.],
    [*Trigger*], [Administrator opens the role assignment panel from a user's detail page.],
    [*Preconditions*], [
      - Authenticated with role management permissions.
      - User exists.
    ],
    [*Postconditions*], [
      - User role assignments updated.
    ],
    [*Main Success Scenario*], [
      1. Opens a user's detail page from the user listing.
      2. System displays user profile with assigned roles and effective permissions.
      3. Opens the role assignment panel.
      4. System displays all available roles with checkboxes for current assignments.
      5. Selects roles to assign and deselects roles to revoke.
      6. Saves the changes.
      7. System persists the updated role assignments.
      8. System confirms and displays updated effective permissions.
    ],
    [*Alternative Flows*], [
      A1. Revoke last role: system warns user loses all role-derived permissions but retains direct grants.
      A2. Incompatible role: system accepts (no incompatibility rules enforced).
      A3. Duplicate role: system silently ignores and retains existing assignment.
    ],
    [*Exception Flows*], [
      E1. Role deleted concurrently: system refreshes available roles list and notifies.
    ],
    [*Related Requirements*], [IDN-FR-12],
  ),
  caption: [UC-ADM-ROL-02 -- Assign User Roles.],
)

==== UC-ADM-ROL-03 — Grant User Permissions

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-ROL-03],
    [*Use Case Name*], [Grant User Permissions],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Grant or revoke granular permissions directly on a user, bypassing role inheritance.],
    [*Trigger*], [Administrator opens the direct permission panel from a user's detail page.],
    [*Preconditions*], [
      - Authenticated with permission management rights.
      - User exists.
    ],
    [*Postconditions*], [
      - User's effective permissions updated.
    ],
    [*Main Success Scenario*], [
      1. Opens a user's detail page from the user listing.
      2. System displays user profile with assigned roles and effective permissions.
      3. Opens the direct permission panel.
      4. System displays permissions catalogue with role-inherited and direct-grant indicators.
      5. Selects permissions to grant directly and deselects to revoke.
      6. Saves the changes.
      7. System persists the updated direct permission grants.
      8. System recalculates and displays updated effective permissions.
    ],
    [*Alternative Flows*], [
      A1. Grant already inherited from role: system accepts; effective permission unchanged but direct grant recorded.
      A2. Revoke permission inherited from role: system warns it cannot be revoked directly; suggests revoking role.
      A3. No permissions selected: system clears all direct grants; effective permissions come from roles only.
    ],
    [*Exception Flows*], [
      E1. Permission removed from catalogue concurrently: system refreshes list and notifies.
    ],
    [*Related Requirements*], [IDN-FR-12],
  ),
  caption: [UC-ADM-ROL-03 -- Grant User Permissions.],
)

==== UC-ADM-ROL-04 — View Permissions Catalogue

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-ROL-04],
    [*Use Case Name*], [View Permissions Catalogue],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Browse the complete list of available permission claims across all modules.],
    [*Trigger*], [Administrator navigates to the permissions catalogue.],
    [*Preconditions*], [
      - Authenticated with permission viewing rights.
    ],
    [*Postconditions*], [
      - Full permission matrix visible for audit and role design.
    ],
    [*Main Success Scenario*], [
      1. Navigates to the permissions catalogue.
      2. System displays all permission claims grouped by domain and module.
      3. System shows each permission: domain, category, action name, and description.
      4. Applies optional filters: domain, category, keyword search.
      5. System refreshes listing with filtered results.
      6. Reviews the permission matrix to plan role designs or audit assignments.
    ],
    [*Alternative Flows*], [
      A1. Filter by role: system highlights permissions assigned to the selected role.
      A2. Filter by user: system highlights the user's effective permission set.
      A3. No permissions match: system displays empty result message.
    ],
    [*Exception Flows*], [
      E1. Retrieval failure: system displays error and offers retry.
    ],
    [*Related Requirements*], [IDN-FR-11],
  ),
  caption: [UC-ADM-ROL-04 -- View Permissions Catalogue.],
)
