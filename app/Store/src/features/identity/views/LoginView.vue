<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useForm } from 'vee-validate'
import { toFormValidator } from '@vee-validate/zod'
import Eye from '@primeicons/vue/eye'
import EyeSlash from '@primeicons/vue/eye-slash'
import Label from 'primevue/label'
import { useAuthStore } from '../stores/authStore'
import { LoginFormSchema } from '../validations'
import type { LoginForm } from '../validations'
import FieldMessage from '@/shared/components/FieldMessage.vue'
import { validateRedirect } from '@/shared/utils/postLoginRedirect'

// Stores: Auth store owns session state; router resumes the guarded target.
const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

// Form: Wire vee-validate to the existing zod login schema.
const { handleSubmit, isSubmitting, errors, defineField } = useForm<LoginForm>({
  validationSchema: toFormValidator(LoginFormSchema),
  initialValues: { credential: '', password: '' },
})

// Fields: Two-way model refs with per-field attrs for the inputs.
const [credential, credentialAttrs] = defineField('credential')
const [password, passwordAttrs] = defineField('password')

// UI: Remember-me is cosmetic for now; mask toggles password visibility.
const rememberMe = ref(false)
const mask = ref(true)

// Feedback: Inline message state for API errors and success.
const apiError = ref<string | null>(null)
const loginSuccess = ref(false)

// Submit: Delegate to the store, then resume the pre-login route on success.
const onSubmit = handleSubmit(async values => {
  apiError.value = null
  const ok = await auth.login(values.credential, values.password)
  if (ok) {
    loginSuccess.value = true
    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : null
    await router.replace(validateRedirect(redirect))
  } else {
    apiError.value = auth.error ?? 'Invalid credentials'
  }
})
</script>

<template>
  <form class="flex flex-col gap-4" novalidate @submit="onSubmit">
    <!-- Section: Form Fields — credential and password with float labels -->
    <FloatLabel variant="on">
      <InputText
        id="credential"
        v-model="credential"
        v-bind="credentialAttrs"
        type="text"
        fluid
        autocomplete="username"
        :invalid="!!errors.credential"
      />
      <Label for="credential">Email or Username</Label>
    </FloatLabel>
    <FieldMessage :error="errors.credential" />

    <FloatLabel variant="on">
      <IconField>
        <InputPassword
          id="password"
          v-model="password"
          v-bind="passwordAttrs"
          :mask="mask"
          fluid
          autocomplete="current-password"
          :invalid="!!errors.password"
        />
        <InputIcon class="cursor-pointer" @click="mask = !mask">
          <Eye v-if="mask" />
          <EyeSlash v-else />
        </InputIcon>
      </IconField>
      <Label for="password">Password</Label>
    </FloatLabel>
    <FieldMessage :error="errors.password" />

    <!-- Section: Form Actions — remember me, forgot-password link and submit -->
    <div class="flex items-center justify-between gap-4">
      <div class="flex items-center gap-2">
        <Checkbox inputId="remember-me" v-model="rememberMe" binary />
        <Label for="remember-me">Remember me</Label>
      </div>
      <Button as="router-link" to="/forgot-password" text size="small" label="Forgot password?" />
    </div>

    <!-- Section: Feedback — inline messages for API errors and success -->
    <Message v-if="apiError" severity="error" :closable="false">{{ apiError }}</Message>
    <Message v-if="loginSuccess" severity="success" :closable="false">
      Signed in successfully.
    </Message>

    <Button type="submit" label="Sign In" fluid :loading="isSubmitting" />

    <!-- Section: Register Link — text button to the create-account route -->
    <Divider align="center">or</Divider>
    <div class="flex justify-center">
      <Button as="router-link" to="/register" text size="small" label="Create account" />
    </div>
  </form>
</template>
