<script setup lang="ts">
import { useConfirm } from 'primevue/useconfirm'
import { formatDateTimeUtc } from '@/shared/utils/date'
import type { WishlistListItem, WishlistDetail, WishedItem } from '../types/wishlist'

const props = defineProps<{
  wishlist: WishlistListItem
  detail?: WishlistDetail | null
  detailLoading?: boolean
  expanded?: boolean
}>()
const emit = defineEmits<{
  toggle: [id: string]
  delete: [id: string]
  removeItem: [listId: string, itemId: string]
  addToCart: [item: WishedItem]
  togglePrivacy: [id: string, isPrivate: boolean]
}>()
const confirm = useConfirm()

function requestDelete(): void {
  confirm.require({
    message: `Delete "${props.wishlist.name}"? This cannot be undone.`,
    header: 'Delete Wishlist',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: () => emit('delete', props.wishlist.id),
  })
}
</script>

<template>
  <div class="bg-white rounded-xl border border-stone-200 p-6">
    <div class="flex flex-wrap items-start justify-between gap-4">
      <div class="min-w-0">
        <div class="flex flex-wrap items-center gap-2">
          <h3 class="font-semibold text-stone-900">{{ wishlist.name }}</h3>
          <span
            class="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium"
            :class="wishlist.isPrivate ? 'bg-stone-100 text-stone-600' : 'bg-green-50 text-green-600'"
          >
            {{ wishlist.isPrivate ? 'Private' : 'Public' }}
          </span>
        </div>
        <p class="text-sm text-stone-500 mt-1">
          {{ wishlist.itemCount }} item{{ wishlist.itemCount === 1 ? '' : 's' }}
        </p>
      </div>
      <div class="flex items-center gap-2">
        <Button
          :label="expanded ? 'Hide' : 'View'"
          severity="secondary"
          outlined
          size="small"
          :icon="expanded ? 'pi pi-chevron-up' : 'pi pi-chevron-down'"
          @click="emit('toggle', wishlist.id)"
        />
        <Button
          :label="wishlist.isPrivate ? 'Make Public' : 'Make Private'"
          severity="secondary"
          outlined
          size="small"
          icon="pi pi-lock"
          @click="emit('togglePrivacy', wishlist.id, !wishlist.isPrivate)"
        />
        <Button
          label="Delete"
          severity="danger"
          outlined
          size="small"
          icon="pi pi-trash"
          @click="requestDelete"
        />
      </div>
    </div>

    <!-- Expanded items -->
    <div v-if="expanded" class="mt-4 border-t border-stone-100 pt-4">
      <div v-if="detailLoading" class="space-y-2">
        <Skeleton v-for="i in 2" :key="i" height="3rem" class="rounded-lg" />
      </div>
      <template v-else-if="detail && detail.wishedItems.length > 0">
        <ul class="space-y-2">
          <li
            v-for="item in detail.wishedItems"
            :key="item.id"
            class="flex flex-wrap items-center justify-between gap-3 rounded-lg bg-stone-50 px-4 py-3"
          >
            <div class="min-w-0">
              <p class="text-sm font-medium text-stone-800">Variant {{ item.variantId }}</p>
              <p class="text-xs text-stone-500 mt-0.5">
                Qty {{ item.quantity }} · Added {{ formatDateTimeUtc(item.addedAtUtc) }}
              </p>
            </div>
            <div class="flex items-center gap-2">
              <Button
                label="Add to Cart"
                size="small"
                icon="pi pi-shopping-cart"
                @click="emit('addToCart', item)"
              />
              <Button
                label="Remove"
                severity="secondary"
                outlined
                size="small"
                icon="pi pi-times"
                @click="emit('removeItem', wishlist.id, item.id)"
              />
            </div>
          </li>
        </ul>
      </template>
      <p v-else class="text-sm text-stone-500 py-2">No items in this wishlist yet.</p>
    </div>
  </div>
</template>
