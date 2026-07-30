<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Plus from '@primeicons/vue/plus'
import Card from 'primevue/card'
import { Form } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useTaxonomyStore } from '../stores/taxonomyStore'
import { TaxonApi } from '../services/taxonApi'
import { TaxonRuleApi } from '../services/taxonRuleApi'
import { taxonSchema } from '../validations/taxon'
import type { TaxonForm } from '../validations/taxon'
import type { TaxonRuleListItem } from '../types/taxonRule'
import { TAXON_SORT_ORDERS, TAXON_MATCH_POLICIES } from '../types/taxon'
import TaxonRuleFormDialog from '../components/TaxonRuleFormDialog.vue'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const confirm = useConfirm()
const { handleResult } = useApiErrorHandler()
const taxonomyStore = useTaxonomyStore()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit Taxon' : 'New Taxon')
const pageDescription = computed(() =>
  isEdit.value
    ? 'Edit the details of the taxon.'
    : 'Create a new taxon by filling out the form below.',
)
const activeTab = ref('0')

const resolver = zodResolver(taxonSchema)

const form = ref<TaxonForm>({
  taxonomyId: (route.query.taxonomyId as string) || '',
  parentId: (route.query.parentId as string) || null,
  name: '',
  presentation: '',
  slug: '',
  description: null,
  position: 0,
  metaTitle: null,
  metaDescription: null,
  metaKeywords: null,
  imageUrl: null,
  squareImageUrl: null,
  automatic: false,
  rulesMatchPolicy: 'All',
  sortOrder: 'Manual',
  hideFromNav: false,
})

const loading = ref(false)

const parentOptions = ref<{ label: string; value: string }[]>([])
const dialogVisible = ref(false)
const editingRule = ref<TaxonRuleListItem | null>(null)

const {
  items: rules,
  loading: rulesLoading,
  refresh: refreshRules,
} = usePagedQuery<TaxonRuleListItem>('', {
  allowedFilterFields: [],
  allowedSortFields: [],
  defaultPageSize: 100,
})

async function initEditMode(id: string) {
  const result = await TaxonApi.getTaxon(id)
  if (result.isSuccess) {
    const t = result.value
    form.value = {
      taxonomyId: t.taxonomyId,
      parentId: t.parentId,
      name: t.name,
      presentation: t.presentation,
      slug: t.slug,
      description: t.description,
      position: t.position,
      metaTitle: t.metaTitle,
      metaDescription: t.metaDescription,
      metaKeywords: t.metaKeywords,
      imageUrl: t.imageUrl,
      squareImageUrl: t.squareImageUrl,
      automatic: t.automatic,
      rulesMatchPolicy: t.rulesMatchPolicy,
      sortOrder: t.sortOrder,
      hideFromNav: t.hideFromNav,
    }

    await Promise.all([loadParents(result.value.taxonomyId), loadRules(id)])
  } else {
    handleResult(result)
    router.push('/catalog/taxons')
  }
}

async function loadParents(taxonomyId: string) {
  const result = await TaxonApi.getTree(taxonomyId)
  if (result.isSuccess && result.value?.tree) {
    const flat: { label: string; value: string }[] = [{ label: '(None — root level)', value: '' }]
    function walk(nodes: any[], depth: number) {
      for (const n of nodes) {
        flat.push({ label: '  '.repeat(depth) + '|-- ' + n.name, value: n.id })
        if (n.children?.length) walk(n.children, depth + 1)
      }
    }
    walk(result.value.tree, 1)
    parentOptions.value = flat
  }
}

async function loadRules(taxonId: string) {
  const result = await TaxonRuleApi.getRules(taxonId)
  if (result.isSuccess) {
    rules.value = result.items
  }
}

onMounted(async () => {
  await taxonomyStore.fetchActive()
  if (isEdit.value) {
    await initEditMode(route.params.id as string)
  } else if (form.value.taxonomyId) {
    await loadParents(form.value.taxonomyId)
  }
})

