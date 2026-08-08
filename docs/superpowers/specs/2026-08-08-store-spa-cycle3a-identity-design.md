# Store SPA Rebuild — Cycle 3a: Identity

Date: 2026-08-08
Scope: Identity domain — Login, Register, Forgot/Reset password, Sessions
Tier: 3a of 3 (Identity → Ordering → Profile)

## Visual Direction (inherited from Cycle 1)

Minimal clean e-commerce. Neutral palette, white cards, subtle borders.
All identity views use AuthLayout (centered card with ReSys.Shop logo).
Vee-validate + Zod for form validation — inline field errors, blur validation, submit button disabled until valid.

## Views

### 1. LoginView

**Layout:** AuthLayout wrapper. Form card with logo, email field, password field, submit button, forgot password link, Google button, register link.

**Form:**
- Email: `InputText` with vee-validate `Field`, Zod: `email().min(1)`
- Password: `Password` component with toggle feedback icon, Zod: `min(1, "Required")`
- Forgot password: `router-link` to `/forgot-password`, text-xs, right-aligned
- Submit: `Button`, full-width, teal, label "Sign In", disabled until form valid. Spinner on submit.
- Divider: `border-t border-neutral-200` with "or" text in center
- Google: `Button`, outlined, full-width, icon="pi pi-google", label "Continue with Google". Calls `authStore.loginWithGoogle()`.
- Register link: "Don't have an account? Register" — router-link to `/register`

**States:**
- Idle: Both fields empty, submit disabled (opacity-50, cursor-not-allowed)
- Valid: Submit enabled, full teal
- Submitting: Spinner icon + "Signing in..."
- Field error: Red border + red text below field (ErrorMessage)
- Server error: Toast notification (e.g. "Invalid email or password")
- Success: Router push to stored redirect query param (or `/`), Toast "Welcome back, [name]"

**Data:** `authStore.login(credential, password)`. Route meta: `guestOnly: true`.

### 2. RegisterView

**Layout:** AuthLayout wrapper. Form card with "Create your account" heading, fields, submit, Google, sign-in link.

**Form:**
- Full name: `InputText`, Zod: `min(1, "Required")`
- Email: `InputText`, Zod: `email()`
- Password: `Password`, Zod: `min(8, "Min 8 characters")`
- Confirm password: `Password`, Zod: `.refine((data: any) => data.password === data.confirmPassword, "Passwords must match")`
- Submit: "Create Account", teal, full-width, disabled until valid
- Divider + Google button (same as Login)
- Sign-in link: "Already have an account? Sign In" → `/login`

**States:** Same pattern as Login. Success → auto-login → redirect `/`.

**Data:** `authStore.register(fullName, email, password)`. Route meta: `guestOnly: true`.

### 3. ForgotPasswordView

**Layout:** AuthLayout wrapper. Form card with "Reset your password" heading, descriptive text, email field, submit, success state.

**Form:**
- Description: "Enter your email and we'll send you a link to reset your password." (text-sm, text-neutral-500)
- Email: `InputText`, Zod: `email()`
- Submit: "Send Reset Link", teal, full-width

**States:**
- After submit (success): Replace form with success message — green CheckCircle icon, "Check your email — We sent a reset link to [email]", "[Resend]" link (30s cooldown)
- Server error: Toast "No account found with that email" (should still show success for security — but per current authStore, actual result tells us)
- Back link: "← Back to Sign In" → `/login`

**Data:** `authStore.forgotPassword(email)`. Route meta: `guestOnly: true`.

### 4. ResetPasswordView

**Layout:** AuthLayout wrapper. Form card with "Set new password" heading, two password fields, submit.

**Form:**
- New password: `Password`, Zod: `min(8)`
- Confirm password: `Password`, must match
- Hidden field: token (from URL query `?token=xxx&email=xxx`)
- Submit: "Set New Password", teal, full-width

**States:**
- Success: Toast "Password reset successfully. Please sign in." → redirect `/login`
- Server error: Toast "Invalid or expired reset token"
- Missing token: Error state — "Invalid reset link. Request a new one." with link to `/forgot-password`

**Data:** `authStore.resetPassword(token, newPassword)`. Route meta: `guestOnly: true`.

### 5. SessionsView

**Layout:** AccountLayout wrapper (sidebar active on Sessions). Page title "Active Sessions".

**Content:**
- Session list: Card per session with device icon, browser + OS name, location, last active time, "Current session" badge (Tag) or "Revoke" button
- Device icons: pi pi-desktop (desktop), pi pi-mobile (mobile), pi pi-tablet (tablet)
- Revoke button: `Button`, outlined, severity="danger", size="small", label "Revoke"
- Revoke All: Text button "Revoke All Other Sessions" at bottom, confirms via PrimeVue ConfirmDialog

**States:**
- Loading: 3 Skeleton rows
- Empty: "No other active sessions" (when only current session exists)
- Error: Error message with retry

**Data:** `sessionApi.getSessions()`, `sessionApi.revokeSession(id)`, `sessionApi.revokeAll()`. Route meta: `requiresAuth: true`.

## Shared Pattern

All form views follow the same pattern:
1. `useForm` from vee-validate with Zod schema via `@vee-validate/zod`
2. `Field` component wraps each `InputText`/`Password`
3. `ErrorMessage` component below each field with Tailwind `text-red-600 text-xs mt-1`
4. Submit button: teal, full-width, `:disabled="!meta.valid || isSubmitting"`, loading spinner on submit
5. `handleSubmit` calls store action, shows Toast on error, redirects on success
6. Google button (Login + Register only): outlined, full-width, icon="pi pi-google"

## Testing

Smoke tests for each view:
1. LoginView renders email + password fields + sign in button
2. RegisterView renders 4 fields + create account button
3. ForgotPasswordView renders email field + send reset link button
4. ResetPasswordView renders 2 password fields + set new password button
5. SessionsView renders session list (mocked)
