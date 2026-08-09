<script setup lang="ts">
import { computed, ref } from "vue";
import type { ComponentPublicInstance } from "vue";
import type { MenuItem } from "primevue/menuitem";
import type ContextMenu from "primevue/contextmenu";
import type Popover from "primevue/popover";
import type { StoreProductListItemResponse } from "../types";
import { formatCurrency } from "@/shared/utils/currency";
import { useWishlists } from "@/features/profile/composables/useWishlists";
import { useQuickAdd } from "@/features/ordering/composables/useQuickAdd";

const props = withDefaults(
  defineProps<{
    product: StoreProductListItemResponse;
    ratingAverage?: number;
    ratingCount?: number;
    showSimilarity?: boolean;
    similarityScore?: number;
  }>(),
  {
    ratingAverage: 0,
    ratingCount: 0,
    showSimilarity: false,
    similarityScore: 0,
  },
);

const emit = defineEmits<{
  "add-to-cart": [product: StoreProductListItemResponse];
  "toggle-wishlist": [product: StoreProductListItemResponse];
}>();

const wishlistStore = useWishlists();
const { add: quickAdd } = useQuickAdd();

const quickViewMenu = ref<InstanceType<typeof ContextMenu> | null>(null);
const quickViewPopover = ref<InstanceType<typeof Popover> | null>(null);
const quickViewButton = ref<ComponentPublicInstance | null>(null);

// Derive: Primary image and alt text from the master variant.
const imageUrl = computed(() => props.product.masterVariant?.images?.[0]?.url ?? null);
const imageAlt = computed(
  () => props.product.masterVariant?.images?.[0]?.alt ?? props.product.name,
);

// Derive: Price from the master variant, compare-at from the first marked price.
const price = computed(() => props.product.masterVariant?.price ?? null);
const compareAtPrice = computed(
  () =>
    props.product.masterVariant?.prices.find((p) => p.compareAtAmount != null)?.compareAtAmount ??
    null,
);
const isOnSale = computed(
  () => price.value != null && compareAtPrice.value != null && compareAtPrice.value > price.value,
);
const formattedPrice = computed(() => (price.value != null ? formatCurrency(price.value) : null));
const formattedCompareAt = computed(() =>
  compareAtPrice.value != null ? formatCurrency(compareAtPrice.value) : null,
);

// Derive: Similarity percentage badge for visual search results.
const similarityPercent = `${(props.similarityScore * 100).toFixed(1)}%`;

// Derive: Short description (first 120 chars).
const shortDescription = computed(() =>
  props.product.description ? props.product.description.slice(0, 120) + (props.product.description.length > 120 ? "..." : "") : null,
);

// Derive: Category names from classifications.
const categories = computed(() =>
  props.product.classifications?.map((c) => c.name).join(", ") ?? null,
);

// Wishlist: Variant-level membership is tracked by the wishlist store.
const isWishlisted = computed(() =>
  wishlistStore.wishlistedVariantIds.has(props.product.masterVariantId),
);
const wishlistLabel = computed(() =>
  isWishlisted.value ? "Remove from wishlist" : "Add to wishlist",
);

// Toggle: Add/remove the master variant on the default wishlist, then emit for analytics.
async function toggleWishlist(): Promise<void> {
  const variantId = props.product.masterVariantId;
  const detail = Object.values(wishlistStore.details).find((d) => d.isDefault);
  if (variantId && detail) {
    if (isWishlisted.value) {
      const item = detail.wishedItems.find((i) => i.variantId === variantId);
      if (item) await wishlistStore.removeItem(detail.id, item.id);
    } else {
      await wishlistStore.addItem(detail.id, { variantId, quantity: 1 });
    }
  }
  emit("toggle-wishlist", props.product);
}

// Guard: Stop card navigation, then quick-add the master variant and emit for analytics.
function onAddToCart(event: Event): void {
  event.stopPropagation();
  void quickAdd(props.product.masterVariantId);
  emit("add-to-cart", props.product);
}

// Guard: Stop card navigation, then toggle wishlist membership.
function onToggleWishlist(event: Event): void {
  event.stopPropagation();
  void toggleWishlist();
}

// Guard: Stop card navigation before opening the quick-view popover.
function onQuickView(event: Event): void {
  event.stopPropagation();
  quickViewPopover.value?.toggle(event, quickViewButton.value?.$el);
}

// Menu: Show the per-card context menu at the pointer position.
function onContextMenu(event: Event): void {
  event.stopPropagation();
  quickViewMenu.value?.show(event);
}

