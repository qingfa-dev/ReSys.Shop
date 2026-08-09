<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { MenuItem } from 'primevue/menuitem'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { formatCurrency } from '@/shared/utils/currency'
import { useProductDetail } from '../composables/useProductDetail'
import { useCart } from '@/features/ordering/composables/useCart'
import { useWishlistStore } from '@/features/profile/stores/wishlistStore'
import { useAvailabilityStore } from '@/features/inventory/stores/availabilityStore'
import type { AvailabilityEntry } from '@/features/inventory/types/availability'
import ProductCard from '../components/ProductCard.vue'

// Type: Breadcrumb trail items carry a router target and a current-page marker
interface BreadcrumbItemModel extends MenuItem {
  to?: string
  current?: boolean
}

const route = useRoute()
const router = useRouter()
const detail = useProductDetail()
const cart = useCart()
const wishlist = useWishlistStore()
const availability = useAvailabilityStore()
const notify = useNotify()
const { handleError } = useApiErrorHandler()

// Title: Browser tab reflects the product name once loaded
usePageTitle(() => detail.product?.name ?? 'Product')

// Gallery: Fullscreen zoom/rotate viewer launched from the Galleria item click
const galleryOpen = ref(false)
const galleryActiveIndex = ref(0)

// Tabs: Active tab id for the description/details/reviews panels
const activeTab = ref('description')

// Accordion: Shipping panel open by default
const shippingOpen = ref<string | null>('shipping')

// Stock: Per-variant availability entry from the inventory sidecar — null until resolved
const stockEntry = ref<AvailabilityEntry | null>(null)

// Rating: DTO exposes no rating fields — block stays dormant until the API adds them
const ratingAverage = ref<number | null>(null)
const ratingCount = ref<number | null>(null)

// Breadcrumb: Home + Shop prefix, then the first classification trail, then the product
const homeItem = computed<MenuItem>(() => ({ label: 'Home', icon: 'pi pi-home', to: '/' }))
const breadcrumbItems = computed<MenuItem[]>(() => {
  const items: BreadcrumbItemModel[] = [{ label: 'Shop', to: '/shop' }]
  const trail = detail.product?.classifications?.[0]?.breadcrumb ?? []
  // Map: Taxon trail entries navigate back to the shop filtered by taxon id
  for (const crumb of trail) items.push({ label: crumb.name, to: `/shop?taxon=${crumb.id}` })
  if (detail.product) items.push({ label: detail.product.name, current: true })
  return items
})

// Images: Selected variant images, falling back to the master variant gallery
const images = computed(() => {
  const source = detail.selectedVariant ?? detail.product?.masterVariant
  const list = source?.images.length ? source.images : detail.product?.masterVariant?.images
  return list ?? []
})

// Price: Selected variant price and compare-at strikethrough
const price = computed(() => detail.selectedVariant?.price ?? detail.product?.masterVariant?.price ?? null)
const compareAtPrice = computed(() =>
  detail.selectedVariant?.prices.find(p => p.compareAtAmount != null)?.compareAtAmount
    ?? detail.product?.masterVariant?.prices.find(p => p.compareAtAmount != null)?.compareAtAmount
    ?? null,
)
const isOnSale = computed(() => price.value != null && compareAtPrice.value != null && compareAtPrice.value > price.value)
const formattedPrice = computed(() => (price.value != null ? formatCurrency(price.value) : null))
const formattedCompareAt = computed(() => (compareAtPrice.value != null ? formatCurrency(compareAtPrice.value) : null))

// Badges: Promo tag derived from compare-at pricing — DTO has no "new" flag
const badges = computed(() => {
  const list: { label: string; severity: 'danger' | 'info' }[] = []
  if (isOnSale.value) list.push({ label: 'Sale', severity: 'danger' })
  return list
})

// Variants: DTO exposes optionValues per variant — build the Select from them
const variantOptions = computed(() =>
  (detail.product?.variants ?? []).map(variant => ({
    value: variant.id,
    label: variant.optionValues.length > 0
      ? variant.optionValues.map(o => o.presentation ?? o.name).join(' / ')
      : variant.sku ?? 'Default',
  })),
)

// Meter: Stock level from the availability entry — available vs reserved split
const stockMeter = computed(() => {
  const entry = stockEntry.value
  if (!entry || entry.countOnHand <= 0) return null
  return {
    max: entry.countOnHand,
    value: [
      { label: 'Available', value: entry.availableCount, color: 'var(--p-primary-color)' },
      { label: 'Reserved', value: entry.reservedCount, color: 'var(--p-surface-300)' },
    ],
  }
})

