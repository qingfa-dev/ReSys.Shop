<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import { ListLayout } from '@/shared/components'
import StockItemListTable from '../components/StockItemListTable.vue'

const { t } = useI18n()
const route = useRoute()
</script>

<template>
  <ListLayout>
    <template #header>
      <PageHeader
        :title="t('inventory.stocks.title')"
        :subtitle="t('inventory.stocks.subtitle')"
        :icon="route.meta?.icon as string | undefined"
      />
    </template>
    <StockItemListTable />
  </ListLayout>
</template>
