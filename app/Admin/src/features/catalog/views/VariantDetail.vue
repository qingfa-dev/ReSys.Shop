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
import Select from 'primevue/select'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Dialog from 'primevue/dialog'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { VariantApi } from '../services/variantApi'
import { VariantImageApi } from '../services/variantImageApi'
import { VariantPriceApi } from '../services/variantPriceApi'
import type { PriceRequest } from '../types/variantPrice'
import type { VariantForm } from '../validations/variant'
import { variantSchema } from '../validations/variant'
import type { VariantImage } from '../types/variantImage'
import type { Price } from '../types/variantPrice'
import type { OptionValueAssignment } from '../types/variant'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const confirm = useConfirm()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const productId = computed(() => route.query.productId as string)
const pageTitle = computed(() => isEdit.value ? 'Edit Variant' : 'New Variant')
const pageDescription = computed(() =>
  isEdit.value
    ? 'Edit variant details.'
    : 'Create a new variant.',
)
const activeTab = ref('0')

const resolver = zodResolver(variantSchema)

const form = ref<VariantForm>({
  sku: '',
  position: 0,
  isMaster: false,
  trackInventory: true,
  weight: null,
  weightUnit: null,
  height: null,
  width: null,
  depth: null,
  dimensionsUnit: null,
  price: null,
  costPrice: null,
  costCurrency: null,
})

const loading = ref(false)
const formLoaded = ref(!isEdit.value)
const loadedProductId = ref<string | undefined>()

const weightUnitOptions = [
  { label: 'Gram (g)', value: 'g' },
  { label: 'Kilogram (kg)', value: 'kg' },
  { label: 'Pound (lb)', value: 'lb' },
  { label: 'Ounce (oz)', value: 'oz' },
]

const dimensionsUnitOptions = [
  { label: 'Inch (in)', value: 'in' },
  { label: 'Centimeter (cm)', value: 'cm' },
  { label: 'Millimeter (mm)', value: 'mm' },
]

const fileInputRef = ref<HTMLInputElement>()

async function initEditMode(id: string) {
  const result = await VariantApi.getVariant(id)
  if (result.isSuccess) {
    const v = result.value!
    loadedProductId.value = v.productId
    form.value = {
      sku: v.sku,
      position: v.position,
      isMaster: v.isMaster,
      trackInventory: v.trackInventory,
      weight: v.weight ?? null,
      weightUnit: v.weightUnit ?? null,
      height: v.height ?? null,
      width: v.width ?? null,
      depth: v.depth ?? null,
      dimensionsUnit: v.dimensionsUnit ?? null,
      price: v.price ?? null,
      costPrice: v.costPrice ?? null,
      costCurrency: v.costCurrency ?? null,
    }
    await loadOptionValues()
    formLoaded.value = true
  } else {
    handleResult(result)
    router.push('/catalog/variants')
  }
}

onMounted(() => {
  if (isEdit.value) {
    initEditMode(route.params.id as string)
  }
})

watch(() => route.params.id, (newId) => {
  if (newId && newId !== 'new') {
    formLoaded.value = false
    initEditMode(newId as string)
  }
})

