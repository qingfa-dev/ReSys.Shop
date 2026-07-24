<script setup lang="ts">
import Button from 'primevue/button'
import { useOrder } from '@/features/ordering/composables/useCart'
import { onMounted } from 'vue'

const { addresses, isLoading, loadCheckoutData } = useOrder()

onMounted(() => {
  loadCheckoutData()
})
</script>

<template>
  <div class="addresses-view">
    <div class="section-header">
      <h2>My Addresses</h2>
      <Button label="Add Address" icon="pi pi-plus" size="small" />
    </div>
    
    <div v-if="isLoading" class="loading">
      <i class="pi pi-spin pi-spinner"></i>
      Loading addresses...
    </div>
    
    <div v-else-if="addresses.length === 0" class="empty">
      <i class="pi pi-map-marker"></i>
      <p>You haven't saved any addresses yet.</p>
      <Button label="Add Address" />
    </div>
    
    <div v-else class="address-list">
      <div v-for="address in addresses" :key="address.id" class="address-card">
        <div class="address-content">
          <p class="name">{{ address.firstName }} {{ address.lastName }}</p>
          <p>{{ address.address1 }}</p>
          <p v-if="address.address2">{{ address.address2 }}</p>
          <p>{{ address.city }}, {{ address.state }} {{ address.postalCode }}</p>
          <p>{{ address.country }}</p>
        </div>
        <div class="address-actions">
          <Button icon="pi pi-pencil" severity="text" />
          <Button icon="pi pi-trash" severity="danger text" />
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.addresses-view {
  h2 {
    font-size: var(--font-size-xl);
    margin-bottom: 1.5rem;
  }
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
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

.address-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1rem;
}

.address-card {
  background: var(--color-surface);
  border-radius: var(--radius-lg);
  padding: 1.5rem;
  box-shadow: var(--shadow-sm);
  display: flex;
  justify-content: space-between;
}

.address-content {
  p {
    margin: 0;
    line-height: 1.5;
    
    &.name {
      font-weight: var(--font-weight-semibold);
      margin-bottom: 0.5rem;
    }
  }
}

.address-actions {
  display: flex;
  gap: 0.25rem;
}
</style>
