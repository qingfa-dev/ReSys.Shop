<script setup lang="ts">
import RegisterForm from '@/features/identity/components/RegisterForm.vue'
</script>

<template>
  <div class="register-view">
    <div class="register-card">
      <div class="card-header">
        <h1>Create Account</h1>
        <p>Join us and start shopping</p>
      </div>

      <RegisterForm />

      <div class="card-footer">
        <p>Already have an account? <a href="/login">Sign in</a></p>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.register-view {
  min-height: calc(100vh - 80px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  background: var(--color-surface-ground);
}

.register-card {
  width: 100%;
  max-width: 480px;
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
</style>