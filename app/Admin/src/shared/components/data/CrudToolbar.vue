<script setup lang="ts">
import Card from 'primevue/card'
import Plus from '@primeicons/vue/plus'
import Trash from '@primeicons/vue/trash'
import Upload from '@primeicons/vue/upload'
import Search from '@primeicons/vue/search'

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
      <template #content>
        <Toolbar class="mb-4">
          <template #start>
            <Button :label="newLabel" severity="secondary" class="mr-2" @click="emit('new')">
              <Plus />
            </Button>
            <Button :label="deleteLabel" severity="secondary" :disabled="deleteDisabled" @click="emit('delete')">
              <Trash />
            </Button>
          </template>
          <template #end>
            <Button :label="exportLabel" severity="secondary" @click="emit('export')">
              <Upload />
            </Button>
          </template>
        </Toolbar>
        <div class="flex justify-between items-center">
          <slot name="header-left" />
          <IconField>
            <InputIcon> <Search /> </InputIcon>
            <InputText :placeholder="searchPlaceholder" fluid @update:modelValue="emit('update:search', $event ?? '')" />
          </IconField>
        </div>
      </template>
    </Card>
</template>
