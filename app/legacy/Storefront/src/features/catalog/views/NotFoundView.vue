<script setup lang="ts">
import { useRouter } from 'vue-router'
import Button from 'primevue/button'

const router = useRouter()
</script>

<template>
  <div class="not-found-view">
    <div class="content">
      <div class="error-code">404</div>
      <h1>Page Not Found</h1>
      <p>Sorry, we couldn't find the page you're looking for.</p>
      
      <div class="actions">
        <Button label="Go Home" @click="router.push('/')" />
        <Button label="Go Back" severity="secondary" outlined @click="router.back()" />
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.not-found-view {
  min-height: calc(100vh - 80px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  text-align: center;
}

.content {
  max-width: 500px;
}

.error-code {
  font-family: var(--font-display);
  font-size: 8rem;
  font-weight: var(--font-weight-bold);
  line-height: 1;
  background: linear-gradient(135deg, var(--color-primary) 0%, var(--color-primary-light) 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin-bottom: 1rem;
}

h1 {
  font-size: var(--font-size-3xl);
  margin-bottom: 1rem;
}

p {
  color: var(--color-text-muted);
  margin-bottom: 2rem;
  font-size: var(--font-size-lg);
}

.actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
}
</style>
