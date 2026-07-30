<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import PickList from 'primevue/picklist'
import { PageShell } from '@panel'
import { FormSection, FormField } from '@form'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { ProductApi } from '../services/productApi'
import { ProductOptionTypeApi } from '../services/productOptionTypeApi'
import type { OptionTypeAssignment } from '../services/productOptionTypeApi'
import { ProductClassificationApi } from '../services/productClassificationApi'
import type { ClassificationAssignment } from '../services/productClassificationApi'
import { productSchema } from '../validations/product'
import type { ProductForm } from '../validations/product'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const confirm = useConfirm()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit Product' : 'New Product')
const pageDescription = computed(() =>
  isEdit.value
    ? 'Edit the details of the product.'
    : 'Create a new product by filling out the form below.',
)
const activeTab = ref('0')

const form = ref<ProductForm & { status?: string }>({
  name: '',
  slug: '',
  description: null,
  metaTitle: null,
  metaDescription: null,
  metaKeywords: null,
  availableOn: null,
  discontinueOn: null,
  trackInventory: true,
  styleCode: null,
  seasonName: null,
  materialComposition: null,
  careInstructions: null,
  fitNotes: null,
  department: null,
  genderTarget: null,
  status: 'Draft',
})

const fieldErrors = ref<Record<string, string>>({})
const saving = ref(false)

const unassignedOptionTypes = ref<OptionTypeAssignment[]>([])
const assignedOptionTypes = ref<OptionTypeAssignment[]>([])
const optionTypesLoading = ref(false)

const unassignedClassifications = ref<ClassificationAssignment[]>([])
const assignedClassifications = ref<ClassificationAssignment[]>([])
const classificationsLoading = ref(false)

async function initEditMode(id: string) {
  const result = await ProductApi.getProduct(id)
  if (result.isSuccess) {
    const p = result.value
    form.value = {
      name: p.name,
      slug: p.slug,
      description: p.description,
      metaTitle: p.metaTitle,
      metaDescription: p.metaDescription,
      metaKeywords: p.metaKeywords,
      availableOn: p.availableOn,
      discontinueOn: p.discontinueOn,
      trackInventory: p.trackInventory,
      styleCode: p.styleCode,
      seasonName: p.seasonName,
      materialComposition: p.materialComposition,
      careInstructions: p.careInstructions,
      fitNotes: p.fitNotes,
      department: p.department,
      genderTarget: p.genderTarget,
      status: p.status,
    }
  } else {
    handleResult(result)
    router.push('/catalog/products')
  }
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

watch(activeTab, (tab) => {
  if (isEdit.value && tab === '4' && unassignedOptionTypes.value.length === 0 && assignedOptionTypes.value.length === 0) {
    loadOptionTypes()
  }
  if (isEdit.value && tab === '5' && unassignedClassifications.value.length === 0 && assignedClassifications.value.length === 0) {
    loadClassifications()
  }
})

async function loadOptionTypes() {
  optionTypesLoading.value = true
  const result = await ProductOptionTypeApi.getOptionTypes(route.params.id as string)
  if (result.isSuccess && result.value?.items) {
    unassignedOptionTypes.value = result.value.items.filter(i => !i.isAssigned)
    assignedOptionTypes.value = result.value.items.filter(i => i.isAssigned)
  }
  optionTypesLoading.value = false
}

async function loadClassifications() {
  classificationsLoading.value = true
  const result = await ProductClassificationApi.getClassifications(route.params.id as string)
  if (result.isSuccess && result.value?.items) {
    unassignedClassifications.value = result.value.items.filter(i => !i.isAssigned)
    assignedClassifications.value = result.value.items.filter(i => i.isAssigned)
  }
  classificationsLoading.value = false
}

async function saveOptionTypes() {
  const items = assignedOptionTypes.value.map((a, i) => ({
    optionTypeId: a.optionTypeId,
    position: i,
  }))
  const result = await ProductOptionTypeApi.syncOptionTypes(route.params.id as string, items)
  if (result.isSuccess) {
    notify.success('Option types saved')
    await loadOptionTypes()
  } else {
    notify.error('Failed to save option types', result.errors?.[0]?.message)
  }
}

async function saveClassifications() {
  const items = assignedClassifications.value.map((a, i) => ({
    taxonId: a.taxonId,
    position: i,
  }))
  const result = await ProductClassificationApi.syncClassifications(route.params.id as string, items)
  if (result.isSuccess) {
    notify.success('Classifications saved')
    await loadClassifications()
  } else {
    notify.error('Failed to save classifications', result.errors?.[0]?.message)
  }
}

async function onSave() {
  fieldErrors.value = {}
  const parsed = productSchema.safeParse(form.value)

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
    slug: data.slug,
    description: data.description ?? null,
    metaTitle: data.metaTitle ?? null,
    metaDescription: data.metaDescription ?? null,
    metaKeywords: data.metaKeywords ?? null,
    availableOn: data.availableOn ?? null,
    discontinueOn: data.discontinueOn ?? null,
    trackInventory: data.trackInventory,
    styleCode: data.styleCode ?? null,
    seasonName: data.seasonName ?? null,
    materialComposition: data.materialComposition ?? null,
    careInstructions: data.careInstructions ?? null,
    fitNotes: data.fitNotes ?? null,
    department: data.department ?? null,
    genderTarget: data.genderTarget ?? null,
  }

  const result = isEdit.value
    ? await ProductApi.updateProduct(route.params.id as string, request)
    : await ProductApi.createProduct(request)

  saving.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Product updated' : 'Product created')
    if (!isEdit.value && result.value) {
      const created = result.value
      form.value = {
        ...form.value,
        name: created.name,
        slug: created.slug,
        description: created.description,
        metaTitle: created.metaTitle,
        metaDescription: created.metaDescription,
        metaKeywords: created.metaKeywords,
        availableOn: created.availableOn,
        discontinueOn: created.discontinueOn,
        trackInventory: created.trackInventory,
        styleCode: created.styleCode,
        seasonName: created.seasonName,
        materialComposition: created.materialComposition,
        careInstructions: created.careInstructions,
        fitNotes: created.fitNotes,
        department: created.department,
        genderTarget: created.genderTarget,
        status: created.status,
      }
      router.replace(`/catalog/products/${created.id}`)
    }
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/catalog/products')
}
</script>

