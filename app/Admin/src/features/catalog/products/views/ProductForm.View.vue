<script setup lang="ts">
import { onMounted, computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useForm } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import { CreateProductSchema } from '../schemas/CreateProduct.Schema';
import { useProductStore } from '../stores/product.store';
import { storeToRefs } from 'pinia';
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
import PageHeader from '@/shared/components/PageHeader.Component.vue';
import MetadataManager from '@/shared/components/MetadataManager.Component.vue';
import ProductImageManager from '../components/ProductImageManager.Component.vue';
import ProductVariantManager from '../components/ProductVariantManager.Component.vue';
import ProductClassificationManager from '../components/ProductClassificationManager.Component.vue';
import ProductOptionTypeManager from '../components/ProductOptionTypeManager.Component.vue';
import ProductInventoryManager from '../components/ProductInventoryManager.Component.vue';
import type { ProductDetail } from '../types/Product.Response.Type';
import type { CreateProductRequest, UpdateProductRequest } from '../types/Product.Request.Type';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const store = useProductStore();
const { submitting, current_product } = storeToRefs(store);

const isEdit = computed(() => route.name === 'catalog.products.edit');
const productId = computed(() => route.params.id as string);

const activeTab = ref(0);

const { defineField, handleSubmit, errors, setValues } = useForm({
    validationSchema: toTypedSchema(CreateProductSchema),
    initialValues: {
        name: '',
        slug: '',
        sku: '',
        price: 0,
        description: '',
        weight: null,
        height: null,
        width: null,
        depth: null,
        metaTitle: '',
        metaDescription: '',
        metaKeywords: '',
    }
});

const [name] = defineField('name');
const [slug] = defineField('slug');
const [sku] = defineField('sku');
const [price] = defineField('price');
const [description] = defineField('description');
const [weight] = defineField('weight');
const [height] = defineField('height');
const [width] = defineField('width');
const [depth] = defineField('depth');
const [metaTitle] = defineField('metaTitle');
const [metaDescription] = defineField('metaDescription');
const [metaKeywords] = defineField('metaKeywords');

const isActive = ref(true);
const isVisible = ref(true);
const public_metadata = ref<Record<string, string>>({});
const private_metadata = ref<Record<string, string>>({});

const generateSlug = () => {
    if (!name.value || (isEdit.value && slug.value)) return;
    slug.value = name.value.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
};

onMounted(async () => {
    if (isEdit.value) {
        const result = await store.fetchProductById(productId.value);
        if (result.isSuccess && current_product.value) {
            const p: ProductDetail = current_product.value;
            isActive.value = p.status === 'Active';
            isVisible.value = true;
            setValues({
                name: p.name,
                slug: p.slug,
                sku: p.sku || '',
                price: p.price,
                description: p.description || '',
                weight: p.weight,
                height: p.height,
                width: p.width,
                depth: p.depth,
                metaTitle: p.metaTitle || '',
                metaDescription: p.metaDescription || '',
                metaKeywords: p.metaKeywords || '',
            });
        }
    } else {
        store.current_product = null;
    }
});

const onSubmit = handleSubmit(async (values) => {
    const payload: CreateProductRequest = {
        name: values.name,
        slug: values.slug,
        description: values.description,
        price: values.price,
        sku: values.sku,
        trackInventory: true,
        weight: values.weight,
        height: values.height,
        width: values.width,
        depth: values.depth,
    };

    if (isEdit.value) {
        const result = await store.updateProduct(productId.value, payload);
        if (result?.isSuccess) router.push({ name: 'catalog.products.list' });
    } else {
        const result = await store.createProduct(payload);
        if (result?.isSuccess) router.push({ name: 'catalog.products.list' });
    }
});
</script>

