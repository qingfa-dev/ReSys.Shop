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
import { RegisterFormSchema, type RegisterFormData } from '@/features/identity/types/request'

const router = useRouter()
const { register, isLoading, error: authError } = useAuth()

const formInitialValues: RegisterFormData = {
  email: '',
  password: '',
  confirmPassword: '',
  firstName: '',
  lastName: '',
  phone: '',
  agreeTerms: false,
}

const formError = ref('')

async function onFormSubmit(event: any) {
  if (!event.valid) return

  formError.value = ''

  try {
    await register(
      event.values.email,
      event.values.password,
      event.values.firstName,
      event.values.lastName,
      event.values.phone
    )
    router.push('/')
  } catch {
    formError.value = authError.value || 'Registration failed. Please try again.'
  }
}
</script>

<template>
  <Form
    v-slot="$form"
    :initialValues="formInitialValues"
    :resolver="zodResolver(RegisterFormSchema)"
    @submit="onFormSubmit"
    class="register-form"
  >
    <div v-if="formError" class="error-message">
      <i class="pi pi-exclamation-circle"></i>
      {{ formError }}
    </div>

    <div class="name-fields">
      <FormField v-slot="$field" name="firstName" initialValue="" class="form-field">
        <label for="firstName">First Name</label>
        <InputText id="firstName" placeholder="John" v-bind="$field.props" />
        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
          {{ $field.error?.message }}
        </Message>
      </FormField>

      <FormField v-slot="$field" name="lastName" initialValue="" class="form-field">
        <label for="lastName">Last Name</label>
        <InputText id="lastName" placeholder="Doe" v-bind="$field.props" />
        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
          {{ $field.error?.message }}
        </Message>
      </FormField>
    </div>

    <FormField v-slot="$field" name="email" initialValue="" class="form-field">
      <label for="email">Email</label>
      <InputText id="email" type="email" placeholder="your@email.com" class="w-full" v-bind="$field.props" />
      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
        {{ $field.error?.message }}
      </Message>
    </FormField>

    <FormField v-slot="$field" name="password" initialValue="" class="form-field">
      <label for="password">Password</label>
      <Password id="password" placeholder="Create a password" toggleMask class="w-full" v-bind="$field.props" />
      <small>Must be at least 8 characters</small>
      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
        {{ $field.error?.message }}
      </Message>
    </FormField>

    <FormField v-slot="$field" name="confirmPassword" initialValue="" class="form-field">
      <label for="confirmPassword">Confirm Password</label>
      <Password id="confirmPassword" placeholder="Confirm your password" :feedback="false" toggleMask class="w-full" v-bind="$field.props" />
      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
        {{ $field.error?.message }}
      </Message>
    </FormField>

    <FormField v-slot="$field" name="agreeTerms" initialValue="false" class="form-field">
      <label class="agree-terms">
        <Checkbox v-model="$field.value" v-bind="$field.props" :binary="true" />
        <span>I agree to the <a href="/terms">Terms</a> and <a href="/privacy">Privacy Policy</a></span>
      </label>
      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">
        {{ $field.error?.message }}
      </Message>
    </FormField>

    <Button
      label="Create Account"
      type="submit"
      :loading="isLoading"
      size="large"
      class="submit-btn"
    />
  </Form>
</template>

<style scoped lang="scss">
.register-form {
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

.name-fields {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
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

  small {
    font-size: var(--font-size-xs);
    color: var(--color-text-muted);
  }
}

.agree-terms {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  cursor: pointer;

  a {
    color: var(--color-primary);

    &:hover {
      text-decoration: underline;
    }
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