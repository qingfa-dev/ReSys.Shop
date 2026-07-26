==== Payment Processing
// Diagram placeholder for Payment Processing

#figure(
  table(
    columns: (auto, auto, auto, 1fr, auto, 1fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Flow*], [*Postcondition*], [*Related FR*]),
    [UC-STR-PAY-01], [Create payment intent], [Customer],
    [At the Payment step of checkout, the system creates a Stripe PaymentIntent for the order total. The customer is presented with a Stripe payment form to enter card details or select a saved payment method.],
    [PaymentIntent created with pending status. Stripe payment form displayed.],
    [PAY-FR-01],
    [UC-STR-PAY-02], [Confirm payment], [Customer],
    [After entering payment details, the customer submits the payment. Stripe processes the transaction and returns a confirmation. The system records the payment state and proceeds to order finalisation.],
    [Payment confirmed; order moves to Complete state. PaymentIntent state updated to Succeeded.],
    [PAY-FR-02],
  ),
  caption: [Customer use cases — Payment Processing.],
)

==== Account Management
// Diagram placeholder for Account Management

#figure(
  table(
    columns: (auto, auto, auto, 1fr, auto, 1fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Flow*], [*Postcondition*], [*Related FR*]),
    [UC-STR-IDN-01], [Register], [Customer],
    [Enter email, password, and profile information. Submit registration form. System validates uniqueness of email, hashes password, creates user account, and sends email verification.],
    [User account created. Verification email sent. Customer can log in after email confirmation.],
    [IDN-FR-01],
    [UC-STR-IDN-02], [Login with password], [Customer],
    [Enter registered email and password. System validates credentials, issues JWT access token (15-minute lifetime) and refresh token. Tokens returned in secure HTTP-only cookies.],
    [Customer authenticated; session established. Guest cart associated with account if one exists.],
    [IDN-FR-02, IDN-FR-04],
    [UC-STR-IDN-03], [Login with Google], [Customer],
    [Click Google login button. Redirected to Google OAuth consent screen. After authorisation, Google redirects back with authorisation code. System exchanges code for tokens and creates or links user account.],
    [Customer authenticated via Google OAuth. New users auto-registered with Google profile data.],
    [IDN-FR-03],
    [UC-STR-IDN-04], [Refresh session], [Customer],
    [Before access token expires, the client sends the refresh token to obtain a new access token and refresh token pair. Previous refresh token invalidated.],
    [Session extended without requiring re-login. Access token lifetime renewed.],
    [IDN-FR-04, IDN-FR-05],
    [UC-STR-IDN-05], [Logout], [Customer],
    [Explicitly terminate the current session. System invalidates the refresh token and clears authentication cookies.],
    [Session terminated. Refresh token invalidated. Cookies cleared.],
    [IDN-FR-16],
    [UC-STR-IDN-06], [Reset password], [Customer],
    [Request password reset link via registered email. System sends time-limited, single-use reset token. Customer clicks link, enters new password. Token consumed and invalidated.],
    [Password updated. All existing refresh tokens revoked for security.],
    [IDN-FR-08, IDN-FR-14],
    [UC-STR-IDN-07], [Change password], [Customer],
    [While authenticated, enter current password and new password. System verifies current password before update.],
    [Password changed without invalidating current session tokens.],
    [IDN-FR-14],
  ),
  caption: [Customer use cases — Account Management.],
)
