<script setup lang="ts">
import { ref, computed } from 'vue'
import type { TaxonTreeNode } from '../types'

const props = withDefaults(defineProps<{
  nodes: TaxonTreeNode[]
  selectedIds: string[]
  depth?: number
  showSearch?: boolean
  maxVisible?: number
}>(), {
  depth: 0,
  showSearch: false,
  maxVisible: 5,
})

const emit = defineEmits<{
  toggle: [id: string]
}>()

const expandedNodeIds = ref<Set<string>>(new Set())
const searchQuery = ref('')
const showAll = ref(false)

function matchesSearch(node: TaxonTreeNode, q: string): boolean {
  if (!q) return true
  const lower = q.toLowerCase()
  return node.name.toLowerCase().includes(lower)
    || node.children.some(c => matchesSearch(c, lower))
}

const filteredNodes = computed(() => {
  if (!props.showSearch || !searchQuery.value) return props.nodes
  return props.nodes.filter(n => matchesSearch(n, searchQuery.value))
})

const visibleNodes = computed(() => {
  if (!props.maxVisible || showAll.value) return filteredNodes.value
  return filteredNodes.value.slice(0, props.maxVisible)
})

const hasMore = computed(() =>
  props.maxVisible > 0 && filteredNodes.value.length > props.maxVisible
)

function toggleExpand(nodeId: string): void {
  if (expandedNodeIds.value.has(nodeId)) {
    expandedNodeIds.value.delete(nodeId)
  } else {
    expandedNodeIds.value.add(nodeId)
  }
}

function isExpanded(nodeId: string): boolean {
  return expandedNodeIds.value.has(nodeId)
}
</script>
<template>
  <div>
    <!-- Search input for taxonomy filtering -->
    <div v-if="showSearch && nodes.length > 3" class="mb-3">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText
          v-model="searchQuery"
          placeholder="Search..."
          size="small"
          class="w-full text-xs"
        />
      </IconField>
    </div>

    <!-- Taxonomy tree nodes -->
    <ul :class="depth === 0 ? 'space-y-0.5' : ''">
      <li v-for="node in visibleNodes" :key="node.id">
        <div
          class="group flex items-center gap-1 rounded-md py-1 pr-1 transition-colors hover:bg-neutral-100"
          :style="{ paddingLeft: `${depth * 16}px` }"
        >
          <!-- Expand/collapse chevron for nodes with children -->
          <Button
            v-if="node.hasChildren"
            :icon="isExpanded(node.id) ? 'pi pi-chevron-down' : 'pi pi-chevron-right'"
            severity="secondary"
            text
            rounded
            size="small"
            class="!p-0 !w-5 !h-5"
            @click="toggleExpand(node.id)"
          />
          <span v-else class="h-5 w-5 shrink-0" />

          <!-- Checkbox + label -->
          <div class="flex flex-1 min-w-0 items-center gap-2 cursor-pointer py-0.5">
            <Checkbox
              :model-value="selectedIds.includes(node.id)"
              binary
              size="small"
              @change="emit('toggle', node.id)"
            />
            <span
              class="truncate text-sm"
              :class="selectedIds.includes(node.id) ? 'font-semibold text-neutral-900' : 'text-neutral-700'"
              @click="emit('toggle', node.id)"
            >{{ node.name }}</span>
            <!-- Child count badge -->
            <Tag
              v-if="node.hasChildren && node.children.length > 0"
              :value="String(node.children.length)"
              severity="secondary"
              class="ml-auto shrink-0 !text-[10px]"
            />
          </div>
        </div>

        <!-- Recursive children (only when expanded) -->
        <template v-if="node.hasChildren && isExpanded(node.id)">
          <TaxonTree
            :nodes="node.children"
            :selected-ids="selectedIds"
            :depth="depth + 1"
            :max-visible="0"
            @toggle="emit('toggle', $event)"
          />
        </template>
      </li>
    </ul>

    <!-- Show more / Show less toggle -->
    <Button
      v-if="hasMore"
      :label="showAll ? 'Show less' : `Show all ${filteredNodes.length} items`"
      severity="secondary"
      text
      size="small"
      class="mt-1 text-xs"
      :style="{ paddingLeft: `${depth * 16 + 20}px` }"
      @click="showAll = !showAll"
    />

    <!-- No results from search -->
    <p
      v-if="showSearch && searchQuery && filteredNodes.length === 0"
      class="py-2 text-center text-xs text-neutral-400"
    >
      No matches
    </p>
  </div>
</template>
