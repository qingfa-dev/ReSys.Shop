<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import { ListLayout } from '@/shared/components'
import ProductListTable from '../components/ProductListTable.vue'

const { t } = useI18n()
const route = useRoute()
</script>

<template>
  <ListLayout>
    <template #header>
      <PageHeader
        :title="t('catalog.products.titles.list')"
        :subtitle="t('catalog.products.descriptions.list')"
        :icon="route.meta?.icon as string | undefined"
      />
    </template>
    <ProductListTable />
  </ListLayout>
</template>
