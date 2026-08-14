<script setup lang="ts">
import Label from 'primevue/label'
import { computed, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { formatCurrency } from '@/shared/utils/currency'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { useLocation } from '@/features/location/composables'
import { getPaymentMethods } from '@/features/payment/services/paymentApi'
import type { PaymentMethod } from '@/features/payment/types/payment'
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

// Methods: Customer-facing payment methods for the payment panel.
const paymentMethods = ref<PaymentMethod[]>([])
const selectedPaymentMethodId = ref<string | null>(null)

// Load: Fetch active, customer-facing payment methods for selection.
async function loadPaymentMethods(): Promise<void> {
  const result = await getPaymentMethods({ pageSize: 50 })
  paymentMethods.value = result.isSuccess ? result.items : []
  selectedPaymentMethodId.value = paymentMethods.value[0]?.id ?? null
}

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

// Watch: Load payment methods when entering the payment panel.
watch(
  () => checkout.displayStep,
  async (step) => {
    if (step === 3) await loadPaymentMethods()
  },
  { immediate: true },
)

// Action: Card → create intent and advance to Review (payment happens on confirm);
// COD → advance to Review for explicit placement.
async function onContinueFromPayment(): Promise<void> {
  const method = paymentMethods.value.find((m) => m.id === selectedPaymentMethodId.value)
  if (!method) return

  const origin = window.location.origin
  const ok = await checkout.createPaymentIntent(method.id, {
    returnUrl: `${origin}/checkout/return`,
    cancelUrl: `${origin}/checkout`,
  })
  if (!ok) return

  // Confirm-before-charge: both paths land on Review; the card's hosted-checkout
  // redirect happens only after the customer confirms on the Review panel.
  await advanceToReview()
}

// Action: Confirm the reviewed order and redirect to Stripe for the card charge.
function onConfirmAndPay(): void {
  if (checkout.checkoutUrl) {
    window.location.href = checkout.checkoutUrl
  }
}

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
  if (!checkout.paymentIntentId) return
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
  if (validOk && checkout.backendStep === 3 && checkout.paymentIntentId) {
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

        <!-- Panel: Payment — customer-facing payment method selection and redirect -->
        <StepPanel :value="3">
          <div class="max-w-xl space-y-5">
            <Message v-if="checkout.error" severity="error" :closable="false">{{ checkout.error }}</Message>
            <Message v-if="paymentMethods.length === 0 && !checkout.loading" severity="warn" :closable="false">
              No payment methods are available.
            </Message>
            <!-- Section: Payment Methods — radio list of customer-facing methods -->
            <RadioButtonGroup v-model="selectedPaymentMethodId" class="flex flex-col gap-3">
              <div v-for="method in paymentMethods" :key="method.id" class="flex items-center gap-3">
                <RadioButton :input-id="`pm-${method.id}`" :value="method.id" />
                <Label :for="`pm-${method.id}`" class="cursor-pointer">{{ method.name }}</Label>
              </div>
            </RadioButtonGroup>
            <ButtonGroup>
              <Button label="Back" icon="pi pi-arrow-left" variant="text" @click="goToStep(2)" />
              <Button
                label="Continue"
                icon="pi pi-arrow-right"
                iconPos="right"
                :disabled="!selectedPaymentMethodId"
                :loading="checkout.loading"
                @click="onContinueFromPayment"
              />
            </ButtonGroup>
          </div>
        </StepPanel>

        <!-- Panel: Review — item tickets and the order summary card before placement -->
        <StepPanel :value="4">
          <div class="grid gap-6 lg:grid-cols-3">
            <Message v-if="checkout.error" severity="error" :closable="false" class="lg:col-span-3">{{ checkout.error }}</Message>

            <!-- Zone: Items — ticket cards mirroring the cart drawer for a familiar read -->
            <div class="flex flex-col gap-3 lg:col-span-2">
              <div
                v-for="item in cart.items"
                :key="item.id"
                class="flex items-center gap-4 rounded-lg border border-surface-200 bg-surface-0 p-4"
              >
                <Image
                  v-if="item.productImageUrl"
                  :src="item.productImageUrl"
                  :alt="item.productName ?? item.variantName"
                  imageClass="h-16 w-16 shrink-0 rounded-md object-cover"
                />
                <div
                  v-else
                  class="flex h-16 w-16 shrink-0 items-center justify-center rounded-md bg-surface-100"
                >
                  <i class="pi pi-image text-lg text-placeholder" />
                </div>
                <div class="min-w-0 flex-1">
                  <div class="truncate font-semibold">{{ item.productName ?? item.variantName }}</div>
                  <div class="text-sm text-muted">{{ item.sku }}</div>
                </div>
                <div class="flex shrink-0 flex-col items-end gap-1">
                  <div class="text-sm text-muted">{{ item.quantity }} × {{ formatCurrency(item.price) }}</div>
                  <div class="font-semibold">{{ formatCurrency(item.total) }}</div>
                </div>
              </div>
            </div>

            <!-- Zone: Summary — order totals and the placement action -->
            <Panel header="Order Summary" class="h-fit">
              <div class="flex flex-col gap-3 text-sm">
                <div class="flex justify-between">
                  <span class="text-muted">Subtotal</span>
                  <span>{{ formatCurrency(cart.subtotal) }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-muted">Shipping</span>
                  <span>{{ shippingCost === null ? 'Calculated at checkout' : formatCurrency(shippingCost) }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-muted">Payment method</span>
                  <span>{{ paymentMethods.find((m) => m.id === selectedPaymentMethodId)?.name ?? '—' }}</span>
                </div>
                <Divider />
                <div class="flex justify-between font-semibold">
                  <span>Total</span>
                  <span>{{ formatCurrency(total) }}</span>
                </div>
              </div>
              <div class="mt-4 flex flex-col gap-2">
                <Button
                  v-if="checkout.checkoutUrl"
                  label="Confirm and Pay"
                  icon="pi pi-credit-card"
                  :loading="checkout.loading"
                  class="w-full"
                  @click="onConfirmAndPay"
                />
                <Button v-else label="Place Order" icon="pi pi-check" :loading="checkout.loading" class="w-full" @click="onPlaceOrder" />
                <Button label="Back" icon="pi pi-arrow-left" variant="text" class="w-full" @click="goToStep(3)" />
              </div>
            </Panel>
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
