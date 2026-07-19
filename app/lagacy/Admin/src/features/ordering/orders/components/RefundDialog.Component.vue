<script setup lang="ts">
import { ref } from 'vue';
import type { RefundPaymentRequest } from '../../fulfillment/types/fulfillment.request.type';
import { useFormatter } from '@/shared/composables/formatter.use';
import { useI18n } from 'vue-i18n';

const props = defineProps<{
    payment: {
        id: string;
        amount: number;
        methodName: string;
    };
}>();

const emit = defineEmits<{
    (e: 'save', data: RefundPaymentRequest): void;
    (e: 'close'): void;
}>();

const { t } = useI18n();
const { formatCurrency } = useFormatter();

const amount = ref(props.payment.amount);
const reason = ref('Customer requested cancellation');

const onSave = () => {
    emit('save', {
        amountCents: Math.round(amount.value * 100),  // TODO: update when backend supports decimal
        reason: reason.value
    });
};
</script>

<template>
    <Dialog :header="t('ordering.actions.process_refund')" visible modal class="w-full max-w-md" @update:visible="$emit('close')">
        <div class="flex flex-col gap-6 py-4">
            <div class="p-4 bg-surface-50 dark:bg-surface-800 rounded-2xl border border-surface-100 dark:border-surface-700">
                <div class="flex justify-between items-center mb-2">
                    <span class="text-xs font-black uppercase tracking-widest text-surface-400">{{ t('ordering.labels.amount') }}</span>
                    <span class="font-bold">{{ formatCurrency(payment.amount) }}</span>
                </div>
                <div class="flex justify-between items-center">
                    <span class="text-xs font-black uppercase tracking-widest text-surface-400">{{ t('ordering.labels.method') }}</span>
                    <Tag :value="payment.methodName" severity="secondary" class="text-[10px]" />
                </div>
            </div>

            <div class="flex flex-col gap-2">
                <label class="font-bold text-sm">Refund Amount</label>
                <InputNumber v-model="amount" mode="currency" currency="USD" locale="en-US" class="w-full" inputClass="h-12" :max="payment.amount" />
                <small class="text-surface-500">Partial refunds are supported.</small>
            </div>

            <div class="flex flex-col gap-2">
                <label class="font-bold text-sm">{{ t('ordering.labels.reason') }}</label>
                <Textarea v-model="reason" rows="3" class="w-full" placeholder="Why is this payment being refunded?" />
            </div>
        </div>

        <template #footer>
            <div class="flex justify-end gap-3 pt-4 border-t border-surface-100 dark:border-surface-800">
                <Button :label="t('common.cancel')" severity="secondary" text @click="$emit('close')" />
                <Button :label="t('ordering.actions.process_refund')" icon="pi pi-undo" severity="danger" @click="onSave" class="rounded-xl" />
            </div>
        </template>
    </Dialog>
</template>
