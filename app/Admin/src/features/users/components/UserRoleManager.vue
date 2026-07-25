<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useToast } from '@/shared/composables/useToast'
import Checkbox from 'primevue/checkbox'
import Button from 'primevue/button'
import { UserRoleApi } from '../api'
import type { UserRoleItem } from '../types'

const props = defineProps<{ userId: string }>()
const toast = useToast()

const items = ref<UserRoleItem[]>([])
const loading = ref(false)
const saving = ref(false)

async function load() {
  loading.value = true
  const result = await UserRoleApi.get(props.userId)
  if (result.isSuccess) {
    items.value = result.value.items ?? []
  } else {
    toast.error(result.message ?? 'Failed to load roles')
  }
  loading.value = false
}

function toggle(item: UserRoleItem) {
  item.isAssigned = !item.isAssigned
}

async function onSave() {
  saving.value = true
  const data = { items: items.value.filter(i => i.isAssigned).map(i => ({ roleId: i.roleId })) }
  const result = await UserRoleApi.sync(props.userId, data)
  saving.value = false
  if (result.isSuccess) {
    toast.success('Roles updated successfully')
  } else {
    toast.error(result.message ?? 'Failed to update roles')
  }
}

onMounted(load)
</script>

<template>
  <div>
    <h3 class="text-lg font-medium mb-3">Roles</h3>
    <div v-if="loading" class="text-surface-500">Loading roles...</div>
    <div v-else class="flex flex-col gap-2">
      <div v-for="item in items" :key="item.roleId" class="flex items-center gap-2">
        <Checkbox
          :input-id="'role-' + item.roleId"
          :binary="true"
          :model-value="item.isAssigned"
          @update:model-value="toggle(item)"
        />
        <label :for="'role-' + item.roleId">{{ item.name }}</label>
      </div>
      <div class="mt-3">
        <Button label="Save Roles" icon="pi pi-check" :loading="saving" @click="onSave" />
      </div>
    </div>
  </div>
</template>
