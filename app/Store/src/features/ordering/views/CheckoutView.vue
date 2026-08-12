<script setup lang="ts">
import Label from 'primevue/label'
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { formatCurrency } from '@/shared/utils/currency'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { useLocation } from '@/features/location/composables'
import { usePayment } from '@/features/payment/composables/usePayment'
import { getPaymentMethods } from '@/features/payment/services/paymentApi'
import { useAddresses } from '@/features/profile/composables/useAddresses'
import type { AddressInput } from '@/features/profile/types'
import { useShipping } from '@/features/shipping/composables'
import { useCart } from '../composables/useCart'
import { useCheckout } from '../composables/useCheckout'

usePageTitle('Checkout')

// Step: Numeric union matching the checkout store's five wizard panels.
type CheckoutStep = 1 | 2 | 3 | 4 | 5

const router = useRouter()
const cart = useCart()
const checkout = useCheckout(() => cart)
const addresses = useAddresses()
const shipping = useShipping()
const location = useLocation()
const auth = useAuthStore()
const payment = usePayment()

// Stripe: Load the SDK so Elements can mount the hosted card form later.
payment.init()

// Card: The Stripe Elements card form mounts into this container on the payment panel.
const cardContainer = ref<HTMLElement | null>(null)
const paymentMethodId = ref<string | null>(null)

// Address: Form state for the shipping panel; email pre-filled from the session.
const selectedAddressId = ref<string | null>(null)
const firstName = ref('')
const lastName = ref('')
const address1 = ref('')
const city = ref('')
const zipCode = ref('')
const phone = ref('')
const email = ref(auth.user?.email ?? '')
const showAddressError = ref(false)

// Label: Human-readable options for the saved-address selector.
const addressOptions = computed(() =>
  addresses.shippingAddresses.map((addr) => ({
    id: addr.id,
    label: addr.label ?? `${addr.firstName} ${addr.lastName ?? ''} - ${addr.address1}, ${addr.city}`.trim(),
  })),
)

// Cascade: Leaf value of the CascadeSelect — the state id when a state is chosen,
// otherwise the country id (PrimeVue v5 emits only the leaf optionValue).
const cascadeValue = ref<string | null>(null)
const cascadeOptions = computed(() =>
  location.countries.map((country) => {
    const states = location.states.filter((state) => state.countryId === country.id)
    return {
      id: country.id,
      name: country.name,
      ...(states.length > 0 ? { children: states } : {}),
    }
  }),
)

// Country: Resolve the selected country and state from the location store path fields.
const selectedCountry = computed(() => location.countries.find((c) => c.id === location.selectedCountryId))
const selectedState = computed(() => location.states.find((s) => s.id === location.selectedStateId))

// Label: Full country / state path for the cascade control's value display.
const cascadeLabel = computed(() =>
  selectedState.value
    ? `${selectedCountry.value?.name ?? ''} / ${selectedState.value.name}`
    : selectedCountry.value?.name ?? '',
)

// Sync: Translate the cascade leaf value (state id, else country id) into store fields.
watch(cascadeValue, (leaf) => {
  const state = leaf ? location.states.find((s) => s.id === leaf) : undefined
  if (state) {
    location.selectedCountryId = state.countryId
    location.selectedStateId = state.id
    return
  }
  location.selectedCountryId = leaf ?? null
  location.selectedStateId = null
})

// Required: State is mandatory only when the selected country mandates it and has states.
const stateRequired = computed(() => {
  const country = location.countries.find((c) => c.id === location.selectedCountryId)
  if (!country?.statesRequired) return false
  return location.states.some((state) => state.countryId === country.id)
})

// Valid: Core shipping fields (and state where required) must be filled before saving.
const addressValid = computed(() =>
  firstName.value.trim() !== '' &&
  address1.value.trim() !== '' &&
  city.value.trim() !== '' &&
  email.value.trim() !== '' &&
  location.selectedCountryId !== null &&
  (!stateRequired.value || location.selectedStateId !== null),
)

// Prefill: Populate the form from a saved address, matching cascade nodes by name.
function selectSavedAddress(id: string | null): void {
  if (!id) return
  const addr = addresses.addresses.find((a) => a.id === id)
  if (!addr) return
  firstName.value = addr.firstName
  lastName.value = addr.lastName ?? ''
  address1.value = addr.address1
  city.value = addr.city
  zipCode.value = addr.zipCode ?? ''
  phone.value = addr.phone ?? ''
  const country = location.countries.find((c) => c.name === addr.countryName)
  const state = addr.stateProvince ? location.states.find((s) => s.name === addr.stateProvince) : undefined
  cascadeValue.value = state?.id ?? country?.id ?? null
  showAddressError.value = false
}

