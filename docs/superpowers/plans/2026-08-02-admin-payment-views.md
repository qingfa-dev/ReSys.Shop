# Admin Payment Views — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace 3 Payment placeholder views (PaymentsList, PaymentMethodsList, PaymentMethodDetail) with functional UIs. PaymentsList is read-only. PaymentMethodsList has full CRUD. PaymentMethodDetail is a single-form tab with activate/deactivate toggle.

**Architecture:** Standard list+detail pattern. PaymentMethodDetail has a single form tab (no sub-tabs) since this entity has no related data to display. Activate/deactivate uses the existing `activatePaymentMethod`/`deactivatePaymentMethod` API methods via a toggle switch.

**Tech Stack:** Vue 3 + TypeScript, PrimeVue (DataTable, Form, Card, ToggleSwitch, Select), existing `PaymentApi`/`PaymentMethodApi`

**Global Constraints:**
- Follows established Catalog/Location view patterns
- All services, types, validations already exist
- View files already exist as placeholders — modify in place

---

### Task 1: PaymentsList.vue

**Files:**
- Modify: `app/Admin/src/features/payment/views/PaymentsList.vue`

**Interfaces:**
- Consumes: `PaymentApi.getPayments(query)` → `PagedResult<PaymentListItem>`
- Consumes: `PAYMENT_FILTER_FIELDS`, `PAYMENT_SORT_FIELDS` (if they exist) from `../types/payment`
- Consumes: `PAYMENT` from `@/shared/constants/api` → `${PAYMENT}/payments`
- Note: Read-only, no detail route

- [ ] **Step 1: Write PaymentsList.vue**

Read-only DataTable. Columns: Payment #, Order #, Method, Amount (currency format), Status (Tag badge), Created At. Filter by Status dropdown. No create/edit/delete. Search bar, Reload, Export.

(Standard read-only pattern — see `StockReservationsList.vue` or `StockMovementsList.vue`. Toolbar: search + status filter Select + reload + export. No New button.)

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/payment/views/PaymentsList.vue
git commit -m "feat(payment): implement payments list view (read-only)"
```

---

### Task 2: PaymentMethodsList.vue

**Files:**
- Modify: `app/Admin/src/features/payment/views/PaymentMethodsList.vue`

**Interfaces:**
- Consumes: `PaymentMethodApi.getPaymentMethods(query)` → `PagedResult<PaymentMethodListItem>`, `deletePaymentMethod(id)` → `Result<void>`
- Consumes: `PAYMENT_METHOD_FILTER_FIELDS`, `PAYMENT_METHOD_SORT_FIELDS`, `PAYMENT_METHOD_SEARCH_FIELDS`
- Consumes: `PAYMENT` → `${PAYMENT}/payment-methods`

- [ ] **Step 1: Write PaymentMethodsList.vue**

Standard list view. Columns: Display Name, Provider, Active (Tag badge), Actions (Edit, Delete). Create → `/payment/payment-methods/new`.

(Standard pattern — see `UsersList.vue` template. Use `PaymentMethodApi`, `PaymentMethodListItem`, path `/payment/payment-methods`.)

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/payment/views/PaymentMethodsList.vue
git commit -m "feat(payment): implement payment methods list view"
```

---

### Task 3: PaymentMethodDetail.vue

**Files:**
- Modify: `app/Admin/src/features/payment/views/PaymentMethodDetail.vue`

**Interfaces:**
- Consumes: `PaymentMethodApi.getPaymentMethod(id)`, `createPaymentMethod(request)`, `updatePaymentMethod(id, request)`
- Consumes: `PaymentMethodApi.activatePaymentMethod(id)`, `deactivatePaymentMethod(id)`
- Consumes: `paymentMethodSchema`, `PaymentMethodForm` from `../validations/paymentMethod`

- [ ] **Step 1: Write PaymentMethodDetail.vue**

Single form tab (no sub-tabs). Fields: Name, Display Name, Provider Type (`Select`: CreditCard/PayPal/BankTransfer/Cash/Other), Is Active (`ToggleSwitch`), Configuration (`Textarea` for JSON, optional).

Is Active toggle calls `activatePaymentMethod`/`deactivatePaymentMethod` on change (only in edit mode).

(Pattern: single-tab form similar to `CountryDetail.vue`. No `Tabs` component — form directly in `<Form>` with `class="flex flex-col gap-4"`. Import `ref`, `computed`, `onMounted`, `useRoute`, `useRouter`, `Form`, `FormField`, `zodResolver`.)

