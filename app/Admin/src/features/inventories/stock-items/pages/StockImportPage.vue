<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import { stockRepository } from '../api/stock.api';
import PageShell from '@/shared/components/navigation/PageShell.vue';
import PageHeader from '@/shared/components/navigation/PageHeader.vue';

const { t } = useI18n();
const toast = useToast();

const selectedFile = ref<File | null>(null);
const uploading = ref(false);

const onFileSelect = (event: { files: File[] }) => {
    selectedFile.value = event.files[0] || null;
};

const onFileRemove = () => {
    selectedFile.value = null;
};

const handleImport = async () => {
    if (!selectedFile.value) return;
    uploading.value = true;
    try {
        const result = await stockRepository.importStockItems(selectedFile.value);
        if (result.isSuccess) {
            toast.add({ severity: 'success', summary: t('common.success'), detail: t('inventory.messages.import_success'), life: 3000 });
            selectedFile.value = null;
        } else {
            toast.add({ severity: 'error', summary: t('common.error'), detail: result.errors?.join(', ') || t('common.unknown_error'), life: 5000 });
        }
    } catch {
        toast.add({ severity: 'error', summary: t('common.error'), detail: t('common.unknown_error'), life: 5000 });
    } finally {
        uploading.value = false;
    }
};
</script>

<template>
    <PageShell maxWidth="2xl">
        <PageHeader :title="t('inventory.titles.import_stock')" />
        <div class="card p-8">
            <p class="mb-6 text-sm text-surface-500">
                {{ t('inventory.messages.import_stock_csv_info') }}
            </p>
            <FileUpload
                mode="basic"
                accept=".csv"
                :auto="false"
                :disabled="uploading"
                :chooseLabel="t('inventory.actions.choose_file')"
                @select="onFileSelect"
                @remove="onFileRemove"
            />
            <p v-if="selectedFile" class="mt-4 text-sm text-surface-700">
                <span class="font-semibold">{{ t('inventory.labels.selected_file') }}:</span>
                {{ selectedFile.name }}
            </p>
            <div class="mt-6 flex gap-3">
                <Button
                    :label="t('inventory.actions.import')"
                    icon="pi pi-upload"
                    :loading="uploading"
                    :disabled="!selectedFile"
                    @click="handleImport"
                />
            </div>
        </div>
    </PageShell>
</template>
