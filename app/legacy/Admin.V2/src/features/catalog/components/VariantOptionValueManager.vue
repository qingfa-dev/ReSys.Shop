<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import Button from 'primevue/button'
import { useToast } from '@/shared/composables/useToast'
import { VariantOptionValueApi } from '../api/variant-option-value.api'
import type { VariantOptionValueItem } from '../types'

const props = defineProps<{ variantId: string }>()

const toast = useToast()

const items = ref<VariantOptionValueItem[]>([])
const loading = ref(true)
const saving = ref(false)

const groups = computed(() => {
  const map = new Map<string, { optionTypeId: string; optionTypeName: string; values: VariantOptionValueItem[] }>()
  for (const item of items.value) {
    if (!map.has(item.optionTypeId)) {
      map.set(item.optionTypeId, { optionTypeId: item.optionTypeId, optionTypeName: item.optionTypeName, values: [] })
    }
    map.get(item.optionTypeId)!.values.push(item)
  }
  return Array.from(map.values())
})

onMounted(async () => {
  loading.value = true
  const result = await VariantOptionValueApi.get(props.variantId)
  if (result.isSuccess) {
    items.value = result.value.items
  } else {
    toast.error(result.message ?? 'Failed to load option values')
  }
  loading.value = false
})

function toggle(item: VariantOptionValueItem) {
  item.isAssigned = !item.isAssigned
}

async function sync() {
  saving.value = true
  const assignedIds = items.value.filter((i) => i.isAssigned).map((i) => i.optionValueId)
  const result = await VariantOptionValueApi.sync(props.variantId, { optionValueIds: assignedIds })
  if (result.isSuccess) {
    toast.success('Option values updated')
  } else {
    toast.error(result.message ?? 'Failed to update option values')
  }
  saving.value = false
}
</script>

<template>
  <div>
    <div v-if="loading">Loading...</div>
    <div v-else>
      <div v-for="group in groups" :key="group.optionTypeId" class="mb-4">
        <h4 class="text-surface-900 dark:text-surface-0 font-semibold mb-2">{{ group.optionTypeName }}</h4>
        <div v-for="item in group.values" :key="item.optionValueId" class="flex align-items-center gap-2 mb-1">
          <input
            type="checkbox"
            :checked="item.isAssigned"
            :id="'ov-' + item.optionValueId"
            @change="toggle(item)"
          />
          <label :for="'ov-' + item.optionValueId">{{ item.name }}{{ item.presentation ? ` (${item.presentation})` : '' }}</label>
        </div>
      </div>
      <Button :label="saving ? 'Saving...' : 'Save Option Values'" :disabled="saving" @click="sync" />
    </div>
  </div>
</template>