<template>
  <PageShell :title="pageTitle" :description="pageDescription">
    <!-- Page actions -->
    <div class="flex justify-end gap-2 mb-8">
      <Button label="Save" icon="pi pi-check" severity="primary" @click="onSave()" />
      <Button label="Cancel" icon="pi pi-times" severity="secondary" @click="onCancel()" />
    </div>

    <!-- Tabs -->
    <Tabs v-model:value="activeTab">
      <TabList>
        <Tab value="0">General</Tab>
        <Tab value="1">SEO</Tab>
        <Tab value="2">Fashion</Tab>
        <Tab value="3">Timing</Tab>
        <Tab v-if="isEdit" value="4">Option Types</Tab>
        <Tab v-if="isEdit" value="5">Classifications</Tab>
      </TabList>

      <TabPanels>
        <TabPanel value="0">
          <FormSection title="Product Details">
            <!-- Tab 0: General -->
            <FormField label="Name" :required="true" :invalid="!!fieldErrors.name" class="mb-4">
              <InputText v-model="form.name" fluid />
              <small v-if="fieldErrors.name" class="text-red-500">{{ fieldErrors.name }}</small>
            </FormField>
            <FormField label="Slug" :required="true" :invalid="!!fieldErrors.slug" help-text="Lowercase alphanumeric with hyphens" class="mb-4">
              <InputText v-model="form.slug" fluid />
              <small v-if="fieldErrors.slug" class="text-red-500">{{ fieldErrors.slug }}</small>
            </FormField>
            <FormField label="Description" :invalid="!!fieldErrors.description" class="mb-4">
              <Textarea v-model="form.description" fluid rows="4" />
              <small v-if="fieldErrors.description" class="text-red-500">{{ fieldErrors.description }}</small>
            </FormField>
            <FormField v-if="isEdit" label="Status">
              <Select v-model="form.status" :options="['Draft', 'Active', 'Archived']" fluid />
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel value="1">
          <FormSection title="Search Engine Optimization">
            <!-- Tab 1: SEO -->
            <FormField label="Meta Title" :invalid="!!fieldErrors.metaTitle" class="mb-4">
              <InputText v-model="form.metaTitle" fluid />
              <small v-if="fieldErrors.metaTitle" class="text-red-500">{{ fieldErrors.metaTitle }}</small>
            </FormField>
            <FormField label="Meta Description" :invalid="!!fieldErrors.metaDescription" class="mb-4">
              <Textarea v-model="form.metaDescription" fluid rows="3" />
              <small v-if="fieldErrors.metaDescription" class="text-red-500">{{ fieldErrors.metaDescription }}</small>
            </FormField>
            <FormField label="Meta Keywords" :invalid="!!fieldErrors.metaKeywords">
              <InputText v-model="form.metaKeywords" fluid />
              <small v-if="fieldErrors.metaKeywords" class="text-red-500">{{ fieldErrors.metaKeywords }}</small>
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel value="2">
          <FormSection title="Fashion Attributes">
            <!-- Tab 2: Fashion -->
            <!-- 2-col grid fields -->
            <div class="grid grid-cols-2 gap-4">
              <FormField label="Style Code" :invalid="!!fieldErrors.styleCode">
                <InputText v-model="form.styleCode" fluid />
                <small v-if="fieldErrors.styleCode" class="text-red-500">{{ fieldErrors.styleCode }}</small>
              </FormField>
              <FormField label="Season" :invalid="!!fieldErrors.seasonName">
                <InputText v-model="form.seasonName" fluid />
                <small v-if="fieldErrors.seasonName" class="text-red-500">{{ fieldErrors.seasonName }}</small>
              </FormField>
              <FormField label="Department" :invalid="!!fieldErrors.department">
                <InputText v-model="form.department" fluid />
                <small v-if="fieldErrors.department" class="text-red-500">{{ fieldErrors.department }}</small>
              </FormField>
              <FormField label="Gender Target" :invalid="!!fieldErrors.genderTarget">
                <InputText v-model="form.genderTarget" fluid />
                <small v-if="fieldErrors.genderTarget" class="text-red-500">{{ fieldErrors.genderTarget }}</small>
              </FormField>
            </div>
            <!-- Textarea fields -->
            <FormField label="Material Composition" :invalid="!!fieldErrors.materialComposition" class="mb-4">
              <Textarea v-model="form.materialComposition" fluid rows="2" />
              <small v-if="fieldErrors.materialComposition" class="text-red-500">{{ fieldErrors.materialComposition }}</small>
            </FormField>
            <FormField label="Care Instructions" :invalid="!!fieldErrors.careInstructions" class="mb-4">
              <Textarea v-model="form.careInstructions" fluid rows="2" />
              <small v-if="fieldErrors.careInstructions" class="text-red-500">{{ fieldErrors.careInstructions }}</small>
            </FormField>
            <FormField label="Fit Notes" :invalid="!!fieldErrors.fitNotes">
              <Textarea v-model="form.fitNotes" fluid rows="2" />
              <small v-if="fieldErrors.fitNotes" class="text-red-500">{{ fieldErrors.fitNotes }}</small>
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel value="3">
          <FormSection title="Availability">
            <!-- Tab 3: Timing -->
            <FormField label="Available On" class="mb-4">
              <InputText v-model="form.availableOn" fluid type="date" />
            </FormField>
            <FormField label="Discontinue On" class="mb-4">
              <InputText v-model="form.discontinueOn" fluid type="date" />
            </FormField>
            <FormField label="Track Inventory" help-text="Enable inventory tracking for this product">
              <ToggleSwitch v-model="form.trackInventory" />
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel v-if="isEdit" value="4">
          <PickList
            v-model:target="assignedOptionTypes"
            :source="unassignedOptionTypes"
            source-header="Available"
            target-header="Assigned"
            :loading="optionTypesLoading"
            list-style="height: 300px"
            source-filter-placeholder="Search..."
            target-filter-placeholder="Search..."
          >
            <template #item="{ item }">
              <div class="flex items-center gap-2">
                <span class="font-medium">{{ item.name }}</span>
                <span class="text-muted-color text-sm">({{ item.presentation }})</span>
              </div>
            </template>
          </PickList>
          <div class="mt-3">
            <Button label="Save Option Types" severity="primary" @click="saveOptionTypes" />
          </div>
        </TabPanel>

        <TabPanel v-if="isEdit" value="5">
          <PickList
            v-model:target="assignedClassifications"
            :source="unassignedClassifications"
            source-header="Unassigned"
            target-header="Assigned"
            :loading="classificationsLoading"
            list-style="height: 300px"
            source-filter-placeholder="Search..."
            target-filter-placeholder="Search..."
          >
            <template #item="{ item }">
              <div class="flex items-center gap-2">
                <span class="font-medium">{{ item.name }}</span>
                <span v-if="item.prettyName" class="text-muted-color text-sm">({{ item.prettyName }})</span>
              </div>
            </template>
          </PickList>
          <div class="mt-3">
            <Button label="Save Classifications" severity="primary" @click="saveClassifications" />
          </div>
        </TabPanel>
      </TabPanels>
    </Tabs>
  </PageShell>
</template>
