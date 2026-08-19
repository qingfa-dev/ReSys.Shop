<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useNotify } from '@/shared/composables/useNotify'
import { formatCurrency } from '@/shared/utils/currency'
import { useCart } from '../composables/useCart'

usePageTitle('Cart')

const cart = useCart()
const notify = useNotify()

// Promo: Local coupon entry until the cart API exposes server-side codes.
const couponCode = ref('')
const couponApplied = ref(false)

// Total: Server-computed grand total (items + shipping + adjustments).
const total = computed(() => cart.total)

onMounted(() => {
  // Load: Refresh cart items on page entry.
  void cart.fetchCart()
})

// Guard: Keep quantity at one unit minimum when the input is cleared.
function updateQuantity(lineItemId: string, quantity: number | null): void {
  void cart.updateQuantity(lineItemId, quantity ?? 1)
}

function removeItem(lineItemId: string): void {
  void cart.removeItem(lineItemId)
}

// Apply: Validate the promo field, then surface the availability notice (no backend yet).
function applyCoupon(): void {
  const code = couponCode.value.trim()
  if (!code) { notify.warn('Enter a promo code'); return }
  couponApplied.value = true
  notify.info('Promo codes are coming soon')
}

function clearCoupon(): void {
  couponApplied.value = false
  couponCode.value = ''
}
</script>

<template>
  <!-- Section: Page Header — title for the cart surface -->
  <div class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
    <h1 class="mb-8 text-2xl font-bold">Shopping Cart</h1>

    <!-- Empty State: Browse the catalog when no items are in the cart -->
    <div v-if="cart.isEmpty" class="flex flex-col items-center gap-4 py-16 text-center">
      <Message severity="info" :closable="false">
        Your cart is empty.
      </Message>
      <Button as="router-link" to="/shop" label="Continue Shopping" variant="text" />
    </div>

    <!-- Content: Line-item DataView beside the order summary card -->
    <div v-else class="grid grid-cols-1 gap-8 lg:grid-cols-[1fr_22rem]">
      <!-- Items: List of line items with quantity controls and remove actions -->
      <DataView :value="cart.items" layout="list">
        <template #list="{ items }">
          <div v-for="item in items" :key="item.id" class="flex items-center gap-4 border-b border-surface-200 py-4">
            <Image
              v-if="item.productImageUrl"
              :src="item.productImageUrl"
              :alt="item.productName ?? item.variantName"
              imageClass="h-20 w-20 shrink-0 rounded-lg object-cover"
            />
            <div
              v-else
              class="flex h-20 w-20 shrink-0 items-center justify-center rounded-lg bg-surface-100"
            >
              <i class="pi pi-image text-2xl text-placeholder" />
            </div>

            <div class="min-w-0 flex-1">
              <RouterLink
                v-if="item.productId"
                :to="`/products/${item.productId}`"
                class="truncate font-semibold text-brand hover:underline"
              >
                {{ item.productName ?? item.variantName }}
              </RouterLink>
              <div v-else class="truncate font-semibold">{{ item.productName ?? item.variantName }}</div>
              <div class="text-sm text-muted">{{ item.sku }}</div>
              <div class="mt-1 text-sm text-muted">{{ formatCurrency(item.price) }} each</div>
            </div>

            <InputNumber
              :model-value="item.quantity"
              :min="1"
              aria-label="Quantity"
              @update:model-value="updateQuantity(item.id, $event)"
            />

            <div class="w-20 text-right font-semibold">{{ formatCurrency(item.total) }}</div>

            <Button
              icon="pi pi-trash"
              variant="text"
              severity="secondary"
              rounded
              aria-label="Remove item"
              v-tooltip.left="'Remove item'"
              @click="removeItem(item.id)"
            />
          </div>
        </template>
      </DataView>

      <!-- Summary: Subtotal, shipping, adjustments, tax placeholder, total and promo entry -->
      <Card class="self-start">
        <template #title>Order Summary</template>
        <template #content>
          <div class="flex flex-col gap-3">
            <div class="flex items-center justify-between text-sm">
              <span class="text-muted">Items ({{ cart.itemCount }})</span>
              <span>{{ formatCurrency(cart.subtotal) }}</span>
            </div>
            <div class="flex items-center justify-between text-sm">
              <span class="text-muted">Shipping</span>
              <span>{{ formatCurrency(cart.shipping) }}</span>
            </div>
            <div v-if="cart.adjustments !== 0" class="flex items-center justify-between text-sm">
              <span class="text-muted">Adjustments / Discounts</span>
              <span>{{ formatCurrency(cart.adjustments) }}</span>
            </div>
            <div class="flex items-center justify-between text-sm">
              <span class="text-muted">Tax</span>
              <span class="text-subtle">Calculated at checkout</span>
            </div>

            <Divider />

            <!-- Total: Order total with divider above it -->
            <div class="flex items-center justify-between font-semibold">
              <span>Total</span>
              <span>{{ formatCurrency(total) }}</span>
            </div>

            <!-- Promo: InputGroup entry with removable applied-coupon chip -->
            <div class="mt-2 space-y-2">
              <InputGroup>
                <InputText
                  v-model="couponCode"
                  placeholder="Promo code"
                  :disabled="couponApplied"
                  aria-label="Promo code"
                />
                <Button label="Apply" :disabled="couponApplied" @click="applyCoupon" />
              </InputGroup>
              <Chip
                v-if="couponApplied"
                :label="couponCode.trim()"
                removable
                aria-label="Remove promo code"
                @remove="clearCoupon"
              />
            </div>

            <!-- Action: Primary checkout button -->
            <Button as="router-link" to="/checkout" label="Proceed to Checkout" icon="pi pi-arrow-right" iconPos="right" class="mt-2 w-full" />
          </div>
        </template>
      </Card>
    </div>
  </div>
</template>
