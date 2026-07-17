<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const router = useRouter()

const searchQuery = ref('')
const overlay = ref()

interface SearchResult {
  label: string
  description: string
  to: string | { name: string }
}

const allPages: SearchResult[] = [
  { label: 'Dashboard', description: 'Reports & overview', to: { name: 'reports.dashboard' } },
  { label: 'Products', description: 'Product catalog', to: { name: 'catalog.products.list' } },
  { label: 'Orders', description: 'Order management', to: { name: 'ordering.orders.list' } },
  { label: 'Users', description: 'User accounts', to: { name: 'admin-users' } },
  { label: 'Countries', description: 'Location data', to: { name: 'countries.list' } },
]

const searchResults = ref<SearchResult[]>([])

watch(searchQuery, (query) => {
  if (!query.trim()) {
    searchResults.value = []
    return
  }
  const q = query.toLowerCase()
  searchResults.value = allPages.filter(
    (page) => page.label.toLowerCase().includes(q) || page.description.toLowerCase().includes(q),
  )
})

function goTo(to: string | { name: string }) {
  overlay.value?.hide()
  searchQuery.value = ''
  router.push(to as string)
}

function onFocus(event: FocusEvent) {
  overlay.value?.show(event, event.target as HTMLElement)
}
</script>

<template>
  <div class="relative">
    <IconField>
      <InputIcon class="pi pi-search" />
      <InputText
        v-model="searchQuery"
        :placeholder="t('layout.search')"
        class="w-64 rounded-lg"
        @focus="onFocus"
        @keydown.escape="overlay?.hide()"
      />
    </IconField>
    <OverlayPanel ref="overlay">
      <div class="flex flex-col gap-2" style="min-width: 280px">
        <div
          v-for="result in searchResults"
          :key="typeof result.to === 'string' ? result.to : result.to.name"
          class="cursor-pointer rounded p-2 hover:bg-surface-100"
          @click="goTo(result.to)"
        >
          <span class="font-medium">{{ result.label }}</span>
          <span class="ml-2 text-sm text-color-secondary">{{ result.description }}</span>
        </div>
        <div v-if="searchResults.length === 0 && searchQuery.length > 0" class="p-2 text-sm text-color-secondary">
          {{ t('layout.noResults') }}
        </div>
      </div>
    </OverlayPanel>
  </div>
</template>
