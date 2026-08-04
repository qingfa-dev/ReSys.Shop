<script setup lang="ts">
import type { Category, ProductInventory } from '../../types'

interface Props {
  category?: Category
  tags?: string[]
  inventory?: ProductInventory
}

const props = defineProps<Props>()

function formatStock(quantity: number): string {
  return `${quantity} units`
}
</script>

<template>
  <div class="product-details">
    <div class="detail-item">
      <span class="detail-label">Category:</span>
      <span class="detail-value">{{ category?.name || 'N/A' }}</span>
    </div>
    <div class="detail-item">
      <span class="detail-label">Tags:</span>
      <span class="detail-value">{{ tags?.length ? tags.join(', ') : 'N/A' }}</span>
    </div>
    <div class="detail-item">
      <span class="detail-label">Stock:</span>
      <span class="detail-value">{{ inventory ? formatStock(inventory.quantity) : '0 units' }}</span>
    </div>
  </div>
</template>

<style scoped lang="scss">
.product-details {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  font-size: var(--font-size-sm);
}

.detail-item {
  display: flex;
  justify-content: space-between;

  .detail-label {
    color: var(--color-text-muted);
    font-weight: var(--font-weight-semibold);
  }

  .detail-value {
    color: var(--color-text);
  }
}
</style>
