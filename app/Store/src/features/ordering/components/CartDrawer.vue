<script setup lang="ts">
import { useCartStore } from '../stores/cartStore'
import CartItem from './CartItem.vue'
import OrderSummary from './OrderSummary.vue'

defineProps<{ visible: boolean }>()
const emit = defineEmits<{ 'update:visible': [value: boolean] }>()
const cart = useCartStore()

function handleUpdateQuantity(lineItemId: string, qty: number): void {
  cart.updateQuantity(lineItemId, qty)
}

function handleRemove(lineItemId: string): void {
  cart.removeItem(lineItemId)
}
</script>
<template>
  <Drawer :visible="visible" @update:visible="emit('update:visible', $event)" header="Shopping Cart" position="right" class="w-96">
    <div v-if="cart.items.length === 0" class="text-center py-8">
      <i class="pi pi-shopping-cart text-4xl text-stone-300 mb-4" />
      <p class="text-stone-500">Your cart is empty</p>
      <router-link to="/shop" class="text-teal-600 hover:underline mt-2 inline-block">Continue Shopping</router-link>
    </div>
    <div v-else class="space-y-4">
      <CartItem v-for="item in cart.items" :key="item.id" :item="item" @update-quantity="handleUpdateQuantity" @remove="handleRemove" />
    </div>
    <template #footer>
      <OrderSummary v-if="cart.items.length > 0" :item-count="cart.itemCount" :subtotal="cart.subtotal" />
    </template>
  </Drawer>
</template>
