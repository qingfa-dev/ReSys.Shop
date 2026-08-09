<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import type { CommandMenuItem } from 'primevue/commandmenu'
import { useSearch } from '../composables/useSearch'

// Search: Singleton owns query/results/visibility; this palette only mirrors it.
const search = useSearch()
const router = useRouter()

// Scorer: The API already filters results, so accept every command unchanged.
const acceptAll = (): number => 1

// Skeleton: Show placeholder rows only while a query is in flight.
const showSkeleton = computed(() => search.loading.value && search.query.value.trim().length > 0)

// Commands: One palette command per API result; the footer hosts the "view all" action.
const commands = computed<CommandMenuItem[]>(() => {
  if (search.loading.value) return []
  return search.results.value.map((item, index) => ({
    label: item.name,
    icon: 'pi pi-box',
    command: () => search.navigateToResult(index),
  }))
})

// Dialog: Two-way mirror — open() shows it; any dismiss path closes the search.
const dialogVisible = computed({
  get: () => search.isOpen.value,
  set: (visible: boolean) => {
    if (!visible) search.close()
  },
})

// Input: Forward palette keystrokes to the singleton; the debounce lives in useSearch.
function onSearchInput(value: string): void {
  search.query.value = value
  void search.search()
}

// Navigate: Send the current query to the full shop results page, then dismiss.
function viewAllResults(): void {
  void router.push({ path: '/shop', query: { q: search.query.value.trim() } })
  search.close()
}
</script>

<template>
  <!-- Section: Search Palette — Dialog mirroring the singleton open flag hosts the CommandMenu -->
  <Dialog
    v-model:visible="dialogVisible"
    modal
    :show-header="false"
    dismissable-mask
    class="overflow-hidden"
    :style="{ width: 'min(38rem, 94vw)' }"
    :pt="{ content: { class: 'p-0!' } }"
  >
    <CommandMenu
      :model="commands"
      :search="search.query.value"
      :filter="acceptAll"
      placeholder="Search products..."
      aria-label="Product search"
      class="w-full rounded-none border-none"
      @update:search="onSearchInput"
    >
      <!-- Header: Search icon beside the palette input -->
      <template #header="{ inputProps }">
        <IconField class="w-full px-1">
          <InputIcon class="pi pi-search" />
          <InputText v-bind="inputProps" class="w-full border-none bg-transparent" />
        </IconField>
      </template>

      <!-- Loading: Skeleton rows stand in for the list while the API responds -->
      <template #empty>
        <div v-if="showSkeleton" class="flex flex-col gap-2 p-1">
          <Skeleton v-for="i in 3" :key="i" class="h-8 w-full" />
        </div>
        <Message v-else severity="info" :closable="false" class="w-full">No products found</Message>
      </template>

      <!-- Loading: Same stand-in when the filter empties the list -->
      <template #emptyfilter>
        <div v-if="showSkeleton" class="flex flex-col gap-2 p-1">
          <Skeleton v-for="i in 3" :key="i" class="h-8 w-full" />
        </div>
        <Message v-else severity="info" :closable="false" class="w-full">No products found</Message>
      </template>

      <!-- Footer: View-all shortcut alongside the palette keyboard hints -->
      <template #footer>
        <div class="flex w-full items-center justify-between px-1">
          <span class="text-xs text-muted">↑↓ Navigate, ↵ Select</span>
          <Button
            v-if="search.query.value.trim()"
            size="small"
            variant="text"
            label="View all results"
            icon="pi pi-arrow-right"
            iconPos="right"
            @click="viewAllResults"
          />
        </div>
      </template>
    </CommandMenu>
  </Dialog>
</template>
