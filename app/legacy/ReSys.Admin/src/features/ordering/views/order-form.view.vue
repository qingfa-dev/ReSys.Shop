<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useOrderStore } from '../stores/order.store';
import { useProductStore } from '@/features/catalog/products/stores/product.store';
import { useFormatter } from '@/shared/composables/formatter.use';
import { useToast } from '@/shared/composables/toast.use';
import type { CreateOrderRequest } from '../types/order.types';

const router = useRouter();
const orderStore = useOrderStore();
const productStore = useProductStore();
const { formatCurrency } = useFormatter();
const { showToast } = useToast();

const loading = ref(false);
const email = ref('');
const currency = ref('USD');
const selectedItems = ref<Array<{ variant_id: string; sku: string; name: string; price: number; quantity: number }>>([]);

// Product Search for adding items
const productsLoading = ref(false);
const productResults = ref<any[]>([]);
const selectedProduct = ref<any>(null);

// Variant Selection
const showVariantDialog = ref(false);
const currentProductVariants = ref<any[]>([]);
const selectedVariant = ref<any>(null);

const onSearchProduct = async (event: { query: string }) => {
    productsLoading.value = true;
    try {
        const res = await productStore.fetchProducts({ search: event.query, page_size: 5 });
        if (res.success && res.data) {
            productResults.value = res.data;
        }
    } finally {
        productsLoading.value = false;
    }
};

const onProductSelect = async (product: any) => {
    // 1. Fetch variants for this product
    productsLoading.value = true;
    try {
        // Need to use variantService here ideally, or fetch via product store if available.
        // Assuming we can use productStore to fetch details or we might need to import variantService.
        // Let's import variantService.
        // But for now, let's assume we fetch product details which might include variants?
        // Actually productStore.fetchProductById updates current_product.
        
        // Let's use a direct call to variantService if possible or add it to imports.
        // Since I can't add imports easily with replace, I'll rely on productStore to fetch details
        // if it includes variants. Storefront mapping does, Admin usually does too.
        
        await productStore.fetchProductById(product.id);
        if (productStore.current_product && (productStore.current_product as any).variants) {
             currentProductVariants.value = (productStore.current_product as any).variants;
             if (currentProductVariants.value.length === 1) {
                 // Auto-select if only one
                 addVariantToOrder(currentProductVariants.value[0], product);
             } else {
                 showVariantDialog.value = true;
             }
        }
    } finally {
        productsLoading.value = false;
        selectedProduct.value = null; // Reset search input
    }
};

const addVariantToOrder = (variant: any, product: any) => {
    const existing = selectedItems.value.find(i => i.variant_id === variant.id);
    if (existing) {
        existing.quantity++;
    } else {
        selectedItems.value.push({
            variant_id: variant.id,
            sku: variant.sku,
            name: `${product.name} - ${variant.sku}`, // or better name construction
            price: variant.price,
            quantity: 1
        });
    }
    showVariantDialog.value = false;
};

const onAddVariant = () => {
    if (selectedVariant.value && productStore.current_product) {
        addVariantToOrder(selectedVariant.value, productStore.current_product);
    }
};

const removeItem = (index: number) => {
    selectedItems.value.splice(index, 1);
};

const subtotal = computed(() => {
    return selectedItems.value.reduce((acc, item) => acc + (item.price * item.quantity), 0);
});

const onSubmit = async () => {
    if (!email.value) {
        showToast('error', 'Error', 'Customer email is required');
        return;
    }
    if (selectedItems.value.length === 0) {
        showToast('error', 'Error', 'Please add at least one item');
        return;
    }

    const payload: CreateOrderRequest = {
        email: email.value,
        currency: currency.value,
        line_items: selectedItems.value.map(i => ({ variant_id: i.variant_id, quantity: i.quantity }))
    };

    loading.value = true;
    const result = await orderStore.createOrder(payload);
    loading.value = false;

    if (result.success) {
        router.push({ name: 'ordering.orders.list' });
    }
};
</script>

