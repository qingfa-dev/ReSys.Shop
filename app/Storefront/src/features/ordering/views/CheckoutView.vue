<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useRouter } from "vue-router";
import { useCartStore } from "@/features/ordering/store";
import { useOrderStore } from "@/features/ordering/store";
import OrderSummary from "@/features/ordering/components/OrderSummary.vue";
import { v4 as uuidv4 } from "uuid";

const router = useRouter();
const cartStore = useCartStore();
const orderStore = useOrderStore();

// Form state
const email = ref("");
const firstName = ref("");
const lastName = ref("");
const address = ref("");
const city = ref("");
const state = ref("");
const zip = ref("");
const shippingMethod = ref<string>("");
const paymentMethod = ref<string>("");
const isLoading = ref(false);
const orderError = ref<string | null>(null);

// Mock shipping/payment methods (in real app these come from server)
const shippingMethods = [
  { id: uuidv4(), name: "Standard Shipping", price: 0, estimatedDays: 5 },
  { id: uuidv4(), name: "Express Shipping", price: 15, estimatedDays: 2 },
];

const paymentMethods = [
  { id: uuidv4(), name: "Credit Card", type: "card" },
  { id: uuidv4(), name: "PayPal", type: "paypal" },
];

const canPlaceOrder = computed(() => {
  return (
    email.value &&
    firstName.value &&
    lastName.value &&
    address.value &&
    city.value &&
    state.value &&
    zip.value &&
    shippingMethod.value &&
    paymentMethod.value &&
    (cartStore.cart?.items?.length ?? 0) > 0
  );
});

onMounted(() => {
  // If cart is empty, redirect to shop
  if (!cartStore.cart?.items || cartStore.cart.items.length === 0) {
    router.push("/cart");
  }

  // Pre-select first options
  if (shippingMethods.length > 0 && shippingMethods[0]) {
    shippingMethod.value = shippingMethods[0]!.id;
  }
  if (paymentMethods.length > 0 && paymentMethods[0]) {
    paymentMethod.value = paymentMethods[0]!.id;
  }
});

async function handlePlaceOrder() {
  if (!canPlaceOrder.value) return;

  isLoading.value = true;
  orderError.value = null;

  try {
    // Prepare checkout request — the full checkout flow is rewritten in a
    // later task. paymentIntentId is produced by the Stripe integration.
    const checkoutRequest = { paymentIntentId: '' };

    // Call checkout
    const order = await orderStore.checkout(checkoutRequest);

    // Redirect to order confirmation
    router.push(`/orders/${order.id}`);
  } catch (error) {
    orderError.value = error instanceof Error ? error.message : "Failed to place order";
  } finally {
    isLoading.value = false;
  }
}

function handleBackToCart() {
  router.push("/cart");
}
</script>

