# Admin Profile Views — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace 4 Profile placeholder views (ProfilesList, ProfileDetail, AddressesList, AddressDetail) with functional CRUD UIs. ProfileDetail has Profile + Addresses tabs. AddressDetail is a single-form tab.

**Architecture:** Standard list+detail pattern. ProfilesList has no create (profiles come from registration). ProfileDetail has two tabs: Profile form + Addresses table with inline add/edit/delete. AddressDetail uses single form tab.

**Tech Stack:** Vue 3 + TypeScript, PrimeVue (DataTable, Form, Tabs, Card, Select, ToggleSwitch, DatePicker), existing `ProfileApi`/`AddressApi`

**Global Constraints:**
- Follows established Catalog/Location view patterns
- All services, types, validations already exist
- View files already exist as placeholders — modify in place

---

### Task 1: ProfilesList.vue

**Files:**
- Modify: `app/Admin/src/features/profile/views/ProfilesList.vue`

**Interfaces:**
- Consumes: `ProfileApi.getProfiles(query)` → `PagedResult<ProfileListItem>`
- Consumes: `PROFILE_FILTER_FIELDS`, `PROFILE_SORT_FIELDS`, `PROFILE_SEARCH_FIELDS` from `../types/profile`
- Consumes: `PROFILE` from `@/shared/constants/api` → `${PROFILE}/profiles`
- Note: No create, no delete (profiles managed through registration)

- [ ] **Step 1: Write ProfilesList.vue**

DataTable without New or Delete buttons. Columns: First Name, Last Name, Phone, Email, Created At, Actions (View → navigates to detail). Search by name/email/phone. Reload, Export.

(Pattern: similar to `OrdersList.vue` — no create/delete. Toolbar: search + reload + export. No New button.)

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/profile/views/ProfilesList.vue
git commit -m "feat(profile): implement profiles list view"
```

---

### Task 2: ProfileDetail.vue

**Files:**
- Modify: `app/Admin/src/features/profile/views/ProfileDetail.vue`

**Interfaces:**
- Consumes: `ProfileApi.getProfile(id)` → `Result<ProfileDetail>`, `updateProfile(id, request)` → `Result<ProfileDetail>`
- Consumes: `AddressApi.getAddressesByProfile(id)` → `PagedResult<AddressListItem>` for the Addresses tab
- Consumes: `AddressApi.createAddress(request)`, `updateAddress(id, request)`, `deleteAddress(id)`
- Consumes: `profileSchema`, `ProfileForm` from `../validations/profile`

- [ ] **Step 1: Write ProfileDetail.vue**

Two-tab view:

**Tab 0 (Profile):** Form fields: First Name, Last Name, Phone Number, Date of Birth (`DatePicker`), Gender (`Select`: Male/Female/Other). Edit-only (no create — profiles are created via registration).

**Tab 1 (Addresses):** Table of addresses: Type, Street, City, State, Country, Postal Code, Is Default (Tag badge), Actions (Edit, Delete). "Add Address" button that opens an inline edit form or navigates to address detail page. Delete with confirmation dialog.

```vue
<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Card from 'primevue/card'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import DatePicker from 'primevue/datepicker'
import Tag from 'primevue/tag'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Message from 'primevue/message'
import { Form, FormField, type FormSubmitEvent, zodResolver } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { ProfileApi } from '../services/profileApi'
import { AddressApi } from '../services/addressApi'
import { profileSchema, type ProfileForm } from '../validations/profile'
import type { AddressListItem } from '../types/address'

const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const resolver = zodResolver(profileSchema)
const form = ref<ProfileForm>({ firstName: '', lastName: '', phoneNumber: '', dateOfBirth: null, gender: '' })
const formLoaded = ref(false)
const loading = ref(false)
const activeTab = ref('0')
const addresses = ref<AddressListItem[]>([])
const addressesLoaded = ref(false)
const genders = ['Male', 'Female', 'Other']

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')

async function initEditMode(id: string) {
  const result = await ProfileApi.getProfile(id)
  if (result.isSuccess) {
    const p = result.value
    form.value = {
      firstName: p.firstName ?? '',
      lastName: p.lastName ?? '',
      phoneNumber: p.phoneNumber ?? '',
      dateOfBirth: p.dateOfBirth ? new Date(p.dateOfBirth) : null,
      gender: p.gender ?? '',
    }
    formLoaded.value = true
  } else { handleResult(result); router.push('/profile/profiles') }
}

async function loadAddresses() {
  if (addressesLoaded.value) return
  const result = await AddressApi.getAddressesByProfile(route.params.id as string)
  if (result.isSuccess) { addresses.value = result.items; addressesLoaded.value = true }
}

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return
  loading.value = true
  const data = event.values as ProfileForm
  const request = {
    firstName: data.firstName, lastName: data.lastName,
    phoneNumber: data.phoneNumber || null,
    dateOfBirth: data.dateOfBirth?.toISOString() ?? null,
    gender: data.gender || null,
  }
  const result = await ProfileApi.updateProfile(route.params.id as string, request as any)
  loading.value = false
  if (result.isSuccess) notify.success('Profile', 'Saved')
  else handleResult(result)
}

