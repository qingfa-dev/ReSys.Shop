<script setup lang="ts">
import { ref, computed } from 'vue'
import Button from 'primevue/button'
import Image from 'primevue/image'
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
  (e: 'quickView', product: Product): void
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

function handleImageError() {
  imageError.value = true
}

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

function handleQuickView(e: Event) {
  e.stopPropagation()
  emit('quickView', props.product)
}
</script>

<template>
  <article class="product-card-grid">
    <div class="product-image-wrapper" @click="emit('click', product)">
      <div 
        class="product-image" 
        :style="primaryImage ? { backgroundImage: `url(${primaryImage})` } : {}"
        @error="handleImageError"
      >
        <span v-if="isOnSale" class="badge sale">Sale</span>
        <span v-if="product.tags?.includes('new')" class="badge new">New</span>
      </div>
      
      <div class="product-actions">
        <button 
          class="action-btn" 
          aria-label="Add to wishlist"
          @click.stop="handleAddToWishlist"
        >
          <i class="pi pi-heart"></i>
        </button>
        <button 
          v-if="showActions"
          class="action-btn" 
          aria-label="Quick view"
          @click.stop="handleQuickView"
        >
          <i class="pi pi-eye"></i>
        </button>
      </div>
    </div>

    <div class="product-content">
      <span class="product-category">{{ product.category?.name }}</span>
      <h3 class="product-name" @click="emit('click', product)">{{ product.name }}</h3>
      
      <div class="product-price">
        <span class="price">{{ formattedPrice }}</span>
        <span v-if="isOnSale" class="compare-price">{{ formattedComparePrice }}</span>
      </div>

      <ProductOptionPicker
        v-if="hasOptions"
        ref="optionPickerRef"
        :colors="colors"
        :sizes="sizes"
        :compact="true"
        @option-change="handleOptionChange"
      />

      <div v-if="showActions" class="product-buttons">
        <Button 
          label="Add to Cart" 
          size="small" 
          outlined 
          @click.stop="handleAddToCart"
        />
      </div>
    </div>
  </article>
</template>

<style scoped lang="scss">
.product-card-grid {
  display: flex;
  flex-direction: column;
  
  &:hover {
    .product-image-wrapper {
      box-shadow: var(--shadow-lg);
    }
    
    .product-actions {
      opacity: 1;
      transform: translateY(0);
    }

    .product-buttons {
      opacity: 1;
      transform: translateY(0);
    }
  }
}

.product-image-wrapper {
  position: relative;
  border-radius: var(--radius-lg);
  overflow: hidden;
  background: var(--color-surface);
  box-shadow: var(--shadow-sm);
  transition: box-shadow var(--transition-normal);
  cursor: pointer;
}

.product-image {
  aspect-ratio: 3/4;
  background-color: var(--color-surface-ground);
  background-size: cover;
  background-position: center;
  position: relative;
  
  &:empty::before {
    content: '';
    position: absolute;
    inset: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(135deg, #e7e5e4 0%, #d6d3d1 100%);
  }
}

.badge {
  position: absolute;
  top: 1rem;
  left: 1rem;
  padding: 0.25rem 0.75rem;
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

.product-actions {
  position: absolute;
  bottom: 1rem;
  right: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  opacity: 0;
  transform: translateY(10px);
  transition: all var(--transition-normal);
}

.action-btn {
  width: 40px;
  height: 40px;
  border: none;
  background: var(--color-surface);
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: var(--shadow-md);
  cursor: pointer;
  transition: all var(--transition-fast);
  
  &:hover {
    background: var(--color-primary);
    color: white;
  }
  
  i {
    font-size: var(--font-size-base);
  }
}

.product-content {
  padding: 1rem 0;
}

.product-category {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.product-name {
  font-family: var(--font-body);
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-medium);
  margin: 0.25rem 0 0.5rem;
  color: var(--color-text);
  line-height: var(--line-height-tight);
  cursor: pointer;
  
  &:hover {
    color: var(--color-primary);
  }
}

.product-price {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  
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

.product-buttons {
  margin-top: 1rem;
  opacity: 0;
  transform: translateY(10px);
  transition: all var(--transition-normal);
}
</style>
