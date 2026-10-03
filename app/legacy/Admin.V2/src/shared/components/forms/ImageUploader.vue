<template>
  <div>
    <div
      class="relative flex flex-col items-center justify-center gap-2 rounded-border border-2 border-dashed p-6 text-center transition-colors"
      :class="isDragOver ? 'border-primary-400 bg-primary-50 dark:bg-primary-400/10' : 'border-surface-300 dark:border-surface-600'"
      @dragover.prevent="isDragOver = true" @dragleave.prevent="isDragOver = false" @drop.prevent="onDrop">
      <template v-if="!previews.length">
        <i class="pi pi-cloud-upload text-2xl text-surface-400" />
        <p class="text-sm text-surface-500">
          Drag & drop {{ multiple ? 'images' : 'an image' }}, or
          <button type="button" class="text-primary-600 hover:underline" @click="fileInput?.click()">browse</button>
        </p>
        <p class="text-xs text-surface-400">PNG, JPG up to {{ maxSizeMb }}MB</p>
      </template>

      <div v-else class="grid grid-cols-3 gap-3 sm:grid-cols-4">
        <div v-for="(src, i) in previews" :key="i"
          class="group relative aspect-square overflow-hidden rounded-border border border-surface-200 dark:border-surface-700">
          <img :src="src" class="h-full w-full object-cover" />
          <button type="button"
            class="absolute right-1 top-1 flex h-6 w-6 items-center justify-center rounded-full bg-black/60 text-white opacity-0 transition-opacity group-hover:opacity-100"
            @click="removeAt(i)">
            <i class="pi pi-times text-xs" />
          </button>
        </div>
        <button v-if="multiple" type="button"
          class="flex aspect-square items-center justify-center rounded-border border border-dashed border-surface-300 text-surface-400 hover:border-primary-400 hover:text-primary-500 dark:border-surface-600"
          @click="fileInput?.click()">
          <i class="pi pi-plus" />
        </button>
      </div>

      <input ref="fileInput" type="file" accept="image/*" :multiple="multiple" class="hidden" @change="onSelect" />
    </div>
    <small v-if="error" class="mt-1 block text-red-500">{{ error }}</small>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';

const props = withDefaults(
  defineProps<{
    modelValue: File[];
    multiple?: boolean;
    maxSizeMb?: number;
    error?: string;
  }>(),
  { multiple: false, maxSizeMb: 5 },
);

const emit = defineEmits<{ 'update:modelValue': [File[]] }>();

const fileInput = ref<HTMLInputElement>();
const isDragOver = ref(false);
const previews = ref<string[]>([]);

function addFiles(fileList: FileList | File[]) {
  const files = Array.from(fileList).filter((f) => f.size <= props.maxSizeMb * 1024 * 1024);
  const next = props.multiple ? [...props.modelValue, ...files] : files.slice(0, 1);
  emit('update:modelValue', next);
  previews.value = next.map((f) => URL.createObjectURL(f));
}

function onSelect(e: Event) {
  const files = (e.target as HTMLInputElement).files;
  if (files) addFiles(files);
}

function onDrop(e: DragEvent) {
  isDragOver.value = false;
  if (e.dataTransfer?.files) addFiles(e.dataTransfer.files);
}

function removeAt(i: number) {
  const next = [...props.modelValue];
  next.splice(i, 1);
  previews.value.splice(i, 1);
  emit('update:modelValue', next);
}
</script>
