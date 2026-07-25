<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Column from 'primevue/column'
import DataTable from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useI18n } from 'vue-i18n'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { usePaymentMethodStore } from '../store/payment-method.store'
import { PaymentMethodApi } from '../api'
import { ROUTE } from '../routes'

const router = useRouter()
const { confirmDelete } = useConfirm()
const toast = useToast()
const { t } = useI18n()
const store = usePaymentMethodStore()

onMounted(() => store.fetchMany())

function goToCreate() { router.push({ name: ROUTE.METHODS.CREATE }) }
function goToView(id: string) { router.push({ name: ROUTE.METHODS.VIEW, params: { id } }) }
function goToEdit(id: string) { router.push({ name: ROUTE.METHODS.EDIT, params: { id } }) }

async function onDelete(id: string) {
  confirmDelete({
    target: t('payment.methods.messages.delete_confirm_target'),
    onAccept: async () => {
      const result = await PaymentMethodApi.delete(id)
      if (result.isSuccess) {
        toast.success(t('payment.methods.messages.delete_success'))
        await store.fetchMany()
      } else {
        toast.error(result.message ?? t('payment.methods.messages.delete_failed'))
      }
    },
  })
}

async function onToggleActive(id: string, isActive: boolean) {
  const result = isActive
    ? await PaymentMethodApi.deactivate(id)
    : await PaymentMethodApi.activate(id)
  if (result.isSuccess) {
    toast.success(isActive
      ? t('payment.methods.messages.deactivate_success')
      : t('payment.methods.messages.activate_success'))
    await store.fetchMany()
  } else {
    toast.error(result.message ?? t('payment.methods.messages.update_failed'))
  }
}

function onPageChange(e: { page: number; rows: number }) { store.setPage(e.page + 1) }
</script>

<template>
  <div>
    <TableToolbar
      v-model:query="store.searchQuery"
      :search-placeholder="t('payment.methods.table.search_placeholder')"
      :create-label="t('payment.methods.actions.create')"
      @create="goToCreate"
    />
    <LoadingSkeleton v-if="store.loading && store.items.length === 0" :rows="5" :columns="6" />
    <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany" />
    <EmptyState
      v-else-if="store.items.length === 0"
      :title="t('payment.methods.messages.empty_list')"
      :description="t('payment.methods.messages.empty_description')"
    />
    <DataTable
      v-else
      :rows="[...store.items]"
      :loading="store.loading"
      :total-records="store.totalRecords"
      :page-size="store.query.pageSize"
      :first="(store.query.page - 1) * store.query.pageSize"
      @page="onPageChange"
    >
      <Column :header="t('payment.methods.table.name')" field="name" sortable />
      <Column :header="t('payment.methods.table.code')" field="code" />
      <Column :header="t('payment.methods.table.order')" field="displayOrder" sortable />
      <Column :header="t('payment.methods.table.test_mode')" field="isTestMode">
        <template #body="{ data }">
          <i v-if="data.isTestMode" class="pi pi-check" style="color: var(--p-green-500)" />
          <i v-else class="pi pi-times" style="color: var(--p-red-400)" />
        </template>
      </Column>
      <Column :header="t('payment.methods.table.active')" field="isActive">
        <template #body="{ data }">
          <i v-if="data.isActive" class="pi pi-check" style="color: var(--p-green-500)" />
          <i v-else class="pi pi-times" style="color: var(--p-red-400)" />
        </template>
      </Column>
      <template #rowActions="{ data }">
        <ActionMenu
          :items="[
            { label: t('payment.methods.actions.view'), icon: 'pi pi-eye', command: () => goToView(data.id) },
            { label: t('payment.methods.actions.edit'), icon: 'pi pi-pencil', command: () => goToEdit(data.id) },
            { label: data.isActive ? t('payment.methods.actions.deactivate') : t('payment.methods.actions.activate'), icon: data.isActive ? 'pi pi-pause' : 'pi pi-play', command: () => onToggleActive(data.id, data.isActive) },
            { label: t('payment.methods.actions.delete'), icon: 'pi pi-trash', command: () => onDelete(data.id) },
          ]"
        />
      </template>
    </DataTable>
  </div>
</template>
