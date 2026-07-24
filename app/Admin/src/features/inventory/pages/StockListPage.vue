<script setup lang="ts">
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import StockItemListTable from '../components/StockItemListTable.vue'

const route = useRoute()
</script>

<template>
  <div>
    <PageHeader title="Stock Items" :icon="route.meta?.icon as string | undefined" />
    <StockItemListTable />
  </div>
</template>
