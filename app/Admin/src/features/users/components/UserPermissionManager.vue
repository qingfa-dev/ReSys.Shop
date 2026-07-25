<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useToast } from '@/shared/composables/useToast'
import Checkbox from 'primevue/checkbox'
import Button from 'primevue/button'
import { UserPermissionApi } from '../api'
import type { UserPermissionItem } from '../types'

const props = defineProps<{ userId: string }>()
const toast = useToast()

const items = ref<UserPermissionItem[]>([])
const loading = ref(false)
const saving = ref(false)

async function load() {
  loading.value = true
  const result = await UserPermissionApi.get(props.userId)
  if (result.isSuccess) {
    items.value = result.value.items ?? []
  } else {
    toast.error(result.message ?? 'Failed to load permissions')
  }
  loading.value = false
}

function toggle(item: UserPermissionItem) {
  item.isAssigned = !item.isAssigned
}

async function onSave() {
  saving.value = true
  const data = { items: items.value.filter(i => i.isAssigned).map(i => ({ permissionId: i.permissionId })) }
  const result = await UserPermissionApi.sync(props.userId, data)
  saving.value = false
  if (result.isSuccess) {
    toast.success('Permissions updated successfully')
  } else {
    toast.error(result.message ?? 'Failed to update permissions')
  }
}

onMounted(load)
</script>

<template>
  <div>
    <h3 class="text-lg font-medium mb-3">Permissions</h3>
    <div v-if="loading" class="text-surface-500">Loading permissions...</div>
    <div v-else class="flex flex-col gap-2">
      <div v-for="item in items" :key="item.permissionId" class="flex items-center gap-2">
        <Checkbox
          :input-id="'perm-' + item.permissionId"
          :binary="true"
          :model-value="item.isAssigned"
          @update:model-value="toggle(item)"
        />
        <label :for="'perm-' + item.permissionId">{{ item.name }}</label>
      </div>
      <div class="mt-3">
        <Button label="Save Permissions" icon="pi pi-check" :loading="saving" @click="onSave" />
      </div>
    </div>
  </div>
</template>
