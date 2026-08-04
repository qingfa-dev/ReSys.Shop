<script setup lang="ts">
import { onMounted } from 'vue'
import { useCartStore } from '../stores/cartStore'
import CartItem from '../components/CartItem.vue'
import OrderSummary from '../components/OrderSummary.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const cart = useCartStore()

onMounted(() => cart.fetchCart())
</script>
<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <h1 class="text-2xl font-bold text-stone-900 mb-8">Shopping Cart</h1>
    <EmptyState
      v-if="!cart.loading && cart.items.length === 0"
      icon="pi pi-shopping-bag"
      message="Your cart is empty"
      action-label="Continue Shopping"
      action-to="/shop"
    />
    <div v-else class="flex flex-col lg:flex-row gap-8">
      <div class="flex-1">
        <CartItem
          v-for="item in cart.items"
          :key="item.id"
          :item="item"
          @update-quantity="(id, qty) => cart.updateQuantity(id, qty)"
          @remove="(id) => cart.removeItem(id)"
        />
      </div>
      <div class="w-full lg:w-80">
        <OrderSummary :item-count="cart.itemCount" :subtotal="cart.subtotal" />
      </div>
    </div>
  </div>
</template>