watch(activeTab, (tab) => {
  if (isEdit.value && tab === '3' && images.value.length === 0 && !imagesLoaded.value) {
    loadImages()
  }
  if (isEdit.value && tab === '4' && optionValueAssignments.value.length === 0) {
    loadOptionValues()
  }
})

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return

  const data = event.values as VariantForm
  loading.value = true

  const request = {
    sku: data.sku,
    position: data.position,
    trackInventory: data.trackInventory,
    isMaster: data.isMaster,
    weight: data.weight ?? undefined,
    weightUnit: data.weightUnit ?? undefined,
    height: data.height ?? undefined,
    width: data.width ?? undefined,
    depth: data.depth ?? undefined,
    dimensionsUnit: data.dimensionsUnit ?? undefined,
    price: data.price ?? undefined,
    costPrice: data.costPrice ?? undefined,
    costCurrency: data.costCurrency ?? undefined,
    productId: isEdit.value ? loadedProductId.value ?? '' : productId.value,
    optionValueIds: isEdit.value
      ? undefined
      : selectedOptionValueIds.value.length > 0
        ? selectedOptionValueIds.value
        : undefined,
  }

  let result
  if (isEdit.value) {
    result = await VariantApi.updateVariant(route.params.id as string, request)
  } else {
    const pid = productId.value
    if (!pid) {
      notify.error('Product ID is required')
      loading.value = false
      return
    }
    result = await VariantApi.createVariant(request)
  }

  loading.value = false

  if (result.isSuccess) {
    if (isEdit.value) {
      const variantId = route.params.id as string
      const originalAssignedIds = optionValueAssignments.value
        .filter((o) => o.isAssigned)
        .map((o) => o.optionValueId)
      const toAssign = selectedOptionValueIds.value.filter(
        (id) => !originalAssignedIds.includes(id),
      )
      const toRevoke = originalAssignedIds.filter(
        (id) => !selectedOptionValueIds.value.includes(id),
      )
      let optionValueDiffError: string | undefined
      if (toAssign.length > 0) {
        const assignResult = await VariantApi.assignOptionValues(variantId, toAssign)
        if (!assignResult.isSuccess) {
          optionValueDiffError = assignResult.errors?.[0]?.message
        }
      }
      if (toRevoke.length > 0) {
        const revokeResult = await VariantApi.revokeOptionValues(variantId, toRevoke)
        if (!revokeResult.isSuccess && !optionValueDiffError) {
          optionValueDiffError = revokeResult.errors?.[0]?.message
        }
      }
      if (optionValueDiffError) {
        notify.error('Variant updated but option value diff failed', optionValueDiffError)
      } else {
        notify.success('Variant updated')
      }
    } else {
      notify.success('Variant created')
      const created = result.value!
      router.replace(`/catalog/variants/${created.id}?productId=${productId.value}`)
    }
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push(`/catalog/variants${productId.value ? `?productId=${productId.value}` : ''}`)
}

const images = ref<VariantImage[]>([])
const imagesLoaded = ref(false)
const uploadLoading = ref(false)

async function loadImages() {
  if (!isEdit.value) return
  const result = await VariantImageApi.listImages(route.params.id as string)
  if (result.isSuccess) {
    images.value = result.items
    imagesLoaded.value = true
  } else {
    handleResult(result)
  }
}

function onFileSelect(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file || !isEdit.value) return

  const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp']
  if (!allowedTypes.includes(file.type)) {
    notify.error('Invalid file type', 'Allowed: JPEG, PNG, GIF, WebP')
    return
  }
  if (file.size > 10 * 1024 * 1024) {
    notify.error('File too large', 'File must be under 10 MB')
    return
  }

  uploadImage(file)
  target.value = ''
}

async function uploadImage(file: File) {
  uploadLoading.value = true
  const result = await VariantImageApi.uploadImage({ variantId: route.params.id as string, file })
  if (result.isSuccess) {
    notify.success('Image uploaded')
    await loadImages()
  } else {
    notify.error('Upload failed', result.errors?.[0]?.message)
  }
  uploadLoading.value = false
}

function confirmDeleteImage(image: VariantImage) {
  confirm.require({
    message: 'This permanently deletes the image. Continue?',
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const result = await VariantImageApi.deleteImage(image.id)
      if (result.isSuccess) {
        notify.success('Image deleted')
        await loadImages()
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message)
      }
    },
  })
}

const optionValueAssignments = ref<OptionValueAssignment[]>([])
const selectedOptionValueIds = ref<string[]>([])
const optionValuesLoading = ref(false)

