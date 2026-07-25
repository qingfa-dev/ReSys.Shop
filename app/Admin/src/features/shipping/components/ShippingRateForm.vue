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
import { useShippingRate } from '../composables/useShippingRate'
import { ShippingRateForms } from '../schemas'
import { ShippingRateFormMapper } from '../mappers/shipping-rate.mapper'
import { ROUTE } from '../routes'

const { id, mode, route, router, toast, api } = useShippingRate()
const { t } = useI18n()

const schemas = new ShippingRateForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(
    mode.value === 'create' ? schemas.create() : schemas.update(),
  ),
})

const [name] = defineField('name')
const [shippingMethodId] = defineField('shippingMethodId')
const [rate] = defineField('rate')
const [currency] = defineField('currency')
const [minOrderAmount] = defineField('minOrderAmount')
const [maxOrderAmount] = defineField('maxOrderAmount')
const [minWeight] = defineField('minWeight')
const [maxWeight] = defineField('maxWeight')

const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const title = computed(() => {
  if (mode.value === 'create') return 'Create Shipping Rate'
  if (mode.value === 'edit') return `Edit: ${name.value || ''}`
  return name.value || 'Shipping Rate'
})

async function loadShippingRate() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  const result = await api.get(id.value)
  if (result.isSuccess) {
    setValues({
      name: result.value.name,
      shippingMethodId: result.value.shippingMethodId,
      rate: result.value.rate,
      currency: result.value.currency,
      minOrderAmount: result.value.minOrderAmount ?? undefined,
      maxOrderAmount: result.value.maxOrderAmount ?? undefined,
      minWeight: result.value.minWeight ?? undefined,
      maxWeight: result.value.maxWeight ?? undefined,
    })
  } else {
    loadError.value = result.message ?? 'Failed to load shipping rate'
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? ShippingRateFormMapper.toCreate(values)
    : ShippingRateFormMapper.toUpdate(values)
  const result = id.value
    ? await api.update(id.value, data)
    : await api.create(data)
  saving.value = false
  if (result.isSuccess) {
    toast.success(id.value ? 'Shipping rate updated' : 'Shipping rate created')
    const newId = result.value.id
    router.replace({ name: ROUTE.RATES.VIEW, params: { id: newId } })
  } else {
    toast.error(result.message ?? 'Save failed')
  }
})

function cancel() {
  if (id.value) router.push({ name: ROUTE.RATES.VIEW, params: { id: id.value } })
  else router.push({ name: ROUTE.RATES.LIST })
}

function toggleEdit() {
  router.push({ name: ROUTE.RATES.EDIT, params: { id: id.value } })
}

onMounted(loadShippingRate)
</script>

<template>
  <div>
    <PageHeader :title="title" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <button v-if="mode === 'view'" class="p-button p-component" @click="toggleEdit">Edit</button>
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadShippingRate" />
    <div v-else class="card">
      <div class="grid">
        <div class="col-6">
          <FormField label="Name" :error="errors.name" required>
            <input v-model="name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Shipping Method ID" :error="errors.shippingMethodId" required>
            <input v-model="shippingMethodId" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-6">
          <FormField label="Rate" :error="errors.rate" required>
            <input v-model.number="rate" type="number" step="0.01" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Currency" :error="errors.currency" required>
            <input v-model="currency" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" placeholder="e.g. USD" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-4">
          <FormField label="Min Order Amount" :error="errors.minOrderAmount">
            <input v-model.number="minOrderAmount" type="number" step="0.01" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-4">
          <FormField label="Max Order Amount" :error="errors.maxOrderAmount">
            <input v-model.number="maxOrderAmount" type="number" step="0.01" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-4">
          <FormField label="Min Weight" :error="errors.minWeight">
            <input v-model.number="minWeight" type="number" step="0.01" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-4">
          <FormField label="Max Weight" :error="errors.maxWeight">
            <input v-model.number="maxWeight" type="number" step="0.01" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? 'Create' : 'Save'"
        cancel-label="Cancel"
        @save="save"
        @cancel="cancel"
      />
    </div>
  </div>
</template>