watch(() => route.params.id, (newId) => {
  if (newId && newId !== 'new') {
    initEditMode(newId as string)
  }
})

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return

  const data = event.values as TaxonForm
  loading.value = true

  const request = {
    taxonomyId: data.taxonomyId,
    parentId: data.parentId || null,
    name: data.name,
    presentation: data.presentation,
    slug: data.slug,
    description: data.description ?? null,
    position: data.position,
    metaTitle: data.metaTitle ?? null,
    metaDescription: data.metaDescription ?? null,
    metaKeywords: data.metaKeywords ?? null,
    imageUrl: data.imageUrl ?? null,
    squareImageUrl: data.squareImageUrl ?? null,
    automatic: data.automatic,
    rulesMatchPolicy: data.rulesMatchPolicy,
    sortOrder: data.sortOrder,
    hideFromNav: data.hideFromNav,
  }

  const result = isEdit.value
    ? await TaxonApi.updateTaxon(route.params.id as string, request)
    : await TaxonApi.createTaxon(request)

  loading.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Taxon updated' : 'Taxon created')
    if (!isEdit.value && result.value) {
      const created = result.value
      form.value = {
        ...form.value,
        taxonomyId: created.taxonomyId,
        parentId: created.parentId,
        name: created.name,
        presentation: created.presentation,
        slug: created.slug,
        description: created.description,
        position: created.position,
        metaTitle: created.metaTitle,
        metaDescription: created.metaDescription,
        metaKeywords: created.metaKeywords,
        imageUrl: created.imageUrl,
        squareImageUrl: created.squareImageUrl,
        automatic: created.automatic,
        rulesMatchPolicy: created.rulesMatchPolicy,
        sortOrder: created.sortOrder,
        hideFromNav: created.hideFromNav,
      }
      router.replace(`/catalog/taxons/${created.id}`)
    }
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/catalog/taxons')
}

function openAddRule() {
  editingRule.value = null
  dialogVisible.value = true
}

function openEditRule(rule: TaxonRuleListItem) {
  editingRule.value = rule
  dialogVisible.value = true
}

function onRuleSaved() {
  refreshRules()
  loadRules(route.params.id as string)
}

