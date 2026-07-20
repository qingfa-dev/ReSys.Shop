<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useOptionTypeStore } from '@/features/catalog/option-types/store/option-type.store';
import { useApiErrorHandler } from '@/common/composables/api-error-handler.use';
import { useToast } from '@/common/composables/toast.use';
import { productRepository } from '../../api/product.api';
import FormField from '@/shared/components/form/FormField.vue';

const props = defineProps<{
    productId: string;
}>();

const optionTypeStore = useOptionTypeStore();
const { handleApiResult } = useApiErrorHandler();
const { showToast } = useToast();
const { t } = useI18n();

const availableOptionTypes = ref<{ label: string; value: string }[]>([]);
const selectedOptionTypes = ref<string[]>([]);
const loading = ref(false);
const saving = ref(false);

const loadData = async () => {
    loading.value = true;
    try {
        // 1. Get all available option types
        await optionTypeStore.fetchList({ pageSize: 100 });
        availableOptionTypes.value = optionTypeStore.items.map(ot => ({
            label: `${ot.presentation} (${ot.name})`,
            value: ot.id
        }));

        // 2. Get currently assigned option types for this product
        const result = await productRepository.getOptionTypes(props.productId);
        if (result.isSuccess && result.value) {
            selectedOptionTypes.value = result.value.map((ot: { id: string }) => ot.id);
        }
    } finally {
        loading.value = false;
    }
};

const onSave = async () => {
    saving.value = true;
    try {
        const result = await productRepository.updateOptionTypes(props.productId, selectedOptionTypes.value);

        if (handleApiResult(result)) {
            showToast('success', t('common.updated'), t('catalog.products.option_types.messages.update_success'));
        }
    } finally {
        saving.value = false;
    }
};

onMounted(() => {
    loadData();
});
</script>

<template>
    <div class="flex flex-col gap-6">
        <div>
            <h3 class="text-lg font-bold m-0">Option Types</h3>
            <p class="text-sm text-surface-500 m-0">Select the attributes (e.g. Size, Color) that this product varies by.</p>
        </div>

        <div v-if="loading" class="flex justify-center py-12">
            <ProgressSpinner style="width: 40px; height: 40px" />
        </div>

        <div v-else class="flex flex-col gap-4">
            <div class="p-4 border border-surface-200 dark:border-surface-700 rounded-xl bg-surface-50 dark:bg-surface-800/50">
                <FormField label="Assigned Options" name="assignedOptions">
                    <MultiSelect 
                        v-model="selectedOptionTypes" 
                        :options="availableOptionTypes" 
                        optionLabel="label" 
                        optionValue="value" 
                        display="chip" 
                        filter
                        placeholder="Select option types..." 
                        class="w-full"
                    />
                </FormField>
            </div>

            <div class="flex justify-end">
                <Button label="Save Changes" icon="pi pi-check" @click="onSave" :loading="saving" class="rounded-xl px-6" />
            </div>
        </div>
    </div>
</template>
