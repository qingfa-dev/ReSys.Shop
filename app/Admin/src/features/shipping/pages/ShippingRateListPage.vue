<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import { ListLayout } from '@/shared/components'
import ShippingRateListTable from '../components/ShippingRateListTable.vue'

const { t } = useI18n()
const route = useRoute()
</script>

<template>
  <ListLayout>
    <template #header>
      <PageHeader
        :title="t('shipping.rates.titles.list')"
        :subtitle="t('shipping.rates.descriptions.list')"
        :icon="route.meta?.icon as string | undefined"
      />
    </template>
    <ShippingRateListTable />
  </ListLayout>
</template>
