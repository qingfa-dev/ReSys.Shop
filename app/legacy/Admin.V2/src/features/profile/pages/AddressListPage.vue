<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import { ListLayout } from '@/shared/components'
import AddressListTable from '../components/AddressListTable.vue'

const { t } = useI18n()
const route = useRoute()
</script>

<template>
  <ListLayout>
    <template #header>
      <PageHeader
        :title="t('profile.addresses.title')"
        :subtitle="t('profile.addresses.subtitle')"
        :icon="route.meta?.icon as string | undefined"
      />
    </template>
    <AddressListTable />
  </ListLayout>
</template>
