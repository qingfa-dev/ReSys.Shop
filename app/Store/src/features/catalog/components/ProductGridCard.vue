<script setup lang="ts">
import { computed, ref } from "vue";
import type { ComponentPublicInstance } from "vue";
import { RouterLink } from "vue-router";
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
  <!-- Section: Product Card — router-linked card with quick actions and context menu -->
  <RouterLink :to="`/products/${product.id}`" class="group block" @contextmenu="onContextMenu">
    <Card class="overflow-hidden">
      <template #header>
        <div class="relative">
          <Image
            v-if="imageUrl"
            :src="imageUrl"
            :alt="imageAlt"
            imageClass="aspect-square w-full object-cover"
            preview
          />
          <div v-else class="flex aspect-square items-center justify-center bg-surface-100">
            <i class="pi pi-image text-3xl text-placeholder" />
          </div>
          <Tag v-if="isOnSale" value="Sale" severity="danger" class="absolute left-2 top-2" />
          <Tag
            v-if="showSimilarity"
            :value="similarityPercent"
            severity="info"
            class="absolute right-2 top-2"
          />
        </div>
      </template>

      <template #title>
        <span class="line-clamp-2 text-sm font-semibold">{{ product.name }}</span>
      </template>

      <template #content>
        <Rating v-if="ratingCount > 0" :modelValue="ratingAverage" readonly />
        <div class="mt-1 flex items-baseline gap-2">
          <span v-if="formattedPrice" class="text-lg font-bold text-brand">
            {{ formattedPrice }}
          </span>
          <span v-else class="text-lg font-bold text-subtle"> — </span>
          <span v-if="formattedCompareAt" class="text-sm text-muted line-through">
            {{ formattedCompareAt }}
          </span>
        </div>
      </template>

      <template #footer>
        <div class="flex items-center justify-between gap-2">
          <Button
            :icon="isWishlisted ? 'pi pi-heart-fill' : 'pi pi-heart'"
            variant="text"
            :severity="isWishlisted ? 'danger' : 'contrast'"
            rounded
            class="text-muted! hover:text-brand!"
            :aria-label="wishlistLabel"
            v-tooltip.bottom="wishlistLabel"
            @click="onToggleWishlist"
          />
          <div class="flex items-center gap-1">
            <Button
              label="Add to cart"
              icon="pi pi-shopping-cart"
              severity="success"
              size="small"
              @click="onAddToCart"
            />
            <Button
              ref="quickViewButton"
              icon="pi pi-eye"
              variant="text"
              severity="contrast"
              rounded
              class="text-muted! hover:text-brand!"
              aria-label="Quick view"
              v-tooltip.bottom="'Quick view'"
              @click="onQuickView"
            />
          </div>
        </div>
      </template>
    </Card>
  </RouterLink>

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
