==== Payment Processing

// Diagram placeholder: Payment Processing use case diagram

==== UC-STR-PAY-01 — Create Payment Intent

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-PAY-01],
    [*Use Case Name*], [Create Payment Intent],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [Payment Gateway],
    [*Goal*], [At checkout payment step, enter payment details to initiate the payment process.],
    [*Trigger*], [Customer reaches the payment step during checkout.],
    [*Preconditions*], [
      - Customer is authenticated.
      - Checkout has reached payment step.
      - Order has a calculated total.
    ],
    [*Postconditions*], [
      - Payment intent created. Customer can proceed to confirm payment.
    ],
    [*Main Success Scenario*], [
      1. System displays payment step with order total.
      2. System lists available payment methods configured for storefront.
      3. Selects a payment method.
      4. Enters payment details or selects a saved payment method.
      5. System submits payment details to gateway to create intent.
      6. System receives confirmation intent is ready.
      7. System displays review summary with masked payment details.
    ],
    [*Alternative Flows*], [
      A1. Invalid payment details: system returns gateway validation error, prompts correction.
      A2. Cancels payment step: system returns to shipping method step; no intent created.
      A3. Saved method expired: system notifies and prompts new details.
    ],
    [*Exception Flows*], [
      E1. Payment gateway unreachable: system displays error; checkout progress saved.
    ],
    [*Related Requirements*], [PAY-FR-01],
  ),
  caption: [UC-STR-PAY-01 -- Create Payment Intent.],
)

==== UC-STR-PAY-02 — Confirm Payment

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-PAY-02],
    [*Use Case Name*], [Confirm Payment],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [Payment Gateway],
    [*Goal*], [Submit the payment for authorisation to finalise the order.],
    [*Trigger*], [Customer has created a payment intent and is ready to confirm.],
    [*Preconditions*], [
      - Payment intent is authorisable.
      - Inventory is reserved for order items.
    ],
    [*Postconditions*], [
      - Payment confirmed. Order transitions to Confirmed state.
    ],
    [*Main Success Scenario*], [
      1. Reviews final order summary including line items, shipping, tax, and total.
      2. Clicks Confirm Payment.
      3. System submits payment intent confirmation to gateway.
      4. System receives authorisation confirmation from gateway.
      5. System updates payment state to confirmed.
      6. System transitions order to Confirmed state.
      7. System clears cart.
      8. System displays order confirmation page with order number and summary.
    ],
    [*Alternative Flows*], [
      A1. Authorisation declined: system displays reason and offers retry with different method.
      A2. Additional authentication required: system redirects to authentication flow and resumes after.
      A3. Aborts confirmation: system retains payment intent and checkout state.
    ],
    [*Exception Flows*], [
      E1. Payment confirms but order creation fails: system voids payment and notifies to retry.
      E2. Gateway timeout: system marks payment pending; advises checking order history; webhook updates state.
    ],
    [*Related Requirements*], [PAY-FR-02],
  ),
  caption: [UC-STR-PAY-02 -- Confirm Payment.],
)

==== Authentication

// Diagram placeholder: Authentication use case diagram

==== UC-STR-AUT-01 — Register

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-AUT-01],
    [*Use Case Name*], [Register],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [Email Service],
    [*Goal*], [Create a new account with email, password, and profile information; verify email.],
    [*Trigger*], [Customer navigates to registration page and submits the form.],
    [*Preconditions*], [
      - Customer has a valid email not already registered.
    ],
    [*Postconditions*], [
      - User account created. Email verification sent.
    ],
    [*Main Success Scenario*], [
      1. Navigates to registration page.
      2. Enters email, password, and basic profile information (name).
      3. Submits the registration form.
      4. System validates email not already registered and password meets strength requirements.
      5. System creates the user account.
      6. System sends email verification message.
      7. System displays confirmation page indicating verification is required.
    ],
    [*Alternative Flows*], [
      A1. Email already registered: system rejects and suggests login, with link to password reset.
      A2. Password does not meet strength requirements: system highlights requirements and prompts retry.
      A3. Guest cart exists during registration: system associates cart with new account upon verification.
    ],
    [*Exception Flows*], [
      E1. Verification message fails to send: system creates account flagged unverified; customer can resend.
    ],
    [*Related Requirements*], [IDN-FR-01],
  ),
  caption: [UC-STR-AUT-01 -- Register.],
)

