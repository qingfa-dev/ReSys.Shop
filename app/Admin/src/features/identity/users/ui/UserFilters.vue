<template>
  <div class="mb-3 flex gap-2">
    <InputText v-model="searchDebounced" placeholder="Search by name or email" class="w-64" />
    <Select v-model="status" :options="statusOptions" option-label="label" option-value="value" placeholder="Status" show-clear class="w-40" />
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useDebouncedRef } from '@/shared/composables/useDebouncedRef'

const props = defineProps<{ modelValue: { search: string; status?: string } }>()
const emit = defineEmits<{ 'update:modelValue': [v: { search: string; status?: string }] }>()

const search = ref(props.modelValue.search)
const status = ref<string | undefined>(props.modelValue.status)
const searchDebounced = useDebouncedRef(search, 300)

const statusOptions = [
  { label: 'Active', value: 'active' },
  { label: 'Inactive', value: 'inactive' },
  { label: 'Invited', value: 'invited' },
  { label: 'Suspended', value: 'suspended' },
]

watch([searchDebounced, status], () => {
  emit('update:modelValue', { search: searchDebounced.value, status: status.value })
})
</script>
