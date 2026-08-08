<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useConfirm } from 'primevue/useconfirm'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useWishlistStore } from '../stores/wishlistStore'

usePageTitle('My Wishlists')
const wishlistStore = useWishlistStore()
const confirm = useConfirm()

// Bootstrap: Fetch wishlists on mount
onMounted(() => wishlistStore.fetchWishlists())

// Modal: Control create dialog visibility
const showCreate = ref(false)
const newName = ref('')
const newDescription = ref('')
const newVisibility = ref<'Public' | 'Private'>('Public')

// Visibility: Map visibility label to isPrivate boolean
const visibilityOptions = ['Public', 'Private']

// Open: Reset create form and show dialog
function openCreate(): void {
  newName.value = ''
  newDescription.value = ''
  newVisibility.value = 'Public'
  showCreate.value = true
}

// Create: Persist new wishlist via store
async function create(): Promise<void> {
  const ok = await wishlistStore.createWishlist({
    name: newName.value,
    isPrivate: newVisibility.value === 'Private',
  })
  if (ok) showCreate.value = false
}

// Confirm: Show delete dialog before removing wishlist
function confirmDelete(id: string): void {
  confirm.require({
    message: 'This wishlist and all its items will be removed.',
    header: 'Delete Wishlist',
    icon: 'pi pi-exclamation-triangle',
    rejectProps: { label: 'Cancel', severity: 'secondary', outlined: true },
    acceptProps: { label: 'Delete', severity: 'danger' },
    accept: () => wishlistStore.deleteWishlist(id),
  })
}

// Severity: Map visibility to tag color
function visibilitySeverity(isPrivate: boolean): 'success' | 'secondary' {
  return isPrivate ? 'secondary' : 'success'
}
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Section: Page Header — breadcrumb navigation, title, and create button -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'My Wishlists' }]" />
    <div class="flex items-center justify-between mt-4 mb-8">
      <h1 class="text-2xl font-bold text-neutral-900">My Wishlists</h1>
      <Button label="New Wishlist" icon="pi pi-plus" @click="openCreate" />
    </div>

    <!-- Section: Loading State — skeleton placeholder -->
    <div v-if="wishlistStore.loading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      <Skeleton v-for="i in 3" :key="i" width="100%" height="10rem" />
    </div>

    <!-- Section: Error State — show error message -->
    <Message v-else-if="wishlistStore.error" severity="error" class="mb-4">
      {{ wishlistStore.error }}
    </Message>

    <!-- Section: Empty State — no wishlists found -->
    <div
      v-else-if="wishlistStore.lists.length === 0"
      class="text-center py-12 text-neutral-500"
    >
      <p>No wishlists yet</p>
    </div>

    <!-- Section: Wishlist Cards — grid of wishlist entries -->
    <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      <Card v-for="list in wishlistStore.lists" :key="list.id">
        <template #content>
          <div class="space-y-2">
            <div class="flex items-center justify-between">
              <h3 class="font-semibold text-neutral-900">{{ list.name }}</h3>
              <Tag
                :value="list.isPrivate ? 'Private' : 'Public'"
                :severity="visibilitySeverity(list.isPrivate)"
              />
            </div>
            <p class="text-sm text-neutral-500">{{ list.itemCount }} items</p>
          </div>
          <div class="mt-4">
            <Button
              label="Delete"
              size="small"
              severity="danger"
              outlined
              @click="confirmDelete(list.id)"
            />
          </div>
        </template>
      </Card>
    </div>

    <!-- Section: Create Dialog — modal for new wishlist form -->
    <ConfirmDialog />
    <Dialog
      v-model:visible="showCreate"
      header="New Wishlist"
      :modal="true"
      :style="{ width: '450px' }"
    >
      <div class="space-y-4 py-2">
        <div>
          <label for="wishlist-name" class="block text-sm font-medium text-neutral-700 mb-1">
            Name
          </label>
          <InputText id="wishlist-name" v-model="newName" class="w-full" />
        </div>
        <div>
          <label for="wishlist-desc" class="block text-sm font-medium text-neutral-700 mb-1">
            Description
          </label>
          <InputText id="wishlist-desc" v-model="newDescription" class="w-full" />
        </div>
        <div>
          <label for="wishlist-vis" class="block text-sm font-medium text-neutral-700 mb-1">
            Visibility
          </label>
          <Select
            id="wishlist-vis"
            v-model="newVisibility"
            :options="visibilityOptions"
            class="w-full"
          />
        </div>
      </div>
      <template #footer>
        <Button label="Cancel" severity="secondary" outlined @click="showCreate = false" />
        <Button label="Create" :loading="wishlistStore.saving" @click="create" />
      </template>
    </Dialog>
  </div>
</template>
