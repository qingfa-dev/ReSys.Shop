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
    class="w-full sm:w-96"
    @update:visible="emit('update:visible', $event)"
  >
    <!-- Empty State: Browse the catalog when no items are in the cart -->
    <div v-if="cart.isEmpty" class="flex flex-col items-center gap-4 py-16 text-center">
      <Message severity="info" :closable="false">
        Your cart is empty.
      </Message>
      <Button as="router-link" to="/shop" label="Continue Shopping" variant="text" />
    </div>

    <!-- Items: Compact rows with thumbnail, quantity input and line total -->
    <div v-else class="flex flex-col gap-4">
      <div v-for="item in cart.items" :key="item.id" class="flex items-start gap-3">
        <Image
          v-if="item.productImageUrl"
          :src="item.productImageUrl"
          :alt="item.productName ?? item.variantName"
          imageClass="h-14 w-14 sm:h-16 sm:w-16 shrink-0 rounded-lg object-cover"
        />
        <div
          v-else
          class="flex h-14 w-14 sm:h-16 sm:w-16 shrink-0 items-center justify-center rounded-lg bg-surface-100"
        >
          <i class="pi pi-image text-xl text-placeholder" />
        </div>

        <div class="min-w-0 flex-1">
          <div class="truncate text-sm font-semibold">{{ item.productName ?? item.variantName }}</div>
          <div class="text-xs text-muted">{{ item.sku }}</div>
          <div class="mt-1 text-xs text-muted">{{ formatCurrency(item.price) }} each</div>
        </div>

        <!-- Actions: Quantity, total, and remove — right-aligned -->
        <div class="flex items-center gap-2 shrink-0">
          <InputNumber
            :model-value="item.quantity"
            :min="1"
            size="small"
            inputClass="w-12 text-center"
            aria-label="Quantity"
            @update:model-value="updateQuantity(item.id, $event)"
          />
          <span class="w-16 text-right text-sm font-semibold">{{ formatCurrency(item.total) }}</span>
          <Button
            icon="pi pi-trash"
            variant="text"
            severity="secondary"
            rounded
            size="small"
            aria-label="Remove item"
            v-tooltip.left="'Remove item'"
            @click="removeItem(item.id)"
          />
        </div>
      </div>

      <Divider />

      <!-- Totals: Cart subtotal -->
      <div class="flex items-center justify-between">
        <span class="text-sm text-muted">Subtotal</span>
        <span class="font-semibold">{{ formatCurrency(cart.subtotal) }}</span>
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
