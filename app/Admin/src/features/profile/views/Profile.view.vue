<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useProfileStore } from '../stores/profile.store'
import { storeToRefs } from 'pinia'
import { useToast } from '@/shared/composables/toast.use'

const store = useProfileStore()
const { profile, loading } = storeToRefs(store)
const { showToast } = useToast()

const notifyEmail = ref(true)
const notifySms = ref(false)
const notifyPush = ref(true)

const passwordForm = ref({ current: '', newPass: '', confirm: '' })
const savingPassword = ref(false)
const showAddressDialog = ref(false)
const editingAddress = ref<any>(null)

const addresses = ref([
  { id: '1', label: 'Warehouse A', street: '123 Commerce St', city: 'Portland', state: 'OR', zip: '97201', isDefault: true },
  { id: '2', label: 'Office', street: '456 Business Ave', city: 'Seattle', state: 'WA', zip: '98101', isDefault: false },
])

onMounted(() => {
  store.fetchProfile()
})

function onChangePassword() {
  if (passwordForm.value.newPass !== passwordForm.value.confirm) {
    showToast('error', 'Error', 'Passwords do not match')
    return
  }
  savingPassword.value = true
  setTimeout(() => {
    showToast('success', 'Updated', 'Password changed successfully')
    passwordForm.value = { current: '', newPass: '', confirm: '' }
    savingPassword.value = false
  }, 800)
}

function openAddressDialog(address?: any) {
  editingAddress.value = address || null
  showAddressDialog.value = true
}

function deleteAddress(id: string) {
  addresses.value = addresses.value.filter(a => a.id !== id)
  showToast('info', 'Removed', 'Address deleted')
}

function saveAddress(data: any) {
  if (editingAddress.value) {
    Object.assign(editingAddress.value, data)
  } else {
    addresses.value.push({ ...data, id: String(Date.now()) })
  }
  showAddressDialog.value = false
  showToast('success', 'Saved', 'Address saved')
}
</script>

<template>
  <div class="card">
    <div class="font-semibold text-xl mb-4">My Profile</div>

    <div v-if="loading" class="flex justify-center py-20">
      <ProgressSpinner />
    </div>

    <div v-else class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Column 1: Account details + password -->
      <div class="flex flex-col gap-6">
        <!-- Account Details -->
        <div class="card !p-6">
          <div class="text-lg font-semibold mb-4 flex items-center gap-2">
            <i class="pi pi-user text-primary"></i>
            <span>Account Details</span>
          </div>

          <div v-if="profile" class="flex flex-col gap-4">
            <div>
              <span class="text-xs font-medium uppercase tracking-wider text-surface-400">Email</span>
              <p class="text-base font-medium mt-1">{{ profile.email }}</p>
            </div>
            <div class="grid grid-cols-2 gap-4">
              <div>
                <span class="text-xs font-medium uppercase tracking-wider text-surface-400">First Name</span>
                <p class="text-base font-medium mt-1">{{ profile.firstName || '—' }}</p>
              </div>
              <div>
                <span class="text-xs font-medium uppercase tracking-wider text-surface-400">Last Name</span>
                <p class="text-base font-medium mt-1">{{ profile.lastName || '—' }}</p>
              </div>
            </div>
            <div>
              <span class="text-xs font-medium uppercase tracking-wider text-surface-400">Phone</span>
              <p class="text-base font-medium mt-1">{{ profile.phone || '—' }}</p>
            </div>
          </div>
          <div v-else class="text-sm text-surface-400 italic">Loading profile...</div>
        </div>

        <!-- Change Password -->
        <div class="card !p-6">
          <div class="text-lg font-semibold mb-4 flex items-center gap-2">
            <i class="pi pi-lock text-primary"></i>
            <span>Change Password</span>
          </div>

          <form @submit.prevent="onChangePassword" class="flex flex-col gap-4">
            <div>
              <label class="block text-sm font-medium mb-1">Current Password</label>
              <Password v-model="passwordForm.current" toggleMask class="w-full" inputClass="w-full" :feedback="false" />
            </div>
            <div>
              <label class="block text-sm font-medium mb-1">New Password</label>
              <Password v-model="passwordForm.newPass" toggleMask class="w-full" inputClass="w-full" />
            </div>
            <div>
              <label class="block text-sm font-medium mb-1">Confirm New Password</label>
              <Password v-model="passwordForm.confirm" toggleMask class="w-full" inputClass="w-full" :feedback="false" />
            </div>
            <Button type="submit" label="Update Password" icon="pi pi-check" :loading="savingPassword" class="w-full" />
          </form>
        </div>
      </div>

      <!-- Column 2: Addresses + Notifications -->
      <div class="flex flex-col gap-6">
        <!-- Addresses -->
        <div class="card !p-6">
          <div class="flex items-center justify-between mb-4">
            <div class="text-lg font-semibold flex items-center gap-2">
              <i class="pi pi-map-marker text-primary"></i>
              <span>Saved Addresses</span>
            </div>
            <Button icon="pi pi-plus" label="Add" size="small" @click="openAddressDialog()" />
          </div>

          <DataTable :value="addresses" stripedRows showGridlines size="small">
            <Column field="label" header="Label" style="min-width: 120px" />
            <Column field="street" header="Street" />
            <Column field="city" header="City" />
            <Column field="zip" header="ZIP" style="width: 80px" />
            <Column header="Default" style="width: 80px">
              <template #body="{ data }">
                <Tag v-if="data.isDefault" value="Default" severity="info" rounded />
              </template>
            </Column>
            <Column header="" style="width: 100px">
              <template #body="{ data }">
                <Button icon="pi pi-pencil" text rounded size="small" @click="openAddressDialog(data)" />
                <Button icon="pi pi-trash" text rounded severity="danger" size="small" @click="deleteAddress(data.id)" />
              </template>
            </Column>
          </DataTable>

          <p v-if="addresses.length === 0" class="text-sm text-surface-400 italic py-4 text-center">No addresses saved.</p>
        </div>

        <!-- Notifications -->
        <div class="card !p-6">
          <div class="text-lg font-semibold mb-4 flex items-center gap-2">
            <i class="pi pi-bell text-primary"></i>
            <span>Notification Preferences</span>
          </div>

          <div class="flex flex-col gap-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm font-medium">Email Notifications</p>
                <p class="text-xs text-surface-400">Order updates, invoices</p>
              </div>
              <InputSwitch v-model="notifyEmail" />
            </div>
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm font-medium">SMS Alerts</p>
                <p class="text-xs text-surface-400">Urgent inventory alerts</p>
              </div>
              <InputSwitch v-model="notifySms" />
            </div>
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm font-medium">Push Notifications</p>
                <p class="text-xs text-surface-400">New orders, fulfillment updates</p>
              </div>
              <InputSwitch v-model="notifyPush" />
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Address Dialog -->
    <Dialog v-model:visible="showAddressDialog" :header="editingAddress ? 'Edit Address' : 'New Address'" modal :style="{ width: '480px' }">
      <AddressFormDialog :address="editingAddress" @save="saveAddress" @cancel="showAddressDialog = false" />
    </Dialog>
  </div>
</template>
