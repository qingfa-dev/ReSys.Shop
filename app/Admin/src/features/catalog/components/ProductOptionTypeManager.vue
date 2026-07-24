<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useToast } from '@/shared/composables/useToast'
import { OptionTypeApi } from '../api/option-type.api'
import { ProductOptionTypeApi } from '../api/product-option-type.api'
import type { ProductOptionTypeAssignmentItem } from '../types'

const props = defineProps<{ productId: string }>()

const toast = useToast()

interface OptionTypeEntry {
  optionTypeId: string
  name: string
  presentation: string | null
  position: number
  isAssigned: boolean
}

const items = ref<OptionTypeEntry[]>([])
const loading = ref(true)
const saving = ref(false)

onMounted(async () => {
  loading.value = true
  const [allResult, assignedResult] = await Promise.all([
    OptionTypeApi.getMany({ page: 1, pageSize: 1000, sort: [{ field: 'position', direction: 'Ascending' }] }),
    ProductOptionTypeApi.get(props.productId),
  ])

  const assignedIds = new Map<string, number>()
  if (assignedResult.isSuccess) {
    for (const a of assignedResult.value.items) {
      assignedIds.set(a.optionTypeId, a.position)
    }
  }

  if (allResult.isSuccess) {
    items.value = allResult.items.map((ot) => ({
      optionTypeId: ot.id,
      name: ot.name,
      presentation: ot.presentation,
      position: assignedIds.get(ot.id) ?? ot.position,
      isAssigned: assignedIds.has(ot.id),
    }))
  }
  loading.value = false
})

function toggle(item: OptionTypeEntry) {
  item.isAssigned = !item.isAssigned
}

async function sync() {
  saving.value = true
  const payload: ProductOptionTypeAssignmentItem[] = items.value
    .filter((i) => i.isAssigned)
    .map((i) => ({ optionTypeId: i.optionTypeId, position: i.position }))
  const result = await ProductOptionTypeApi.sync(props.productId, { items: payload })
  if (result.isSuccess) {
    toast.success('Option types updated')
  } else {
    toast.error(result.message ?? 'Failed to update option types')
  }
  saving.value = false
}
</script>

<template>
  <div class="card">
    <h3>Option Types</h3>
    <div v-if="loading">Loading...</div>
    <div v-else>
      <div v-for="item in items" :key="item.optionTypeId" class="flex align-items-center gap-2 mb-2">
        <input
          type="checkbox"
          :checked="item.isAssigned"
          :id="'ot-' + item.optionTypeId"
          @change="toggle(item)"
        />
        <label :for="'ot-' + item.optionTypeId" class="flex-1">{{ item.name }} {{ item.presentation ? `(${item.presentation})` : '' }}</label>
        <input
          v-model.number="item.position"
          type="number"
          class="p-inputtext p-component"
          style="width: 80px"
          placeholder="Position"
          min="0"
        />
      </div>
      <button class="p-button p-component mt-3" :disabled="saving" @click="sync">
        {{ saving ? 'Saving...' : 'Save Option Types' }}
      </button>
    </div>
  </div>
</template>
