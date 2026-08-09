<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useFilters } from '../composables/useFilters'
import { useTaxonomy } from '../composables/useTaxonomy'
import { formatCurrency } from '@/shared/utils/currency'
import TaxonTree from './TaxonTree.vue'
import type { StoreOptionValueListItemResponse, TaxonTreeNode } from '../types'
import type { WritableComputedRef } from 'vue'

const catalogFilters = useFilters()
const taxonomy = useTaxonomy()

// Term: Keep local taxonomy search for future use; v5 Tree filter is internal state
// only (no filterValue prop), so TaxonTree renders its own search input instead
const taxonSearchTerm = ref('')

// Options: Map of option value ids per type for group-scoped selection proxies
const valueIdsByType = computed(() => {
  const map = new Map<string, Set<string>>()
  for (const type of taxonomy.optionTypes) {
    map.set(type.id, new Set(type.values.map(v => v.id)))
  }
  return map
})

// Proxy: Bridge the composable's global option selection to a per-type array for widget v-model
function optionSelection(optionTypeId: string): WritableComputedRef<string[]> {
  return computed<string[]>({
    get: () => {
      const ids = valueIdsByType.value.get(optionTypeId)
      if (!ids) return []
      return catalogFilters.selectedOptionValueIds.filter(id => ids.has(id))
    },
    set: (next) => {
      const ids = valueIdsByType.value.get(optionTypeId)
      if (!ids) return
      for (const id of ids) {
        if (catalogFilters.selectedOptionValueIds.includes(id) !== next.includes(id)) {
          catalogFilters.toggleOptionValue(id)
        }
      }
    },
  })
}

// Group: Option type with its selection proxy for CheckboxGroup or MultiSelect binding
interface OptionFilterGroup {
  id: string
  label: string
  values: StoreOptionValueListItemResponse[]
  selection: WritableComputedRef<string[]>
}

const optionFilterGroups = computed<OptionFilterGroup[]>(() =>
  taxonomy.optionTypes.map(type => ({
    id: type.id,
    label: type.presentation ?? type.name,
    values: type.values,
    selection: optionSelection(type.id),
  })),
)

// Helper: Resolve an option value by id for active-filter chip labels
function findOptionValue(id: string): StoreOptionValueListItemResponse | undefined {
  for (const type of taxonomy.optionTypes) {
    const value = type.values.find(v => v.id === id)
    if (value) return value
  }
  return undefined
}

// Helper: Resolve a taxon by id across every taxonomy group tree for chip labels
function findTaxon(id: string, nodes: TaxonTreeNode[] = taxonomy.taxonomyGroups.flatMap(g => g.tree)): TaxonTreeNode | undefined {
  for (const node of nodes) {
    if (node.id === id) return node
    const child = findTaxon(id, node.children)
    if (child) return child
  }
  return undefined
}

// Bounds: Fixed price slider range covering the catalogue price span
const priceBounds = { min: 0, max: 1000 }
const priceRange = ref<[number, number]>([priceBounds.min, priceBounds.max])

// Sync: Seed the local range from composable prices and reset when cleared externally
watch(
  () => [catalogFilters.minPrice, catalogFilters.maxPrice] as const,
  ([min, max]) => {
    if (min == null && max == null) {
      priceRange.value = [priceBounds.min, priceBounds.max]
    } else {
      if (min != null) priceRange.value[0] = min
      if (max != null) priceRange.value[1] = max
    }
  },
  { immediate: true },
)

// Commit: Push the local slider state into the composable (emits filter:changed)
function commitPrice(): void {
  catalogFilters.setPriceRange(priceRange.value[0], priceRange.value[1])
}

// Input: Proxy the local slider bounds for the min/max number inputs
const minInput = computed({
  get: () => priceRange.value[0],
  set: (value: number | null) => {
    priceRange.value[0] = value ?? priceBounds.min
  },
})

const maxInput = computed({
  get: () => priceRange.value[1],
  set: (value: number | null) => {
    priceRange.value[1] = value ?? priceBounds.max
  },
})

// Chip: Active filter summary with its per-chip clear callback
interface ActiveFilterChip {
  id: string
  label: string
  clear: () => void
}

