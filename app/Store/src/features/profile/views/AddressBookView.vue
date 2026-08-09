<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useConfirm } from 'primevue/useconfirm'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useNotify } from '@/shared/composables/useNotify'
import { useLocationCascade } from '@/features/location/composables/useLocationCascade'
import { useAddresses } from '../composables/useAddresses'
import { AddressInputSchema } from '../validations'
import type { Address, AddressInput } from '../types'

usePageTitle('Addresses')

// Stores: Address list state plus the confirm service for row deletes.
const addressStore = useAddresses()
const confirm = useConfirm()
const notify = useNotify()
const cascade = useLocationCascade()

// Dialog: Shared create/edit modal; the editing address is null for a new row.
const dialogOpen = ref(false)
const editing = ref<Address | null>(null)

// Form: Draft fields for the address dialog, seeded on open.
const label = ref('')
const firstName = ref('')
const lastName = ref('')
const address1 = ref('')
const address2 = ref('')
const city = ref('')
const zipCode = ref('')
const phone = ref('')
const isDefault = ref(false)
const formError = ref<string | null>(null)

// Cascade: Leaf value of the CascadeSelect — the state id when a state is chosen,
// otherwise the country id (PrimeVue v5 emits only the leaf optionValue).
// Note: useLocationCascade returns refs (not store state), so script access
// goes through .value; the template unwraps them automatically.
const cascadeValue = ref<string | null>(null)
const cascadeOptions = computed(() =>
  cascade.countries.value.map((country) => {
    const states = cascade.states.value.filter((state) => state.countryId === country.id)
    return {
      id: country.id,
      name: country.name,
      ...(states.length > 0 ? { children: states } : {}),
    }
  }),
)

// Country: Resolve the selected country and state from the cascade fields.
const selectedCountry = computed(() => cascade.countries.value.find((c) => c.id === cascade.selectedCountryId.value))
const selectedState = computed(() => cascade.states.value.find((s) => s.id === cascade.selectedStateId.value))

// Label: Full country / state path for the cascade control's value display.
const cascadeLabel = computed(() =>
  selectedState.value
    ? `${selectedCountry.value?.name ?? ''} / ${selectedState.value.name}`
    : selectedCountry.value?.name ?? '',
)

// Sync: Translate the cascade leaf value (state id, else country id) into fields.
watch(cascadeValue, (leaf) => {
  const state = leaf ? cascade.states.value.find((s) => s.id === leaf) : undefined
  if (state) {
    cascade.selectedCountryId.value = state.countryId
    cascade.selectedStateId.value = state.id
    return
  }
  cascade.selectedCountryId.value = leaf ?? null
  cascade.selectedStateId.value = null
})

// Open: Seed the dialog from an existing address, or reset it for a new row.
function openDialog(address: Address | null): void {
  editing.value = address
  label.value = address?.label ?? ''
  firstName.value = address?.firstName ?? ''
  lastName.value = address?.lastName ?? ''
  address1.value = address?.address1 ?? ''
  address2.value = address?.address2 ?? ''
  city.value = address?.city ?? ''
  zipCode.value = address?.zipCode ?? ''
  phone.value = address?.phone ?? ''
  isDefault.value = address?.isDefault ?? false
  const country = address ? cascade.countries.value.find((c) => c.name === address.countryName) : undefined
  const state = address?.stateProvince ? cascade.states.value.find((s) => s.name === address.stateProvince) : undefined
  cascadeValue.value = state?.id ?? country?.id ?? null
  formError.value = null
  dialogOpen.value = true
  // Load: Fetch the location catalog lazily on first dialog use.
  void cascade.loadCountries()
}

