<script setup lang="ts">
import Card from 'primevue/card'

interface Props {
  newLabel?: string
  deleteLabel?: string
  exportLabel?: string
  deleteDisabled?: boolean
  searchPlaceholder?: string
}

withDefaults(defineProps<Props>(), {
  newLabel: 'New',
  deleteLabel: 'Delete',
  exportLabel: 'Export',
  deleteDisabled: false,
  searchPlaceholder: 'Search...',
})

const emit = defineEmits<{
  (e: 'new'): void
  (e: 'delete'): void
  (e: 'export'): void
  (e: 'update:search', value: string): void
}>()
</script>

<template>
    <Card class="mb-6">
    <Toolbar class="mb-4">
      <template #start>
        <Button :label="newLabel" icon="pi pi-plus" severity="secondary" class="mr-2" @click="emit('new')" />
        <Button :label="deleteLabel" icon="pi pi-trash" severity="secondary" :disabled="deleteDisabled" @click="emit('delete')" />
      </template>
      <template #end>
        <Button :label="exportLabel" icon="pi pi-upload" severity="secondary" @click="emit('export')" />
      </template>
    </Toolbar>
    <div class="flex justify-between items-center">
      <slot name="header-left" />
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText :placeholder="searchPlaceholder" fluid @update:modelValue="emit('update:search', $event ?? '')" />
      </IconField>
    </div>
  </Card>
</template>
