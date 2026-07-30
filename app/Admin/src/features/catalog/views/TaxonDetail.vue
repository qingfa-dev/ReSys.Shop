<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Toolbar from 'primevue/toolbar'
import Plus from '@primeicons/vue/plus'
import { PageShell } from '@panel'
import { FormSection, FormField } from '@form'
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
const activeTab = ref('0')

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

const fieldErrors = ref<Record<string, string>>({})
const saving = ref(false)

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

async function onSave() {
  fieldErrors.value = {}
  const parsed = taxonSchema.safeParse({
    ...form.value,
    parentId: form.value.parentId || null,
    description: form.value.description || null,
  })

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

  saving.value = false

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
  <PageShell :title="pageTitle">
    <div class="flex items-center gap-2 text-muted-color mb-4">
      <router-link to="/" class="hover:text-primary">Home</router-link>
      <i class="pi pi-angle-right text-xs" />
      <router-link to="/catalog/taxons" class="hover:text-primary">Taxons</router-link>
      <i class="pi pi-angle-right text-xs" />
      <span>{{ pageTitle }}</span>
    </div>
    <Toolbar class="mb-8">
      <template #start>
        <h1 class="text-2xl font-bold">{{ pageTitle }}</h1>
      </template>
      <template #end>
        <Button label="Save" icon="pi pi-check" severity="primary" @click="onSave()" />
        <Button label="Cancel" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </template>
    </Toolbar>

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
          <FormSection title="General">
            <FormField label="Taxonomy" :required="true" :invalid="!!fieldErrors.taxonomyId">
              <Select v-model="form.taxonomyId" :options="taxonomyStore.activeTaxonomies" option-label="name" option-value="id" fluid :disabled="!isEdit && !!route.query.taxonomyId" />
              <small v-if="fieldErrors.taxonomyId" class="text-red-500">{{ fieldErrors.taxonomyId }}</small>
            </FormField>
            <FormField label="Parent" help-text="Leave empty for root-level taxon">
              <Select v-model="form.parentId" :options="parentOptions" option-label="label" option-value="value" fluid show-clear />
            </FormField>
            <FormField label="Name" :required="true" :invalid="!!fieldErrors.name">
              <InputText v-model="form.name" fluid class="w-full" />
              <small v-if="fieldErrors.name" class="text-red-500">{{ fieldErrors.name }}</small>
            </FormField>
            <FormField label="Presentation" :required="true" :invalid="!!fieldErrors.presentation">
              <InputText v-model="form.presentation" fluid class="w-full" />
              <small v-if="fieldErrors.presentation" class="text-red-500">{{ fieldErrors.presentation }}</small>
            </FormField>
            <FormField label="Slug" :required="true" :invalid="!!fieldErrors.slug" help-text="Lowercase alphanumeric with hyphens (e.g. running-shoes)">
              <InputText v-model="form.slug" fluid class="w-full" />
              <small v-if="fieldErrors.slug" class="text-red-500">{{ fieldErrors.slug }}</small>
            </FormField>
            <FormField label="Description" :invalid="!!fieldErrors.description">
              <Textarea v-model="form.description" fluid class="w-full" rows="3" />
              <small v-if="fieldErrors.description" class="text-red-500">{{ fieldErrors.description }}</small>
            </FormField>
            <FormField label="Position" :invalid="!!fieldErrors.position" help-text="Sort order">
              <InputNumber v-model="form.position" fluid :min="-1" class="w-full" />
              <small v-if="fieldErrors.position" class="text-red-500">{{ fieldErrors.position }}</small>
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel value="1">
          <FormSection title="Settings">
            <FormField label="Sort Order">
              <Select v-model="form.sortOrder" :options="TAXON_SORT_ORDERS" fluid />
            </FormField>
            <FormField label="Hide from Navigation">
              <ToggleSwitch v-model="form.hideFromNav" />
            </FormField>
            <FormField label="Automatic Classification" help-text="Use rules to auto-assign products">
              <ToggleSwitch v-model="form.automatic" />
            </FormField>
            <FormField label="Rules Match Policy" help-text="How multiple rules are combined">
              <Select v-model="form.rulesMatchPolicy" :options="TAXON_MATCH_POLICIES" fluid />
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel value="2">
          <FormSection title="SEO">
            <FormField label="Meta Title" :invalid="!!fieldErrors.metaTitle">
              <InputText v-model="form.metaTitle" fluid class="w-full" />
              <small v-if="fieldErrors.metaTitle" class="text-red-500">{{ fieldErrors.metaTitle }}</small>
            </FormField>
            <FormField label="Meta Description" :invalid="!!fieldErrors.metaDescription">
              <Textarea v-model="form.metaDescription" fluid class="w-full" rows="3" />
              <small v-if="fieldErrors.metaDescription" class="text-red-500">{{ fieldErrors.metaDescription }}</small>
            </FormField>
            <FormField label="Meta Keywords" :invalid="!!fieldErrors.metaKeywords">
              <InputText v-model="form.metaKeywords" fluid class="w-full" />
              <small v-if="fieldErrors.metaKeywords" class="text-red-500">{{ fieldErrors.metaKeywords }}</small>
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel value="3">
          <FormSection title="Images">
            <FormField label="Image URL" :invalid="!!fieldErrors.imageUrl">
              <InputText v-model="form.imageUrl" fluid class="w-full" />
              <small v-if="fieldErrors.imageUrl" class="text-red-500">{{ fieldErrors.imageUrl }}</small>
            </FormField>
            <FormField label="Square Image URL" :invalid="!!fieldErrors.squareImageUrl">
              <InputText v-model="form.squareImageUrl" fluid class="w-full" />
              <small v-if="fieldErrors.squareImageUrl" class="text-red-500">{{ fieldErrors.squareImageUrl }}</small>
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel v-if="isEdit" value="4">
          <Card>
            <template #content>
              <Toolbar>
                <template #start>
                  <Button label="Add Rule" severity="secondary" @click="openAddRule">
                    <Plus />
                  </Button>
                </template>
              </Toolbar>
            </template>
          </Card>

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

    <TaxonRuleFormDialog
      v-if="isEdit"
      :visible="dialogVisible"
      :taxon-id="(route.params.id as string) || ''"
      :editing-rule="editingRule"
      @update:visible="dialogVisible = $event"
      @saved="onRuleSaved"
    />
  </PageShell>
</template>
