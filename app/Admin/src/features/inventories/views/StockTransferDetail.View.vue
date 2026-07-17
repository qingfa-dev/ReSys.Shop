<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useInventoryStore } from '../stores/inventory.store';
import { useProductStore } from '@/features/catalog/products/stores/product.store';
import { useToast } from '@/shared/composables/toast.use';
import { useFormatter } from '@/shared/composables/formatter.use';
import { useI18n } from 'vue-i18n';
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
import type { StockTransferDetail } from '../types/inventory.types';

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
const selectedProduct = ref<any>(null);
const productResults = ref<any[]>([]);
const quantity = ref(1);

async function loadTransfer() {
    loading.value = true;
    try {
        const res = await store.inventoryService.getTransferDetail(transferId.value);
        if (res.success && res.data) {
            transfer.value = res.data;
        }
    } finally {
        loading.value = false;
    }
}

const onSearchProduct = async (event: { query: string }) => {
    const res = await productStore.fetchProducts({ search: event.query, pageSize: 5 });
    if (res.success && res.data) {
        productResults.value = res.data;
    }
};

async function onAddItem() {
    if (!selectedProduct.value) return;
    processing.value = true;
    try {
        const res = await store.inventoryService.addTransferItem(transferId.value, selectedProduct.value.id, quantity.value);
        if (res.success) {
            showToast('success', 'Success', 'Item added to transfer');
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
        if (res.success) {
            showToast('success', 'Success', t('inventory.messages.create_transfer_success') || 'Transfer shipped');
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
        if (res.success) {
            showToast('success', 'Success', 'Stock received at destination');
            await loadTransfer();
        }
    } finally {
        processing.value = false;
    }
}

const getStatusSeverity = (state?: string) => {
    switch (state) {
        case 'Received': return 'success';
        case 'InTransit': return 'info';
        case 'Draft': return 'warning';
        case 'Canceled': return 'danger';
        default: return 'secondary';
    }
};

onMounted(() => {
    loadTransfer();
});
</script>

<template>
    <div class="p-6 max-w-6xl mx-auto">
        <AppBreadcrumb :locales="t" />
        
        <div v-if="transfer" class="flex flex-col md:flex-row md:items-center justify-between gap-4 mt-4 mb-8">
            <div class="flex items-center gap-4">
                <Button icon="pi pi-arrow-left" text rounded severity="secondary" @click="router.back()" class="bg-surface-100 dark:bg-surface-800" />
                <div class="flex flex-col">
                    <div class="flex items-center gap-3">
                        <h2 class="text-4xl font-black tracking-tighter text-surface-900 dark:text-surface-50 m-0">
                            {{ transfer.referenceNumber }}
                        </h2>
                        <Tag :value="transfer.state" :severity="getStatusSeverity(transfer.state)" rounded class="font-bold px-3" />
                    </div>
                    <p class="text-sm text-surface-500 m-0">Initiated on {{ formatDate(transfer.createdAtUtc) }}</p>
                </div>
            </div>
            <div class="flex items-center gap-3">
                <Button v-if="transfer.state === 'Draft'" :label="t('inventory.actions.ship')" icon="pi pi-send" class="rounded-xl px-6" :loading="processing" @click="onShip" />
                <Button v-if="transfer.state === 'InTransit'" :label="t('inventory.actions.receive')" icon="pi pi-download" severity="success" class="rounded-xl px-6" :loading="processing" @click="onReceive" />
            </div>
        </div>

        <div v-if="loading" class="flex justify-center p-20">
            <ProgressSpinner />
        </div>

        <div v-else-if="transfer" class="grid grid-cols-1 lg:grid-cols-3 gap-8">
            <!-- Left: Items -->
            <div class="lg:col-span-2 flex flex-col gap-6">
                <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
                    <template #title>
                        <div class="flex justify-between items-center p-4">
                            <span class="text-xl font-black uppercase tracking-tight">Merchandise</span>
                            <Button v-if="transfer.state === 'Draft'" :label="t('inventory.actions.add')" icon="pi pi-plus" size="small" text @click="itemDialog = true" />
                        </div>
                    </template>
                    <template #content>
                        <DataTable :value="transfer.items" class="p-datatable-sm" stripedRows showGridlines>
                            <template #empty>
                                <div class="p-8 text-center text-surface-400 italic">No items added to this transfer yet.</div>
                            </template>
                            <Column header="Product">
                                <template #body="{ data }">
                                    <div class="flex flex-col">
                                        <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.variantName }}</span>
                                        <small class="font-mono text-xs text-surface-500 uppercase">{{ data.sku }}</small>
                                    </div>
                                </template>
                            </Column>
                            <Column field="quantity" header="Quantity" class="text-right font-mono font-bold"></Column>
                            <Column class="w-12 text-right" v-if="transfer.state === 'Draft'">
                                <template #body>
                                    <Button icon="pi pi-trash" severity="danger" text rounded />
                                </template>
                            </Column>
                        </DataTable>
                    </template>
                </Card>

                <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900" v-if="transfer.reason">
                    <template #title><span class="text-sm font-black uppercase tracking-widest text-surface-400">Transfer Reason</span></template>
                    <template #content>
                        <p class="m-0 italic text-surface-600 dark:text-surface-300">"{{ transfer.reason }}"</p>
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
                                <span class="text-xs font-black uppercase tracking-widest text-surface-500">Destination</span>
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
        <Dialog v-model:visible="itemDialog" header="Add Transfer Item" modal class="w-full max-w-lg">
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
                <Button label="Cancel" severity="secondary" text @click="itemDialog = false" />
                <Button label="Add to Transfer" icon="pi pi-plus" :loading="processing" @click="onAddItem" />
            </template>
        </Dialog>
    </div>
</template>
