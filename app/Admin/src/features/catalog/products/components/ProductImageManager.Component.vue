<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use';
import { useToast } from '@/shared/composables/toast.use';
import apiClient from '@/shared/api/http/api.client';
import type { ProductImage } from '../types/Product.Response.Type';
import ProductImageUploader from './images/ProductImageUploader.Component.vue';
import ProductImageList from './images/ProductImageList.Component.vue';
import { productService } from '../services/product.service';

const props = defineProps<{
    productId: string;
}>();

const { handleApiResult } = useApiErrorHandler();
const { showToast } = useToast();
const { t } = useI18n();

const images = ref<ProductImage[]>([]);
const loading = ref(false);

const loadImages = async () => {
    loading.value = true;
    try {
        const result = await productService.getImages(props.productId);
        if (result.isSuccess && result.items) {
            images.value = result.items;
        }
    } finally {
        loading.value = false;
    }
};

const handleUpload = async (payload: { file: File, role: number, alt: string, onSuccess: () => void }) => {
    try {
        const result = await productService.uploadImage(props.productId, payload.file, payload.role, payload.alt);
        
        if (result.isSuccess) {
            payload.onSuccess();
            await loadImages();
        } else {
            handleApiResult(result);
        }
    } catch (e) {
        showToast('error', t('common.error'), t('catalog.products.images.messages.upload_failed'));
    }
};

const onDelete = async (id: string) => {
    const result = await productService.deleteImage(id);
    if (result.isSuccess) {
        showToast('success', t('common.deleted'), t('catalog.products.images.messages.delete_success'));
        await loadImages();
    } else {
        handleApiResult(result);
    }
};

const onUpdateImage = async (payload: { id: string, role: number, alt: string }) => {
    try {
        const result = await productService.updateImage(payload.id, { alt: payload.alt, role: payload.role });

        if (result.isSuccess) {
            showToast('success', t('common.updated'), t('catalog.products.images.messages.update_success'));
            await loadImages();
        } else {
            handleApiResult(result);
        }
    } catch (e) {
        showToast('error', t('common.error'), t('catalog.products.images.messages.update_failed'));
    }
};

onMounted(() => {
    loadImages();
});
</script>

<template>
    <div class="flex flex-col gap-8">
        <!-- Header -->
        <div>
            <h3 class="text-lg font-bold m-0">Visual Assets</h3>
            <p class="text-sm text-surface-500 m-0">Upload and organize product images for the storefront.</p>
        </div>

        <!-- Uploader Section -->
        <ProductImageUploader :existingImages="images" @upload="handleUpload" />

        <!-- List Section -->
        <div v-if="loading" class="flex justify-center py-20">
            <ProgressSpinner />
        </div>

        <div v-else-if="images.length === 0" class="py-20 text-center border-2 border-dashed border-surface-200 dark:border-surface-800 rounded-3xl">
            <i class="pi pi-image text-4xl text-surface-200 mb-4"></i>
            <p class="text-surface-400 italic">No images uploaded yet.</p>
        </div>

        <ProductImageList v-else :images="images" @update-image="onUpdateImage" @delete="onDelete" />
    </div>
</template>
