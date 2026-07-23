<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import ProductListTable from '../components/ProductListTable.vue'

const { t } = useI18n()
const route = useRoute()
</script>

<template>
  <div>
    <PageHeader :title="t('catalog.products.titles.list')" :subtitle="t('catalog.products.descriptions.list')" :icon="route.meta?.icon as string | undefined" />
    <ProductListTable />
  </div>
</template>