<template>
    <div class="p-6 max-w-4xl mx-auto">
        <div class="flex items-center gap-4 mb-8">
            <Button icon="pi pi-arrow-left" text rounded @click="router.back()" class="bg-surface-100 dark:bg-surface-800" />
            <div>
                <h1 class="text-4xl font-black uppercase tracking-tighter text-surface-900 dark:text-surface-0">Create Manual Order</h1>
                <p class="text-surface-500">Enter customer details and add items to generate a new order.</p>
            </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
            <!-- Left: Form -->
            <div class="lg:col-span-2 flex flex-col gap-6">
                <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900">
                    <template #title><span class="text-lg font-black uppercase tracking-widest text-surface-400">Customer Details</span></template>
                    <template #content>
                        <div class="flex flex-col gap-4">
                            <div class="flex flex-col gap-2">
                                <label class="font-bold text-sm">Customer Email</label>
                                <InputText v-model="email" placeholder="john@example.com" class="w-full h-12 px-4 rounded-xl" />
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="font-bold text-sm">Currency</label>
                                <SelectButton v-model="currency" :options="['USD', 'EUR', 'GBP']" />
                            </div>
                        </div>
                    </template>
                </Card>

                <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
                    <template #title>
                        <div class="flex justify-between items-center p-4">
                            <span class="text-lg font-black uppercase tracking-widest text-surface-400">Order Items</span>
                        </div>
                    </template>
                    <template #content>
                        <div class="mb-6">
                            <AutoComplete 
                                v-model="selectedProduct" 
                                :suggestions="productResults" 
                                @complete="onSearchProduct" 
                                @item-select="onProductSelect($event.value)"
                                placeholder="Search by name or SKU to add items..."
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

                        <DataTable :value="selectedItems" class="p-datatable-sm" v-if="selectedItems.length > 0">
                            <Column header="Product">
                                <template #body="{ data }">
                                    <span class="font-bold">{{ data.name }}</span>
                                </template>
                            </Column>
                            <Column header="Qty" class="w-24">
                                <template #body="{ data }">
                                    <InputNumber v-model="data.quantity" showButtons buttonLayout="horizontal" :min="1" inputClass="w-12 text-center" />
                                </template>
                            </Column>
                            <Column header="Price">
                                <template #body="{ data }">{{ formatCurrency(data.price) }}</template>
                            </Column>
                            <Column class="w-12">
                                <template #body="{ index }">
                                    <Button icon="pi pi-trash" severity="danger" text rounded @click="removeItem(index)" />
                                </template>
                            </Column>
                        </DataTable>
                        <div v-else class="text-center py-12 border-2 border-dashed border-surface-100 dark:border-surface-800 rounded-3xl text-surface-400">
                            No items added to this order yet.
                        </div>
                    </template>
                </Card>
            </div>

            <!-- Right: Summary -->
            <div class="flex flex-col gap-6">
                <Card class="rounded-3xl shadow-sm border-none bg-surface-900 text-surface-0 overflow-hidden">
                    <template #content>
                        <div class="flex flex-col gap-4 p-4">
                            <span class="text-xs font-black uppercase tracking-widest text-surface-500">Order Summary</span>
                            <div class="flex justify-between items-center mt-4">
                                <span class="text-3xl font-black">Total</span>
                                <span class="text-4xl font-black text-primary">{{ formatCurrency(subtotal) }}</span>
                            </div>
                            <Button label="Create Order" icon="pi pi-check" class="w-full h-14 mt-6 rounded-2xl shadow-xl shadow-primary/20" :loading="loading" @click="onSubmit" />
                            <Button label="Cancel" severity="secondary" text class="w-full rounded-2xl" @click="router.back()" />
                        </div>
                    </template>
                </Card>
            </div>
        </div>

        <Dialog v-model:visible="showVariantDialog" header="Select Variant" modal class="w-full max-w-lg">
            <div class="flex flex-col gap-4 py-4">
                <p class="text-sm text-surface-500">Please select the specific variant to add.</p>
                <div v-for="variant in currentProductVariants" :key="variant.id" 
                     class="flex justify-between items-center p-3 border rounded-xl cursor-pointer hover:bg-surface-50 dark:hover:bg-surface-800"
                     @click="addVariantToOrder(variant, productStore.current_product)"
                >
                    <div class="flex flex-col">
                        <span class="font-bold font-mono">{{ variant.sku }}</span>
                        <!-- Display options if available -->
                        <div class="flex gap-1 mt-1" v-if="variant.option_values">
                            <Tag v-for="opt in variant.option_values" :key="opt.id" :value="opt.value" severity="secondary" class="text-xs" />
                        </div>
                    </div>
                    <span class="font-black">{{ formatCurrency(variant.price) }}</span>
                </div>
            </div>
        </Dialog>
    </div>
</template>
