<template>
  <div class="recommendation-carousel">
    <div class="carousel-header">
      <h3>{{ title }}</h3>
      <div class="carousel-controls">
        <button @click="scrollPrev" class="scroll-btn" :disabled="currentIndex === 0">❮</button>
        <button @click="scrollNext" class="scroll-btn" :disabled="currentIndex >= maxScroll">
          ❯
        </button>
      </div>
    </div>

    <div class="carousel-container" ref="container">
      <div
        class="carousel-track"
        :style="{ transform: `translateX(-${currentIndex * itemWidth}px)` }"
      >
        <div
          v-for="product in products"
          :key="product.id"
          class="carousel-item"
          @click="selectProduct(product)"
        >
          <div class="product-image">
            <img :src="product.image" :alt="product.name" />
            <div v-if="product.badge" class="product-badge">{{ product.badge }}</div>
            <div class="score-badge">{{ (product.score * 100).toFixed(0) }}% match</div>
          </div>
          <div class="product-info">
            <p class="product-name">{{ product.name }}</p>
            <p class="product-price">${{ product.price.toFixed(2) }}</p>
            <p class="product-reason">{{ product.reason }}</p>
            <button class="add-btn" @click.stop="addToCart(product)">Add to Cart</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";

interface Props {
  title?: string;
  products: any[];
  itemWidth?: number;
}

const props = withDefaults(defineProps<Props>(), {
  title: "Recommended For You",
  itemWidth: 200,
});

const emit = defineEmits<{
  productSelect: [product: any];
  addToCart: [product: any];
}>();

const container = ref<HTMLElement>();
const currentIndex = ref(0);

const maxScroll = computed(() => Math.max(0, Math.ceil(props.products.length - 5)));

function scrollPrev() {
  currentIndex.value = Math.max(0, currentIndex.value - 1);
}

function scrollNext() {
  currentIndex.value = Math.min(maxScroll.value, currentIndex.value + 1);
}

function selectProduct(product: any) {
  emit("productSelect", product);
}

function addToCart(product: any) {
  emit("addToCart", product);
}
</script>

<style scoped lang="scss">
.recommendation-carousel {
  margin: 2rem 0;

  .carousel-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;

    h3 {
      font-size: var(--font-size-xl);
      margin: 0;
    }

    .carousel-controls {
      display: flex;
      gap: 0.5rem;

      .scroll-btn {
        width: 36px;
        height: 36px;
        border: 1px solid var(--color-border);
        background: var(--color-surface);
        border-radius: 50%;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: all var(--transition-fast);

        &:hover:not(:disabled) {
          border-color: var(--color-primary);
          color: var(--color-primary);
        }

        &:disabled {
          opacity: 0.5;
          cursor: not-allowed;
        }
      }
    }
  }

  .carousel-container {
    overflow: hidden;
    border-radius: var(--radius-lg);
  }

  .carousel-track {
    display: flex;
    gap: 1rem;
    transition: transform 0.3s ease;
  }

  .carousel-item {
    flex: 0 0 200px;
    cursor: pointer;
    border-radius: var(--radius-lg);
    overflow: hidden;
    background: var(--color-surface);
    border: 1px solid var(--color-border-light);
    transition: all var(--transition-fast);

    &:hover {
      border-color: var(--color-primary);
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);

      .add-btn {
        background: var(--color-primary);
        color: white;
      }
    }

    .product-image {
      position: relative;
      width: 100%;
      height: 200px;
      background: #f5f5f5;
      overflow: hidden;

      img {
        width: 100%;
        height: 100%;
        object-fit: cover;
      }

      .product-badge {
        position: absolute;
        top: 0.5rem;
        right: 0.5rem;
        background: var(--color-primary);
        color: white;
        padding: 0.25rem 0.75rem;
        border-radius: var(--radius-sm);
        font-size: 0.75rem;
        font-weight: var(--font-weight-medium);
      }

      .score-badge {
        position: absolute;
        bottom: 0.5rem;
        left: 0.5rem;
        background: rgba(0, 0, 0, 0.7);
        color: white;
        padding: 0.25rem 0.75rem;
        border-radius: var(--radius-sm);
        font-size: 0.75rem;
      }
    }

    .product-info {
      padding: 1rem;

      .product-name {
        font-size: var(--font-size-sm);
        font-weight: var(--font-weight-medium);
        margin: 0 0 0.5rem;
        line-height: 1.3;
      }

      .product-price {
        font-size: var(--font-size-base);
        font-weight: var(--font-weight-medium);
        color: var(--color-primary);
        margin: 0 0 0.5rem;
      }

      .product-reason {
        font-size: 0.75rem;
        color: var(--color-text-secondary);
        margin: 0 0 0.75rem;
      }

      .add-btn {
        width: 100%;
        padding: 0.5rem;
        border: 1px solid var(--color-border);
        background: var(--color-surface);
        border-radius: var(--radius-md);
        cursor: pointer;
        font-size: var(--font-size-sm);
        transition: all var(--transition-fast);

        &:hover {
          background: var(--color-primary);
          color: white;
          border-color: var(--color-primary);
        }
      }
    }
  }
}
</style>
