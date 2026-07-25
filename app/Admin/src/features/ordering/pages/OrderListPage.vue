<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import OrderListTable from '../components/OrderListTable.vue'

const { t } = useI18n()
const route = useRoute()
</script>

<template>
  <div>
    <PageHeader :title="t('ordering.orders.titles.list')" :icon="route.meta?.icon as string | undefined" />
    <OrderListTable />
  </div>
</template>
