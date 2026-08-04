<script setup lang="ts">
import { ref } from 'vue'
import type { TaxonTreeNode } from '../types/taxon'

defineProps<{ nodes: TaxonTreeNode[] }>()
const emit = defineEmits<{ select: [taxonId: string] }>()

// State: Track expanded taxon ids within this subtree
const expandedIds = ref<Set<string>>(new Set())

// Trigger: Toggle expansion of a taxon node
function toggle(node: TaxonTreeNode): void {
  const next = new Set(expandedIds.value)
  if (next.has(node.id)) {
    next.delete(node.id)
  } else {
    next.add(node.id)
  }
  expandedIds.value = next
}

// Map: Whether a taxon node is currently expanded
function isExpanded(node: TaxonTreeNode): boolean {
  return expandedIds.value.has(node.id)
}
</script>
<template>
  <!-- Section: Category Tree -->
  <ul class="space-y-1">
    <li v-for="node in nodes" :key="node.id">
      <button
        class="flex items-center gap-2 w-full text-left px-2 py-1.5 rounded text-sm hover:bg-stone-100 transition-colors"
        :class="{ 'font-semibold text-stone-900': node.depth === 0 }"
        @click="emit('select', node.id)"
      >
        <i
          v-if="node.hasChildren"
          class="pi text-xs text-stone-400 transition-transform"
          :class="isExpanded(node) ? 'pi-chevron-down' : 'pi-chevron-right'"
          @click.stop="toggle(node)"
        />
        <span v-else class="w-3" />
        {{ node.presentation ?? node.name }}
      </button>
      <CategoryTree
        v-if="node.hasChildren && isExpanded(node)"
        :nodes="node.children"
        class="ml-4"
        @select="(id) => emit('select', id)"
      />
    </li>
  </ul>
</template>
