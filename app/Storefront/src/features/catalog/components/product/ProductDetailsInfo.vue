<script setup lang="ts">
import { computed } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Divider from 'primevue/divider'

interface ProductDetailItem {
  label: string
  value: string
}

interface Props {
  material?: string
  careInstructions?: string[]
  dimensions?: string
  weight?: string
  origin?: string
  additionalDetails?: ProductDetailItem[]
}

const props = defineProps<Props>()

const detailSections = computed(() => {
  const sections: { title: string; items: ProductDetailItem[] }[] = []

  if (props.material || props.careInstructions?.length || props.dimensions || props.weight || props.origin) {
    const items: ProductDetailItem[] = []
    
    if (props.material) {
      items.push({ label: 'Material', value: props.material })
    }
    if (props.careInstructions?.length) {
      items.push({ label: 'Care Instructions', value: props.careInstructions.join(', ') })
    }
    if (props.dimensions) {
      items.push({ label: 'Dimensions', value: props.dimensions })
    }
    if (props.weight) {
      items.push({ label: 'Weight', value: props.weight })
    }
    if (props.origin) {
      items.push({ label: 'Country of Origin', value: props.origin })
    }
    
    sections.push({ title: 'Product Details', items })
  }

  if (props.additionalDetails?.length) {
    sections.push({ title: 'Additional Information', items: props.additionalDetails })
  }

  return sections
})

const hasDetails = computed(() => detailSections.value.length > 0)
</script>

<template>
  <div v-if="hasDetails" class="product-details-info">
    <div v-for="section in detailSections" :key="section.title" class="detail-section">
      <Divider align="left">
        <span class="section-title">{{ section.title }}</span>
      </Divider>
      <DataTable :value="section.items" striped class="detail-table" tableStyle="min-width: 50rem">
        <Column field="label" header="Detail" class="detail-label">
          <template #body="slotProps">
            <span class="detail-label-text">{{ slotProps.data.label }}</span>
          </template>
        </Column>
        <Column field="value" header="" class="detail-value">
          <template #body="slotProps">
            <span class="detail-value-text">{{ slotProps.data.value }}</span>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>

<style scoped lang="scss">
.product-details-info {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.detail-section {
  :deep(.p-divider) {
    margin-bottom: 1rem;
    padding-bottom: 0;
  }

  :deep(.p-divider-content) {
    background: transparent;
    padding: 0;
  }

  .section-title {
    font-size: var(--font-size-lg);
    font-weight: var(--font-weight-semibold);
    color: var(--color-text);
    background: transparent;
    padding: 0;
  }
}

.detail-table {
  :deep(.p-datatable-thead > tr > th) {
    display: none;
  }

  :deep(.p-datatable-tbody > tr) {
    border-bottom: 1px solid var(--color-border-light);
    
    &:last-child {
      border-bottom: none;
    }

    > td {
      padding: 0.75rem 0;
      border: none;
    }
  }

  .detail-label {
    width: 40%;
  }

  .detail-label-text {
    font-weight: var(--font-weight-medium);
    color: var(--color-text-muted);
    font-size: var(--font-size-sm);
  }

  .detail-value {
    width: 60%;
  }

  .detail-value-text {
    color: var(--color-text);
    font-size: var(--font-size-sm);
    line-height: 1.5;
  }
}
</style>
