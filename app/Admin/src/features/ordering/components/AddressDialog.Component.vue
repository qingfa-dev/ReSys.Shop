<script setup lang="ts">
import { ref, onMounted } from 'vue';
import type { AddressDetail } from '../types/order.domain.types';
import type { UpdateAddressesRequest } from '../types/order.request.types';

const props = defineProps<{
    shippingAddress?: AddressDetail;
    billingAddress?: AddressDetail;
}>();

const emit = defineEmits<{
    (e: 'save', data: UpdateAddressesRequest): void;
    (e: 'close'): void;
}>();

const shipAddr = ref<Partial<AddressDetail>>({
    firstName: '',
    lastName: '',
    address1: '',
    address2: '',
    city: '',
    zipCode: '',
    countryCode: 'US'
});

const billAddr = ref<Partial<AddressDetail>>({
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
    if (props.shippingAddress) {
        shipAddr.value = { ...props.shippingAddress };
    }
    if (props.billingAddress) {
        billAddr.value = { ...props.billingAddress };
        
        // Simple check if they are the same
        const s = props.shippingAddress;
        const b = props.billingAddress;
        if (s && b && s.address1 === b.address1 && s.zipCode === b.zipCode) {
            sameAsShipping.value = true;
        } else {
            sameAsShipping.value = false;
        }
    }
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
    <Dialog header="Edit Order Addresses" visible modal class="w-full max-w-4xl" @update:visible="$emit('close')">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-8 py-4">
            <!-- Shipping -->
            <div class="flex flex-col gap-4">
                <h3 class="font-black uppercase tracking-widest text-surface-400 text-sm">Shipping Address</h3>
                <div class="grid grid-cols-2 gap-3">
                    <div class="flex flex-col gap-1">
                        <label class="text-xs font-bold">First Name</label>
                        <InputText v-model="shipAddr.firstName" class="p-inputtext-sm" />
                    </div>
                    <div class="flex flex-col gap-1">
                        <label class="text-xs font-bold">Last Name</label>
                        <InputText v-model="shipAddr.lastName" class="p-inputtext-sm" />
                    </div>
                </div>
                <div class="flex flex-col gap-1">
                    <label class="text-xs font-bold">Address line 1</label>
                    <InputText v-model="shipAddr.address1" class="p-inputtext-sm" />
                </div>
                <div class="flex flex-col gap-1">
                    <label class="text-xs font-bold">Address line 2</label>
                    <InputText v-model="shipAddr.address2" class="p-inputtext-sm" />
                </div>
                <div class="grid grid-cols-2 gap-3">
                    <div class="flex flex-col gap-1">
                        <label class="text-xs font-bold">City</label>
                        <InputText v-model="shipAddr.city" class="p-inputtext-sm" />
                    </div>
                    <div class="flex flex-col gap-1">
                        <label class="text-xs font-bold">Zip Code</label>
                        <InputText v-model="shipAddr.zipCode" class="p-inputtext-sm" />
                    </div>
                </div>
                 <div class="flex flex-col gap-1">
                    <label class="text-xs font-bold">Country Code (ISO)</label>
                    <InputText v-model="shipAddr.countryCode" class="p-inputtext-sm" />
                </div>
            </div>

            <!-- Billing -->
            <div class="flex flex-col gap-4">
                <div class="flex justify-between items-center">
                    <h3 class="font-black uppercase tracking-widest text-surface-400 text-sm">Billing Address</h3>
                    <div class="flex items-center gap-2">
                        <Checkbox v-model="sameAsShipping" :binary="true" inputId="same" />
                        <label for="same" class="text-xs font-bold cursor-pointer">Same as shipping</label>
                    </div>
                </div>

                <div v-if="!sameAsShipping" class="flex flex-col gap-4 animate-fade-in">
                    <div class="grid grid-cols-2 gap-3">
                        <div class="flex flex-col gap-1">
                            <label class="text-xs font-bold">First Name</label>
                            <InputText v-model="billAddr.firstName" class="p-inputtext-sm" />
                        </div>
                        <div class="flex flex-col gap-1">
                            <label class="text-xs font-bold">Last Name</label>
                            <InputText v-model="billAddr.lastName" class="p-inputtext-sm" />
                        </div>
                    </div>
                    <div class="flex flex-col gap-1">
                        <label class="text-xs font-bold">Address line 1</label>
                        <InputText v-model="billAddr.address1" class="p-inputtext-sm" />
                    </div>
                    <div class="flex flex-col gap-1">
                        <label class="text-xs font-bold">Address line 2</label>
                        <InputText v-model="billAddr.address2" class="p-inputtext-sm" />
                    </div>
                    <div class="grid grid-cols-2 gap-3">
                        <div class="flex flex-col gap-1">
                            <label class="text-xs font-bold">City</label>
                            <InputText v-model="billAddr.city" class="p-inputtext-sm" />
                        </div>
                        <div class="flex flex-col gap-1">
                            <label class="text-xs font-bold">Zip Code</label>
                            <InputText v-model="billAddr.zipCode" class="p-inputtext-sm" />
                        </div>
                    </div>
                    <div class="flex flex-col gap-1">
                        <label class="text-xs font-bold">Country Code (ISO)</label>
                        <InputText v-model="billAddr.countryCode" class="p-inputtext-sm" />
                    </div>
                </div>
                <div v-else class="flex items-center justify-center h-full border-2 border-dashed border-surface-100 dark:border-surface-800 rounded-3xl text-surface-400 italic text-sm text-center p-8">
                    Billing information will match the shipping details.
                </div>
            </div>
        </div>

        <template #footer>
            <div class="flex justify-end gap-3 pt-4 border-t border-surface-100 dark:border-surface-800">
                <Button label="Cancel" severity="secondary" text @click="$emit('close')" />
                <Button label="Update Addresses" icon="pi pi-check" @click="onSave" class="rounded-xl" />
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
