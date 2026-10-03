<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import ToggleSwitch from 'primevue/toggleswitch'
import Skeleton from 'primevue/skeleton'
import Message from 'primevue/message'
import { useConfirm } from 'primevue/useconfirm'
import { useWishlistStore } from '@/features/profile/store/wishlist'
import type { Wishlist } from '@/features/profile/types/entity/wishlist.entity'

const router = useRouter()
const wishlistStore = useWishlistStore()
const confirm = useConfirm()

// Create dialog
const showCreateDialog = ref(false)
const newName = ref('')
const newIsPublic = ref(false)
const isCreating = ref(false)

// Expanded wishlist
const expandedId = ref<string | null>(null)

onMounted(() => {
  wishlistStore.fetchWishlists()
})

function toggleExpand(id: string) {
  expandedId.value = expandedId.value === id ? null : id
}

async function handleCreate() {
  if (!newName.value.trim()) return
  isCreating.value = true
  const result = await wishlistStore.createWishlist({ name: newName.value.trim(), isPublic: newIsPublic.value })
  isCreating.value = false
  if (result.isSuccess) {
    showCreateDialog.value = false
    newName.value = ''
    newIsPublic.value = false
  }
}

function confirmDelete(wishlist: Wishlist) {
  confirm.require({
    message: `Delete "${wishlist.name}"? All items will be removed.`,
    header: 'Delete Wishlist',
    icon: 'pi pi-exclamation-triangle',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: () => wishlistStore.deleteWishlist(wishlist.id),
  })
}

function formatPrice(price: number): string {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price)
}
</script>

<template>
  <div class="wishlists-view">
    <div class="page-header">
      <h1>Wishlists</h1>
      <Button label="Create Wishlist" icon="pi pi-plus" @click="showCreateDialog = true" />
    </div>

    <!-- Loading -->
    <div v-if="wishlistStore.loading" class="skeleton-grid">
      <Skeleton v-for="i in 3" :key="i" width="100%" height="160px" />
    </div>

    <!-- Empty -->
    <div v-else-if="wishlistStore.wishlists.length === 0" class="empty-state">
      <i class="pi pi-heart"></i>
      <h3>No wishlists yet</h3>
      <p>Browse products and save your favorites to a wishlist.</p>
      <Button label="Browse Products" icon="pi pi-shopping-bag" @click="router.push('/shop')" />
    </div>

    <!-- Error -->
    <Message v-if="wishlistStore.error" severity="error" :closable="false">{{ wishlistStore.error }}</Message>

    <!-- Wishlist grid -->
    <div v-else class="wishlist-grid">
      <div v-for="wishlist in wishlistStore.wishlists" :key="wishlist.id"
           class="wishlist-card" :class="{ expanded: expandedId === wishlist.id }">
        <div class="card-main" @click="toggleExpand(wishlist.id)">
          <div class="card-icon">
            <i :class="wishlist.isPublic ? 'pi pi-globe' : 'pi pi-lock'" />
          </div>
          <div class="card-info">
            <strong>{{ wishlist.name }}</strong>
            <span class="item-count">{{ wishlist.itemCount }} item(s)</span>
          </div>
          <span class="privacy-badge" :class="wishlist.isPublic ? 'public' : 'private'">
            {{ wishlist.isPublic ? 'Public' : 'Private' }}
          </span>
          <Button icon="pi pi-trash" class="p-button-text p-button-danger" severity="danger"
                  @click.stop="confirmDelete(wishlist)" aria-label="Delete wishlist" />
        </div>

        <!-- Expanded items -->
        <div v-if="expandedId === wishlist.id" class="card-items">
          <div v-if="!wishlist.items || wishlist.items.length === 0" class="items-empty">
            <p>No items in this wishlist yet.</p>
          </div>
          <div v-else class="items-scroll">
            <div v-for="item in wishlist.items" :key="item.id" class="wished-item">
              <img v-if="item.productImage" :src="item.productImage" :alt="item.productName"
                   class="item-thumb" @click="router.push('/products/' + item.productId)" />
              <div v-else class="item-thumb-placeholder">
                <i class="pi pi-image" />
              </div>
              <div class="item-info">
                <span class="item-name" @click="router.push('/products/' + item.productId)">{{ item.productName }}</span>
                <span class="item-price">{{ formatPrice(item.price) }}</span>
              </div>
              <Button icon="pi pi-times" class="p-button-text p-button-sm" severity="secondary"
                      @click="wishlistStore.removeItem(wishlist.id, item.id)" aria-label="Remove item" />
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Create dialog -->
    <Dialog v-model:visible="showCreateDialog" header="Create Wishlist" :modal="true" class="wishlist-dialog">
      <div class="dialog-field">
        <label for="wl-name">Name</label>
        <InputText id="wl-name" v-model="newName" placeholder="e.g., Summer Collection" class="full-width" />
      </div>
      <div class="dialog-field">
        <label for="wl-public">Public</label>
        <ToggleSwitch v-model="newIsPublic" input-id="wl-public" />
      </div>
      <template #footer>
        <Button label="Cancel" class="p-button-text" @click="showCreateDialog = false" />
        <Button label="Create" icon="pi pi-check" :loading="isCreating" :disabled="!newName.trim()" @click="handleCreate" />
      </template>
    </Dialog>
  </div>