==== UC-STR-AUT-02 — Login with Password

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-AUT-02],
    [*Use Case Name*], [Login with Password],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [Authenticate with email and password to establish a session.],
    [*Trigger*], [Customer navigates to login page and submits credentials.],
    [*Preconditions*], [
      - Customer has a registered and verified account.
    ],
    [*Postconditions*], [
      - Customer authenticated. Session established with access and refresh tokens. Guest cart associated if exists.
    ],
    [*Main Success Scenario*], [
      1. Navigates to login page.
      2. Enters registered email and password.
      3. Submits the login form.
      4. System validates credentials against stored identity.
      5. System issues access token (short lifetime) and refresh token (longer lifetime).
      6. System associates any existing guest cart with the authenticated account.
      7. System redirects to storefront home page or previously intended page.
    ],
    [*Alternative Flows*], [
      A1. Invalid credentials: system rejects with generic message not disclosing which was incorrect.
      A2. Account disabled: system rejects with message to contact support.
      A3. Email not verified: system rejects and offers to resend verification.
      A4. Consecutive failed attempts: system temporarily locks account with wait time.
    ],
    [*Exception Flows*], [
      E1. Token issuance fails: system reports failure and suggests retry.
    ],
    [*Related Requirements*], [IDN-FR-02],
  ),
  caption: [UC-STR-AUT-02 -- Login with Password.],
)

==== UC-STR-AUT-03 — Login with Google

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-AUT-03],
    [*Use Case Name*], [Login with Google],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [Google OAuth],
    [*Goal*], [Authenticate via Google OAuth; the system creates or links the user account.],
    [*Trigger*], [Customer clicks Login with Google on the login or registration page.],
    [*Preconditions*], [
      - Customer has a valid Google account.
    ],
    [*Postconditions*], [
      - Customer authenticated. New users auto-registered. Existing users logged in with linked account.
    ],
    [*Main Success Scenario*], [
      1. Clicks Login with Google on the storefront.
      2. System redirects to Google's OAuth consent screen.
      3. Grants consent on Google's page.
      4. System receives OAuth callback with authorisation code.
      5. System exchanges code for identity tokens from Google.
      6. System looks up email in user database.
      7. If email is registered: authenticates user and issues tokens.
      8. If email is new: creates account with Google profile data, marks email verified, issues tokens.
      9. System redirects to storefront home page.
    ],
    [*Alternative Flows*], [
      A1. Denies consent: system returns to login page without authentication.
      A2. Existing account created via password: system links Google identity to existing account.
      A3. Google returns error: system displays error and suggests password login.
    ],
    [*Exception Flows*], [
      E1. OAuth token exchange fails: system reports failure and suggests retry.
    ],
    [*Related Requirements*], [IDN-FR-03],
  ),
  caption: [UC-STR-AUT-03 -- Login with Google.],
)

==== UC-STR-AUT-04 — Reset Password

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-AUT-04],
    [*Use Case Name*], [Reset Password],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [Email Service],
    [*Goal*], [Request a password reset link via email; set a new password using the time-limited link.],
    [*Trigger*], [Customer clicks Forgot Password on login page and submits email.],
    [*Preconditions*], [
      - Customer has a registered account with valid email.
    ],
    [*Postconditions*], [
      - Password updated. All existing sessions revoked.
    ],
    [*Main Success Scenario*], [
      1. Clicks Forgot Password on login page.
      2. System displays form requesting registered email.
      3. Enters email and submits.
      4. System always displays confirmation (prevents email enumeration).
      5. If registered, generates time-limited single-use reset token and sends via email.
      6. Opens email and clicks reset link.
      7. System verifies token is valid and not expired.
      8. System displays new password form.
      9. Enters and confirms new password.
      10. Submits the form.
      11. System validates password strength and updates password.
      12. System revokes all active sessions.
      13. System confirms reset and prompts login with new password.
    ],
    [*Alternative Flows*], [
      A1. Reset token expired: system displays message and prompts new reset request.
      A2. Attempts to reuse consumed token: system displays expired message and prompts new request.
      A3. New password does not meet strength requirements: system highlights requirements and prompts retry.
    ],
    [*Exception Flows*], [
      E1. Reset email fails to send: system displays generic confirmation but logs failure internally.
    ],
    [*Related Requirements*], [IDN-FR-08],
  ),
  caption: [UC-STR-AUT-04 -- Reset Password.],
)

