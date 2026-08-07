<script setup lang="ts">
import { computed, ref } from 'vue'
import type { TaxonomyGroup, TaxonTreeNode } from '../types/taxon'
import type { StoreOptionTypeListItem, StoreOptionValueListItemResponse } from '../types/optionType'
import TaxonTreeNodes from './TaxonTreeNodes.vue'
import FilterPriceRange from './FilterPriceRange.vue'
import { useCatalogStore } from '../stores/catalogStore'

const props = defineProps<{
  taxonomyGroups: TaxonomyGroup[]
  optionTypes: (StoreOptionTypeListItem & { values: StoreOptionValueListItemResponse[] })[]
  selectedTaxonIds: string[]
  selectedOptionValueIds: string[]
}>()
const emit = defineEmits<{ toggleTaxon: [id: string]; toggleOptionValue: [id: string]; clear: [] }>()
const catalog = useCatalogStore()

// State: Track expanded taxonomy sections
const expandedTaxonomyIds = ref<Set<string>>(new Set())

// Map: Whether any filter is currently selected
const hasSelection = computed(() =>
  props.selectedTaxonIds.length > 0
  || props.selectedOptionValueIds.length > 0
  || catalog.minPrice != null
  || catalog.maxPrice != null
)

// Map: Only render option types that are filterable
const filterableTypes = computed(() => props.optionTypes.filter(t => t.filterable))

const priceRange = computed({
  get: () => ({ min: catalog.minPrice, max: catalog.maxPrice }),
  set: (v: { min: number | null; max: number | null }) => catalog.setPriceRange(v.min, v.max),
})

// Map: Find a taxon by ID in a tree (recursive)
function findTaxon(nodes: TaxonTreeNode[], id: string): TaxonTreeNode | null {
  for (const node of nodes) {
    if (node.id === id) return node
    const found = findTaxon(node.children, id)
    if (found) return found
  }
  return null
}

// Map: Resolve taxon name from selected IDs
function getTaxonName(id: string): string {
  for (const group of props.taxonomyGroups) {
    const found = findTaxon(group.tree, id)
    if (found) return found.name
  }
  return id
}

// Map: Resolve option value name from selected IDs
function getOptionName(id: string): string {
  for (const type of props.optionTypes) {
    const found = type.values.find(v => v.id === id)
    if (found) return found.presentation ?? found.name
  }
  return id
}

// Trigger: Toggle expansion of a taxonomy accordion section
function toggleTaxonomy(id: string): void {
  const next = new Set(expandedTaxonomyIds.value)
  if (next.has(id)) next.delete(id)
  else next.add(id)
  expandedTaxonomyIds.value = next
}

// Map: Whether a taxonomy section is expanded
function isTaxonomyExpanded(id: string): boolean {
  return expandedTaxonomyIds.value.has(id)
}
</script>
<template>
  <!-- Section: Sidebar Filters -->
  <div class="space-y-6">
    <!-- Section: Active Filter Chips -->
    <div v-if="hasSelection" class="flex flex-wrap gap-2 mb-4">
      <Chip v-for="id in selectedTaxonIds" :key="id" :label="getTaxonName(id)" removable @remove="emit('toggleTaxon', id)" />
      <Chip v-for="id in selectedOptionValueIds" :key="id" :label="getOptionName(id)" removable @remove="emit('toggleOptionValue', id)" />
    </div>

    <!-- Section: Clear Action -->
    <div v-if="hasSelection" class="flex justify-end">
      <Button label="Clear all" text severity="secondary" size="small" @click="emit('clear')" />
    </div>

    <!-- Section: Taxonomy Groups -->
    <section v-for="group in taxonomyGroups" :key="group.taxonomy.id" class="space-y-1">
      <button
        class="flex items-center gap-2 w-full text-left px-2 py-1.5 rounded text-sm font-semibold text-stone-900 hover:bg-stone-100 transition-colors"
        @click="toggleTaxonomy(group.taxonomy.id)"
      >
        <i
          class="pi text-xs text-stone-400 transition-transform"
          :class="isTaxonomyExpanded(group.taxonomy.id) ? 'pi-chevron-down' : 'pi-chevron-right'"
        />
        {{ group.taxonomy.presentation ?? group.taxonomy.name }}
      </button>
      <div v-if="isTaxonomyExpanded(group.taxonomy.id)" class="ml-2">
        <TaxonTreeNodes
          :nodes="group.tree"
          :selected-ids="selectedTaxonIds"
          @toggle="(id) => emit('toggleTaxon', id)"
        />
      </div>
    </section>

    <!-- Section: Option Type Groups -->
    <section v-for="type in filterableTypes" :key="type.id" class="space-y-2">
      <h3 class="text-sm font-semibold text-stone-900">{{ type.presentation ?? type.name }}</h3>
      <div class="space-y-1">
        <label
          v-for="value in type.values"
          :key="value.id"
          class="flex items-center gap-2 text-sm text-stone-700 cursor-pointer"
        >
          <input
            type="checkbox"
            class="rounded border-stone-300 text-stone-900 focus:ring-stone-900"
            :checked="selectedOptionValueIds.includes(value.id)"
            @change="emit('toggleOptionValue', value.id)"
          />
          {{ value.presentation ?? value.name }}
        </label>
      </div>
    </section>

    <!-- Section: Price Range -->
    <FilterPriceRange v-model="priceRange" />
  </div>
</template>