```vue
<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Select from 'primevue/select'
import ToggleSwitch from 'primevue/toggleswitch'
import Message from 'primevue/message'
import { Form, FormField, type FormSubmitEvent, zodResolver } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { PaymentMethodApi } from '../services/paymentMethodApi'
import { paymentMethodSchema, type PaymentMethodForm } from '../validations/paymentMethod'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const resolver = zodResolver(paymentMethodSchema)
const form = ref<PaymentMethodForm>({ name: '', displayName: '', providerType: '', isActive: true, configuration: '' })
const formLoaded = ref(false)
const loading = ref(false)
const providerTypes = ['CreditCard', 'PayPal', 'BankTransfer', 'Cash', 'Other']
const isActive = ref(true)

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit Payment Method' : 'New Payment Method')

async function initEditMode(id: string) {
  const result = await PaymentMethodApi.getPaymentMethod(id)
  if (result.isSuccess) {
    const m = result.value
    form.value = { name: m.name, displayName: m.displayName ?? '', providerType: m.providerType ?? '', isActive: (m as any).isActive ?? true, configuration: (m as any).configuration ?? '' }
    isActive.value = (m as any).isActive ?? true
    formLoaded.value = true
  } else { handleResult(result); router.push('/payment/payment-methods') }
}

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return
  loading.value = true
  const data = event.values as PaymentMethodForm
  const request = { name: data.name, displayName: data.displayName, providerType: data.providerType, configuration: data.configuration || null }
  const result = isEdit.value
    ? await PaymentMethodApi.updatePaymentMethod(route.params.id as string, request as any)
    : await PaymentMethodApi.createPaymentMethod(request as any)
  loading.value = false
  if (result.isSuccess) {
    notify.success(pageTitle.value, 'Saved')
    if (!isEdit.value && result.value) router.replace(`/payment/payment-methods/${(result.value as any).id}`)
  } else { handleResult(result) }
}

async function toggleActive(value: boolean) {
  if (!isEdit.value) return
  const id = route.params.id as string
  const result = value
    ? await PaymentMethodApi.activatePaymentMethod(id)
    : await PaymentMethodApi.deactivatePaymentMethod(id)
  if (result.isSuccess) {
    isActive.value = value
    notify.success(value ? 'Activated' : 'Deactivated')
  } else {
    isActive.value = !value // revert
    handleResult(result)
  }
}

onMounted(() => {
  if (isEdit.value) initEditMode(route.params.id as string)
  else formLoaded.value = true
})
watch(() => route.params.id, (newId) => { if (newId && newId !== 'new') initEditMode(newId as string) })
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="flex items-center gap-4 mb-6">
      <Button icon="pi pi-arrow-left" severity="secondary" text rounded @click="router.push('/payment/payment-methods')" />
      <h1 class="text-2xl font-semibold">{{ pageTitle }}</h1>
    </div>

    <Form id="payment-method-form" :key="String(formLoaded)" :resolver="resolver" :initial-values="form" @submit="onSubmit">
      <Card>
        <template #content>
          <div class="flex flex-col gap-4">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <FormField v-slot="$field" name="name" class="flex flex-col gap-1">
                <label>Name <span class="text-red-500">*</span></label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="displayName" class="flex flex-col gap-1">
                <label>Display Name</label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <FormField v-slot="$field" name="providerType" class="flex flex-col gap-1">
                <label>Provider Type</label>
                <Select :options="providerTypes" fluid show-clear />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="isActive" class="flex flex-col gap-1">
                <label>Active</label>
                <ToggleSwitch :model-value="isActive" @change="toggleActive" />
              </FormField>
            </div>
            <FormField v-slot="$field" name="configuration" class="flex flex-col gap-1">
              <label>Configuration (JSON)</label>
              <Textarea fluid rows="4" />
              <small class="text-muted-color">Optional provider-specific settings in JSON format</small>
              <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
            </FormField>
          </div>
        </template>
      </Card>
    </Form>

    <div class="flex gap-3 mt-4">
      <Button label="Save" icon="pi pi-check" form="payment-method-form" type="submit" :loading="loading" />
      <Button label="Cancel" icon="pi pi-times" severity="secondary" @click="router.push('/payment/payment-methods')" />
    </div>
  </div>
</template>
```

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/payment/views/PaymentMethodDetail.vue
git commit -m "feat(payment): implement payment method detail view"
```
