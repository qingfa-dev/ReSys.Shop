<script setup lang="ts">
import Label from 'primevue/label'
import { ref } from 'vue'
import { useForm, useField } from 'vee-validate'
import { toFormValidator } from '@vee-validate/zod'
import { useAuthStore } from '../stores/authStore'
import { RegisterFormSchema } from '../validations'
import type { RegisterForm } from '../validations'
import { usePasswordStrength } from '../composables/usePasswordStrength'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'

// Store: Auth store owns the registration request.
const auth = useAuthStore()
const { applyFieldErrors } = useApiErrorHandler()

// Form: Wire vee-validate to the existing zod register schema.
const { handleSubmit, isSubmitting, errors, defineField, setFieldError } = useForm<RegisterForm>({
  validationSchema: toFormValidator(RegisterFormSchema),
  initialValues: { firstName: '', lastName: '', email: '', password: '', confirmPassword: '' },
})

// Fields: Two-way model refs with per-field attrs for the inputs.
const [firstName, firstNameAttrs] = defineField('firstName')
const [lastName, lastNameAttrs] = defineField('lastName')
const [email, emailAttrs] = defineField('email')
const [password, passwordAttrs] = defineField('password')
const [confirmPassword, confirmPasswordAttrs] = defineField('confirmPassword')

// Terms: Field-level rule keeps the checkbox outside the zod schema.
const { value: agreeToTerms, errorMessage: termsError } = useField<boolean | null>(
  'agreeToTerms',
  value => (value === true ? true : 'You must agree to the terms'),
  { initialValue: false },
)

// Meter: Live strength feedback for the first password field.
const strengthInfo = usePasswordStrength(password)

// Feedback: Inline message state for API errors and success.
const apiError = ref<string | null>(null)
const registered = ref(false)

// UserName: Sanitize the email local part to the backend username pattern.
function deriveUserName(email: string): string {
  const local = (email.split('@')[0] ?? '').replace(/[^a-zA-Z0-9._-]/g, '').replace(/^[^a-zA-Z0-9]+|[^a-zA-Z0-9]+$/g, '')
  if (local.length >= 3) return local.slice(0, 32)
  return `user${Math.random().toString(36).slice(2, 10)}`
}

// Submit: Delegate to the store; keep the form until the user signs in.
const onSubmit = handleSubmit(async values => {
  apiError.value = null
  const ok = await auth.register({
    email: values.email,
    userName: deriveUserName(values.email),
    password: values.password,
    firstName: values.firstName,
    lastName: values.lastName,
    acceptTerm: agreeToTerms.value === true,
  })
  if (ok) {
    registered.value = true
  } else {
    // Map: Push field-scoped backend errors into their inputs
    const remaining = applyFieldErrors(auth.errors, (f, m) => setFieldError(f, m))
    apiError.value = remaining.length > 0 ? remaining.map(e => e.message).join(' ') : (auth.error ?? 'Registration failed')
  }
})
</script>

<template>
  <form v-if="!registered" class="flex flex-col gap-4" novalidate @submit="onSubmit">
    <!-- Section: Form Fields — name, email and passwords with float labels -->
    <div class="flex gap-3">
      <FloatLabel variant="on" class="w-full">
        <InputText
          id="firstName"
          v-model="firstName"
          v-bind="firstNameAttrs"
          fluid
          autocomplete="given-name"
          :invalid="!!errors.firstName"
        />
        <Label for="firstName">First name</Label>
      </FloatLabel>
      <FloatLabel variant="on" class="w-full">
        <InputText
          id="lastName"
          v-model="lastName"
          v-bind="lastNameAttrs"
          fluid
          autocomplete="family-name"
          :invalid="!!errors.lastName"
        />
        <Label for="lastName">Last name</Label>
      </FloatLabel>
    </div>
    <div class="flex flex-col gap-1">
      <Message v-if="errors.firstName" severity="error" size="small" variant="simple">
        {{ errors.firstName }}
      </Message>
      <Message v-if="errors.lastName" severity="error" size="small" variant="simple">
        {{ errors.lastName }}
      </Message>
    </div>

    <FloatLabel variant="on">
      <InputText
        id="email"
        v-model="email"
        v-bind="emailAttrs"
        type="email"
        fluid
        autocomplete="email"
        :invalid="!!errors.email"
      />
      <Label for="email">Email</Label>
    </FloatLabel>
    <Message v-if="errors.email" severity="error" size="small" variant="simple">
      {{ errors.email }}
    </Message>

    <FloatLabel variant="on">
      <InputPassword
        id="password"
        v-model="password"
        v-bind="passwordAttrs"
        fluid
        autocomplete="new-password"
        :invalid="!!errors.password"
      />
      <Label for="password">Password</Label>
    </FloatLabel>
    <Message v-if="errors.password" severity="error" size="small" variant="simple">
      {{ errors.password }}
    </Message>

    <!-- Section: Strength Meter — live feedback as the password improves -->
    <div v-if="strengthInfo" class="flex flex-col gap-1">
      <ProgressBar
        :value="strengthInfo.percent"
        :show-value="false"
        :pt="{ value: { style: { backgroundColor: strengthInfo.color } } }"
        style="height: 6px"
      />
      <div class="flex justify-end">
        <Tag :severity="strengthInfo.severity" :value="strengthInfo.label" />
      </div>
    </div>

    <FloatLabel variant="on">
      <InputPassword
        id="confirmPassword"
        v-model="confirmPassword"
        v-bind="confirmPasswordAttrs"
        fluid
        autocomplete="new-password"
        :invalid="!!errors.confirmPassword"
      />
      <Label for="confirmPassword">Confirm password</Label>
    </FloatLabel>
    <Message v-if="errors.confirmPassword" severity="error" size="small" variant="simple">
      {{ errors.confirmPassword }}
    </Message>

    <!-- Section: Terms Consent — checkbox with link to the terms page -->
    <div class="flex items-center gap-2">
      <Checkbox inputId="agree-to-terms" v-model="agreeToTerms" binary />
      <Label for="agree-to-terms" class="text-sm text-muted">I agree to the</Label>
      <Button as="router-link" to="/terms" text size="small" label="Terms of Service" />
    </div>
    <Message v-if="termsError" severity="error" size="small" variant="simple">
      {{ termsError }}
    </Message>

    <!-- Section: Feedback — inline message for API errors -->
    <Message v-if="apiError" severity="error" :closable="false">{{ apiError }}</Message>

    <Button type="submit" label="Create Account" fluid :loading="isSubmitting" />

    <!-- Section: Login Link — text button to the sign-in route -->
    <div class="flex justify-center">
      <Button as="router-link" to="/login" text size="small" label="Sign in" />
    </div>
  </form>

  <!-- Section: Success State — confirmation shown once the account is created -->
  <div v-else class="flex flex-col gap-4">
    <Message severity="success" :closable="false">Account created. Please sign in.</Message>
    <div class="flex justify-center">
      <Button as="router-link" to="/login" text size="small" label="Sign in" icon="pi pi-arrow-right" iconPos="right" />
    </div>
  </div>
</template>