<template>
    <div class="p-6 max-w-6xl mx-auto">
        <AppBreadcrumb />
        <PageHeader
          :title="isEdit ? (name || 'Edit Product') : t('catalog.products.titles.create')"
          :description="isEdit ? t('catalog.products.descriptions.edit') : t('catalog.products.descriptions.create')"
          back
        >
          <template #actions>
            <Button :label="t('catalog.products.actions.cancel')" severity="secondary" outlined @click="router.back()" class="rounded-xl px-6" />
            <Button :label="isEdit ? t('catalog.products.actions.save') : t('catalog.products.actions.new')" icon="pi pi-check" class="rounded-xl px-8" :loading="submitting" @click="onSubmit" />
          </template>
        </PageHeader>

        <Card class="border-none shadow-sm rounded-3xl bg-surface-0 dark:bg-surface-900 overflow-hidden">
            <template #content>
                <Tabs v-model:value="activeTab">
                    <TabList scrollable>
                        <Tab :value="0">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-info-circle"></i>
                                <span>{{ t('catalog.products.tabs.general') }}</span>
                            </div>
                        </Tab>
                        <Tab :value="1" v-if="isEdit">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-images"></i>
                                <span>{{ t('catalog.products.tabs.images') }}</span>
                            </div>
                        </Tab>
                        <Tab :value="2" v-if="isEdit">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-list"></i>
                                <span>{{ t('catalog.products.tabs.options') }}</span>
                            </div>
                        </Tab>
                        <Tab :value="3" v-if="isEdit">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-clone"></i>
                                <span>{{ t('catalog.products.tabs.variants') }}</span>
                            </div>
                        </Tab>
                        <Tab :value="4" v-if="isEdit">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-tags"></i>
                                <span>{{ t('catalog.products.tabs.categories') }}</span>
                            </div>
                        </Tab>
                        <Tab :value="8" v-if="isEdit">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-cog"></i>
                                <span>{{ t('catalog.products.tabs.specifications') }}</span>
                            </div>
                        </Tab>
                        <Tab :value="5">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-box"></i>
                                <span>{{ t('catalog.products.tabs.inventory') }}</span>
                            </div>
                        </Tab>
                        <Tab :value="6">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-search"></i>
                                <span>{{ t('catalog.products.tabs.seo') }}</span>
                            </div>
                        </Tab>
                        <Tab :value="7">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-database"></i>
                                <span>{{ t('catalog.products.tabs.metadata') }}</span>
                            </div>
                        </Tab>
                    </TabList>

                    <TabPanels class="p-6">
                        <TabPanel :value="0">
                            <div class="flex flex-col gap-8">
                                <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t('catalog.products.labels.name') }}</label>
                                        <InputText v-model="name" class="w-full rounded-2xl h-12 px-4" :invalid="!!errors.name" @blur="generateSlug" />
                                        <small class="p-error" v-if="errors.name">{{ errors.name }}</small>
                                    </div>
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t('catalog.products.labels.slug') }}</label>
                                        <InputText v-model="slug" class="w-full rounded-2xl h-12 px-4 font-mono text-sm" :invalid="!!errors.slug" />
                                        <small class="p-error" v-if="errors.slug">{{ errors.slug }}</small>
                                    </div>
                                </div>

                                <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t('catalog.products.labels.sku') }}</label>
                                        <InputText v-model="sku" class="w-full rounded-2xl h-12 px-4 font-mono" :invalid="!!errors.sku" />
                                        <small class="p-error" v-if="errors.sku">{{ errors.sku }}</small>
                                    </div>
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t('catalog.products.labels.price') }}</label>
                                        <InputNumber v-model="price" mode="currency" currency="USD" locale="en-US" class="w-full rounded-2xl h-12 overflow-hidden" inputClass="px-4" :invalid="!!errors.price" />
                                        <small class="p-error" v-if="errors.price">{{ errors.price }}</small>
                                    </div>
                                </div>

                                <div class="flex flex-col gap-2">
                                    <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t('catalog.products.labels.description') }}</label>
                                    <Textarea v-model="description" rows="5" class="w-full rounded-2xl p-4" />
                                </div>

                                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                                    <div class="p-6 bg-surface-50 dark:bg-surface-800/50 rounded-3xl border border-surface-100 dark:border-surface-800 flex items-center justify-between">
                                        <div class="flex flex-col">
                                            <span class="font-bold text-surface-900 dark:text-surface-0">Active Status</span>
                                            <p class="text-xs text-surface-500 m-0">Visible in the storefront and buyable.</p>
                                        </div>
                                        <ToggleSwitch v-model="isActive" />
                                    </div>
                                    <div class="p-6 bg-surface-50 dark:bg-surface-800/50 rounded-3xl border border-surface-100 dark:border-surface-800 flex items-center justify-between">
                                        <div class="flex flex-col">
                                            <span class="font-bold text-surface-900 dark:text-surface-0">{{ t('catalog.products.labels.searchable') }}</span>
                                            <p class="text-xs text-surface-500 m-0">Can be found via global search.</p>
                                        </div>
                                        <ToggleSwitch v-model="isVisible" />
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

                        <TabPanel :value="5">
                            <div class="flex flex-col gap-8">
                                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase text-surface-500 ml-1">{{ t('catalog.products.labels.weight') }}</label>
                                        <InputNumber v-model="weight" mode="decimal" :minFractionDigits="2" class="w-full rounded-2xl h-12 overflow-hidden" inputClass="px-4" />
                                    </div>
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase text-surface-500 ml-1">{{ t('catalog.products.labels.height') }}</label>
                                        <InputNumber v-model="height" mode="decimal" class="w-full rounded-2xl h-12 overflow-hidden" inputClass="px-4" />
                                    </div>
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase text-surface-500 ml-1">{{ t('catalog.products.labels.width') }}</label>
                                        <InputNumber v-model="width" mode="decimal" class="w-full rounded-2xl h-12 overflow-hidden" inputClass="px-4" />
                                    </div>
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase text-surface-500 ml-1">{{ t('catalog.products.labels.depth') }}</label>
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
                                    <label class="font-bold text-xs uppercase text-surface-500 ml-1">{{ t('catalog.products.labels.meta_title') }}</label>
                                    <InputText v-model="metaTitle" class="w-full rounded-2xl h-12 px-4" />
                                </div>
                                <div class="flex flex-col gap-2">
                                    <label class="font-bold text-xs uppercase text-surface-500 ml-1">{{ t('catalog.products.labels.meta_description') }}</label>
                                    <Textarea v-model="metaDescription" rows="3" class="w-full rounded-2xl p-4" />
                                </div>
                                <div class="flex flex-col gap-2">
                                    <label class="font-bold text-xs uppercase text-surface-500 ml-1">{{ t('catalog.products.labels.meta_keywords') }}</label>
                                    <InputText v-model="metaKeywords" class="w-full rounded-2xl h-12 px-4" />
                                </div>
                            </div>
                        </TabPanel>

                        <TabPanel :value="7">
                            <div class="flex flex-col gap-12">
                                <MetadataManager v-model="public_metadata" :title="t('catalog.products.labels.public_metadata')" />
                                <Divider />
                                <MetadataManager v-model="private_metadata" :title="t('catalog.products.labels.private_metadata')" />
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
:deep(.p-tabpanel) {
    padding: 0;
}
</style>
