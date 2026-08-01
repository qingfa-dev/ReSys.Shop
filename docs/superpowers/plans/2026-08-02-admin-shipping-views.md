# Admin Shipping Views — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace 4 Shipping placeholder views (ShippingMethodsList, ShippingMethodDetail, ShippingRatesList, ShippingRateDetail) with functional CRUD UIs. Both detail views are single-form tabs.

**Architecture:** Standard list+detail pattern. ShippingMethodDetail has a single form tab with activate/deactivate toggle. ShippingRateDetail has a single form tab with weight range and condition fields.

**Tech Stack:** Vue 3 + TypeScript, PrimeVue (DataTable, Form, Card, Select, ToggleSwitch, InputNumber), existing `ShippingMethodApi`/`ShippingRateApi`

**Global Constraints:**
- Follows established Catalog/Location view patterns
- All services, types, validations already exist
- View files already exist as placeholders — modify in place

---

### Task 1: ShippingMethodsList.vue

**Files:**
- Modify: `app/Admin/src/features/shipping/views/ShippingMethodsList.vue`

**Interfaces:**
- Consumes: `ShippingMethodApi.getShippingMethods(query)` → `PagedResult<ShippingMethodListItem>`, `deleteShippingMethod(id)` → `Result<void>`
- Consumes: `SHIPPING_METHOD_FILTER_FIELDS`, `SHIPPING_METHOD_SORT_FIELDS`, `SHIPPING_METHOD_SEARCH_FIELDS`
- Consumes: `SHIPPING` → `${SHIPPING}/shipping-methods`

- [ ] **Step 1: Write ShippingMethodsList.vue**

Standard list view. Columns: Name, Carrier, Active (Tag badge), Actions (Edit, Delete). Create → `/shipping/shipping-methods/new`.

(Standard pattern — see `UsersList.vue`. Use `ShippingMethodApi`, `ShippingMethodListItem`, path `/shipping/shipping-methods`.)

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/shipping/views/ShippingMethodsList.vue
git commit -m "feat(shipping): implement shipping methods list view"
```

---

### Task 2: ShippingMethodDetail.vue

**Files:**
- Modify: `app/Admin/src/features/shipping/views/ShippingMethodDetail.vue`

**Interfaces:**
- Consumes: `ShippingMethodApi.getShippingMethod(id)`, `createShippingMethod(request)`, `updateShippingMethod(id, request)`
- Consumes: `ShippingMethodApi.activateShippingMethod(id)`, `deactivateShippingMethod(id)`
- Consumes: `shippingMethodSchema`, `ShippingMethodForm` from `../validations/shippingMethod`

- [ ] **Step 1: Write ShippingMethodDetail.vue**

Single form tab (same pattern as `PaymentMethodDetail.vue`). Fields: Name, Description (`Textarea`), Carrier (`Select`: FedEx/UPS/USPS/DHL/Custom), Is Active (`ToggleSwitch` with activate/deactivate API calls on change), Configuration (`Textarea` for JSON, optional).

(Pattern: identical to `PaymentMethodDetail.vue` with different field names and carrier options array `['FedEx','UPS','USPS','DHL','Custom']`. Import `ShippingMethodApi` instead of `PaymentMethodApi`.)

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/shipping/views/ShippingMethodDetail.vue
git commit -m "feat(shipping): implement shipping method detail view"
```

---

### Task 3: ShippingRatesList.vue

**Files:**
- Modify: `app/Admin/src/features/shipping/views/ShippingRatesList.vue`

**Interfaces:**
- Consumes: `ShippingRateApi.getShippingRates(query)` → `PagedResult<ShippingRateListItem>`, `deleteShippingRate(id)` → `Result<void>`
- Consumes: `SHIPPING_RATE_FILTER_FIELDS`, `SHIPPING_RATE_SORT_FIELDS`, `SHIPPING_RATE_SEARCH_FIELDS`
- Consumes: `SHIPPING` → `${SHIPPING}/shipping-rates`

- [ ] **Step 1: Write ShippingRatesList.vue**

Standard list view. Columns: Name, Shipping Method (nested), Price (currency), Min Weight, Max Weight, Condition, Actions (Edit, Delete). Create → `/shipping/shipping-rates/new`.

