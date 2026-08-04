<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import { ListLayout } from '@/shared/components'
import UserListTable from '../components/UserListTable.vue'

const { t } = useI18n()
const route = useRoute()
</script>

<template>
  <ListLayout>
    <template #header>
      <PageHeader
        :title="t('users.customers.title')"
        :subtitle="t('users.customers.subtitle')"
        :icon="route.meta?.icon as string | undefined"
      />
    </template>
    <UserListTable user-type="customer" />
  </ListLayout>
</template>
