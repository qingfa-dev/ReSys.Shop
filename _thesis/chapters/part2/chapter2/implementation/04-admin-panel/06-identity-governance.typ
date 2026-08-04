===== 4. Identity Governance (Identity Context)
The *Identity Management Grid* serves as the *Role-Based Access Control (RBAC)* operations center. It allows Tier 2 Support staff to intervene in User Accounts without requiring direct database access.

- *The Interface:* A searchable, server-side paginated grid of all registered identities, capable of filtering by Role (e.g., "Show all Administrators").
- *The Flow:*
  - *Role Promotion:* Admin promotes User A to "Manager".
  - *Security Side-Effect:* The backend updates the Claim Record and immediately invalidates User A's *Refresh Token*. This forces User A to re-authenticate, preventing privilege escalation latency.

// #figure(
//   figure-placeholder("User Management Grid"),
//   caption: [Identity Management Grid allowing Role assignment and Account auditing.],
// )

Access to these sensitive governance features is protected by a streamlined authentication flow that enforces policy checks (e.g., maximum retry attempts) before granting administrative session tokens.

#figure(
  placement: none,
  image("../../../../../images/ui/admin/ui-admin-login.png", width: 60%),
  caption: [Secure Entry: Administrative login portal implementing brute-force protection and session management.],
)


*Governance & Audit:*
- *UI Safety (Modal Confirmation):* For high-stakes actions like "Promote to Admin", the UI interjects a *Confirmation Modal* explaining the implications. If the session is old (e.g., > 30 minutes), it may trigger a "Sudden Death" re-authentication prompt before allowing the standard modal to confirm.
- *Sequence Flow:* @fig:sq-0015-user demonstrates the "Side-Effect" pattern. Changing a role doesn't just update the `UserClaims` table; it broadcasts a `UserRoleUpdated` integration event which invalidates the target user's *RefreshToken* in the distributed cache, ensuring immediate security compliance.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/admin/sq-0015-user-management.png", width: 60%),
  caption: [User Management Sequence: Role assignment and secure audit logging for administrative actions (UC-0015).],
) <fig:sq-0015-user>
