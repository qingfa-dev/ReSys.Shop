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
import Card from 'primevue/card'
import { Form } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
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

const resolver = zodResolver(productSchema)

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

const loading = ref(false)

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

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return

  const data = event.values as ProductForm
  loading.value = true

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

  loading.value = false

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
  <Card>
    <template #content>
      <div class="font-semibold text-xl mb-4">{{ pageTitle }}</div>
      <p v-if="pageDescription" class="text-muted-color mb-4">{{ pageDescription }}</p>

    <Form v-slot="$form" :resolver="resolver" :initial-values="form" @submit="onSubmit">
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
            <Card>
              <template #content>
                <div class="flex flex-col gap-6">
                  <div class="font-semibold text-xl">Product Details</div>
                  <div class="flex flex-col gap-4">
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Name <span class="text-red-500">*</span></label>
                      <InputText name="name" fluid />
                      <small v-if="$form.name?.invalid" class="text-red-500">{{ $form.name?.errors?.[0]?.message }}</small>
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Slug <span class="text-red-500">*</span></label>
                      <InputText name="slug" fluid />
                      <small v-if="$form.slug?.invalid" class="text-red-500">{{ $form.slug?.errors?.[0]?.message }}</small>
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Description</label>
                      <Textarea name="description" fluid rows="4" />
                      <small v-if="$form.description?.invalid" class="text-red-500">{{ $form.description?.errors?.[0]?.message }}</small>
                    </div>
                    <div v-if="isEdit" class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Status</label>
                      <Select v-model="form.status" :options="['Draft', 'Active', 'Archived']" fluid />
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
                  <div class="font-semibold text-xl">Search Engine Optimization</div>
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

          <TabPanel value="2">
            <Card>
              <template #content>
                <div class="flex flex-col gap-6">
                  <div class="font-semibold text-xl">Fashion Attributes</div>
                  <div class="grid grid-cols-2 gap-4">
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Style Code</label>
                      <InputText name="styleCode" fluid />
                      <small v-if="$form.styleCode?.invalid" class="text-red-500">{{ $form.styleCode?.errors?.[0]?.message }}</small>
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Season</label>
                      <InputText name="seasonName" fluid />
                      <small v-if="$form.seasonName?.invalid" class="text-red-500">{{ $form.seasonName?.errors?.[0]?.message }}</small>
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Department</label>
                      <InputText name="department" fluid />
                      <small v-if="$form.department?.invalid" class="text-red-500">{{ $form.department?.errors?.[0]?.message }}</small>
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Gender Target</label>
                      <InputText name="genderTarget" fluid />
                      <small v-if="$form.genderTarget?.invalid" class="text-red-500">{{ $form.genderTarget?.errors?.[0]?.message }}</small>
                    </div>
                  </div>
                  <div class="flex flex-col gap-1">
                    <label class="text-surface-900 dark:text-surface-0 font-medium">Material Composition</label>
                    <Textarea name="materialComposition" fluid rows="2" />
                    <small v-if="$form.materialComposition?.invalid" class="text-red-500">{{ $form.materialComposition?.errors?.[0]?.message }}</small>
                  </div>
                  <div class="flex flex-col gap-1">
                    <label class="text-surface-900 dark:text-surface-0 font-medium">Care Instructions</label>
                    <Textarea name="careInstructions" fluid rows="2" />
                    <small v-if="$form.careInstructions?.invalid" class="text-red-500">{{ $form.careInstructions?.errors?.[0]?.message }}</small>
                  </div>
                  <div class="flex flex-col gap-1">
                    <label class="text-surface-900 dark:text-surface-0 font-medium">Fit Notes</label>
                    <Textarea name="fitNotes" fluid rows="2" />
                    <small v-if="$form.fitNotes?.invalid" class="text-red-500">{{ $form.fitNotes?.errors?.[0]?.message }}</small>
                  </div>
                </div>
              </template>
            </Card>
          </TabPanel>

          <TabPanel value="3">
            <Card>
              <template #content>
                <div class="flex flex-col gap-6">
                  <div class="font-semibold text-xl">Availability</div>
                  <div class="flex flex-col gap-4">
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Available On</label>
                      <InputText name="availableOn" fluid type="date" />
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Discontinue On</label>
                      <InputText name="discontinueOn" fluid type="date" />
                    </div>
                    <div class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Track Inventory</label>
                      <ToggleSwitch name="trackInventory" />
                    </div>
                  </div>
                </div>
              </template>
            </Card>
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

      <div class="flex justify-end gap-2 pt-4 border-t border-surface mt-4">
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="loading" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </div>
    </Form>
    </template>
  </Card>
</template>
