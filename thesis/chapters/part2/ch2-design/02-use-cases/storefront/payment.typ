==== Payment Processing
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-stf-payment-processing.png",
    width: 70%
  ),
  caption: [Use case diagram for Payment Processing (UC-STR-PAY).],
) <fig-uc-str-pay-d>

==== UC-STR-PAY: Payment Processing

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-STR-PAY: Payment Processing],
    [*Actor*], [Customer],
    [*Support*], [Payment Gateway],
    [*Goal*], [Create and confirm payment for an order.],
    [*Pre/Post*], [
      Pre: customer is authenticated; checkout has reached payment step; order has a calculated total.
      Post: payment authorised; order transitions to Confirmed state.
    ],
    [*Scenario*], [
      *Create Payment Intent*
      + System displays payment step with order total and available payment methods.
      + Selects a payment method and enters payment details.
      + System submits payment details to gateway to create intent.
      + System receives confirmation intent is ready and displays review summary.
      ,
      *Confirm Payment*
      + Reviews final order summary including line items, shipping, tax, and total.
      + Clicks Confirm Payment.
      + System submits payment intent confirmation to gateway.
      + System receives authorisation confirmation.
      + System updates payment state, transitions order to Confirmed, clears cart, and displays order confirmation.
      ,
    ],
    [*Alternatives*], [
      + A1. Invalid payment details → system returns gateway validation error, prompts correction.
      + A2. Authorisation declined → system displays reason and offers retry with different method.
      + A3. Additional authentication required → system redirects to authentication flow and resumes after.
    ],
    [*Exceptions*], [
      + E1. Payment gateway unreachable → system displays error; checkout progress saved.
      + E2. Payment confirms but order creation fails → system voids payment and notifies to retry.
      + E3. Gateway timeout → system marks payment pending; advises checking order history; webhook updates state.
    ],
    [*Requirements*], [PAY-FR-01, PAY-FR-02],
  ),
    kind: table,
  caption: [Payment Processing.],
)

==== Authentication
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-authentication.png",
    width: 70%
  ),
  caption: [Use case diagram for Authentication (UC-STR-AUT).],
) <fig-uc-str-aut-d>

==== UC-STR-AUT: Authentication

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-STR-AUT: Authentication],
    [*Actor*], [Customer],
    [*Support*], [Email Service, Google OAuth],
    [*Goal*], [Register, log in, and manage account credentials.],
    [*Pre/Post*], [
      Pre: none (public access).
      Post: customer authenticated or credentials updated.
    ],
    [*Scenario*], [
      *Register*
      + Navigates to registration page.
      + Enters email, password, and basic profile information.
      + Submits; system validates email not registered and password strength, creates account, sends verification; confirms.
      ,
      *Login with Password*
      + Navigates to login page and enters email and password.
      + Submits; system validates credentials, issues access and refresh tokens, associates guest cart if exists; redirects to home page.
      ,
      *Login with Google*
      + Clicks Login with Google on the storefront.
      + System redirects to Google's OAuth consent screen.
      + Grants consent; system exchanges code, looks up or creates account, issues tokens; redirects to home page.
      ,
      *Reset Password*
      + Clicks Forgot Password and submits registered email.
      + System generates time-limited reset token and sends via email.
      + Opens email, clicks reset link, enters new password.
      + Submits; system validates token, updates password, revokes all sessions; confirms.
      ,
      *Change Password*
      + Navigates to account security settings and selects Change Password.
      + Enters current password and new password.
      + Submits; system verifies current password, validates new password, updates, revokes all sessions except current; confirms.
      ,
    ],
    [*Alternatives*], [
      + A1. Email already registered → system rejects and suggests login with password reset link.
      + A2. Password does not meet strength requirements → system highlights requirements and prompts retry.
      + A3. Invalid credentials → system rejects with generic message.
      + A4. Account disabled → system rejects with message to contact support.
      + A5. Reset token expired → system displays message and prompts new reset request.
      + A6. Current password incorrect (Change) → system rejects and prompts retry.
    ],
    [*Exceptions*], [
      + E1. Verification or reset email fails to send → system creates account/request but logs failure internally.
      + E2. Token issuance fails → system reports failure and suggests retry.
    ],
    [*Requirements*], [IDN-FR-01, IDN-FR-02, IDN-FR-03, IDN-FR-08, IDN-FR-14],
  ),
    kind: table,
  caption: [Authentication.],
)

==== Session Management
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-session-management.png",
    width: 45%
  ),
  caption: [Use case diagram for Session Management (UC-STR-SES).],
) <fig-uc-str-ses-d>

==== UC-STR-SES: Session Management

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-STR-SES: Session Management],
    [*Actor*], [Customer],
    [*Goal*], [Maintain and terminate authenticated sessions.],
    [*Pre/Post*], [
      Pre: customer has an active session.
      Post: session extended or terminated.
    ],
    [*Scenario*], [
      *Refresh Session*
      + Client detects access token will expire soon.
      + Client sends current refresh token to token endpoint.
      + System validates refresh token (not expired, not consumed).
      + System issues new access token with fresh expiry.
      + System issues new refresh token and invalidates previous (token rotation).
      + Client stores new token pair and continues session.
      ,
      *Logout*
      + Clicks Logout in the storefront navigation.
      + System sends current refresh token for invalidation.
      + System invalidates the refresh token and removes the session cookie.
      + System redirects to the storefront home page.
      ,
    ],
    [*Alternatives*], [
      + A1. Refresh token expired → system rejects; client redirects to login.
      + A2. Refresh token consumed (reuse detected) → system revokes all tokens; client redirects to login with security notification.
      + A3. Logout request fails due to network → system clears local tokens and cookies; session expires naturally.
    ],
    [*Exceptions*], [
      + E1. Token issuance or invalidation fails → client retains existing pair if access token still valid; clears locally otherwise.
    ],
    [*Requirements*], [IDN-FR-04, IDN-FR-05, IDN-FR-16],
  ),
    kind: table,
  caption: [Session Management.],
)
