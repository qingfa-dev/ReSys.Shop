# Replace Tailwind Classes with PrimeVue Native Props — Admin Auth Forms

## Problem

The 3 auth pages (Login, ForgotPassword, ResetPassword) use Tailwind/CSS classes (`p-4 text-lg w-full`) for input and button sizing instead of PrimeVue's built-in `size` and `fluid` props. Labels are raw HTML elements with Tailwind styling instead of PrimeVue's `FloatLabel` component. This duplicates work that the Aura theme already handles through design tokens.

## Solution

Replace Tailwind sizing classes with PrimeVue native props on InputText, InputPassword, and Button. Replace raw `<label>` elements with `FloatLabel variant="on"` for floating label UX.

---

## 1. Props Mapping

| Current (Tailwind) | Replacement (PrimeVue) | Applies to |
|---|---|---|
| `class="w-full p-4 text-lg"` | `fluid size="large"` | InputText, Button |
| `:pt="{ input: { class: 'p-4 text-lg' } }"` | `size="large"` | InputPassword |
| `class="w-full"` (on input) | `fluid` | InputText, InputPassword |
| `class="mb-8"`, `class="mb-4"` | Keep as Tailwind (layout spacing) | FloatLabel wrappers |
| `<label class="block text-2xl font-medium mb-2">` | `<FloatLabel variant="on"><label>` | All form labels |

Aura theme design tokens handle font-size and padding natively:
- `--p-inputtext-lg-font-size`, `--p-inputtext-lg-padding-x/y`
- `--p-button-lg-font-size`, `--p-button-lg-padding-x/y`
- FloatLabel handles label animation, positioning, focus states

---

## 2. FloatLabel Pattern

Before:
```html
<label class="block text-surface-900 dark:text-surface-0 text-2xl font-medium mb-2">Email</label>
<IconField class="w-full mb-4">
  <InputIcon><Envelope /></InputIcon>
  <InputText class="w-full p-4 text-lg" ... />
</IconField>
```

After:
```html
<FloatLabel variant="on" class="w-full mb-4">
  <IconField>
    <InputIcon><Envelope /></InputIcon>
    <InputText fluid size="large" ... />
  </IconField>
  <label>Email</label>
</FloatLabel>
```

Error `<small>` elements stay outside FloatLabel, unchanged.

---

## 3. Files Changed

| File | Changes |
|---|---|
| `features/auth/views/LoginPage.vue` | 2 InputText, 1 InputPassword, 1 Button, 3 labels → FloatLabel. Import FloatLabel from `primevue/floatlabel`. |
| `features/auth/views/ForgotPasswordPage.vue` | 1 InputText, 1 Button, 1 label → FloatLabel. Import FloatLabel. |
| `features/auth/views/ResetPasswordPage.vue` | 4 InputText (3 disabled), 1 InputPassword, 1 Button, 4 labels → FloatLabel. Import FloatLabel. |

Removed from all 3 files: `p-4 text-lg` classes, `w-full` on inputs/buttons, `:pt` on InputPassword, raw label Tailwind classes. Imports for `IconField` and `InputIcon` remain.

---

## 4. What's NOT Changed

- Tailwind for layout spacing (`mb-*`, `gap-*`, `flex`, `flex-col`)
- Error `<small>` elements (not PrimeVue components)
- Checkbox styling
- AuthLayout card wrapper
- Vue script logic (no behavior changes)
- All 37 stub feature pages (unchanged)
- Widget components (StatCard, etc. — separate phase)
- Navigation components (UserMenu, AppMenu, etc.)

---

## 5. Verification

- `pnpm run build` — zero TypeScript errors
- `pnpm run test:unit -- run` — all tests pass (no behavior changes)
- `pnpm run lint` — no new warnings
- Visual check: login form renders with large inputs and floating labels