(Standard pattern — see `UsersList.vue`.)

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/shipping/views/ShippingRatesList.vue
git commit -m "feat(shipping): implement shipping rates list view"
```

---

### Task 4: ShippingRateDetail.vue

**Files:**
- Modify: `app/Admin/src/features/shipping/views/ShippingRateDetail.vue`

**Interfaces:**
- Consumes: `ShippingRateApi.getShippingRate(id)`, `createShippingRate(request)`, `updateShippingRate(id, request)`
- Consumes: `ShippingMethodApi.getShippingMethods(...)` → for method selector dropdown
- Consumes: `shippingRateSchema`, `ShippingRateForm` from `../validations/shippingRate`

- [ ] **Step 1: Write ShippingRateDetail.vue**

Single form tab. Fields: Shipping Method (`Select` from `ShippingMethodApi.getShippingMethods({pageSize:100})`), Name, Price (`InputNumber`), Min Weight (`InputNumber`, nullable), Max Weight (`InputNumber`, nullable), Condition Type (`Select`: Weight/Price/Flat).

(Pattern: similar to `PaymentMethodDetail.vue` single-form layout. On mounted: load shipping methods for the Select dropdown. Form fields use `InputNumber` for numeric fields with `:min="0"`. Weight fields are nullable (show clear button).)

```vue
<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Select from 'primevue/select'
import Message from 'primevue/message'
import { Form, FormField, type FormSubmitEvent, zodResolver } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { ShippingRateApi } from '../services/shippingRateApi'
import { ShippingMethodApi } from '../services/shippingMethodApi'
import { shippingRateSchema, type ShippingRateForm } from '../validations/shippingRate'
import type { ShippingMethodListItem } from '../types/shippingMethod'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const resolver = zodResolver(shippingRateSchema)
const form = ref<ShippingRateForm>({ shippingMethodId: '', name: '', price: 0, minWeight: null, maxWeight: null, conditionType: '' })
const formLoaded = ref(false)
const loading = ref(false)
const shippingMethods = ref<ShippingMethodListItem[]>([])
const conditionTypes = ['Weight', 'Price', 'Flat']

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit Shipping Rate' : 'New Shipping Rate')

async function initEditMode(id: string) {
  const result = await ShippingRateApi.getShippingRate(id)
  if (result.isSuccess) {
    const r = result.value
    form.value = {
      shippingMethodId: r.shippingMethodId ?? '',
      name: r.name,
      price: r.price ?? 0,
      minWeight: (r as any).minWeight ?? null,
      maxWeight: (r as any).maxWeight ?? null,
      conditionType: (r as any).conditionType ?? '',
    }
    formLoaded.value = true
  } else { handleResult(result); router.push('/shipping/shipping-rates') }
}

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return
  loading.value = true
  const data = event.values as ShippingRateForm
  const request = {
    shippingMethodId: data.shippingMethodId,
    name: data.name,
    price: data.price,
    minWeight: data.minWeight ?? null,
    maxWeight: data.maxWeight ?? null,
    conditionType: data.conditionType || null,
  }
  const result = isEdit.value
    ? await ShippingRateApi.updateShippingRate(route.params.id as string, request as any)
    : await ShippingRateApi.createShippingRate(request as any)
  loading.value = false
  if (result.isSuccess) {
    notify.success(pageTitle.value, 'Saved')
    if (!isEdit.value && result.value) router.replace(`/shipping/shipping-rates/${(result.value as any).id}`)
  } else { handleResult(result) }
}

async function loadShippingMethods() {
  const result = await ShippingMethodApi.getShippingMethods({ pageSize: 100 })
  if (result.isSuccess) shippingMethods.value = result.items
}

onMounted(async () => {
  await loadShippingMethods()
  if (isEdit.value) initEditMode(route.params.id as string)
  else formLoaded.value = true
})
watch(() => route.params.id, (newId) => { if (newId && newId !== 'new') initEditMode(newId as string) })
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="flex items-center gap-4 mb-6">
      <Button icon="pi pi-arrow-left" severity="secondary" text rounded @click="router.push('/shipping/shipping-rates')" />
      <h1 class="text-2xl font-semibold">{{ pageTitle }}</h1>
    </div>

    <Form id="shipping-rate-form" :key="String(formLoaded)" :resolver="resolver" :initial-values="form" @submit="onSubmit">
      <Card>
        <template #content>
          <div class="flex flex-col gap-4">
            <FormField v-slot="$field" name="shippingMethodId" class="flex flex-col gap-1">
              <label>Shipping Method <span class="text-red-500">*</span></label>
              <Select :options="shippingMethods" option-label="name" option-value="id" fluid show-clear />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <FormField v-slot="$field" name="name" class="flex flex-col gap-1">
              <label>Name <span class="text-red-500">*</span></label>
              <InputText fluid />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <FormField v-slot="$field" name="price" class="flex flex-col gap-1">
                <label>Price <span class="text-red-500">*</span></label>
                <InputNumber fluid :min="0" />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="minWeight" class="flex flex-col gap-1">
                <label>Min Weight</label>
                <InputNumber fluid :min="0" show-clear />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="maxWeight" class="flex flex-col gap-1">
                <label>Max Weight</label>
                <InputNumber fluid :min="0" show-clear />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
            </div>
            <FormField v-slot="$field" name="conditionType" class="flex flex-col gap-1">
              <label>Condition Type</label>
              <Select :options="conditionTypes" fluid show-clear />
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
          </div>
        </template>
      </Card>
    </Form>

    <div class="flex gap-3 mt-4">
      <Button label="Save" icon="pi pi-check" form="shipping-rate-form" type="submit" :loading="loading" />
      <Button label="Cancel" icon="pi pi-times" severity="secondary" @click="router.push('/shipping/shipping-rates')" />
    </div>
  </div>
</template>
```

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/shipping/views/ShippingRateDetail.vue
git commit -m "feat(shipping): implement shipping rate detail view"
```
