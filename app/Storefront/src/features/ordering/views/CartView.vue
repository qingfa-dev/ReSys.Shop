<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useCart } from '@/features/ordering/composables/useCart'
import Button from 'primevue/button'
import CartItem from '@/features/ordering/components/CartItem.vue'
import OrderSummary from '@/features/ordering/components/OrderSummary.vue'

const router = useRouter()
const { items, isEmpty, subtotal, total, isLoading, loadCart, updateQuantity, removeFromCart } = useCart()

onMounted(() => {
  loadCart()
})

function handleQuantityChange(itemId: string, quantity: number) {
  updateQuantity(itemId, quantity)
}

function handleRemoveItem(itemId: string) {
  removeFromCart(itemId)
}

function handleContinueShopping() {
  router.push('/shop')
}

function handleProceedToCheckout() {
  router.push('/checkout')
}

async function handleApplyCoupon(code: string) {
  console.log('Apply coupon:', code)
}
</script>

<template>
  <div class="cart-view">
    <div class="cart-header">
      <h1>Shopping Cart</h1>
      <span v-if="!isEmpty" class="item-count">{{ items.length }} items</span>
    </div>

    <div v-if="isLoading" class="loading-state">
      <i class="pi pi-spin pi-spinner"></i>
      <p>Loading cart...</p>
    </div>

    <div v-else-if="isEmpty" class="empty-cart">
      <div class="empty-icon">
        <i class="pi pi-shopping-cart"></i>
      </div>
      <h2>Your cart is empty</h2>
      <p>Looks like you haven't added anything yet.</p>
      <Button label="Continue Shopping" size="large" @click="handleContinueShopping" />
    </div>

    <div v-else class="cart-content">
      <div class="cart-items">
        <CartItem
          v-for="item in items"
          :key="item.id ?? item.variantId"
          :item="item"
          @update-quantity="(qty) => handleQuantityChange(item.id ?? '', qty)"
          @remove="handleRemoveItem(item.id ?? '')"
        />
      </div>

      <aside class="cart-sidebar">
        <OrderSummary
          :subtotal="subtotal"
          :total="total"
          @apply-coupon="handleApplyCoupon"
          @checkout="handleProceedToCheckout"
        />
      </aside>
    </div>
  </div>
</template>

<style scoped lang="scss">
.cart-view {
  max-width: 1400px;
  margin: 0 auto;
  padding: 2rem;
}

.cart-header {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 2rem;
  
  h1 {
    font-size: var(--font-size-3xl);
  }
  
  .item-count {
    font-size: var(--font-size-sm);
    color: var(--color-text-muted);
    padding: 0.25rem 0.75rem;
    background: var(--color-surface-ground);
    border-radius: var(--radius-full);
  }
}

.loading-state {
  text-align: center;
  padding: 4rem 2rem;
  color: var(--color-text-muted);
  
  i {
    font-size: 2rem;
    margin-bottom: 1rem;
  }
}

.empty-cart {
  text-align: center;
  padding: 4rem 2rem;
  
  .empty-icon {
    width: 120px;
    height: 120px;
    margin: 0 auto 2rem;
    background: var(--color-surface-ground);
    border-radius: var(--radius-full);
    display: flex;
    align-items: center;
    justify-content: center;
    
    i {
      font-size: 3rem;
      color: var(--color-text-muted);
    }
  }
  
  h2 {
    font-size: var(--font-size-2xl);
    margin-bottom: 0.5rem;
  }
  
  p {
    color: var(--color-text-muted);
    margin-bottom: 2rem;
  }
}

.cart-content {
  display: grid;
  grid-template-columns: 1fr 380px;
  gap: 3rem;
  
  @media (max-width: 1024px) {
    grid-template-columns: 1fr;
  }
}

.cart-items {
  min-width: 0;
}
</style>
