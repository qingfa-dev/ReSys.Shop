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
import Message from 'primevue/message'
import InputText from 'primevue/inputtext'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { useProfileDetail } from '../composables/useProfileDetail'
import { ProfileApi } from '../services/profileApi'
import { AddressApi } from '../services/addressApi'
import { profileSchema, type ProfileForm } from '../validations/profile'
import type { ProfileRequest } from '../types/profile'
import type { AddressResponse } from '../types/address'

const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const { profile, fetchProfile } = useProfileDetail()

const resolver = zodResolver(profileSchema)
const form = ref<ProfileForm>({
  userId: '',
  firstName: '',
  lastName: '',
  email: '',
  phoneNumber: '',
  dateOfBirth: '',
})
const formLoaded = ref(false)
const loading = ref(false)
const activeTab = ref('0')

const addresses = ref<AddressResponse[]>([])
const addressesLoaded = ref(false)
const addressesLoading = ref(false)

async function initEditMode(userId: string) {
  // Load: Fetch the profile to seed the editable form.
  const result = await fetchProfile(userId)
  if (!result.isSuccess) {
    handleResult(result)
    router.push('/profile/profiles')
    return
  }
  const p = profile.value
  if (!p) {
    notify.error('Profile not found')
    router.push('/profile/profiles')
    return
  }
  form.value = {
    userId: p.userId,
    firstName: p.firstName ?? '',
    lastName: p.lastName ?? '',
    email: p.email ?? '',
    phoneNumber: p.phoneNumber ?? '',
    dateOfBirth: p.dateOfBirth ? p.dateOfBirth.slice(0, 10) : '',
  }
  formLoaded.value = true
}

async function loadAddresses() {
  // Load: Fetch the user's addresses lazily when the tab opens.
  if (addressesLoaded.value) return
  addressesLoading.value = true
  const userId = route.params.id as string
  const result = await AddressApi.getAddresses(userId, { userId, pageSize: 100 })
  addressesLoading.value = false
  if (result.isSuccess) {
    addresses.value = result.items
    addressesLoaded.value = true
  } else {
    handleResult(result)
  }
}

