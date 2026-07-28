<script setup lang="ts">
import { computed, ref } from 'vue'
import InputText from 'primevue/inputtext'
import Button from 'primevue/button'

interface Props {
  subtotal: number
  shipping?: number
  tax?: number
  discount?: number
  total: number
  showCoupon?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  shipping: 0,
  tax: 0,
  discount: 0,
  showCoupon: true,
})

const emit = defineEmits<{
  (e: 'applyCoupon', code: string): void
  (e: 'checkout'): void
}>()

const formattedSubtotal = computed(() => formatCurrency(props.subtotal))
const formattedShipping = computed(() => props.shipping > 0 ? formatCurrency(props.shipping) : 'Free')
const formattedTax = computed(() => formatCurrency(props.tax))
const formattedDiscount = computed(() => formatCurrency(props.discount))
const formattedTotal = computed(() => formatCurrency(props.total))

function formatCurrency(value: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(value)
}

const couponCode = ref('')

function handleApplyCoupon() {
  if (couponCode.value.trim()) {
    emit('applyCoupon', couponCode.value.trim())
  }
}
</script>

<template>
  <div class="order-summary">
    <h3 class="summary-title">Order Summary</h3>
    
    <div class="summary-rows">
      <div class="summary-row">
        <span class="label">Subtotal</span>
        <span class="value">{{ formattedSubtotal }}</span>
      </div>
      
      <div class="summary-row">
        <span class="label">Shipping</span>
        <span class="value" :class="{ free: shipping === 0 }">{{ formattedShipping }}</span>
      </div>
      
      <div class="summary-row">
        <span class="label">Tax</span>
        <span class="value">{{ formattedTax }}</span>
      </div>
      
      <div v-if="discount > 0" class="summary-row discount">
        <span class="label">Discount</span>
        <span class="value">-{{ formattedDiscount }}</span>
      </div>
    </div>
    
    <div v-if="showCoupon" class="coupon-section">
      <div class="coupon-input">
        <InputText v-model="couponCode" placeholder="Enter coupon code" />
        <Button label="Apply" size="small" @click="handleApplyCoupon" />
      </div>
    </div>
    
    <div class="summary-total">
      <span class="label">Total</span>
      <span class="value">{{ formattedTotal }}</span>
    </div>
    
    <Button 
      label="Proceed to Checkout" 
      size="large" 
      class="checkout-btn" 
      @click="emit('checkout')"
    />
    
    <p class="security-note">
      <i class="pi pi-lock"></i>
      Secure checkout - SSL encrypted
    </p>
  </div>
</template>

<style scoped lang="scss">
.order-summary {
  background: var(--color-surface);
  border-radius: var(--radius-xl);
  padding: 1.5rem;
  box-shadow: var(--shadow-sm);
  position: sticky;
  top: 100px;
}

.summary-title {
  font-family: var(--font-display);
  font-size: var(--font-size-xl);
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--color-border-light);
}

.summary-rows {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-bottom: 1.5rem;
}

.summary-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  
  .label {
    color: var(--color-text-secondary);
    font-size: var(--font-size-sm);
  }
  
  .value {
    font-weight: var(--font-weight-medium);
    color: var(--color-text);
    
    &.free {
      color: var(--color-success);
    }
  }
  
  &.discount .value {
    color: var(--color-success);
  }
}

.coupon-section {
  padding: 1rem 0;
  border-top: 1px solid var(--color-border-light);
  border-bottom: 1px solid var(--color-border-light);
  margin-bottom: 1.5rem;
}

.coupon-input {
  display: flex;
  gap: 0.5rem;
  
  :deep(.p-inputtext) {
    flex: 1;
    padding: 0.5rem 0.75rem;
  }
}

.summary-total {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 0;
  margin-bottom: 1.5rem;
  
  .label {
    font-size: var(--font-size-lg);
    font-weight: var(--font-weight-semibold);
  }
  
  .value {
    font-size: var(--font-size-2xl);
    font-weight: var(--font-weight-bold);
    color: var(--color-primary);
  }
}

.checkout-btn {
  width: 100%;
}

.security-note {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  margin-top: 1rem;
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
  
  i {
    font-size: var(--font-size-sm);
  }
}
</style>
