<script setup lang="ts">
import { ref } from 'vue';
import { useProductStore } from '@/features/catalog/products/stores/product.store';
import { useFormatter } from '@/shared/composables/formatter.use';
import type { AddOrderItemRequest } from '../types/Order.Request.Type';

const emit = defineEmits<{
    (e: 'save', data: AddOrderItemRequest): void;
    (e: 'close'): void;
}>();

const productStore = useProductStore();
const { formatCurrency } = useFormatter();

const quantity = ref(1);
const productsLoading = ref(false);
const productResults = ref<any[]>([]);
const selectedProduct = ref<any>(null);

// Variant Selection
const showVariantList = ref(false);
const currentProductVariants = ref<any[]>([]);

const onSearchProduct = async (event: { query: string }) => {
    productsLoading.value = true;
    try {
        const res = await productStore.fetchProducts({ search: event.query, pageSize: 5 });
        if (res.isSuccess) {
            const data = 'items' in res ? res.items : res.value;
            if (data) productResults.value = data;
        }
    } finally {
        productsLoading.value = false;
    }
};

const onProductSelect = async (product: any) => {
    productsLoading.value = true;
    try {
        await productStore.fetchProductById(product.id);
        if (productStore.current_product && (productStore.current_product as any).variants) {
             currentProductVariants.value = (productStore.current_product as any).variants;
             showVariantList.value = true;
        }
    } finally {
        productsLoading.value = false;
    }
};

const onSelectVariant = (variant: any) => {
    emit('save', {
        variantId: variant.id,
        quantity: quantity.value
    });
};
</script>

<template>
    <Dialog header="Add Item to Order" visible modal class="w-full max-w-xl" @update:visible="$emit('close')">
        <div class="flex flex-col gap-6 py-4">
            <div class="flex flex-col gap-2">
                <label class="font-bold text-sm">Search Product</label>
                <AutoComplete 
                    v-model="selectedProduct" 
                    :suggestions="productResults" 
                    @complete="onSearchProduct" 
                    @item-select="onProductSelect($event.value)"
                    placeholder="Search by name or SKU..."
                    optionLabel="name"
                    class="w-full"
                    inputClass="w-full h-12 px-4 rounded-xl"
                >
                    <template #option="slotProps">
                        <div class="flex justify-between items-center w-full">
                            <div class="flex flex-col">
                                <span class="font-bold">{{ slotProps.option.name }}</span>
                                <small class="text-surface-500">{{ slotProps.option.sku }}</small>
                            </div>
                            <span class="font-black text-primary">{{ formatCurrency(slotProps.option.price) }}</span>
                        </div>
                    </template>
                </AutoComplete>
            </div>

            <div class="flex flex-col gap-2">
                <label class="font-bold text-sm">Quantity</label>
                <InputNumber v-model="quantity" showButtons buttonLayout="horizontal" :min="1" class="w-full" inputClass="h-12 text-center" />
            </div>

            <div v-if="showVariantList" class="flex flex-col gap-3 animate-fade-in">
                <label class="font-black uppercase tracking-widest text-surface-400 text-xs mt-4">Select Variant</label>
                <div v-for="variant in currentProductVariants" :key="variant.id" 
                     class="flex justify-between items-center p-4 border border-surface-100 dark:border-surface-800 rounded-2xl cursor-pointer hover:bg-surface-50 dark:hover:bg-surface-800 transition-colors group"
                     @click="onSelectVariant(variant)"
                >
                    <div class="flex flex-col">
                        <span class="font-bold font-mono group-hover:text-primary transition-colors">{{ variant.sku }}</span>
                        <div class="flex gap-1 mt-1" v-if="variant.option_values">
                            <Tag v-for="opt in variant.option_values" :key="opt.id" :value="opt.value" severity="secondary" class="text-[10px]" />
                        </div>
                    </div>
                    <div class="flex items-center gap-4">
                        <span class="font-black text-lg">{{ formatCurrency(variant.price) }}</span>
                        <i class="pi pi-plus text-primary opacity-0 group-hover:opacity-100 transition-opacity"></i>
                    </div>
                </div>
            </div>
            <div v-else-if="productsLoading" class="flex justify-center py-8">
                <ProgressSpinner size="small" />
            </div>
        </div>

        <template #footer>
            <div class="flex justify-end pt-4 border-t border-surface-100 dark:border-surface-800">
                <Button label="Cancel" severity="secondary" text @click="$emit('close')" />
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