// Menu: Context actions mirror the card's footer buttons.
const menuItems = computed<MenuItem[]>(() => [
  {
    label: "Quick view",
    icon: "pi pi-eye",
    command: ({ originalEvent }) =>
      quickViewPopover.value?.show(originalEvent, quickViewButton.value?.$el),
  },
  {
    label: "Add to cart",
    icon: "pi pi-shopping-cart",
    command: ({ originalEvent }) => onAddToCart(originalEvent),
  },
  {
    label: "Wishlist",
    icon: isWishlisted.value ? "pi pi-heart-fill" : "pi pi-heart",
    command: () => void toggleWishlist(),
  },
]);
</script>

<template>
  <!-- Section: Product List Item — horizontal card with image, details, and actions -->
  <div
    class="group flex gap-4 rounded-lg border border-surface-200 bg-surface-0 p-4 transition-colors hover:border-surface-300"
    @contextmenu="onContextMenu"
  >
    <!-- Image: Fixed-width thumbnail with sale badge -->
    <RouterLink :to="`/products/${product.id}`" class="relative shrink-0">
      <Image
        v-if="imageUrl"
        :src="imageUrl"
        :alt="imageAlt"
        imageClass="h-32 w-32 rounded-lg object-cover sm:h-40 sm:w-40"
        preview
      />
      <div v-else class="flex h-32 w-32 items-center justify-center rounded-lg bg-surface-100 sm:h-40 sm:w-40">
        <i class="pi pi-image text-3xl text-placeholder" />
      </div>
      <Tag v-if="isOnSale" value="Sale" severity="danger" class="absolute left-2 top-2" />
      <Tag
        v-if="showSimilarity"
        :value="similarityPercent"
        severity="info"
        class="absolute right-2 top-2"
      />
    </RouterLink>

    <!-- Details: Title, description, categories, price, rating -->
    <div class="flex min-w-0 flex-1 flex-col justify-between">
      <div class="flex flex-col gap-1">
        <RouterLink :to="`/products/${product.id}`" class="text-base font-semibold text-heading line-clamp-1 hover:text-brand">
          {{ product.name }}
        </RouterLink>
        <p v-if="shortDescription" class="text-sm text-muted line-clamp-2">
          {{ shortDescription }}
        </p>
        <span v-if="categories" class="text-xs text-subtle">
          {{ categories }}
        </span>
      </div>

      <div class="mt-2 flex items-center gap-4">
        <!-- Price -->
        <div class="flex items-baseline gap-2">
          <span v-if="formattedPrice" class="text-lg font-bold text-brand">
            {{ formattedPrice }}
          </span>
          <span v-else class="text-lg font-bold text-subtle"> — </span>
          <span v-if="formattedCompareAt" class="text-sm text-muted line-through">
            {{ formattedCompareAt }}
          </span>
        </div>

        <!-- Rating -->
        <Rating v-if="ratingCount > 0" :modelValue="ratingAverage" readonly />
      </div>
    </div>

    <!-- Actions: Wishlist, add-to-cart, buy-now -->
    <div class="flex shrink-0 flex-col items-end justify-between gap-2">
      <Button
        :icon="isWishlisted ? 'pi pi-heart-fill' : 'pi pi-heart'"
        variant="text"
        severity="secondary"
        rounded
        :aria-label="wishlistLabel"
        v-tooltip.left="wishlistLabel"
        @click="onToggleWishlist"
      />
      <div class="flex flex-col gap-2">
        <Button
          label="Add to cart"
          icon="pi pi-shopping-cart"
          size="small"
          @click="onAddToCart"
        />
        <Button
          ref="quickViewButton"
          label="Quick view"
          icon="pi pi-eye"
          variant="text"
          size="small"
          @click="onQuickView"
        />
      </div>
    </div>
  </div>

  <!-- Section: Context Menu — right-click quick actions -->
  <ContextMenu ref="quickViewMenu" :model="menuItems" />

  <!-- Section: Quick View Popover — image, name, price and detail link -->
  <Popover ref="quickViewPopover">
    <div class="flex w-72 flex-col gap-3 p-2">
      <Image
        v-if="imageUrl"
        :src="imageUrl"
        :alt="imageAlt"
        imageClass="aspect-square w-full rounded-lg object-cover"
      />
      <div class="font-semibold">{{ product.name }}</div>
      <div class="flex items-baseline gap-2">
        <span v-if="formattedPrice" class="text-lg font-bold text-brand">
          {{ formattedPrice }}
        </span>
        <span v-if="formattedCompareAt" class="text-sm text-muted line-through">
          {{ formattedCompareAt }}
        </span>
      </div>
      <Button
        as="router-link"
        :to="`/products/${product.id}`"
        label="View details"
        size="small"
        class="w-full"
      />
    </div>
  </Popover>
</template>
