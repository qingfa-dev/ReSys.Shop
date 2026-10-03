<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useOrderStore } from '../store/order.store';
import { storeToRefs } from 'pinia';
import { useFormatter } from '@/common/composables/formatter.use';
import { useApiErrorHandler } from '@/common/composables/api-error-handler.use';
import ConfirmDialog from '@/shared/components/overlays/ConfirmDialog.vue';
import { OrderStatusMap } from '@/shared/utils/enums';
import StatusBadge from '@/shared/components/feedback/StatusBadge.vue';
import DetailField from '@/shared/components/data-display/DetailField.vue';
import { orderRepository } from '../api/order.api';
import type { UpdateAddressesRequest, AddOrderItemRequest } from '../types/order.request';
import type { RefundPaymentRequest } from '../../fulfillment/types/fulfillment.request';
import type { OrderLineItem } from '../api/order.api';
import AddressDialog from '../components/AddressDialog.vue';
import ItemDialog from '../components/ItemDialog.vue';
import { useI18n } from 'vue-i18n';
import PageShell from '@/shared/components/navigation/PageShell.vue';
import PageHeader from '@/shared/components/navigation/PageHeader.vue';
import InputNumber from 'primevue/inputnumber';

const route = useRoute();
const router = useRouter();
const store = useOrderStore();
const { current_order, loading, submitting } = storeToRefs(store);
const { t } = useI18n();
const { formatCurrency } = useFormatter();
const { handleApiResult } = useApiErrorHandler();

const orderId = route.params.id as string;
const cancelReason = ref('');
const showAddressDialog = ref(false);
const showItemDialog = ref(false);

const lineItems = ref<OrderLineItem[]>([]);
const loadingLineItems = ref(false);
const editingLineItemId = ref<string | null>(null);
const editingQuantity = ref(1);

async function loadLineItems() {
  loadingLineItems.value = true;
  try {
    const result = await orderRepository.listLineItems(orderId);
    if (result.isSuccess) {
      lineItems.value = result.items;
    }
  } finally {
    loadingLineItems.value = false;
  }
}

onMounted(async () => {
    const result = await store.fetchOrderById(orderId);
    if (!result.isSuccess) {
        handleApiResult(result);
    }
    await loadLineItems();
});

const onAdvance = async () => {
    if (!current_order.value) return;
    const result = await store.advanceOrderState(orderId);
    handleApiResult(result);
};

const onSaveAddresses = async (data: UpdateAddressesRequest) => {
    const result = await store.updateOrderAddresses(orderId, data);
    if (result.isSuccess) {
        showAddressDialog.value = false;
        await store.fetchOrderById(orderId);
    }
    handleApiResult(result);
};

const onAddItem = async (data: AddOrderItemRequest) => {
    const result = await store.addOrderItem(orderId, data);
    if (result.isSuccess) {
        showItemDialog.value = false;
        await store.fetchOrderById(orderId);
        await loadLineItems();
    }
    handleApiResult(result);
};

const cancelOrder = async () => {
  const result = await store.cancelOrder(orderId, cancelReason.value);
  handleApiResult(result);
};

const orderStatusMap: Record<number, { label: string; severity: string }> = {
  0: { label: OrderStatusMap[0]!, severity: 'info' },
  1: { label: OrderStatusMap[1]!, severity: 'success' },
  2: { label: OrderStatusMap[2]!, severity: 'danger' },
  4: { label: OrderStatusMap[4]!, severity: 'warn' },
};

const onResume = async () => {
  const result = await store.resumeOrder(orderId);
  handleApiResult(result);
  if (result.isSuccess) await loadLineItems();
};

function startEditLineItem(lineItem: OrderLineItem) {
  editingLineItemId.value = lineItem.id;
  editingQuantity.value = lineItem.quantity;
}

function cancelEditLineItem() {
  editingLineItemId.value = null;
}

async function saveEditLineItem() {
  if (!editingLineItemId.value) return;
  const result = await store.updateLineItem(orderId, editingLineItemId.value, { quantity: editingQuantity.value });
  editingLineItemId.value = null;
  handleApiResult(result);
  if (result.isSuccess) await loadLineItems();
}

