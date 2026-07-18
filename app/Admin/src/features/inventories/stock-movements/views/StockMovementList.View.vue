<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { movementService } from '../services/movement.service';
import { useFormatter } from '@/shared/composables/formatter.use';
import { useI18n } from 'vue-i18n';
import PageShell from '@/shared/components/PageShell.Component.vue';
import PageHeader from '@/shared/components/PageHeader.Component.vue';
import type { DataTablePageEvent } from 'primevue/datatable';
import type { StockMovement } from '../types/stock-movement.response.type';

const { t } = useI18n();

const movements = ref<StockMovement[]>([]);
const loading = ref(false);
const totalCount = ref(0);
const page = ref(1);
const pageSize = ref(10);
const { formatDate } = useFormatter();

const movementTypes: Record<number, string> = {
  1: 'Addition',
  2: 'Removal',
  3: 'Adjustment',
  4: 'Transfer',
};

async function fetchMovements() {
  loading.value = true;
  const result = await movementService.listMovements({ page: page.value, pageSize: pageSize.value });
  if (result.isSuccess) {
    movements.value = result.items ?? [];
    totalCount.value = result.totalCount ?? 0;
  }
  loading.value = false;
}

onMounted(() => fetchMovements());

function onPage(event: DataTablePageEvent) {
  page.value = event.page !== undefined ? event.page + 1 : 1;
  pageSize.value = event.rows;
  fetchMovements();
}
</script>

<template>
  <PageShell maxWidth="7xl">
    <PageHeader :title="t('inventory.titles.stock_movement_history')" />
    <DataTable
      :value="movements"
      :loading="loading"
      :lazy="true"
      :paginator="true"
      :rows="pageSize"
      :totalRecords="totalCount"
      @page="onPage"
      dataKey="id"
      rowHover
      scrollable
      stripedRows
      showGridlines
    >
      <template #empty>
        <div class="flex flex-col items-center justify-center py-20 text-surface-400">
          <i class="mb-4 text-6xl pi pi-history opacity-20"></i>
          <p class="text-xl font-medium">No movement history found.</p>
        </div>
      </template>

      <Column field="createdAtUtc" :header="t('inventory.table.date')">
        <template #body="{ data }">
          <span class="text-sm">{{ formatDate(data.createdAtUtc) }}</span>
        </template>
      </Column>

      <Column field="type" :header="t('inventory.table.type')">
        <template #body="{ data }">
          <Tag :value="movementTypes[data.type] ?? data.type" severity="info" rounded class="px-3" />
        </template>
      </Column>

      <Column field="stockItemId" header="Stock Item">
        <template #body="{ data }">
          <span class="font-mono text-xs">{{ data.stockItemId }}</span>
        </template>
      </Column>

      <Column field="quantity" :header="t('inventory.table.quantity')">
        <template #body="{ data }">
          <span :class="data.quantity < 0 ? 'text-red-500 font-bold' : 'font-bold'">
            {{ data.quantity > 0 ? '+' : '' }}{{ data.quantity }}
          </span>
        </template>
      </Column>

      <Column field="reference" :header="t('inventory.table.reference')">
        <template #body="{ data }">
          <span>{{ data.reference ?? '-' }}</span>
        </template>
      </Column>

      <Column field="reason" :header="t('inventory.table.reason')">
        <template #body="{ data }">
          <span>{{ data.reason ?? '-' }}</span>
        </template>
      </Column>
    </DataTable>
  </PageShell>
</template>
