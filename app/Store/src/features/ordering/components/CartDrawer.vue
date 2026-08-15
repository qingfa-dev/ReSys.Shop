<script setup lang="ts">
import { formatCurrency } from '@/shared/utils/currency'
import { useCart } from '../composables/useCart'

const props = defineProps<{ visible: boolean }>()
const emit = defineEmits<{ 'update:visible': [value: boolean] }>()

const cart = useCart()

// Guard: Fall back to the minimum quantity when the input is cleared.
function updateQuantity(lineItemId: string, quantity: number | null): void {
  void cart.updateQuantity(lineItemId, quantity ?? 1)
}

function removeItem(lineItemId: string): void {
  void cart.removeItem(lineItemId)
}
</script>

<template>
  <!-- Section: Cart Drawer — slide-in panel with line items, totals and checkout footer -->
  <Drawer
    :visible="props.visible"
    position="right"
    header="Shopping Cart"
    class="w-full sm:w-[28rem]"
    @update:visible="emit('update:visible', $event)"
  >
    <!-- Empty State: Browse the catalog when no items are in the cart -->
    <div v-if="cart.isEmpty" class="flex flex-col items-center gap-4 py-16 text-center">
      <Message severity="info" :closable="false">
        Your cart is empty.
      </Message>
      <Button as="router-link" to="/shop" label="Continue Shopping" variant="text" />
    </div>

    <!-- Items: Two-zone tickets — identity block above a stepper and line-total row -->
    <div v-else class="flex flex-col gap-3">
      <div
        v-for="item in cart.items"
        :key="item.id"
        class="flex flex-col rounded-lg border border-surface-200 bg-surface-0 p-3"
      >
        <!-- Identity: Thumbnail, name and variant meta; remove sits top-right -->
        <div class="flex gap-3">
          <Image
            v-if="item.productImageUrl"
            :src="item.productImageUrl"
            :alt="item.productName ?? item.variantName"
            imageClass="h-14 w-14 shrink-0 rounded-md object-cover"
          />
          <div
            v-else
            class="flex h-14 w-14 shrink-0 items-center justify-center rounded-md bg-surface-100"
          >
            <i class="pi pi-image text-lg text-placeholder" />
          </div>

          <div class="min-w-0 flex-1">
            <div class="truncate text-sm font-semibold">{{ item.productName ?? item.variantName }}</div>
            <div class="text-xs text-muted">{{ item.sku }}</div>
            <div class="mt-0.5 text-xs text-muted">{{ formatCurrency(item.price) }} each</div>
          </div>

          <Button
            icon="pi pi-trash"
            variant="text"
            rounded
            size="small"
            class="text-muted! hover:text-danger!"
            aria-label="Remove item"
            v-tooltip.left="'Remove item'"
            @click="removeItem(item.id)"
          />
        </div>

        <!-- Controls: Quantity stepper and line total anchored beneath the hairline -->
        <div class="mt-3 flex min-w-0 items-center justify-between gap-2 border-t border-surface-200 pt-3">
          <InputNumber
            :model-value="item.quantity"
            :min="1"
            size="small"
            show-buttons
            button-layout="horizontal"
            inputClass="w-9 text-center"
            class="shrink-0"
            aria-label="Quantity"
            @update:model-value="updateQuantity(item.id, $event)"
          />
          <span class="shrink-0 text-sm font-semibold">{{ formatCurrency(item.total) }}</span>
        </div>
      </div>

      <Divider />

      <!-- Totals: Subtotal, shipping and server-computed grand total -->
      <div class="flex items-center justify-between">
        <span class="text-sm text-muted">Subtotal</span>
        <span class="font-semibold">{{ formatCurrency(cart.subtotal) }}</span>
      </div>
      <div class="mt-1 flex items-center justify-between">
        <span class="text-sm text-muted">Shipping</span>
        <span class="text-sm font-semibold">{{ formatCurrency(cart.shipping) }}</span>
      </div>
      <Divider class="my-2" />
      <div class="flex items-center justify-between">
        <span class="text-sm font-semibold">Total</span>
        <span class="font-semibold">{{ formatCurrency(cart.total) }}</span>
      </div>
    </div>

    <!-- Footer: Checkout and full cart navigation -->
    <template #footer>
      <div v-if="!cart.isEmpty" class="flex flex-col gap-2">
        <Button as="router-link" to="/checkout" label="Checkout" icon="pi pi-arrow-right" iconPos="right" class="w-full" />
        <Button as="router-link" to="/cart" label="View Cart" variant="text" class="w-full" />
      </div>
    </template>
  </Drawer>
</template>
