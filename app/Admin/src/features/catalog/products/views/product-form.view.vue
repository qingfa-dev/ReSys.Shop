<script setup lang="ts">
import { onMounted, computed, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useForm } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import { createProductSchema } from '../schemas/product.schemas';
import { useProductStore } from '../stores/product.store';
import { storeToRefs } from 'pinia';
import AppBreadcrumb from '@/shared/components/breadcrumb.component.vue';
import MetadataManager from '@/shared/components/metadata-manager.component.vue';
import ProductImageManager from '../components/ProductImageManager.vue';
import ProductVariantManager from '../components/ProductVariantManager.vue';
import ProductClassificationManager from '../components/ProductClassificationManager.vue';
import ProductPropertyManager from '../components/ProductPropertyManager.vue';
import ProductOptionTypeManager from '../components/ProductOptionTypeManager.vue';
import ProductInventoryManager from '../components/ProductInventoryManager.vue';
import { productLocales, type ProductLocales } from '../locales/product.locales';

const t = productLocales as ProductLocales;
const route = useRoute();
const router = useRouter();
const store = useProductStore();
const { submitting, current_product } = storeToRefs(store);

const isEdit = computed(() => route.name === 'catalog.products.edit');
const productId = computed(() => route.params.id as string);

const activeTab = ref(0);

const { defineField, handleSubmit, errors, setValues } = useForm({
    validationSchema: toTypedSchema(createProductSchema),
    initialValues: {
        name: '',
        slug: '',
        sku: '',
        price: 0,
        description: '',
        is_active: true,
        is_visible: true,
        weight: null,
        height: null,
        width: null,
        depth: null,
        brand: '',
        meta_title: '',
        meta_description: '',
        meta_keywords: '',
    }
});

const [name] = defineField('name');
const [slug] = defineField('slug');
const [sku] = defineField('sku');
const [price] = defineField('price');
const [description] = defineField('description');
const [is_active] = defineField('is_active');
const [is_visible] = defineField('is_visible');
const [weight] = defineField('weight');
const [height] = defineField('height');
const [width] = defineField('width');
const [depth] = defineField('depth');
const [brand] = defineField('brand');
const [meta_title] = defineField('meta_title');
const [meta_description] = defineField('meta_description');
const [meta_keywords] = defineField('meta_keywords');

const public_metadata = ref<Record<string, any>>({});
const private_metadata = ref<Record<string, any>>({});

// Auto-generate slug from name
const generateSlug = () => {
    if (!name.value || (isEdit.value && slug.value)) return;
    slug.value = name.value.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
};

onMounted(async () => {
    if (isEdit.value) {
        const result = await store.fetchProductById(productId.value);
        if (result.success && current_product.value) {
            setValues({
                name: current_product.value.name,
                slug: current_product.value.slug,
                sku: current_product.value.sku || '',
                price: current_product.value.price,
                description: current_product.value.description || '',
                is_active: current_product.value.is_active,
                is_visible: current_product.value.is_visible,
                weight: (current_product.value as any).weight,
                height: (current_product.value as any).height,
                width: (current_product.value as any).width,
                depth: (current_product.value as any).depth,
                brand: (current_product.value as any).brand || '',
                meta_title: (current_product.value as any).meta_title || '',
                meta_description: (current_product.value as any).meta_description || '',
                meta_keywords: (current_product.value as any).meta_keywords || '',
            });
            public_metadata.value = current_product.value.public_metadata || {};
            private_metadata.value = current_product.value.private_metadata || {};
        }
    } else {
        store.current_product = null;
    }
});

const onSubmit = handleSubmit(async (values) => {
    const payload = {
        ...values,
        public_metadata: public_metadata.value,
        private_metadata: private_metadata.value
    };

    if (isEdit.value) {
        const result = await store.updateProduct(productId.value, payload);
        if (result?.success) router.push({ name: 'catalog.products.list' });
    } else {
        const result = await store.createProduct(payload);
        if (result?.success) router.push({ name: 'catalog.products.list' });
    }
});
</script>

