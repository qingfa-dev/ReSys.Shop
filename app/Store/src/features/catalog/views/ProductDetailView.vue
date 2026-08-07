<script setup lang="ts">
import { watch } from 'vue'
import { useRoute } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useProductDetailStore } from '../stores/productDetailStore'

usePageTitle('Product')
const route = useRoute()
const detail = useProductDetailStore()

watch(() => route.params.slug, (slug) => {
  if (typeof slug === 'string') detail.load(slug)
}, { immediate: true })
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Shop', to: '/shop' }, { label: 'Product' }]" />
    <Skeleton v-if="detail.loading" width="100%" height="20rem" />
    <Card v-else-if="detail.product">
      <template #content>
        <h1 class="text-2xl font-bold text-neutral-900">{{ detail.product.name }}</h1>
      </template>
    </Card>
    <Message severity="error" v-else-if="detail.error" class="mt-4">{{ detail.error }}</Message>
    <Message severity="info" class="mt-4">
      Product gallery, variant selector, add-to-cart, description tabs, similar products will be implemented here.
    </Message>
  </div>
</template>
