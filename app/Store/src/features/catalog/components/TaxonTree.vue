<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { TreeNode } from 'primevue/treenode'
import { useCatalogStore } from '../stores/catalogStore'
import type { TaxonTreeNode } from '../types'

const props = defineProps<{
  nodes: TaxonTreeNode[]
}>()

const catalog = useCatalogStore()

// Map: Convert taxonomy nodes to PrimeVue TreeNode shape (key = taxon id).
function toTreeNode(node: TaxonTreeNode): TreeNode {
  return {
    key: node.id,
    label: node.name,
    leaf: node.children.length === 0 ? true : undefined,
    children: node.children.length > 0 ? node.children.map(toTreeNode) : undefined,
  }
}

const treeNodes = computed<TreeNode[]>(() => props.nodes.map(toTreeNode))

// Expand: Roots with children start expanded; preserve keys the user toggled.
const expandedKeys = ref<Record<string, boolean>>({})
watch(
  () => props.nodes,
  (nodes) => {
    const seed: Record<string, boolean> = {}
    for (const node of nodes) {
      if (node.hasChildren) seed[node.id] = true
    }
    expandedKeys.value = { ...seed, ...expandedKeys.value }
  },
  { immediate: true, deep: true },
)

// Select: Mirror store selection into checkbox keys ({ checked, partialChecked }).
const selectionKeys = computed<Record<string, { checked: boolean; partialChecked: boolean }>>({
  get: () => {
    const keys: Record<string, { checked: boolean; partialChecked: boolean }> = {}
    for (const id of catalog.selectedTaxonIds) {
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
    // Diff: Toggle every taxon whose membership changed (partial parents excluded).
    // Snapshot: Iterate a copy — toggleTaxon splices the live array, so a direct
    // loop skips the element after each removal (catalogStore.ts:40).
    // oxlint-disable-next-line unicorn/no-useless-spread -- false positive: the copy is required
    for (const id of [...catalog.selectedTaxonIds]) {
      if (!fullyChecked.has(id)) catalog.toggleTaxon(id)
    }
    for (const id of fullyChecked) {
      if (!catalog.selectedTaxonIds.includes(id)) catalog.toggleTaxon(id)
    }
  },
})
</script>

<template>
  <!-- Section: Taxon Tree — checkbox tree; store is the single source of truth -->
  <Tree
    v-model:expanded-keys="expandedKeys"
    v-model:selection-keys="selectionKeys"
    :value="treeNodes"
    selection-mode="checkbox"
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
</template>