<template>
    <div class="p-6 max-w-6xl mx-auto">
        <AppBreadcrumb :locales="t" />
        
        <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mt-4 mb-8">
            <div class="flex items-center gap-4">
                <Button icon="pi pi-arrow-left" text rounded severity="secondary" @click="router.back()" class="bg-surface-100 dark:bg-surface-800" />
                <div>
                    <h2 class="text-4xl font-black tracking-tighter text-surface-900 dark:text-surface-50 m-0">
                        {{ isEdit ? (name || 'Edit Product') : t.titles?.create }}
                    </h2>
                    <p class="text-sm text-surface-500 m-0">
                        {{ isEdit ? t.descriptions?.edit : t.descriptions?.create }}
                    </p>
                </div>
            </div>
            <div class="flex items-center gap-3">
                <Button :label="t.actions?.cancel" severity="secondary" outlined @click="router.back()" class="rounded-xl px-6" />
                <Button :label="isEdit ? t.actions?.save : t.actions?.new" icon="pi pi-check" class="rounded-xl px-8 shadow-xl shadow-primary/20" :loading="submitting" @click="onSubmit" />
            </div>
        </div>

        <Card class="border-none shadow-sm rounded-3xl bg-surface-0 dark:bg-surface-900 overflow-hidden">
            <template #content>
                <Tabs v-model:value="activeTab">
                    <TabList scrollable>
                        <Tab :value="0">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-info-circle"></i>
                                <span>{{ t.tabs?.general }}</span>
                            </div>
                        </Tab>
                        <Tab :value="1" v-if="isEdit">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-images"></i>
                                <span>{{ t.tabs?.images }}</span>
                            </div>
                        </Tab>
                        <Tab :value="2" v-if="isEdit">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-list"></i>
                                <span>Options</span>
                            </div>
                        </Tab>
                        <Tab :value="3" v-if="isEdit">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-clone"></i>
                                <span>{{ t.tabs?.variants }}</span>
                            </div>
                        </Tab>
                        <Tab :value="4" v-if="isEdit">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-tags"></i>
                                <span>{{ t.tabs?.categories }}</span>
                            </div>
                        </Tab>
                        <Tab :value="8" v-if="isEdit">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-cog"></i>
                                <span>Specifications</span>
                            </div>
                        </Tab>
                        <Tab :value="5">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-box"></i>
                                <span>{{ t.tabs?.inventory }}</span>
                            </div>
                        </Tab>
                        <Tab :value="6">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-search"></i>
                                <span>{{ t.tabs?.seo }}</span>
                            </div>
                        </Tab>
                        <Tab :value="7">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-database"></i>
                                <span>{{ t.tabs?.metadata }}</span>
                            </div>
                        </Tab>
                    </TabList>

                    <TabPanels class="p-6">
                        <TabPanel :value="0">
                            <div class="flex flex-col gap-8">
                                <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t.labels?.name }}</label>
                                        <InputText v-model="name" class="w-full rounded-2xl h-12 px-4" :invalid="!!errors.name" @blur="generateSlug" />
                                        <small class="text-red-500 ml-1" v-if="errors.name">{{ errors.name }}</small>
                                    </div>
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t.labels?.slug }}</label>
                                        <InputText v-model="slug" class="w-full rounded-2xl h-12 px-4 font-mono text-sm" :invalid="!!errors.slug" />
                                        <small class="text-red-500 ml-1" v-if="errors.slug">{{ errors.slug }}</small>
                                    </div>
                                </div>

                                <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t.labels?.sku }}</label>
                                        <InputText v-model="sku" class="w-full rounded-2xl h-12 px-4 font-mono" :invalid="!!errors.sku" />
                                        <small class="text-red-500 ml-1" v-if="errors.sku">{{ errors.sku }}</small>
                                    </div>
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t.labels?.price }}</label>
                                        <InputNumber v-model="price" mode="currency" currency="USD" locale="en-US" class="w-full rounded-2xl h-12 overflow-hidden" inputClass="px-4" :invalid="!!errors.price" />
                                        <small class="text-red-500 ml-1" v-if="errors.price">{{ errors.price }}</small>
                                    </div>
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t.labels?.brand }}</label>
                                        <InputText v-model="brand" class="w-full rounded-2xl h-12 px-4" />
                                    </div>
                                </div>

                                <div class="flex flex-col gap-2">
                                    <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t.labels?.description }}</label>
                                    <Textarea v-model="description" rows="5" class="w-full rounded-2xl p-4" />
                                </div>

                                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                                    <div class="p-6 bg-surface-50 dark:bg-surface-800/50 rounded-3xl border border-surface-100 dark:border-surface-800 flex items-center justify-between">
                                        <div class="flex flex-col">
                                            <span class="font-bold text-surface-900 dark:text-surface-0">Active Status</span>
                                            <p class="text-xs text-surface-500 m-0">Visible in the storefront and buyable.</p>
                                        </div>
                                        <ToggleSwitch v-model="is_active" />
                                    </div>
                                    <div class="p-6 bg-surface-50 dark:bg-surface-800/50 rounded-3xl border border-surface-100 dark:border-surface-800 flex items-center justify-between">
                                        <div class="flex flex-col">
                                            <span class="font-bold text-surface-900 dark:text-surface-0">Searchable</span>
                                            <p class="text-xs text-surface-500 m-0">Can be found via global search.</p>
                                        </div>
                                        <ToggleSwitch v-model="is_visible" />
                                    </div>
                                </div>
                            </div>
                        </TabPanel>

                        <TabPanel :value="1" v-if="isEdit">
                            <ProductImageManager :productId="productId" />
                        </TabPanel>

                        <TabPanel :value="2" v-if="isEdit">
                            <ProductOptionTypeManager :productId="productId" />
                        </TabPanel>

                        <TabPanel :value="3" v-if="isEdit">
                            <ProductVariantManager :productId="productId" />
                        </TabPanel>

                        <TabPanel :value="4" v-if="isEdit">
                            <ProductClassificationManager :productId="productId" />
                        </TabPanel>

                        <TabPanel :value="8" v-if="isEdit">
                            <ProductPropertyManager :productId="productId" />
                        </TabPanel>

                        <TabPanel :value="5">
                            <div class="flex flex-col gap-8">
                                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase text-surface-500 ml-1">{{ t.labels?.weight }}</label>
                                        <InputNumber v-model="weight" mode="decimal" :minFractionDigits="2" class="w-full rounded-2xl h-12 overflow-hidden" inputClass="px-4" />
                                    </div>
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase text-surface-500 ml-1">{{ t.labels?.height }}</label>
                                        <InputNumber v-model="height" mode="decimal" class="w-full rounded-2xl h-12 overflow-hidden" inputClass="px-4" />
                                    </div>
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase text-surface-500 ml-1">{{ t.labels?.width }}</label>
                                        <InputNumber v-model="width" mode="decimal" class="w-full rounded-2xl h-12 overflow-hidden" inputClass="px-4" />
                                    </div>
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase text-surface-500 ml-1">{{ t.labels?.depth }}</label>
                                        <InputNumber v-model="depth" mode="decimal" class="w-full rounded-2xl h-12 overflow-hidden" inputClass="px-4" />
                                    </div>
                                </div>

                                <Divider />

                                <ProductInventoryManager :productId="productId" v-if="isEdit" />
                            </div>
                        </TabPanel>

                        <TabPanel :value="6">
                            <div class="flex flex-col gap-8 max-w-3xl">
                                <div class="flex flex-col gap-2">
                                    <label class="font-bold text-xs uppercase text-surface-500 ml-1">{{ t.labels?.meta_title }}</label>
                                    <InputText v-model="meta_title" class="w-full rounded-2xl h-12 px-4" />
                                </div>
                                <div class="flex flex-col gap-2">
                                    <label class="font-bold text-xs uppercase text-surface-500 ml-1">{{ t.labels?.meta_description }}</label>
                                    <Textarea v-model="meta_description" rows="3" class="w-full rounded-2xl p-4" />
                                </div>
                                <div class="flex flex-col gap-2">
                                    <label class="font-bold text-xs uppercase text-surface-500 ml-1">{{ t.labels?.meta_keywords }}</label>
                                    <InputText v-model="meta_keywords" class="w-full rounded-2xl h-12 px-4" />
                                </div>
                            </div>
                        </TabPanel>

                        <TabPanel :value="7">
                            <div class="flex flex-col gap-12">
                                <MetadataManager v-model="public_metadata" :title="t.labels?.public_metadata" />
                                <Divider />
                                <MetadataManager v-model="private_metadata" :title="t.labels?.private_metadata" />
                            </div>
                        </TabPanel>
                    </TabPanels>
                </Tabs>
            </template>
        </Card>
    </div>
</template>

<style scoped>
:deep(.p-tablist-tab-list) {
    padding: 0 0.5rem;
    border-bottom: 1px solid var(--p-surface-100);
}
.dark :deep(.p-tablist-tab-list) {
    border-bottom-color: var(--p-surface-800);
}
:deep(.p-tabpanel) {
    padding: 0;
}
</style>