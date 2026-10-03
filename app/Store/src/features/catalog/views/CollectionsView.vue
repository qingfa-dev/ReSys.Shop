<script setup lang="ts">
import { onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useTaxonomy } from '../composables/useTaxonomy'

// Title: Browser tab title for the collections page
usePageTitle('Collections')

const taxonomy = useTaxonomy()

onMounted(() => {
  // Load: Root taxons for the collection grid — composable guards duplicate fetches
  void taxonomy.loadTaxonomyGroups()
})
</script>

<template>
  <div class="mx-auto max-w-screen-2xl px-4 py-8 sm:px-6 lg:px-8">
    <!-- Section: Page Header — breadcrumb and heading -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Collections' }]" class="mb-6" />
    <h1 class="mb-8 text-2xl font-semibold tracking-tight text-heading">
      Collections
    </h1>

    <!-- Section: Loading State — skeleton cards while root taxons load -->
    <div v-if="taxonomy.taxonsLoading" class="grid grid-cols-2 gap-6 lg:grid-cols-4">
      <div v-for="n in 8" :key="n" class="space-y-3">
        <Skeleton class="aspect-[3/4] w-full rounded-2xl" />
        <Skeleton width="60%" height="1rem" />
        <Skeleton width="40%" height="1rem" />
      </div>
    </div>

    <!-- Section: Collection Grid — root taxon cards linking into the shop filter -->
    <div v-else-if="taxonomy.collections.length > 0" class="grid grid-cols-2 gap-6 lg:grid-cols-4">
      <RouterLink
        v-for="collection in taxonomy.collections"
        :key="collection.id"
        :to="`/shop?taxon=${collection.id}`"
        class="group block"
      >
        <Card class="overflow-hidden">
          <template #header>
            <Image
              v-if="collection.imageUrl"
              :src="collection.imageUrl"
              :alt="collection.presentation ?? collection.name"
              imageClass="aspect-[3/4] w-full object-cover"
            />
            <div
              v-else
              class="flex aspect-[3/4] items-center justify-center bg-surface-100"
            >
              <i class="pi pi-images text-4xl text-placeholder" />
            </div>
          </template>
          <template #title>
            <span class="text-sm font-semibold">{{ collection.presentation ?? collection.name }}</span>
          </template>
          <template #content>
            <Tag :value="`${collection.productCount ?? 0} items`" severity="secondary" />
          </template>
        </Card>
      </RouterLink>
    </div>

    <!-- Section: Empty State — shown when no root taxons exist -->
    <div v-else class="py-24 text-center">
      <p class="text-muted">No collections available yet.</p>
    </div>
  </div>
</template>
