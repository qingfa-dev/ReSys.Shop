<script setup lang="ts">
import { watch } from 'vue'

const props = withDefaults(defineProps<{
  placeholder?: string
  debounce?: number
}>(), {
  placeholder: 'Search...',
  debounce: 300,
})

const model = defineModel<string>({ default: '' })
const emit = defineEmits<{
  search: [value: string]
}>()

let timer: ReturnType<typeof setTimeout> | null = null

watch(model, (val) => {
  if (timer) clearTimeout(timer)
  timer = setTimeout(() => {
    emit('search', val)
  }, props.debounce)
})

function clear() {
  model.value = ''
  emit('search', '')
}
</script>

<template>
  <IconField>
    <InputIcon class="pi pi-search" />
    <InputText
      v-model="model"
      :placeholder="placeholder"
      class="w-full min-w-64"
    />
    <InputIcon
      v-if="model"
      class="pi pi-times cursor-pointer"
      @click="clear"
    />
  </IconField>
</template>
