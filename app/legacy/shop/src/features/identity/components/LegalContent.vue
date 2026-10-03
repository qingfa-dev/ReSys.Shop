<script setup lang="ts">
defineProps<{
  title: string
  lastUpdated?: string
}>()
</script>

<template>
  <div class="legal-view">
    <div class="legal-container">
      <header class="legal-header">
        <h1>{{ title }}</h1>
        <p v-if="lastUpdated" class="last-updated">Last updated: {{ lastUpdated }}</p>
      </header>
      
      <div class="legal-content">
        <slot />
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.legal-view {
  min-height: calc(100vh - 80px);
  padding: 3rem 2rem;
  background: var(--color-surface-ground);
}

.legal-container {
  max-width: 800px;
  margin: 0 auto;
  background: var(--color-surface);
  border-radius: var(--radius-xl);
  padding: 3rem;
  box-shadow: var(--shadow-lg);
}

.legal-header {
  margin-bottom: 2rem;
  padding-bottom: 1.5rem;
  border-bottom: 1px solid var(--color-border-light);

  h1 {
    font-size: var(--font-size-3xl);
    margin-bottom: 0.5rem;
  }

  .last-updated {
    color: var(--color-text-muted);
    font-size: var(--font-size-sm);
  }
}

.legal-content {
  :deep(h2) {
    font-size: var(--font-size-xl);
    margin: 2rem 0 1rem;
    color: var(--color-text);
  }

  :deep(h3) {
    font-size: var(--font-size-lg);
    margin: 1.5rem 0 0.75rem;
    color: var(--color-text);
  }

  :deep(p) {
    margin-bottom: 1rem;
    line-height: 1.7;
    color: var(--color-text-secondary);
  }

  :deep(ul),
  :deep(ol) {
    margin: 1rem 0;
    padding-left: 1.5rem;

    li {
      margin-bottom: 0.5rem;
      line-height: 1.6;
      color: var(--color-text-secondary);
    }
  }

  :deep(a) {
    color: var(--color-primary);
    text-decoration: none;

    &:hover {
      text-decoration: underline;
    }
  }

  :deep(.highlight) {
    background: var(--color-surface-ground);
    padding: 0.25rem 0.5rem;
    border-radius: var(--radius-sm);
    font-weight: var(--font-weight-medium);
  }
}
</style>