<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use';
import { useToast } from '@/shared/composables/toast.use';
import { useFormatter } from '@/shared/composables/formatter.use';
import { useConfirm } from 'primevue/useconfirm';
import { variantService } from '../services/variant.service';
import VariantGenerationDialog from './dialogs/VariantGenerationDialog.Component.vue';
import VariantFormDialog from './VariantFormDialog.Component.vue';
import type { VariantSummary, VariantDetail } from '../types/Variant.Response.Type';
import type { CreateVariantRequest } from '../types/Variant.Request.Type';
import type { ServerResult } from '@/shared/api/types/result.types';

const { t } = useI18n();

const props = defineProps<{
    productId: string;
}>();

const { handleApiResult } = useApiErrorHandler();
const { showToast } = useToast();
const { formatCurrency } = useFormatter();
const confirm = useConfirm();

const variants = ref<VariantSummary[]>([]);
const loading = ref(false);
const showGenerator = ref(false);
const showForm = ref(false);
const selectedVariant = ref<VariantDetail | null>(null);

const loadVariants = async () => {
    loading.value = true;
    try {
        const result = await variantService.listByProductId(props.productId);
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

const openEdit = async (variant: VariantSummary) => {
    loading.value = true;
    try {
        const result = await variantService.getById(variant.id);
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
        let result: ServerResult<VariantDetail>;
        if (selectedVariant.value) {
            result = await variantService.update(selectedVariant.value.id, data);
        } else {
            result = await variantService.create(props.productId, data);
        }

        if (handleApiResult(result)) {
            showToast('success', 'Success', `Variant ${selectedVariant.value ? 'updated' : 'created'} successfully`);
            showForm.value = false;
            await loadVariants();
        }
    } catch (e) {
        showToast('error', 'Error', 'Failed to save variant');
    }
};

const onDelete = (variant: VariantSummary) => {
    confirm.require({
        message: (t('catalog.products.confirm.delete_message') || '').replace('{name}', variant.sku ?? ''),
        header: t('catalog.products.confirm.delete_header'),
        icon: 'pi pi-exclamation-triangle',
        acceptClass: 'p-button-danger',
        accept: async () => {
            try {
                const result = await variantService.delete(variant.id);
                if (handleApiResult(result)) {
                    showToast('success', 'Deleted', 'Variant removed');
                    await loadVariants();
                }
            } catch (e) {
                showToast('error', 'Error', 'Failed to delete variant');
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
                <Column header="Options">
                    <template #body="{ data }">
                        <div class="flex gap-1 flex-wrap">
                            <Tag v-for="(opt, idx) in data.options || []" :key="idx" :value="`${opt.name}: ${opt.value}`" severity="secondary" class="text-[10px]" />
                        </div>
                    </template>
                </Column>
                <Column field="price" :header="t('catalog.products.table.price')" class="text-right">
                    <template #body="{ data }">
                        <span class="font-black">{{ formatCurrency(data.price) }}</span>
                    </template>
                </Column>
                <Column field="status" :header="t('catalog.products.table.status')" class="text-center w-24">
                    <template #body="{ data }">
                        <Tag :value="data.status || 'Active'" :severity="(data.status || 'Active') === 'Active' ? 'success' : 'secondary'" rounded class="text-[10px]" />
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
