<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getTaxons } from '../services/taxonApi'
import type { StoreTaxonListItemResponse } from '../types/taxon'
import SkeletonGrid from '@/shared/components/SkeletonGrid.vue'

const taxons = ref<StoreTaxonListItemResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

// Trigger: Fetch top-level taxons on mount.
onMounted(async () => {
  const result = await getTaxons({ pageNumber: 1, pageSize: 8 })
  if (result.isSuccess) taxons.value = result.items.filter(t => t.depth === 0)
  else error.value = result.message ?? 'Failed to load categories'
  loading.value = false
})
</script>
<template>
  <!-- Section: Category Grid -->
  <section v-if="loading || taxons.length > 0" class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
    <h2 class="text-2xl font-bold text-stone-900 mb-8">Shop by Category</h2>
    <!-- Section: Loading -->
    <SkeletonGrid v-if="loading" :count="4" />
    <!-- Section: Error -->
    <Message v-else-if="error" severity="error" class="mb-4">{{ error }}</Message>
    <!-- Section: Grid -->
    <div v-else class="grid grid-cols-2 lg:grid-cols-4 gap-4">
      <router-link
        v-for="taxon in taxons"
        :key="taxon.id"
        :to="`/shop?taxonId=${taxon.id}`"
        class="group relative aspect-[4/3] rounded-xl overflow-hidden bg-stone-200"
      >
        <img
          v-if="taxon.imageUrl"
          :src="taxon.imageUrl"
          :alt="taxon.name"
          class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
        />
        <div class="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent" />
        <div class="absolute bottom-0 left-0 right-0 p-4">
          <h3 class="text-white text-lg font-semibold">{{ taxon.name }}</h3>
          <p class="text-white/70 text-sm">{{ taxon.taxonCount }} products</p>
        </div>
      </router-link>
    </div>
  </section>
</template>