function confirmDeleteAddress(id: string, label: string) {
  confirm.require({
    message: `Delete address "${label}"?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel', acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const result = await AddressApi.deleteAddress(id)
      if (result.isSuccess) {
        addresses.value = addresses.value.filter((a) => a.id !== id)
        notify.success('Deleted', label)
      } else { handleResult(result) }
    },
  })
}

function navigateToAddAddress() {
  router.push(`/profile/addresses/new?profileId=${route.params.id}`)
}

function navigateToEditAddress(id: string) {
  router.push(`/profile/addresses/${id}`)
}

onMounted(() => { if (isEdit.value) initEditMode(route.params.id as string) })
watch(() => route.params.id, (newId) => { if (newId && newId !== 'new') initEditMode(newId as string) })
watch(activeTab, (tab) => { if (tab === '1') loadAddresses() })
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="flex items-center gap-4 mb-6">
      <Button icon="pi pi-arrow-left" severity="secondary" text rounded @click="router.push('/profile/profiles')" />
      <h1 class="text-2xl font-semibold">Profile Detail</h1>
    </div>

    <Form id="profile-form" :key="String(formLoaded)" :resolver="resolver" :initial-values="form" @submit="onSubmit">
      <Tabs v-model:value="activeTab">
        <TabList>
          <Tab value="0">Profile</Tab>
          <Tab value="1">Addresses</Tab>
        </TabList>
        <TabPanels>
          <TabPanel value="0">
            <Card>
              <template #content>
                <div class="flex flex-col gap-4">
                  <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <FormField v-slot="$field" name="firstName" class="flex flex-col gap-1">
                      <label>First Name</label>
                      <InputText fluid />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                    <FormField v-slot="$field" name="lastName" class="flex flex-col gap-1">
                      <label>Last Name</label>
                      <InputText fluid />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                  </div>
                  <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <FormField v-slot="$field" name="phoneNumber" class="flex flex-col gap-1">
                      <label>Phone</label>
                      <InputText fluid />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                    <FormField v-slot="$field" name="gender" class="flex flex-col gap-1">
                      <label>Gender</label>
                      <Select :options="genders" fluid show-clear />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                  </div>
                  <FormField v-slot="$field" name="dateOfBirth" class="flex flex-col gap-1">
                    <label>Date of Birth</label>
                    <DatePicker fluid show-time />
                    <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                  </FormField>
                </div>
              </template>
            </Card>
          </TabPanel>
          <TabPanel value="1">
            <Card>
              <template #content>
                <div class="flex justify-between items-center mb-4">
                  <h3 class="text-lg font-semibold">Addresses</h3>
                  <Button label="Add Address" icon="pi pi-plus" size="small" @click="navigateToAddAddress" />
                </div>
                <DataTable :value="addresses" scrollable>
                  <Column field="addressType" header="Type" />
                  <Column field="street" header="Street" />
                  <Column field="city" header="City" />
                  <Column field="state" header="State" />
                  <Column field="country" header="Country" />
                  <Column field="postalCode" header="Postal Code" />
                  <Column field="isDefault" header="Default">
                    <template #body="{ data }"><Tag :value="data.isDefault ? 'Yes' : 'No'" :severity="data.isDefault ? 'success' : 'secondary'" /></template>
                  </Column>
                  <Column header="Actions" header-style="width:8rem">
                    <template #body="{ data }">
                      <Button icon="pi pi-pencil" severity="secondary" text rounded @click="navigateToEditAddress(data.id)" />
                      <Button icon="pi pi-trash" severity="danger" text rounded @click="confirmDeleteAddress(data.id, data.street ?? '')" />
                    </template>
                  </Column>
                  <template #empty>No addresses saved.</template>
                </DataTable>
              </template>
            </Card>
          </TabPanel>
        </TabPanels>
      </Tabs>
    </Form>

    <div class="flex gap-3 mt-4">
      <Button label="Save" icon="pi pi-check" form="profile-form" type="submit" :loading="loading" />
      <Button label="Cancel" icon="pi pi-times" severity="secondary" @click="router.push('/profile/profiles')" />
    </div>
  </div>
</template>
```

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/profile/views/ProfileDetail.vue
git commit -m "feat(profile): implement profile detail view with addresses tab"
```

---

### Task 3: AddressesList.vue

**Files:**
- Modify: `app/Admin/src/features/profile/views/AddressesList.vue`

**Interfaces:**
- Consumes: `AddressApi.getAddresses(query)` → `PagedResult<AddressListItem>`, `deleteAddress(id)` → `Result<void>`
- Consumes: `ADDRESS_FILTER_FIELDS`, `ADDRESS_SORT_FIELDS`, `ADDRESS_SEARCH_FIELDS` from `../types/address`
- Consumes: `PROFILE` → `${PROFILE}/addresses`

- [ ] **Step 1: Write AddressesList.vue**

Standard list view. Columns: Street, City, State, Country, Postal Code, Type, Is Default (Tag), Actions (Edit, Delete). Create → `/profile/addresses/new`.

(Standard pattern — see `UsersList.vue`.)

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/profile/views/AddressesList.vue
git commit -m "feat(profile): implement addresses list view"
```

---

### Task 4: AddressDetail.vue

**Files:**
- Modify: `app/Admin/src/features/profile/views/AddressDetail.vue`

**Interfaces:**
- Consumes: `AddressApi.getAddress(id)`, `createAddress(request)`, `updateAddress(id, request)`
- Consumes: `addressSchema`, `AddressForm` from `../validations/address`

- [ ] **Step 1: Write AddressDetail.vue**

Single form tab. Fields: Street, City, State, Country, Postal Code, Address Type (`Select`: Shipping/Billing/Both), Is Default (`ToggleSwitch`). If query param `profileId` present, pre-fill Profile selector.

(Pattern: single-tab form like `CountryDetail.vue` or `PaymentMethodDetail.vue`. No sub-tabs. Import ProfileApi to fetch current profile if `profileId` param is provided.)

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/profile/views/AddressDetail.vue
git commit -m "feat(profile): implement address detail view"
```
