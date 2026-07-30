<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import TreeTable from 'primevue/treetable'
import Column from 'primevue/column'
import Plus from '@primeicons/vue/plus'
import Card from 'primevue/card'
import { FormSection, FormField } from '@form'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { TaxonomyApi } from '../services/taxonomyApi'
import { TaxonApi } from '../services/taxonApi'
import { taxonomySchema } from '../validations/taxonomy'
import type { TaxonomyForm } from '../validations/taxonomy'
import type { TaxonTreeItem } from '../types/taxon'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const confirm = useConfirm()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit Taxonomy' : 'New Taxonomy')
const pageDescription = computed(() =>
  isEdit.value
    ? 'Edit the details of the taxonomy.'
    : 'Create a new taxonomy by filling out the form below.',
)

const form = ref<TaxonomyForm>({
  name: '',
  presentation: '',
  position: 1,
})

const fieldErrors = ref<Record<string, string>>({})
const saving = ref(false)

const treeNodes = ref<TaxonTreeItem[]>([])
const treeLoading = ref(false)

async function initEditMode(id: string) {
  const result = await TaxonomyApi.getTaxonomy(id)
  if (result.isSuccess) {
    const t = result.value
    form.value = {
      name: t.name,
      presentation: t.presentation,
      position: t.position,
    }
  } else {
    handleResult(result)
    router.push('/catalog/taxonomies')
  }

  await loadTree(id)
}

async function loadTree(taxonomyId: string) {
  treeLoading.value = true
  const result = await TaxonApi.getTree(taxonomyId)
  if (result.isSuccess && result.value?.tree) {
    treeNodes.value = result.value.tree
  }
  treeLoading.value = false
}

onMounted(() => {
  if (isEdit.value) {
    initEditMode(route.params.id as string)
  }
})

watch(() => route.params.id, (newId) => {
  if (newId && newId !== 'new') {
    initEditMode(newId as string)
  }
})

async function onSave() {
  fieldErrors.value = {}
  const parsed = taxonomySchema.safeParse(form.value)

  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      const field = String(issue.path[0])
      if (!fieldErrors.value[field]) {
        fieldErrors.value[field] = issue.message
      }
    }
    return
  }

  saving.value = true
  const data = parsed.data
  const request = {
    name: data.name,
    presentation: data.presentation,
    position: data.position,
  }

  const result = isEdit.value
    ? await TaxonomyApi.updateTaxonomy(route.params.id as string, request)
    : await TaxonomyApi.createTaxonomy(request)

  saving.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Taxonomy updated' : 'Taxonomy created')
    if (!isEdit.value && result.value) {
      const created = result.value
      form.value = {
        name: created.name,
        presentation: created.presentation,
        position: created.position,
      }
      router.replace(`/catalog/taxonomies/${created.id}`)
    }
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/catalog/taxonomies')
}

function navigateToCreateTaxon(parentId: string | null = null) {
  const base = `/catalog/taxons/new?taxonomyId=${route.params.id}`
  router.push(parentId ? `${base}&parentId=${parentId}` : base)
}

function navigateToEditTaxon(id: string) {
  router.push(`/catalog/taxons/${id}`)
}

function confirmDeleteTaxon(node: TaxonTreeItem) {
  confirm.require({
    message: `Are you sure you want to delete "${node.name}"?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const result = await TaxonApi.deleteTaxon(node.id)
      if (result.isSuccess) {
        notify.success('Taxon deleted', `${node.name} has been removed.`)
        if (isEdit.value) {
          await loadTree()
        }
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete taxon.')
      }
    },
  })
}
</script>

<template>
  <!-- Page shell -->
  <Card>
    <template #content>
      <div class="font-semibold text-xl mb-4">{{ pageTitle }}</div>
      <p v-if="pageDescription" class="text-muted-color mb-4">{{ pageDescription }}</p>
    <!-- Page actions -->
    <div class="flex justify-end gap-2 mb-8">
      <Button label="Save" icon="pi pi-check" severity="primary" @click="onSave()" />
      <Button label="Cancel" icon="pi pi-times" severity="secondary" @click="onCancel()" />
    </div>

    <!-- Form section -->
    <FormSection title="Taxonomy Details">
      <FormField label="Name" :required="true" :invalid="!!fieldErrors.name">
        <InputText v-model="form.name" fluid />
        <small v-if="fieldErrors.name" class="text-red-500">{{ fieldErrors.name }}</small>
      </FormField>
      <FormField label="Presentation" :required="true" :invalid="!!fieldErrors.presentation">
        <InputText v-model="form.presentation" fluid />
        <small v-if="fieldErrors.presentation" class="text-red-500">{{ fieldErrors.presentation }}</small>
      </FormField>
      <FormField label="Position" :invalid="!!fieldErrors.position" help-text="Sort order (lower = first)">
        <InputNumber v-model="form.position" fluid :min="-1" />
        <small v-if="fieldErrors.position" class="text-red-500">{{ fieldErrors.position }}</small>
      </FormField>
    </FormSection>

    <!-- Child entity: Taxons tree -->
    <Toolbar v-if="isEdit">
      <template #start>
        <Button label="Add Taxon" severity="secondary" @click="navigateToCreateTaxon()">
          <Plus />
        </Button>
      </template>
    </Toolbar>

    <TreeTable
      v-if="isEdit"
      :value="treeNodes"
      :loading="treeLoading"
      class="mt-0"
    >
      <Column field="name" header="Name" :expander="true" />
      <Column field="slug" header="Slug" />
      <Column field="position" header="Position" />
      <Column field="childrenCount" header="Children" />
      <Column field="taxonRuleCount" header="Rules" />
      <Column field="productCount" header="Products" />
      <Column header="" body-style="text-align: right; width: 6rem">
        <template #body="{ node }">
          <div class="flex justify-end gap-2">
            <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEditTaxon(node.data.id)" />
            <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="confirmDeleteTaxon(node.data)" />
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="text-center py-8 text-muted-color">No taxons defined. Add one to start building your category tree.</div>
      </template>
    </TreeTable>
    </template>
  </Card>
</template>
