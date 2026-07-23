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
import DataTable from '@/shared/components/data/DataTable.vue'
import Column from 'primevue/column'
import Sidebar from 'primevue/sidebar'
import Button from 'primevue/button'
import { useToast } from '@/shared/composables/useToast'
import { useConfirm } from '@/shared/composables/useConfirm'
import { CatalogForms } from '../schemas'
import { TaxonomyFormMapper } from '../mappers/taxonomy.mapper'
import { TaxonomyApi } from '../api'
import type { TaxonResponse, TaxonRequest } from '../types'
import { ROUTE } from '../routes'

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { confirmDelete } = useConfirm()
const { t } = useI18n()

const id = computed(() => route.params.id as string | undefined)
const mode = computed<'create' | 'view' | 'edit'>(() => {
  if (!id.value) return 'create'
  if (route.name?.toString().endsWith('.edit')) return 'edit'
  return 'view'
})

const schemas = new CatalogForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(
    mode.value === 'create' ? schemas.createTaxonomy() : schemas.updateTaxonomy(),
  ),
})

const [name] = defineField('name')
const [presentation] = defineField('presentation')

const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const taxons = ref<TaxonResponse[]>([])
const taxonsLoading = ref(false)

const taxonSlideoverVisible = ref(false)
const editingTaxon = ref<TaxonResponse | null>(null)
const taxonForm = ref<TaxonRequest>({ name: '', presentation: null })
const taxonSaving = ref(false)

const title = computed(() => {
  if (mode.value === 'create') return 'Create Taxonomy'
  if (mode.value === 'edit') return `Edit: ${name.value || 'Taxonomy'}`
  return name.value || 'Taxonomy Detail'
})

async function loadTaxonomy() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  const result = await TaxonomyApi.get(id.value)
  if (result.isSuccess) {
    setValues({ name: result.value.name, presentation: result.value.presentation })
  } else {
    loadError.value = result.message ?? 'Failed to load taxonomy'
  }
  loading.value = false
}

async function loadTaxons() {
  if (!id.value) return
  taxonsLoading.value = true
  const result = await TaxonomyApi.getTaxons(id.value)
  if (result.isSuccess) { taxons.value = result.value }
  taxonsLoading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? TaxonomyFormMapper.toCreate(values)
    : TaxonomyFormMapper.toUpdate(values)
  const result = id.value
    ? await TaxonomyApi.update(id.value, data)
    : await TaxonomyApi.create(data)
  saving.value = false
  if (result.isSuccess) {
    toast.success(id.value ? t('catalog.taxonomy.updated') : t('catalog.taxonomy.created'))
    const newId = result.value.id
    router.replace({ name: ROUTE.TAXONOMIES.VIEW, params: { id: newId } })
  } else {
    toast.error(result.message ?? 'Save failed')
  }
})

function cancel() {
  if (id.value) router.push({ name: ROUTE.TAXONOMIES.VIEW, params: { id: id.value } })
  else router.push({ name: ROUTE.TAXONOMIES.LIST })
}

function toggleEdit() {
  router.push({ name: ROUTE.TAXONOMIES.EDIT, params: { id: id.value } })
}

function openAddTaxon() {
  editingTaxon.value = null
  taxonForm.value = { name: '', presentation: null }
  taxonSlideoverVisible.value = true
}

function openEditTaxon(taxon: TaxonResponse) {
  editingTaxon.value = taxon
  taxonForm.value = { name: taxon.name, presentation: taxon.presentation }
  taxonSlideoverVisible.value = true
}

async function saveTaxon() {
  if (!taxonForm.value.name.trim() || !id.value) return
  taxonSaving.value = true
  const data: TaxonRequest = { name: taxonForm.value.name, presentation: taxonForm.value.presentation || null }
  const result = editingTaxon.value
    ? await TaxonomyApi.updateTaxon(id.value, editingTaxon.value.id, data)
    : await TaxonomyApi.createTaxon(id.value, data)
  taxonSaving.value = false
  if (result.isSuccess) {
    toast.success(editingTaxon.value ? 'Taxon updated' : 'Taxon created')
    taxonSlideoverVisible.value = false
    await loadTaxons()
  } else { toast.error(result.message ?? 'Save failed') }
}

function confirmDeleteTaxon(taxon: TaxonResponse) {
  confirmDelete({
    target: 'this taxon',
    onAccept: () => deleteTaxonAction(taxon),
  })
}

async function deleteTaxonAction(taxon: TaxonResponse) {
  if (!id.value) return
  const result = await TaxonomyApi.deleteTaxon(id.value, taxon.id)
  if (result.isSuccess) {
    toast.success('Taxon deleted')
    await loadTaxons()
  } else { toast.error(result.message ?? 'Delete failed') }
}

onMounted(async () => {
  await loadTaxonomy()
  if (id.value) await loadTaxons()
})
</script>

<template>
  <div>
    <PageHeader :title="title" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <button v-if="mode === 'view'" class="p-button p-component" @click="toggleEdit">Edit</button>
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadTaxonomy" />
    <div v-else class="card">
      <div class="grid">
        <div class="col-6">
          <FormField label="Name" :error="errors.name" required>
            <input v-model="name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Presentation">
            <input v-model="presentation" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>

      <fieldset v-if="id" class="mt-6 border border-surface-200 dark:border-surface-700 rounded-lg p-4">
        <legend class="text-lg font-semibold text-surface-900 dark:text-surface-0 px-2">Taxons</legend>
        <div class="flex justify-end mb-3">
          <Button label="Add Taxon" icon="pi pi-plus" size="small" @click="openAddTaxon" />
        </div>
        <DataTable
          :rows="taxons"
          :loading="taxonsLoading"
          empty-title="No taxons"
          empty-description="Add a taxon to get started."
        >
          <Column field="name" header="Name">
            <template #body="{ data }">
              <span :style="{ paddingLeft: data.depth * 1.5 + 'rem' }">{{ data.name }}</span>
            </template>
          </Column>
          <Column field="presentation" header="Presentation" />
          <Column field="slug" header="Slug" />
          <Column header="Children">
            <template #body="{ data }">
              {{ data.childrenCount }}
            </template>
          </Column>
          <template #rowActions="{ data }">
            <div class="flex gap-1">
              <Button icon="pi pi-pencil" severity="secondary" text rounded size="small" @click="openEditTaxon(data)" />
              <Button icon="pi pi-trash" severity="danger" text rounded size="small" @click="confirmDeleteTaxon(data)" />
            </div>
          </template>
        </DataTable>
      </fieldset>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? 'Create Taxonomy' : 'Save Changes'"
        cancel-label="Cancel"
        @save="save"
        @cancel="cancel"
      />
    </div>

    <Sidebar v-model:visible="taxonSlideoverVisible" header="Taxon" position="right" class="w-full sm:w-96">
      <div class="flex flex-col gap-4">
        <FormField label="Name" required>
          <input v-model="taxonForm.name" type="text" class="p-inputtext p-component w-full" />
        </FormField>
        <FormField label="Presentation">
          <input v-model="taxonForm.presentation" type="text" class="p-inputtext p-component w-full" />
        </FormField>
        <div class="flex gap-2 justify-end mt-4">
          <Button label="Cancel" severity="secondary" text @click="taxonSlideoverVisible = false" />
          <Button :label="editingTaxon ? 'Update' : 'Create'" icon="pi pi-check" :loading="taxonSaving" @click="saveTaxon" />
        </div>
      </div>
    </Sidebar>
  </div>
</template>
