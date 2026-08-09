<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import type { MenuItem } from 'primevue/menuitem'
import type Menu from 'primevue/menu'
import type { AutoCompleteOptionSelectEvent } from 'primevue/autocomplete'
import { useSearch } from '@/features/catalog/composables/useSearch'
import SearchOverlay from '@/features/catalog/components/SearchOverlay.vue'
import { useCartStore } from '@/features/ordering/stores/cartStore'
import { useAuthStore } from '@/features/identity/stores/authStore'

// Actions: Parent layout hosts MobileNav (Task 11) and CartDrawer (Task 31).
const emit = defineEmits<{
  'open-mobile-nav': []
  'open-cart': []
}>()

const router = useRouter()
// Search: Destructure refs so template bindings unwrap them as top-level properties.
const { query, results, loading, open, clear, search, navigateToResult } = useSearch()
const cartStore = useCartStore()
const authStore = useAuthStore()
const userMenu = ref<InstanceType<typeof Menu> | null>(null)

// Nav: Primary storefront routes; Menubar v5 has no router support so command navigates.
const navItems: MenuItem[] = [
  { label: 'Home', icon: 'pi pi-home', command: () => router.push('/') },
  { label: 'Shop', icon: 'pi pi-shopping-bag', command: () => router.push('/shop') },
  { label: 'Collections', icon: 'pi pi-images', command: () => router.push('/collections') },
  { label: 'Visual Search', icon: 'pi pi-camera', command: () => router.push('/recommendations') },
]

// Account: Popup menu for the authenticated user; sign-out delegates to the auth store.
const userItems: MenuItem[] = [
  { label: 'My Profile', icon: 'pi pi-user', command: () => router.push('/account/profile') },
  { label: 'Orders', icon: 'pi pi-shopping-bag', command: () => router.push('/account/orders') },
  { label: 'Sessions', icon: 'pi pi-shield', command: () => router.push('/account/sessions') },
  { separator: true },
  { label: 'Sign Out', icon: 'pi pi-sign-out', command: () => void authStore.logout() },
]

// Initial: First letter of the username for the avatar badge.
const userInitial = computed(() => authStore.user?.userName?.trim()?.[0]?.toUpperCase() ?? 'U')

// Suggest: Flatten search results into AutoComplete options keyed by product id.
const suggestions = computed(() => results.value.map(r => ({ id: r.id, label: r.name, slug: r.slug })))


// Select: Locate the chosen suggestion in the results and navigate to its product page.
function onOptionSelect(event: AutoCompleteOptionSelectEvent): void {
  const option = event.value as { id: string; label: string; slug: string }
  const index = results.value.findIndex(r => r.id === option.id)
  if (index !== -1) navigateToResult(index)
}

// Load: Refresh the cart badge on mount.
onMounted(() => {
  void cartStore.fetchCart()
})
</script>

<template>
  <!-- Section: Sticky Header — blur backdrop wrapping brand, nav, search and actions -->
  <header
    class="sticky top-0 z-40 border-b border-surface-200 bg-surface-0/80 backdrop-blur"
  >
    <div class="mx-auto flex h-16 max-w-7xl items-center gap-2 px-4 sm:px-6">
      <!-- Mobile Nav Trigger: Opens the MobileNav drawer below lg (Task 11) -->
      <Button
        icon="pi pi-bars"
        variant="text"
        severity="secondary"
        rounded
        class="lg:hidden"
        aria-label="Open navigation menu"
        v-tooltip.bottom="'Open navigation menu'"
        @click="emit('open-mobile-nav')"
      />

      <!-- Brand: Wordmark links back to the storefront home -->
      <Button as="router-link" to="/" variant="text" class="px-2" aria-label="ReSys.Shop home">
        <i class="pi pi-sparkles text-xl text-brand" />
        <span class="ml-2 text-lg font-semibold tracking-tight">ReSys.Shop</span>
      </Button>

      <!-- Primary Nav: Menubar with the main storefront routes on lg+ -->
      <Menubar :model="navItems" class="hidden lg:flex" />

      <!-- Catalog: Direct link to /shop replacing the removed MegaMenu (TASK-010) -->
      <Button as="router-link" to="/shop" label="Catalog" variant="text" class="hidden lg:flex" />

      <div class="flex-1" />

      <!-- Search: Inline AutoComplete with product suggestions on md+ -->
      <AutoComplete
        v-model="query"
        :suggestions="suggestions"
        optionLabel="label"
        :loading="loading"
        placeholder="Search products..."
        emptySearchMessage="No products found"
        class="hidden w-44 md:block xl:w-64"
        @complete="search()"
        @clear="clear()"
        @option-select="onOptionSelect"
      />

      <!-- Search Overlay Trigger: Opens the full-screen overlay below md (Task 19) -->
      <Button
        icon="pi pi-search"
        variant="text"
        severity="secondary"
        rounded
        class="md:hidden"
        aria-label="Open search overlay"
        v-tooltip.bottom="'Search'"
        @click="open()"
      />

      <!-- Cart: Opens the CartDrawer overlay via the parent layout (Task 31) -->
      <OverlayBadge v-if="cartStore.itemCount > 0" :value="cartStore.itemCount" severity="danger">
        <Button
          icon="pi pi-shopping-cart"
          variant="text"
          severity="secondary"
          rounded
          aria-label="Open cart"
          v-tooltip.bottom="'Open cart'"
          @click="emit('open-cart')"
        />
      </OverlayBadge>
      <Button
        v-else
        icon="pi pi-shopping-cart"
        variant="text"
        severity="secondary"
        rounded
        aria-label="Open cart"
        v-tooltip.bottom="'Open cart'"
        @click="emit('open-cart')"
      />

      <!-- User Area: Account popup menu when signed in, Sign In button otherwise -->
      <template v-if="authStore.isAuthenticated">
        <Button
          variant="text"
          severity="secondary"
          rounded
          :aria-label="`Account menu for ${authStore.user?.userName ?? 'user'}`"
          v-tooltip.bottom="authStore.user?.userName"
          @click="userMenu?.toggle($event)"
        >
          <Avatar
            :label="userInitial"
            shape="circle"
            size="small"
            class="bg-primary-100 text-primary-900"
          />
          <i class="pi pi-chevron-down ml-2" />
        </Button>
        <Menu ref="userMenu" :model="userItems" popup />
      </template>
      <Button v-else as="router-link" to="/login" label="Sign In" icon="pi pi-sign-in" iconPos="right" />
    </div>
  </header>

  <!-- Search Palette: Mounted once so the header button and Ctrl+K share one overlay (Task 19) -->
  <SearchOverlay />
</template>