// Action: Persist the chosen or newly entered address, then advance to delivery.
async function continueToDelivery(): Promise<void> {
  if (selectedAddressId.value) {
    await checkout.saveAddress(selectedAddressId.value, email.value)
    return
  }
  if (!addressValid.value) {
    showAddressError.value = true
    return
  }
  const input: AddressInput = {
    addressType: 'Shipping',
    firstName: firstName.value,
    lastName: lastName.value || undefined,
    address1: address1.value,
    city: city.value,
    zipCode: zipCode.value || undefined,
    phone: phone.value || undefined,
    isDefault: false,
    countryName: selectedCountry.value?.name ?? '',
    countryCode: selectedCountry.value?.isoCode ?? undefined,
    stateProvince: selectedState.value?.name ?? undefined,
    stateCode: selectedState.value?.abbreviation ?? undefined,
  }
  const created = await addresses.createAddress(input)
  if (created) {
    const saved = addresses.addresses[addresses.addresses.length - 1]
    if (saved) await checkout.saveAddress(saved.id, email.value)
  }
}

// Delivery: Local radio selection; persisted through the checkout store on continue.
const selectedShippingId = ref<string | null>(null)

// Cost: Resolve a method's customer-facing price from the fetched rates.
function methodCost(methodId: string): number | null {
  const rate = shipping.rates.find((r) => r.shippingMethodId === methodId)
  return rate ? (rate.finalPrice ?? rate.cost) : null
}

// Rate: Customer-facing price for the checkout-selected method.
const shippingCost = computed(() =>
  checkout.shippingMethodId ? methodCost(checkout.shippingMethodId) : null,
)

// Action: Persist the chosen shipping method, then advance to the payment panel.
async function continueToPayment(): Promise<void> {
  if (!selectedShippingId.value) return
  shipping.selectMethod(selectedShippingId.value)
  await checkout.selectShippingRate(selectedShippingId.value)
}

// Resolve: Pick the active gateway method (e.g. Credit Card) for the payment intent.
async function resolvePaymentMethod(): Promise<void> {
  if (paymentMethodId.value) return
  const result = await getPaymentMethods({ pageSize: 50 })
  const method = result.isSuccess ? (result.items.find((m) => m.active) ?? result.items[0]) : undefined
  paymentMethodId.value = method?.id ?? null
}

// Mount: Attach the Stripe Elements card form to the payment panel container.
async function mountCard(): Promise<void> {
  await nextTick()
  if (!checkout.paymentClientSecret || !cardContainer.value) return
  payment.unmount()
  await payment.mount(checkout.paymentClientSecret, cardContainer.value)
}

// Watch: Prepare the payment intent and card form on entry to the payment panel.
watch(
  () => checkout.displayStep,
  async (step) => {
    if (step !== 3) {
      payment.unmount()
      return
    }
    await resolvePaymentMethod()
    // Guard: Create the intent lazily; skip it on a reload where the backend is already at Payment.
    if (!checkout.paymentClientSecret && paymentMethodId.value) {
      if (checkout.backendStep === 2) {
        await checkout.createPaymentIntent(paymentMethodId.value)
      } else if (checkout.backendStep === 3) {
        checkout.error = 'Payment needs to be re-initiated. Go back and re-save your shipping method.'
      }
    }
    await mountCard()
  },
  { immediate: true },
)

// Total: Subtotal plus the selected shipping rate; tax is not modelled yet.
const total = computed(() => cart.subtotal + (shippingCost.value ?? 0))

// Action: Submit the order through the checkout store and land on confirmation.
async function onPlaceOrder(): Promise<void> {
  await checkout.placeOrder()
}

// Navigate: Allow stepping back through completed panels; forward flow is action-driven.
function goToStep(value: number): void {
  if (value >= 1 && value <= Math.max(checkout.displayStep, checkout.backendStep)) {
    checkout.displayStep = value as CheckoutStep
  }
}

// Navigate: Force-refresh cart state and confirm readiness before showing the review panel.
async function advanceToReview(): Promise<void> {
  if (!checkout.paymentClientSecret) return
  checkout.loading = true
  checkout.error = null
  const cartOk = await cart.fetchCart(true)
  if (!cartOk) {
    checkout.error = 'Could not refresh the cart. Please try again.'
    checkout.loading = false
    return
  }
  const validOk = await checkout.validateCheckout()
  checkout.loading = false
  // Gate: Only land on Review when the re-synced backend is still at Payment with a live intent.
  if (validOk && checkout.backendStep === 3 && checkout.paymentClientSecret) {
    checkout.displayStep = 4
  }
}

