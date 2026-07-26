==== Payment Processing

// Diagram placeholder: Payment Processing use case diagram

*UC-STR-PAY-01 — Create payment intent.*
*Primary Actor:* Customer. \
*Main Flow:* At the Payment step of checkout, the system presents a payment form. The customer enters card details or selects a saved payment method. \
*Postcondition:* Payment intent created. Customer can complete payment. \
*Related FR:* PAY-FR-01.

#v(0.5cm)
*UC-STR-PAY-02 — Confirm payment.*
*Primary Actor:* Customer. \
*Main Flow:* Submit the payment. After successful authorisation, the system finalises the order. \
*Postcondition:* Payment confirmed. Order moves to Complete state. \
*Related FR:* PAY-FR-02.

==== Account Management

// Diagram placeholder: Account Management use case diagram

*UC-STR-IDN-01 — Register.*
*Primary Actor:* Customer. \
*Main Flow:* Enter email, password, and profile information to create a new account. Verify email address. \
*Postcondition:* User account created. Email verification sent. Customer can log in after confirmation. \
*Related FR:* IDN-FR-01.

#v(0.5cm)
*UC-STR-IDN-02 — Login with password.*
*Primary Actor:* Customer. \
*Main Flow:* Enter registered email and password. The system authenticates and establishes a session. \
*Postcondition:* Customer authenticated. Session established. Guest cart associated with account if one exists. \
*Related FR:* IDN-FR-02, IDN-FR-04.

#v(0.5cm)
*UC-STR-IDN-03 — Login with Google.*
*Primary Actor:* Customer. \
*Main Flow:* Authenticate via Google OAuth. The system creates or links the user account. \
*Postcondition:* Customer authenticated. New users auto-registered with Google profile data. \
*Related FR:* IDN-FR-03.

#v(0.5cm)
*UC-STR-IDN-04 — Refresh session.*
*Primary Actor:* Customer. \
*Main Flow:* Before the access token expires, renew the session without requiring re-login. \
*Postcondition:* Session extended. Access token lifetime renewed. \
*Related FR:* IDN-FR-04, IDN-FR-05.

#v(0.5cm)
*UC-STR-IDN-05 — Logout.*
*Primary Actor:* Customer. \
*Main Flow:* Explicitly terminate the current session. \
*Postcondition:* Session terminated. Refresh token invalidated. \
*Related FR:* IDN-FR-16.

#v(0.5cm)
*UC-STR-IDN-06 — Reset password.*
*Primary Actor:* Customer. \
*Main Flow:* Request a password reset link via registered email. Set a new password using the time-limited link. \
*Postcondition:* Password updated. All existing sessions revoked for security. \
*Related FR:* IDN-FR-08, IDN-FR-14.

#v(0.5cm)
*UC-STR-IDN-07 — Change password.*
*Primary Actor:* Customer. \
*Main Flow:* While authenticated, enter the current password and a new password. \
*Postcondition:* Password changed without invalidating the current session. \
*Related FR:* IDN-FR-14.
