# Task 12 - Review Finding Fix Report

## Issue: Wrong i18n key, hardcoded string, dead destructuring in Payment components

**Finding date:** 2026-07-25

### Problems
1. **PaymentDetailForm.vue** — `t('payment.payments.detail.actions.back')` used as title prefix, producing "Back #<order>" instead of "Payment #<order>".
2. **PaymentMethodListTable.vue** — hardcoded string `'this payment method'` in `confirmDelete({ target: ... })` instead of i18n key.
3. **PaymentDetailForm.vue** — dead `confirmDelete` destructuring from `useConfirm()` (never called in the component).

### Fix applied
1. **PaymentDetailForm.vue** — Changed title prefix key from `t('payment.payments.detail.actions.back')` to `t('payment.payments.detail.title_prefix')`.
2. **PaymentMethodListTable.vue** — Replaced `target: 'this payment method'` with `target: t('payment.methods.messages.delete_confirm_target')`.
3. **PaymentDetailForm.vue** — Removed unused `import { useConfirm }` and dead `const { confirmDelete } = useConfirm()`.
4. **payment.json** — Added `payments.detail.title_prefix: "Payment"` and `methods.messages.delete_confirm_target: "this payment method"`.

### Files changed
- `app/Admin/src/features/payment/components/PaymentDetailForm.vue`
- `app/Admin/src/features/payment/components/PaymentMethodListTable.vue`
- `app/Admin/src/shared/localization/messages/en/payment.json`
