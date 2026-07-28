==== Payment Processing
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-stf-payment-processing.png",
    width: 100%
  ),
  caption: [Use case diagram for Payment Processing (UC-STR-PAY).],
) <fig-uc-str-pay-d>

==== UC-STR-PAY: Payment Processing

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-PAY],
    [*Use Case Name*], [Payment Processing],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [Payment Gateway],
    [*Goal*], [Create and confirm payment for an order.],
    [*Trigger*], [Customer reaches the payment step during checkout.],
    [*Preconditions*], [
      - Customer is authenticated.
      - Checkout has reached payment step.
      - Order has a calculated total.
    ],
    [*Postconditions*], [
      - Payment authorised. Order transitions to Confirmed state.
    ],
    [*Main Success Scenario*], [
      *Create Payment Intent*
      1. System displays payment step with order total and available payment methods.
      2. Selects a payment method and enters payment details.
      3. System submits payment details to gateway to create intent.
      4. System receives confirmation intent is ready and displays review summary.
      ,
      *Confirm Payment*
      1. Reviews final order summary including line items, shipping, tax, and total.
      2. Clicks Confirm Payment.
      3. System submits payment intent confirmation to gateway.
      4. System receives authorisation confirmation.
      5. System updates payment state, transitions order to Confirmed, clears cart, and displays order confirmation.
    ],
    [*Alternative Flows*], [
      A1. Invalid payment details: system returns gateway validation error, prompts correction.
      A2. Authorisation declined: system displays reason and offers retry with different method.
      A3. Additional authentication required: system redirects to authentication flow and resumes after.
    ],
    [*Exception Flows*], [
      E1. Payment gateway unreachable: system displays error; checkout progress saved.
      E2. Payment confirms but order creation fails: system voids payment and notifies to retry.
      E3. Gateway timeout: system marks payment pending; advises checking order history; webhook updates state.
    ],
    [*Related Requirements*], [PAY-FR-01, PAY-FR-02],
  ),
    kind: table,
  caption: [Payment Processing.],
)

==== Authentication
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-authentication.png",
    width: 100%
  ),
  caption: [Use case diagram for Authentication (UC-STR-AUT).],
) <fig-uc-str-aut-d>

==== UC-STR-AUT: Authentication

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-AUT],
    [*Use Case Name*], [Authentication],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [Email Service, Google OAuth],
    [*Goal*], [Register, log in, and manage account credentials.],
    [*Trigger*], [Customer navigates to login or registration page.],
    [*Preconditions*], [
      - None (public access).
    ],
    [*Postconditions*], [
      - Customer authenticated or credentials updated.
    ],
    [*Main Success Scenario*], [
      *Register*
      1. Navigates to registration page.
      2. Enters email, password, and basic profile information.
      3. Submits. System validates email not registered and password strength, creates account, sends verification. Confirms.
      ,
      *Login with Password*
      1. Navigates to login page and enters email and password.
      2. Submits. System validates credentials, issues access and refresh tokens, associates guest cart if exists. Redirects to home page.
      ,
      *Login with Google*
      1. Clicks Login with Google on the storefront.
      2. System redirects to Google's OAuth consent screen.
      3. Grants consent. System exchanges code, looks up or creates account, issues tokens. Redirects to home page.
      ,
      *Reset Password*
      1. Clicks Forgot Password and submits registered email.
      2. System generates time-limited reset token and sends via email.
      3. Opens email, clicks reset link, enters new password.
      4. Submits. System validates token, updates password, revokes all sessions. Confirms.
      ,
      *Change Password*
      1. Navigates to account security settings and selects Change Password.
      2. Enters current password and new password.
      3. Submits. System verifies current password, validates new password, updates, revokes all sessions except current. Confirms.
    ],
    [*Alternative Flows*], [
      A1. Email already registered: system rejects and suggests login with password reset link.
      A2. Password does not meet strength requirements: system highlights requirements and prompts retry.
      A3. Invalid credentials: system rejects with generic message.
      A4. Account disabled: system rejects with message to contact support.
      A5. Reset token expired: system displays message and prompts new reset request.
      A6. Current password incorrect (Change): system rejects and prompts retry.
    ],
    [*Exception Flows*], [
      E1. Verification or reset email fails to send: system creates account/request but logs failure internally.
      E2. Token issuance fails: system reports failure and suggests retry.
    ],
    [*Related Requirements*], [IDN-FR-01, IDN-FR-02, IDN-FR-03, IDN-FR-08, IDN-FR-14],
  ),
    kind: table,
  caption: [Authentication.],
)

==== Session Management
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-session-management.png",
    width: 100%
  ),
  caption: [Use case diagram for Session Management (UC-STR-SES).],
) <fig-uc-str-ses-d>

==== UC-STR-SES: Session Management

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-SES],
    [*Use Case Name*], [Session Management],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [Maintain and terminate authenticated sessions.],
    [*Trigger*], [Client detects token expiry or customer initiates logout.],
    [*Preconditions*], [
      - Customer has an active session.
    ],
    [*Postconditions*], [
      - Session extended or terminated.
    ],
    [*Main Success Scenario*], [
      *Refresh Session*
      1. Client detects access token will expire soon.
      2. Client sends current refresh token to token endpoint.
      3. System validates refresh token (not expired, not consumed).
      4. System issues new access token with fresh expiry.
      5. System issues new refresh token and invalidates previous (token rotation).
      6. Client stores new token pair and continues session.
      ,
      *Logout*
      1. Clicks Logout in the storefront navigation.
      2. System sends current refresh token for invalidation.
      3. System invalidates the refresh token and removes the session cookie.
      4. System redirects to the storefront home page.
    ],
    [*Alternative Flows*], [
      A1. Refresh token expired: system rejects; client redirects to login.
      A2. Refresh token consumed (reuse detected): system revokes all tokens; client redirects to login with security notification.
      A3. Logout request fails due to network: system clears local tokens and cookies; session expires naturally.
    ],
    [*Exception Flows*], [
      E1. Token issuance or invalidation fails: client retains existing pair if access token still valid; clears locally otherwise.
    ],
    [*Related Requirements*], [IDN-FR-04, IDN-FR-05, IDN-FR-16],
  ),
    kind: table,
  caption: [Session Management.],
)
