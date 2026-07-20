<script setup lang="ts">
import { useNewsletter } from '@/app/composables'

const { email, isLoading, isSuccess, error, subscribe } = useNewsletter()
</script>

<template>
  <div class="newsletter-form-wrapper">
    <h4>Newsletter</h4>
    <p v-if="!isSuccess">Subscribe for exclusive offers and updates.</p>
    
    <div v-if="isSuccess" class="newsletter-success">
      <i class="pi pi-check-circle"></i>
      <span>Thank you for subscribing!</span>
    </div>
    
    <form v-else class="newsletter-form" @submit.prevent="subscribe">
      <input 
        v-model="email"
        type="email" 
        placeholder="Enter your email"
        :disabled="isLoading"
        @input="error = ''"
      />
      <button type="submit" :disabled="isLoading">
        <i v-if="isLoading" class="pi pi-spin pi-spinner"></i>
        <i v-else class="pi pi-arrow-right"></i>
      </button>
    </form>
    
    <p v-if="error" class="newsletter-error">{{ error }}</p>
  </div>
</template>

<style scoped lang="scss">
.newsletter-form-wrapper {
  h4 {
    font-family: var(--font-body);
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-semibold);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    margin-bottom: 0.5rem;
    color: var(--color-text);
  }
  
  p {
    color: var(--color-text-muted);
    font-size: var(--font-size-sm);
    margin-bottom: 1rem;
  }
}

.newsletter-success {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--color-success);
  font-size: var(--font-size-sm);
  
  i {
    font-size: var(--font-size-lg);
  }
}

.newsletter-form {
  display: flex;
  gap: 0;
  
  input {
    flex: 1;
    padding: 0.75rem 1rem;
    border: 1px solid var(--color-border);
    border-right: none;
    border-radius: var(--radius-md) 0 0 var(--radius-md);
    background: var(--color-surface-ground);
    color: var(--color-text);
    outline: none;
    font-size: var(--font-size-sm);
    transition: border-color var(--transition-fast);
    
    &:focus {
      border-color: var(--color-primary);
    }
    
    &:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
    
    &::placeholder {
      color: var(--color-text-muted);
    }
  }
  
  button {
    padding: 0.75rem 1rem;
    border: 1px solid var(--color-primary);
    border-radius: 0 var(--radius-md) var(--radius-md) 0;
    background: var(--color-primary);
    color: white;
    transition: background var(--transition-fast);
    cursor: pointer;
    
    &:hover:not(:disabled) {
      background: var(--color-primary-hover);
    }
    
    &:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
    
    i {
      font-size: var(--font-size-base);
    }
  }
}

.newsletter-error {
  color: var(--color-error);
  font-size: var(--font-size-xs);
  margin-top: 0.5rem;
}
</style>
