# Store SPA Rebuild — Cycle 3c: Profile

Date: 2026-08-08
Scope: Profile domain — Profile edit, Address book, Change password, Notifications, Preferences, Wishlists
Tier: 3c of 3

## Visual Direction (inherited from Cycle 1)

Minimal clean e-commerce. Neutral palette, white cards, subtle borders.
All views use AccountLayout (left sidebar with active indicator).

## Views

### 1. ProfileView — Edit Profile
Form fields: fullName, email, phone (InputText). Save button + delete account button in Danger Zone section.
Data: profileStore.fetchProfile(), profileStore.updateProfile(req), profileStore.deleteProfile() via ConfirmDialog.
States: Loading (Skeleton), loaded (form pre-filled), saving (spinner).

### 2. AddressBookView — CRUD
Card list of addresses. "Add Address" button opens modal with form fields (name, line1, line2, city, state, zip, country, type, isDefault). Edit/Delete per card. Default badge on primary address.
Data: addressStore.fetchAddresses(), createAddress(), updateAddress(), deleteAddress().

### 3. ChangePasswordView — Form
Current password, new password, confirm password fields. Vee-validate with Zod.
Data: authStore.changePassword(current, new).

### 4. NotificationPrefsView — Toggles
Checkbox toggles for order_updates, new_arrivals, marketing emails. Save button.
Data: notificationApi.getNotificationPrefs(), setNotificationPrefs().

### 5. PreferencesView — Placeholder
Dropdowns for currency, language. Save currently non-functional (API TBD).
Data: preferences schema (read-only for now).

### 6. WishlistsView — Grid
Card grid of wishlist names with item counts and visibility badges. "New Wishlist" button opens create modal.
View button routes to wishlist detail (future).
Data: wishlistStore.fetchWishlists(), createWishlist(), updateWishlist(), deleteWishlist().