function confirmDeleteRule(rule: TaxonRuleListItem) {
  confirm.require({
    message: `Are you sure you want to delete this rule?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const result = await TaxonRuleApi.deleteRule(route.params.id as string, rule.id)
      if (result.isSuccess) {
        notify.success('Rule deleted')
        loadRules(route.params.id as string)
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete rule.')
      }
    },
  })
}
</script>

<template>
  <Card>
    <template #content>
      <div class="font-semibold text-xl mb-4">{{ pageTitle }}</div>
      <p v-if="pageDescription" class="text-muted-color mb-4">{{ pageDescription }}</p>

    <Form v-slot="$form" :resolver="resolver" :initial-values="form" @submit="onSubmit">
      <Tabs v-model:value="activeTab">
        <TabList>
          <Tab value="0">General</Tab>
          <Tab value="1">Settings</Tab>
          <Tab value="2">SEO</Tab>
          <Tab value="3">Images</Tab>
          <Tab v-if="isEdit" value="4">Rules</Tab>
        </TabList>

        <TabPanels>
          <TabPanel value="0">
            <Card>
              <template #content>
                <div class="flex flex-col gap-6">
                  <div class="font-semibold text-xl">General</div>
                  <div class="flex flex-col gap-4">
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Taxonomy <span class="text-red-500">*</span></label>
                      <Select name="taxonomyId" :options="taxonomyStore.activeTaxonomies" option-label="name" option-value="id" fluid :disabled="!isEdit && !!route.query.taxonomyId" />
                      <small v-if="$form.taxonomyId?.invalid" class="text-red-500">{{ $form.taxonomyId?.errors?.[0]?.message }}</small>
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Parent</label>
                      <Select name="parentId" :options="parentOptions" option-label="label" option-value="value" fluid show-clear />
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Name <span class="text-red-500">*</span></label>
                      <InputText name="name" fluid />
                      <small v-if="$form.name?.invalid" class="text-red-500">{{ $form.name?.errors?.[0]?.message }}</small>
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Presentation <span class="text-red-500">*</span></label>
                      <InputText name="presentation" fluid />
                      <small v-if="$form.presentation?.invalid" class="text-red-500">{{ $form.presentation?.errors?.[0]?.message }}</small>
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Slug <span class="text-red-500">*</span></label>
                      <InputText name="slug" fluid />
                      <small v-if="$form.slug?.invalid" class="text-red-500">{{ $form.slug?.errors?.[0]?.message }}</small>
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Description</label>
                      <Textarea name="description" fluid rows="3" />
                      <small v-if="$form.description?.invalid" class="text-red-500">{{ $form.description?.errors?.[0]?.message }}</small>
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Position</label>
                      <InputNumber name="position" fluid :min="-1" />
                      <small v-if="$form.position?.invalid" class="text-red-500">{{ $form.position?.errors?.[0]?.message }}</small>
                    </div>
                  </div>
                </div>
              </template>
            </Card>
          </TabPanel>

          <TabPanel value="1">
            <Card>
              <template #content>
                <div class="flex flex-col gap-6">
                  <div class="font-semibold text-xl">Settings</div>
                  <div class="flex flex-col gap-4">
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Sort Order</label>
                      <Select name="sortOrder" :options="TAXON_SORT_ORDERS" fluid />
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Hide from Navigation</label>
                      <ToggleSwitch name="hideFromNav" />
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Automatic Classification</label>
                      <ToggleSwitch name="automatic" />
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Rules Match Policy</label>
                      <Select name="rulesMatchPolicy" :options="TAXON_MATCH_POLICIES" fluid />
                    </div>
                  </div>
                </div>
              </template>
            </Card>
          </TabPanel>

          <TabPanel value="2">
            <Card>
              <template #content>
                <div class="flex flex-col gap-6">
                  <div class="font-semibold text-xl">SEO</div>
                  <div class="flex flex-col gap-4">
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Meta Title</label>
                      <InputText name="metaTitle" fluid />
                      <small v-if="$form.metaTitle?.invalid" class="text-red-500">{{ $form.metaTitle?.errors?.[0]?.message }}</small>
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Meta Description</label>
                      <Textarea name="metaDescription" fluid rows="3" />
                      <small v-if="$form.metaDescription?.invalid" class="text-red-500">{{ $form.metaDescription?.errors?.[0]?.message }}</small>
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Meta Keywords</label>
                      <InputText name="metaKeywords" fluid />
                      <small v-if="$form.metaKeywords?.invalid" class="text-red-500">{{ $form.metaKeywords?.errors?.[0]?.message }}</small>
                    </div>
                  </div>
                </div>
              </template>
            </Card>
          </TabPanel>

          <TabPanel value="3">
            <Card>
              <template #content>
                <div class="flex flex-col gap-6">
                  <div class="font-semibold text-xl">Images</div>
                  <div class="flex flex-col gap-4">
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Image URL</label>
                      <InputText name="imageUrl" fluid />
                      <small v-if="$form.imageUrl?.invalid" class="text-red-500">{{ $form.imageUrl?.errors?.[0]?.message }}</small>
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Square Image URL</label>
                      <InputText name="squareImageUrl" fluid />
                      <small v-if="$form.squareImageUrl?.invalid" class="text-red-500">{{ $form.squareImageUrl?.errors?.[0]?.message }}</small>
                    </div>
                  </div>
                </div>
              </template>
            </Card>
          </TabPanel>

          <TabPanel v-if="isEdit" value="4">
            <Toolbar>
              <template #start>
                <Button label="Add Rule" severity="secondary" @click="openAddRule">
                  <Plus />
                </Button>
              </template>
            </Toolbar>

            <DataTable :value="rules" :loading="rulesLoading" data-key="id">
              <Column field="type" header="Type" />
              <Column field="matchPolicy" header="Match Policy" />
              <Column field="value" header="Value" />
              <Column header="" body-style="text-align: right; width: 6rem">
                <template #body="{ data }">
                  <div class="flex justify-end gap-2">
                    <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="openEditRule(data)" />
                    <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="confirmDeleteRule(data)" />
                  </div>
                </template>
              </Column>
              <template #empty>
                <div class="text-center py-8 text-muted-color">No rules defined.</div>
              </template>
            </DataTable>
          </TabPanel>
        </TabPanels>
      </Tabs>

      <div class="flex justify-end gap-2 pt-4 border-t border-surface mt-4">
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="loading" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </div>
    </Form>

    <TaxonRuleFormDialog
      v-if="isEdit"
      :visible="dialogVisible"
      :taxon-id="(route.params.id as string) || ''"
      :editing-rule="editingRule"
      @update:visible="dialogVisible = $event"
      @saved="onRuleSaved"
    />
    </template>
  </Card>
</template>
