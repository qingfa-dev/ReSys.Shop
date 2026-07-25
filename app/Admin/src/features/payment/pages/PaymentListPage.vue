<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import PaymentListTable from '../components/PaymentListTable.vue'

const { t } = useI18n()
const route = useRoute()
</script>

<template>
  <div>
    <PageHeader :title="t('payment.payments.titles.list')" :icon="route.meta?.icon as string | undefined" />
    <PaymentListTable />
  </div>
</template>
