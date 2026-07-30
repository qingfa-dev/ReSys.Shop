<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import TreeTable from 'primevue/treetable'
import Column from 'primevue/column'
import Plus from '@primeicons/vue/plus'
import Card from 'primevue/card'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { TaxonomyApi } from '../services/taxonomyApi'
import { TaxonApi } from '../services/taxonApi'
import { taxonomySchema, taxonomyName, taxonomyPresentation, taxonomyPosition } from '../validations/taxonomy'
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

const taxonomyResolver = zodResolver(taxonomySchema)
const nameResolver = zodResolver(taxonomyName)
const presentationResolver = zodResolver(taxonomyPresentation)
const positionResolver = zodResolver(taxonomyPosition)
const saving = ref(false)
const formLoaded = ref(!isEdit.value)

const treeNodes = ref<any[]>([])
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
    formLoaded.value = true
  } else {
    handleResult(result)
    router.push('/catalog/taxonomies')
  }

  await loadTree(id)
}

function addTreeNodeKeys(nodes: any[]): any[] {
  return nodes.map(n => ({
    ...n,
    key: n.id,
    children: n.children ? addTreeNodeKeys(n.children) : [],
  }))
}

async function loadTree(taxonomyId: string) {
  treeLoading.value = true
  const result = await TaxonApi.getTree(taxonomyId)
  if (result.isSuccess && result.value?.tree) {
    treeNodes.value = addTreeNodeKeys(result.value.tree) as any
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
    initEditMode(newId as string).then(() => {
      formLoaded.value = true
    })
  }
})

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return

  saving.value = true
  const data = event.values as TaxonomyForm
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
          await loadTree(route.params.id as string)
        }
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete taxon.')
      }
    },
  })
}
</script>

<template>
  <div class="flex flex-col h-full p-4">
    <div class="flex-none flex justify-between items-start gap-4 mb-4">
      <div>
        <div class="font-semibold text-xl">{{ pageTitle }}</div>
        <p v-if="pageDescription" class="text-muted-color mt-1">{{ pageDescription }}</p>
      </div>
      <div class="flex items-center gap-2 shrink-0">
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="saving" form="taxonomy-form" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </div>
    </div>

    <div class="flex-1 min-h-0 overflow-auto">
      <Card>
        <template #content>
          <Form id="taxonomy-form" v-slot="$form" :key="String(formLoaded)" :resolver="taxonomyResolver" :initial-values="form" class="flex flex-col gap-4" @submit="onSubmit">
              <FormField v-slot="$field" name="name" :resolver="nameResolver" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Name <span class="text-red-500">*</span></label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="presentation" :resolver="presentationResolver" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Presentation <span class="text-red-500">*</span></label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="position" :resolver="positionResolver" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Position</label>
                <InputNumber fluid :min="-1" />
                <small class="text-muted-color">Sort order (lower = first)</small>
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
            </Form>
        </template>
      </Card>

      <Toolbar v-if="isEdit" class="mb-4 mt-4">
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
    </div>
  </div>
</template>
