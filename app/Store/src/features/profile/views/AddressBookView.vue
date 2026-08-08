<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useConfirm } from 'primevue/useconfirm'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useAddressStore } from '../stores/addressStore'
import type { Address, AddressInput } from '../types'

usePageTitle('Address Book')
const addressStore = useAddressStore()
const confirm = useConfirm()

// Bootstrap: Fetch addresses on mount
onMounted(() => addressStore.fetchAddresses())

// Modal: Control add/edit dialog visibility
const showModal = ref(false)
const editingId = ref<string | null>(null)

// Form: Reactive fields for add/edit dialog
const formName = ref('')
const line1 = ref('')
const line2 = ref('')
const city = ref('')
const state = ref('')
const zipCode = ref('')
const countryIso = ref('')
const addressType = ref<'Shipping' | 'Billing' | 'Other'>('Shipping')
const isDefault = ref(false)

// Severity: Map address type to tag color
function typeSeverity(t: string): 'info' | 'warn' | 'success' | 'secondary' {
  if (t === 'Shipping') return 'info'
  if (t === 'Billing') return 'warn'
  return 'secondary'
}

// Open: Show create dialog with blank form
function openCreate(): void {
  editingId.value = null
  formName.value = ''
  line1.value = ''
  line2.value = ''
  city.value = ''
  state.value = ''
  zipCode.value = ''
  countryIso.value = ''
  addressType.value = 'Shipping'
  isDefault.value = false
  showModal.value = true
}

// Open: Show edit dialog pre-filled with address data
function openEdit(addr: Address): void {
  editingId.value = addr.id
  formName.value = addr.label || `${addr.firstName} ${addr.lastName || ''}`.trim()
  line1.value = addr.address1
  line2.value = addr.address2 || ''
  city.value = addr.city
  state.value = addr.stateProvince || ''
  zipCode.value = addr.zipCode || ''
  countryIso.value = addr.countryCode || ''
  addressType.value = addr.addressType
  isDefault.value = addr.isDefault
  showModal.value = true
}

// Build: Construct address input payload from form fields
function buildInput(): AddressInput {
  const [firstName, ...rest] = formName.value.trim().split(' ')
  const lastName = rest.join(' ')
  return {
    addressType: addressType.value,
    firstName: firstName || '',
    lastName: lastName || '',
    address1: line1.value,
    address2: line2.value || undefined,
    city: city.value,
    stateProvince: state.value || undefined,
    zipCode: zipCode.value || undefined,
    countryCode: countryIso.value || undefined,
    countryName: countryIso.value || '',
    isDefault: isDefault.value,
  }
}

// Save: Create or update address based on editingId
async function save(): Promise<void> {
  const input = buildInput()
  let ok: boolean
  if (editingId.value) {
    ok = await addressStore.updateAddress(editingId.value, input)
  } else {
    ok = await addressStore.createAddress(input)
  }
  if (ok) showModal.value = false
}

