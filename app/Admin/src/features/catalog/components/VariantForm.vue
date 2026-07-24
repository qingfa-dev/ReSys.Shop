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
import Checkbox from 'primevue/checkbox'
import Select from 'primevue/select'
import { useToast } from '@/shared/composables/useToast'
import { VariantForms } from '../schemas'
import { VariantFormMapper } from '../mappers/variant.mapper'
import { VariantApi } from '../api'
import { ROUTE } from '../routes'

const route = useRoute()
const router = useRouter()
const { t } = useI18n()
const toast = useToast()

const id = computed(() => route.params.id as string | undefined)
const productId = computed(() => route.params.productId as string)
const mode = computed<'create' | 'view' | 'edit'>(() => {
  if (!id.value) return 'create'
  if (route.name?.toString().endsWith('.edit')) return 'edit'
  return 'view'
})

const schemas = new VariantForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(
    mode.value === 'create' ? schemas.create() : schemas.update(),
  ),
})

const [sku] = defineField('sku')
const [position] = defineField('position')
const [trackInventory] = defineField('trackInventory')
const [weight] = defineField('weight')
const [weightUnit] = defineField('weightUnit')
const [height] = defineField('height')
const [width] = defineField('width')
const [depth] = defineField('depth')
const [dimensionsUnit] = defineField('dimensionsUnit')
const [price] = defineField('price')
const [costPrice] = defineField('costPrice')
const [costCurrency] = defineField('costCurrency')
const [isMaster] = defineField('isMaster')

const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const title = computed(() => {
  if (mode.value === 'create') return t('catalog.variants.titles.create')
  if (mode.value === 'edit') return `${t('catalog.variants.actions.edit')}: ${sku.value || ''}`
  return sku.value || t('catalog.variants.titles.view')
})

async function loadVariant() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  const result = await VariantApi.get(id.value)
  if (result.isSuccess) {
    setValues({
      sku: result.value.sku,
      position: result.value.position,
      trackInventory: result.value.trackInventory ?? undefined,
      weight: result.value.weight ?? undefined,
      weightUnit: result.value.weightUnit ?? undefined,
      height: result.value.height ?? undefined,
      width: result.value.width ?? undefined,
      depth: result.value.depth ?? undefined,
      dimensionsUnit: result.value.dimensionsUnit ?? undefined,
      price: result.value.price ?? undefined,
      costPrice: result.value.costPrice ?? undefined,
      costCurrency: result.value.costCurrency ?? undefined,
      isMaster: result.value.isMaster ?? undefined,
    })
  } else {
    loadError.value = result.message ?? 'Failed to load variant'
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? VariantFormMapper.toCreate(values)
    : VariantFormMapper.toUpdate(values)
  const result = id.value
    ? await VariantApi.update(id.value, data)
    : await VariantApi.create(productId.value, data)
  saving.value = false
  if (result.isSuccess) {
    toast.success(id.value ? t('catalog.variants.messages.update_success') : t('catalog.variants.messages.create_success'))
    const newId = result.value.id
    router.replace({ name: ROUTE.VARIANTS.VIEW, params: { productId: productId.value, id: newId } })
  } else {
    toast.error(result.message ?? 'Save failed')
  }
})

function cancel() {
  if (id.value) router.push({ name: ROUTE.VARIANTS.VIEW, params: { productId: productId.value, id: id.value } })
  else router.push({ name: ROUTE.VARIANTS.LIST, params: { productId: productId.value } })
}

function toggleEdit() {
  router.push({ name: ROUTE.VARIANTS.EDIT, params: { productId: productId.value, id: id.value } })
}

const weightUnitOptions = [
  { label: 'G', value: 'G' },
  { label: 'Kg', value: 'Kg' },
  { label: 'Lb', value: 'Lb' },
  { label: 'Oz', value: 'Oz' },
]

const dimensionsUnitOptions = [
  { label: 'Mm', value: 'Mm' },
  { label: 'Cm', value: 'Cm' },
  { label: 'In', value: 'In' },
  { label: 'Ft', value: 'Ft' },
]

onMounted(async () => {
  if (id.value) await loadVariant()
})
</script>

<template>
  <div>
    <PageHeader :title="title" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <button v-if="mode === 'view'" class="p-button p-component" @click="toggleEdit">{{ t('catalog.variants.actions.edit') }}</button>
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadVariant" />
    <div v-else class="card">
      <div class="grid">
        <div class="col-6">
          <FormField :label="t('catalog.variants.labels.sku')" :error="errors.sku" required>
            <input v-model="sku" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField :label="t('catalog.variants.labels.position')" :error="errors.position">
            <input v-model.number="position" type="number" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-6">
          <FormField :label="t('catalog.variants.labels.track_inventory')">
            <div class="flex align-items-center gap-2 mt-1">
              <Checkbox v-model="trackInventory" :binary="true" :disabled="mode === 'view'" input-id="trackInventory" />
              <label for="trackInventory">{{ t('catalog.variants.descriptions.track_inventory') }}</label>
            </div>
          </FormField>
        </div>
        <div class="col-6">
          <FormField :label="t('catalog.variants.labels.is_master')">
            <div class="flex align-items-center gap-2 mt-1">
              <Checkbox v-model="isMaster" :binary="true" :disabled="mode === 'view'" input-id="isMaster" />
              <label for="isMaster">{{ t('catalog.variants.descriptions.is_master') }}</label>
            </div>
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-4">
          <FormField :label="t('catalog.variants.labels.weight')">
            <input v-model.number="weight" type="number" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-2">
          <FormField :label="t('catalog.variants.labels.weight_unit')">
            <Select v-model="weightUnit" :options="weightUnitOptions" option-label="label" option-value="value" class="w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-2">
          <FormField :label="t('catalog.variants.labels.height')">
            <input v-model.number="height" type="number" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-2">
          <FormField :label="t('catalog.variants.labels.width')">
            <input v-model.number="width" type="number" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-2">
          <FormField :label="t('catalog.variants.labels.depth')">
            <input v-model.number="depth" type="number" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-2">
          <FormField :label="t('catalog.variants.labels.dimensions_unit')">
            <Select v-model="dimensionsUnit" :options="dimensionsUnitOptions" option-label="label" option-value="value" class="w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-4">
          <FormField :label="t('catalog.variants.labels.price')">
            <input v-model.number="price" type="number" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-4">
          <FormField :label="t('catalog.variants.labels.cost_price')">
            <input v-model.number="costPrice" type="number" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-2">
          <FormField :label="t('catalog.variants.labels.cost_currency')">
            <input v-model="costCurrency" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>

      <!-- Prices section (Phase 2) -->
      <div class="mt-6 border border-surface-200 dark:border-surface-700 rounded-lg p-4">
        <div class="text-lg font-semibold text-surface-900 dark:text-surface-0 px-2 mb-3">Prices section</div>
      </div>

      <!-- Option Values section (Phase 3) -->
      <div class="mt-6 border border-surface-200 dark:border-surface-700 rounded-lg p-4">
        <div class="text-lg font-semibold text-surface-900 dark:text-surface-0 px-2 mb-3">Option Values section</div>
      </div>

      <!-- Images section (Phase 4) -->
      <div class="mt-6 border border-surface-200 dark:border-surface-700 rounded-lg p-4">
        <div class="text-lg font-semibold text-surface-900 dark:text-surface-0 px-2 mb-3">Images section</div>
      </div>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? t('catalog.variants.actions.save_create') : t('catalog.variants.actions.save_edit')"
        :cancel-label="t('catalog.variants.actions.cancel')"
        @save="save"
        @cancel="cancel"
      />
    </div>
  </div>
</template>