// Save: Build the address input, validate against the shared schema, persist.
async function saveAddress(): Promise<void> {
  const input: AddressInput = {
    addressType: editing.value?.addressType ?? 'Shipping',
    firstName: firstName.value,
    lastName: lastName.value || undefined,
    address1: address1.value,
    address2: address2.value || undefined,
    city: city.value,
    zipCode: zipCode.value || undefined,
    phone: phone.value || undefined,
    label: label.value || undefined,
    isDefault: isDefault.value,
    countryName: selectedCountry.value?.name ?? '',
    countryCode: selectedCountry.value?.isoCode ?? undefined,
    stateProvince: selectedState.value?.name ?? undefined,
    stateCode: selectedState.value?.abbreviation ?? undefined,
  }
  const parsed = AddressInputSchema.safeParse(input)
  if (!parsed.success) {
    formError.value = 'Complete the required fields: name, address, city and country.'
    return
  }
  const ok = editing.value
    ? await addressStore.updateAddress(editing.value.id, parsed.data)
    : await addressStore.createAddress(parsed.data)
  if (ok) {
    dialogOpen.value = false
    notify.success(editing.value ? 'Address updated' : 'Address added')
  } else {
    formError.value = addressStore.error ?? 'Could not save the address'
  }
}

// Default: Re-submit the address with the default flag so the server re-assigns it.
async function setDefault(address: Address): Promise<void> {
  const input: AddressInput = {
    addressType: address.addressType,
    firstName: address.firstName,
    lastName: address.lastName ?? undefined,
    address1: address.address1,
    address2: address.address2 ?? undefined,
    city: address.city,
    zipCode: address.zipCode ?? undefined,
    phone: address.phone ?? undefined,
    label: address.label ?? undefined,
    isDefault: true,
    countryName: address.countryName,
    countryCode: address.countryCode ?? undefined,
    stateProvince: address.stateProvince ?? undefined,
    stateCode: address.stateCode ?? undefined,
  }
  const ok = await addressStore.updateAddress(address.id, input)
  if (ok) notify.success('Default address updated')
  else notify.error(addressStore.error ?? 'Could not update the default address')
}

// Confirm: Ask before removing an address, then delegate to the store.
function confirmDelete(address: Address): void {
  confirm.require({
    message: `Delete the address "${address.label ?? address.address1}"?`,
    header: 'Delete Address',
    icon: 'pi pi-exclamation-triangle',
    acceptProps: { label: 'Delete', severity: 'danger' },
    rejectProps: { label: 'Cancel', severity: 'secondary', outlined: true },
    accept: () => void removeAddress(address),
  })
}

// Remove: Persist the deletion and toast the outcome.
async function removeAddress(address: Address): Promise<void> {
  const ok = await addressStore.deleteAddress(address.id)
  if (ok) notify.success('Address deleted')
  else notify.error(addressStore.error ?? 'Could not delete the address')
}

// Format: One-line display of the full street block for the table.
function addressLines(address: Address): string {
  const parts = [address.address1, address.address2, address.city].filter(Boolean)
  const region = [address.stateProvince, address.countryName].filter(Boolean).join(', ')
  if (region) parts.push(region)
  return parts.join(', ')
}

onMounted(() => {
  // Load: Refresh the address list on page entry.
  void addressStore.fetchAddresses()
})
</script>

