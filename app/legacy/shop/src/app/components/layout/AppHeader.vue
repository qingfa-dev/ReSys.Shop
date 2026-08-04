<script setup lang="ts">
import { ref, onMounted, onUnmounted } from "vue";
import { RouterLink, useRouter } from "vue-router";
import { useNavigation } from "@/app/composables";
import { usePreferencesStore, useUIStore } from "@/app/stores";
import MobileNav from "./MobileNav.vue";
import NotificationBell from "@/features/notifications/components/NotificationBell.vue";
import SearchOverlay from "@/features/search/components/SearchOverlay.vue";

const preferencesStore = usePreferencesStore();
const uiStore = useUIStore();
const router = useRouter();
const { navLinks, isActive } = useNavigation();

const isScrolled = ref(false);
const searchOverlayVisible = ref(false);

function openSearch() {
  searchOverlayVisible.value = true;
}

function goToRecommendations() {
  router.push("/recommendations");
}

function handleScroll() {
  isScrolled.value = window.scrollY > 20;
}

onMounted(() => {
  window.addEventListener("scroll", handleScroll, { passive: true });
});

onUnmounted(() => {
  window.removeEventListener("scroll", handleScroll);
});
</script>

<template>
  <header class="app-header" :class="{ scrolled: isScrolled }">
    <div class="header-container">
      <RouterLink to="/" class="logo">
        <span class="logo-text">ReSys</span>
        <span class="logo-dot">.</span>
        <span class="logo-text">Shop</span>
      </RouterLink>

      <nav class="nav-desktop">
        <RouterLink
          v-for="link in navLinks"
          :key="link.path"
          :to="link.path"
          class="nav-link"
          :class="{ 'router-link-active': isActive(link.path) }"
        >
          {{ link.name }}
        </RouterLink>
      </nav>

      <div class="header-actions">
        <button class="action-btn search-btn" aria-label="Search" @click="openSearch">
          <i class="pi pi-search"></i>
        </button>

        <button
          class="action-btn image-search-btn"
          aria-label="Image Search"
          @click="goToRecommendations"
        >
          <i class="pi pi-camera"></i>
        </button>

        <button
          class="action-btn theme-btn"
          aria-label="Toggle theme"
          @click="preferencesStore.toggleTheme"
        >
          <i :class="preferencesStore.isDark ? 'pi pi-sun' : 'pi pi-moon'"></i>
        </button>

        <NotificationBell />

        <RouterLink to="/cart" class="action-btn cart-btn" aria-label="Cart">
          <i class="pi pi-shopping-cart"></i>
          <span class="cart-badge">0</span>
        </RouterLink>

        <RouterLink to="/account" class="action-btn user-btn" aria-label="Account">
          <i class="pi pi-user"></i>
        </RouterLink>

        <button class="mobile-menu-btn" aria-label="Menu" @click="uiStore.toggleMobileMenu">
          <i :class="uiStore.mobileMenuOpen ? 'pi pi-times' : 'pi pi-bars'"></i>
        </button>
      </div>
    </div>

    <MobileNav :is-open="uiStore.mobileMenuOpen" @close="uiStore.closeMobileMenu" />

    <SearchOverlay v-model:visible="searchOverlayVisible" />
  </header>
</template>

<style scoped lang="scss">
.app-header {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: var(--z-fixed);
  background: var(--color-surface);
  border-bottom: 1px solid var(--color-border-light);
  transition: all var(--transition-normal);
}

.header-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem 2rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 2rem;
}

.logo {
  font-family: var(--font-display);
  font-size: var(--font-size-xl);
  font-weight: var(--font-weight-bold);
  text-decoration: none;
  display: flex;
  align-items: center;

  .logo-text {
    color: var(--color-text);
  }

  .logo-dot {
    color: var(--color-primary);
    margin: 0 2px;
  }
}

.nav-desktop {
  display: flex;
  align-items: center;
  gap: 2rem;

  @media (max-width: 768px) {
    display: none;
  }
}

.nav-link {
  font-family: var(--font-body);
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-secondary);
  text-decoration: none;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  transition: color var(--transition-fast);
  position: relative;

  &::after {
    content: "";
    position: absolute;
    bottom: -4px;
    left: 0;
    width: 0;
    height: 2px;
    background: var(--color-primary);
    transition: width var(--transition-normal);
  }

  &:hover,
  &.router-link-active {
    color: var(--color-text);

    &::after {
      width: 100%;
    }
  }
}

