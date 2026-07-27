<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import { ListLayout } from '@/shared/components'
import PaymentListTable from '../components/PaymentListTable.vue'

const { t } = useI18n()
const route = useRoute()
</script>

<template>
  <ListLayout>
    <template #header>
      <PageHeader
        :title="t('payment.payments.title')"
        :subtitle="t('payment.payments.subtitle')"
        :icon="route.meta?.icon as string | undefined"
      />
    </template>
    <PaymentListTable />
  </ListLayout>
</template>
