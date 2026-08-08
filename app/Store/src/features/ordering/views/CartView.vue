<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useCartStore } from '../stores/cartStore'

usePageTitle('Cart')
const cart = useCartStore()

onMounted(() => { cart.fetchCart() })

function updateQty(id: string, qty: number): void {
  if (qty < 1) return
  cart.updateQuantity(id, qty)
}
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Cart' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-2">Shopping Cart</h1>
    <p v-if="!cart.isEmpty" class="text-sm text-neutral-500 mb-8">{{ cart.itemCount }} {{ cart.itemCount === 1 ? 'item' : 'items' }}</p>

    <div v-if="cart.loading && cart.isEmpty" class="space-y-4">
      <Skeleton v-for="i in 3" :key="i" height="6rem" />
    </div>
    <div v-else-if="cart.isEmpty" class="text-center py-16">
      <i class="pi pi-shopping-cart text-5xl text-neutral-300 mb-4 block" />
      <p class="text-lg font-medium text-neutral-900 mb-2">Your cart is empty</p>
      <p class="text-sm text-neutral-500 mb-6">Looks like you haven't added anything yet.</p>
      <Button label="Continue Shopping" severity="secondary" outlined as="router-link" to="/shop" />
    </div>
    <div v-else-if="cart.error" class="text-center py-16">
      <p class="text-neutral-500 mb-4">{{ cart.error }}</p>
      <Button label="Retry" severity="secondary" outlined @click="cart.fetchCart()" />
    </div>
    <div v-else class="flex flex-col lg:flex-row gap-8">
      <div class="flex-1 space-y-4">
        <div v-for="item in cart.items" :key="item.id" class="flex gap-4 p-4 bg-white rounded-lg border border-neutral-200">
          <img v-if="item.productImageUrl" :src="item.productImageUrl" :alt="item.productName ?? ''" class="w-20 h-24 object-cover rounded-md bg-neutral-100" />
          <div v-else class="w-20 h-24 rounded-md bg-neutral-100 flex items-center justify-center shrink-0"><i class="pi pi-image text-neutral-300" /></div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-neutral-900 truncate">{{ item.productName ?? item.variantName }}</p>
            <p class="text-xs text-neutral-500 mt-0.5">{{ item.sku }}</p>
            <div class="flex items-center gap-2 mt-2">
              <Button icon="pi pi-minus" text rounded size="small" :disabled="item.quantity <= 1" @click="updateQty(item.id, item.quantity - 1)" />
              <span class="text-sm font-medium w-6 text-center">{{ item.quantity }}</span>
              <Button icon="pi pi-plus" text rounded size="small" @click="updateQty(item.id, item.quantity + 1)" />
            </div>
          </div>
          <div class="text-right shrink-0">
            <p class="text-sm font-medium text-neutral-900 font-mono">${{ item.total.toFixed(2) }}</p>
            <Button icon="pi pi-times" text rounded size="small" severity="danger" class="mt-1" @click="cart.removeItem(item.id)" />
          </div>
        </div>
        <button class="text-xs text-red-600 hover:text-red-800" @click="cart.clearCart()">Clear cart</button>
      </div>
      <div class="lg:w-80 shrink-0">
        <div class="bg-white border border-neutral-200 rounded-lg p-6 sticky top-24">
          <h2 class="text-sm font-semibold text-neutral-900 mb-4">Order Summary</h2>
          <div class="flex justify-between text-sm mb-2"><span class="text-neutral-500">Subtotal</span><span class="text-neutral-900 font-mono">${{ cart.subtotal.toFixed(2) }}</span></div>
          <div class="flex justify-between text-sm mb-2"><span class="text-neutral-500">Shipping</span><span class="text-neutral-400 text-xs">Calculated at checkout</span></div>
          <div class="flex justify-between text-sm mb-4"><span class="text-neutral-500">Tax</span><span class="text-neutral-400 text-xs">Calculated at checkout</span></div>
          <div class="border-t border-neutral-200 pt-4 mb-4"><div class="flex justify-between"><span class="text-sm font-semibold text-neutral-900">Total</span><span class="text-sm font-semibold text-neutral-900 font-mono">${{ cart.subtotal.toFixed(2) }}</span></div></div>
          <Button label="Checkout" severity="primary" class="w-full" as="router-link" to="/checkout" />
          <router-link to="/shop" class="block text-center text-sm text-neutral-500 hover:text-neutral-900 mt-3">Continue Shopping</router-link>
        </div>
      </div>
    </div>
  </div>
</template>
