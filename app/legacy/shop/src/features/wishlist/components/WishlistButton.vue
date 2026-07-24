<template>
  <button
    class="wishlist-button"
    :class="{ 'is-wishlisted': isWish }"
    @click="toggleWishlist"
    :title="isWish ? 'Remove from wishlist' : 'Add to wishlist'"
  >
    <i class="pi" :class="isWish ? 'pi-heart-fill' : 'pi-heart'"></i>
  </button>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useWishlist } from "../composables/useWishlist";

interface Props {
  productId: string;
  product?: any;
}

const props = defineProps<Props>();

const { isWishlisted, addToWishlist, removeFromWishlist } = useWishlist();

const isWish = computed(() => isWishlisted(props.productId));

function toggleWishlist() {
  if (isWish.value) {
    removeFromWishlist(props.productId);
  } else if (props.product) {
    addToWishlist(props.product);
  }
}
</script>

<style scoped lang="scss">
.wishlist-button {
  width: 44px;
  height: 44px;
  border: 2px solid var(--color-border);
  background: var(--color-surface);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all var(--transition-fast);
  color: var(--color-text-secondary);

  &:hover:not(.is-wishlisted) {
    border-color: var(--color-primary);
    color: var(--color-primary);
  }

  &.is-wishlisted {
    background: var(--color-primary);
    border-color: var(--color-primary);
    color: white;
  }

  i {
    font-size: 1.25rem;
  }
}
</style>
