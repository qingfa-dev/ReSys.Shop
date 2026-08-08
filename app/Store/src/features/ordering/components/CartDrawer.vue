<script setup lang="ts">
import { watch } from 'vue'
import { useCartStore } from '../stores/cartStore'

const visible = defineModel<boolean>('visible', { default: false })
const cart = useCartStore()

// Fetch: Reload cart contents each time the drawer opens.
watch(visible, (open) => {
  if (open) cart.fetchCart()
})

// Update: Guard against decrementing below quantity 1.
function updateQty(lineItemId: string, qty: number): void {
  if (qty < 1) return
  cart.updateQuantity(lineItemId, qty)
}
</script>
<template>
  <Teleport to="body">
    <Transition name="slide">
      <div v-if="visible" class="fixed inset-0 z-50 flex justify-end">
        <!-- Section: Backdrop — dismiss drawer on click -->
        <div class="absolute inset-0 bg-black/50" @click="visible = false" />
        <div class="relative w-full max-w-md bg-white shadow-xl flex flex-col">
          <!-- Section: Drawer Header — cart item count and close button -->
          <div class="flex items-center justify-between px-6 py-4 border-b border-neutral-200">
            <h2 class="text-lg font-semibold text-neutral-900">Cart ({{ cart.itemCount }})</h2>
            <Button icon="pi pi-times" text rounded @click="visible = false" />
          </div>

          <!-- Section: Drawer Body — loading skeleton, empty state, or item list -->
          <div class="flex-1 overflow-y-auto px-6 py-4">
            <div v-if="cart.loading" class="space-y-4">
              <Skeleton v-for="i in 3" :key="i" height="5rem" />
            </div>
            <div v-else-if="cart.isEmpty" class="flex flex-col items-center justify-center h-full text-center">
              <i class="pi pi-shopping-cart text-4xl text-neutral-300 mb-4" />
              <p class="text-neutral-500 mb-4">Your cart is empty</p>
              <Button label="Continue Shopping" as="router-link" to="/shop" @click="visible = false" />
            </div>
            <!-- Section: Cart Items — line items with quantity controls -->
            <ul v-else class="divide-y divide-neutral-100">
              <li v-for="item in cart.items" :key="item.id" class="flex gap-4 py-4">
                <img
                  v-if="item.productImageUrl"
                  :src="item.productImageUrl"
                  :alt="item.productName ?? ''"
                  class="w-16 h-20 object-cover rounded-md bg-neutral-100"
                />
                <div v-else class="w-16 h-20 rounded-md bg-neutral-100" />
                <div class="flex-1 min-w-0">
                  <p class="text-sm font-medium text-neutral-900 truncate">{{ item.productName ?? item.variantName }}</p>
                  <p class="text-sm text-neutral-500 mt-0.5">{{ item.sku }}</p>
                  <div class="flex items-center gap-2 mt-2">
                    <Button icon="pi pi-minus" text rounded size="small" @click="updateQty(item.id, item.quantity - 1)" />
                    <span class="text-sm font-medium w-6 text-center">{{ item.quantity }}</span>
                    <Button icon="pi pi-plus" text rounded size="small" @click="updateQty(item.id, item.quantity + 1)" />
                  </div>
                </div>
                <div class="text-sm font-medium text-neutral-900 whitespace-nowrap">
                  ${{ item.total.toFixed(2) }}
                </div>
              </li>
            </ul>
          </div>

          <!-- Section: Drawer Footer — subtotal summary and checkout CTA -->
          <div v-if="!cart.isEmpty" class="border-t border-neutral-200 px-6 py-4 space-y-3">
            <div class="flex justify-between text-sm">
              <span class="text-neutral-500">Subtotal</span>
              <span class="font-medium text-neutral-900">${{ cart.subtotal.toFixed(2) }}</span>
            </div>
            <Button
              label="Checkout"
              class="w-full"
              as="router-link"
              to="/checkout"
              @click="visible = false"
            />
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>
<style scoped>
.slide-enter-active,
.slide-leave-active {
  transition: transform 0.3s ease;
}
.slide-enter-from,
.slide-leave-to {
  transform: translateX(100%);
}
@media (prefers-reduced-motion: reduce) {
  .slide-enter-active,
  .slide-leave-active {
    transition: none;
  }
}
</style>
