<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useAddressStore } from '../stores/address.store'
import { userService } from '@/features/users/services/user.service'
import { storeToRefs } from 'pinia'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Select from 'primevue/select'
import Message from 'primevue/message'
import PageShell from '@/shared/components/navigation/PageShell.vue'
import PageHeader from '@/shared/components/navigation/PageHeader.vue'
import { useI18n } from 'vue-i18n'

const store = useAddressStore()
const { items, loading } = storeToRefs(store)
const { t } = useI18n()

const selectedUserId = ref<string | null>(null)
const users = ref<Array<{ id: string; label: string }>>([])

onMounted(async () => {
  const [staffResult, customerResult] = await Promise.all([
    userService.list(),
    userService.listCustomers(),
  ])
  const staffOptions = (staffResult.items ?? []).map(u => ({ id: u.id, label: `${u.email ?? u.id}` }))
  const customerOptions = (customerResult.items ?? []).map(u => ({ id: u.id, label: `${u.email ?? u.id}` }))
  users.value = [...staffOptions, ...customerOptions]
})

watch(selectedUserId, async (userId) => {
  if (userId) {
    await store.fetchAll(userId)
  }
})
</script>

<template>
  <PageShell :card="false">
    <PageHeader :title="t('profile.titles.addresses')" />

    <div class="flex flex-col gap-4">
      <Select
        v-model="selectedUserId"
        :options="users"
        optionLabel="label"
        optionValue="id"
        :placeholder="t('profile.placeholders.select_user')"
        filter
        showClear
        class="w-full max-w-md"
      />

      <Message v-if="!selectedUserId" severity="info" :closable="false">
        {{ t('profile.messages.select_user_to_view_addresses') }}
      </Message>

      <DataTable v-if="selectedUserId" :value="items" :loading="loading" dataKey="id" class="mt-4">
        <Column field="address1" :header="t('profile.labels.address')" />
        <Column field="city" :header="t('profile.labels.city')" />
        <Column field="stateProvince" :header="t('profile.labels.state')" />
        <Column field="country" :header="t('profile.labels.country')" />
        <Column field="isDefault" :header="t('profile.labels.default')">
          <template #body="{ data }">
            <i :class="data.isDefault ? 'pi pi-check text-green-500' : 'pi pi-times text-red-500'" />
          </template>
        </Column>
      </DataTable>
    </div>
  </PageShell>
</template>
