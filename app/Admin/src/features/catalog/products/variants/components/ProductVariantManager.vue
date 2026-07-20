<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useApiErrorHandler } from '@/common/composables/api-error-handler.use';
import { useToast } from '@/common/composables/toast.use';
import { useFormatter } from '@/common/composables/formatter.use';
import { useConfirm } from 'primevue/useconfirm';
import { variantRepository } from '../api/variant.api';
import VariantGenerationDialog from './VariantGenerationDialog.vue';
import VariantFormDialog from './VariantFormDialog.vue';
import type { VariantSummaryModel, VariantDetailModel } from '../models/variant.model';
import type { CreateVariantRequest } from '../types/variant.request';
import type { ServerResult } from '@/common/api/types/result.types';

const { t } = useI18n();

const props = defineProps<{
    productId: string;
}>();

const { handleApiResult } = useApiErrorHandler();
const { showToast } = useToast();
const { formatCurrency } = useFormatter();
const confirm = useConfirm();

const variants = ref<VariantSummaryModel[]>([]);
const loading = ref(false);
const showGenerator = ref(false);
const showForm = ref(false);
const selectedVariant = ref<VariantDetailModel | null>(null);

const loadVariants = async () => {
    loading.value = true;
    try {
        const result = await variantRepository.listByProductId(props.productId);
        if (result.isSuccess && result.value) {
            variants.value = result.value || [];
        }
    } finally {
        loading.value = false;
    }
};

const openCreate = () => {
    selectedVariant.value = null;
    showForm.value = true;
};

const openEdit = async (variant: VariantSummaryModel) => {
    loading.value = true;
    try {
        const result = await variantRepository.getById(variant.id);
        if (result.isSuccess && result.value) {
            selectedVariant.value = result.value;
            showForm.value = true;
        }
    } finally {
        loading.value = false;
    }
};

const onSaveVariant = async (data: CreateVariantRequest) => {
    try {
        let result: ServerResult<VariantDetailModel>;
        if (selectedVariant.value) {
            result = await variantRepository.update(selectedVariant.value.id, data);
        } else {
            result = await variantRepository.create(props.productId, data);
        }

        if (handleApiResult(result)) {
            showToast('success', t('common.success'), selectedVariant.value ? t('catalog.products.variants.messages.update_success') : t('catalog.products.variants.messages.create_success'));
            showForm.value = false;
            await loadVariants();
        }
    } catch (e) {
        showToast('error', t('common.error'), t('catalog.products.variants.messages.save_failed'));
    }
};

const onDelete = (variant: VariantSummaryModel) => {
    confirm.require({
        message: t('catalog.products.confirm.delete_message').replace('{name}', variant.sku ?? ''),
        header: t('catalog.products.confirm.delete_header'),
        icon: 'pi pi-exclamation-triangle',
        acceptClass: 'p-button-danger',
        accept: async () => {
            try {
                const result = await variantRepository.delete(variant.id);
                if (handleApiResult(result)) {
                    showToast('success', t('common.deleted'), t('catalog.products.variants.messages.delete_success'));
                    await loadVariants();
                }
            } catch (e) {
                showToast('error', t('common.error'), t('catalog.products.variants.messages.delete_failed'));
            }
        }
    });
};

onMounted(() => {
    loadVariants();
});
</script>

<template>
    <div class="flex flex-col gap-6">
        <div class="flex items-center justify-between">
            <div>
                <h3 class="text-lg font-bold m-0">{{ t('catalog.products.variants.sku_variants') }}</h3>
                <p class="text-sm text-surface-500 m-0">{{ t('catalog.products.variants.sku_desc') }}</p>
            </div>
            <div class="flex gap-2">
                <Button :label="t('catalog.products.variants.generate')" icon="pi pi-bolt" outlined severity="warn" class="rounded-xl" @click="showGenerator = true" />
                <Button :label="t('catalog.products.actions.new')" icon="pi pi-plus" class="rounded-xl" @click="openCreate" />
            </div>
        </div>

        <div class="overflow-hidden border border-surface-100 dark:border-surface-800 rounded-2xl bg-surface-0 dark:bg-surface-900 shadow-sm">
            <DataTable :value="variants" :loading="loading" class="p-datatable-sm" rowHover>
                <template #empty>
                    <div class="py-12 text-center text-surface-400 italic">{{ t('catalog.products.variants.empty') }}</div>
                </template>

                <Column field="sku" :header="t('catalog.products.table.sku')">
                    <template #body="{ data }">
                        <div class="flex items-center gap-2">
                            <span class="font-mono text-xs font-bold">{{ data.sku }}</span>
                            <Tag v-if="data.isMaster" value="Master" severity="primary" class="text-[8px]" />
                        </div>
                    </template>
                </Column>
                <Column field="price" :header="t('catalog.products.table.price')" class="text-right">
                    <template #body="{ data }">
                        <span class="font-black">{{ data.priceDisplay }}</span>
                    </template>
                </Column>
                <Column class="w-32 text-right">
                    <template #body="{ data }">
                        <div class="flex justify-end gap-1">
                            <Button icon="pi pi-pencil" text rounded size="small" severity="secondary" @click="openEdit(data)" />
                            <Button v-if="!data.isMaster" icon="pi pi-trash" text rounded size="small" severity="danger" @click="onDelete(data)" />
                        </div>
                    </template>
                </Column>
            </DataTable>
        </div>

        <VariantGenerationDialog 
            v-model:visible="showGenerator" 
            :productId="productId" 
            @generated="loadVariants" 
        />

        <VariantFormDialog
            v-model="showForm"
            :variant="selectedVariant"
            :productId="productId"
            @save="onSaveVariant"
        />
    </div>
</template>
