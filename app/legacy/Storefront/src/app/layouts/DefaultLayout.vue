<script setup lang="ts">
import { onMounted } from "vue";
import { RouterView } from "vue-router";
import { AppHeader, AppFooter } from "@/app/components/layout";
import ScrollToTop from "@/app/components/ui/ScrollToTop.vue";
import { useUIStore } from "@/app/stores";

const uiStore = useUIStore();

onMounted(() => {
  uiStore.hydrate();
});
</script>

<template>
  <div class="app-wrapper">
    <AppHeader />

    <main class="app-main">
      <RouterView v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </RouterView>
    </main>

    <AppFooter />

    <ScrollToTop />
  </div>
</template>

<style scoped lang="scss">
.app-wrapper {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.app-main {
  flex: 1;
  padding-top: 80px;
}
</style>

<style lang="scss">
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
