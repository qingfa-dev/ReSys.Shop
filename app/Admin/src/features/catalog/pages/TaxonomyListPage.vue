<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import TaxonomyListTable from '../components/TaxonomyListTable.vue'

const { t } = useI18n()
const route = useRoute()
</script>

<template>
  <div>
    <PageHeader :title="t('catalog.taxonomies.titles.list')" :subtitle="t('catalog.taxonomies.descriptions.list')" :icon="route.meta?.icon as string | undefined" />
    <TaxonomyListTable />
  </div>
</template>
