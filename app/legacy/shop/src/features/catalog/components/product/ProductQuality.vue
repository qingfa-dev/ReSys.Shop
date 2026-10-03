<script setup lang="ts">
interface QualityFeature {
  icon: string
  title: string
  description: string
}

interface Props {
  features?: QualityFeature[]
  warranty?: string
  certifications?: string[]
  shipping?: string
  returns?: string
}

const props = defineProps<Props>()

const hasContent = computed(() => {
  return (props.features && props.features.length > 0) ||
    props.warranty ||
    (props.certifications && props.certifications.length > 0) ||
    props.shipping ||
    props.returns
})

import { computed } from 'vue'
</script>

<template>
  <div v-if="hasContent" class="product-quality">
    <div v-if="features && features.length > 0" class="features-section">
      <div v-for="feature in features" :key="feature.title" class="feature-item">
        <i :class="feature.icon" class="feature-icon"></i>
        <div class="feature-content">
          <h4>{{ feature.title }}</h4>
          <p>{{ feature.description }}</p>
        </div>
      </div>
    </div>

    <div class="quality-badges">
      <div v-if="certifications && certifications.length > 0" class="badge-group">
        <span class="badge-label">Certifications:</span>
        <span v-for="cert in certifications" :key="cert" class="badge">{{ cert }}</span>
      </div>
    </div>

    <div class="policy-section">
      <div v-if="warranty" class="policy-item">
        <i class="pi pi-shield"></i>
        <div>
          <h4>Warranty</h4>
          <p>{{ warranty }}</p>
        </div>
      </div>

      <div v-if="shipping" class="policy-item">
        <i class="pi pi-truck"></i>
        <div>
          <h4>Shipping</h4>
          <p>{{ shipping }}</p>
        </div>
      </div>

      <div v-if="returns" class="policy-item">
        <i class="pi pi-refresh"></i>
        <div>
          <h4>Returns</h4>
          <p>{{ returns }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.product-quality {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.features-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

.feature-item {
  display: flex;
  gap: 0.75rem;
  padding: 1rem;
  background: var(--color-surface-ground);
  border-radius: var(--radius-md);

  .feature-icon {
    font-size: 1.5rem;
    color: var(--color-primary);
    flex-shrink: 0;
  }

  h4 {
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-semibold);
    margin-bottom: 0.25rem;
  }

  p {
    font-size: var(--font-size-xs);
    color: var(--color-text-muted);
    line-height: 1.4;
  }
}

.quality-badges {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;

  .badge-group {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    flex-wrap: wrap;
  }

  .badge-label {
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
  }

  .badge {
    padding: 0.25rem 0.5rem;
    background: var(--color-surface-ground);
    border-radius: var(--radius-sm);
    font-size: var(--font-size-xs);
    color: var(--color-text-secondary);
  }
}

.policy-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid var(--color-border-light);
}

.policy-item {
  display: flex;
  gap: 0.75rem;
  align-items: flex-start;

  i {
    font-size: 1.25rem;
    color: var(--color-primary);
    margin-top: 2px;
  }

  h4 {
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-semibold);
    margin-bottom: 0.25rem;
  }

  p {
    font-size: var(--font-size-xs);
    color: var(--color-text-muted);
    line-height: 1.4;
  }
}
</style>
