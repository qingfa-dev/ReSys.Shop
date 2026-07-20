<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useInventoryStore } from '../../store/inventory.store';
import { useProductStore } from '@/features/catalog/products/store/product.store';
import { useToast } from '@/common/composables/toast.use';
import { useFormatter } from '@/common/composables/formatter.use';
import { useI18n } from 'vue-i18n';
import PageShell from '@/shared/components/navigation/PageShell.vue';
import PageHeader from '@/shared/components/navigation/PageHeader.vue';
import type { StockTransferDetail } from '../types/stock-transfer.response';
import type { ProductSummary } from '@/features/catalog/products/types/product.response';

const { t } = useI18n();

const route = useRoute();
const router = useRouter();
const store = useInventoryStore();
const productStore = useProductStore();
const { showToast } = useToast();
const { formatDate, formatCurrency } = useFormatter();

const transferId = computed(() => route.params.id as string);
const transfer = ref<StockTransferDetail | null>(null);
const loading = ref(false);
const processing = ref(false);

// Add Item Logic
const itemDialog = ref(false);
const selectedProduct = ref<ProductSummary | null>(null);
const productResults = ref<ProductSummary[]>([]);
const quantity = ref(1);

async function loadTransfer() {
    loading.value = true;
    try {
        const res = await store.inventoryService.getTransferDetail(transferId.value);
        if (res.isSuccess && res.value) {
            transfer.value = res.value;
        }
    } finally {
        loading.value = false;
    }
}

const onSearchProduct = async (event: { query: string }) => {
    const res = await productStore.fetchProducts({ search: event.query, pageSize: 5 });
    if (!res) return;
    if (res.isSuccess) {
        const data = 'items' in res ? res.items : res.value;
        if (data) productResults.value = data;
    }
};

async function onAddItem() {
    if (!selectedProduct.value) return;
    processing.value = true;
    try {
        const res = await store.inventoryService.addTransferItem(transferId.value, selectedProduct.value.id, quantity.value);
        if (res.isSuccess) {
            showToast('success', t('common.success'), t('inventory.messages.item_added_to_transfer'));
            itemDialog.value = false;
            selectedProduct.value = null;
            quantity.value = 1;
            await loadTransfer();
        }
    } finally {
        processing.value = false;
    }
}

async function onShip() {
    processing.value = true;
    try {
        const res = await store.inventoryService.shipTransfer(transferId.value);
        if (res.isSuccess) {
            showToast('success', t('common.success'), t('inventory.messages.transfer_shipped'));
            await loadTransfer();
        }
    } finally {
        processing.value = false;
    }
}

async function onReceive() {
    processing.value = true;
    try {
        const res = await store.inventoryService.receiveTransfer(transferId.value);
        if (res.isSuccess) {
            showToast('success', t('common.success'), t('inventory.messages.stock_received'));
            await loadTransfer();
        }
    } finally {
        processing.value = false;
    }
}

const getStatusSeverity = (status?: number) => {
    switch (status) {
        case 2: return 'success';
        case 1: return 'info';
        case 0: return 'warning';
        case 3: return 'danger';
        default: return 'secondary';
    }
};

const statusLabel = (status?: number) => {
    switch (status) {
        case 0: return 'Draft';
        case 1: return 'In Transit';
        case 2: return 'Received';
        case 3: return 'Canceled';
        default: return 'Unknown';
    }
};

onMounted(() => {
    loadTransfer();
});
</script>

