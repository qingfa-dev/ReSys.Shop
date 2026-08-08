<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useCartStore } from '../stores/cartStore'

usePageTitle('Cart')
const cart = useCartStore()

onMounted(() => {
  cart.fetchCart()
})
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Page Header -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Cart' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Shopping Cart</h1>

    <!-- Content Card -->
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
      Cart items list, quantity controls, remove buttons, order summary, and checkout CTA will be implemented here.
    </Message>
  </div>
</template>
