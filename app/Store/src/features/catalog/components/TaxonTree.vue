<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { TreeNode } from 'primevue/treenode'
import type { TaxonTreeNode } from '../types'

const props = withDefaults(defineProps<{
  nodes: TaxonTreeNode[]
  selectedIds: string[]
  showSearch?: boolean
}>(), {
  showSearch: false,
})

const emit = defineEmits<{
  toggle: [id: string]
}>()

// Map: Convert taxonomy tree into PrimeVue TreeNode structure (key = taxon id)
function toTreeNode(node: TaxonTreeNode): TreeNode {
  return {
    key: node.id,
    label: node.name,
    leaf: !node.hasChildren,
    children: node.children.length > 0 ? node.children.map(toTreeNode) : undefined,
  }
}

const treeNodes = computed<TreeNode[]>(() => props.nodes.map(toTreeNode))

// State: Per-node expand/collapse keys — root nodes expanded by default
const expandedKeys = ref<Record<string, boolean>>({})

watch(treeNodes, (nodes) => {
  const next: Record<string, boolean> = {}
  for (const n of nodes) {
    if (n.children?.length) next[n.key] = true
  }
  expandedKeys.value = next
}, { immediate: true })

// Selection: Sync PrimeVue checkbox cascade with the catalog store
const selectionKeys = computed({
  get: () => {
    const keys: Record<string, { checked: boolean; partialChecked: boolean }> = {}
    for (const id of props.selectedIds) {
      keys[id] = { checked: true, partialChecked: false }
    }
    return keys
  },
  set: (keys: Record<string, { checked: boolean; partialChecked: boolean }>) => {
    // Filter: Keep only fully checked keys (ignore partial-checked parents)
    const checkedIds = Object.keys(keys).filter(k => keys[k].checked)
    const prev = new Set(props.selectedIds)
    for (const id of checkedIds) {
      if (!prev.has(id)) emit('toggle', id)
    }
    for (const id of props.selectedIds) {
      if (!checkedIds.includes(id)) emit('toggle', id)
    }
  },
})
</script>
<template>
  <Tree
    v-model:expanded-keys="expandedKeys"
    v-model:selection-keys="selectionKeys"
    :nodes="treeNodes"
    selection-mode="checkbox"
    :filter="showSearch"
    filter-placeholder="Search..."
    filter-mode="lenient"
    class="!border-none !bg-transparent !p-0 !text-sm"
  >
    <!-- Node content — toggler and checkbox are rendered by Tree itself -->
    <template #default="{ node }">
      <div class="flex min-w-0 flex-1 items-center gap-2 pr-2">
        <span class="truncate text-sm text-neutral-700">{{ node.label }}</span>
        <Tag
          v-if="node.children && node.children.length > 0"
          :value="String(node.children.length)"
          severity="secondary"
          class="ml-auto shrink-0 !py-0 !text-[10px]"
        />
      </div>
    </template>
  </Tree>
</template>