async function removeLineItem(lineItemId: string) {
  const result = await store.removeLineItem(orderId, lineItemId);
  handleApiResult(result);
  if (result.isSuccess) await loadLineItems();
}
</script>

<template>
    <PageShell :card="false" gap maxWidth="7xl">
        <template v-if="current_order">
            <PageHeader back :title="'Order ' + current_order.number">
                <template #badge>
                    <StatusBadge :status="current_order.status" :status-map="orderStatusMap" />
                </template>
                <template #actions>
                    <Button 
                    :label="t('ordering.actions.advance_status')" 
                    icon="pi pi-arrow-right" 
                    :loading="submitting"
                    @click="onAdvance"
                    v-if="current_order.status !== 1 && current_order.status !== 2"
                    class="rounded-xl px-6"
                />
                <ConfirmDialog
                    :header="t('ordering.titles.confirm_cancellation') || 'Cancel Order'"
                    :message="t('ordering.messages.cancel_order_confirm') || 'Are you sure you want to cancel this order?'"
                    icon="pi pi-exclamation-triangle"
                    severity="danger"
                    @confirm="cancelOrder"
                    v-if="current_order.status !== 1 && current_order.status !== 2"
                >
                    {{ t('ordering.actions.cancel_order') || 'Cancel Order' }}
                </ConfirmDialog>
                <Button
                    :label="t('ordering.actions.resume_order')"
                    icon="pi pi-undo"
                    severity="warn"
                    outlined
                    @click="onResume"
                    v-if="current_order.status === 2"
                    class="rounded-xl px-6"
                />
                </template>
            </PageHeader>
        </template>

        <div v-if="loading && !current_order" class="flex justify-center py-20">
            <ProgressSpinner />
        </div>

        <div v-else-if="current_order" class="grid grid-cols-1 lg:grid-cols-3 gap-8">
            <!-- Left Col: Items and Totals -->
            <div class="lg:col-span-2 flex flex-col gap-6">
                <!-- Items Card -->
                <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
                    <template #title>
                        <div class="flex justify-between items-center p-4">
                            <span class="text-xl font-black uppercase tracking-tight">{{ t('ordering.titles.items') }}</span>
                            <Button 
                                :label="t('ordering.actions.add_item')" 
                                icon="pi pi-plus" 
                                size="small" 
                                text 
                                v-if="current_order.status !== 1 && current_order.status !== 2" 
                                @click="showItemDialog = true"
                            />
                        </div>
                    </template>
                    <template #content>
                        <DataTable :value="lineItems" stripedRows class="text-sm" v-if="lineItems.length">
                            <Column field="sku" :header="t('ordering.table.sku')" />
                            <Column field="name" :header="t('ordering.table.product')" />
                            <Column :header="t('ordering.table.qty')">
                                <template #body="slotProps">
                                    <div v-if="editingLineItemId === slotProps.data.id" class="flex items-center gap-2">
                                        <InputNumber v-model="editingQuantity" :min="1" size="small" style="width: 80px" />
                                        <Button icon="pi pi-check" size="small" rounded text severity="success" @click="saveEditLineItem" />
                                        <Button icon="pi pi-times" size="small" rounded text severity="danger" @click="cancelEditLineItem" />
                                    </div>
                                    <span v-else>{{ slotProps.data.quantity }}</span>
                                </template>
                            </Column>
                            <Column :header="t('ordering.table.price')">
                                <template #body="slotProps">
                                    {{ formatCurrency(slotProps.data.unitPriceCents / 100) }}
                                </template>
                            </Column>
                            <Column header="Total">
                                <template #body="slotProps">
                                    <span class="font-bold">{{ formatCurrency(slotProps.data.totalPriceCents / 100) }}</span>
                                </template>
                            </Column>
                            <Column :header="t('ordering.table.actions')">
                                <template #body="slotProps">
                                    <div class="flex gap-1">
                                        <Button
                                            v-if="editingLineItemId !== slotProps.data.id"
                                            icon="pi pi-pencil"
                                            size="small"
                                            rounded
                                            text
                                            severity="secondary"
                                            @click="startEditLineItem(slotProps.data)"
                                            :disabled="current_order.status === 2"
                                        />
                                        <ConfirmDialog
                                            :header="t('ordering.titles.confirm_remove')"
                                            :message="t('ordering.messages.remove_line_item_confirm')"
                                            @confirm="removeLineItem(slotProps.data.id)"
                                            :disabled="current_order.status === 2"
                                        />
                                    </div>
                                </template>
                            </Column>
                        </DataTable>
                        <div v-else-if="!loadingLineItems" class="text-center py-8 text-surface-400 italic">
                            {{ t('ordering.messages.no_items') }}
                        </div>
                        <div v-else class="text-center py-8">
                            <ProgressSpinner style="width: 24px; height: 24px" />
                        </div>

                        <div class="flex flex-col gap-3 mt-6 pt-6 border-t border-surface-100 dark:border-surface-800 max-w-sm ml-auto">
                            <div class="flex justify-between">
                                <span class="text-surface-500 font-medium uppercase text-xs tracking-widest">{{ t('ordering.labels.subtotal') }}</span>
                                <span class="font-bold">{{ current_order.itemTotalDisplay }}</span>
                            </div>
                            <div class="flex justify-between">
                                <span class="text-surface-500 font-medium uppercase text-xs tracking-widest">{{ t('ordering.labels.shipping') }}</span>
                                <span class="font-bold">{{ current_order.shipmentTotalDisplay }}</span>
                            </div>
                            <Divider />
                            <div class="flex justify-between items-center">
                                <span class="text-xl font-black">{{ t('ordering.labels.total') }}</span>
                                <span class="text-4xl font-black text-primary">{{ current_order.totalDisplay }}</span>
                            </div>
                        </div>
                    </template>
                </Card>

                <!-- Addresses -->
                <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900">
                    <template #title>
                        <div class="flex justify-between items-center">
                            <span class="text-sm font-black uppercase tracking-widest text-surface-400">{{ t('ordering.labels.shipping_address') }}</span>
                            <Button 
                                icon="pi pi-pencil" 
                                text 
                                size="small" 
                                rounded 
                                severity="secondary" 
                                @click="showAddressDialog = true"
                                v-if="current_order.status !== 1 && current_order.status !== 2"
                            />
                        </div>
                    </template>
                    <template #content>
                        <DetailField :label="t('ordering.labels.shipping_address')" :value="current_order.shipAddressId" :empty-text="t('ordering.messages.no_shipping_address')" />
                    </template>
                </Card>
            </div>

            <!-- Right Col: Customer and Logistics -->
            <div class="flex flex-col gap-6">
                <Panel :header="t('ordering.panels.customer_communication')" class="rounded-3xl shadow-sm border-none overflow-hidden">
                    <div class="flex flex-col gap-4">
                        <div class="flex items-center gap-4">
                            <Avatar icon="pi pi-user" size="large" shape="circle" class="bg-primary/10 text-primary w-12 h-12" />
                            <DetailField :label="t('ordering.labels.account')" :value="current_order.email || 'Guest Checkout'" />
                        </div>
                        <Button :label="t('ordering.actions.view_profile')" icon="pi pi-external-link" text size="small" class="w-full justify-start px-0" />
                    </div>
                </Panel>


            </div>
        </div>

        <AddressDialog
            v-if="showAddressDialog && current_order"
            :shipAddressId="current_order.shipAddressId"
            :billAddressId="current_order.billAddressId"
            @save="onSaveAddresses"
            @close="showAddressDialog = false"
        />

        <ItemDialog
            v-if="showItemDialog"
            @save="onAddItem"
            @close="showItemDialog = false"
        />
    </PageShell>
</template>

<style scoped>
:deep(.p-card-body) {
    padding: 0;
}
:deep(.p-card-content) {
    padding: 1.5rem;
}
:deep(.p-panel-header) {
    background: transparent;
    border: none;
    padding: 1.5rem 1.5rem 0.5rem 1.5rem;
    font-weight: 900;
    text-transform: uppercase;
    font-size: 0.875rem;
    letter-spacing: 0.1em;
    color: var(--p-surface-400);
}
:deep(.p-panel-content) {
    border: none;
    padding: 1.5rem;
}
</style>
