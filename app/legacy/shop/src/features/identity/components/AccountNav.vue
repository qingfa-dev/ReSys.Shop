<script setup lang="ts">
defineProps<{
  items?: Array<{ to: string; icon: string; label: string }>
}>()

const defaultItems = [
  { to: '/account/orders', icon: 'pi pi-box', label: 'My Orders' },
  { to: '/account/addresses', icon: 'pi pi-map-marker', label: 'Addresses' },
  { to: '/account/settings', icon: 'pi pi-cog', label: 'Settings' },
]

const navItems = defaultItems
</script>

<template>
  <div class="account-nav">
    <RouterLink
      v-for="item in (items || navItems)"
      :key="item.to"
      :to="item.to"
      class="nav-item"
    >
      <i :class="item.icon"></i>
      <span>{{ item.label }}</span>
    </RouterLink>
  </div>
</template>

<style scoped lang="scss">
.account-nav {
  display: flex;
  gap: 1rem;
  margin-bottom: 2rem;
  border-bottom: 1px solid var(--color-border-light);
  padding-bottom: 1rem;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  border-radius: var(--radius-md);
  color: var(--color-text-secondary);
  text-decoration: none;
  transition: all var(--transition-fast);

  &:hover {
    background: var(--color-surface-ground);
    color: var(--color-text);
  }

  &.router-link-active {
    background: var(--color-primary);
    color: white;
  }

  i {
    font-size: var(--font-size-lg);
  }
}
</style>