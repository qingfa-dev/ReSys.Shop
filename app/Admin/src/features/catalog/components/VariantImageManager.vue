<script setup lang="ts">
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { VariantImageApi } from '../api'
import type { VariantImageDetailResponse } from '../types'

const props = defineProps<{
  variantId: string
  images: VariantImageDetailResponse[]
}>()

const emit = defineEmits<{
  'update:images': [value: VariantImageDetailResponse[]]
}>()

const { t } = useI18n()
const { confirmDelete } = useConfirm()
const toast = useToast()
const uploading = ref(false)

const fileInput = ref<HTMLInputElement | null>(null)

const MAX_IMAGES = 10

async function onFilesSelected(event: Event) {
  const input = event.target as HTMLInputElement
  if (!input.files?.length) return

  const files = Array.from(input.files).filter(f => f.type.startsWith('image/'))
  if (props.images.length + files.length > MAX_IMAGES) {
    toast.error(t('catalog.variants.images.max_limit', { max: MAX_IMAGES }))
    return
  }

  uploading.value = true
  for (const file of files) {
    try {
      const formData = new FormData()
      formData.append('file', file)
      formData.append('position', String(props.images.length + 1))
      formData.append('type', 'gallery')
      const result = await VariantImageApi.upload(props.variantId, formData)
      if (result.isSuccess) {
        emit('update:images', [...props.images, result.value])
        toast.success(t('catalog.variants.images.upload_success', { name: file.name }))
      } else {
        console.error(result.message)
        toast.error(
          t('catalog.variants.images.upload_failed', {
            name: file.name,
            reason: result.message ?? '',
          }),
        )
      }
    } catch (err) {
      console.error(err)
      toast.error(t('catalog.variants.images.upload_failed', { name: file.name, reason: '' }))
    }
  }
  uploading.value = false
  input.value = ''
}

function deleteImage(image: VariantImageDetailResponse) {
  confirmDelete({
    target: `image ${image.fileName}`,
    onAccept: async () => {
      try {
        const result = await VariantImageApi.delete(image.id)
        if (result.isSuccess) {
          emit(
            'update:images',
            props.images.filter(i => i.id !== image.id),
          )
          toast.success(t('catalog.variants.images.delete_success'))
        } else {
          console.error(result.message)
          toast.error(t('catalog.variants.images.delete_failed'))
        }
      } catch (err) {
        console.error(err)
        toast.error(t('catalog.variants.images.delete_failed'))
      }
    },
  })
}

function onDragStart(index: number, event: DragEvent) {
  event.dataTransfer!.effectAllowed = 'move'
  event.dataTransfer!.setData('text/plain', String(index))
}

function onDrop(targetIndex: number, event: DragEvent) {
  event.preventDefault()
  const sourceIndex = Number(event.dataTransfer!.getData('text/plain'))
  if (sourceIndex === targetIndex) return
  const reordered = [...props.images]
  const [moved] = reordered.splice(sourceIndex, 1)
  reordered.splice(targetIndex, 0, moved)
  emit('update:images', reordered)
}

function onDragOver(event: DragEvent) {
  event.preventDefault()
  event.dataTransfer!.dropEffect = 'move'
}

const dropActive = ref(false)
function onDragEnter() { dropActive.value = true }
function onDragLeave() { dropActive.value = false }
</script>

<template>
  <div class="flex flex-col gap-4">
    <div class="flex items-center justify-between">
      <h3 class="text-lg font-semibold">{{ t('catalog.variants.images.title') }}</h3>
      <div>
        <input
          ref="fileInput"
          type="file"
          multiple
          accept="image/*"
          class="hidden"
          :disabled="uploading || images.length >= MAX_IMAGES"
          @change="onFilesSelected"
        />
        <Button
          :label="uploading ? t('catalog.variants.images.uploading') : t('catalog.variants.images.upload')"
          icon="pi pi-upload"
          :disabled="uploading || images.length >= MAX_IMAGES"
          @click="fileInput?.click()"
        />
      </div>
    </div>

    <div
      v-if="!images.length"
      class="rounded-border border-2 border-dashed border-surface-300 p-8 text-center text-surface-500 dark:border-surface-600"
    >
      <i class="pi pi-images text-3xl mb-2" />
      <p>{{ t('catalog.variants.images.empty') }}</p>
    </div>

    <div
      v-else
      class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4"
      :class="{ 'border-2 border-dashed border-primary-400 bg-primary-50 dark:bg-primary-900/20': dropActive }"
      @dragenter="onDragEnter"
      @dragleave="onDragLeave"
    >
      <div
        v-for="(image, index) in images"
        :key="image.id"
        class="group relative cursor-move rounded-border border border-surface-200 overflow-hidden"
        draggable="true"
        @dragstart="onDragStart(index, $event)"
        @dragover="onDragOver"
        @drop="onDrop(index, $event)"
      >
        <img :src="image.url" :alt="image.fileName" class="aspect-square w-full object-cover" />
        <Button
          icon="pi pi-times"
          severity="danger"
          rounded
          size="small"
          class="absolute top-1 right-1 opacity-0 group-hover:opacity-100 transition-opacity"
          @click="deleteImage(image)"
        />
        <i
          v-if="image.type === 'primary'"
          class="pi pi-star-fill absolute bottom-1 left-1 text-yellow-500"
        />
      </div>
    </div>
  </div>
</template>
