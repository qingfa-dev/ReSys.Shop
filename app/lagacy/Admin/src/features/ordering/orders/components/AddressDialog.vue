<script setup lang="ts">
import { ref, onMounted } from 'vue';
import type { UpdateAddressesRequest } from '../types/order.request';
import { useI18n } from 'vue-i18n';
import FormField from '@/shared/components/form/FormField.vue';

const props = defineProps<{
    shipAddressId?: string | null;
    billAddressId?: string | null;
}>();

const emit = defineEmits<{
    (e: 'save', data: UpdateAddressesRequest): void;
    (e: 'close'): void;
}>();

const { t } = useI18n();

const shipAddr = ref({
    firstName: '',
    lastName: '',
    address1: '',
    address2: '',
    city: '',
    zipCode: '',
    countryCode: 'US'
});

const billAddr = ref({
    firstName: '',
    lastName: '',
    address1: '',
    address2: '',
    city: '',
    zipCode: '',
    countryCode: 'US'
});

const sameAsShipping = ref(true);

onMounted(() => {
    // Address details loaded via separate endpoint
});

const onSave = () => {
    const payload: UpdateAddressesRequest = {
        shippingAddress: shipAddr.value,
        billingAddress: sameAsShipping.value ? shipAddr.value : billAddr.value
    };
    emit('save', payload);
};
</script>

<template>
    <Dialog :header="t('ordering.actions.update_addresses')" visible modal class="w-full max-w-4xl" @update:visible="$emit('close')">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-8 py-4">
            <!-- Shipping -->
            <div class="flex flex-col gap-4">
                <h3 class="font-black uppercase tracking-widest text-surface-400 text-sm">{{ t('ordering.labels.shipping_address') }}</h3>
                <div class="grid grid-cols-2 gap-3">
                    <FormField label="First Name" name="shipFirstName">
                        <InputText v-model="shipAddr.firstName" class="p-inputtext-sm" />
                    </FormField>
                    <FormField label="Last Name" name="shipLastName">
                        <InputText v-model="shipAddr.lastName" class="p-inputtext-sm" />
                    </FormField>
                </div>
                <FormField label="Address line 1" name="shipAddress1">
                    <InputText v-model="shipAddr.address1" class="p-inputtext-sm" />
                </FormField>
                <FormField label="Address line 2" name="shipAddress2">
                    <InputText v-model="shipAddr.address2" class="p-inputtext-sm" />
                </FormField>
                <div class="grid grid-cols-2 gap-3">
                    <FormField label="City" name="shipCity">
                        <InputText v-model="shipAddr.city" class="p-inputtext-sm" />
                    </FormField>
                    <FormField label="Zip Code" name="shipZipCode">
                        <InputText v-model="shipAddr.zipCode" class="p-inputtext-sm" />
                    </FormField>
                </div>
                 <FormField label="Country Code (ISO)" name="shipCountryCode">
                    <InputText v-model="shipAddr.countryCode" class="p-inputtext-sm" />
                </FormField>
            </div>

            <!-- Billing -->
            <div class="flex flex-col gap-4">
                <div class="flex justify-between items-center">
                    <h3 class="font-black uppercase tracking-widest text-surface-400 text-sm">{{ t('ordering.labels.billing_address') }}</h3>
                    <div class="flex items-center gap-2">
                        <Checkbox v-model="sameAsShipping" :binary="true" inputId="same" />
                        <label for="same" class="text-xs font-bold cursor-pointer">Same as shipping</label>
                    </div>
                </div>

                <div v-if="!sameAsShipping" class="flex flex-col gap-4 animate-fade-in">
                    <div class="grid grid-cols-2 gap-3">
                        <FormField label="First Name" name="billFirstName">
                            <InputText v-model="billAddr.firstName" class="p-inputtext-sm" />
                        </FormField>
                        <FormField label="Last Name" name="billLastName">
                            <InputText v-model="billAddr.lastName" class="p-inputtext-sm" />
                        </FormField>
                    </div>
                    <FormField label="Address line 1" name="billAddress1">
                        <InputText v-model="billAddr.address1" class="p-inputtext-sm" />
                    </FormField>
                    <FormField label="Address line 2" name="billAddress2">
                        <InputText v-model="billAddr.address2" class="p-inputtext-sm" />
                    </FormField>
                    <div class="grid grid-cols-2 gap-3">
                        <FormField label="City" name="billCity">
                            <InputText v-model="billAddr.city" class="p-inputtext-sm" />
                        </FormField>
                        <FormField label="Zip Code" name="billZipCode">
                            <InputText v-model="billAddr.zipCode" class="p-inputtext-sm" />
                        </FormField>
                    </div>
                    <FormField label="Country Code (ISO)" name="billCountryCode">
                        <InputText v-model="billAddr.countryCode" class="p-inputtext-sm" />
                    </FormField>
                </div>
                <div v-else class="flex items-center justify-center h-full border-2 border-dashed border-surface-100 dark:border-surface-800 rounded-3xl text-surface-400 italic text-sm text-center p-8">
                    Billing information will match the shipping details.
                </div>
            </div>
        </div>

        <template #footer>
            <div class="flex justify-end gap-3 pt-4 border-t border-surface-100 dark:border-surface-800">
                <Button :label="t('common.cancel')" severity="secondary" text @click="$emit('close')" />
                <Button :label="t('ordering.actions.update_addresses')" icon="pi pi-check" @click="onSave" class="rounded-xl" />
            </div>
        </template>
    </Dialog>
</template>

<style scoped>
.animate-fade-in {
    animation: fadeIn 0.3s ease-in-out;
}
@keyframes fadeIn {
    from { opacity: 0; transform: translateY(-10px); }
    to { opacity: 1; transform: translateY(0); }
}
</style>
