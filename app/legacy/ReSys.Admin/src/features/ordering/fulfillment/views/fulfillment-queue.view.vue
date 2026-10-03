<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useFulfillmentStore } from '../stores/fulfillment.store';
import { storeToRefs } from 'pinia';
import { useFormatter } from '@/shared/composables/formatter.use';
import { useRouter } from 'vue-router';

const store = useFulfillmentStore();
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
    <div class="card">
        <div class="flex justify-between items-center mb-6">
            <h1 class="text-2xl font-bold">Fulfillment Queue</h1>
            <p class="text-surface-500">Orders ready for picking and packing.</p>
        </div>

        <DataTable :value="queue" :loading="loading" dataKey="id">
            <Column field="number" header="Order #">
                <template #body="{ data }">
                    <span class="font-bold text-primary cursor-pointer" @click="router.push({ name: 'ordering.orders.detail', params: { id: data.id } })">
                        {{ data.number }}
                    </span>
                </template>
            </Column>
            <Column field="email" header="Customer">
                 <template #body="{ data }">
                    <span>{{ data.email || 'Guest' }}</span>
                </template>
            </Column>
            <Column field="created_at" header="Date">
                <template #body="{ data }">{{ formatDate(data.created_at) }}</template>
            </Column>
            <Column header="Actions">
                <template #body="{ data }">
                    <Button label="Ship Order" icon="pi pi-box" severity="success" size="small" @click="shipOrder(data.id)" />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
