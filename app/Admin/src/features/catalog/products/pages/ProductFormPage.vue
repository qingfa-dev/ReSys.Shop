<script setup lang="ts">
import { onMounted, computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useForm } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import { createCreateProductSchema } from '../types/create-product.field';
import { useProductStore } from '../store/product.store';
import { storeToRefs } from 'pinia';
import PageShell from '@/shared/components/navigation/PageShell.vue';
import PageHeader from '@/shared/components/navigation/PageHeader.vue';
import FormField from '@/shared/components/form/FormField.vue';
import MetadataManager from '@/shared/components/data-display/MetadataManager.vue';
import ProductImageManager from '../variants/components/ProductImageManager.vue';
import ProductVariantManager from '../variants/components/ProductVariantManager.vue';
import ProductClassificationManager from '../classifications/components/ProductClassificationManager.vue';
import ProductOptionTypeManager from '../option-types/components/ProductOptionTypeManager.vue';
import ProductInventoryManager from '../variants/components/ProductInventoryManager.vue';
import type { ProductDetailModel } from '../models/product.model';
import type { CreateProductRequest, UpdateProductRequest } from '../types/product.request';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const store = useProductStore();
const { submitting, current_product } = storeToRefs(store);

const isEdit = computed(() => route.name === 'catalog.products.edit');
const productId = computed(() => route.params.id as string);

const activeTab = ref(0);

const { defineField, handleSubmit, errors, setValues } = useForm({
    validationSchema: toTypedSchema(createCreateProductSchema(t)),
    initialValues: {
        name: '',
        slug: '',
        description: '',
        metaTitle: '',
        metaDescription: '',
        metaKeywords: '',
    }
});

const [name] = defineField('name');
const [slug] = defineField('slug');
const [description] = defineField('description');
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
            const p: ProductDetailModel = current_product.value;
            isActive.value = p.status === 1;
            isVisible.value = true;
            setValues({
                name: p.name,
                slug: p.slug,
                description: p.description || '',
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
        price: values.price ?? 0,
        trackInventory: true,
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
    <PageShell maxWidth="7xl">
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
                                    <FormField :label="t('catalog.products.labels.name')" name="name" :error="errors.name">
                                        <InputText v-model="name" class="w-full rounded-2xl h-12 px-4" :invalid="!!errors.name" @blur="generateSlug" />
                                    </FormField>
                                    <FormField :label="t('catalog.products.labels.slug')" name="slug" :error="errors.slug">
                                        <InputText v-model="slug" class="w-full rounded-2xl h-12 px-4 font-mono text-sm" :invalid="!!errors.slug" />
                                    </FormField>
                                </div>

                                <FormField :label="t('catalog.products.labels.description')" name="description">
                                    <Textarea v-model="description" rows="5" class="w-full rounded-2xl p-4" />
                                </FormField>

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
                                <Divider />

                                <ProductInventoryManager :productId="productId" v-if="isEdit" />
                            </div>
                        </TabPanel>

                        <TabPanel :value="6">
                            <div class="flex flex-col gap-8 max-w-3xl">
                                <FormField :label="t('catalog.products.labels.meta_title')" name="metaTitle">
                                    <InputText v-model="metaTitle" class="w-full rounded-2xl h-12 px-4" />
                                </FormField>
                                <FormField :label="t('catalog.products.labels.meta_description')" name="metaDescription">
                                    <Textarea v-model="metaDescription" rows="3" class="w-full rounded-2xl p-4" />
                                </FormField>
                                <FormField :label="t('catalog.products.labels.meta_keywords')" name="metaKeywords">
                                    <InputText v-model="metaKeywords" class="w-full rounded-2xl h-12 px-4" />
                                </FormField>
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
    </PageShell>
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
