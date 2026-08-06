<script setup lang="ts">
import { watch } from 'vue'
import { useSearch } from '../composables/useSearch'
import { formatCurrency } from '@/shared/utils/currency'

const search = useSearch()

// Trigger: Debounced search when query changes.
watch(() => search.query.value, () => search.search())

// Map: Keyboard navigation.
function onKeyDown(e: KeyboardEvent): void {
  if (e.key === 'ArrowDown') {
    e.preventDefault()
    search.selectedIndex.value = Math.min(search.selectedIndex.value + 1, search.results.value.length - 1)
  } else if (e.key === 'ArrowUp') {
    e.preventDefault()
    search.selectedIndex.value = Math.max(search.selectedIndex.value - 1, 0)
  } else if (e.key === 'Enter') {
    search.navigateToResult(search.selectedIndex.value)
  } else if (e.key === 'Escape') {
    search.close()
  }
}

// Map: Format price for display.
function displayPrice(price: number | null): string {
  return price != null ? formatCurrency(price) : 'Contact'
}
</script>
<template>
  <!-- Section: Search Overlay -->
  <Dialog
    :visible="search.isOpen.value"
    modal
    :style="{ width: '600px' }"
    :breakpoints="{ '768px': '100vw' }"
    :pt="{ root: 'border-0', content: 'px-0 pb-0' }"
    @update:visible="(val: boolean) => { if (!val) search.close() }"
  >
    <template #header>
      <span class="p-input-icon-left w-full">
        <i class="pi pi-search text-stone-400" />
        <InputText
          :model-value="search.query.value"
          placeholder="Search products..."
          class="w-full border-0 shadow-none text-lg"
          autofocus
          @update:model-value="(val: string | undefined) => { if (val !== undefined) search.query.value = val }"
          @keydown="onKeyDown"
        />
      </span>
    </template>
    <!-- Section: Loading State -->
    <div v-if="search.loading.value" class="px-6 py-4 space-y-3">
      <div v-for="i in 3" :key="i" class="flex items-center gap-3 animate-pulse">
        <div class="w-12 h-12 bg-stone-200 rounded-lg shrink-0" />
        <div class="flex-1 space-y-1.5">
          <div class="h-4 bg-stone-200 rounded w-3/4" />
          <div class="h-3 bg-stone-200 rounded w-1/4" />
        </div>
      </div>
    </div>
    <!-- Section: Results -->
    <ul v-else-if="search.results.value.length > 0" class="divide-y divide-stone-100">
      <li
        v-for="(item, idx) in search.results.value"
        :key="item.id"
        class="flex items-center gap-3 px-6 py-3 cursor-pointer transition-colors"
        :class="idx === search.selectedIndex.value ? 'bg-teal-50' : 'hover:bg-stone-50'"
        @click="search.navigateToResult(idx)"
      >
        <img
          v-if="item.masterVariant?.images?.[0]?.url"
          :src="item.masterVariant.images[0].url"
          :alt="item.masterVariant.images[0].alt ?? item.name"
          class="w-12 h-12 rounded-lg object-cover bg-stone-100 shrink-0"
        />
        <div v-else class="w-12 h-12 rounded-lg bg-stone-100 flex items-center justify-center shrink-0">
          <i class="pi pi-image text-stone-400" />
        </div>
        <div class="flex-1 min-w-0">
          <p class="text-sm font-medium text-stone-900 truncate">{{ item.name }}</p>
          <p class="text-sm font-semibold text-stone-700">{{ displayPrice(item.masterVariant?.price ?? null) }}</p>
        </div>
      </li>
    </ul>
    <!-- Section: Empty State -->
    <div v-else-if="search.query.value.trim() && !search.loading.value" class="px-6 py-8 text-center">
      <p class="text-stone-500">No products found for "{{ search.query.value }}"</p>
    </div>
    <!-- Section: View All Footer -->
    <div v-if="search.query.value.trim() && search.results.value.length > 0" class="px-6 py-3 border-t border-stone-100">
      <router-link :to="`/shop?search=${encodeURIComponent(search.query.value)}`" class="text-sm text-teal-600 hover:text-teal-700 font-medium" @click="search.close()">
        View all results for "{{ search.query.value }}" &rarr;
      </router-link>
    </div>
  </Dialog>
</template>