const activeChips = computed<ActiveFilterChip[]>(() => {
  const chips: ActiveFilterChip[] = []
  for (const id of catalogFilters.selectedTaxonIds) {
    const node = findTaxon(id)
    chips.push({
      id: `taxon-${id}`,
      label: node ? (node.presentation ?? node.name) : id,
      clear: () => catalogFilters.toggleTaxon(id),
    })
  }
  for (const id of catalogFilters.selectedOptionValueIds) {
    const value = findOptionValue(id)
    chips.push({
      id: `option-${id}`,
      label: value ? (value.presentation ?? value.name) : id,
      clear: () => catalogFilters.toggleOptionValue(id),
    })
  }
  if (catalogFilters.searchQuery) {
    chips.push({
      id: 'search',
      label: `Search: ${catalogFilters.searchQuery}`,
      clear: () => catalogFilters.setSearch(''),
    })
  }
  if (catalogFilters.minPrice != null) {
    chips.push({
      id: 'min-price',
      label: `From ${formatCurrency(catalogFilters.minPrice)}`,
      clear: () => catalogFilters.setPriceRange(null, catalogFilters.maxPrice),
    })
  }
  if (catalogFilters.maxPrice != null) {
    chips.push({
      id: 'max-price',
      label: `To ${formatCurrency(catalogFilters.maxPrice)}`,
      clear: () => catalogFilters.setPriceRange(catalogFilters.minPrice, null),
    })
  }
  return chips
})
</script>

<template>
  <div class="flex flex-col gap-6">
    <!-- Section: Taxon Search — local filter term kept for future use (v5 Tree has no filterValue prop) -->
    <IconField>
      <InputIcon class="pi pi-filter" />
      <InputText
        v-model="taxonSearchTerm"
        type="search"
        placeholder="Search categories..."
        class="w-full"
      />
    </IconField>

    <!-- Section: Taxonomy Tree — one accordion tab per taxonomy group -->
    <Accordion multiple>
      <AccordionPanel
        v-for="group in taxonomy.taxonomyGroups"
        :key="group.taxonomy.id"
        :value="group.taxonomy.id"
      >
        <AccordionHeader>{{ group.taxonomy.presentation ?? group.taxonomy.name }}</AccordionHeader>
        <AccordionContent>
          <TaxonTree :nodes="group.tree" />
        </AccordionContent>
      </AccordionPanel>
    </Accordion>

    <!-- Section: Option Values — checkbox list or multi-select per filterable option type -->
    <Panel
      v-for="group in optionFilterGroups"
      :key="group.id"
      :header="group.label"
      toggleable
    >
      <MultiSelect
        v-if="group.values.length > 8"
        v-model="group.selection.value"
        :options="group.values"
        optionLabel="name"
        optionValue="id"
        :placeholder="`Filter ${group.label}...`"
        class="w-full"
      />
      <CheckboxGroup
        v-else
        v-model="group.selection.value"
        :name="group.id"
        class="flex flex-col gap-2"
      >
        <div v-for="value in group.values" :key="value.id" class="flex items-center gap-2">
          <Checkbox :inputId="`filter-${value.id}`" :value="value.id" />
          <Label
            :for="`filter-${value.id}`"
            class="text-sm text-muted"
          >
            {{ value.presentation ?? value.name }}
          </Label>
        </div>
      </CheckboxGroup>
    </Panel>

    <!-- Section: Price Range — slider with min/max number inputs -->
    <Panel header="Price" toggleable>
      <div class="flex flex-col gap-3">
        <Slider
          v-model="priceRange"
          range
          :min="priceBounds.min"
          :max="priceBounds.max"
          class="w-full"
          @change="commitPrice"
        />
        <div class="flex items-center gap-2">
          <InputNumber
            v-model="minInput"
            :min="priceBounds.min"
            :max="priceBounds.max"
            placeholder="Min"
            class="w-full"
            @update:modelValue="commitPrice"
          />
          <InputNumber
            v-model="maxInput"
            :min="priceBounds.min"
            :max="priceBounds.max"
            placeholder="Max"
            class="w-full"
            @update:modelValue="commitPrice"
          />
        </div>
      </div>
    </Panel>

    <!-- Section: Active Filters — removable chips, clear-all and active count -->
    <div v-if="catalogFilters.activeFilterCount > 0" class="flex flex-col gap-3">
      <div class="flex items-center justify-between gap-2">
        <span class="text-sm font-semibold text-body">
          Active Filters
        </span>
        <div class="flex items-center gap-2">
          <Tag :value="catalogFilters.activeFilterCount" severity="secondary" />
          <Button
            label="Clear all"
            variant="text"
            size="small"
            @click="catalogFilters.clearFilters()"
          />
        </div>
      </div>
      <div class="flex flex-wrap gap-2">
        <Chip
          v-for="chip in activeChips"
          :key="chip.id"
          :label="chip.label"
          removable
          @remove="chip.clear"
        >
          <template #removeicon="{ removeCallback, keydownCallback }">
            <i
              class="pi pi-times"
              tabindex="0"
              :aria-label="`Remove filter ${chip.label}`"
              @click="removeCallback"
              @keydown="keydownCallback"
            />
          </template>
        </Chip>
      </div>
    </div>
  </div>
</template>
