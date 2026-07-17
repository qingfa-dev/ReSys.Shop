<template>
  <span class="p-input-icon-left relative">
    <i class="pi pi-search" />
    <InputText
      v-model="searchQuery"
      :placeholder="generalLocales.layout.search"
      class="w-64 rounded-border"
      @focus="overlayVisible = true"
      @keydown.escape="overlayVisible = false"
    />
    <OverlayPanel ref="op" :visible="overlayVisible" @hide="overlayVisible = false">
      <div class="flex flex-col gap-2" style="min-width: 280px">
        <div
          v-for="result in searchResults"
          :key="result.to"
          class="cursor-pointer rounded p-2 hover:bg-surface-100"
          @click="goTo(result.to)"
        >
          <span class="font-medium">{{ result.label }}</span>
          <span class="ml-2 text-sm text-color-secondary">{{ result.description }}</span>
        </div>
        <div v-if="searchResults.length === 0 && searchQuery.length > 0" class="p-2 text-sm text-color-secondary">
          {{ generalLocales.layout.noResults }}
        </div>
      </div>
    </OverlayPanel>
  </span>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { generalLocales } from '@/shared/locales/general.locales'

const router = useRouter()

const searchQuery = ref('')
const overlayVisible = ref(false)

interface SearchResult {
  label: string
  description: string
  to: string
}

const searchResults = ref<SearchResult[]>([])

const allPages: SearchResult[] = [
  { label: 'Dashboard', description: 'Overview and statistics', to: '/' },
  { label: 'Users', description: 'Manage user accounts', to: '/identity/users' },
]

watch(searchQuery, (query) => {
  if (!query.trim()) {
    searchResults.value = []
    return
  }
  const q = query.toLowerCase()
  searchResults.value = allPages.filter(
    (page) =>
      page.label.toLowerCase().includes(q) || page.description.toLowerCase().includes(q),
  )
})

function goTo(to: string) {
  overlayVisible.value = false
  searchQuery.value = ''
  router.push(to)
}
</script>
