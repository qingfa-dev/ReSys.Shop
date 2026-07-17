<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useProductStore } from '../stores/product.store';
import { useTaxonomyStore } from '@/features/catalog/taxonomies/stores/taxonomy.store';
import { storeToRefs } from 'pinia';
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use';

const props = defineProps<{
    productId: string;
}>();

const productStore = useProductStore();
const taxonomyStore = useTaxonomyStore();
const { current_classifications, loading } = storeToRefs(productStore);
const { taxonomies } = storeToRefs(taxonomyStore);
const { handleApiResult } = useApiErrorHandler();

onMounted(async () => {
    await productStore.fetchClassifications(props.productId);
    await taxonomyStore.fetchTaxonomies();
});

const onToggleTaxon = async (taxonId: string) => {
    const currentIds = current_classifications.value.map((c: any) => c.taxonId);
    const hasTaxon = currentIds.includes(taxonId);
    
    let newIds = [];
    if (hasTaxon) {
        newIds = currentIds.filter((id: string) => id !== taxonId);
    } else {
        newIds = [...currentIds, taxonId];
    }

    const result = (await productStore.updateClassifications(props.productId, {
        taxonIds: newIds,
        mainTaxonId: current_classifications.value.find((c: any) => c.isMain)?.taxonId
    }));
    handleApiResult(result);
};

const onSetMain = async (taxonId: string) => {
    const result = (await productStore.updateClassifications(props.productId, {
        taxonIds: current_classifications.value.map((c: any) => c.taxonId),
        mainTaxonId: taxonId
    }));
    handleApiResult(result);
};
</script>

<template>
    <div class="flex flex-col gap-4">
        <div v-if="loading" class="flex justify-center p-8">
            <ProgressSpinner />
        </div>
        <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div v-for="taxonomy in taxonomies" :key="taxonomy.id" class="border rounded-xl p-4">
                <h3 class="font-bold mb-4">{{ taxonomy.presentation || taxonomy.name }}</h3>
                <!-- Tree or flat list of taxons would go here -->
                <p class="text-xs text-surface-500 italic">Select categories from this taxonomy to classify the product.</p>
            </div>
        </div>
    </div>
</template>
