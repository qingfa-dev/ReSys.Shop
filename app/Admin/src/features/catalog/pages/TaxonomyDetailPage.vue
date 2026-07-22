<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
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
import { getTaxonomy, createTaxonomy, updateTaxonomy, getTaxons, createTaxon, updateTaxon, deleteTaxon } from '../api/taxonomies'
import type { TaxonomyRequest, TaxonResponse, TaxonRequest } from '../models/Taxonomy'

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { confirmDelete } = useConfirm()

const id = computed(() => route.params.id as string | undefined)
const mode = computed<'create' | 'view' | 'edit'>(() => {
  if (!id.value) return 'create'
  if (route.name?.toString().endsWith('.edit')) return 'edit'
  return 'view'
})

const loading = ref(false)
const saving = ref(false)
const error = ref<string | null>(null)
const form = ref<TaxonomyRequest>({ name: '', presentation: null })
const formErrors = ref<Record<string, string>>({})

const taxons = ref<TaxonResponse[]>([])
const taxonsLoading = ref(false)

const taxonSlideoverVisible = ref(false)
const editingTaxon = ref<TaxonResponse | null>(null)
const taxonForm = ref<TaxonRequest>({ name: '', presentation: null })
const taxonSaving = ref(false)

const title = computed(() => {
  if (mode.value === 'create') return 'Create Taxonomy'
  if (mode.value === 'edit') return `Edit: ${form.value.name || 'Taxonomy'}`
  return form.value.name || 'Taxonomy Detail'
})

function validate(): boolean {
  formErrors.value = {}
  if (!form.value.name.trim()) formErrors.value.name = 'Required'
  return Object.keys(formErrors.value).length === 0
}

async function loadTaxonomy() {
  if (!id.value) return
  loading.value = true; error.value = null
  const result = await getTaxonomy(id.value)
  if (result.success) { form.value = { name: result.data.name, presentation: result.data.presentation } }
  else { error.value = result.error?.message ?? 'Failed to load taxonomy' }
  loading.value = false
}

async function loadTaxons() {
  if (!id.value) return
  taxonsLoading.value = true
  const result = await getTaxons(id.value)
  if (result.success) { taxons.value = result.data }
  taxonsLoading.value = false
}

async function save() {
  if (!validate()) return
  saving.value = true
  const data: TaxonomyRequest = { ...form.value }
  const result = id.value ? await updateTaxonomy(id.value, data) : await createTaxonomy(data)
  saving.value = false
  if (result.success) {
    toast.success(id.value ? 'Taxonomy updated' : 'Taxonomy created')
    if (mode.value === 'create') {
      router.replace({ name: 'catalog.taxonomies.view', params: { id: result.data.id } })
    } else {
      router.replace({ name: 'catalog.taxonomies.view', params: { id: id.value } })
    }
  } else { toast.error(result.error?.message ?? 'Save failed') }
}

function cancel() {
  if (id.value) router.push({ name: 'catalog.taxonomies.view', params: { id: id.value } })
  else router.push({ name: 'catalog.taxonomies.list' })
}

function toggleEdit() { router.push({ name: 'catalog.taxonomies.edit', params: { id: id.value } }) }

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
    ? await updateTaxon(id.value, editingTaxon.value.id, data)
    : await createTaxon(id.value, data)
  taxonSaving.value = false
  if (result.success) {
    toast.success(editingTaxon.value ? 'Taxon updated' : 'Taxon created')
    taxonSlideoverVisible.value = false
    await loadTaxons()
  } else { toast.error(result.error?.message ?? 'Save failed') }
}

function confirmDeleteTaxon(taxon: TaxonResponse) {
  confirmDelete({
    target: 'this taxon',
    onAccept: () => deleteTaxonAction(taxon),
  })
}

async function deleteTaxonAction(taxon: TaxonResponse) {
  if (!id.value) return
  const result = await deleteTaxon(id.value, taxon.id)
  if (result.success) {
    toast.success('Taxon deleted')
    await loadTaxons()
  } else { toast.error(result.error?.message ?? 'Delete failed') }
}

onMounted(async () => {
  await loadTaxonomy()
  if (id.value) await loadTaxons()
})
</script>

<template>
  <div>
    <PageHeader :title="title">
      <template #actions>
        <button v-if="mode === 'view'" class="p-button p-component" @click="toggleEdit">Edit</button>
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="error" :title="error" @retry="loadTaxonomy" />
    <div v-else class="card">
      <div class="grid">
        <div class="col-6">
          <FormField label="Name" :error="formErrors.name" required>
            <input v-model="form.name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Presentation">
            <input v-model="form.presentation" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
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
