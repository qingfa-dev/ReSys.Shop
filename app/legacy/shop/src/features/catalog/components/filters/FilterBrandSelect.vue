<script setup lang="ts">
import { ref, computed } from 'vue'
import AutoComplete from 'primevue/autocomplete'
import Chip from 'primevue/chip'

interface Brand {
  name: string
  slug: string
}

interface Props {
  brands?: Brand[]
  selectedSlugs?: string[]
}

const props = withDefaults(defineProps<Props>(), {
  brands: () => [],
  selectedSlugs: () => [],
})

const emit = defineEmits<{
  (e: 'update:selectedSlugs', slugs: string[]): void
}>()

const searchQuery = ref('')
const filteredBrands = ref<Brand[]>([...props.brands])

const selectedBrands = computed({
  get: () => props.brands.filter(b => props.selectedSlugs.includes(b.slug)),
  set: (value) => emit('update:selectedSlugs', value.map(b => b.slug)),
})

function searchBrands(event: { query: string }) {
  const query = event.query.toLowerCase()
  filteredBrands.value = props.brands.filter(brand =>
    brand.name.toLowerCase().includes(query)
  )
}

function onBrandSelect(event: { value: Brand }) {
  const newSlugs = [...props.selectedSlugs, event.value.slug]
  emit('update:selectedSlugs', newSlugs)
  searchQuery.value = ''
}

function removeBrand(slug: string) {
  const newSlugs = props.selectedSlugs.filter(s => s !== slug)
  emit('update:selectedSlugs', newSlugs)
}

function getBrandName(slug: string) {
  return props.brands.find(b => b.slug === slug)?.name || slug
}
</script>

<template>
  <div class="filter-brand-select">
    <div v-if="selectedBrands.length > 0" class="selected-chips">
      <Chip
        v-for="brand in selectedBrands"
        :key="brand.slug"
        :label="brand.name"
        removable
        @remove="removeBrand(brand.slug)"
        class="brand-chip"
      />
    </div>
    
    <AutoComplete
      v-model="searchQuery"
      :suggestions="filteredBrands"
      @complete="searchBrands"
      @item-select="onBrandSelect"
      option-label="name"
      placeholder="Search brands..."
      :dropdown="false"
      :force-selection="false"
      class="brand-search"
    >
      <template #option="{ option }">
        <div class="brand-option">
          <span>{{ option.name }}</span>
        </div>
      </template>
    </AutoComplete>
    
    <div v-if="selectedBrands.length > 0" class="clear-brands">
      <button @click="emit('update:selectedSlugs', [])">
        Clear all brands
      </button>
    </div>
  </div>
</template>

<style scoped lang="scss">
.filter-brand-select {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.selected-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  
  .brand-chip {
    :deep(.p-chip-text) {
      font-size: var(--font-size-sm);
    }
    
    :deep(.p-chip-remove-icon) {
      margin-left: 0.25rem;
    }
  }
}

.brand-search {
  width: 100%;
  
  :deep(.p-autocomplete-input) {
    width: 100%;
    padding: 0.5rem 0.75rem;
    border: 1px solid var(--color-border);
    border-radius: var(--radius-md);
    font-size: var(--font-size-sm);
    
    &:focus {
      border-color: var(--color-primary);
      box-shadow: 0 0 0 2px rgba(var(--color-primary-rgb), 0.1);
    }
  }
  
  :deep(.p-autocomplete-dropdown) {
    display: none;
  }
}

.brand-option {
  padding: 0.375rem 0;
  font-size: var(--font-size-sm);
}

.clear-brands {
  button {
    background: none;
    border: none;
    color: var(--color-primary);
    font-size: var(--font-size-xs);
    cursor: pointer;
    text-decoration: underline;
    padding: 0;
    
    &:hover {
      text-decoration: none;
    }
  }
}
</style>
