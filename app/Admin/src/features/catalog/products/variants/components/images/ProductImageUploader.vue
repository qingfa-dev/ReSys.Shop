<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { ProductImage } from '../../../types/product-image.response';

const { t } = useI18n();

const props = defineProps<{
    existingImages: ProductImage[];
}>();

const emit = defineEmits(['upload']);

const selectedRole = ref(3);
const currentFile = ref<File | null>(null);
const filePreview = ref<string | null>(null);
const altText = ref('');
const uploading = ref(false);
const showRoleConflictConfirm = ref(false);

const roles = [
    { label: t('catalog.products.images.roles.primary'), value: 0 },
    { label: t('catalog.products.images.roles.thumbnail'), value: 1 },
    { label: t('catalog.products.images.roles.square'), value: 2 },
    { label: t('catalog.products.images.roles.gallery'), value: 3 },
    { label: t('catalog.products.images.roles.search'), value: 4 }
];

const handleFileSelect = (event: { files: File[] }) => {
    const file = event.files[0];
    if (file) {
        currentFile.value = file;
        filePreview.value = URL.createObjectURL(file);
        altText.value = file.name.split('.')[0] ?? '';
        selectedRole.value = 3;
    }
};

const clearSelection = () => {
    if (filePreview.value) URL.revokeObjectURL(filePreview.value);
    currentFile.value = null;
    filePreview.value = null;
    altText.value = '';
    selectedRole.value = 3;
};

const proceedToUpload = () => {
    if (!currentFile.value) return;

    if (selectedRole.value === 0 || selectedRole.value === 4) {
        const roleMap: Record<number, string> = { 0: 'Default', 4: 'Search' };
        const targetRoleString = roleMap[selectedRole.value];
        
        const hasConflict = props.existingImages.some(i => i.role === selectedRole.value);
        if (hasConflict) {
            showRoleConflictConfirm.value = true;
            return;
        }
    }
    executeUpload();
};

const executeUpload = () => {
    if (!currentFile.value) return;
    
    uploading.value = true;
    emit('upload', {
        file: currentFile.value,
        role: selectedRole.value,
        alt: altText.value || currentFile.value.name,
        onSuccess: () => {
            uploading.value = false;
            showRoleConflictConfirm.value = false;
            clearSelection();
        }
    });
};
</script>

<template>
    <div class="p-6 border border-surface-200 dark:border-surface-700 rounded-2xl bg-surface-50 dark:bg-surface-800/50">
        <h4 class="text-base font-bold mb-4 mt-0">{{ t('catalog.products.images.add_new') }}</h4>
        
        <div class="flex flex-col md:flex-row gap-6 items-start">
            <div class="flex-1 w-full md:w-auto">
                <div v-if="!currentFile" class="w-full">
                    <FileUpload mode="basic" name="file" accept="image/*" :maxFileSize="5000000" @select="handleFileSelect" :auto="false" :chooseLabel="t('catalog.products.images.select_prompt')" class="w-full" />
                </div>
                
                <div v-else class="flex flex-col gap-4 w-full">
                    <div class="relative w-full aspect-video rounded-xl overflow-hidden border border-surface-200 dark:border-surface-700 bg-surface-900 flex items-center justify-center group">
                        <Image :src="filePreview || ''" preview class="max-w-full max-h-full object-contain" />
                        <Button icon="pi pi-times" rounded severity="danger" class="absolute top-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity" @click="clearSelection" />
                    </div>
                    
                    <div class="flex items-center justify-between text-xs text-surface-500 px-1">
                        <span class="truncate max-w-[200px]">{{ currentFile.name }}</span>
                        <span>{{ (currentFile.size / 1024).toFixed(1) }} KB</span>
                    </div>
                </div>
            </div>

            <div v-if="currentFile" class="flex-1 flex flex-col gap-4 w-full md:w-auto animate-fade-in">
                <div class="flex flex-col gap-2">
                    <label class="font-bold text-xs uppercase text-surface-500">{{ t('catalog.products.images.role_label') }}</label>
                    <SelectButton v-model="selectedRole" :options="roles" optionLabel="label" optionValue="value" :allowEmpty="false" />
                    <div class="text-xs text-surface-500 mt-1">
                        <span v-if="selectedRole === 3">{{ t('catalog.products.images.roles.desc_gallery') }}</span>
                        <span v-else-if="selectedRole === 0" class="text-primary font-bold">{{ t('catalog.products.images.roles.desc_primary') }}</span>
                        <span v-else-if="selectedRole === 4" class="text-blue-500 font-bold">{{ t('catalog.products.images.roles.desc_search') }}</span>
                        <span v-else-if="selectedRole === 1">{{ t('catalog.products.images.roles.desc_thumbnail') }}</span>
                        <span v-else-if="selectedRole === 2">{{ t('catalog.products.images.roles.desc_square') }}</span>
                    </div>
                </div>

                <div class="flex flex-col gap-2">
                    <label class="font-bold text-xs uppercase text-surface-500">{{ t('catalog.products.images.alt_text') }}</label>
                    <InputText v-model="altText" :placeholder="t('catalog.products.images.alt_placeholder')" class="w-full" />
                </div>

                <div class="flex justify-end pt-2">
                    <Button :label="t('catalog.products.images.upload_now')" icon="pi pi-upload" @click="proceedToUpload" :loading="uploading" />
                </div>
            </div>
        </div>

        <Dialog v-model:visible="showRoleConflictConfirm" :header="t('catalog.products.images.conflict_header')" modal :style="{ width: '350px' }">
            <div class="flex flex-col gap-4 items-center text-center p-2">
                <i class="pi pi-exclamation-triangle text-4xl text-orange-500"></i>
                <p class="m-0">
                    {{ (t('catalog.products.images.conflict_msg') || '').replace('{role}', roles.find(r => r.value === selectedRole)?.label || '') }}
                </p>
            </div>
            <template #footer>
                <Button :label="t('catalog.products.confirm.reject_label')" text severity="secondary" @click="showRoleConflictConfirm = false" />
                <Button label="Replace & Upload" severity="warning" icon="pi pi-refresh" @click="executeUpload" :loading="uploading" />
            </template>
        </Dialog>
    </div>
</template>

<style scoped>
.animate-fade-in {
    animation: fadeIn 0.3s ease-in-out;
}
@keyframes fadeIn {
    from { opacity: 0; transform: translateY(5px); }
    to { opacity: 1; transform: translateY(0); }
}
</style>
