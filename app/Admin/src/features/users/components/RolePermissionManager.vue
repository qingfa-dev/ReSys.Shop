<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useToast } from '@/shared/composables/useToast'
import Checkbox from 'primevue/checkbox'
import Button from 'primevue/button'
import { RolePermissionApi } from '../api'
import type { UserPermissionItem } from '../types'

const props = defineProps<{ roleId: string }>()
const toast = useToast()

const items = ref<UserPermissionItem[]>([])
const grouped = ref<Record<string, UserPermissionItem[]>>({})
const loading = ref(false)
const saving = ref(false)

async function load() {
  loading.value = true
  const result = await RolePermissionApi.get(props.roleId)
  if (result.isSuccess) {
    const all = result.value.items ?? []
    const map: Record<string, UserPermissionItem[]> = {}
    for (const item of all) {
      const group = item.name.split('.')[0] || 'Other'
      if (!map[group]) map[group] = []
      map[group].push(item)
    }
    grouped.value = map
    items.value = all
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
  const data = { items: items.value.filter(i => i.isAssigned).map(i => ({ roleId: i.permissionId })) }
  const result = await RolePermissionApi.sync(props.roleId, data)
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
    <div v-else>
      <div v-for="(perms, group) in grouped" :key="group" class="mb-4">
        <h4 class="text-sm font-medium text-surface-600 uppercase mb-2">{{ group }}</h4>
        <div class="flex flex-col gap-2">
          <div v-for="item in perms" :key="item.permissionId" class="flex items-center gap-2">
            <Checkbox
              :input-id="'rperm-' + item.permissionId"
              :binary="true"
              :model-value="item.isAssigned"
              @update:model-value="toggle(item)"
            />
            <label :for="'rperm-' + item.permissionId">{{ item.name }}</label>
          </div>
        </div>
      </div>
      <div class="mt-3">
        <Button label="Save Permissions" icon="pi pi-check" :loading="saving" @click="onSave" />
      </div>
    </div>
  </div>
</template>