<template>
  <div class="checkout-view">
    <div class="checkout-header">
      <button class="back-btn" @click="handleBackToCart">
        <i class="pi pi-arrow-left"></i>
        Back to Cart
      </button>
      <h1>Checkout</h1>
    </div>

    <div class="checkout-content">
      <div class="checkout-form">
        <section class="form-section">
          <h2>Contact Information</h2>
          <div class="form-field">
            <label>Email</label>
            <input v-model="email" type="email" placeholder="your@email.com" />
          </div>
        </section>

        <section class="form-section">
          <h2>Shipping Address</h2>
          <div class="form-row">
            <div class="form-field">
              <label>First Name</label>
              <input v-model="firstName" type="text" placeholder="John" />
            </div>
            <div class="form-field">
              <label>Last Name</label>
              <input v-model="lastName" type="text" placeholder="Doe" />
            </div>
          </div>
          <div class="form-field">
            <label>Address</label>
            <input v-model="address" type="text" placeholder="123 Main St" />
          </div>
          <div class="form-row">
            <div class="form-field">
              <label>City</label>
              <input v-model="city" type="text" placeholder="New York" />
            </div>
            <div class="form-field">
              <label>State</label>
              <input v-model="state" type="text" placeholder="NY" />
            </div>
            <div class="form-field">
              <label>ZIP</label>
              <input v-model="zip" type="text" placeholder="10001" />
            </div>
          </div>
        </section>

        <section class="form-section">
          <h2>Shipping Method</h2>
          <div class="shipping-options">
            <label v-for="method in shippingMethods" :key="method.id" class="shipping-option">
              <input v-model="shippingMethod" type="radio" :value="method.id" />
              <div class="option-content">
                <span class="option-name">{{ method.name }}</span>
                <span class="option-price">${{ method.price }}</span>
              </div>
              <span class="option-days"
                >{{ method.estimatedDays }}-{{ method.estimatedDays + 2 }} business days</span
              >
            </label>
          </div>
        </section>

        <section class="form-section">
          <h2>Payment</h2>
          <div class="payment-methods">
            <label v-for="method in paymentMethods" :key="method.id" class="payment-option">
              <input v-model="paymentMethod" type="radio" :value="method.id" />
              <span>{{ method.name }}</span>
            </label>
          </div>
        </section>
      </div>

      <!-- Error Message -->
      <div v-if="orderError" class="error-banner">
        <p>{{ orderError }}</p>
      </div>

      <aside class="checkout-sidebar">
        <OrderSummary
          :subtotal="cartStore.cart?.subtotal || 0"
          :shipping="shippingMethods.find((m) => m.id === shippingMethod)?.price || 0"
          :tax="cartStore.cart?.tax || 0"
          :total="cartStore.cart?.total || 0"
          :showCoupon="true"
          :disabled="!canPlaceOrder"
          :loading="isLoading"
          @checkout="handlePlaceOrder"
        />
      </aside>
    </div>
  </div>
</template>

<style scoped lang="scss">
.checkout-view {
  max-width: 1400px;
  margin: 0 auto;
  padding: 2rem;
}

.error-banner {
  background-color: #fee;
  border-left: 4px solid #f44;
  padding: 1rem;
  margin-bottom: 2rem;
  border-radius: 4px;
  color: #c00;
  font-weight: var(--font-weight-medium);
}

.checkout-header {
  margin-bottom: 2rem;

  .back-btn {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    background: none;
    border: none;
    color: var(--color-text-secondary);
    cursor: pointer;
    margin-bottom: 1rem;
    font-size: var(--font-size-sm);

    &:hover {
      color: var(--color-text);
    }
  }

  h1 {
    font-size: var(--font-size-3xl);
  }
}

.checkout-content {
  display: grid;
  grid-template-columns: 1fr 380px;
  gap: 3rem;

  @media (max-width: 1024px) {
    grid-template-columns: 1fr;
  }
}

.checkout-form {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.form-section {
  background: var(--color-surface);
  border-radius: var(--radius-xl);
  padding: 1.5rem;

  h2 {
    font-size: var(--font-size-lg);
    margin-bottom: 1.5rem;
    padding-bottom: 0.75rem;
    border-bottom: 1px solid var(--color-border-light);
  }
}

.form-field {
  margin-bottom: 1rem;

  label {
    display: block;
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    margin-bottom: 0.5rem;
  }

  input {
    width: 100%;
    padding: 0.75rem 1rem;
    border: 1px solid var(--color-border);
    border-radius: var(--radius-md);
    font-size: var(--font-size-base);

    &:focus {
      outline: none;
      border-color: var(--color-primary);
    }
  }
}

.form-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 1rem;
}

.shipping-options,
.payment-methods {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.shipping-option,
.payment-option {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: border-color var(--transition-fast);

  &:hover {
    border-color: var(--color-primary);
  }

  input:checked + .option-content,
  input:checked {
    border-color: var(--color-primary);
  }
}

.option-content {
  flex: 1;
  display: flex;
  justify-content: space-between;

  .option-name {
    font-weight: var(--font-weight-medium);
  }

  .option-price {
    color: var(--color-primary);
    font-weight: var(--font-weight-semibold);
  }
}

.option-days {
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
}
</style>
