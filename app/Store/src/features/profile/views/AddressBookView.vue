<script setup lang="ts">
import { onMounted, ref } from 'vue'
import * as addressApi from '../services/addressApi'
import type { Address, AddressInput } from '../types/address'
import AddressCard from '../components/AddressCard.vue'
import AddressForm from '../components/AddressForm.vue'
import { useNotify } from '@/shared/composables/useNotify'

const notify = useNotify()
const addresses = ref<Address[]>([])
const loading = ref(false)
const saving = ref(false)
const error = ref<string | null>(null)
const showForm = ref(false)
const editing = ref<Address | null>(null)

function toAddressInput(a: Address): AddressInput {
  return {
    addressType: a.addressType,
    firstName: a.firstName,
    lastName: a.lastName,
    address1: a.address1,
    address2: a.address2,
    city: a.city,
    zipCode: a.zipCode,
    phone: a.phone,
    label: a.label,
    isDefault: a.isDefault,
    countryName: a.countryName,
    stateProvince: a.stateProvince,
    countryCode: a.countryCode,
    stateCode: a.stateCode,
  }
}

async function loadAddresses(): Promise<void> {
  loading.value = true
  error.value = null
  try {
    const result = await addressApi.getAddresses()
    if (result.isSuccess) {
      addresses.value = result.items
    } else {
      addresses.value = []
      error.value = result.message ?? result.errors[0]?.message ?? 'Unable to load addresses.'
    }
  } catch {
    // The error interceptor throws HttpError on network failures / non-Result 5xx.
    addresses.value = []
    error.value = 'Unable to load addresses.'
  } finally {
    loading.value = false
  }
}

function startAdd(): void {
  editing.value = null
  showForm.value = true
}

function startEdit(address: Address): void {
  editing.value = address
  showForm.value = true
}

function cancelForm(): void {
  showForm.value = false
  editing.value = null
}

async function onFormSubmit(payload: AddressInput): Promise<void> {
  saving.value = true
  error.value = null
  try {
    const result = editing.value
      ? await addressApi.updateAddress(editing.value.id, payload)
      : await addressApi.createAddress(payload)
    if (result.isSuccess) {
      notify.success(editing.value ? 'Address updated' : 'Address added', 'Your address has been saved.')
      showForm.value = false
      editing.value = null
      await loadAddresses()
    } else {
      error.value = result.message ?? 'Unable to save the address.'
    }
  } catch {
    error.value = 'Unable to save the address.'
  } finally {
    saving.value = false
  }
}

async function onDelete(id: string): Promise<void> {
  error.value = null
  try {
    const result = await addressApi.deleteAddress(id)
    if (result.isSuccess) {
      notify.success('Address deleted', 'The address was removed.')
      await loadAddresses()
    } else {
      error.value = result.message ?? 'Unable to delete the address.'
    }
  } catch {
    error.value = 'Unable to delete the address.'
  }
}

// There is no dedicated `{id}/default` endpoint — Set default re-sends the address with
// isDefault: true through the Update route (the backend has no /default route).
async function onSetDefault(id: string): Promise<void> {
  const target = addresses.value.find((a) => a.id === id)
  if (!target) return
  saving.value = true
  error.value = null
  try {
    const result = await addressApi.updateAddress(id, { ...toAddressInput(target), isDefault: true })
    if (result.isSuccess) {
      notify.success('Default set', 'This address is now the default.')
      await loadAddresses()
    } else {
      error.value = result.message ?? 'Unable to set the default address.'
    }
  } catch {
    error.value = 'Unable to set the default address.'
  } finally {
    saving.value = false
  }
}

onMounted(loadAddresses)
</script>

<template>
  <div>
    <!-- Section: Page Header -->
    <div class="flex flex-wrap items-center justify-between gap-4 mb-6">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Addresses</h1>
        <p class="text-sm text-gray-500 mt-1">Manage your shipping and billing addresses.</p>
      </div>
      <Button v-if="!showForm" label="Add New Address" icon="pi pi-plus" @click="startAdd" />
    </div>

    <!-- Section: Error -->
    <Message v-if="error" severity="error" :closable="false" class="mb-4">{{ error }}</Message>

    <!-- Section: Loading -->
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <Skeleton v-for="i in 4" :key="i" height="12rem" class="rounded-xl" />
    </div>

    <template v-else>
      <!-- Section: Inline Add/Edit Form -->
      <div v-if="showForm" class="mb-6">
        <AddressForm
          :key="editing?.id ?? 'new'"
          :initial="editing"
          :submitting="saving"
          @submit="onFormSubmit"
          @cancel="cancelForm"
        />
      </div>

      <!-- Section: Empty -->
      <div v-if="!showForm && addresses.length === 0" class="text-center py-16">
        <i class="pi pi-map-marker text-4xl text-gray-300 mb-4 block" />
        <p class="text-gray-500">No saved addresses yet</p>
        <Button label="Add an address" severity="secondary" class="mt-4" @click="startAdd" />
      </div>

      <!-- Section: Address Cards -->
      <div v-else-if="addresses.length > 0" class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <AddressCard
          v-for="address in addresses"
          :key="address.id"
          :address="address"
          :busy="saving"
          @edit="startEdit"
          @delete="onDelete"
          @set-default="onSetDefault"
        />
      </div>
    </template>
  </div>
</template>
