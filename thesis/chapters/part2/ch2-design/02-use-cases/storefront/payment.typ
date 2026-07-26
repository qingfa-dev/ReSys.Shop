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
    [*Goal*], [At the Payment step of checkout, enter card details or select a saved payment method to initiate the payment process.],
    [*Trigger*], [Customer reaches the payment step during checkout.],
    [*Preconditions*], [
      - Customer is authenticated.
      - Checkout process has reached the payment step.
      - Order has a calculated total amount.
    ],
    [*Postconditions*], [
      - Payment intent created with the payment gateway.
      - Customer can proceed to confirm payment.
    ],
    [*Main Success Scenario*], [
      1. System -- Displays the payment step in the checkout flow with the order total.
      2. System -- Lists available payment methods configured for the storefront.
      3. Customer -- Selects a payment method.
      4. Customer -- Enters payment details (e.g. card number, expiry, CVC) or selects a previously saved payment method.
      5. System -- Submits the payment details to the payment gateway to create a payment intent.
      6. System -- Receives confirmation that the intent was created and is ready for confirmation.
      7. System -- Displays a review summary of the payment with masked payment details.
    ],
    [*Alternative Flows*], [
      A1. Payment details are invalid -- System returns the validation error from the gateway and prompts the customer to correct the information.
      A2. Customer cancels the payment step and navigates back -- System returns to the shipping method step; no payment intent is created.
      A3. Saved payment method has expired -- System notifies the customer and prompts to enter new payment details.
    ],
    [*Exception Flows*], [
      E1. Payment gateway is unreachable -- System displays an error message indicating the payment service is temporarily unavailable and the checkout progress has been saved.
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
    [*Goal*], [Submit the payment for authorisation; after successful confirmation, the system finalises the order.],
    [*Trigger*], [Customer has created a payment intent and is ready to confirm the payment.],
    [*Preconditions*], [
      - Payment intent has been created and is authorisable.
      - Inventory is reserved for the order items.
    ],
    [*Postconditions*], [
      - Payment confirmed.
      - Order transitions to Confirmed state.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Reviews the final order summary including line items, shipping, tax, and total.
      2. Customer -- Clicks Confirm Payment.
      3. System -- Submits the payment intent confirmation to the payment gateway.
      4. System -- Receives authorisation confirmation from the gateway.
      5. System -- Updates the payment state to confirmed.
      6. System -- Transitions the order to Confirmed state.
      7. System -- Clears the cart.
      8. System -- Displays the order confirmation page with the order number and a summary.
    ],
    [*Alternative Flows*], [
      A1. Payment authorisation is declined -- System displays the decline reason and offers the customer the option to retry with a different payment method.
      A2. Additional authentication is required by the payment gateway -- System redirects the customer to the authentication flow and resumes the confirmation upon completion.
      A3. Customer aborts confirmation -- System retains the payment intent (which may expire) and retains the checkout state.
    ],
    [*Exception Flows*], [
      E1. Payment gateway confirms but the order creation fails -- System voids the confirmed payment and notifies the customer to retry.
      E2. Payment gateway times out -- System marks the payment as pending and advises the customer to check their order history for the outcome; a webhook will update the state when the gateway responds.
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
    [*Goal*], [Create a new account with email, password, and profile information; verify email address.],
    [*Trigger*], [Customer navigates to the registration page and submits the registration form.],
    [*Preconditions*], [
      - Customer has a valid email address not already registered.
    ],
    [*Postconditions*], [
      - User account created.
      - Email verification message sent.
      - Customer can log in after email confirmation.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Navigates to the registration page.
      2. Customer -- Enters email, password, and basic profile information (name).
      3. Customer -- Submits the registration form.
      4. System -- Validates that the email is not already registered and the password meets strength requirements.
      5. System -- Creates the user account.
      6. System -- Sends an email verification message to the registered email address.
      7. System -- Displays a confirmation page indicating that verification is required.
    ],
    [*Alternative Flows*], [
      A1. Email is already registered -- System rejects and suggests the customer log in instead, with a link to the password reset page.
      A2. Password does not meet strength requirements -- System highlights the requirements (minimum length, character variety) and prompts retry.
      A3. Customer registers while having a guest cart -- System associates the guest cart with the new account upon email verification.
    ],
    [*Exception Flows*], [
      E1. Email verification message fails to send -- System creates the account but flags it as unverified; the customer can request a new verification message from the account settings.
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
    [*Goal*], [Enter registered email and password to authenticate and establish a session.],
    [*Trigger*], [Customer navigates to the login page and submits credentials.],
    [*Preconditions*], [
      - Customer has a registered and verified account.
    ],
    [*Postconditions*], [
      - Customer authenticated.
      - Session established with access token and refresh token.
      - Guest cart associated with account if one exists.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Navigates to the login page.
      2. Customer -- Enters registered email and password.
      3. Customer -- Submits the login form.
      4. System -- Validates the credentials against the stored identity.
      5. System -- Issues an access token (short lifetime) and a refresh token (longer lifetime).
      6. System -- Associates any existing guest cart with the authenticated account.
      7. System -- Redirects the customer to the storefront home page or the previously intended page.
    ],
    [*Alternative Flows*], [
      A1. Invalid credentials -- System rejects with a generic error message that does not disclose whether the email or password was incorrect.
      A2. Account is disabled -- System rejects and displays a message that the account has been disabled; instructs the customer to contact support.
      A3. Email is not yet verified -- System rejects and offers to resend the verification message.
      A4. Consecutive failed attempts -- System temporarily locks the account and displays a wait time before retry.
    ],
    [*Exception Flows*], [
      E1. Token issuance fails -- System reports the failure and suggests the customer retry.
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
      - Customer authenticated.
      - New users auto-registered with Google profile data.
      - Existing users logged in with their linked account.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Clicks Login with Google on the storefront.
      2. System -- Redirects the customer to Google's OAuth consent screen.
      3. Customer -- Grants consent on Google's page.
      4. System -- Receives the OAuth callback with an authorisation code.
      5. System -- Exchanges the code for identity tokens from Google.
      6. System -- Looks up the email in the user database.
      7. System -- If the email is registered: authenticates the user and issues tokens.
      8. System -- If the email is new: creates a user account with Google profile data, marks email as verified, and issues tokens.
      9. System -- Redirects the customer to the storefront home page.
    ],
    [*Alternative Flows*], [
      A1. Customer denies consent on Google -- System returns to the storefront login page without authentication.
      A2. Existing account with the same email was created via password registration -- System links the Google identity to the existing account and authenticates.
      A3. Google returns an error (e.g. technical issue) -- System displays an error message and suggests the customer try password login instead.
    ],
    [*Exception Flows*], [
      E1. OAuth token exchange fails -- System reports the failure and suggests retrying.
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
    [*Goal*], [Request a password reset link via registered email; set a new password using the time-limited link.],
    [*Trigger*], [Customer clicks Forgot Password on the login page and submits their email.],
    [*Preconditions*], [
      - Customer has a registered account with a valid email.
    ],
    [*Postconditions*], [
      - Password updated.
      - All existing sessions revoked for security.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Clicks Forgot Password on the login page.
      2. System -- Displays a form requesting the registered email address.
      3. Customer -- Enters their email and submits.
      4. System -- Always displays a confirmation message (whether or not the email is registered, to prevent enumeration).
      5. System -- If the email is registered, generates a time-limited, single-use reset token and sends it via email.
      6. Customer -- Opens the email and clicks the reset link.
      7. System -- Verifies that the token is valid and has not expired.
      8. System -- Displays a new password form.
      9. Customer -- Enters and confirms a new password.
      10. Customer -- Submits the form.
      11. System -- Validates the password strength and updates the password.
      12. System -- Revokes all active sessions for the user.
      13. System -- Confirms the password reset and prompts the customer to log in with the new password.
    ],
    [*Alternative Flows*], [
      A1. Reset token has expired -- System displays a message that the link is no longer valid and prompts the customer to request a new reset.
      A2. Customer attempts to reuse a consumed token -- System displays the expired message and prompts a new reset request.
      A3. New password does not meet strength requirements -- System highlights the requirements and prompts retry.
    ],
    [*Exception Flows*], [
      E1. Reset email fails to send -- System displays a generic confirmation (to prevent enumeration) but logs the failure internally.
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
    [*Goal*], [While authenticated, enter the current password and a new password to update credentials.],
    [*Trigger*], [Customer navigates to the security settings and selects Change Password.],
    [*Preconditions*], [
      - Customer is authenticated.
      - Customer knows the current password.
    ],
    [*Postconditions*], [
      - Password changed.
      - Current session remains active.
      - All other sessions revoked.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Navigates to account security settings.
      2. Customer -- Selects Change Password.
      3. System -- Displays a form requesting the current password and a new password.
      4. Customer -- Enters the current password and the new password (with confirmation).
      5. Customer -- Submits the form.
      6. System -- Verifies the current password is correct.
      7. System -- Validates the new password meets strength requirements.
      8. System -- Updates the password.
      9. System -- Revokes all active sessions except the current one.
      10. System -- Confirms the password change.
    ],
    [*Alternative Flows*], [
      A1. Current password is incorrect -- System rejects and prompts the customer to retry.
      A2. New password is the same as the current password -- System rejects and prompts the customer to choose a different password.
      A3. New password does not meet strength requirements -- System highlights the requirements and prompts retry.
    ],
    [*Exception Flows*], [
      E1. System fails to update the password -- System reports the failure and suggests retrying.
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
    [*Goal*], [Before the access token expires, renew the session without requiring re-login.],
    [*Trigger*], [The client application detects the access token is about to expire and requests a refresh.],
    [*Preconditions*], [
      - Customer has an active session with a valid refresh token.
    ],
    [*Postconditions*], [
      - Session extended.
      - New access token and refresh token issued.
      - Previous refresh token invalidated.
    ],
    [*Main Success Scenario*], [
      1. System (client) -- Detects that the access token will expire soon.
      2. System (client) -- Sends the current refresh token to the token endpoint.
      3. System -- Validates the refresh token (not expired, not previously consumed).
      4. System -- Issues a new access token with a fresh expiry.
      5. System -- Issues a new refresh token and invalidates the previous one (token rotation).
      6. System (client) -- Stores the new token pair and continues the session.
    ],
    [*Alternative Flows*], [
      A1. Refresh token has expired -- System rejects and the client redirects the customer to the login page.
      A2. Refresh token has been consumed (reuse detected) -- System revokes all active tokens for the user and the client redirects to the login page with a security notification.
    ],
    [*Exception Flows*], [
      E1. Token issuance fails -- System reports the failure; the client retains the existing token pair if the access token is still valid.
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
      - Current session terminated.
      - Refresh token invalidated.
      - Access token will no longer be accepted if used.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Clicks Logout in the storefront navigation.
      2. System -- Sends the current refresh token for invalidation.
      3. System -- Invalidates the refresh token.
      4. System -- Removes the session cookie from the client.
      5. System -- Redirects the customer to the storefront home page.
    ],
    [*Alternative Flows*], [
      A1. Customer's access token is already expired -- System still invalidates the refresh token and clears the session cookie.
      A2. Logout request fails due to network issue -- System clears local tokens and cookies on the client side; the session will naturally expire.
    ],
    [*Exception Flows*], [
      E1. Token invalidation fails on the server -- System clears local tokens and cookies on the client side; the server-side tokens will expire naturally.
    ],
    [*Related Requirements*], [IDN-FR-05, IDN-FR-16],
  ),
  caption: [UC-STR-SES-02 -- Logout.],
)
