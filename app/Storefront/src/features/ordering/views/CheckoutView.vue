<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import { useCartStore, useOrderStore } from '@/features/ordering/store'
import OrderSummary from '@/features/ordering/components/OrderSummary.vue'
import { cartService } from '@/features/ordering/services/cart/cart.service'
import { paymentIntentService } from '@/features/payment/services/payment-intent/payment-intent.service'
import type { CartEntity } from '@/features/ordering/types/entity'

const router = useRouter()
const cartStore = useCartStore()
const orderStore = useOrderStore()

// cartStore.cart is typed as the schema Cart (no id/currency), but the service
// actually populates the richer CartEntity — cast once so checkout can read id/currency.
const checkoutCart = computed(() => cartStore.cart as CartEntity | null)

// Step tracking: 1=Address, 2=Delivery, 3=Payment, 4=Confirm, 5=Complete
const currentStep = ref(1)
const isLoading = ref(false)
const orderError = ref<string | null>(null)

// Selected values per step
const selectedAddressId = ref<string | null>(null)
const selectedShippingMethodId = ref<string | null>(null)
const selectedPaymentMethodId = ref<string | null>(null)

// New address inline form
const showNewAddressForm = ref(false)
const newAddress = ref({
  firstName: '', phone: '', address1: '', city: '',
  stateProvince: '', zipCode: '', countryName: 'Vietnam',
})

// Computed: can proceed from each step
const canProceedFromAddress = computed(() =>
  !!selectedAddressId.value || showNewAddressForm.value)
const canProceedFromDelivery = computed(() =>
  !!selectedShippingMethodId.value)
const canProceedFromPayment = computed(() =>
  !!selectedPaymentMethodId.value)

// Computed: selected entities
const selectedAddress = computed(() =>
  orderStore.addresses.find(a => a.id === selectedAddressId.value))
const selectedShippingMethod = computed(() =>
  orderStore.shippingMethods.find(m => m.id === selectedShippingMethodId.value))
const selectedPaymentMethod = computed(() =>
  orderStore.paymentMethods.find(m => m.id === selectedPaymentMethodId.value))

// Email is optional on the backend UpdateCheckout request — omit if unavailable
const customerEmail = ref('')

onMounted(async () => {
  // Guard: redirect to cart if empty
  if (!cartStore.cart?.items || cartStore.cart.items.length === 0) {
    router.push('/cart')
    return
  }
  // Await: Load addresses, shipping methods, and payment methods in parallel
  await orderStore.fetchCheckoutData()
  // Pre-select defaults
  if (orderStore.addresses.length > 0) {
    const def = orderStore.addresses.find(a => a.isDefault) ?? orderStore.addresses[0]
    if (def) selectedAddressId.value = def.id
  }
  if (orderStore.shippingMethods.length > 0) {
    selectedShippingMethodId.value = orderStore.shippingMethods[0]!.id
  }
  if (orderStore.paymentMethods.length > 0) {
    selectedPaymentMethodId.value = orderStore.paymentMethods[0]!.id
  }
})

// Step navigation
function nextStep() { if (currentStep.value < 5) currentStep.value++ }
function prevStep() { if (currentStep.value > 1) currentStep.value-- }

// Place order — real backend contract:
// 1. PUT /cart with { shipAddressId, shippingMethodId, currency, email }
// 2. POST /cart/shipping-rate { shippingMethodId }
// 3. POST /payment/create-intent { amount, currency, orderId, paymentMethodId }
// 4. POST /cart/checkout { paymentIntentId }
async function handlePlaceOrder() {
  const cart = checkoutCart.value
  if (!cart) return
  if (!selectedAddressId.value || !selectedShippingMethodId.value || !selectedPaymentMethodId.value) return
  isLoading.value = true
  orderError.value = null
  try {
    // Prepare: set address + shipping method on the server-side cart
    await cartService.updateCheckoutDetails({
      currency: cart.currency ?? 'VND',
      email: customerEmail.value || undefined,
      shipAddressId: selectedAddressId.value,
      shippingMethodId: selectedShippingMethodId.value,
    })
    // Select: apply the shipping rate to the cart
    await cartService.selectShippingRate(selectedShippingMethodId.value)

    // Payment: create an intent against the draft cart/order id
    const intentResult = await paymentIntentService.createPaymentIntent({
      amount: cartStore.total,
      currency: cart.currency ?? 'VND',
      orderId: cart.id,
      paymentMethodId: selectedPaymentMethodId.value,
    })
    if (!intentResult.isSuccess || !intentResult.data) {
      throw new Error(intentResult.message || 'Payment intent creation failed')
    }

    // Place: finalize the order with the payment intent id
    await orderStore.checkout({ paymentIntentId: intentResult.data.id })
    currentStep.value = 5
  } catch (error) {
    orderError.value = error instanceof Error ? error.message : 'Failed to place order'
  } finally {
    isLoading.value = false
  }
}

