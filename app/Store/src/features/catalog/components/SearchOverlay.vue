<script setup lang="ts">
import { watch, nextTick, ref, onMounted, onUnmounted } from "vue";
import { useSearch } from "../composables/useSearch";

const {
  isOpen,
  query,
  results,
  loading,
  selectedIndex,
  error,
  close,
  clear,
  search: doSearch,
  navigateToResult,
} = useSearch();
const inputRef = ref<HTMLInputElement | null>(null);

// Focus: Auto-focus input when overlay opens.
watch(isOpen, (open) => {
  if (open) nextTick(() => inputRef.value?.focus());
});

// Keyboard: Global handler for overlay navigation.
function onKeyDown(e: KeyboardEvent): void {
  if (!isOpen.value) return;
  if (e.key === "Escape") {
    e.preventDefault();
    close();
    return;
  }
  if (!results.value.length) return;
  if (e.key === "ArrowDown") {
    e.preventDefault();
    selectedIndex.value = (selectedIndex.value + 1) % results.value.length;
  } else if (e.key === "ArrowUp") {
    e.preventDefault();
    selectedIndex.value = (selectedIndex.value - 1 + results.value.length) % results.value.length;
  } else if (e.key === "Enter") {
    e.preventDefault();
    navigateToResult(selectedIndex.value);
  }
}

onMounted(() => document.addEventListener("keydown", onKeyDown));
onUnmounted(() => document.removeEventListener("keydown", onKeyDown));
</script>
<template>
  <!-- Section: Overlay — full-screen search modal with backdrop -->
  <Teleport to="body">
    <div v-if="isOpen" class="fixed inset-0 z-50 flex items-start justify-center pt-[10vh]">
      <div class="absolute inset-0 bg-black/50 backdrop-blur-sm" @click="close()" />
      <!-- Section: Content Card — search input and results -->
      <div
        class="relative bg-white rounded-xl shadow-2xl w-full max-w-2xl mx-4 overflow-hidden max-h-[70vh] flex flex-col"
      >
        <!-- Section: Search Input — debounced text input with icon -->
        <div class="flex items-center gap-3 p-4 border-b border-neutral-100">
          <i class="pi pi-search text-neutral-400" />
          <input
            ref="inputRef"
            type="text"
            :value="query"
            placeholder="Search products..."
            class="w-full text-lg outline-none bg-transparent"
            @input="doSearch()"
          />
          <button
            v-if="query"
            class="text-neutral-400 hover:text-neutral-600 transition-colors"
            @click="clear()"
          >
            <i class="pi pi-times" />
          </button>
        </div>

        <!-- Section: Loading State — spinner during API call -->
        <div v-if="loading" class="p-8 text-center text-neutral-400">
          <i class="pi pi-spin pi-spinner text-2xl mb-2 block" />
          <p class="text-sm">Searching...</p>
        </div>

        <!-- Section: Results List — navigable product results -->
        <div v-else-if="results.length" class="overflow-y-auto max-h-[50vh]">
          <ul>
            <li
              v-for="(item, index) in results"
              :key="item.id"
              class="px-4 py-3 cursor-pointer flex items-center gap-3 border-b border-neutral-50 last:border-0 transition-colors"
              :class="index === selectedIndex ? 'bg-neutral-100' : 'hover:bg-neutral-50'"
              @mouseenter="selectedIndex = index"
              @click="navigateToResult(index)"
            >
              <img
                v-if="item.masterVariant?.images?.[0]?.url"
                :src="item.masterVariant.images[0].url"
                :alt="item.name"
                class="w-10 h-10 rounded object-cover shrink-0"
              />
              <div v-else class="w-10 h-10 rounded bg-neutral-100 shrink-0" />
              <div class="min-w-0">
                <p class="text-sm font-medium text-neutral-900 truncate">{{ item.name }}</p>
                <p v-if="item.masterVariant?.price" class="text-xs text-neutral-500">
                  {{ item.masterVariant.price.toLocaleString() }} {{ item.masterVariant.currency }}
                </p>
              </div>
              <i
                v-if="index === selectedIndex"
                class="pi pi-arrow-right text-neutral-400 ml-auto shrink-0"
              />
            </li>
          </ul>
        </div>

        <!-- Section: Empty State — shown when query entered but no results -->
        <div v-else-if="query && !loading" class="p-8 text-center text-neutral-400">
          <i class="pi pi-search text-2xl mb-2 block" />
          <p class="text-sm">No results found for "{{ query }}"</p>
        </div>

        <!-- Section: Hint — shown when no query entered -->
        <div v-else class="p-4 text-sm text-neutral-500 text-center">
          Type to search products, collections, and more...
        </div>

        <!-- Section: Error State — API error message -->
        <div v-if="error" class="px-4 py-2 bg-red-50 text-red-600 text-sm text-center">
          {{ error }}
        </div>

        <!-- Section: Keyboard Hints — navigation shortcuts -->
        <div
          class="px-4 py-2 border-t border-neutral-100 flex items-center gap-4 text-[11px] text-neutral-400"
        >
          <span><kbd class="px-1 py-0.5 border border-neutral-200 rounded">↑↓</kbd> navigate</span>
          <span><kbd class="px-1 py-0.5 border border-neutral-200 rounded">↵</kbd> select</span>
          <span><kbd class="px-1 py-0.5 border border-neutral-200 rounded">esc</kbd> close</span>
        </div>
      </div>
    </div>
  </Teleport>
</template>
