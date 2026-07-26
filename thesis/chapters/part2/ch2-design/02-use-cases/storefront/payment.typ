==== Payment Processing

// Diagram placeholder: Payment Processing use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-STR-PAY-01], [Create payment intent], [Customer],
    [At the Payment step of checkout, the system presents a payment form. The customer enters card details or selects a saved payment method.],
    [Payment intent created. Customer can complete payment.],
    [PAY-FR-01],
    [UC-STR-PAY-02], [Confirm payment], [Customer],
    [Submit the payment. After successful authorisation, the system finalises the order.],
    [Payment confirmed. Order moves to Complete state.],
    [PAY-FR-02],
  ),
  caption: [Customer use cases — Payment Processing.],
)

==== Account Management

// Diagram placeholder: Account Management use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-STR-IDN-01], [Register], [Customer],
    [Enter email, password, and profile information to create a new account. Verify email address.],
    [User account created. Email verification sent. Customer can log in after confirmation.],
    [IDN-FR-01],
    [UC-STR-IDN-02], [Login with password], [Customer],
    [Enter registered email and password. The system authenticates and establishes a session.],
    [Customer authenticated. Session established. Guest cart associated with account if one exists.],
    [IDN-FR-02, IDN-FR-04],
    [UC-STR-IDN-03], [Login with Google], [Customer],
    [Authenticate via Google OAuth. The system creates or links the user account.],
    [Customer authenticated. New users auto-registered with Google profile data.],
    [IDN-FR-03],
    [UC-STR-IDN-04], [Refresh session], [Customer],
    [Before the access token expires, renew the session without requiring re-login.],
    [Session extended. Access token lifetime renewed.],
    [IDN-FR-04, IDN-FR-05],
    [UC-STR-IDN-05], [Logout], [Customer],
    [Explicitly terminate the current session.],
    [Session terminated. Refresh token invalidated.],
    [IDN-FR-16],
    [UC-STR-IDN-06], [Reset password], [Customer],
    [Request a password reset link via registered email. Set a new password using the time-limited link.],
    [Password updated. All existing sessions revoked for security.],
    [IDN-FR-08, IDN-FR-14],
    [UC-STR-IDN-07], [Change password], [Customer],
    [While authenticated, enter the current password and a new password.],
    [Password changed without invalidating the current session.],
    [IDN-FR-14],
  ),
  caption: [Customer use cases — Account Management (Identity module).],
)
