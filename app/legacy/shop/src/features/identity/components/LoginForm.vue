<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Button from 'primevue/button'
import Checkbox from 'primevue/checkbox'
import Message from 'primevue/message'
import { useAuth } from '@/features/identity/composables/useAuth'
import { LoginRequestSchema, type LoginRequest } from '@/features/identity/types/request'

const router = useRouter()
const { login, isLoading, error: authError } = useAuth()

const formInitialValues: LoginRequest = {
  email: '',
  password: '',
  rememberMe: false,
}

const formError = ref('')

async function onFormSubmit(event: any) {
  if (!event.valid) return

  formError.value = ''

  try {
    await login(event.values.email, event.values.password, event.values.rememberMe)
    router.push('/')
  } catch {
    formError.value = authError.value || 'Login failed. Please try again.'
  }
}
</script>

<template>
  <Form
    v-slot="$form"
    :initialValues="formInitialValues"
    :resolver="zodResolver(LoginRequestSchema)"
    @submit="onFormSubmit"
    class="login-form"
  >
    <div v-if="formError" class="error-message">
      <i class="pi pi-exclamation-circle"></i>
      {{ formError }}
    </div>

    <FormField v-slot="$field" name="email" initialValue="" class="form-field">
      <label for="email">Email</label>
      <InputText
        id="email"
        type="email"
        placeholder="your@email.com"
        class="w-full"
        v-bind="$field.props"
      />
      <Message
        v-if="$field?.invalid"
        severity="error"
        size="small"
        variant="simple"
      >
        {{ $field.error?.message }}
      </Message>
    </FormField>

    <FormField v-slot="$field" name="password" initialValue="" class="form-field">
      <label for="password">Password</label>
      <Password
        id="password"
        placeholder="Enter your password"
        :feedback="false"
        toggleMask
        class="w-full"
        v-bind="$field.props"
      />
      <Message
        v-if="$field?.invalid"
        severity="error"
        size="small"
        variant="simple"
      >
        {{ $field.error?.message }}
      </Message>
    </FormField>

    <div class="form-options">
      <label class="remember-me">
        <FormField v-slot="$field" name="rememberMe" initialValue="false">
          <Checkbox v-model="$field.value" v-bind="$field.props" :binary="true" />
        </FormField>
        <span>Remember me</span>
      </label>
      <a href="/forgot-password" class="forgot-link">Forgot password?</a>
    </div>

    <Button
      label="Sign In"
      type="submit"
      :loading="isLoading"
      size="large"
      class="submit-btn"
    />
  </Form>
</template>

<style scoped lang="scss">
.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.error-message {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  background: rgba(220, 38, 38, 0.1);
  border: 1px solid var(--color-danger);
  border-radius: var(--radius-md);
  color: var(--color-danger);
  font-size: var(--font-size-sm);
}

.form-field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;

  label {
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    color: var(--color-text);
  }

  :deep(.p-inputtext),
  :deep(.p-password-input) {
    width: 100%;
    padding: 0.75rem 1rem;
    border-radius: var(--radius-md);
  }
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.remember-me {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: var(--font-size-sm);
  cursor: pointer;
}

.forgot-link {
  font-size: var(--font-size-sm);
  color: var(--color-primary);

  &:hover {
    text-decoration: underline;
  }
}

.submit-btn {
  width: 100%;
  margin-top: 0.5rem;
}

.w-full {
  width: 100%;
}
</style>