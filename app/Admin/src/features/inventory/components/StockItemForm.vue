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
import { AppCard } from '@/shared/components'
import Button from 'primevue/button'
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
  if (mode.value === 'create') return t('inventory.stock_items.form.create_title')
  if (mode.value === 'edit') return t('inventory.stock_items.form.edit_title', { sku: variantId.value || '' })
  return t('inventory.stock_items.form.view_title')
})

async function loadStockItem() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  try {
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
  } catch (err) {
    console.error(err)
    loadError.value = 'Failed to load stock item'
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? StockItemFormMapper.toCreate(values)
    : StockItemFormMapper.toUpdate(values)
  try {
    const result = id.value
      ? await api.update(id.value, data)
      : await api.create(data)
    saving.value = false
    if (result.isSuccess) {
      toast.success(id.value ? t('inventory.stock_items.messages.update_success') : t('inventory.stock_items.messages.create_success'))
      const newId = result.value.id
      router.replace({ name: ROUTE.STOCKS.VIEW, params: { id: newId } })
    } else {
      toast.error(result.message ?? 'Save failed')
    }
  } catch (err) {
    console.error(err)
    saving.value = false
    toast.error('Save failed')
  }
})

function cancel() {
  if (id.value) router.push({ name: ROUTE.STOCKS.VIEW, params: { id: id.value } })
  else router.push({ name: ROUTE.STOCKS.LIST })
}

function toggleEdit() {
  router.push({ name: ROUTE.STOCKS.EDIT, params: { id: id.value } })
}

onMounted(async () => {
  await loadStockItem()
})
</script>

<template>
  <div>
    <PageHeader :title="title" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <Button
          v-if="mode === 'view'"
          :label="t('inventory.stock_items.actions.edit')"
          icon="pi pi-pencil"
          size="small"
          @click="toggleEdit"
        />
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="4" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadStockItem" />
    <AppCard v-else>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('inventory.stock_items.labels.variant_id')" :error="errors.variantId" required>
            <input v-model="variantId" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('inventory.stock_items.labels.location_id')" :error="errors.locationId" required>
            <input v-model="locationId" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('inventory.stock_items.labels.quantity')" :error="errors.quantity">
            <input v-model="quantity" type="number" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('inventory.stock_items.labels.low_stock_threshold')" :error="errors.lowStockThreshold">
            <input v-model="lowStockThreshold" type="number" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? t('inventory.stock_items.actions.save_create') : t('inventory.stock_items.actions.save_edit')"
        :cancel-label="t('inventory.stock_items.actions.cancel')"
        @save="save"
        @cancel="cancel"
      />
    </AppCard>
  </div>
</template>