</template>

<style scoped lang="scss">
.wishlists-view {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;

  h1 {
    font-family: var(--font-display);
    font-size: var(--font-size-2xl);
  }
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.skeleton-grid {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.wishlist-grid {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.wishlist-card {
  border: 1px solid var(--color-border-light);
  border-radius: var(--radius-md);
  overflow: hidden;

  &.expanded {
    border-color: var(--color-primary);
  }
}

.card-main {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 1.25rem;
  cursor: pointer;
  transition: background var(--transition-fast);

  &:hover { background: var(--color-surface-hover); }
}

.card-icon {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  background: var(--color-surface-ground);
  display: flex;
  align-items: center;
  justify-content: center;

  i { color: var(--color-text-secondary); }
}

.card-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.125rem;

  strong { font-size: var(--font-size-base); }

  .item-count {
    font-size: var(--font-size-sm);
    color: var(--color-text-secondary);
  }
}

.privacy-badge {
  padding: 0.125rem 0.5rem;
  border-radius: var(--radius-full);
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-semibold);

  &.public  { background: #dbeafe; color: #1d4ed8; }
  &.private { background: var(--color-surface-ground); color: var(--color-text-secondary); }
}

.card-items {
  border-top: 1px solid var(--color-border-light);
  padding: 0.75rem 1.25rem;
  background: var(--color-surface-ground);
}

.items-scroll {
  display: flex;
  gap: 0.75rem;
  overflow-x: auto;
}

.wished-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.375rem;
  min-width: 120px;
}

.item-thumb {
  width: 100px;
  height: 100px;
  object-fit: cover;
  border-radius: var(--radius-sm);
  cursor: pointer;
}

.item-thumb-placeholder {
  width: 100px;
  height: 100px;
  border-radius: var(--radius-sm);
  background: var(--color-surface);
  display: flex;
  align-items: center;
  justify-content: center;

  i { font-size: 2rem; color: var(--color-text-secondary); }
}

.item-info {
  text-align: center;

  .item-name {
    font-size: var(--font-size-xs);
    display: block;
    cursor: pointer;
    &:hover { color: var(--color-primary); }
  }

  .item-price {
    font-size: var(--font-size-xs);
    color: var(--color-primary);
    font-weight: var(--font-weight-semibold);
  }
}

.items-empty {
  text-align: center;
  padding: 1rem;
  color: var(--color-text-secondary);
}

.empty-state {
  text-align: center;
  padding: 4rem 1rem;

  i { font-size: 4rem; color: var(--color-text-secondary); margin-bottom: 1rem; }
  h3 { margin-bottom: 0.5rem; }
  p { color: var(--color-text-secondary); margin-bottom: 1.5rem; }
}

// Dialog
.wishlist-dialog { width: 420px; }

.dialog-field {
  margin-bottom: 1rem;

  label {
    display: block;
    margin-bottom: 0.375rem;
    font-weight: var(--font-weight-medium);
  }
}

.full-width { width: 100%; }
</style>
