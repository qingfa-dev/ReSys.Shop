<script setup lang="ts">
import Label from 'primevue/label'
import { computed, onMounted, ref, watch } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useNotify } from '@/shared/composables/useNotify'
import { formatDateTimeUtc } from '@/shared/utils/date'
import { useWishlists } from '../composables/useWishlists'
import type { WishlistDetail } from '../types'

usePageTitle('My Wishlists')

// Store: Wishlist summaries plus the per-list detail cache.
const wishlistStore = useWishlists()
const notify = useNotify()

// Tabs: Active tab key mirrors the selected wishlist id (Tabs v5 emits string|number).
const activeTab = ref<string | number>('')

// Create: New-list dialog state.
const createOpen = ref(false)
const newName = ref('')

// Tabs: Build tab models from the list summaries, keeping the item count handy.
const tabItems = computed(() =>
  wishlistStore.lists.map((list) => ({
    id: list.id,
    name: list.name,
    itemCount: list.itemCount,
  })),
)

// Detail: Active list detail resolved from the store cache by the tab key.
const activeDetail = computed<WishlistDetail | null>(() =>
  activeTab.value ? (wishlistStore.details[String(activeTab.value)] ?? null) : null,
)

// Select: Keep the first list active while no tab is chosen yet.
watch(
  () => wishlistStore.lists,
  (lists) => {
    if (!activeTab.value && lists.length > 0) activeTab.value = lists[0]?.id ?? ''
  },
  { immediate: true },
)

// Open: Reset the create form and show the dialog.
function openCreate(): void {
  newName.value = ''
  createOpen.value = true
}

// Create: Persist the new list, refresh details, then select the new tab.
async function onCreate(): Promise<void> {
  const name = newName.value.trim()
  if (!name) {
    notify.warn('Enter a wishlist name')
    return
  }
  const ok = await wishlistStore.createWishlist({ name, isPrivate: false })
  if (ok) {
    createOpen.value = false
    await wishlistStore.fetchWishlists()
    notify.success('Wishlist created')
  } else {
    notify.error(wishlistStore.error ?? 'Could not create the wishlist')
  }
}

// Remove: Delete one item from the active list and toast the outcome.
async function onRemoveItem(itemId: string): Promise<void> {
  if (!activeTab.value) return
  const ok = await wishlistStore.removeItem(String(activeTab.value), itemId)
  if (ok) notify.success('Item removed from wishlist')
  else notify.error(wishlistStore.error ?? 'Could not remove the item')
}

onMounted(() => {
  // Load: Refresh wishlists; the list watcher selects the first tab.
  void wishlistStore.fetchWishlists()
})
</script>

<template>
  <!-- Section: Content Card — per-list tabs with item rows and a create action -->
  <Card>
    <template #title>My Wishlists</template>
    <template #content>
      <!-- Section: Error State -->
      <Message v-if="wishlistStore.error" severity="error" :closable="false" class="mb-4">
        {{ wishlistStore.error }}
      </Message>

      <!-- Section: Empty State — no lists yet -->
      <div v-if="wishlistStore.lists.length === 0 && !wishlistStore.loading" class="flex flex-col items-center gap-4 py-12">
        <Message severity="info" :closable="false">You have no wishlists yet.</Message>
        <Button label="Create your first list" icon="pi pi-plus" @click="openCreate" />
      </div>

      <!-- Section: Tabs — one tab per wishlist, item counts as tags -->
      <div v-else class="flex flex-col gap-4">
        <Tabs v-model:value="activeTab">
          <TabList>
            <Tab v-for="tab in tabItems" :key="tab.id" :value="tab.id">
              <span class="flex items-center gap-2">
                {{ tab.name }}
                <Tag :value="String(tab.itemCount)" severity="secondary" rounded />
              </span>
            </Tab>
          </TabList>
          <TabPanels>
            <TabPanel v-for="tab in tabItems" :key="tab.id" :value="tab.id">
              <!-- Section: Item Rows — wished variants with quantity and removal -->
              <DataView v-if="activeDetail" :value="activeDetail.wishedItems" layout="list">
                <template #list="{ items }">
                  <div v-for="item in items" :key="item.id" class="flex items-center gap-4 border-b border-surface-200 py-4 last:border-b-0">
                    <div class="flex h-14 w-14 shrink-0 items-center justify-center rounded-lg bg-surface-100 text-muted">
                      <i class="pi pi-heart-fill" />
                    </div>
                    <div class="min-w-0 flex-1">
                      <div class="font-mono text-sm font-medium">{{ item.variantId }}</div>
                      <div class="mt-0.5 text-sm text-muted">
                        Qty {{ item.quantity }} · added {{ formatDateTimeUtc(item.addedAtUtc) }}
                      </div>
                    </div>
                    <Button
                      icon="pi pi-trash"
                      variant="text"
                      severity="danger"
                      rounded
                      aria-label="Remove item"
                      v-tooltip.left="'Remove from wishlist'"
                      @click="onRemoveItem(item.id)"
                    />
                  </div>
                </template>
                <template #empty>
                  <Message severity="info" :closable="false">This wishlist is empty.</Message>
                </template>
              </DataView>
            </TabPanel>
          </TabPanels>
        </Tabs>

        <!-- Section: Add Action — creates a new list via dialog -->
        <div>
          <Button label="New list" icon="pi pi-plus" variant="outlined" @click="openCreate" />
        </div>
      </div>
    </template>
  </Card>

  <!-- Section: Create Dialog — single name field for the new list -->
  <Dialog v-model:visible="createOpen" header="New Wishlist" modal class="w-full max-w-sm">
    <Fluid>
      <FloatLabel variant="on">
        <InputText id="wishlist-name" v-model="newName" fluid />
        <Label for="wishlist-name">Wishlist name</Label>
      </FloatLabel>
    </Fluid>
    <template #footer>
      <Button label="Cancel" severity="secondary" variant="text" @click="createOpen = false" />
      <Button label="Create" icon="pi pi-check" :loading="wishlistStore.saving" @click="onCreate" />
    </template>
  </Dialog>
</template>