// Stock: Message prefers the availability entry, falling back to the DTO stock label
const stockSeverity = computed<'success' | 'warn' | 'error'>(() => {
  const entry = stockEntry.value
  if (entry) {
    if (entry.available) return 'success'
    if (entry.backorderable) return 'warn'
    return 'error'
  }
  if (detail.isInStock) return 'success'
  return detail.stockLabel ? 'warn' : 'error'
})
const stockMessage = computed(() => {
  const entry = stockEntry.value
  if (entry) {
    if (entry.available) return 'In stock'
    if (entry.backorderable) return 'Available for backorder'
    return 'Out of stock'
  }
  return detail.stockLabel ?? (detail.isInStock ? 'In stock' : 'Out of stock')
})

// Tabs: Details rows are rendered only for populated DTO fields
const detailRows = computed(() => {
  const p = detail.product
  if (!p) return []
  return [
    { label: 'Style code', value: p.styleCode },
    { label: 'Material', value: p.materialComposition },
    { label: 'Care', value: p.careInstructions },
    { label: 'Fit', value: p.fitNotes },
    { label: 'Department', value: p.department },
    { label: 'Target', value: p.genderTarget },
    { label: 'Season', value: p.seasonName },
  ].filter(row => row.value != null && row.value.length > 0)
})

const description = computed(() =>
  detail.product?.description?.length
    ? detail.product.description
    : 'No description available for this product yet.',
)

// Purchase: SplitButton items — buy now routes to the cart, wishlist adds to the default list
const purchaseMenu = computed<MenuItem[]>(() => [
  { label: 'Buy Now', icon: 'pi pi-bolt', command: () => void buyNow() },
  { label: 'Add to Wishlist', icon: 'pi pi-heart', command: () => void addToWishlist() },
])

// Load: Fetch the product by slug and reset navigation-sensitive state
function loadProduct(slug: string): void {
  if (!slug) return
  galleryActiveIndex.value = 0
  activeTab.value = 'description'
  stockEntry.value = null
  void detail.load(slug)
}

// Gallery: Open the fullscreen viewer at the clicked image index
function openGallery(index: number): void {
  galleryActiveIndex.value = index
  galleryOpen.value = true
}

// Cart: Add the selected variant with the current quantity
async function addToCart(): Promise<boolean> {
  const variantId = detail.selectedVariantId
  if (!variantId) return false
  const ok = await cart.addItem(variantId, detail.quantity)
  if (ok) notify.success('Added to cart')
  else handleError(new Error(cart.error ?? 'Failed to add to cart'))
  return ok
}

// Buy: Add to cart, then proceed straight to the cart page
async function buyNow(): Promise<void> {
  const ok = await addToCart()
  if (ok) await router.push('/cart')
}

// Wishlist: Add the selected variant to the default list
async function addToWishlist(): Promise<void> {
  const variantId = detail.selectedVariantId
  const list = Object.values(wishlist.details).find(d => d.isDefault)
  if (!variantId || !list) {
    notify.warn('Sign in to save items to a wishlist')
    return
  }
  const ok = await wishlist.addItem(list.id, { variantId, quantity: detail.quantity })
  if (ok) notify.success('Added to wishlist')
  else handleError(new Error(wishlist.error ?? 'Failed to update wishlist'))
}

// Watch: Refresh availability when the selected variant changes
watch(() => detail.selectedVariantId, async id => {
  stockEntry.value = null
  if (!id) return
  const entry = await availability.check(id)
  // Guard: Ignore stale responses after rapid variant switches
  if (entry && detail.selectedVariantId === id) stockEntry.value = entry
})

onMounted(() => {
  const slug = route.params.slug
  loadProduct(typeof slug === 'string' ? slug : '')
})

// Watch: Reload when navigating between products
watch(() => route.params.slug, slug => {
  loadProduct(typeof slug === 'string' ? slug : '')
})
</script>

