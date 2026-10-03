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
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { useTaxonDetail } from '../composables/useTaxonDetail'
import { useActiveTaxonomies } from '../composables/useActiveTaxonomies'
import { TaxonApi } from '../services/taxonApi'
import { TaxonRuleApi } from '../services/taxonRuleApi'
import { taxonSchema, taxonTaxonomyId, taxonParentId, taxonName, taxonPresentation, taxonSlug, taxonDescription, taxonPosition, taxonMetaTitle, taxonMetaDescription, taxonMetaKeywords, taxonImageUrl, taxonSquareImageUrl, taxonSortOrder, taxonRulesMatchPolicy } from '../validations/taxon'
import type { TaxonForm } from '../validations/taxon'
import type { TaxonRuleListItem } from '../types/taxonRule'
import { TAXON_SORT_ORDERS, TAXON_MATCH_POLICIES } from '../types/taxon'
import TaxonRuleFormDialog from '../components/TaxonRuleFormDialog.vue'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const confirm = useConfirm()
const { handleResult } = useApiErrorHandler()
const { items: activeTaxonomies, load: loadActiveTaxonomies } = useActiveTaxonomies()
const { fetchDetail, fetchRules, rules, rulesLoading } = useTaxonDetail()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit Taxon' : 'New Taxon')
const pageDescription = computed(() =>
  isEdit.value
    ? 'Edit the details of the taxon.'
    : 'Create a new taxon by filling out the form below.',
)
const activeTab = ref('0')

const resolver = zodResolver(taxonSchema)
const taxonomyIdResolver = zodResolver(taxonTaxonomyId)
const parentIdResolver = zodResolver(taxonParentId)
const nameResolver = zodResolver(taxonName)
const presentationResolver = zodResolver(taxonPresentation)
const slugResolver = zodResolver(taxonSlug)
const descriptionResolver = zodResolver(taxonDescription)
const positionResolver = zodResolver(taxonPosition)
const metaTitleResolver = zodResolver(taxonMetaTitle)
const metaDescriptionResolver = zodResolver(taxonMetaDescription)
const metaKeywordsResolver = zodResolver(taxonMetaKeywords)
const imageUrlResolver = zodResolver(taxonImageUrl)
const squareImageUrlResolver = zodResolver(taxonSquareImageUrl)
const sortOrderResolver = zodResolver(taxonSortOrder)
const rulesMatchPolicyResolver = zodResolver(taxonRulesMatchPolicy)

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
const formLoaded = ref(!isEdit.value)

const parentOptions = ref<{ label: string; value: string }[]>([])
const dialogVisible = ref(false)
const editingRule = ref<TaxonRuleListItem | null>(null)

async function initEditMode(id: string) {
  const result = await fetchDetail(id)
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
    formLoaded.value = true

    await Promise.all([loadParents(result.value.taxonomyId), loadRules(id)])
  } else {
    handleResult(result)
    router.push('/catalog/taxons')
  }
}

// Call: Catalog service — sibling taxons scoped to the selected taxonomy
async function loadParents(taxonomyId: string) {
  const result = await TaxonApi.getList(taxonomyId, {})
  if (result.isSuccess) {
    parentOptions.value = [
      { label: '(None — root level)', value: '' },
      ...result.items.map((t) => ({ label: t.name, value: t.id })),
    ]
  }
}

async function loadRules(taxonId: string) {
  await fetchRules(taxonId)
}