==== UC-STR-AUT-05 — Change Password

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-AUT-05],
    [*Use Case Name*], [Change Password],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [While authenticated, enter current password and a new password to update credentials.],
    [*Trigger*], [Customer navigates to security settings and selects Change Password.],
    [*Preconditions*], [
      - Customer is authenticated.
      - Customer knows current password.
    ],
    [*Postconditions*], [
      - Password changed. Current session retained. All other sessions revoked.
    ],
    [*Main Success Scenario*], [
      1. Navigates to account security settings.
      2. Selects Change Password.
      3. System displays form requesting current password and new password.
      4. Enters current password and new password (with confirmation).
      5. Submits the form.
      6. System verifies current password is correct.
      7. System validates new password meets strength requirements.
      8. System updates the password.
      9. System revokes all active sessions except current.
      10. System confirms password change.
    ],
    [*Alternative Flows*], [
      A1. Current password incorrect: system rejects and prompts retry.
      A2. New password same as current: system rejects and prompts different password.
      A3. New password does not meet strength requirements: system highlights requirements and prompts retry.
    ],
    [*Exception Flows*], [
      E1. System fails to update password: system reports failure and suggests retry.
    ],
    [*Related Requirements*], [IDN-FR-14],
  ),
  caption: [UC-STR-AUT-05 -- Change Password.],
)

==== Session Management

// Diagram placeholder: Session Management use case diagram

==== UC-STR-SES-01 — Refresh Session

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-SES-01],
    [*Use Case Name*], [Refresh Session],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [Renew the session without requiring re-login before the access token expires.],
    [*Trigger*], [Client detects access token is about to expire and requests a refresh.],
    [*Preconditions*], [
      - Customer has active session with valid refresh token.
    ],
    [*Postconditions*], [
      - Session extended. New access and refresh tokens issued. Previous refresh token invalidated.
    ],
    [*Main Success Scenario*], [
      1. System (client) detects access token will expire soon.
      2. System (client) sends current refresh token to token endpoint.
      3. System validates refresh token (not expired, not consumed).
      4. System issues new access token with fresh expiry.
      5. System issues new refresh token and invalidates previous (token rotation).
      6. System (client) stores new token pair and continues session.
    ],
    [*Alternative Flows*], [
      A1. Refresh token expired: system rejects; client redirects to login.
      A2. Refresh token consumed (reuse detected): system revokes all tokens; client redirects to login with security notification.
    ],
    [*Exception Flows*], [
      E1. Token issuance fails: client retains existing pair if access token still valid.
    ],
    [*Related Requirements*], [IDN-FR-04, IDN-FR-16],
  ),
  caption: [UC-STR-SES-01 -- Refresh Session.],
)

==== UC-STR-SES-02 — Logout

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-SES-02],
    [*Use Case Name*], [Logout],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [Explicitly terminate the current session.],
    [*Trigger*], [Customer clicks the logout button in the storefront.],
    [*Preconditions*], [
      - Customer has an active session.
    ],
    [*Postconditions*], [
      - Current session terminated. Refresh token invalidated.
    ],
    [*Main Success Scenario*], [
      1. Clicks Logout in the storefront navigation.
      2. System sends current refresh token for invalidation.
      3. System invalidates the refresh token.
      4. System removes the session cookie from the client.
      5. System redirects to the storefront home page.
    ],
    [*Alternative Flows*], [
      A1. Access token already expired: system still invalidates refresh token and clears session cookie.
      A2. Logout request fails due to network issue: system clears local tokens and cookies; session expires naturally.
    ],
    [*Exception Flows*], [
      E1. Token invalidation fails on server: system clears local tokens and cookies; server-side tokens expire naturally.
    ],
    [*Related Requirements*], [IDN-FR-05, IDN-FR-16],
  ),
  caption: [UC-STR-SES-02 -- Logout.],
)
