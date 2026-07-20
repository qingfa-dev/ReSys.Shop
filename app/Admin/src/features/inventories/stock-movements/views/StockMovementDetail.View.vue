<script setup lang="ts">
import { onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useMovementStore } from '../stores/movement.store'
import { storeToRefs } from 'pinia'
import { useToast } from '@/common/composables/toast.use'
import { useFormatter } from '@/common/composables/formatter.use'
import { useI18n } from 'vue-i18n'
import PageShell from '@/shared/components/navigation/PageShell.vue'
import PageHeader from '@/shared/components/navigation/PageHeader.vue'
import DetailField from '@/shared/components/data-display/DetailField.vue'

const route = useRoute()
const router = useRouter()
const store = useMovementStore()
const { showToast } = useToast()
const { formatDate } = useFormatter()
const { t } = useI18n()
const { current, loading } = storeToRefs(store)

const movementId = route.params.id as string

const movementTypes: Record<number, string> = {
  1: 'Addition',
  2: 'Removal',
  3: 'Adjustment',
  4: 'Transfer',
}

const typeSeverity: Record<number, string> = {
  1: 'success',
  2: 'danger',
  3: 'warn',
  4: 'info',
}

onMounted(async () => {
  const result = await store.fetchById(movementId)
  if (!result.isSuccess) {
    showToast('error', t('common.error'), result.message || 'Failed to load movement')
    router.push({ name: 'inventory.movements.list' })
  }
})
</script>

<template>
  <PageShell :card="false" gap maxWidth="7xl">
    <template v-if="current">
      <PageHeader
        back
        :title="t('inventory.titles.movement_detail') || 'Movement Detail'"
        :description="`#${current.id}`"
      >
        <template #badge>
          <Tag
            :value="movementTypes[current.type] ?? current.type"
            :severity="typeSeverity[current.type] ?? 'secondary'"
            rounded
            class="px-4 py-2 text-lg font-bold"
          />
        </template>
      </PageHeader>

      <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
        <template #title>
          <span class="text-xl font-black uppercase tracking-tight p-4 block">
            {{ t('inventory.titles.movement_information') || 'Movement Information' }}
          </span>
        </template>
        <template #content>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-8 p-6">
            <DetailField label="Date" :value="formatDate(current.createdAtUtc)" />
            <DetailField label="Type" :value="movementTypes[current.type] ?? String(current.type)" />
            <DetailField label="Stock Item" :value="current.stockItemId" />
            <DetailField label="SKU" :value="current.stockItemId" />
            <DetailField label="Quantity" :value="current.quantity" />
            <DetailField label="From Location" />
            <DetailField label="To Location" />
            <DetailField label="Reference" :value="current.reference" />
            <DetailField label="Notes" :value="current.reason" />
            <DetailField label="Created By" :value="current.createdBy" />
          </div>
        </template>
      </Card>
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
