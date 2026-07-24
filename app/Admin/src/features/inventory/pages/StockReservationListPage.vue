<script setup lang="ts">
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import StockReservationListTable from '../components/StockReservationListTable.vue'

const route = useRoute()
</script>

<template>
  <div>
    <PageHeader title="Stock Reservations" :icon="route.meta?.icon as string | undefined" />
    <StockReservationListTable />
  </div>
</template>
