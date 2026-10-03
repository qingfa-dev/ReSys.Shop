<script setup lang="ts">
import { useOrder } from '@/features/ordering/composables/useCart'
import { onMounted } from 'vue'

const { orders, isLoading, loadOrders } = useOrder()

onMounted(() => {
  loadOrders()
})
</script>

<template>
  <div class="orders-view">
    <h2>My Orders</h2>
    
    <div v-if="isLoading" class="loading">
      <i class="pi pi-spin pi-spinner"></i>
      Loading orders...
    </div>
    
    <div v-else-if="orders.length === 0" class="empty">
      <i class="pi pi-inbox"></i>
      <p>You haven't placed any orders yet.</p>
    </div>
    
    <div v-else class="order-list">
      <div v-for="order in orders" :key="order.id" class="order-item">
        <div class="order-header">
          <span class="order-number">Order #{{ order.orderNumber }}</span>
          <span class="order-status">{{ order.status }}</span>
        </div>
        <div class="order-details">
          <span>{{ order.items.length }} items</span>
          <span class="order-total">${{ order.total }}</span>
          <span class="order-date">{{ new Date(order.createdAt).toLocaleDateString() }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.orders-view {
  h2 {
    font-size: var(--font-size-xl);
    margin-bottom: 1.5rem;
  }
}

.loading, .empty {
  text-align: center;
  padding: 3rem;
  color: var(--color-text-muted);
  
  i {
    font-size: 3rem;
    margin-bottom: 1rem;
    display: block;
  }
}

.order-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.order-item {
  background: var(--color-surface);
  border-radius: var(--radius-lg);
  padding: 1.5rem;
  box-shadow: var(--shadow-sm);
}

.order-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 0.75rem;
  
  .order-number {
    font-weight: var(--font-weight-semibold);
  }
  
  .order-status {
    font-size: var(--font-size-sm);
    padding: 0.25rem 0.75rem;
    background: var(--color-surface-ground);
    border-radius: var(--radius-full);
  }
}

.order-details {
  display: flex;
  gap: 1.5rem;
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
  
  .order-total {
    color: var(--color-primary);
    font-weight: var(--font-weight-medium);
  }
}
</style>
