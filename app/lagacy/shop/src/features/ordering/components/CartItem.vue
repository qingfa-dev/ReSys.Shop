<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import InputNumber from 'primevue/inputnumber'
import type { CartItem } from '@/features/ordering/types'

const router = useRouter()

interface Props {
  item: CartItem
  readonly?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  readonly: false,
})

const emit = defineEmits<{
  (e: 'updateQuantity', quantity: number): void
  (e: 'remove'): void
}>()

const formattedPrice = computed(() => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(props.item.price)
})

const lineTotal = computed(() => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(props.item.price * props.item.quantity)
})

function handleQuantityChange(value: number | null) {
  if (value && value > 0) {
    emit('updateQuantity', value)
  }
}

function handleRemove() {
  emit('remove')
}

function handleProductClick() {
  router.push(`/product/${props.item.productId}`)
}
</script>

<template>
  <div class="cart-item">
    <div class="item-image" @click="handleProductClick">
      <img v-if="item.productImage" :src="item.productImage" :alt="item.productName" />
      <div v-else class="image-placeholder"></div>
    </div>
    
    <div class="item-details">
      <div class="item-header">
        <div>
          <h4 class="item-name" @click="handleProductClick">{{ item.productName }}</h4>
          <span v-if="item.variantName" class="item-variant">{{ item.variantName }}</span>
        </div>
        
        <button 
          v-if="!readonly" 
          class="remove-btn" 
          @click="handleRemove"
          aria-label="Remove item"
        >
          <i class="pi pi-times"></i>
        </button>
      </div>
      
      <div class="item-footer">
        <div class="quantity-control">
          <InputNumber
            :modelValue="item.quantity"
            :min="1"
            :max="99"
            :readonly="readonly"
            showButtons
            buttonLayout="horizontal"
            :step="1"
            @update:modelValue="handleQuantityChange"
            class="qty-input"
          >
            <template #decrementicon>
              <i class="pi pi-minus"></i>
            </template>
            <template #incrementicon>
              <i class="pi pi-plus"></i>
            </template>
          </InputNumber>
        </div>
        
        <div class="item-price">
          <span class="unit-price">{{ formattedPrice }} each</span>
          <span class="line-total">{{ lineTotal }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.cart-item {
  display: flex;
  gap: 1rem;
  padding: 1.5rem 0;
  border-bottom: 1px solid var(--color-border-light);
  
  &:first-child {
    padding-top: 0;
  }
  
  &:last-child {
    border-bottom: none;
  }
}

.item-image {
  width: 100px;
  height: 130px;
  border-radius: var(--radius-md);
  overflow: hidden;
  flex-shrink: 0;
  cursor: pointer;
  
  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }
  
  .image-placeholder {
    width: 100%;
    height: 100%;
    background: var(--color-surface-ground);
  }
}

.item-details {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.item-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.item-name {
  font-family: var(--font-body);
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-medium);
  color: var(--color-text);
  cursor: pointer;
  margin-bottom: 0.25rem;
  
  &:hover {
    color: var(--color-primary);
  }
}

.item-variant {
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
}

.remove-btn {
  width: 32px;
  height: 32px;
  border: none;
  background: transparent;
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-muted);
  cursor: pointer;
  transition: all var(--transition-fast);
  
  &:hover {
    background: var(--color-surface-ground);
    color: var(--color-danger);
  }
}

.item-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
}

.quantity-control {
  :deep(.p-inputnumber) {
    width: 120px;
  }
  
  :deep(.p-inputnumber-input) {
    width: 40px;
    text-align: center;
    border-color: var(--color-border);
  }
  
  :deep(.p-button) {
    width: 32px;
    height: 32px;
    background: var(--color-surface);
    border-color: var(--color-border);
    color: var(--color-text);
    
    &:hover {
      background: var(--color-surface-ground);
      border-color: var(--color-primary);
    }
  }
}

.item-price {
  text-align: right;
  
  .unit-price {
    display: block;
    font-size: var(--font-size-sm);
    color: var(--color-text-muted);
    margin-bottom: 0.25rem;
  }
  
  .line-total {
    font-size: var(--font-size-lg);
    font-weight: var(--font-weight-semibold);
    color: var(--color-text);
  }
}

@media (max-width: 480px) {
  .item-image {
    width: 80px;
    height: 100px;
  }
  
  .item-footer {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.75rem;
  }
  
  .item-price {
    text-align: left;
    display: flex;
    gap: 1rem;
  }
}
</style>
