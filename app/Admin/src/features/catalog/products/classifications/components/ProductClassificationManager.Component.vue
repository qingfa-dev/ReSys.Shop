<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useProductStore } from '../../stores/product.store';
import { useTaxonomyStore } from '@/features/catalog/taxonomies/stores/taxonomy.store';
import { storeToRefs } from 'pinia';
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use';
import type { ServerResult } from '@/shared/api/types/result.types';
import type { TreeNode } from 'primevue/tree';
import { useToast } from '@/shared/composables/toast.use';
import apiClient from '@/shared/api/http/api.client';
import type { ProductClassification } from '../../types/Product.Response.Type';

const props = defineProps<{
    productId: string;
}>();

const productStore = useProductStore();
const taxonomyStore = useTaxonomyStore();
const { current_classifications } = storeToRefs(productStore);
const { taxonomies } = storeToRefs(taxonomyStore);
const { handleApiResult } = useApiErrorHandler();
const { showToast } = useToast();
const { t } = useI18n();

const loading = ref(false);
const trees = ref<Record<string, TreeNode[]>>({});

const loadHierarchy = async () => {
    loading.value = true;
    try {
        await productStore.fetchClassifications(props.productId);
        await taxonomyStore.fetchTaxonomies({ pageSize: 100 });
        
        // Fetch tree for each taxonomy
        for (const tax of taxonomies.value) {
            const res = await apiClient.get(`catalog/taxonomies/${tax.id}/taxons/tree`);
            const result = res.data as ServerResult<{ id: string; presentation: string; children: unknown[] }[]>;
            if (result.isSuccess && result.value) {
                trees.value[tax.id] = result.value.map(mapNode);
            }
        }
    } finally {
        loading.value = false;
    }
};

interface RawTaxonNode { id: string; presentation: string; children?: RawTaxonNode[] }

const mapNode = (node: RawTaxonNode): TreeNode => {
    return {
        key: node.id,
        label: node.presentation,
        data: node,
        children: node.children?.map(mapNode) || []
    };
};

onMounted(async () => {
    await loadHierarchy();
});

const onToggleTaxon = async (taxonId: string) => {
    const currentIds = current_classifications.value.map((c: ProductClassification) => c.taxonId);
    const hasTaxon = currentIds.includes(taxonId);
    
    let newIds = [];
    if (hasTaxon) {
        newIds = currentIds.filter((id: string) => id !== taxonId);
    } else {
        newIds = [...currentIds, taxonId];
    }

    const result = (await productStore.updateClassifications(props.productId, {
        taxonIds: newIds,
        mainTaxonId: current_classifications.value.find((c: ProductClassification) => c.isMain)?.taxonId
    }));
    
    if (result.isSuccess) {
        showToast('success', t('common.updated'), t('catalog.products.messages.classifications_saved'));
    } else {
        handleApiResult(result);
    }
};

const onSetMain = async (taxonId: string) => {
    const result = (await productStore.updateClassifications(props.productId, {
        taxonIds: current_classifications.value.map((c: ProductClassification) => c.taxonId),
        mainTaxonId: taxonId
    }));
    
    if (result.isSuccess) {
        showToast('success', t('common.updated'), 'Main category updated');
    } else {
        handleApiResult(result);
    }
};

const isSelected = (taxonId: string) => current_classifications.value.some((c: ProductClassification) => c.taxonId === taxonId);
const isMain = (taxonId: string) => current_classifications.value.some((c: ProductClassification) => c.taxonId === taxonId && c.isMain);
</script>

<template>
    <div class="flex flex-col gap-6">
        <div>
            <h3 class="text-lg font-bold m-0">{{ t('catalog.products.titles.classifications') }}</h3>
            <p class="text-sm text-surface-500 m-0">Assign this product to hierarchical categories across multiple taxonomies.</p>
        </div>

        <div v-if="loading" class="flex justify-center py-20">
            <ProgressSpinner />
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div v-for="taxonomy in taxonomies" :key="taxonomy.id" class="bg-surface-50 dark:bg-surface-800/50 rounded-3xl border border-surface-100 dark:border-surface-800 flex flex-col overflow-hidden">
                <div class="p-4 bg-surface-0 dark:bg-surface-900 border-b border-surface-100 dark:border-surface-800 flex items-center justify-between">
                    <span class="font-black text-sm uppercase tracking-tighter">{{ taxonomy.presentation || taxonomy.name }}</span>
                    <Badge :value="taxonomy.taxonsCount" severity="secondary" />
                </div>
                
                <div class="p-4 overflow-y-auto max-h-[400px]">
                    <Tree :value="trees[taxonomy.id]" class="bg-transparent border-none p-0">
                        <template #default="{ node }">
                            <div class="flex items-center justify-between w-full p-1">
                                <div class="flex items-center gap-3">
                                    <Checkbox 
                                        :modelValue="isSelected(node.key)" 
                                        @update:modelValue="onToggleTaxon(node.key)"
                                        :binary="true" 
                                    />
                                    <span :class="{'font-bold text-primary': isSelected(node.key)}">{{ node.label }}</span>
                                </div>
                                <div v-if="isSelected(node.key)" class="flex items-center gap-2">
                                    <Tag v-if="isMain(node.key)" value="Main" severity="primary" rounded class="text-[9px] font-black uppercase" />
                                    <Button v-else icon="pi pi-star" text rounded size="small" severity="secondary" @click="onSetMain(node.key)" v-tooltip.left="'Set as Main'" />
                                </div>
                            </div>
                        </template>
                    </Tree>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
:deep(.p-tree-node-content) {
    padding: 0.25rem 0 !important;
    background: transparent !important;
}
:deep(.p-tree-node-children) {
    padding-left: 1.5rem;
    border-left: 1px dashed var(--p-surface-200);
    margin-left: 0.75rem;
}
</style>
