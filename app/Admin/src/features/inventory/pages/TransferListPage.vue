<script setup lang="ts">
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import StockTransferListTable from '../components/StockTransferListTable.vue'

const route = useRoute()
</script>

<template>
  <div>
    <PageHeader title="Stock Transfers" :icon="route.meta?.icon as string | undefined" />
    <StockTransferListTable />
  </div>
</template>
