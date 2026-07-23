<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useToast } from '@/shared/composables/useToast'
import { ProductForms } from '../schemas'
import { ProductFormMapper } from '../mappers/product.mapper'
import { ProductApi } from '../api'
import { ROUTE } from '../routes'

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { t } = useI18n()

const id = computed(() => route.params.id as string | undefined)
const mode = computed<'create' | 'view' | 'edit'>(() => {
  if (!id.value) return 'create'
  if (route.name?.toString().endsWith('.edit')) return 'edit'
  return 'view'
})

const schemas = new ProductForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(
    mode.value === 'create' ? schemas.create() : schemas.update(),
  ),
})

const [name] = defineField('name')
const [slug] = defineField('slug')
const [description] = defineField('description')
const [status] = defineField('status')
const [department] = defineField('department')
const [genderTarget] = defineField('genderTarget')
const [styleCode] = defineField('styleCode')

const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const title = computed(() => {
  if (mode.value === 'create') return t('catalog.products.titles.create')
  if (mode.value === 'edit') return `${t('catalog.products.actions.edit')}: ${name.value || ''}`
  return name.value || t('catalog.products.titles.edit')
})

async function load() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  const result = await ProductApi.get(id.value)
  if (result.isSuccess) {
    setValues({
      name: result.value.name,
      slug: result.value.slug,
      description: result.value.description ?? undefined,
      status: result.value.status,
      department: result.value.department ?? undefined,
      genderTarget: result.value.genderTarget ?? undefined,
      styleCode: result.value.styleCode ?? undefined,
    })
  } else {
    loadError.value = result.message ?? 'Failed to load product'
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? ProductFormMapper.toCreate(values)
    : ProductFormMapper.toUpdate(values)
  const result = id.value
    ? await ProductApi.update(id.value, data)
    : await ProductApi.create(data)
  saving.value = false
  if (result.isSuccess) {
    toast.success(id.value ? t('catalog.products.messages.update_success') : t('catalog.products.messages.create_success'))
    const newId = result.value.id
    router.replace({ name: ROUTE.PRODUCTS.VIEW, params: { id: newId } })
  } else {
    toast.error(result.message ?? 'Save failed')
  }
})

function cancel() {
  if (id.value) router.push({ name: ROUTE.PRODUCTS.VIEW, params: { id: id.value } })
  else router.push({ name: ROUTE.PRODUCTS.LIST })
}

function toggleEdit() {
  router.push({ name: ROUTE.PRODUCTS.EDIT, params: { id: id.value } })
}

onMounted(() => { load() })
</script>

<template>
  <div>
    <PageHeader :title="title" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <button v-if="mode === 'view'" class="p-button p-component" @click="toggleEdit">{{
          t('catalog.products.actions.edit') }}</button>
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="load" />
    <div v-else class="card">
      <div class="grid">
        <div class="col-6">
          <FormField :label="t('catalog.products.labels.name')" :error="errors.name" required>
            <input v-model="name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField :label="t('catalog.products.labels.slug')" :error="errors.slug" required>
            <input v-model="slug" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField :label="t('catalog.products.labels.status')">
            <select v-model="status" class="p-inputtext p-component w-full" :disabled="mode === 'view'">
              <option value="Draft">Draft</option>
              <option value="Active">Active</option>
              <option value="Archived">Archived</option>
            </select>
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Department">
            <input v-model="department" type="text" class="p-inputtext p-component w-full"
              :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Gender Target">
            <input v-model="genderTarget" type="text" class="p-inputtext p-component w-full"
              :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Style Code">
            <input v-model="styleCode" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-12">
          <FormField :label="t('catalog.products.labels.description')">
            <textarea v-model="description" rows="4" class="p-inputtext p-component w-full"
              :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <FormActions v-if="mode !== 'view'" :loading="saving" :save-label="t('catalog.products.actions.save')"
        :cancel-label="t('catalog.products.actions.cancel')" @save="save" @cancel="cancel" />
    </div>
  </div>
</template>
