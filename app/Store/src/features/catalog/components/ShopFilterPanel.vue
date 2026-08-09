<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { TreeNode } from 'primevue/treenode'
import { useFilters } from '../composables/useFilters'
import { useTaxonomy } from '../composables/useTaxonomy'
import { formatCurrency } from '@/shared/utils/currency'
import type { StoreOptionValueListItemResponse, TaxonTreeNode } from '../types'
import type { WritableComputedRef } from 'vue'

const catalogFilters = useFilters()
const taxonomy = useTaxonomy()

// Term: Tree filter is internal state (no external binding needed)
// Kept for potential future use with filterValue prop if PrimeVue adds it

// Tree: Convert taxonomy groups into a single flat Tree with taxonomy names as root nodes
function toTreeNode(node: TaxonTreeNode): TreeNode {
  return {
    key: node.id,
    label: node.presentation ?? node.name,
    leaf: node.children.length === 0 ? true : undefined,
    children: node.children.length > 0 ? node.children.map(toTreeNode) : undefined,
  }
}

const treeNodes = computed<TreeNode[]>(() =>
  taxonomy.taxonomyGroups.map(group => ({
    key: `taxonomy-${group.taxonomy.id}`,
    label: group.taxonomy.presentation ?? group.taxonomy.name,
    children: group.tree.map(toTreeNode),
  })),
)

// Expand: Taxonomy root nodes start expanded; preserve user toggles
const expandedKeys = ref<Record<string, boolean>>({})
watch(
  treeNodes,
  (groups) => {
    const seed: Record<string, boolean> = {}
    for (const group of groups) {
      if (group.children?.length) seed[group.key] = true
    }
    expandedKeys.value = { ...seed, ...expandedKeys.value }
  },
  { immediate: true },
)

// Select: Mirror composable selection into checkbox keys
const selectionKeys = computed<Record<string, { checked: boolean; partialChecked: boolean }>>({
  get: () => {
    const keys: Record<string, { checked: boolean; partialChecked: boolean }> = {}
    for (const id of catalogFilters.selectedTaxonIds) {
      keys[id] = { checked: true, partialChecked: false }
    }
    return keys
  },
  set: (keys) => {
    const fullyChecked = new Set(
      Object.entries(keys)
        .filter(([, meta]) => meta && meta.checked && !meta.partialChecked)
        .map(([key]) => key),
    )
    // oxlint-disable-next-line unicorn/no-useless-spread -- false positive: the copy is required
    for (const id of [...catalogFilters.selectedTaxonIds]) {
      if (!fullyChecked.has(id)) catalogFilters.toggleTaxon(id)
    }
    for (const id of fullyChecked) {
      if (!catalogFilters.selectedTaxonIds.includes(id)) catalogFilters.toggleTaxon(id)
    }
  },
})

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
    <!-- Section: Product Search — filters products by name/description -->
    <IconField>
      <InputIcon class="pi pi-search" />
      <InputText
        :model-value="catalogFilters.searchQuery"
        type="search"
        placeholder="Search products..."
        class="w-full"
        @update:model-value="catalogFilters.setSearch($event)"
      />
    </IconField>

    <!-- Section: Taxonomy Tree — checkbox tree grouped by taxonomy root, with built-in filter -->
    <Tree
      v-model:expanded-keys="expandedKeys"
      v-model:selection-keys="selectionKeys"
      :value="treeNodes"
      selection-mode="checkbox"
      filter
      filter-mode="lenient"
      filter-placeholder="Search categories..."
    >
      <template #default="{ node }">
        <div class="flex w-full items-center gap-2">
          <span class="truncate text-sm">{{ node.label }}</span>
          <Tag
            v-if="node.children?.length"
            :value="String(node.children.length)"
            severity="secondary"
            size="small"
            class="ml-auto shrink-0"
          />
        </div>
      </template>
    </Tree>

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
          fluid
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
