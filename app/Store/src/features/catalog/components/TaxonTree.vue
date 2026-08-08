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

// Filter: Case-insensitive name search across taxonomy tree
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
    <div v-if="showSearch && nodes.length > 3" class="relative mb-3">
      <svg
        xmlns="http://www.w3.org/2000/svg"
        class="absolute left-2.5 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-neutral-400"
        fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"
      >
        <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
      </svg>
      <input
        v-model="searchQuery"
        type="text"
        placeholder="Search..."
        class="w-full rounded-lg border border-neutral-200 py-1.5 pl-8 pr-3 text-xs text-neutral-700 outline-none transition placeholder:text-neutral-400 focus:border-neutral-400"
      />
    </div>

    <!-- Taxonomy tree nodes -->
    <ul :class="depth === 0 ? 'space-y-0.5' : ''">
      <li v-for="node in visibleNodes" :key="node.id">
        <div
          class="group flex items-center gap-1 rounded-md py-1 pr-1 transition-colors hover:bg-neutral-100"
          :style="{ paddingLeft: `${depth * 16}px` }"
        >
          <!-- Expand/collapse chevron for nodes with children -->
          <button
            v-if="node.hasChildren"
            class="flex h-5 w-5 shrink-0 items-center justify-center rounded text-neutral-400 transition-colors hover:text-neutral-700"
            @click="toggleExpand(node.id)"
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              class="h-3 w-3 transition-transform duration-200"
              :class="{ 'rotate-90': isExpanded(node.id) }"
              fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="3"
            >
              <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
            </svg>
          </button>
          <span v-else class="h-5 w-5 shrink-0" />

          <!-- Checkbox -->
          <label class="flex flex-1 min-w-0 items-center gap-2 cursor-pointer py-0.5">
            <input
              type="checkbox"
              :checked="selectedIds.includes(node.id)"
              class="h-3.5 w-3.5 shrink-0 rounded border-neutral-300 text-teal-600 focus:ring-teal-500"
              @change="emit('toggle', node.id)"
            />
            <span class="truncate text-sm" :class="selectedIds.includes(node.id) ? 'font-semibold text-neutral-900' : 'text-neutral-700'">
              {{ node.name }}
            </span>
            <!-- Child count badge -->
            <span
              v-if="node.hasChildren && node.children.length > 0"
              class="ml-auto shrink-0 rounded-full bg-neutral-100 px-1.5 py-0.5 text-[10px] leading-none text-neutral-400"
            >
              {{ node.children.length }}
            </span>
          </label>
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
    <button
      v-if="hasMore"
      class="mt-1 w-full py-1 text-xs font-medium text-neutral-400 transition-colors hover:text-neutral-700"
      :style="{ paddingLeft: `${depth * 16 + 20}px` }"
      @click="showAll = !showAll"
    >
      {{ showAll ? 'Show less' : `Show all ${filteredNodes.length} items` }}
    </button>

    <!-- No results from search -->
    <p
      v-if="showSearch && searchQuery && filteredNodes.length === 0"
      class="py-2 text-center text-xs text-neutral-400"
    >
      No matches
    </p>
  </div>
</template>
