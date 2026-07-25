<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import { ListLayout } from '@/shared/components'
import VariantListTable from '../components/VariantListTable.vue'

const { t } = useI18n()
const route = useRoute()
</script>

<template>
  <ListLayout>
    <template #header>
      <PageHeader
        :title="t('catalog.variants.titles.list')"
        :subtitle="t('catalog.variants.descriptions.list')"
        :icon="route.meta?.icon as string | undefined"
      />
    </template>
    <VariantListTable />
  </ListLayout>
</template>