// Formatting
function formatPrice(price: number): string {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price)
}
</script>

<template>
  <div class="checkout-view">
    <!-- Progress indicator -->
    <div class="checkout-progress">
      <template v-for="step in 5" :key="step">
        <div class="step" :class="{ active: currentStep >= step, completed: currentStep > step }">
          <span class="step-number">
            <i v-if="currentStep > step" class="pi pi-check" />
            <span v-else>{{ step }}</span>
          </span>
          <span class="step-label">{{ ['Address','Delivery','Payment','Confirm','Complete'][step-1] }}</span>
        </div>
        <div v-if="step < 5" class="step-connector" :class="{ active: currentStep > step }" />
      </template>
    </div>

    <!-- Step 1: Address -->
    <div v-if="currentStep === 1" class="step-content">
      <h2>Shipping Address</h2>
      <div v-if="orderStore.addresses.length === 0 && !showNewAddressForm" class="empty-state">
        <i class="pi pi-map-marker"></i>
        <p>No saved addresses. Please add one to continue.</p>
        <Button label="Add New Address" icon="pi pi-plus" @click="showNewAddressForm = true" />
      </div>
      <div v-for="addr in orderStore.addresses" :key="addr.id"
           class="address-card" :class="{ selected: selectedAddressId === addr.id }"
           @click="selectedAddressId = addr.id">
        <div class="card-radio">
          <i :class="selectedAddressId === addr.id ? 'pi pi-circle-fill' : 'pi pi-circle'" />
        </div>
        <div class="address-details">
          <strong>{{ addr.firstName }} {{ addr.lastName }}</strong>
          <span>{{ addr.address1 }}, {{ addr.city }}, {{ addr.state }} {{ addr.postalCode }}</span>
          <span v-if="addr.isDefault" class="default-badge">Default</span>
        </div>
      </div>
      <Button label="+ Add New Address" class="p-button-text" @click="showNewAddressForm = !showNewAddressForm" />
      <!-- MVP: dropped — inline address creation simplified for demo -->
      <div v-if="showNewAddressForm" class="new-address-form">
        <div class="form-row">
          <InputText v-model="newAddress.firstName" placeholder="Full Name" />
          <InputText v-model="newAddress.phone" placeholder="Phone" />
        </div>
        <InputText v-model="newAddress.address1" placeholder="Street Address" class="full-width" />
        <div class="form-row">
          <InputText v-model="newAddress.city" placeholder="City" />
          <InputText v-model="newAddress.stateProvince" placeholder="State/Province" />
          <InputText v-model="newAddress.zipCode" placeholder="ZIP/Postal Code" />
        </div>
      </div>
      <div class="step-actions">
        <Button label="Continue to Delivery" :disabled="!canProceedFromAddress" @click="nextStep" />
      </div>
    </div>

    <!-- Step 2: Delivery -->
    <div v-if="currentStep === 2" class="step-content">
      <h2>Shipping Method</h2>
      <div v-if="orderStore.shippingMethods.length === 0" class="empty-state">
        <i class="pi pi-truck"></i>
        <p>No shipping methods available for your location.</p>
      </div>
      <div v-for="method in orderStore.shippingMethods" :key="method.id"
           class="method-card" :class="{ selected: selectedShippingMethodId === method.id }"
           @click="selectedShippingMethodId = method.id">
        <div class="card-radio">
          <i :class="selectedShippingMethodId === method.id ? 'pi pi-circle-fill' : 'pi pi-circle'" />
        </div>
        <div class="method-details">
          <strong>{{ method.name }}</strong>
          <span>{{ method.adminName || method.calculatorType || 'Shipping' }}</span>
        </div>
        <span class="method-rate">{{ formatPrice(method.price ?? 0) }}</span>
      </div>
      <div class="step-actions">
        <Button label="Back" class="p-button-outlined" @click="prevStep" />
        <Button label="Continue to Payment" :disabled="!canProceedFromDelivery" @click="nextStep" />
      </div>
    </div>

    <!-- Step 3: Payment -->
    <div v-if="currentStep === 3" class="step-content">
      <h2>Payment Method</h2>
      <div v-for="method in orderStore.paymentMethods" :key="method.id"
           class="method-card" :class="{ selected: selectedPaymentMethodId === method.id }"
           @click="selectedPaymentMethodId = method.id">
        <div class="card-radio">
          <i :class="selectedPaymentMethodId === method.id ? 'pi pi-circle-fill' : 'pi pi-circle'" />
        </div>
        <div class="method-details">
          <strong>{{ method.name }}</strong>
          <span>{{ method.description }}</span>
        </div>
        <i class="pi pi-credit-card" />
      </div>
      <!-- MVP: dropped — non-essential for demo -->
      <div v-if="false" class="save-payment">
        <input type="checkbox" id="save-payment" />
        <label for="save-payment">Save payment method for future orders</label>
      </div>
      <div class="step-actions">
        <Button label="Back" class="p-button-outlined" @click="prevStep" />
        <Button label="Continue to Confirm" :disabled="!canProceedFromPayment" @click="nextStep" />
      </div>
    </div>

    <!-- Step 4: Confirm -->
    <div v-if="currentStep === 4" class="step-content">
      <h2>Order Summary</h2>
      <OrderSummary
        :subtotal="cartStore.subtotal"
        :shipping="cartStore.shipping"
        :tax="cartStore.tax"
        :discount="cartStore.discount"
        :total="cartStore.total"
        :show-coupon="false"
      />
      <div class="summary-details">
        <div class="summary-row">
          <strong>Shipping to:</strong>
          <span>{{ selectedAddress?.address1 }}, {{ selectedAddress?.city }}, {{ selectedAddress?.state }} {{ selectedAddress?.postalCode }}</span>
        </div>
        <div class="summary-row">
          <strong>Delivery:</strong>
          <span>{{ selectedShippingMethod?.name }}</span>
        </div>
        <div class="summary-row">
          <strong>Payment:</strong>
          <span>{{ selectedPaymentMethod?.name }}</span>
        </div>
        <div class="totals-table">
          <div class="total-row"><span>Item Total</span><span>{{ formatPrice(cartStore.subtotal) }}</span></div>
          <div class="total-row"><span>Shipping</span><span>{{ formatPrice(selectedShippingMethod?.price ?? 0) }}</span></div>
          <div class="total-row grand"><span>Grand Total</span><span>{{ formatPrice(cartStore.total) }}</span></div>
        </div>
      </div>
      <p v-if="orderError" class="checkout-error"><i class="pi pi-exclamation-circle"></i> {{ orderError }}</p>
      <div class="step-actions">
        <Button label="Back" class="p-button-outlined" @click="prevStep" />
        <Button label="Place Order" icon="pi pi-check" :disabled="isLoading" :loading="isLoading" @click="handlePlaceOrder" />
      </div>
    </div>

    <!-- Step 5: Complete -->
    <div v-if="currentStep === 5" class="step-content complete-step">
      <i class="pi pi-check-circle"></i>
      <h2>Order Confirmed</h2>
      <p v-if="orderStore.currentOrder?.orderNumber">
        Order #{{ orderStore.currentOrder.orderNumber }} has been placed.
      </p>
      <p>Thank you for your purchase. You will receive a confirmation email shortly.</p>
      <Button label="Continue Shopping" icon="pi pi-shopping-bag" @click="router.push('/shop')" />
    </div>
  </div>
