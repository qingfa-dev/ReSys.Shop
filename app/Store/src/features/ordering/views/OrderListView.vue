<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { formatCurrency } from '@/shared/utils/currency'
import { formatDateTimeUtc } from '@/shared/utils/date'
import { useOrders } from '../composables/useOrders'
import type { PageState } from 'primevue/paginator'
import type { OrderStatus } from '../types'

usePageTitle('Orders')

const orders = useOrders()

// Severity: orderStore exposes statuses but no Tag severity mapping, so the
// view maps each status to a Tag severity (mirrors the Admin SPA's mapping).
const statusSeverity: Record<OrderStatus, 'warn' | 'success' | 'danger' | 'secondary'> = {
  Draft: 'warn',
  Placed: 'success',
  Canceled: 'danger',
  Expired: 'secondary',
}

// Pagination: Zero-based first index for the Paginator from the 1-based store page
const first = computed(() => (orders.page - 1) * orders.pageSize)

// Page: Forward Paginator page state to the store and refetch
function onPage(event: PageState): void {
  orders.goToPage(event.page + 1)
}

onMounted(() => {
  // Load: Fetch the first order page on entry.
  void orders.fetchOrders()
})
</script>

<template>
  <Card>
    <template #title>Orders</template>
    <template #content>
      <!-- Section: Loading State — skeleton rows while the list fetches -->
      <div v-if="orders.loading && orders.items.length === 0" class="flex flex-col gap-3">
        <Skeleton v-for="i in 4" :key="i" height="4rem" />
      </div>

      <!-- Section: Error State — message and retry when the fetch fails -->
      <div v-else-if="orders.error" class="flex flex-col items-center gap-4 py-8">
        <Message severity="error" :closable="false">{{ orders.error }}</Message>
        <Button label="Retry" severity="secondary" outlined @click="orders.fetchOrders" />
      </div>

      <!-- Section: Data Table — orders with status tags and detail links -->
      <DataTable v-else :value="orders.items" dataKey="id" tableStyle="min-width: 40rem">
        <!-- Section: Table Columns -->
        <Column header="Order">
          <template #body="{ data }">
            <RouterLink
              :to="`/account/orders/${data.id}`"
              class="font-medium text-brand hover:underline"
            >
              {{ data.number }}
            </RouterLink>
          </template>
        </Column>
        <Column header="Date">
          <template #body="{ data }">{{ formatDateTimeUtc(data.createdAtUtc) }}</template>
        </Column>
        <Column header="Items">
          <template #body="{ data }">{{ data.itemCount }}</template>
        </Column>
        <Column header="Total">
          <template #body="{ data }">{{ data.currency }} {{ formatCurrency(data.total) }}</template>
        </Column>
        <Column header="Status">
          <template #body="{ data }">
            <Tag :value="data.status" :severity="statusSeverity[data.status as OrderStatus]" rounded />
          </template>
        </Column>

        <!-- Section: Empty State — prompt to browse the catalog when no orders exist -->
        <template #empty>
          <div class="flex flex-col items-center gap-4 py-8">
            <Message severity="info" :closable="false">No orders yet.</Message>
            <Button as="router-link" to="/shop" label="Start Shopping" icon="pi pi-shopping-bag" />
          </div>
        </template>
      </DataTable>

      <!-- Section: Paginator — bound to the order store paging state -->
      <Paginator
        v-if="orders.totalCount > 0"
        class="mt-4"
        :rows="orders.pageSize"
        :totalRecords="orders.totalCount"
        :first="first"
        @page="onPage"
      />
    </template>
  </Card>
</template>
