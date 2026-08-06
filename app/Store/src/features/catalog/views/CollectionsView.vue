<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getTaxons } from '../services/taxonApi'
import { useCatalogStore } from '../stores/catalogStore'
import type { StoreTaxonListItemResponse } from '../types/taxon'

const router = useRouter()
const catalog = useCatalogStore()

const taxons = ref<StoreTaxonListItemResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

// Map: Breadcrumb trail for the collections page
const breadcrumbItems = computed(() => [
  { label: 'Home', to: '/' },
  { label: 'Collections' },
])

// Trigger: Load top-level taxons on mount
onMounted(async () => {
  const result = await getTaxons({ pageNumber: 1, pageSize: 100 })
  if (result.isSuccess) taxons.value = result.items.filter(t => t.parentId === null)
  else error.value = result.message
  loading.value = false
})

// Map: Open a collection in the shop filtered by the taxon
function openCollection(taxon: StoreTaxonListItemResponse): void {
  catalog.toggleTaxon(taxon.id)
  router.push('/shop')
}
</script>
<template>
  <!-- Section: Collections Page -->
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Section: Breadcrumb -->
    <Breadcrumb :model="breadcrumbItems" class="mb-4" />
    <!-- Section: Page Header -->
    <div class="mb-8">
      <h1 class="text-2xl font-bold text-stone-900">Collections</h1>
      <p class="text-stone-500 mt-1">Browse the shop by collection.</p>
    </div>

    <!-- Section: Error State -->
    <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>

    <!-- Section: Loading State -->
    <div v-else-if="loading" class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
      <div v-for="i in 8" :key="i" class="aspect-square bg-stone-200 rounded-xl animate-pulse" />
    </div>

    <!-- Section: Empty State -->
    <div v-else-if="taxons.length === 0" class="text-center py-16">
      <i class="pi pi-tags text-4xl text-stone-300 mb-4" />
      <p class="text-stone-500">No collections available yet.</p>
    </div>

    <!-- Section: Collection Grid -->
    <div v-else class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
      <button
        v-for="taxon in taxons"
        :key="taxon.id"
        class="group bg-white rounded-xl border border-stone-200 overflow-hidden shadow-sm hover:shadow-md transition-shadow text-left"
        @click="openCollection(taxon)"
      >
        <div class="aspect-square bg-stone-100 overflow-hidden">
          <img
            v-if="taxon.imageUrl"
            :src="taxon.imageUrl"
            :alt="taxon.name"
            class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-200"
          />
          <div v-else class="w-full h-full flex items-center justify-center text-stone-400">
            <i class="pi pi-image text-4xl" />
          </div>
        </div>
        <div class="p-4">
          <p class="text-sm font-medium text-stone-900">{{ taxon.name }}</p>
          <p class="mt-1 text-xs text-stone-500">{{ taxon.taxonCount }} products</p>
        </div>
      </button>
    </div>
  </div>
</template>
