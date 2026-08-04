<script setup lang="ts">
import { ref } from 'vue'

const emit = defineEmits<{ fileSelected: [file: File] }>()
const isDragging = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']

function onDragOver(e: DragEvent): void {
  e.preventDefault()
  isDragging.value = true
}
function onDragLeave(): void { isDragging.value = false }

function onDrop(e: DragEvent): void {
  e.preventDefault()
  isDragging.value = false
  const file = e.dataTransfer?.files?.[0]
  if (file) emit('fileSelected', file)
}

function onFileInput(e: Event): void {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) emit('fileSelected', file)
  input.value = '' // Reset so same file can be re-selected
}

function onBrowseClick(): void {
  fileInput.value?.click()
}
</script>
<template>
  <!-- Section: CBIR Dropzone -->
  <div
    class="border-2 border-dashed rounded-2xl p-12 text-center transition-all duration-200 cursor-pointer min-h-[300px] flex flex-col items-center justify-center"
    :class="isDragging
      ? 'border-gray-900 bg-gray-50'
      : 'border-gray-300 hover:border-gray-400 bg-white'"
    @dragover="onDragOver"
    @dragleave="onDragLeave"
    @drop="onDrop"
    @click="onBrowseClick"
  >
    <i class="pi pi-cloud-upload text-5xl text-gray-400 mb-4" />
    <p class="text-lg font-medium text-gray-900">Drop an image here or click to browse</p>
    <p class="text-sm text-gray-500 mt-2">JPEG, PNG, or WebP up to 10 MB</p>
    <Button label="Choose an image" severity="secondary" class="mt-6" />
    <input
      ref="fileInput"
      type="file"
      :accept="ALLOWED_TYPES.join(',')"
      class="hidden"
      @change="onFileInput"
    />
  </div>
</template>