.app-megamenu {
  background: transparent;
  border: none;
  width: 100%;
  gap: 0.5rem;

  :deep(.p-megamenu-root-list) {
    gap: 0.5rem;
  }

  :deep(.p-menuitem) {
    margin: 0;
  }

  :deep(.p-menuitem-link) {
    padding: 0.5rem 1rem;
    font-family: var(--font-body);
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    color: var(--color-text-secondary);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    border-radius: var(--radius-md);
    transition: all var(--transition-fast);

    &:hover {
      background: var(--color-surface-ground);
      color: var(--color-text);
    }

    .p-menuitem-icon {
      margin-right: 0.5rem;
    }
  }

  :deep(.p-megamenu-panel) {
    background: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-lg);
    box-shadow: var(--shadow-lg);
    margin-top: 0.5rem;
  }

  :deep(.p-megamenu-submenu-label) {
    font-family: var(--font-body);
    font-size: var(--font-size-xs);
    font-weight: var(--font-weight-semibold);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: var(--color-text);
    padding: 0.75rem 1rem 0.5rem;
    border-bottom: 1px solid var(--color-border-light);
    margin-bottom: 0.5rem;
  }

  :deep(.p-megamenu-submenu) {
    padding: 0.5rem 0;
  }

  :deep(.p-megamenu-submenu .p-menuitem-link) {
    padding: 0.5rem 1rem;
    font-size: var(--font-size-sm);
    color: var(--color-text-secondary);
    text-transform: none;
    letter-spacing: normal;

    &:hover {
      background: var(--color-surface-ground);
      color: var(--color-text);
    }

    .p-menuitem-icon {
      font-size: var(--font-size-xs);
      margin-right: 0.5rem;
      color: var(--color-primary);
    }
  }
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.action-btn {
  width: 40px;
  height: 40px;
  border: none;
  background: transparent;
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-secondary);
  transition: all var(--transition-fast);
  position: relative;
  text-decoration: none;

  &:hover {
    background: var(--color-surface-ground);
    color: var(--color-text);
  }

  i {
    font-size: var(--font-size-lg);
  }
}

.cart-badge {
  position: absolute;
  top: 2px;
  right: 2px;
  min-width: 18px;
  height: 18px;
  padding: 0 4px;
  background: var(--color-primary);
  color: white;
  font-size: 10px;
  font-weight: var(--font-weight-bold);
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
}

.mobile-menu-btn {
  display: none;

  @media (max-width: 768px) {
    display: flex;
  }

  width: 40px;
  height: 40px;
  border: none;
  background: transparent;
  border-radius: var(--radius-md);
  align-items: center;
  justify-content: center;
  color: var(--color-text);

  i {
    font-size: var(--font-size-lg);
  }
}

.mega-menu-container {
  position: fixed;
  top: 73px;
  left: 0;
  right: 0;
  z-index: 98;
  background: var(--color-surface);
  border-bottom: 1px solid var(--color-border-light);
  padding: 0 2rem;
  max-width: 1400px;
  margin: 0 auto;
}

.app-megamenu {
  background: transparent;
  border: none;
  width: 100%;
  gap: 0;

  :deep(.p-megamenu-root-list) {
    gap: 0;
    justify-content: flex-start;
  }

  :deep(.p-menuitem) {
    margin: 0;
  }

  :deep(.p-menuitem-content) {
    border-radius: 0;
  }

  :deep(.p-menuitem-link) {
    padding: 0.75rem 1rem;
    font-family: var(--font-body);
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    color: var(--color-text-secondary);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    border-radius: 0;
    transition: all var(--transition-fast);

    &:hover {
      background: transparent;
      color: var(--color-text);
    }

    .p-menuitem-icon {
      margin-right: 0.5rem;
    }
  }

  :deep(.p-menuitem-active > .p-menuitem-link) {
    background: transparent;
    color: var(--color-text);
    box-shadow: inset 0 -2px 0 var(--color-primary);
  }

  :deep(.p-megamenu-panel) {
    background: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-lg);
    box-shadow: var(--shadow-lg);
    margin-top: 0.5rem;
    left: 50%;
    transform: translateX(-50%);
    max-width: 1400px;
    width: 100%;
  }

  :deep(.p-megamenu-submenu-label) {
    font-family: var(--font-body);
    font-size: var(--font-size-xs);
    font-weight: var(--font-weight-semibold);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: var(--color-text);
    padding: 0.75rem 1rem 0.5rem;
    border-bottom: 1px solid var(--color-border-light);
    margin-bottom: 0.5rem;
  }

  :deep(.p-megamenu-submenu) {
    padding: 0.5rem 0;
  }

  :deep(.p-megamenu-submenu .p-menuitem-link) {
    padding: 0.5rem 1rem;
    font-size: var(--font-size-sm);
    color: var(--color-text-secondary);
    text-transform: none;
    letter-spacing: normal;
    border-radius: var(--radius-md);
    margin: 0 0.5rem;
    width: calc(100% - 1rem);

    &:hover {
      background: var(--color-surface-ground);
      color: var(--color-text);
    }

    .p-menuitem-icon {
      font-size: var(--font-size-xs);
      margin-right: 0.5rem;
      color: var(--color-primary);
    }
  }
}

:deep(.search-menu-item) {
  .p-menuitem-link {
    padding: 0.5rem 0.75rem;

    .p-menuitem-icon {
      font-size: var(--font-size-base);
      margin-right: 0;
      color: var(--color-text-secondary);
      transition: color var(--transition-fast);
    }

    &:hover .p-menuitem-icon {
      color: var(--color-primary);
    }
  }
}
</style>
