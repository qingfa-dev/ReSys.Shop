==== Payment Processing

// Diagram placeholder: Payment Processing use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-STR-PAY-01], [Create payment intent], [Customer], [At the Payment step of checkout, enter card details or select a saved payment method.], [Customer is authenticated. Checkout process has reached the payment step.], [Payment intent created. Customer can complete payment.],
  [UC-STR-PAY-02], [Confirm payment], [Customer], [Submit the payment; after successful authorisation, the system finalises the order.], [Payment intent has been created and is authorisable.], [Payment confirmed. Order moves to Complete state.],
)

==== Authentication

// Diagram placeholder: Authentication use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-STR-AUT-01], [Register], [Customer], [Create a new account with email, password, and profile information; verify email address.], [The customer has a valid email address not already registered.], [User account created. Email verification sent. Customer can log in after confirmation.],
  [UC-STR-AUT-02], [Login with password], [Customer], [Enter registered email and password to authenticate and establish a session.], [Customer has a registered and verified account.], [Customer authenticated. Session established. Guest cart associated with account if one exists.],
  [UC-STR-AUT-03], [Login with Google], [Customer], [Authenticate via Google OAuth; the system creates or links the user account.], [Customer has a valid Google account.], [Customer authenticated. New users auto-registered with Google profile data.],
  [UC-STR-AUT-04], [Reset password], [Customer], [Request a password reset link via registered email; set a new password using the time-limited link.], [Customer has a registered account with a valid email.], [Password updated. All existing sessions revoked for security.],
  [UC-STR-AUT-05], [Change password], [Customer], [While authenticated, enter the current password and a new password.], [Customer is authenticated and knows the current password.], [Password changed without invalidating the current session.],
)

==== Session Management

// Diagram placeholder: Session Management use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-STR-SES-01], [Refresh session], [Customer], [Before the access token expires, renew the session without requiring re-login.], [Customer has an active session with a valid refresh token.], [Session extended. Access token lifetime renewed.],
  [UC-STR-SES-02], [Logout], [Customer], [Explicitly terminate the current session.], [Customer has an active session.], [Session terminated. Refresh token invalidated.],
)
