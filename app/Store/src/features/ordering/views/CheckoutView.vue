<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useCheckoutStore } from '../stores/checkoutStore'

usePageTitle('Checkout')
const checkout = useCheckoutStore()

onMounted(() => {
  checkout.init()
})
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Page Header -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Cart', to: '/cart' }, { label: 'Checkout' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Checkout</h1>

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
      5-step checkout stepper, address form, delivery selection, payment integration, and order confirmation will be implemented here.
    </Message>
  </div>
</template>
