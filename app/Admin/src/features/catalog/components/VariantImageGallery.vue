<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import Sidebar from 'primevue/sidebar'
import Button from 'primevue/button'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { VariantImageForms } from '../schemas'
import { VariantImageFormMapper } from '../mappers/variant-image.mapper'
import { VariantImageApi } from '../api/variant-image.api'
import type { VariantImageDetailResponse, EmbeddingDetailResponse } from '../types'

const props = defineProps<{ variantId: string }>()

const { t } = useI18n()
const toast = useToast()
const { confirmDelete } = useConfirm()

const images = ref<VariantImageDetailResponse[]>([])
const loading = ref(false)
const uploading = ref(false)
const saving = ref(false)
const sidebarVisible = ref(false)
const editingImage = ref<VariantImageDetailResponse | null>(null)
const embedding = ref<EmbeddingDetailResponse | null>(null)
const embeddingLoading = ref(false)

const schemas = new VariantImageForms(t)
const { handleSubmit, defineField, errors, resetForm, setValues } = useForm({
  validationSchema: toTypedSchema(schemas.update()),
})

const [alt] = defineField('alt')
const [position] = defineField('position')
const [imageType] = defineField('type')

async function load() {
  loading.value = true
  const result = await VariantImageApi.list(props.variantId)
  if (result.isSuccess) {
    images.value = result.value.images
  } else {
    toast.error(result.message ?? 'Failed to load images')
  }
  loading.value = false
}

function handleFileUpload(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  upload(file)
}

async function upload(file: File) {
  uploading.value = true
  const formData = new FormData()
  formData.append('file', file)
  formData.append('position', String(images.value.length + 1))
  formData.append('type', 'default')
  const result = await VariantImageApi.upload(props.variantId, formData)
  uploading.value = false
  if (result.isSuccess) {
    toast.success('Image uploaded')
    await load()
  } else {
    toast.error(result.message ?? 'Failed to upload image')
  }
}

function openEdit(image: VariantImageDetailResponse) {
  editingImage.value = image
  setValues({
    alt: image.alt ?? undefined,
    position: image.position,
    type: image.type,
  })
  embedding.value = null
  sidebarVisible.value = true
}

const submit = handleSubmit(async (values) => {
  if (!editingImage.value) return
  saving.value = true
  const data = VariantImageFormMapper.toUpdate(values)
  const result = await VariantImageApi.update(editingImage.value.id, data)
  saving.value = false
  if (result.isSuccess) {
    toast.success('Image metadata updated')
    sidebarVisible.value = false
    await load()
  } else {
    toast.error(result.message ?? 'Failed to update image metadata')
  }
})

async function remove(image: VariantImageDetailResponse) {
  confirmDelete({
    target: `image ${image.fileName}`,
    onAccept: async () => {
      const result = await VariantImageApi.delete(image.id)
      if (result.isSuccess) {
        toast.success('Image deleted')
        await load()
      } else {
        toast.error(result.message ?? 'Failed to delete image')
      }
    },
  })
}

async function loadEmbedding(imageId: string) {
  embeddingLoading.value = true
  const result = await VariantImageApi.get(imageId)
  embeddingLoading.value = false
}

async function regenerateEmbedding() {
  if (!editingImage.value) return
  embeddingLoading.value = true
  const result = await VariantImageApi.regenerateEmbedding(editingImage.value.id, {
    modelName: 'fashion-clip',
    modelVersion: '1',
  })
  embeddingLoading.value = false
  if (result.isSuccess) {
    embedding.value = result.value
    toast.success('Embedding regenerated')
  } else {
    toast.error(result.message ?? 'Failed to regenerate embedding')
  }
}

onMounted(load)
</script>

<template>
  <div>
    <div class="flex justify-content-between align-items-center mb-3">
      <h3 class="m-0">{{ t('catalog.variants.images.title') }}</h3>
      <label class="p-button p-component p-button-sm" :class="{ 'p-disabled': uploading }">
        {{ uploading ? 'Uploading...' : 'Upload Image' }}
        <input type="file" accept="image/*" class="hidden" @change="handleFileUpload" />
      </label>
    </div>
    <div v-if="loading">Loading...</div>
    <div v-else-if="images.length === 0" class="text-surface-500">No images</div>
    <div v-else class="grid">
      <div v-for="image in images" :key="image.id" class="col-12 sm:col-6 md:col-4 lg:col-3">
        <div class="border border-surface-200 dark:border-surface-700 rounded-lg overflow-hidden">
          <img :src="image.url" :alt="image.alt ?? image.fileName" class="w-full h-32 object-cover cursor-pointer" @click="openEdit(image)" />
          <div class="p-2 flex justify-content-between align-items-center">
            <span class="text-sm text-truncate">{{ image.fileName }}</span>
            <Button icon="pi pi-trash" size="small" class="p-button-text p-button-danger" @click="remove(image)" />
          </div>
        </div>
      </div>
    </div>
    <Sidebar v-model:visible="sidebarVisible" :header="editingImage?.fileName ?? 'Image'" position="right">
      <form @submit="submit" class="flex flex-column gap-3">
        <div v-if="editingImage">
          <img :src="editingImage.url" :alt="editingImage.alt ?? editingImage.fileName" class="w-full h-48 object-cover rounded mb-3" />
        </div>
        <div>
          <label class="block font-medium mb-1">{{ t('catalog.variants.images.alt') }}</label>
          <input v-model="alt" type="text" class="p-inputtext p-component w-full" />
        </div>
        <div>
          <label class="block font-medium mb-1">{{ t('catalog.variants.images.position') }}</label>
          <input v-model.number="position" type="number" class="p-inputtext p-component w-full" :invalid="!!errors.position" />
          <small v-if="errors.position" class="text-red-500">{{ errors.position }}</small>
        </div>
        <div>
          <label class="block font-medium mb-1">{{ t('catalog.variants.images.type') }}</label>
          <input v-model="imageType" type="text" class="p-inputtext p-component w-full" :invalid="!!errors.type" />
          <small v-if="errors.type" class="text-red-500">{{ errors.type }}</small>
        </div>
        <div v-if="editingImage" class="mt-3 border-top-1 pt-3">
          <h4 class="font-semibold mb-2">Embedding</h4>
          <div v-if="embedding" class="text-sm">
            <div>Model: {{ embedding.modelName }} v{{ embedding.modelVersion }}</div>
            <div>Dimensions: {{ embedding.dimensions }}</div>
            <div>Created: {{ embedding.createdAt }}</div>
          </div>
          <div v-else class="text-sm text-surface-500 mb-2">No embedding</div>
          <Button
            type="button"
            :label="embeddingLoading ? 'Processing...' : 'Regenerate Embedding'"
            size="small"
            :disabled="embeddingLoading"
            @click="regenerateEmbedding"
          />
        </div>
        <div class="flex justify-content-end gap-2 mt-3">
          <Button type="button" :label="t('catalog.variants.actions.cancel')" class="p-button-secondary" @click="sidebarVisible = false" />
          <Button type="submit" :label="t('catalog.variants.actions.save')" :loading="saving" :disabled="saving" />
        </div>
      </form>
    </Sidebar>
  </div>
</template>
