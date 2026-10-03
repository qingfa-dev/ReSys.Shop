<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import { ListLayout } from '@/shared/components'
import OptionTypeListTable from '../components/OptionTypeListTable.vue'

const { t } = useI18n()
const route = useRoute()
</script>

<template>
  <ListLayout>
    <template #header>
      <PageHeader
        :title="t('catalog.option_types.titles.list')"
        :subtitle="t('catalog.option_types.descriptions.list')"
        :icon="route.meta?.icon as string | undefined"
      />
    </template>
    <OptionTypeListTable />
  </ListLayout>
</template>
