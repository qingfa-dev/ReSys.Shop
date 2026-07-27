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
import Checkbox from 'primevue/checkbox'
import { useStockLocation } from '../composables/useStockLocation'
import { StockLocationForms } from '../schemas'
import { StockLocationFormMapper } from '../mappers/stock-location.mapper'
import { ROUTE } from '../routes'

const { id, mode, route, router, toast, api } = useStockLocation()
const { t } = useI18n()

const schemas = new StockLocationForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(
    mode.value === 'create' ? schemas.create() : schemas.update(),
  ),
})

const [name] = defineField('name')
const [code] = defineField('code')
const [address1] = defineField('address1')
const [address2] = defineField('address2')
const [city] = defineField('city')
const [state] = defineField('state')
const [postalCode] = defineField('postalCode')
const [country] = defineField('country')
const [phone] = defineField('phone')
const [isDefault] = defineField('isDefault')
const [isActive] = defineField('isActive')

const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const title = computed(() => {
  if (mode.value === 'create') return t('inventory.locations.titles.create')
  if (mode.value === 'edit') return `${t('inventory.locations.actions.edit')}: ${name.value || ''}`
  return name.value || t('inventory.locations.titles.view')
})

async function loadStockLocation() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  try {
    const result = await api.get(id.value)
    if (result.isSuccess) {
      setValues({
        name: result.value.name,
        code: result.value.code,
        address1: result.value.address1 ?? undefined,
        address2: result.value.address2 ?? undefined,
        city: result.value.city ?? undefined,
        state: result.value.state ?? undefined,
        postalCode: result.value.postalCode ?? undefined,
        country: result.value.country ?? undefined,
        phone: result.value.phone ?? undefined,
        isDefault: result.value.isDefault ?? undefined,
        isActive: result.value.isActive ?? undefined,
      })
    } else {
      loadError.value = result.message ?? 'Failed to load stock location'
    }
  } catch (err) {
    console.error(err)
    loadError.value = 'Failed to load stock location'
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? StockLocationFormMapper.toCreate(values)
    : StockLocationFormMapper.toUpdate(values)
  try {
    const result = id.value
      ? await api.update(id.value, data)
      : await api.create(data)
    saving.value = false
    if (result.isSuccess) {
      toast.success(id.value ? t('inventory.locations.messages.update_success') : t('inventory.locations.messages.create_success'))
      const newId = result.value.id
      router.replace({ name: ROUTE.LOCATIONS.VIEW, params: { id: newId } })
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
  if (id.value) router.push({ name: ROUTE.LOCATIONS.VIEW, params: { id: id.value } })
  else router.push({ name: ROUTE.LOCATIONS.LIST })
}

function toggleEdit() {
  router.push({ name: ROUTE.LOCATIONS.EDIT, params: { id: id.value } })
}

onMounted(async () => {
  await loadStockLocation()
})
</script>

<template>
  <div>
    <PageHeader :title="title" :subtitle="t('inventory.locations.subtitle')" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <Button
          v-if="mode === 'view'"
          :label="t('inventory.locations.actions.edit')"
          icon="pi pi-pencil"
          size="small"
          @click="toggleEdit"
        />
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadStockLocation" />
    <AppCard v-else>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('inventory.locations.labels.name')" :error="errors.name" required>
            <input v-model="name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('inventory.locations.labels.code')" :error="errors.code" required>
            <input v-model="code" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('inventory.locations.labels.address1')" :error="errors.address1">
            <input v-model="address1" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('inventory.locations.labels.address2')" :error="errors.address2">
            <input v-model="address2" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('inventory.locations.labels.city')" :error="errors.city">
            <input v-model="city" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('inventory.locations.labels.state')" :error="errors.state">
            <input v-model="state" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('inventory.locations.labels.postal_code')" :error="errors.postalCode">
            <input v-model="postalCode" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('inventory.locations.labels.country')" :error="errors.country">
            <input v-model="country" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('inventory.locations.labels.phone')" :error="errors.phone">
            <input v-model="phone" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('inventory.locations.labels.is_default')">
            <div class="flex items-center gap-2 mt-1">
              <Checkbox v-model="isDefault" :binary="true" :disabled="mode === 'view'" input-id="isDefault" />
              <label for="isDefault">{{ t('inventory.locations.descriptions.is_default') }}</label>
            </div>
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('inventory.locations.labels.is_active')">
            <div class="flex items-center gap-2 mt-1">
              <Checkbox v-model="isActive" :binary="true" :disabled="mode === 'view'" input-id="isActive" />
              <label for="isActive">{{ t('inventory.locations.descriptions.is_active') }}</label>
            </div>
          </FormField>
        </div>
      </div>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? t('inventory.locations.actions.save_create') : t('inventory.locations.actions.save_edit')"
        :cancel-label="t('inventory.locations.actions.cancel')"
        @save="save"
        @cancel="cancel"
      />
    </AppCard>
  </div>
</template>
