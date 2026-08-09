<script setup lang="ts">
import { ref } from 'vue'
import { useForm, useField } from 'vee-validate'
import { toFormValidator } from '@vee-validate/zod'
import { useAuthStore } from '../stores/authStore'
import { RegisterFormSchema } from '../validations'
import type { RegisterForm } from '../validations'
import { usePasswordStrength } from '../composables/usePasswordStrength'

// Store: Auth store owns the registration request.
const auth = useAuthStore()

// Form: Wire vee-validate to the existing zod register schema.
const { handleSubmit, isSubmitting, errors, defineField } = useForm<RegisterForm>({
  validationSchema: toFormValidator(RegisterFormSchema),
  initialValues: { fullName: '', email: '', password: '', confirmPassword: '' },
})

// Fields: Two-way model refs with per-field attrs for the inputs.
const [fullName, fullNameAttrs] = defineField('fullName')
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

// Submit: Delegate to the store; keep the form until the user signs in.
const onSubmit = handleSubmit(async values => {
  apiError.value = null
  const ok = await auth.register({
    fullName: values.fullName,
    email: values.email,
    password: values.password,
  })
  if (ok) {
    registered.value = true
  } else {
    apiError.value = auth.error ?? 'Registration failed'
  }
})
</script>

<template>
  <form v-if="!registered" class="flex flex-col gap-4" novalidate @submit="onSubmit">
    <!-- Section: Form Fields — name, email and passwords with float labels -->
    <FloatLabel variant="on">
      <InputText
        id="fullName"
        v-model="fullName"
        v-bind="fullNameAttrs"
        fluid
        autocomplete="name"
        :invalid="!!errors.fullName"
      />
      <Label for="fullName">Full name</Label>
    </FloatLabel>
    <Message v-if="errors.fullName" severity="error" size="small" variant="simple">
      {{ errors.fullName }}
    </Message>

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