</template>

<style scoped lang="scss">
.checkout-view {
  max-width: 900px;
  margin: 0 auto;
  padding: 2rem;
}

// Progress indicator
.checkout-progress {
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 3rem;
}

.step {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;

  .step-number {
    width: 36px;
    height: 36px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: var(--font-weight-semibold);
    font-size: var(--font-size-sm);
    background: var(--color-surface-ground);
    color: var(--color-text-secondary);
    border: 2px solid var(--color-border-light);
    transition: all var(--transition-fast);
  }

  .step-label {
    font-size: var(--font-size-xs);
    color: var(--color-text-secondary);
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }

  &.active .step-number {
    background: var(--color-primary);
    color: white;
    border-color: var(--color-primary);
  }

  &.active .step-label {
    color: var(--color-primary);
    font-weight: var(--font-weight-semibold);
  }

  &.completed .step-number {
    background: #22c55e;
    color: white;
    border-color: #22c55e;
  }

  &.completed .step-label {
    color: #22c55e;
  }
}

.step-connector {
  width: 48px;
  height: 2px;
  background: var(--color-border-light);
  margin: 0 0.5rem;
  margin-bottom: 1.25rem;

  &.active {
    background: var(--color-primary);
  }
}

// Step content
.step-content {
  h2 {
    font-family: var(--font-display);
    font-size: var(--font-size-2xl);
    margin-bottom: 1.5rem;
  }
}

