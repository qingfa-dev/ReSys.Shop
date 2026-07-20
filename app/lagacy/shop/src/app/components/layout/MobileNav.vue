<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { useNavigation } from '@/app/composables'

defineProps<{
  isOpen: boolean
}>()

const emit = defineEmits<{
  close: []
}>()

const { navLinks, isActive } = useNavigation()

function handleLinkClick() {
  emit('close')
}
</script>

<template>
  <transition name="slide-down">
    <div v-if="isOpen" class="mobile-menu">
      <nav class="mobile-nav">
        <RouterLink 
          v-for="link in navLinks" 
          :key="link.path"
          :to="link.path"
          class="mobile-nav-link"
          :class="{ 'router-link-active': isActive(link.path) }"
          @click="handleLinkClick"
        >
          {{ link.name }}
        </RouterLink>
      </nav>
    </div>
  </transition>
</template>

<style scoped lang="scss">
.mobile-menu {
  background: var(--color-surface);
  border-top: 1px solid var(--color-border-light);
  padding: 1rem 2rem;
}

.mobile-nav {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.mobile-nav-link {
  padding: 0.75rem 0;
  font-size: var(--font-size-lg);
  color: var(--color-text);
  text-decoration: none;
  border-bottom: 1px solid var(--color-border-light);
  transition: color var(--transition-fast);
  
  &:last-child {
    border-bottom: none;
  }
  
  &:hover,
  &.router-link-active {
    color: var(--color-primary);
  }
}

.slide-down-enter-active,
.slide-down-leave-active {
  transition: all var(--transition-normal);
}

.slide-down-enter-from,
.slide-down-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>
