<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useCatalogStore } from '../stores/catalogStore'

usePageTitle('Collections')
const catalog = useCatalogStore()

onMounted(() => {
  catalog.loadTaxonomyGroups()
})
</script>

<template>
  <!-- Section: Page Header — breadcrumb and heading -->
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Collections' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Collections</h1>

    <!-- Section: Loading State — skeleton grid while taxonomy groups load -->
    <div v-if="catalog.taxonsLoading" class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
      <div v-for="n in 8" :key="n" class="space-y-3">
        <Skeleton width="100%" height="auto" class="aspect-[3/4] rounded-lg" />
        <Skeleton width="60%" height="0.75rem" />
        <Skeleton width="80%" height="1rem" />
      </div>
    </div>

    <!-- Section: Collection Grid — taxonomy group image cards linking to shop -->
    <div v-else class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
      <router-link
        v-for="group in catalog.taxonomyGroups"
        :key="group.taxonomy.id"
        to="/shop"
        class="group block"
      >
        <div class="aspect-[3/4] bg-neutral-100 rounded-lg flex flex-col items-center justify-center p-6 text-center transition-shadow hover:shadow-md">
          <h3 class="text-lg font-semibold text-neutral-800">{{ group.taxonomy.name }}</h3>
          <p class="mt-2 text-sm text-neutral-400">Browse collection</p>
        </div>
      </router-link>
    </div>

    <!-- Section: Empty State — shown when no collections are available -->
    <div v-if="!catalog.taxonsLoading && catalog.taxonomyGroups.length === 0" class="py-24 text-center">
      <p class="text-neutral-500">No collections available yet.</p>
    </div>
  </div>
</template>
