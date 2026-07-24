<script setup lang="ts">
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import StockMovementListTable from '../components/StockMovementListTable.vue'

const route = useRoute()
</script>

<template>
  <div>
    <PageHeader title="Stock Movements" :icon="route.meta?.icon as string | undefined" />
    <StockMovementListTable />
  </div>
</template>
