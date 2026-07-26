==== User Management

// Diagram placeholder: User Management use case diagram

*UC-ADM-IDN-01 — Manage users.*
*Primary Actor:* Administrator. \
*Main Flow:* Create user accounts. Update profile fields. Enable or disable accounts. View user detail including assigned roles and permissions. \
*Postcondition:* User account created or modified. Status changes take effect on next authentication. \
*Related FR:* IDN-FR-01, IDN-FR-09, IDN-FR-13.

==== Role and Permission Governance

// Diagram placeholder: Role and Permission use case diagram

*UC-ADM-IDN-02 — Manage roles.*
*Primary Actor:* Administrator. \
*Main Flow:* Create, update, or remove roles. Assign or revoke permissions per role. List roles with paging. \
*Postcondition:* Role configuration updated. Users assigned to modified roles receive updated permission sets. \
*Related FR:* IDN-FR-07, IDN-FR-11.

#v(0.5cm)
*UC-ADM-IDN-03 — Assign user roles.*
*Primary Actor:* Administrator. \
*Main Flow:* Assign or revoke roles for individual users. View current role assignments. \
*Postcondition:* User role assignments updated. Effective permissions recalculated on next token issuance. \
*Related FR:* IDN-FR-07, IDN-FR-12.

#v(0.5cm)
*UC-ADM-IDN-04 — Grant user permissions.*
*Primary Actor:* Administrator. \
*Main Flow:* Grant or revoke granular permissions directly on a user account. \
*Postcondition:* User's effective permissions updated to include direct grants plus role-derived permissions. \
*Related FR:* IDN-FR-07, IDN-FR-12.

#v(0.5cm)
*UC-ADM-IDN-05 — View permissions catalogue.*
*Primary Actor:* Administrator. \
*Main Flow:* Browse the complete list of available permission claims across all modules. \
*Postcondition:* Full permission matrix visible for audit and role design. \
*Related FR:* IDN-FR-07.