// Confirm: Show delete dialog before removing address
function confirmDelete(id: string): void {
  confirm.require({
    message: 'This address will be permanently removed.',
    header: 'Delete Address',
    icon: 'pi pi-exclamation-triangle',
    rejectProps: { label: 'Cancel', severity: 'secondary', outlined: true },
    acceptProps: { label: 'Delete', severity: 'danger' },
    accept: () => addressStore.deleteAddress(id),
  })
}
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Section: Page Header — breadcrumb navigation, title, and add button -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Address Book' }]" />
    <div class="flex items-center justify-between mt-4 mb-8">
      <h1 class="text-2xl font-bold text-neutral-900">Address Book</h1>
      <Button label="Add Address" icon="pi pi-plus" @click="openCreate" />
    </div>

    <!-- Section: Loading State — skeleton placeholder -->
    <div v-if="addressStore.loading" class="space-y-4">
      <Skeleton width="100%" height="6rem" />
      <Skeleton width="100%" height="6rem" />
    </div>

    <!-- Section: Error State — show error message -->
    <Message v-else-if="addressStore.error" severity="error" class="mb-4">
      {{ addressStore.error }}
    </Message>

    <!-- Section: Empty State — no addresses found -->
    <div
      v-else-if="addressStore.addresses.length === 0"
      class="text-center py-12 text-neutral-500"
    >
      <p>No addresses yet</p>
    </div>

    <!-- Section: Address Cards — list of address entries -->
    <div v-else class="grid gap-4">
      <Card v-for="addr in addressStore.addresses" :key="addr.id">
        <template #content>
          <div class="space-y-2">
            <div class="flex items-center justify-between">
              <h3 class="font-semibold text-neutral-900">
                {{ addr.firstName }} {{ addr.lastName || '' }}
              </h3>
              <div class="flex gap-2">
                <Tag :value="addr.addressType" :severity="typeSeverity(addr.addressType)" />
                <Tag v-if="addr.isDefault" value="Default" severity="success" />
              </div>
            </div>
            <p class="text-sm text-neutral-600">{{ addr.address1 }}</p>
            <p v-if="addr.address2" class="text-sm text-neutral-600">{{ addr.address2 }}</p>
            <p class="text-sm text-neutral-600">
              {{ addr.city }}<span v-if="addr.stateProvince">, {{ addr.stateProvince }}</span>
              <span v-if="addr.zipCode"> {{ addr.zipCode }}</span>
            </p>
            <p class="text-sm text-neutral-600">{{ addr.countryName }}</p>
          </div>
          <div class="flex gap-2 mt-4">
            <Button
              label="Edit"
              size="small"
              severity="secondary"
              outlined
              @click="openEdit(addr)"
            />
            <Button
              label="Delete"
              size="small"
              severity="danger"
              outlined
              @click="confirmDelete(addr.id)"
            />
          </div>
        </template>
      </Card>
    </div>

    <!-- Section: Add/Edit Dialog — modal for address form -->
    <ConfirmDialog />
    <Dialog
      v-model:visible="showModal"
      :header="editingId ? 'Edit Address' : 'Add Address'"
      :modal="true"
      :style="{ width: '500px' }"
    >
      <div class="space-y-4 py-2">
        <div>
          <label for="addr-name" class="block text-sm font-medium text-neutral-700 mb-1">
            Name
          </label>
          <InputText id="addr-name" v-model="formName" class="w-full" />
        </div>
        <div>
          <label for="addr-line1" class="block text-sm font-medium text-neutral-700 mb-1">
            Address line 1
          </label>
          <InputText id="addr-line1" v-model="line1" class="w-full" />
        </div>
        <div>
          <label for="addr-line2" class="block text-sm font-medium text-neutral-700 mb-1">
            Address line 2
          </label>
          <InputText id="addr-line2" v-model="line2" class="w-full" />
        </div>
        <div class="flex gap-4">
          <div class="flex-1">
            <label for="addr-city" class="block text-sm font-medium text-neutral-700 mb-1">
              City
            </label>
            <InputText id="addr-city" v-model="city" class="w-full" />
          </div>
          <div class="flex-1">
            <label for="addr-state" class="block text-sm font-medium text-neutral-700 mb-1">
              State
            </label>
            <InputText id="addr-state" v-model="state" class="w-full" />
          </div>
        </div>
        <div class="flex gap-4">
          <div class="flex-1">
            <label for="addr-zip" class="block text-sm font-medium text-neutral-700 mb-1">
              ZIP Code
            </label>
            <InputText id="addr-zip" v-model="zipCode" class="w-full" />
          </div>
          <div class="flex-1">
            <label for="addr-country" class="block text-sm font-medium text-neutral-700 mb-1">
              Country ISO
            </label>
            <InputText
              id="addr-country"
              v-model="countryIso"
              class="w-full"
              placeholder="US"
            />
          </div>
        </div>
        <div>
          <label for="addr-type" class="block text-sm font-medium text-neutral-700 mb-1">
            Type
          </label>
          <Select
            id="addr-type"
            v-model="addressType"
            :options="['Shipping', 'Billing', 'Other']"
            class="w-full"
          />
        </div>
        <div class="flex items-center gap-2">
          <input
            id="addr-default"
            v-model="isDefault"
            type="checkbox"
            class="rounded border-neutral-300"
          />
          <label for="addr-default" class="text-sm text-neutral-700">Set as default</label>
        </div>
      </div>
      <template #footer>
        <Button label="Cancel" severity="secondary" outlined @click="showModal = false" />
        <Button label="Save" :loading="addressStore.saving" @click="save" />
      </template>
    </Dialog>
  </div>
</template>
