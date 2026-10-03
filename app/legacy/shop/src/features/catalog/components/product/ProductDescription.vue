<script setup lang="ts">
import { computed } from 'vue'
import Divider from 'primevue/divider'

interface Props {
  shortDescription?: string
  longDescription?: string
}

const props = defineProps<Props>()

const hasLongDescription = computed(() => props.longDescription && props.longDescription.length > 0)
</script>

<template>
  <div class="product-description">
    <div v-if="shortDescription" class="short-description">
      <p>{{ shortDescription }}</p>
    </div>

    <div v-if="hasLongDescription" class="long-description">
      <Divider align="left">
        <span class="p-divider__title">Description</span>
      </Divider>
      <div class="description-content">
        <p>{{ longDescription }}</p>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.product-description {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.short-description {
  p {
    font-size: var(--font-size-base);
    line-height: 1.7;
    color: var(--color-text);
  }
}

.long-description {
  :deep(.p-divider) {
    margin-bottom: 1rem;
    padding-bottom: 0.75rem;
  }

  :deep(.p-divider-content) {
    background: transparent;
  }

  .p-divider__title {
    font-size: var(--font-size-lg);
    font-weight: var(--font-weight-semibold);
    color: var(--color-text);
    padding: 0;
    background: transparent;
  }

  .description-content {
    p {
      font-size: var(--font-size-base);
      line-height: 1.8;
      color: var(--color-text-secondary);
      white-space: pre-line;
    }
  }
}
</style>
