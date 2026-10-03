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
  if (mode.value === 'create') return t('shipping.rates.form.create_title')
  if (mode.value === 'edit') return t('shipping.rates.form.edit_title', { name: name.value || '' })
  return name.value || t('shipping.rates.form.view_title')
})

async function loadShippingRate() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  try {
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
      loadError.value = result.message ?? t('shipping.rates.messages.load_failed')
    }
  } catch (err) {
    console.error(err)
    loadError.value = t('shipping.rates.messages.load_failed')
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? ShippingRateFormMapper.toCreate(values)
    : ShippingRateFormMapper.toUpdate(values)
  try {
    const result = id.value
      ? await api.update(id.value, data)
      : await api.create(data)
    saving.value = false
    if (result.isSuccess) {
      toast.success(id.value
        ? t('shipping.rates.messages.update_success')
        : t('shipping.rates.messages.create_success'))
      const newId = result.value.id
      router.replace({ name: ROUTE.RATES.VIEW, params: { id: newId } })
    } else {
      toast.error(result.message ?? t('shipping.rates.messages.save_failed'))
    }
  } catch (err) {
    console.error(err)
    saving.value = false
    toast.error(t('shipping.rates.messages.save_failed'))
  }
})

function cancel() {
  if (id.value) router.push({ name: ROUTE.RATES.VIEW, params: { id: id.value } })
  else router.push({ name: ROUTE.RATES.LIST })
}

function toggleEdit() {
  router.push({ name: ROUTE.RATES.EDIT, params: { id: id.value } })
}

onMounted(async () => {
  await loadShippingRate()
})
</script>

<template>
  <div>
    <PageHeader :title="title" :subtitle="t('shipping.rates.form.subtitle')" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <Button
          v-if="mode === 'view'"
          :label="t('shipping.rates.actions.edit')"
          icon="pi pi-pencil"
          size="small"
          @click="toggleEdit"
        />
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadShippingRate" />
    <AppCard v-else>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('shipping.rates.labels.name')" :error="errors.name" required>
            <input v-model="name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('shipping.rates.labels.shipping_method_id')" :error="errors.shippingMethodId" required>
            <input v-model="shippingMethodId" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('shipping.rates.labels.rate')" :error="errors.rate" required>
            <input v-model.number="rate" type="number" step="0.01" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('shipping.rates.labels.currency')" :error="errors.currency" required>
            <input v-model="currency" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" placeholder="e.g. USD" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('shipping.rates.labels.min_order_amount')" :error="errors.minOrderAmount">
            <input v-model.number="minOrderAmount" type="number" step="0.01" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('shipping.rates.labels.max_order_amount')" :error="errors.maxOrderAmount">
            <input v-model.number="maxOrderAmount" type="number" step="0.01" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('shipping.rates.labels.min_weight')" :error="errors.minWeight">
            <input v-model.number="minWeight" type="number" step="0.01" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('shipping.rates.labels.max_weight')" :error="errors.maxWeight">
            <input v-model.number="maxWeight" type="number" step="0.01" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? t('shipping.rates.actions.save_create') : t('shipping.rates.actions.save_edit')"
        :cancel-label="t('shipping.rates.actions.cancel')"
        @save="save"
        @cancel="cancel"
      />
    </AppCard>
  </div>
</template>