<template>
  <!-- Section: Content Card — address table with create/edit dialog actions -->
  <Card>
    <template #title>Addresses</template>
    <template #content>
      <!-- Section: Error State -->
      <Message v-if="addressStore.error" severity="error" :closable="false" class="mb-4">
        {{ addressStore.error }}
      </Message>

      <!-- Section: Loading State -->
      <div v-if="addressStore.loading" class="flex flex-col gap-3">
        <Skeleton v-for="i in 3" :key="i" height="4rem" />
      </div>

      <!-- Section: Data Table — addresses with default marker and row actions -->
      <DataTable v-else :value="addressStore.addresses" dataKey="id" size="small">
        <!-- Section: Table Columns -->
        <Column header="Label">
          <template #body="{ data }">
            <Tag :value="data.label ?? 'Address'" severity="secondary" rounded />
          </template>
        </Column>
        <Column field="firstName" header="Name">
          <template #body="{ data }">{{ data.firstName }} {{ data.lastName ?? '' }}</template>
        </Column>
        <Column header="Address">
          <template #body="{ data }">
            <span class="text-sm text-muted">{{ addressLines(data) }}</span>
          </template>
        </Column>
        <Column header="Default">
          <template #body="{ data }">
            <Tag v-if="data.isDefault" value="Default" severity="success" rounded />
          </template>
        </Column>

        <!-- Section: Row Actions — edit, delete with confirmation, set default -->
        <Column header="Actions">
          <template #body="{ data }">
            <div class="flex items-center gap-1">
              <Button
                icon="pi pi-pencil"
                variant="text"
                severity="secondary"
                rounded
                aria-label="Edit address"
                v-tooltip.bottom="'Edit address'"
                @click="openDialog(data)"
              />
              <Button
                icon="pi pi-trash"
                variant="text"
                severity="danger"
                rounded
                aria-label="Delete address"
                v-tooltip.bottom="'Delete address'"
                @click="confirmDelete(data)"
              />
              <Button
                v-if="!data.isDefault"
                label="Set default"
                variant="text"
                size="small"
                class="ml-2"
                @click="setDefault(data)"
              />
            </div>
          </template>
        </Column>

        <!-- Section: Empty State -->
        <template #empty>
          <Message severity="info" :closable="false">No addresses yet — add one to speed up checkout.</Message>
        </template>
      </DataTable>

      <!-- Section: Add Action — opens the shared address dialog for a new row -->
      <div class="mt-4">
        <Button label="Add Address" icon="pi pi-plus" @click="openDialog(null)" />
      </div>
    </template>
  </Card>

  <!-- Section: Address Dialog — create/edit form in a Fluid layout -->
  <Dialog
    v-model:visible="dialogOpen"
    :header="editing ? 'Edit Address' : 'Add Address'"
    modal
    class="w-full max-w-lg"
  >
    <Fluid class="flex flex-col gap-4">
      <!-- Form Fields: Core address inputs with float labels -->
      <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <FloatLabel variant="on">
          <InputText id="address-first-name" v-model="firstName" fluid />
          <Label for="address-first-name">First name</Label>
        </FloatLabel>
        <FloatLabel variant="on">
          <InputText id="address-last-name" v-model="lastName" fluid />
          <Label for="address-last-name">Last name</Label>
        </FloatLabel>
      </div>
      <FloatLabel variant="on">
        <InputText id="address-label" v-model="label" fluid />
        <Label for="address-label">Label (e.g. Home)</Label>
      </FloatLabel>
      <FloatLabel variant="on">
        <InputText id="address-address1" v-model="address1" fluid />
        <Label for="address-address1">Street address</Label>
      </FloatLabel>
      <FloatLabel variant="on">
        <Textarea id="address-address2" v-model="address2" rows="2" fluid />
        <Label for="address-address2">Notes (apartment, landmark…)</Label>
      </FloatLabel>
      <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <FloatLabel variant="on">
          <InputText id="address-city" v-model="city" fluid />
          <Label for="address-city">City</Label>
        </FloatLabel>
        <FloatLabel variant="on">
          <InputText id="address-zip" v-model="zipCode" fluid />
          <Label for="address-zip">ZIP / postal code</Label>
        </FloatLabel>
      </div>
      <FloatLabel variant="on">
        <InputMask id="address-phone" v-model="phone" mask="(999) 999-9999" fluid />
        <Label for="address-phone">Phone</Label>
      </FloatLabel>

      <!-- Label-Paired Input: Country/state cascade sits below its own label -->
      <div>
        <Label for="address-country" class="mb-1 block text-sm font-medium">Country / State</Label>
        <CascadeSelect
          id="address-country"
          v-model="cascadeValue"
          :options="cascadeOptions"
          optionLabel="name"
          optionValue="id"
          optionGroupLabel="name"
          optionGroupChildren="children"
          placeholder="Country / State"
          class="w-full"
        >
          <!-- Label: Show the full country / state path instead of the leaf only -->
          <template #value="{ placeholder }">{{ cascadeLabel || placeholder }}</template>
        </CascadeSelect>
      </div>

      <!-- Default Toggle: Mark the saved row as the default address -->
      <div class="flex items-center justify-between rounded-lg border border-surface-200 px-3 py-2">
        <Label for="address-default" class="text-sm font-medium">Set as default address</Label>
        <ToggleSwitch id="address-default" v-model="isDefault" />
      </div>

      <!-- Feedback: Inline message for validation and API errors -->
      <Message v-if="formError" severity="warn" :closable="false">{{ formError }}</Message>
    </Fluid>

    <template #footer>
      <Button label="Cancel" severity="secondary" variant="text" @click="dialogOpen = false" />
      <Button label="Save Address" icon="pi pi-check" :loading="addressStore.saving" @click="saveAddress" />
    </template>
  </Dialog>
</template>