async function onSubmit(event: FormSubmitEvent) {
  // Validate: Require the form to pass validation before saving.
  if (!event.valid) return
  loading.value = true
  const data = event.values as ProfileForm
  const request: ProfileRequest = {
    userId: data.userId,
    firstName: data.firstName,
    lastName: data.lastName,
    email: data.email,
    phoneNumber: data.phoneNumber || undefined,
    dateOfBirth: data.dateOfBirth || undefined,
  }
  // Call: Persist the profile changes.
  const result = await ProfileApi.updateProfile(request)
  loading.value = false
  if (result.isSuccess) {
    notify.success('Profile updated')
    router.push('/profile/profiles')
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/profile/profiles')
}

function navigateToAddAddress() {
  router.push(`/profile/addresses/new?userId=${route.params.id}`)
}

function navigateToEditAddress(id: string) {
  router.push(`/profile/addresses/${id}?userId=${route.params.id}`)
}

function confirmDeleteAddress(id: string, label: string) {
  // Trigger: Confirm before deleting an address from the profile.
  confirm.require({
    message: `Delete address "${label}"?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      // Call: Delete the address from the profile.
      const result = await AddressApi.deleteAddress(route.params.id as string, id)
      if (result.isSuccess) {
        addresses.value = addresses.value.filter(a => a.id !== id)
        notify.success('Deleted', label)
      } else {
        handleResult(result)
      }
    },
  })
}

onMounted(() => {
  if (isEdit.value) initEditMode(route.params.id as string)
})

watch(() => route.params.id, (newId) => {
  if (newId && newId !== 'new') initEditMode(newId as string)
})

watch(activeTab, (tab) => {
  if (tab === '1') loadAddresses()
})
</script>

<template>
  <div class="flex flex-col h-full p-4">
    <!-- Section: Page Header — title and save/cancel controls -->
    <div class="flex-none flex justify-between items-start gap-4 mb-4">
      <div>
        <div class="font-semibold text-xl">Profile Detail</div>
        <p class="text-muted-color mt-1">View and edit the profile and addresses for a user.</p>
      </div>
      <div class="flex items-center gap-2 shrink-0">
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="loading" form="profile-form" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </div>
    </div>

    <!-- Section: Content Card — tabbed profile form and addresses -->
    <div class="flex-1 min-h-0 overflow-auto">
      <!-- Section: Tabs — switch between profile fields and addresses -->
      <Form id="profile-form" :key="String(formLoaded)" :resolver="resolver" :initial-values="form" @submit="onSubmit">
        <Tabs v-model:value="activeTab">
          <TabList>
            <Tab value="0">Profile</Tab>
            <Tab value="1">Addresses</Tab>
          </TabList>
          <TabPanels>
            <TabPanel value="0">
              <!-- Section: Form Fields — profile identity and contact inputs -->
              <Card>
                <template #content>
                  <div class="flex flex-col gap-4">
                    <FormField v-slot="$field" name="userId" class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">User ID <span class="text-red-500">*</span></label>
                      <InputText fluid readonly />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <FormField v-slot="$field" name="firstName" class="flex flex-col gap-1">
                        <label class="text-surface-900 dark:text-surface-0 font-medium">First Name <span class="text-red-500">*</span></label>
                        <InputText fluid />
                        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                      </FormField>
                      <FormField v-slot="$field" name="lastName" class="flex flex-col gap-1">
                        <label class="text-surface-900 dark:text-surface-0 font-medium">Last Name <span class="text-red-500">*</span></label>
                        <InputText fluid />
                        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                      </FormField>
                    </div>
                    <FormField v-slot="$field" name="email" class="flex flex-col gap-1">
                      <label class="text-surface-900 dark:text-surface-0 font-medium">Email <span class="text-red-500">*</span></label>
                      <InputText fluid />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <FormField v-slot="$field" name="phoneNumber" class="flex flex-col gap-1">
                        <label class="text-surface-900 dark:text-surface-0 font-medium">Phone Number</label>
                        <InputText fluid />
                        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                      </FormField>
                      <FormField v-slot="$field" name="dateOfBirth" class="flex flex-col gap-1">
                        <label class="text-surface-900 dark:text-surface-0 font-medium">Date of Birth</label>
                        <InputText fluid type="date" />
                        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                      </FormField>
                    </div>
                  </div>
                </template>
              </Card>
            </TabPanel>

            <TabPanel value="1">
              <!-- Section: Addresses — the user's saved addresses -->
              <Card>
                <template #content>
                  <div class="flex justify-between items-center mb-4">
                    <h3 class="text-lg font-semibold">Addresses</h3>
                    <Button label="Add Address" icon="pi pi-plus" size="small" @click="navigateToAddAddress" />
                  </div>
                  <DataTable :value="addresses" :loading="addressesLoading" scrollable data-key="id">
                    <Column field="addressType" header="Type" />
                    <Column field="firstName" header="First Name" />
                    <Column field="address1" header="Address" />
                    <Column field="city" header="City" />
                    <Column field="countryName" header="Country" />
                    <Column field="isDefault" header="Default">
                      <template #body="{ data }">
                        <Tag :value="data.isDefault ? 'Yes' : 'No'" :severity="data.isDefault ? 'success' : 'secondary'" />
                      </template>
                    </Column>
                    <Column header="Actions" header-style="width:8rem">
                      <template #body="{ data }">
                        <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEditAddress(data.id)" />
                        <Button icon="pi pi-trash" severity="danger" text rounded aria-label="Delete" @click="confirmDeleteAddress(data.id, data.label ?? data.address1)" />
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
    </div>
  </div>
</template>
