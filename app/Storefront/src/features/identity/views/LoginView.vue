<script setup lang="ts">
import LoginForm from '@/features/identity/components/LoginForm.vue'
</script>

<template>
  <div class="login-view">
    <div class="login-card">
      <div class="card-header">
        <h1>Welcome Back</h1>
        <p>Sign in to your account</p>
      </div>

      <LoginForm />

      <div class="card-footer">
        <p>Don't have an account? <a href="/register">Create one</a></p>
      </div>

      <!-- MVP: dropped — no frontend or backend implementation for external OAuth -->
      <div v-if="false">
        <div class="social-login">
          <p>Or continue with</p>
          <div class="social-buttons">
            <Button label="Google" icon="pi pi-google" severity="secondary" outlined />
            <Button label="Facebook" icon="pi pi-facebook" severity="secondary" outlined />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.login-view {
  min-height: calc(100vh - 80px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  background: var(--color-surface-ground);
}

.login-card {
  width: 100%;
  max-width: 440px;
  background: var(--color-surface);
  border-radius: var(--radius-xl);
  padding: 2.5rem;
  box-shadow: var(--shadow-lg);
}

.card-header {
  text-align: center;
  margin-bottom: 2rem;
  
  h1 {
    font-size: var(--font-size-2xl);
    margin-bottom: 0.5rem;
  }
  
  p {
    color: var(--color-text-muted);
  }
}

.card-footer {
  text-align: center;
  margin-top: 1.5rem;
  padding-top: 1.5rem;
  border-top: 1px solid var(--color-border-light);
  
  p {
    font-size: var(--font-size-sm);
    color: var(--color-text-muted);
  }
  
  a {
    color: var(--color-primary);
    font-weight: var(--font-weight-medium);
    
    &:hover {
      text-decoration: underline;
    }
  }
}

.social-login {
  margin-top: 1.5rem;
  text-align: center;
  
  p {
    font-size: var(--font-size-sm);
    color: var(--color-text-muted);
    margin-bottom: 1rem;
  }
}

.social-buttons {
  display: flex;
  gap: 1rem;
  
  :deep(.p-button) {
    flex: 1;
  }
}
</style>