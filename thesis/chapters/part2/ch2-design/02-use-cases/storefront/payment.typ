==== Payment Processing
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-stf-payment-processing.png",
    width: 70%
  ),
  caption: [Use case diagram for Payment Processing (UC-STR-PAY).],
) <fig-uc-str-pay-d>

==== UC-STR-PAY: Payment Processing

*Goal:* Create and confirm payment for an order. *Trigger:* the customer reaches the payment step of checkout with a calculated order total. *Related requirements:* PAY-GRP-01. The flow creates a payment intent through the gateway, reviews the final order summary, and confirms; alternatives cover invalid or declined payment details and additional authentication, while exceptions handle an unreachable gateway, order-creation failure after payment, and gateway timeouts.

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
    [*Support*], [Email Service],
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
    [*Requirements*], [IDN-GRP-01, IDN-GRP-02],
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
    [*Requirements*], [IDN-GRP-01],
  ),
    kind: table,
  caption: [Session Management.],
)
