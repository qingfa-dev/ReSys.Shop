<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use';
import { useToast } from '@/shared/composables/toast.use';
import apiClient from '@/shared/api/api.client';
import type { ApiResult } from '@/shared/api/api.types';
import type { ProductImage } from '../types/product.types';
import ProductImageUploader from './images/ProductImageUploader.vue';
import ProductImageList from './images/ProductImageList.vue';
import { productService } from '../services/product.service';

const props = defineProps<{
    productId: string;
}>();

const { handleApiResult } = useApiErrorHandler();
const { showToast } = useToast();

const images = ref<ProductImage[]>([]);
const loading = ref(false);

const loadImages = async () => {
    loading.value = true;
    try {
        const result = await productService.getImages(props.productId);
        if (result.success && result.data) {
            images.value = result.data;
        }
    } finally {
        loading.value = false;
    }
};

const handleUpload = async (payload: { file: File, role: number, alt: string, onSuccess: () => void }) => {
    try {
        const result = await productService.uploadImage(props.productId, payload.file, payload.role, payload.alt);
        
        if (result.success) {
            showToast('success', 'Uploaded', 'Image uploaded successfully');
            payload.onSuccess();
            await loadImages();
        } else {
            handleApiResult(result);
        }
    } catch (e) {
        showToast('error', 'Error', 'Upload failed');
    }
};

const onDelete = async (id: string) => {
    const result = await productService.deleteImage(props.productId, id);
    if (result.success) {
        showToast('success', 'Deleted', 'Image removed');
        await loadImages();
    } else {
        handleApiResult(result);
    }
};

const onUpdateImage = async (payload: { id: string, role: number, alt: string }) => {
    try {
        const result = await productService.updateImage(props.productId, payload.id, payload.role, payload.alt);

        if (result.success) {
            showToast('success', 'Updated', 'Image details updated');
            await loadImages();
        } else {
            handleApiResult(result);
        }
    } catch (e) {
        showToast('error', 'Error', 'Failed to update image');
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