<template>
  <div class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
    <!-- Section: Breadcrumb — home / shop / taxon trail / current product -->
    <Breadcrumb :model="breadcrumbItems" :home="homeItem" class="mb-6">
      <template #item="{ item, label, icon, props }">
        <span
          v-if="(item as BreadcrumbItemModel).current"
          class="font-medium text-heading"
        >
          {{ label }}
        </span>
        <RouterLink
          v-else
          :to="(item as BreadcrumbItemModel).to ?? '/shop'"
          v-bind="props.action"
          class="flex items-center gap-1 text-muted transition-colors hover:text-brand"
        >
          <i v-if="typeof icon === 'string' && icon" :class="icon" />
          {{ label }}
        </RouterLink>
      </template>
    </Breadcrumb>

    <!-- Loading: Skeleton blocks while the detail fetch is in flight -->
    <div v-if="detail.loading && !detail.product" class="grid grid-cols-1 gap-10 lg:grid-cols-2">
      <Skeleton class="aspect-square w-full rounded-2xl" />
      <div class="space-y-4">
        <Skeleton width="60%" height="2rem" />
        <Skeleton width="40%" height="1.5rem" />
        <Skeleton width="100%" height="3rem" />
        <Skeleton width="100%" height="3rem" />
      </div>
    </div>

    <!-- Error State: Load failure message with a way back to the shop -->
    <div v-else-if="detail.error && !detail.product" class="flex flex-col items-center gap-4 py-16 text-center">
      <Message severity="error" :closable="false">{{ detail.error }}</Message>
      <Button label="Back to shop" icon="pi pi-arrow-left" as="router-link" to="/shop" />
    </div>

    <!-- Content: Two-column detail layout once the product resolves -->
    <div v-else-if="detail.product" class="grid grid-cols-1 gap-10 lg:grid-cols-2">
      <!-- Gallery: Responsive thumbnail carousel opening the fullscreen viewer -->
      <div class="min-w-0">
        <Galleria v-if="images.length > 0"
          :value="images"
          v-model:activeIndex="galleryActiveIndex"
          :numVisible="4"
          :circular="true"
          :responsiveOptions="[
            { breakpoint: '1024px', numVisible: 3 },
            { breakpoint: '640px', numVisible: 2 },
          ]"
          class="w-full"
        >
          <template #item="{ item }">
            <div
              role="button"
              tabindex="0"
              aria-label="Open image viewer"
              class="cursor-zoom-in"
              @click="openGallery(images.indexOf(item))"
              @keydown.enter="openGallery(images.indexOf(item))"
            >
              <img
                :src="item.url"
                :alt="item.alt ?? detail.product?.name ?? 'Product image'"
                class="aspect-square w-full rounded-2xl object-cover"
              />
            </div>
          </template>
          <template #thumbnail="{ item }">
            <img
              :src="item.url"
              :alt="item.alt ?? detail.product?.name ?? 'Product image'"
              class="aspect-square w-full object-cover"
            />
          </template>
        </Galleria>

        <!-- Fallback: Placeholder block when the product has no images -->
        <div
          v-else
          class="flex aspect-square items-center justify-center rounded-2xl bg-surface-100"
        >
          <i class="pi pi-image text-4xl text-placeholder" />
        </div>

        <!-- Viewer: Fullscreen Gallery with zoom, rotate, flip and download actions -->
        <Gallery v-if="galleryOpen"
          fullscreen
          v-model:activeIndex="galleryActiveIndex"
          @update:fullscreen="galleryOpen = $event"
        >
          <GalleryBackdrop />
          <GalleryPrev><i class="pi pi-chevron-left" /></GalleryPrev>
          <GalleryNext><i class="pi pi-chevron-right" /></GalleryNext>
          <GalleryHeader class="justify-end gap-0.5">
            <GalleryRotateLeft><i class="pi pi-replay" /></GalleryRotateLeft>
            <GalleryRotateRight><i class="pi pi-refresh" /></GalleryRotateRight>
            <GalleryZoomIn><i class="pi pi-search-plus" /></GalleryZoomIn>
            <GalleryZoomOut><i class="pi pi-search-minus" /></GalleryZoomOut>
            <GalleryFlipX><i class="pi pi-arrows-h" /></GalleryFlipX>
            <GalleryFlipY><i class="pi pi-arrows-v" /></GalleryFlipY>
            <GalleryDownload><i class="pi pi-download" /></GalleryDownload>
            <GalleryFullScreen><i class="pi pi-window-maximize" /></GalleryFullScreen>
          </GalleryHeader>
          <GalleryContent>
            <GalleryItem v-for="image in images" :key="image.id">
              <img :src="image.url" :alt="image.alt ?? detail.product?.name ?? 'Product image'" />
            </GalleryItem>
          </GalleryContent>
          <GalleryFooter>
            <GalleryThumbnail>
              <GalleryThumbnailContent>
                <GalleryThumbnailItem v-for="(image, index) in images" :key="image.id" :index="index">
                  <img draggable="false" :src="image.url" class="h-full w-full object-cover" />
                </GalleryThumbnailItem>
              </GalleryThumbnailContent>
            </GalleryThumbnail>
          </GalleryFooter>
        </Gallery>
      </div>

      <!-- Purchase Panel: title, rating, price, variant, quantity, actions, stock -->
      <div class="space-y-6">
        <div>
          <h1 class="text-3xl font-semibold tracking-tight text-heading">
            {{ detail.product.name }}
          </h1>
          <div class="mt-2 flex items-center gap-3">
            <Rating v-if="ratingAverage !== null" :modelValue="ratingAverage" readonly />
            <span v-if="ratingCount !== null && ratingCount > 0" class="text-sm text-muted">
              {{ ratingCount }} reviews
            </span>
            <MeterGroup v-if="stockMeter" :value="stockMeter.value" :max="stockMeter.max" class="w-full max-w-64" />
          </div>
        </div>

        <!-- Price: Current price, compare-at strikethrough and promo badges -->
        <div class="flex flex-wrap items-center gap-3">
          <span
            v-if="formattedPrice"
            class="text-2xl font-bold text-heading"
          >
            {{ formattedPrice }}
          </span>
          <span v-if="formattedCompareAt" class="text-lg text-muted line-through">
            {{ formattedCompareAt }}
          </span>
          <Tag v-for="badge in badges" :key="badge.label" :value="badge.label" :severity="badge.severity" />
        </div>

        <!-- Variant: Select driven by the DTO option values when more than one exists -->
        <Select
          v-if="variantOptions.length > 1"
          :modelValue="detail.selectedVariantId"
          :options="variantOptions"
          optionLabel="label"
          optionValue="value"
          class="w-full max-w-xs"
          :aria-label="`Select variant of ${detail.product.name}`"
          @change="detail.selectVariant($event.value)"
        />

        <!-- Purchase: Quantity stepper and split add-to-cart button -->
        <div class="flex flex-wrap items-center gap-3">
          <InputNumber
            v-model="detail.quantity"
            :min="1"
            :max="99"
            :showButtons="true"
            buttonLayout="horizontal"
            inputClass="w-12 text-center"
            class="w-32"
            aria-label="Quantity"
          />
          <SplitButton label="Add to Cart"
            icon="pi pi-shopping-cart"
            :model="purchaseMenu"
            :disabled="!detail.selectedVariantId"
            @click="addToCart"
          />
        </div>

        <!-- Stock: Availability message from the inventory sidecar entry -->
        <Message :severity="stockSeverity" :closable="false">
          {{ stockMessage }}
        </Message>

        <Divider />

        <!-- Tabs: Description, details and reviews panels -->
        <Tabs v-model:value="activeTab">
          <TabList>
            <Tab value="description">Description</Tab>
            <Tab value="details">Details</Tab>
            <Tab value="reviews">Reviews</Tab>
          </TabList>
          <TabPanels>
            <TabPanel value="description">
              <p class="text-muted">{{ description }}</p>
            </TabPanel>
            <TabPanel value="details">
              <dl class="grid grid-cols-1 gap-2 sm:grid-cols-2">
                <div
                  v-for="row in detailRows"
                  :key="row.label"
                  class="flex justify-between gap-4 border-b border-surface-200 py-2"
                >
                  <dt class="text-sm text-muted">{{ row.label }}</dt>
                  <dd class="text-sm font-medium text-heading">
                    {{ row.value }}
                  </dd>
                </div>
              </dl>
            </TabPanel>
            <TabPanel value="reviews">
              <p class="text-muted">
                No reviews yet — check back after the first customers share their experience.
              </p>
            </TabPanel>
          </TabPanels>
        </Tabs>

        <!-- Accordion: Shipping and returns policy -->
        <Accordion v-model:value="shippingOpen">
          <AccordionPanel value="shipping">
            <AccordionHeader>Shipping &amp; Returns</AccordionHeader>
            <AccordionContent>
              <p class="text-sm text-muted">
                Free standard shipping on orders over $100, delivered within 3–5 business days.
                Returns are accepted within 30 days of delivery in original condition.
              </p>
            </AccordionContent>
          </AccordionPanel>
        </Accordion>
      </div>
    </div>

    <!-- Related: Grid of related products loaded by the detail store -->
    <section v-if="detail.product && detail.relatedProducts.length > 0" class="mt-16">
      <h2 class="mb-6 text-2xl font-semibold tracking-tight text-heading">
        You may also like
      </h2>
      <div class="grid grid-cols-2 gap-6 lg:grid-cols-4">
        <ProductCard v-for="product in detail.relatedProducts" :key="product.id" :product="product" />
      </div>
    </section>
  </div>
</template>