// Address cards
.address-card, .method-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 1.25rem;
  border: 1px solid var(--color-border-light);
  border-radius: var(--radius-md);
  margin-bottom: 0.75rem;
  cursor: pointer;
  transition: border-color var(--transition-fast);

  &:hover { border-color: var(--color-primary); }
  &.selected { border-color: var(--color-primary); background: rgba(var(--color-primary-rgb), 0.04); }

  .card-radio i { font-size: 1.25rem; color: var(--color-primary); }
}

.address-details, .method-details {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;

  strong { font-size: var(--font-size-base); }
  span { font-size: var(--font-size-sm); color: var(--color-text-secondary); }
}

.default-badge {
  display: inline-block;
  padding: 0.125rem 0.5rem;
  background: var(--color-primary);
  color: white;
  border-radius: var(--radius-full);
  font-size: var(--font-size-xs);
  width: fit-content;
}

.method-rate {
  font-weight: var(--font-weight-semibold);
  color: var(--color-primary);
}

// New address form
.new-address-form {
  margin: 1rem 0;
  padding: 1rem;
  border: 1px dashed var(--color-border-light);
  border-radius: var(--radius-md);

  .form-row {
    display: flex;
    gap: 0.75rem;
    margin-bottom: 0.75rem;
  }

  .full-width { width: 100%; margin-bottom: 0.75rem; }
}

// Summary
.summary-details {
  margin-top: 1.5rem;

  .summary-row {
    display: flex;
    justify-content: space-between;
    padding: 0.5rem 0;
    font-size: var(--font-size-sm);
  }
}

.totals-table {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid var(--color-border-light);

  .total-row {
    display: flex;
    justify-content: space-between;
    padding: 0.25rem 0;
    font-size: var(--font-size-base);

    &.grand {
      font-weight: var(--font-weight-bold);
      font-size: var(--font-size-lg);
      padding-top: 0.75rem;
      border-top: 1px solid var(--color-border-light);
      margin-top: 0.5rem;
    }
  }
}

// Complete step
.complete-step {
  text-align: center;
  padding: 3rem 1rem;

  i {
    font-size: 5rem;
    color: #22c55e;
    margin-bottom: 1.5rem;
  }

  h2 {
    margin-bottom: 0.75rem;
  }

  p {
    color: var(--color-text-secondary);
    margin-bottom: 0.5rem;
  }
}

// Step actions
.step-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  margin-top: 2rem;
}

// Shared
.empty-state {
  text-align: center;
  padding: 3rem 1rem;
  color: var(--color-text-secondary);

  i { font-size: 3rem; margin-bottom: 1rem; }
  p { margin-bottom: 1rem; }
}

.checkout-error {
  color: var(--color-danger);
  margin-top: 1rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
</style>