onMounted(async () => {
  // Load: Refresh reference data and the cart on page entry.
  void addresses.fetchAddresses()
  void location.loadAll()
  void shipping.fetchMethods()
  await cart.fetchCart()
  checkout.displayStep = Math.min(5, checkout.backendStep) as CheckoutStep
  selectedShippingId.value = cart.shippingMethodId
  email.value = cart.email ?? auth.user?.email ?? ''
  if (cart.id) void shipping.fetchRates(cart.id)
  // Guard: Bounce empty carts back to the cart page unless an order was just confirmed.
  if (cart.isEmpty && checkout.displayStep !== 5) {
    await router.push('/cart')
  }
})

onUnmounted(() => {
  // Dispose: Detach the Stripe card element when leaving the view.
  payment.unmount()
})
</script>

<template>
  <!-- Section: Page Header — title for the checkout wizard -->
  <div class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
    <h1 class="mb-8 text-2xl font-bold">Checkout</h1>

    <!-- Section: Wizard — five-panel stepper driven by the checkout store -->
    <Stepper :value="checkout.displayStep" @update:value="goToStep($event)">
      <StepList>
        <Step :value="1">Shipping</Step>
        <Step :value="2">Delivery</Step>
        <Step :value="3">Payment</Step>
        <Step :value="4">Review</Step>
        <Step :value="5">Confirmation</Step>
      </StepList>
      <StepPanels>
        <!-- Panel: Shipping — saved-address picker or a new address form -->
        <StepPanel :value="1">
          <div class="max-w-xl space-y-5">
            <Message v-if="checkout.error" severity="error" :closable="false">{{ checkout.error }}</Message>
            <Message v-if="addresses.error" severity="error" :closable="false">{{ addresses.error }}</Message>
            <div class="grid gap-4">
              <div>
                <Label for="saved-address" class="mb-1 block text-sm font-medium">Saved Address</Label>
                <Select
                  id="saved-address"
                  v-model="selectedAddressId"
                  :options="addressOptions"
                  optionLabel="label"
                  optionValue="id"
                  placeholder="Choose a saved address"
                  class="w-full"
                  @update:model-value="selectSavedAddress($event)"
                />
              </div>
              <Divider />
              <FloatLabel variant="on">
                <InputText id="checkout-first-name" v-model="firstName" class="w-full" />
                <Label for="checkout-first-name">First Name</Label>
              </FloatLabel>
              <FloatLabel variant="on">
                <InputText id="checkout-last-name" v-model="lastName" class="w-full" />
                <Label for="checkout-last-name">Last Name</Label>
              </FloatLabel>
              <FloatLabel variant="on">
                <InputText id="checkout-address1" v-model="address1" class="w-full" />
                <Label for="checkout-address1">Street Address</Label>
              </FloatLabel>
              <FloatLabel variant="on">
                <InputText id="checkout-city" v-model="city" class="w-full" />
                <Label for="checkout-city">City</Label>
              </FloatLabel>
              <FloatLabel variant="on">
                <InputText id="checkout-zip" v-model="zipCode" class="w-full" />
                <Label for="checkout-zip">ZIP / Postal Code</Label>
              </FloatLabel>
              <FloatLabel variant="on">
                <InputMask id="checkout-phone" v-model="phone" mask="(999) 999-9999" class="w-full" />
                <Label for="checkout-phone">Phone</Label>
              </FloatLabel>
              <FloatLabel variant="on">
                <InputText id="checkout-email" v-model="email" type="email" class="w-full" />
                <Label for="checkout-email">Email</Label>
              </FloatLabel>
              <div>
                <Label for="checkout-country" class="mb-1 block text-sm font-medium">Country / State</Label>
                <CascadeSelect
                  id="checkout-country"
                  v-model="cascadeValue"
                  :options="cascadeOptions"
                  optionLabel="name"
                  optionValue="id"
                  optionGroupLabel="name"
                  optionGroupChildren="children"
                  placeholder="Country / State"
                  class="w-full"
                >
                  <!-- Label: Show the full country / state path instead of the leaf only -->
                  <template #value="{ placeholder }">{{ cascadeLabel || placeholder }}</template>
                </CascadeSelect>
              </div>
            </div>
            <Message v-if="showAddressError" severity="warn" :closable="false">
              Complete the required address fields to continue.
            </Message>
            <ButtonGroup>
              <Button as="router-link" to="/cart" label="Back to Cart" icon="pi pi-arrow-left" variant="text" />
              <Button label="Continue to Delivery" icon="pi pi-arrow-right" iconPos="right" :loading="checkout.loading" @click="continueToDelivery" />
            </ButtonGroup>
          </div>
        </StepPanel>

        <!-- Panel: Delivery — shipping method selection from the shipping store -->
        <StepPanel :value="2">
          <div class="max-w-xl space-y-5">
            <Message v-if="checkout.error" severity="error" :closable="false">{{ checkout.error }}</Message>
            <Message v-if="shipping.error" severity="error" :closable="false">{{ shipping.error }}</Message>
            <RadioButtonGroup v-model="selectedShippingId" class="flex flex-col gap-3">
              <div v-for="method in shipping.methods" :key="method.id" class="flex items-center gap-3">
                <RadioButton :input-id="`method-${method.id}`" :value="method.id" />
                <Label :for="`method-${method.id}`" class="flex w-full cursor-pointer items-center justify-between">
                  <span>{{ method.name }}</span>
                  <span v-if="methodCost(method.id) !== null" class="font-mono text-sm">{{ formatCurrency(methodCost(method.id)!) }}</span>
                  <span v-else class="text-sm text-muted">Calculated at checkout</span>
                </Label>
              </div>
            </RadioButtonGroup>
            <Message v-if="shipping.methods.length === 0 && !shipping.loading" severity="info" :closable="false">
              Shipping methods are loading.
            </Message>
            <ButtonGroup>
              <Button label="Back" icon="pi pi-arrow-left" variant="text" @click="goToStep(1)" />
              <Button label="Continue to Payment" icon="pi pi-arrow-right" iconPos="right" :loading="checkout.loading" :disabled="!selectedShippingId" @click="continueToPayment" />
            </ButtonGroup>
          </div>
        </StepPanel>

        <!-- Panel: Payment — Stripe Elements card form bound to the payment intent -->
        <StepPanel :value="3">
          <div class="max-w-xl space-y-5">
            <Message v-if="checkout.error" severity="error" :closable="false">{{ checkout.error }}</Message>
            <Message v-if="payment.error" severity="error" :closable="false">{{ payment.error }}</Message>
            <Message v-if="!paymentMethodId && !checkout.loading" severity="warn" :closable="false">
              No active payment method is available.
            </Message>
            <!-- Card: Stripe Elements mounts the hosted card form into this container -->
            <div ref="cardContainer" class="rounded-lg border border-surface-200 p-4" />
            <p class="text-sm text-muted">
              Card details are processed securely by Stripe. Payment is confirmed when you place the order.
            </p>
            <ButtonGroup>
              <Button label="Back" icon="pi pi-arrow-left" variant="text" @click="goToStep(2)" />
              <Button label="Continue to Review" icon="pi pi-arrow-right" iconPos="right" :disabled="!checkout.paymentClientSecret" :loading="checkout.loading" @click="advanceToReview" />
            </ButtonGroup>
          </div>
        </StepPanel>

        <!-- Panel: Review — line-item table, totals and order placement -->
        <StepPanel :value="4">
          <div class="max-w-xl space-y-5">
            <Message v-if="checkout.error" severity="error" :closable="false">{{ checkout.error }}</Message>
            <DataTable :value="cart.items" size="small">
              <Column header="Product">
                <template #body="{ data }">{{ data.productName ?? data.variantName }}</template>
              </Column>
              <Column field="sku" header="SKU" />
              <Column field="quantity" header="Qty" />
              <Column header="Price">
                <template #body="{ data }">{{ formatCurrency(data.price) }}</template>
              </Column>
              <Column header="Total">
                <template #body="{ data }">{{ formatCurrency(data.total) }}</template>
              </Column>
            </DataTable>
            <div class="flex max-w-md flex-col gap-2 text-sm">
              <div class="flex justify-between">
                <span class="text-muted">Subtotal</span>
                <span>{{ formatCurrency(cart.subtotal) }}</span>
              </div>
              <div class="flex justify-between">
                <span class="text-muted">Shipping</span>
                <span>{{ shippingCost === null ? 'Calculated at checkout' : formatCurrency(shippingCost) }}</span>
              </div>
              <Divider />
              <div class="flex justify-between font-semibold">
                <span>Total</span>
                <span>{{ formatCurrency(total) }}</span>
              </div>
            </div>
            <ButtonGroup>
              <Button label="Back" icon="pi pi-arrow-left" variant="text" @click="goToStep(3)" />
              <Button label="Place Order" icon="pi pi-check" :loading="checkout.loading" @click="onPlaceOrder" />
            </ButtonGroup>
          </div>
        </StepPanel>

        <!-- Panel: Confirmation — success message with order number and account link -->
        <StepPanel :value="5">
          <div class="max-w-xl space-y-5 py-8 text-center">
            <i class="pi pi-check-circle block text-5xl text-success" />
            <Message severity="success" :closable="false">
              <div class="flex flex-col items-center gap-1">
                <span class="font-semibold">Order confirmed!</span>
                <span v-if="checkout.orderId" class="text-sm">Order number: {{ checkout.orderId }}</span>
              </div>
            </Message>
            <p class="text-sm text-muted">A confirmation email has been sent to {{ checkout.email || email }}.</p>
            <Button as="router-link" to="/account/orders" label="View My Orders" icon="pi pi-receipt" />
          </div>
        </StepPanel>
      </StepPanels>
    </Stepper>
  </div>
</template>
