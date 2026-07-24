<script setup lang="ts">
import AccountHeader from '@/features/identity/components/AccountHeader.vue'
import AccountNav from '@/features/identity/components/AccountNav.vue'
</script>

<template>
  <div class="account-view">
    <AccountHeader />
    
    <AccountNav />
    
    <div class="account-content">
      <RouterView />
    </div>
  </div>
</template>

<style scoped lang="scss">
.account-view {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}
</style>