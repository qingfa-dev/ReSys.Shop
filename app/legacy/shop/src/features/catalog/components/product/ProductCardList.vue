<script setup lang="ts">
import { ref, computed } from 'vue'
import Button from 'primevue/button'
import type { Product, ProductColor, ProductSize } from '@/features/catalog/types'
import ProductOptionPicker from './ProductOptionPicker.vue'

const DEFAULT_CURRENCY = 'USD'
const DEFAULT_IMAGE = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="200" height="200" viewBox="0 0 200 200"%3E%3Crect fill="%23e7e5e4" width="200" height="200"/%3E%3Ctext fill="%239ca3af" font-family="sans-serif" font-size="14" x="50%25" y="50%25" text-anchor="middle" dy=".3em"%3ENo Image%3C/text%3E%3C/svg%3E'

interface Props {
  product: Product
  showActions?: boolean
  colors?: ProductColor[]
  sizes?: ProductSize[]
  currency?: string
}

const props = withDefaults(defineProps<Props>(), {
  showActions: true,
  currency: DEFAULT_CURRENCY,
})

const imageError = ref(false)

const emit = defineEmits<{
  (e: 'addToCart', product: Product, colorId?: string, sizeId?: string): void
  (e: 'addToWishlist', product: Product): void
  (e: 'click', product: Product): void
}>()

const selectedColorId = ref<string>('')
const selectedSizeId = ref<string>('')

const formattedPrice = computed(() => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: props.currency,
  }).format(props.product.price)
})

const formattedComparePrice = computed(() => {
  if (!props.product.compareAtPrice) return null
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: props.currency,
  }).format(props.product.compareAtPrice)
})

const isOnSale = computed(() => {
  return props.product.compareAtPrice && props.product.compareAtPrice > props.product.price
})

const primaryImage = computed(() => {
  if (imageError.value) return DEFAULT_IMAGE
  const firstImage = props.product.images?.[0]
  if (!firstImage) return DEFAULT_IMAGE
  if (typeof firstImage === 'string') return firstImage
  return firstImage.url || DEFAULT_IMAGE
})

const hasOptions = computed(() => {
  return (props.colors && props.colors.length > 0) || (props.sizes && props.sizes.length > 0)
})

function handleOptionChange(type: 'color' | 'size', value: string) {
  if (type === 'color') {
    selectedColorId.value = value
  } else {
    selectedSizeId.value = value
  }
}

function handleAddToCart(e: Event) {
  e.stopPropagation()
  emit('addToCart', props.product, selectedColorId.value || undefined, selectedSizeId.value || undefined)
}

function handleAddToWishlist(e: Event) {
  e.stopPropagation()
  emit('addToWishlist', props.product)
}

function handleImageError() {
  imageError.value = true
}
</script>

<template>
  <article class="product-card-list">
    <div class="product-image-wrapper" @click="emit('click', product)">
      <div 
        class="product-image" 
        :style="primaryImage ? { backgroundImage: `url(${primaryImage})` } : {}"
        @error="handleImageError"
      >
        <span v-if="isOnSale" class="badge sale">Sale</span>
        <span v-if="product.tags?.includes('new')" class="badge new">New</span>
      </div>
    </div>

    <div class="product-info" @click="emit('click', product)">
      <h3 class="product-name">{{ product.name }}</h3>
      <span class="product-category">{{ product.category?.name }}</span>
    </div>

    <div class="product-price-section">
      <div class="product-price">
        <span class="price">{{ formattedPrice }}</span>
        <span v-if="isOnSale" class="compare-price">{{ formattedComparePrice }}</span>
      </div>
    </div>

    <div class="product-options">
      <ProductOptionPicker
        v-if="hasOptions"
        ref="optionPickerRef"
        :colors="colors"
        :sizes="sizes"
        :compact="true"
        @option-change="handleOptionChange"
      />
    </div>

    <div class="product-actions-section">
      <div v-if="showActions" class="product-buttons">
        <Button 
          label="Add to Cart" 
          size="small" 
          outlined 
          @click.stop="handleAddToCart"
        />
        <button class="wishlist-btn" @click.stop="handleAddToWishlist">
          <i class="pi pi-heart"></i>
        </button>
      </div>
    </div>
  </article>
</template>

<style scoped lang="scss">
.product-card-list {
  display: grid;
  grid-template-columns: 160px 1fr auto auto auto;
  gap: 1.5rem;
  align-items: center;
  padding: 1.25rem;
  background: var(--color-surface);
  border-radius: var(--radius-lg);
  transition: box-shadow var(--transition-fast);
  
  &:hover {
    box-shadow: var(--shadow-md);
  }

  @media (max-width: 1024px) {
    grid-template-columns: 120px 1fr auto;
    
    .product-options,
    .product-category {
      display: none;
    }
  }

  @media (max-width: 640px) {
    grid-template-columns: 100px 1fr;
    
    .product-price-section,
    .product-actions-section {
      grid-column: 2;
    }
  }
}

.product-image-wrapper {
  position: relative;
  border-radius: var(--radius-md);
  overflow: hidden;
  background: var(--color-surface-ground);
  cursor: pointer;
}

.product-image {
  aspect-ratio: 3/4;
  background-size: cover;
  background-position: center;
  position: relative;
  
  &:empty::before {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(135deg, #e7e5e4 0%, #d6d3d1 100%);
  }
}

.badge {
  position: absolute;
  top: 0.5rem;
  left: 0.5rem;
  padding: 0.2rem 0.5rem;
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-semibold);
  text-transform: uppercase;
  border-radius: var(--radius-full);
  
  &.sale {
    background: var(--color-danger);
    color: white;
  }
  
  &.new {
    background: var(--color-primary);
    color: white;
  }
}

.product-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  min-width: 180px;
  cursor: pointer;
  
  &:hover .product-name {
    color: var(--color-primary);
  }
}

.product-name {
  font-family: var(--font-body);
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-medium);
  color: var(--color-text);
  line-height: var(--line-height-tight);
  margin: 0;
}

.product-category {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.product-price-section {
  min-width: 100px;
}

.product-price {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  
  .price {
    font-size: var(--font-size-lg);
    font-weight: var(--font-weight-semibold);
    color: var(--color-primary);
  }
  
  .compare-price {
    font-size: var(--font-size-sm);
    color: var(--color-text-muted);
    text-decoration: line-through;
  }
}

.product-options {
  min-width: 150px;
}

.product-actions-section {
  min-width: 140px;
}

.product-buttons {
  display: flex;
  gap: 0.5rem;
  align-items: center;

  .wishlist-btn {
    width: 36px;
    height: 36px;
    border: 1px solid var(--color-border);
    background: var(--color-surface);
    border-radius: var(--radius-md);
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: all var(--transition-fast);

    &:hover {
      border-color: var(--color-primary);
      color: var(--color-primary);
    }
  }
}
</style>
