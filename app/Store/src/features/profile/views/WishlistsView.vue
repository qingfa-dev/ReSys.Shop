<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useWishlistStore } from '../stores/wishlistStore'
import { useCartStore } from '@/features/ordering/stores/cartStore'
import type { WishedItem } from '../types/wishlist'
import WishlistCard from '../components/WishlistCard.vue'
import { useNotify } from '@/shared/composables/useNotify'

const store = useWishlistStore()
const cartStore = useCartStore()
const notify = useNotify()

const expandedId = ref<string | null>(null)
const showCreate = ref(false)
const newName = ref('')
const createError = ref<string | null>(null)

async function toggleExpand(id: string): Promise<void> {
  if (expandedId.value === id) {
    expandedId.value = null
    return
  }
  if (!store.details[id]) {
    await store.fetchWishlist(id)
  }
  expandedId.value = id
}

function startCreate(): void {
  newName.value = ''
  createError.value = null
  showCreate.value = true
}

async function onCreate(): Promise<void> {
  const name = newName.value.trim()
  if (!name) {
    createError.value = 'Please enter a name for the wishlist'
    return
  }
  createError.value = null
  const ok = await store.createWishlist({ name, isPrivate: false })
  if (ok) {
    notify.success('Wishlist created', `"${name}" has been added.`)
    showCreate.value = false
  } else {
    notify.error('Create failed', store.error ?? 'Unable to create the wishlist.')
  }
}

async function onTogglePrivacy(id: string, isPrivate: boolean): Promise<void> {
  const ok = await store.updateWishlist(id, { isPrivate })
  if (ok) notify.success('Privacy updated', isPrivate ? 'Wishlist is now private.' : 'Wishlist is now public.')
  else notify.error('Update failed', store.error ?? 'Unable to update the wishlist.')
}

async function onDelete(id: string): Promise<void> {
  const ok = await store.deleteWishlist(id)
  if (ok) {
    if (expandedId.value === id) expandedId.value = null
    notify.success('Wishlist deleted', 'The wishlist was removed.')
  } else {
    notify.error('Delete failed', store.error ?? 'Unable to delete the wishlist.')
  }
}

async function onRemoveItem(listId: string, itemId: string): Promise<void> {
  const ok = await store.removeItem(listId, itemId)
  if (ok) notify.success('Item removed', 'The item was removed from the wishlist.')
  else notify.error('Remove failed', store.error ?? 'Unable to remove the item.')
}

async function onAddToCart(item: WishedItem): Promise<void> {
  const ok = await cartStore.addItem(item.variantId, item.quantity)
  if (ok) notify.success('Added to cart', 'The item was added to your cart.')
  else notify.error('Add failed', cartStore.error ?? 'Unable to add the item to your cart.')
}

onMounted(() => store.fetchWishlists())
</script>

<template>
  <div>
    <!-- Section: Page Header -->
    <div class="flex flex-wrap items-center justify-between gap-4 mb-6">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Wishlists</h1>
        <p class="text-sm text-gray-500 mt-1">Create and manage your saved product lists.</p>
      </div>
      <Button v-if="!showCreate" label="New Wishlist" icon="pi pi-plus" @click="startCreate" />
    </div>

    <!-- Section: Error -->
    <Message v-if="store.error" severity="error" :closable="false" class="mb-4">
      {{ store.error }}
    </Message>

    <!-- Section: Loading -->
    <div v-if="store.loading" class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <Skeleton v-for="i in 4" :key="i" height="10rem" class="rounded-xl" />
    </div>

    <template v-else>
      <!-- Section: Inline Create Form -->
      <div v-if="showCreate" class="mb-6 bg-white rounded-xl border border-gray-200 p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-4">New Wishlist</h3>
        <div class="flex flex-wrap items-start gap-3">
          <div class="flex-1 min-w-56">
            <InputText
              v-model="newName"
              type="text"
              fluid
              placeholder="Wishlist name (e.g. Birthday)"
              @keyup.enter="onCreate"
            />
            <Message v-if="createError" severity="error" size="small" variant="simple" class="mt-1">
              {{ createError }}
            </Message>
          </div>
          <div class="flex gap-3">
            <Button label="Cancel" severity="secondary" outlined @click="showCreate = false" />
            <Button label="Create" icon="pi pi-check" :loading="store.saving" @click="onCreate" />
          </div>
        </div>
      </div>

      <!-- Section: Empty -->
      <div v-if="!showCreate && store.items.length === 0" class="text-center py-16">
        <i class="pi pi-heart text-4xl text-gray-300 mb-4 block" />
        <p class="text-gray-500">No wishlists yet</p>
        <Button label="Create a wishlist" severity="secondary" class="mt-4" @click="startCreate" />
      </div>

      <!-- Section: Wishlist Cards -->
      <div v-else-if="store.items.length > 0" class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <WishlistCard
          v-for="wishlist in store.items"
          :key="wishlist.id"
          :wishlist="wishlist"
          :detail="store.details[wishlist.id] ?? null"
          :detail-loading="store.detailLoadingId === wishlist.id"
          :expanded="expandedId === wishlist.id"
          @toggle="toggleExpand"
          @delete="onDelete"
          @remove-item="onRemoveItem"
          @add-to-cart="onAddToCart"
          @toggle-privacy="onTogglePrivacy"
        />
      </div>
    </template>
  </div>
</template>