async function loadOptionValues() {
  if (!isEdit.value) return
  optionValuesLoading.value = true
  const result = await VariantApi.getOptionValues(route.params.id as string)
  if (result.isSuccess) {
    optionValueAssignments.value = result.items
    selectedOptionValueIds.value = result.items
      .filter((o) => o.isAssigned)
      .map((o) => o.optionValueId)
  } else {
    handleResult(result)
  }
  optionValuesLoading.value = false
}

function toggleOptionValue(optionValueId: string) {
  const idx = selectedOptionValueIds.value.indexOf(optionValueId)
  if (idx >= 0) {
    selectedOptionValueIds.value.splice(idx, 1)
  } else {
    selectedOptionValueIds.value.push(optionValueId)
  }
}

const optionValuesByType = computed(() => {
  const groups = new Map<string, OptionValueAssignment[]>()
  for (const ov of optionValueAssignments.value) {
    const key = ov.optionTypeName || ov.optionTypeId
    if (!groups.has(key)) groups.set(key, [])
    groups.get(key)!.push(ov)
  }
  return [...groups.entries()]
})

const prices = ref<Price[]>([])
const pricesLoaded = ref(false)
const priceDialogVisible = ref(false)
const priceForm = ref<PriceRequest>({
  amount: undefined,
  currency: '',
  compareAtAmount: undefined,
  countryIso: undefined,
})

async function loadPrices() {
  if (!isEdit.value) return
  const result = await VariantPriceApi.listPrices(route.params.id as string)
  if (result.isSuccess) {
    prices.value = result.items
    pricesLoaded.value = true
  } else {
    handleResult(result)
  }
}

watch(activeTab, (tab) => {
  if (isEdit.value && tab === '2' && !pricesLoaded.value) {
    loadPrices()
  }
})

function openPriceDialog() {
  priceForm.value = { amount: undefined, currency: '', compareAtAmount: undefined, countryIso: undefined }
  priceDialogVisible.value = true
}

async function savePrice() {
  if (!priceForm.value.currency) return
  const result = await VariantPriceApi.setPrice({ ...priceForm.value, variantId: route.params.id as string })
  if (result.isSuccess) {
    notify.success('Price saved')
    priceDialogVisible.value = false
    await loadPrices()
  } else {
    notify.error('Failed to save price', result.errors?.[0]?.message)
  }
}

