<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useCatalogStore } from '../stores/catalogStore'
import { useProductListStore } from '../stores/productListStore'

usePageTitle('Home')
const catalog = useCatalogStore()
const productList = useProductListStore()
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Home</h1>
    <Card>
      <template #content>
        <div class="space-y-4">
          <Skeleton width="100%" height="2rem" />
          <Skeleton width="75%" height="1rem" />
          <Skeleton width="50%" height="1rem" />
        </div>
      </template>
    </Card>
    <Message severity="info" class="mt-4">
      Home page content will be implemented here — hero banner, categories, new arrivals, featured products.
    </Message>
  </div>
</template>
