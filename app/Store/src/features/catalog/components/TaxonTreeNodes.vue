<script setup lang="ts">
import { ref } from 'vue'
import type { TaxonTreeNode } from '../types/taxon'

defineProps<{
  nodes: TaxonTreeNode[]
  selectedIds: string[]
}>()
const emit = defineEmits<{ toggle: [id: string] }>()

// State: Track expanded tree nodes
const expandedIds = ref<Set<string>>(new Set())

// Trigger: Toggle expansion of a tree node
function toggleExpand(id: string): void {
  const next = new Set(expandedIds.value)
  if (next.has(id)) next.delete(id)
  else next.add(id)
  expandedIds.value = next
}

// Map: Whether a node is expanded
function isExpanded(id: string): boolean {
  return expandedIds.value.has(id)
}
</script>
<template>
  <div class="space-y-0.5">
    <template v-for="node in nodes" :key="node.id">
      <label class="flex items-center gap-2 text-sm text-stone-700 cursor-pointer">
        <input
          type="checkbox"
          class="rounded border-stone-300 text-stone-900 focus:ring-stone-900"
          :checked="selectedIds.includes(node.id)"
          @change="emit('toggle', node.id)"
        />
        {{ node.presentation ?? node.name }}
        <button
          v-if="node.hasChildren"
          class="pi text-xs text-stone-400 transition-transform ml-auto"
          :class="isExpanded(node.id) ? 'pi-chevron-down' : 'pi-chevron-right'"
          @click.stop="toggleExpand(node.id)"
        />
      </label>
      <div v-if="node.hasChildren && isExpanded(node.id)" class="ml-5">
        <TaxonTreeNodes
          :nodes="node.children"
          :selected-ids="selectedIds"
          @toggle="(id) => emit('toggle', id)"
        />
      </div>
    </template>
  </div>
</template>