function confirmRemovePrice(price: Price) {
  confirm.require({
    message: 'Remove this price entry?',
    header: 'Confirm',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Remove',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const result = await VariantPriceApi.removePrice(
        route.params.id as string,
        price.id,
      )
      if (result.isSuccess) {
        notify.success('Price removed')
        await loadPrices()
      } else {
        notify.error('Remove failed', result.errors?.[0]?.message)
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
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="loading" form="variant-form" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel" />
      </div>
    </div>

    <div class="flex-1 min-h-0 overflow-auto">
      <Card>
        <template #content>
          <Form id="variant-form" v-slot="$form" :resolver="resolver" :initial-values="form" :key="String(formLoaded)" @submit="onSubmit">
            <Tabs v-model:value="activeTab">
              <TabList>
                <Tab value="0">General</Tab>
                <Tab value="1">Physical</Tab>
                <Tab value="2">Pricing</Tab>
                <Tab v-if="isEdit" value="3">Images</Tab>
                <Tab v-if="isEdit" value="4">Option Values</Tab>
              </TabList>
              <TabPanels>
                <TabPanel value="0">
                  <div class="grid grid-cols-2 gap-4">
                    <FormField v-slot="$field" :resolver="undefined" name="sku" class="flex flex-col gap-1">
                      <label>SKU <span class="text-red-500">*</span></label>
                      <InputText v-model="form.sku" />
                      <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                    </FormField>
                    <FormField v-slot="$field" :resolver="undefined" name="position" class="flex flex-col gap-1">
                      <label>Position</label>
                      <InputNumber v-model="form.position" :min="-1" />
                      <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                    </FormField>
                  </div>
                  <div class="flex gap-8 mt-4">
                    <div class="flex items-center gap-2">
                      <ToggleSwitch v-model="form.isMaster" />
                      <label>Master Variant</label>
                    </div>
                    <div class="flex items-center gap-2">
                      <ToggleSwitch v-model="form.trackInventory" />
                      <label>Track Inventory</label>
                    </div>
                  </div>
                </TabPanel>

                <TabPanel value="1">
                  <div class="grid grid-cols-2 gap-4">
                    <div class="flex gap-2 items-end">
                      <FormField v-slot="$field" :resolver="undefined" name="weight" class="flex flex-col gap-1 flex-1">
                        <label>Weight</label>
                        <InputNumber v-model="form.weight" :min="0" :min-fraction-digits="0" :max-fraction-digits="4" />
                        <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                      </FormField>
                      <div class="flex flex-col gap-1">
                        <label class="text-xs">&nbsp;</label>
                        <Select v-model="form.weightUnit" :options="weightUnitOptions" option-label="label" option-value="value" placeholder="Unit" class="w-36" show-clear />
                      </div>
                    </div>
                    <div />
                    <div class="flex gap-2 items-end">
                      <FormField v-slot="$field" :resolver="undefined" name="height" class="flex flex-col gap-1 flex-1">
                        <label>Height</label>
                        <InputNumber v-model="form.height" :min="0" :min-fraction-digits="0" :max-fraction-digits="4" />
                        <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                      </FormField>
                      <FormField v-slot="$field" :resolver="undefined" name="width" class="flex flex-col gap-1 flex-1">
                        <label>Width</label>
                        <InputNumber v-model="form.width" :min="0" :min-fraction-digits="0" :max-fraction-digits="4" />
                        <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                      </FormField>
                      <FormField v-slot="$field" :resolver="undefined" name="depth" class="flex flex-col gap-1 flex-1">
                        <label>Depth</label>
                        <InputNumber v-model="form.depth" :min="0" :min-fraction-digits="0" :max-fraction-digits="4" />
                        <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                      </FormField>
                      <div class="flex flex-col gap-1">
                        <label class="text-xs">&nbsp;</label>
                        <Select v-model="form.dimensionsUnit" :options="dimensionsUnitOptions" option-label="label" option-value="value" placeholder="Unit" class="w-36" show-clear />
                      </div>
                    </div>
                  </div>
                </TabPanel>

                <TabPanel value="2">
                  <div class="grid grid-cols-3 gap-4 mb-6">
                    <FormField v-slot="$field" :resolver="undefined" name="price" class="flex flex-col gap-1">
                      <label>Base Price</label>
                      <InputNumber v-model="form.price" :min="0" :min-fraction-digits="2" :max-fraction-digits="2" />
                      <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                    </FormField>
                    <FormField v-slot="$field" :resolver="undefined" name="costPrice" class="flex flex-col gap-1">
                      <label>Cost Price</label>
                      <InputNumber v-model="form.costPrice" :min="0" :min-fraction-digits="2" :max-fraction-digits="2" />
                      <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                    </FormField>
                    <FormField v-slot="$field" :resolver="undefined" name="costCurrency" class="flex flex-col gap-1">
                      <label>Currency</label>
                      <InputText v-model="form.costCurrency" placeholder="USD" maxlength="3" />
                      <small v-if="$field?.invalid" class="text-red-500">{{ $field.error?.message }}</small>
                    </FormField>
                  </div>

                  <div v-if="isEdit">
                    <div class="flex items-center justify-between mb-3">
                      <div class="font-semibold">Price History</div>
                      <Button label="Add Price" icon="pi pi-plus" severity="secondary" size="small" @click="openPriceDialog" />
                    </div>
                    <DataTable :value="prices" data-key="id">
                      <Column field="amount" header="Amount" />
                      <Column field="currency" header="Currency" />
                      <Column field="compareAtAmount" header="Compare At" />
                      <Column field="countryIso" header="Country" />
                      <Column header="" body-style="text-align: right; width: 4rem">
                        <template #body="{ data }">
                          <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Remove" @click="confirmRemovePrice(data)" />
                        </template>
                      </Column>
                      <template #empty>
                        <div class="text-center py-4 text-muted-color text-sm">No price entries.</div>
                      </template>
                    </DataTable>
                  </div>
                </TabPanel>

                <TabPanel v-if="isEdit" value="3">
                  <div class="mb-3">
                    <input type="file" accept="image/jpeg,image/png,image/gif,image/webp" class="hidden" ref="fileInputRef" @change="onFileSelect" />
                    <Button label="Upload Image" icon="pi pi-upload" severity="secondary" :loading="uploadLoading" @click="fileInputRef?.click()" />
                  </div>
                  <div v-if="images.length === 0" class="text-center py-8 text-muted-color">No images uploaded.</div>
                  <div v-else class="grid grid-cols-4 gap-4">
                    <div v-for="image in images" :key="image.id" class="border rounded-lg overflow-hidden">
                      <img :src="image.url" :alt="image.alt || image.fileName" class="w-full h-32 object-cover" />
                      <div class="p-2 text-xs">
                        <div class="truncate" :title="image.fileName">{{ image.fileName }}</div>
                        <div class="text-muted-color">{{ (image.fileSize / 1024).toFixed(0) }} KB</div>
                        <div class="flex justify-between items-center mt-1">
                          <Tag :value="image.type" severity="info" />
                          <Button icon="pi pi-trash" severity="secondary" text rounded size="small" aria-label="Delete image" @click="confirmDeleteImage(image)" />
                        </div>
                      </div>
                    </div>
                  </div>
                </TabPanel>

                <TabPanel v-if="isEdit" value="4">
                  <div v-if="optionValuesLoading" class="text-center py-4 text-muted-color">Loading option values...</div>
                  <div v-else-if="optionValuesByType.length === 0" class="text-center py-8 text-muted-color">No option types assigned to this product.</div>
                  <div v-else class="space-y-6">
                    <div v-for="[typeName, values] in optionValuesByType" :key="typeName">
                      <div class="font-semibold mb-2">{{ typeName }}</div>
                      <div class="flex flex-wrap gap-3">
                        <div v-for="ov in values" :key="ov.optionValueId" class="flex items-center gap-2">
                          <Checkbox
                            :model-value="selectedOptionValueIds.includes(ov.optionValueId)"
                            :input-id="`ov-${ov.optionValueId}`"
                            @change="toggleOptionValue(ov.optionValueId)"
                          />
                          <label :for="`ov-${ov.optionValueId}`">{{ ov.presentation || ov.name }}</label>
                        </div>
                      </div>
                    </div>
                  </div>
                </TabPanel>
              </TabPanels>
            </Tabs>
          </Form>
        </template>
      </Card>
    </div>

    <Dialog v-model:visible="priceDialogVisible" header="Add Price" :modal="true" :style="{ width: '24rem' }">
      <div class="flex flex-col gap-3">
        <div class="flex flex-col gap-1">
          <label>Currency <span class="text-red-500">*</span></label>
          <InputText v-model="priceForm.currency" placeholder="USD" maxlength="3" />
        </div>
        <div class="flex flex-col gap-1">
          <label>Amount</label>
          <InputNumber v-model="priceForm.amount" :min="0" :min-fraction-digits="2" :max-fraction-digits="2" />
        </div>
        <div class="flex flex-col gap-1">
          <label>Compare At Amount</label>
          <InputNumber v-model="priceForm.compareAtAmount" :min="0" :min-fraction-digits="2" :max-fraction-digits="2" />
        </div>
        <div class="flex flex-col gap-1">
          <label>Country (ISO)</label>
          <InputText v-model="priceForm.countryIso" placeholder="US" maxlength="2" />
        </div>
      </div>
      <template #footer>
        <Button label="Cancel" severity="secondary" @click="priceDialogVisible = false" />
        <Button label="Save" severity="primary" @click="savePrice" />
      </template>
    </Dialog>
  </div>
</template>
