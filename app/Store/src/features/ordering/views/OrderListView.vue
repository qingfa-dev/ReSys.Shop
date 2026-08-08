<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useOrderStore } from '../stores/orderStore'

usePageTitle('My Orders')
const orders = useOrderStore()

onMounted(() => {
  orders.fetchOrders()
})
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Page Header -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'My Orders' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">My Orders</h1>

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
      Order cards list, status filter, pagination, and order summary will be implemented here.
    </Message>
  </div>
</template>
