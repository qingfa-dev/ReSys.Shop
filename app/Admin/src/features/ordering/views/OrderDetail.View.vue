<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useOrderStore } from '../stores/order.store';
import { storeToRefs } from 'pinia';
import { useFormatter } from '@/shared/composables/formatter.use';
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use';
import { useConfirm } from 'primevue/useconfirm';
import type { PaymentDetail } from '../types/Order.Response.Type';
import type { UpdateAddressesRequest, AddOrderItemRequest } from '../types/Order.Request.Type';
import type { RefundPaymentRequest } from '../fulfillment/types/Fulfillment.Request.Type';
import ShipmentDialog from '../components/ShipmentDialog.Component.vue';
import AddressDialog from '../components/AddressDialog.Component.vue';
import ItemDialog from '../components/ItemDialog.Component.vue';
import RefundDialog from '../components/RefundDialog.Component.vue';
import { useI18n } from 'vue-i18n';
import PageShell from '@/shared/components/PageShell.Component.vue';
import PageHeader from '@/shared/components/PageHeader.Component.vue';

const route = useRoute();
const router = useRouter();
const store = useOrderStore();
const confirm = useConfirm();
const { current_order, loading, submitting } = storeToRefs(store);
const { t } = useI18n();
const { formatCurrency } = useFormatter();
const { handleApiResult } = useApiErrorHandler();

const orderId = route.params.id as string;
const cancelReason = ref('');
const showShipmentDialog = ref(false);
const showAddressDialog = ref(false);
const showItemDialog = ref(false);
const showRefundDialog = ref(false);
const selectedPayment = ref<PaymentDetail | null>(null);

onMounted(async () => {
    const result = await store.fetchOrderById(orderId);
    if (!result.isSuccess) {
        handleApiResult(result);
    }
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
    }
    handleApiResult(result);
};

const onRefund = async (data: RefundPaymentRequest) => {
    if (!selectedPayment.value) return;
    const result = await store.refundPayment(orderId, selectedPayment.value.id, data as unknown as Record<string, unknown>);
    if (result.isSuccess) {
        showRefundDialog.value = false;
        selectedPayment.value = null;
        await store.fetchOrderById(orderId);
    }
    handleApiResult(result);
};

const onCancelShipment = (shipmentId: string) => {
    confirm.require({
        message: t('ordering.messages.cancel_shipment_confirm'),
        header: t('ordering.titles.cancel_shipment'),
        icon: 'pi pi-exclamation-triangle',
        acceptClass: 'p-button-danger',
        accept: async () => {
            const result = await store.cancelShipment(orderId, shipmentId);
            handleApiResult(result);
        }
    });
};

const onCancel = () => {
    confirm.require({
        message: t('ordering.messages.cancel_order_confirm'),
        header: t('ordering.titles.confirm_cancellation'),
        icon: 'pi pi-exclamation-triangle',
        acceptClass: 'p-button-danger',
        accept: async () => {
            const result = await store.cancelOrder(orderId, cancelReason.value);
            handleApiResult(result);
        }
    });
};

const getStatusSeverity = (status: string) => {
    switch (status?.toLowerCase()) {
        case 'complete': return 'success';
        case 'processing': return 'info';
        case 'canceled': return 'danger';
        case 'payment_required': return 'warn';
        default: return 'secondary';
    }
};
</script>

