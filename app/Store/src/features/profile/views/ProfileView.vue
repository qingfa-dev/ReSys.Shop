<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useConfirm } from 'primevue/useconfirm'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useProfileStore } from '../stores/profileStore'

usePageTitle('My Profile')
const profileStore = useProfileStore()
const confirm = useConfirm()

const firstName = ref('')
const lastName = ref('')
const email = ref('')
const phone = ref('')

// Watch: Sync form fields when profile loads after init
watch(
  () => profileStore.profile,
  (p) => {
    if (p) {
      firstName.value = p.firstName
      lastName.value = p.lastName
      email.value = p.email
      phone.value = p.phoneNumber || ''
    }
  },
  { immediate: true },
)

// Bootstrap: Load profile from server on mount
onMounted(() => profileStore.init())

// Submit: Persist profile changes via store
async function onSubmit(): Promise<void> {
  await profileStore.updateProfile({
    firstName: firstName.value,
    lastName: lastName.value,
    email: email.value,
    phoneNumber: phone.value || undefined,
  })
}

// Confirm: Show delete-account dialog before irreversible removal
function confirmDelete(): void {
  confirm.require({
    message: 'Your account and all associated data will be permanently deleted.',
    header: 'Delete Account',
    icon: 'pi pi-exclamation-triangle',
    rejectProps: { label: 'Cancel', severity: 'secondary', outlined: true },
    acceptProps: { label: 'Delete', severity: 'danger' },
    accept: () => profileStore.deleteProfile(),
  })
}
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Section: Page Header — breadcrumb navigation and title -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'My Profile' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">My Profile</h1>

    <!-- Section: Loading State — skeleton placeholder while profile loads -->
    <Card v-if="profileStore.loading">
      <template #content>
        <div class="space-y-4">
          <Skeleton width="100%" height="2.5rem" />
          <Skeleton width="100%" height="2.5rem" />
          <Skeleton width="100%" height="2.5rem" />
          <Skeleton width="100%" height="2.5rem" />
        </div>
      </template>
    </Card>

    <!-- Section: Error State — show error message -->
    <Message v-else-if="profileStore.error" severity="error" class="mb-4">
      {{ profileStore.error }}
    </Message>

    <!-- Section: Content Card — profile form fields -->
    <Card v-else>
      <template #content>
        <!-- Section: Form Fields — first name, last name, email, phone -->
        <form @submit.prevent="onSubmit" class="space-y-4">
          <div>
            <label for="firstName" class="block text-sm font-medium text-neutral-700 mb-1">
              First name
            </label>
            <InputText id="firstName" v-model="firstName" class="w-full" />
          </div>

          <div>
            <label for="lastName" class="block text-sm font-medium text-neutral-700 mb-1">
              Last name
            </label>
            <InputText id="lastName" v-model="lastName" class="w-full" />
          </div>

          <div>
            <label for="email" class="block text-sm font-medium text-neutral-700 mb-1">
              Email
            </label>
            <InputText id="email" v-model="email" type="email" class="w-full" />
          </div>

          <div>
            <label for="phone" class="block text-sm font-medium text-neutral-700 mb-1">
              Phone
            </label>
            <InputText id="phone" v-model="phone" class="w-full" />
          </div>

          <!-- Section: Action Footer — save button -->
          <Button
            type="submit"
            label="Save"
            :loading="profileStore.saving"
          />
        </form>
      </template>
    </Card>

    <!-- Section: Danger Zone — delete account with confirmation -->
    <ConfirmDialog />
    <Card v-if="!profileStore.loading">
      <template #content>
        <div class="space-y-4">
          <h2 class="text-lg font-semibold text-red-600">Danger Zone</h2>
          <p class="text-sm text-neutral-500">
            Permanently delete your account and all associated data.
          </p>
          <Button
            label="Delete Account"
            severity="danger"
            outlined
            @click="confirmDelete"
          />
        </div>
      </template>
    </Card>
  </div>
</template>
