<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useFulfillmentStore } from '../stores/fulfillment.store';
import { storeToRefs } from 'pinia';
import { useFormatter } from '@/shared/composables/formatter.use';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';

const store = useFulfillmentStore();
const { t } = useI18n();
const { queue, loading } = storeToRefs(store);
const { formatDate } = useFormatter();
const router = useRouter();

onMounted(() => {
    store.fetchQueue();
});

const shipOrder = (id: string) => {
    router.push({ name: 'ordering.orders.detail', params: { id } });
};
</script>

<template>
    <Card>
        <template #content>
            <div class="flex justify-between items-center mb-6">
            <h1 class="text-2xl font-bold">{{ t('ordering.titles.fulfillment_queue') }}</h1>
            <p class="text-surface-500">{{ t('ordering.descriptions.fulfillment_queue') }}</p>
        </div>

        <DataTable :value="queue" :loading="loading" dataKey="id" stripedRows showGridlines>
            <Column field="number" :header="t('ordering.table.order_number')">
                <template #body="{ data }">
                    <span class="font-bold text-primary cursor-pointer" @click="router.push({ name: 'ordering.orders.detail', params: { id: data.id } })">
                        {{ data.number }}
                    </span>
                </template>
            </Column>
            <Column field="email" :header="t('ordering.table.customer')">
                 <template #body="{ data }">
                    <span>{{ data.email || 'Guest' }}</span>
                </template>
            </Column>
            <Column field="created_at" :header="t('ordering.table.date')">
                <template #body="{ data }">{{ formatDate(data.created_at) }}</template>
            </Column>
            <Column :header="t('ordering.table.actions')">
                <template #body="{ data }">
                    <Button :label="t('ordering.actions.ship_order')" icon="pi pi-box" severity="success" size="small" @click="shipOrder(data.id)" />
                </template>
            </Column>
        </DataTable>
    </template>
</Card>
</template>
