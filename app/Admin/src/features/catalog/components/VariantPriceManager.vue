<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import Sidebar from 'primevue/sidebar'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { VariantPriceForms } from '../schemas'
import { VariantPriceFormMapper } from '../mappers/variant-price.mapper'
import { VariantPriceApi } from '../api/variant-price.api'
import type { VariantPriceResponse } from '../types'

const props = defineProps<{ variantId: string }>()

const { t } = useI18n()
const toast = useToast()
const { confirmDelete } = useConfirm()

const prices = ref<VariantPriceResponse[]>([])
const loading = ref(false)
const saving = ref(false)
const sidebarVisible = ref(false)
const editingPrice = ref<VariantPriceResponse | null>(null)

const schemas = new VariantPriceForms(t)
const { handleSubmit, defineField, errors, resetForm, setValues } = useForm({
  validationSchema: toTypedSchema(schemas.create()),
})

const [amount] = defineField('amount')
const [currency] = defineField('currency')
const [compareAtAmount] = defineField('compareAtAmount')
const [countryIso] = defineField('countryIso')

async function load() {
  loading.value = true
  const result = await VariantPriceApi.list(props.variantId)
  if (result.isSuccess) {
    prices.value = result.value
  } else {
    toast.error(result.message ?? 'Failed to load prices')
  }
  loading.value = false
}

function openAdd() {
  editingPrice.value = null
  resetForm()
  sidebarVisible.value = true
}

function openEdit(price: VariantPriceResponse) {
  editingPrice.value = price
  setValues({
    amount: price.amount ?? undefined,
    currency: price.currency,
    compareAtAmount: price.compareAtAmount ?? undefined,
    countryIso: price.countryIso ?? undefined,
  })
  sidebarVisible.value = true
}

const submit = handleSubmit(async (values) => {
  saving.value = true
  const data = VariantPriceFormMapper.toCreate(values)
  const result = await VariantPriceApi.set(props.variantId, data)
  saving.value = false
  if (result.isSuccess) {
    toast.success(editingPrice.value ? 'Price updated' : 'Price added')
    sidebarVisible.value = false
    await load()
  } else {
    toast.error(result.message ?? 'Failed to save price')
  }
})

async function remove(price: VariantPriceResponse) {
  confirmDelete({
    target: `price ${price.currency} ${price.amount ?? ''}`,
    onAccept: async () => {
      const result = await VariantPriceApi.remove(props.variantId, price.id)
      if (result.isSuccess) {
        toast.success('Price removed')
        await load()
      } else {
        toast.error(result.message ?? 'Failed to remove price')
      }
    },
  })
}

onMounted(load)
</script>

<template>
  <div>
    <div class="flex justify-content-between align-items-center mb-3">
      <h3 class="m-0">{{ t('catalog.variants.prices.title') }}</h3>
      <Button :label="t('catalog.variants.prices.add')" icon="pi pi-plus" size="small" @click="openAdd" />
    </div>
    <DataTable :value="prices" :loading="loading" striped-rows size="small">
      <Column field="amount" :header="t('catalog.variants.prices.amount')" />
      <Column field="currency" :header="t('catalog.variants.prices.currency')" />
      <Column field="compareAtAmount" :header="t('catalog.variants.prices.compare_at')" />
      <Column field="countryIso" :header="t('catalog.variants.prices.country')" />
      <Column header="">
        <template #body="{ data }">
          <Button icon="pi pi-pencil" size="small" class="p-button-text mr-1" @click="openEdit(data)" />
          <Button icon="pi pi-trash" size="small" class="p-button-text p-button-danger" @click="remove(data)" />
        </template>
      </Column>
    </DataTable>
    <Sidebar v-model:visible="sidebarVisible" :header="editingPrice ? 'Edit Price' : 'Add Price'" position="right">
      <form @submit="submit" class="flex flex-column gap-3">
        <div>
          <label class="block font-medium mb-1">{{ t('catalog.variants.prices.amount') }}</label>
          <input v-model.number="amount" type="number" class="p-inputtext p-component w-full" :invalid="!!errors.amount" />
          <small v-if="errors.amount" class="text-red-500">{{ errors.amount }}</small>
        </div>
        <div>
          <label class="block font-medium mb-1">{{ t('catalog.variants.prices.currency') }}</label>
          <input v-model="currency" type="text" class="p-inputtext p-component w-full" :invalid="!!errors.currency" />
          <small v-if="errors.currency" class="text-red-500">{{ errors.currency }}</small>
        </div>
        <div>
          <label class="block font-medium mb-1">{{ t('catalog.variants.prices.compare_at') }}</label>
          <input v-model.number="compareAtAmount" type="number" class="p-inputtext p-component w-full" :invalid="!!errors.compareAtAmount" />
        </div>
        <div>
          <label class="block font-medium mb-1">{{ t('catalog.variants.prices.country') }}</label>
          <input v-model="countryIso" type="text" class="p-inputtext p-component w-full" maxlength="2" />
        </div>
        <div class="flex justify-content-end gap-2 mt-3">
          <Button type="button" :label="t('catalog.variants.actions.cancel')" class="p-button-secondary" @click="sidebarVisible = false" />
          <Button type="submit" :label="t('catalog.variants.actions.save')" :loading="saving" :disabled="saving" />
        </div>
      </form>
    </Sidebar>
  </div>
</template>
