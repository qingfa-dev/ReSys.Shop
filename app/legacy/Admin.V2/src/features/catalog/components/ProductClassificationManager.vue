<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useToast } from '@/shared/composables/useToast'
import { TaxonomyApi } from '../api/taxonomy.api'
import { TaxonApi } from '../api/taxon.api'
import { ProductClassificationApi } from '../api/product-classification.api'
import type { ProductClassificationAssignmentItem } from '../types'

const props = defineProps<{ productId: string }>()

const toast = useToast()

interface ClassificationEntry {
  taxonId: string
  name: string
  prettyName: string | null
  position: number
  isAssigned: boolean
}

const items = ref<ClassificationEntry[]>([])
const loading = ref(true)
const saving = ref(false)
const taxonomyId = ref<string | null>(null)

function flattenTree(tree: any[]): any[] {
  const result: any[] = []
  function walk(nodes: any[]) {
    for (const node of nodes) {
      result.push(node)
      if (node.children?.length) walk(node.children)
    }
  }
  walk(tree)
  return result
}

onMounted(async () => {
  loading.value = true

  const taxonomiesResult = await TaxonomyApi.getMany({ page: 1, pageSize: 1, sort: [{ field: 'name', direction: 'Ascending' }] })
  if (taxonomiesResult.isSuccess && taxonomiesResult.items.length > 0) {
    taxonomyId.value = taxonomiesResult.items[0]?.id ?? null
    if (!taxonomyId.value) {
      toast.error('No taxonomy found')
      loading.value = false
      return
    }
  } else {
    loading.value = false
    return
  }

  const [treeResult, assignedResult] = await Promise.all([
    TaxonApi.getTree(taxonomyId.value),
    ProductClassificationApi.get(props.productId),
  ])

  const assignedIds = new Map<string, number>()
  if (assignedResult.isSuccess) {
    for (const a of assignedResult.value.items) {
      assignedIds.set(a.taxonId, a.position)
    }
  }

  if (treeResult.isSuccess) {
    const allTaxons = flattenTree(treeResult.value.tree)
    items.value = allTaxons.map((t) => ({
      taxonId: t.id,
      name: t.name,
      prettyName: t.prettyName ?? null,
      position: assignedIds.get(t.id) ?? t.position,
      isAssigned: assignedIds.has(t.id),
    }))
  }

  loading.value = false
})

function toggle(item: ClassificationEntry) {
  item.isAssigned = !item.isAssigned
}

async function sync() {
  saving.value = true
  const payload: ProductClassificationAssignmentItem[] = items.value
    .filter((i) => i.isAssigned)
    .map((i) => ({ taxonId: i.taxonId, position: i.position }))
  const result = await ProductClassificationApi.sync(props.productId, { items: payload })
  if (result.isSuccess) {
    toast.success('Classifications updated')
  } else {
    toast.error(result.message ?? 'Failed to update classifications')
  }
  saving.value = false
}
</script>

<template>
  <div class="card">
    <h3>Classifications</h3>
    <div v-if="loading">Loading...</div>
    <div v-else-if="!taxonomyId">No taxonomy available</div>
    <div v-else>
      <div v-for="item in items" :key="item.taxonId" class="flex align-items-center gap-2 mb-2">
        <input
          type="checkbox"
          :checked="item.isAssigned"
          :id="'cl-' + item.taxonId"
          @change="toggle(item)"
        />
        <label :for="'cl-' + item.taxonId" class="flex-1">
          {{ item.prettyName ?? item.name }}
        </label>
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
        {{ saving ? 'Saving...' : 'Save Classifications' }}
      </button>
    </div>
  </div>
</template>
