<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useStockItem } from '../composables/useStockItem'
import { StockItemForms } from '../schemas'
import { StockItemFormMapper } from '../mappers/stock-item.mapper'
import { ROUTE } from '../routes'

const { id, mode, route, router, toast, api } = useStockItem()
const { t } = useI18n()

const schemas = new StockItemForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(
    mode.value === 'create' ? schemas.create() : schemas.update(),
  ),
})

const [variantId] = defineField('variantId')
const [locationId] = defineField('locationId')
const [quantity] = defineField('quantity')
const [lowStockThreshold] = defineField('lowStockThreshold')

const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const title = computed(() => {
  if (mode.value === 'create') return 'Create Stock Item'
  if (mode.value === 'edit') return `Edit Stock Item: ${variantId.value || ''}`
  return 'Stock Item Detail'
})

async function loadStockItem() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  const result = await api.get(id.value)
  if (result.isSuccess) {
    setValues({
      variantId: result.value.variantId ?? undefined,
      locationId: result.value.locationId ?? undefined,
      quantity: result.value.quantity ?? undefined,
      lowStockThreshold: result.value.lowStockThreshold ?? undefined,
    })
  } else {
    loadError.value = result.message ?? 'Failed to load stock item'
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? StockItemFormMapper.toCreate(values)
    : StockItemFormMapper.toUpdate(values)
  const result = id.value
    ? await api.update(id.value, data)
    : await api.create(data)
  saving.value = false
  if (result.isSuccess) {
    toast.success(id.value ? 'Stock item updated' : 'Stock item created')
    const newId = result.value.id
    router.replace({ name: 'inventory.stocks.view', params: { id: newId } })
  } else {
    toast.error(result.message ?? 'Save failed')
  }
})

function cancel() {
  if (id.value) router.push({ name: 'inventory.stocks.view', params: { id: id.value } })
  else router.push({ name: 'inventory.stocks.list' })
}

function toggleEdit() {
  router.push({ name: 'inventory.stocks.edit', params: { id: id.value } })
}

onMounted(async () => {
  await loadStockItem()
})
</script>

<template>
  <div>
    <PageHeader :title="title" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <button v-if="mode === 'view'" class="p-button p-component" @click="toggleEdit">Edit</button>
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="4" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadStockItem" />
    <div v-else class="card">
      <div class="grid">
        <div class="col-6">
          <FormField label="Variant ID" :error="errors.variantId" required>
            <input v-model="variantId" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Location ID" :error="errors.locationId" required>
            <input v-model="locationId" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-6">
          <FormField label="Quantity" :error="errors.quantity">
            <input v-model="quantity" type="number" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Low Stock Threshold" :error="errors.lowStockThreshold">
            <input v-model="lowStockThreshold" type="number" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? 'Create' : 'Save'"
        :cancel-label="'Cancel'"
        @save="save"
        @cancel="cancel"
      />
    </div>
  </div>
</template>
