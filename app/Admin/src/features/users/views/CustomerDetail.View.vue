<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useUserStore } from '../stores/user.store'
import { storeToRefs } from 'pinia'
import { useToast } from '@/shared/composables/toast.use'
import { useFormatter } from '@/shared/composables/formatter.use'
import { useI18n } from 'vue-i18n'
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
import DetailField from '@/shared/components/DetailField.Component.vue'
import StatusBadge from '@/shared/components/StatusBadge.Component.vue'

const route = useRoute()
const router = useRouter()
const store = useUserStore()
const { showToast } = useToast()
const { formatDate } = useFormatter()
const { t } = useI18n()
const { currentCustomer, loading } = storeToRefs(store)

const customerId = route.params.id as string

const activeTab = ref('0')

const statusMap: Record<string | number, { label: string; severity: string }> = {
  true: { label: 'Active', severity: 'success' },
  false: { label: 'Inactive', severity: 'secondary' },
}

onMounted(async () => {
  const result = await store.fetchCustomerById(customerId)
  if (!result.isSuccess) {
    showToast('error', t('common.error'), result.message || t('users.messages.customer_detail_error'))
    router.push({ name: 'users.customers.list' })
  }
})
</script>

<template>
  <PageShell :card="false" gap maxWidth="7xl">
    <template v-if="currentCustomer">
      <PageHeader
        back
        :title="currentCustomer.fullName || 'Customer Profile'"
        :description="currentCustomer.email"
      >
        <template #badge>
          <StatusBadge :status="String(currentCustomer.isActive)" :statusMap="statusMap" />
        </template>
      </PageHeader>

      <Tabs v-model:value="activeTab">
        <TabList>
          <Tab value="0">Profile</Tab>
          <Tab value="1">Orders</Tab>
          <Tab value="2">Addresses</Tab>
        </TabList>
        <TabPanels>
          <TabPanel value="0">
            <div class="grid grid-cols-1 lg:grid-cols-1 gap-6">
              <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
                <template #title>
                  <span class="text-xl font-black uppercase tracking-tight p-4 block">
                    Profile Information
                  </span>
                </template>
                <template #content>
                  <div class="grid grid-cols-1 md:grid-cols-2 gap-8 p-6">
                    <DetailField label="Email" :value="currentCustomer.email" />
                    <DetailField label="Display Name" :value="currentCustomer.fullName" />
                    <DetailField label="Phone" :value="currentCustomer.phoneNumber" />
                    <DetailField label="Member Since" :value="formatDate(currentCustomer.createdAtUtc)" />
                  </div>
                </template>
              </Card>
            </div>
          </TabPanel>
          <TabPanel value="1">
            <div class="flex flex-col items-center justify-center py-16 text-surface-400">
              <i class="pi pi-shopping-bag text-4xl opacity-20 mb-3"></i>
              <p class="text-sm font-medium italic">No orders</p>
            </div>
          </TabPanel>
          <TabPanel value="2">
            <div class="flex flex-col items-center justify-center py-16 text-surface-400">
              <i class="pi pi-map-marker text-4xl opacity-20 mb-3"></i>
              <p class="text-sm font-medium italic">No addresses</p>
            </div>
          </TabPanel>
        </TabPanels>
      </Tabs>
    </template>

    <div v-else-if="loading" class="flex justify-center py-20">
      <ProgressSpinner />
    </div>
  </PageShell>
</template>

<style scoped>
:deep(.p-card-body) {
  padding: 0;
}
:deep(.p-card-content) {
  padding: 0;
}
</style>