onMounted(async () => {
  // Await: Taxonomy options needed by the taxonomy Select
  await loadActiveTaxonomies()
  if (isEdit.value) {
    await initEditMode(route.params.id as string)
  } else if (form.value.taxonomyId) {
    await loadParents(form.value.taxonomyId)
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
  // Validate: Return early when zod form validation fails.
  if (!event.valid) return

  const data = event.values as TaxonForm
  loading.value = true

  // Transform: Normalise the form into the create/update request payload.
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
    rulesMatchPolicy: data.rulesMatchPolicy as 'All' | 'Any',
    sortOrder: data.sortOrder,
    hideFromNav: data.hideFromNav,
  }

  // Call: Persist the taxon, branching between update and create.
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
  loadRules(route.params.id as string)
}

function confirmDeleteRule(rule: TaxonRuleListItem) {
  // Trigger: Confirm before deleting a classification rule.
  confirm.require({
    message: `Are you sure you want to delete this rule?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      // Call: Delete the rule via the API, then reload the rule list.
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
  <div class="flex flex-col h-full p-4">
    <!-- Section: Page Header — dynamic title plus Save and Cancel actions -->
    <div class="flex-none flex justify-between items-start gap-4 mb-4">
      <div>
        <div class="font-semibold text-xl">{{ pageTitle }}</div>
        <p v-if="pageDescription" class="text-muted-color mt-1">{{ pageDescription }}</p>
      </div>
      <div class="flex items-center gap-2 shrink-0">
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="loading" form="taxon-form" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </div>
    </div>

    <div class="flex-1 min-h-0 overflow-auto">
      <!-- Section: Content Card — holds the form and its tabbed field groups -->
      <Card>
        <template #content>
          <Form id="taxon-form" :key="String(formLoaded)" :resolver="resolver" :initial-values="form" @submit="onSubmit">
            <!-- Section: Tabs — switches between general, settings, SEO, images, and rules -->
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
                  <!-- Section: General Fields — taxonomy, parent, name, and description -->
                  <Card>
                    <template #content>
                      <div class="flex flex-col gap-4">
                            <FormField v-slot="$field" name="taxonomyId" :resolver="taxonomyIdResolver" class="flex flex-col gap-1">
                              <label class="text-surface-900 dark:text-surface-0 font-medium">Taxonomy <span class="text-red-500">*</span></label>
                              <Select :options="activeTaxonomies" option-label="name" option-value="id" fluid :disabled="!isEdit && !!route.query.taxonomyId" />
                              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                            </FormField>
                            <FormField name="parentId" :resolver="parentIdResolver" class="flex flex-col gap-1">
                              <label class="text-surface-900 dark:text-surface-0 font-medium">Parent</label>
                              <Select :options="parentOptions" option-label="label" option-value="value" fluid show-clear />
                            </FormField>
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
                            <FormField v-slot="$field" name="slug" :resolver="slugResolver" class="flex flex-col gap-1">
                              <label class="text-surface-900 dark:text-surface-0 font-medium">Slug <span class="text-red-500">*</span></label>
                              <InputText fluid />
                              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                            </FormField>
                            <FormField v-slot="$field" name="description" :resolver="descriptionResolver" class="flex flex-col gap-1">
                              <label class="text-surface-900 dark:text-surface-0 font-medium">Description</label>
                              <Textarea fluid rows="3" />
                              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                            </FormField>
                            <FormField v-slot="$field" name="position" :resolver="positionResolver" class="flex flex-col gap-1">
                              <label class="text-surface-900 dark:text-surface-0 font-medium">Position</label>
                              <InputNumber fluid :min="-1" />
                              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                            </FormField>
                          </div>
                    </template>
                  </Card>
                </TabPanel>

                <TabPanel value="1">
                  <!-- Section: Settings Fields — ordering, nav, and classification policy -->
                  <Card>
                    <template #content>
                      <div class="flex flex-col gap-4">
                            <FormField name="sortOrder" :resolver="sortOrderResolver" class="flex flex-col gap-1">
                              <label class="text-surface-900 dark:text-surface-0 font-medium">Sort Order</label>
                              <Select :options="TAXON_SORT_ORDERS" fluid />
                            </FormField>
                            <FormField name="hideFromNav" class="flex flex-col gap-1">
                              <label class="text-surface-900 dark:text-surface-0 font-medium">Hide from Navigation</label>
                              <ToggleSwitch />
                            </FormField>
                            <FormField name="automatic" class="flex flex-col gap-1">
                              <label class="text-surface-900 dark:text-surface-0 font-medium">Automatic Classification</label>
                              <ToggleSwitch />
                            </FormField>
                            <FormField name="rulesMatchPolicy" :resolver="rulesMatchPolicyResolver" class="flex flex-col gap-1">
                              <label class="text-surface-900 dark:text-surface-0 font-medium">Rules Match Policy</label>
                              <Select :options="TAXON_MATCH_POLICIES" fluid />
                            </FormField>
                          </div>
                    </template>
                  </Card>
                </TabPanel>

                <TabPanel value="2">
                  <!-- Section: SEO Fields — meta title, description, and keywords -->
                  <Card>
                    <template #content>
                      <div class="flex flex-col gap-4">
                            <FormField v-slot="$field" name="metaTitle" :resolver="metaTitleResolver" class="flex flex-col gap-1">
                              <label class="text-surface-900 dark:text-surface-0 font-medium">Meta Title</label>
                              <InputText fluid />
                              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                            </FormField>
                            <FormField v-slot="$field" name="metaDescription" :resolver="metaDescriptionResolver" class="flex flex-col gap-1">
                              <label class="text-surface-900 dark:text-surface-0 font-medium">Meta Description</label>
                              <Textarea fluid rows="3" />
                              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                            </FormField>
                            <FormField v-slot="$field" name="metaKeywords" :resolver="metaKeywordsResolver" class="flex flex-col gap-1">
                              <label class="text-surface-900 dark:text-surface-0 font-medium">Meta Keywords</label>
                              <InputText fluid />
                              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                            </FormField>
                          </div>
                    </template>
                  </Card>
                </TabPanel>

                <TabPanel value="3">
                  <!-- Section: Image Fields — display and square image URLs -->
                  <Card>
                    <template #content>
                      <div class="flex flex-col gap-4">
                            <FormField v-slot="$field" name="imageUrl" :resolver="imageUrlResolver" class="flex flex-col gap-1">
                              <label class="text-surface-900 dark:text-surface-0 font-medium">Image URL</label>
                              <InputText fluid />
                              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                            </FormField>
                            <FormField v-slot="$field" name="squareImageUrl" :resolver="squareImageUrlResolver" class="flex flex-col gap-1">
                              <label class="text-surface-900 dark:text-surface-0 font-medium">Square Image URL</label>
                              <InputText fluid />
                              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                            </FormField>
                          </div>
                    </template>
                  </Card>
                </TabPanel>

                <TabPanel v-if="isEdit" value="4">
                  <!-- Section: Rules — add, edit, search, and delete classification rules -->
                  <Toolbar>
                    <template #start>
                      <Button label="Add Rule" severity="secondary" @click="openAddRule">
                        <Plus />
                      </Button>
                    </template>
                  </Toolbar>

                  <DataTable size="large" :value="rules" :loading="rulesLoading" data-key="id">
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
    </div>
  </div>
</template>