<template>
    <PageShell :card="false" gap maxWidth="7xl">
        <template v-if="current_order">
            <PageHeader back :title="'Order ' + current_order.number">
                <template #badge>
                    <Tag :value="current_order.state" :severity="getStatusSeverity(current_order.state)" class="px-4 py-2 text-lg font-bold rounded-xl" />
                </template>
                <template #actions>
                    <Button 
                        :label="t('ordering.actions.advance_status')" 
                        icon="pi pi-arrow-right" 
                        :loading="submitting"
                        @click="onAdvance"
                        v-if="current_order.state !== 'Complete' && current_order.state !== 'Canceled'"
                        class="rounded-xl px-6"
                    />
                    <Button 
                        :label="t('ordering.actions.cancel_order')" 
                        icon="pi pi-times" 
                        severity="danger" 
                        outlined
                        @click="onCancel"
                        v-if="current_order.state !== 'Complete' && current_order.state !== 'Canceled'"
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
                                v-if="current_order.state !== 'Complete' && current_order.state !== 'Canceled'" 
                                @click="showItemDialog = true"
                            />
                        </div>
                    </template>
                    <template #content>
                        <DataTable :value="current_order.lineItems" class="p-datatable-sm" stripedRows showGridlines>
                            <Column :header="t('ordering.table.product')">
                                <template #body="{ data }">
                                    <div class="flex items-center gap-3">
                                        <div class="flex flex-col">
                                            <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.name }}</span>
                                            <small class="font-mono text-xs text-surface-500 uppercase tracking-widest">{{ data.sku }}</small>
                                        </div>
                                    </div>
                                </template>
                            </Column>
                            <Column field="unitPriceCents" :header="t('ordering.table.price')">
                                <template #body="{ data }">{{ formatCurrency(data.unitPriceCents / 100) }}</template>
                            </Column>
                            <Column field="quantity" :header="t('ordering.table.qty')" class="text-center"></Column>
                            <Column field="totalCents" :header="t('ordering.table.total')" class="text-right font-bold">
                                <template #body="{ data }">{{ formatCurrency(data.totalCents / 100) }}</template>
                            </Column>
                        </DataTable>

                        <div class="flex flex-col gap-3 mt-10 pt-6 border-t border-surface-100 dark:border-surface-800 max-w-sm ml-auto">
                            <div class="flex justify-between">
                                <span class="text-surface-500 font-medium uppercase text-xs tracking-widest">{{ t('ordering.labels.subtotal') }}</span>
                                <span class="font-bold">{{ formatCurrency(current_order.itemTotalCents / 100) }}</span>
                            </div>
                            <div class="flex justify-between">
                                <span class="text-surface-500 font-medium uppercase text-xs tracking-widest">{{ t('ordering.labels.shipping') }}</span>
                                <span class="font-bold">{{ formatCurrency(current_order.shipmentTotalCents / 100) }}</span>
                            </div>
                            <Divider />
                            <div class="flex justify-between items-center">
                                <span class="text-xl font-black">{{ t('ordering.labels.total') }}</span>
                                <span class="text-4xl font-black text-primary">{{ formatCurrency(current_order.totalCents / 100) }}</span>
                            </div>
                        </div>
                    </template>
                </Card>

                <!-- Addresses Grid -->
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
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
                                    v-if="current_order.state !== 'Complete' && current_order.state !== 'Canceled'"
                                />
                            </div>
                        </template>
                        <template #content>
                            <div v-if="current_order.shippingAddress" class="flex flex-col gap-1">
                                <span class="font-bold">{{ current_order.shippingAddress.firstName }} {{ current_order.shippingAddress.lastName }}</span>
                                <span>{{ current_order.shippingAddress.address1 }}</span>
                                <span v-if="current_order.shippingAddress.address2">{{ current_order.shippingAddress.address2 }}</span>
                                <span>{{ current_order.shippingAddress.zipCode }} {{ current_order.shippingAddress.city }}</span>
                                <span class="font-bold uppercase text-xs mt-2">{{ current_order.shippingAddress.countryCode }}</span>
                            </div>
                            <p v-else class="italic text-surface-400">{{ t('ordering.messages.no_shipping_address') }}</p>
                        </template>
                    </Card>

                    <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900">
                        <template #title>
                            <div class="flex justify-between items-center">
                                <span class="text-sm font-black uppercase tracking-widest text-surface-400">{{ t('ordering.labels.billing_address') }}</span>
                                <Button 
                                    icon="pi pi-pencil" 
                                    text 
                                    size="small" 
                                    rounded 
                                    severity="secondary" 
                                    @click="showAddressDialog = true"
                                    v-if="current_order.state !== 'Complete' && current_order.state !== 'Canceled'"
                                />
                            </div>
                        </template>
                        <template #content>
                            <div v-if="current_order.billingAddress" class="flex flex-col gap-1">
                                <span class="font-bold">{{ current_order.billingAddress.firstName }} {{ current_order.billingAddress.lastName }}</span>
                                <span>{{ current_order.billingAddress.address1 }}</span>
                                <span v-if="current_order.billingAddress.address2">{{ current_order.billingAddress.address2 }}</span>
                                <span>{{ current_order.billingAddress.zipCode }} {{ current_order.billingAddress.city }}</span>
                                <span class="font-bold uppercase text-xs mt-2">{{ current_order.billingAddress.countryCode }}</span>
                            </div>
                            <p v-else class="italic text-surface-400">{{ t('ordering.messages.no_billing_address') }}</p>
                        </template>
                    </Card>
                </div>

                 <Panel :header="t('ordering.panels.audit_log')" class="rounded-3xl shadow-sm border-none overflow-hidden" toggleable>
                    <div class="flex flex-col gap-2">
                        <div v-for="(event, i) in current_order.history" :key="i" class="flex justify-between items-center p-4 bg-surface-50 dark:bg-surface-800 rounded-2xl border border-surface-100 dark:border-surface-700">
                            <div class="flex flex-col">
                                <span class="font-bold text-sm">{{ event.description }}</span>
                                <div class="flex items-center gap-2 mt-1">
                                    <Tag :value="event.fromState || 'Initial'" severity="secondary" class="text-[10px]" />
                                    <i class="pi pi-arrow-right text-[10px] text-surface-400"></i>
                                    <Tag :value="event.toState" severity="primary" class="text-[10px]" />
                                </div>
                            </div>
                            <div class="flex flex-col items-end">
                                <span class="text-xs text-surface-400 font-mono">{{ new Date(event.createdAtUtc).toLocaleString() }}</span>
                                <small class="text-[10px] text-surface-500 uppercase font-bold tracking-tighter" v-if="event.triggeredBy">BY: {{ event.triggeredBy }}</small>
                            </div>
                        </div>
                        <p v-if="current_order.history.length === 0" class="text-sm italic text-surface-500 text-center py-4">{{ t('ordering.messages.no_history') }}</p>
                    </div>
                </Panel>
            </div>

            <!-- Right Col: Customer and Logistics -->
            <div class="flex flex-col gap-6">
                <Panel :header="t('ordering.panels.customer_communication')" class="rounded-3xl shadow-sm border-none overflow-hidden">
                    <div class="flex flex-col gap-4">
                        <div class="flex items-center gap-4">
                            <Avatar icon="pi pi-user" size="large" shape="circle" class="bg-primary/10 text-primary w-12 h-12" />
                            <div class="flex flex-col">
                                <span class="text-xs text-surface-400 font-bold uppercase tracking-widest">{{ t('ordering.labels.account') }}</span>
                                <span class="font-black text-lg">{{ current_order.email || 'Guest Checkout' }}</span>
                            </div>
                        </div>
                        <Button :label="t('ordering.actions.view_profile')" icon="pi pi-external-link" text size="small" class="w-full justify-start px-0" />
                    </div>
                </Panel>

                <Panel :header="t('ordering.panels.logistics')" class="rounded-3xl shadow-sm border-none overflow-hidden">
                    <div class="flex flex-col gap-4">
                        <div v-for="shipment in current_order.shipments" :key="shipment.id" class="p-4 border border-surface-100 dark:border-surface-800 rounded-2xl bg-surface-50/50 dark:bg-surface-800/50">
                            <div class="flex justify-between items-start mb-3">
                                <div class="flex flex-col">
                                    <span class="text-xs text-surface-400 font-bold uppercase tracking-tighter">{{ t('ordering.labels.number') }}</span>
                                    <span class="font-mono font-bold">{{ shipment.number }}</span>
                                </div>
                                <div class="flex flex-col items-end gap-2">
                                    <Tag :value="shipment.state" severity="info" class="text-[10px] uppercase font-black" />
                                    <Button 
                                        icon="pi pi-trash" 
                                        severity="danger" 
                                        text 
                                        size="small" 
                                        v-if="shipment.state !== 'Shipped' && shipment.state !== 'Canceled'"
                                        @click="onCancelShipment(shipment.id)"
                                    />
                                </div>
                            </div>
                            <div class="flex items-center gap-2 text-xs text-surface-500">
                                <i class="pi pi-building"></i>
                                <span>{{ shipment.stockLocationName || 'Warehouse TBD' }}</span>
                            </div>
                            <Button :label="t('ordering.actions.track_package')" icon="pi pi-map-marker" size="small" text class="mt-3 w-full" v-if="shipment.trackingNumber" />
                        </div>
                        <p v-if="current_order.shipments.length === 0" class="text-sm italic text-surface-500 text-center py-4">{{ t('ordering.messages.no_shipments') }}</p>
                        <Button :label="t('ordering.actions.create_manual_shipment')" icon="pi pi-box" outlined class="w-full rounded-xl" v-if="current_order.state === 'Processing'" @click="showShipmentDialog = true" />
                    </div>
                </Panel>
                
                 <Panel :header="t('ordering.panels.financials')" class="rounded-3xl shadow-sm border-none overflow-hidden">
                    <div class="flex flex-col gap-4">
                        <div v-for="payment in current_order.payments" :key="payment.id" class="p-4 border border-surface-100 dark:border-surface-800 rounded-2xl bg-surface-50/50 dark:bg-surface-800/50">
                            <div class="flex justify-between items-start mb-3">
                                <div class="flex flex-col">
                                    <span class="text-xs text-surface-400 font-bold uppercase tracking-tighter">{{ t('ordering.labels.method') }}</span>
                                    <span class="font-bold">{{ payment.methodType }}</span>
                                </div>
                                <div class="flex flex-col items-end gap-2">
                                    <Tag :value="payment.state" severity="success" class="text-[10px] uppercase font-black" />
                                    <Button 
                                        :label="t('ordering.actions.refund')" 
                                        icon="pi pi-undo" 
                                        severity="secondary" 
                                        text 
                                        size="small" 
                                        v-if="payment.state === 'Completed' || payment.state === 'Refunded'"
                                        @click="selectedPayment = payment; showRefundDialog = true"
                                    />
                                </div>
                            </div>
                            <div class="flex justify-between items-end">
                                <div class="flex flex-col">
                                    <span class="text-xs text-surface-400 font-bold uppercase tracking-tighter">{{ t('ordering.labels.amount') }}</span>
                                    <span class="font-black text-xl text-primary">{{ formatCurrency(payment.amountCents / 100) }}</span>
                                </div>
                                <span class="text-[10px] text-surface-400 font-mono">{{ new Date(payment.createdAtUtc).toLocaleDateString() }}</span>
                            </div>
                        </div>
                        <p v-if="current_order.payments.length === 0" class="text-sm italic text-surface-500 text-center py-4">{{ t('ordering.messages.no_payments') }}</p>
                        <Button :label="t('ordering.actions.capture_payment')" icon="pi pi-dollar" outlined class="w-full rounded-xl" v-if="current_order.state === 'Payment_Required'" />
                    </div>
                </Panel>
            </div>
        </div>

        <ShipmentDialog 
            v-if="showShipmentDialog && current_order" 
            :order="current_order" 
            @updated="store.fetchOrderById(orderId)" 
            @close="showShipmentDialog = false" 
        />

        <AddressDialog
            v-if="showAddressDialog && current_order"
            :shippingAddress="current_order.shippingAddress"
            :billingAddress="current_order.billingAddress"
            @save="onSaveAddresses"
            @close="showAddressDialog = false"
        />

        <ItemDialog
            v-if="showItemDialog"
            @save="onAddItem"
            @close="showItemDialog = false"
        />

        <RefundDialog
            v-if="showRefundDialog && selectedPayment"
            :payment="selectedPayment"
            @save="onRefund"
            @close="showRefundDialog = false; selectedPayment = null"
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
