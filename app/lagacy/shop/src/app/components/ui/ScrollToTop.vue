<script setup lang="ts">
import { useScroll } from '@/app/composables'

const { showScrollTop, scrollToTop } = useScroll()
</script>

<template>
  <transition name="fade">
    <button 
      v-if="showScrollTop" 
      class="scroll-to-top" 
      @click="scrollToTop"
      aria-label="Scroll to top"
    >
      <i class="pi pi-arrow-up"></i>
    </button>
  </transition>
</template>

<style scoped lang="scss">
.scroll-to-top {
  position: fixed;
  bottom: 2rem;
  right: 2rem;
  width: 48px;
  height: 48px;
  border: none;
  background: var(--color-primary);
  color: white;
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: var(--shadow-lg);
  cursor: pointer;
  z-index: var(--z-fixed);
  transition: all var(--transition-fast);
  
  &:hover {
    transform: translateY(-4px);
    box-shadow: var(--shadow-xl);
  }
  
  &:active {
    transform: translateY(-2px);
  }
  
  i {
    font-size: var(--font-size-lg);
  }
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