<template>
    <PageShell :card="false" gap maxWidth="7xl">
        <template v-if="transfer">
            <PageHeader back :title="transfer.reference" :description="'Initiated on ' + formatDate(transfer.createdAtUtc)">
                <template #badge>
                    <Tag :value="statusLabel(transfer.status)" :severity="getStatusSeverity(transfer.status)" rounded class="font-bold px-3" />
                </template>
                <template #actions>
                    <Button v-if="transfer.status === 0" :label="t('inventory.actions.ship')" icon="pi pi-send" class="rounded-xl px-6" :loading="processing" @click="onShip" />
                    <Button v-if="transfer.status === 1" :label="t('inventory.actions.receive')" icon="pi pi-download" severity="success" class="rounded-xl px-6" :loading="processing" @click="onReceive" />
                </template>
            </PageHeader>
        </template>

        <div v-if="loading" class="flex justify-center p-20">
            <ProgressSpinner />
        </div>

        <div v-else-if="transfer" class="grid grid-cols-1 lg:grid-cols-3 gap-8">
            <!-- Left: Items -->
            <div class="lg:col-span-2 flex flex-col gap-6">
                <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
                    <template #title>
                        <div class="flex justify-between items-center p-4">
                            <span class="text-xl font-black uppercase tracking-tight">{{ t('inventory.titles.merchandise') }}</span>
                                <Button v-if="transfer.status === 0" :label="t('inventory.actions.add')" icon="pi pi-plus" size="small" text @click="itemDialog = true" />
                        </div>
                    </template>
                    <template #content>
                        <DataTable :value="transfer.items" class="p-datatable-sm" stripedRows showGridlines>
                            <template #empty>
                                <div class="p-8 text-center text-surface-400 italic">No items added to this transfer yet.</div>
                            </template>
                            <Column :header="t('inventory.table.product')">
                                <template #body="{ data }">
                                    <div class="flex flex-col">
                                        <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.variantName }}</span>
                                        <small class="font-mono text-xs text-surface-500 uppercase">{{ data.sku }}</small>
                                    </div>
                                </template>
                            </Column>
                            <Column field="quantity" :header="t('inventory.table.quantity')" class="text-right font-mono font-bold"></Column>
                            <Column class="w-12 text-right" v-if="transfer.status === 0">
                                <template #body>
                                    <Button icon="pi pi-trash" severity="danger" text rounded />
                                </template>
                            </Column>
                        </DataTable>
                    </template>
                </Card>

                <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900" v-if="transfer.notes">
                    <template #title><span class="text-sm font-black uppercase tracking-widest text-surface-400">Notes</span></template>
                    <template #content>
                        <p class="m-0 italic text-surface-600 dark:text-surface-300">"{{ transfer.notes }}"</p>
                    </template>
                </Card>
            </div>

            <!-- Right: Logistics -->
            <div class="flex flex-col gap-6">
                <Card class="rounded-3xl shadow-sm border-none bg-surface-900 text-surface-0 overflow-hidden">
                    <template #content>
                        <div class="flex flex-col gap-8 p-4">
                            <div class="flex flex-col gap-2">
                                <span class="text-xs font-black uppercase tracking-widest text-surface-500">Source Location</span>
                                <div class="flex items-center gap-3">
                                    <div class="w-10 h-10 rounded-xl bg-surface-800 flex items-center justify-center">
                                        <i class="pi pi-building text-surface-400"></i>
                                    </div>
                                    <span class="font-bold text-lg">{{ transfer.sourceLocationName }}</span>
                                </div>
                            </div>

                            <div class="flex justify-center -my-4">
                                <i class="pi pi-arrow-down text-primary text-xl"></i>
                            </div>

                            <div class="flex flex-col gap-2">
                                <span class="text-xs font-black uppercase tracking-widest text-surface-500">{{ t('inventory.titles.destination') }}</span>
                                <div class="flex items-center gap-3">
                                    <div class="w-10 h-10 rounded-xl bg-primary/20 flex items-center justify-center text-primary">
                                        <i class="pi pi-map-marker"></i>
                                    </div>
                                    <span class="font-bold text-lg">{{ transfer.destinationLocationName }}</span>
                                </div>
                            </div>
                        </div>
                    </template>
                </Card>
            </div>
        </div>

        <!-- Add Item Dialog -->
        <Dialog v-model:visible="itemDialog" :header="t('inventory.titles.add_transfer_item')" modal class="w-full max-w-lg">
            <div class="flex flex-col gap-6 py-4">
                <div class="flex flex-col gap-2">
                    <label class="font-bold text-sm">Search Product</label>
                    <AutoComplete 
                        v-model="selectedProduct" 
                        :suggestions="productResults" 
                        @complete="onSearchProduct" 
                        optionLabel="name"
                        class="w-full"
                        inputClass="w-full h-12 px-4 rounded-xl"
                        placeholder="Type to search SKU or name..."
                    />
                </div>
                <div class="flex flex-col gap-2">
                    <label class="font-bold text-sm">Quantity to Move</label>
                    <InputNumber v-model="quantity" showButtons :min="1" class="w-full" />
                </div>
            </div>
            <template #footer>
                <Button :label="t('inventory.actions.cancel')" severity="secondary" text @click="itemDialog = false" />
                <Button :label="t('inventory.actions.add_to_transfer')" icon="pi pi-plus" :loading="processing" @click="onAddItem" />
            </template>
        </Dialog>
    </PageShell>
</template>
