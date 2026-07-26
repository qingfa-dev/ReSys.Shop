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
    [*Goal*], [Create user accounts, update profile fields, enable or disable accounts, and view user detail including assigned roles and permissions.],
    [*Trigger*], [Administrator navigates to the user management interface.],
    [*Preconditions*], [
      - Administrator is authenticated with user management permissions.
    ],
    [*Postconditions*], [
      - User account created or modified.
      - Status changes take effect on next authentication.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the user management interface.
      2. System -- Displays the user list with default sorting and pagination.
      3. Administrator -- Applies optional filters: email, name, role, account status.
      4. Administrator -- Creates a new user account by entering email, name, and assigning roles.
      5. Administrator -- Alternatively selects an existing user to view detail or edit.
      6. Administrator -- Modifies profile fields, enables or disables the account, or adjusts role assignments.
      7. Administrator -- Saves the changes.
      8. System -- Validates that the email is unique across the system.
      9. System -- Persists the user account changes.
      10. System -- Confirms the changes; if the account was disabled, all active sessions are revoked.
    ],
    [*Alternative Flows*], [
      A1. Administrator disables a user account -- System revokes all active sessions immediately; the user cannot authenticate until the account is re-enabled.
      A2. Administrator re-enables a disabled account -- System restores the account to active status; the user can authenticate again immediately.
      A3. Email is already registered -- System rejects the creation and prompts the administrator to use a different email.
    ],
    [*Exception Flows*], [
      E1. System fails to persist user changes -- System reports the failure and retains the form data for retry.
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
    [*Goal*], [Create, update, or remove roles; assign or revoke permissions per role; list roles with paging.],
    [*Trigger*], [Administrator navigates to the role management interface.],
    [*Preconditions*], [
      - Administrator is authenticated with role management permissions.
    ],
    [*Postconditions*], [
      - Role configuration updated.
      - Users assigned to modified roles receive updated permission sets on next token issuance.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the role management interface.
      2. System -- Displays the list of existing roles with paging, showing role name, description, and permission count.
      3. Administrator -- Creates a new role with a name and description.
      4. Administrator -- Assigns permissions to the role from the available permissions catalogue.
      5. Administrator -- Optionally edits an existing role's name, description, or permission set.
      6. Administrator -- Saves the changes.
      7. System -- Validates that the role name is unique.
      8. System -- Persists the role configuration.
      9. System -- Confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Administrator removes a role currently assigned to users -- System warns that affected users will lose the role's permissions and asks for confirmation.
      A2. Administrator revokes a permission from a role -- System warns that all users assigned to this role will lose that permission on their next token.
      A3. Role name already exists -- System rejects and prompts the administrator to choose a different name.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification detected -- System detects the role was modified by another session, refreshes the data, and asks the administrator to retry.
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
    [*Goal*], [Assign or revoke roles for individual users and view current role assignments.],
    [*Trigger*], [Administrator opens the role assignment panel from a user's detail page.],
    [*Preconditions*], [
      - Administrator is authenticated with role management permissions.
      - The user exists.
    ],
    [*Postconditions*], [
      - User role assignments updated.
      - Effective permissions recalculated on next token issuance.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Opens a user's detail page from the user listing.
      2. System -- Displays the user profile with currently assigned roles and effective permissions.
      3. Administrator -- Opens the role assignment panel.
      4. System -- Displays all available roles with checkboxes indicating current assignments.
      5. Administrator -- Selects roles to assign and deselects roles to revoke.
      6. Administrator -- Saves the changes.
      7. System -- Persists the updated role assignments.
      8. System -- Confirms the changes and displays the updated effective permissions.
    ],
    [*Alternative Flows*], [
      A1. Administrator revokes the last role from a user -- System warns that the user will lose all role-derived permissions but retains any direct permission grants.
      A2. Administrator assigns an incompatible role -- System accepts the assignment (no role incompatibility rules are enforced).
      A3. Administrator assigns a role the user already has -- System silently ignores the duplicate and retains the existing assignment.
    ],
    [*Exception Flows*], [
      E1. Role was deleted by a concurrent session -- System refreshes the available roles list and notifies the administrator.
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
    [*Goal*], [Grant or revoke granular permissions directly on a user account, bypassing role inheritance.],
    [*Trigger*], [Administrator opens the direct permission panel from a user's detail page.],
    [*Preconditions*], [
      - Administrator is authenticated with permission management rights.
      - The user exists.
    ],
    [*Postconditions*], [
      - User's effective permissions updated to include direct grants plus role-derived permissions.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Opens a user's detail page from the user listing.
      2. System -- Displays the user profile with currently assigned roles and effective permissions.
      3. Administrator -- Opens the direct permission panel.
      4. System -- Displays the permissions catalogue with indicators showing which permissions are inherited from roles and which are directly granted.
      5. Administrator -- Selects permissions to grant directly and deselects permissions to revoke.
      6. Administrator -- Saves the changes.
      7. System -- Persists the updated direct permission grants.
      8. System -- Recalculates and displays the user's updated effective permissions.
    ],
    [*Alternative Flows*], [
      A1. Administrator grants a permission already inherited from a role -- System accepts the grant; the effective permission remains unchanged but the direct grant is recorded for audit.
      A2. Administrator revokes a permission inherited from a role -- System warns that the permission cannot be revoked directly because it comes from a role; suggests revoking the role instead.
      A3. No permissions are selected -- System clears all direct grants and the user's effective permissions are determined solely by assigned roles.
    ],
    [*Exception Flows*], [
      E1. Permission was removed from the system catalogue by a concurrent session -- System refreshes the permissions list and notifies the administrator.
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
    [*Trigger*], [Administrator navigates to the permissions catalogue interface.],
    [*Preconditions*], [
      - Administrator is authenticated with permission viewing rights.
    ],
    [*Postconditions*], [
      - Full permission matrix visible for audit and role design.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the permissions catalogue interface.
      2. System -- Displays the complete list of permission claims grouped by domain and module.
      3. System -- Shows each permission with: domain, category, action name, and description.
      4. Administrator -- Applies optional filters: domain, category, keyword search.
      5. System -- Refreshes the listing with filtered results.
      6. Administrator -- Reviews the permission matrix to plan role designs or audit current assignments.
    ],
    [*Alternative Flows*], [
      A1. Administrator filters by role -- System highlights which permissions are assigned to the selected role, showing the permission-to-role mapping.
      A2. Administrator filters by user -- System highlights which permissions the selected user has (through roles or direct grants), showing the effective permission set.
      A3. No permissions match the applied filters -- System displays an empty result message.
    ],
    [*Exception Flows*], [
      E1. System fails to retrieve permission data -- System displays an error message and offers a retry option.
    ],
    [*Related Requirements*], [IDN-FR-11],
  ),
  caption: [UC-ADM-ROL-04 -- View Permissions Catalogue.],
)
