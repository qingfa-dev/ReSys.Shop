import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

const RECENTLY_VIEWED_KEY = 'shop_recently_viewed'
const MAX_RECENTLY_VIEWED = 10

export const useUIStore = defineStore('ui', () => {
  const searchOpen = ref(false)
  const mobileNavOpen = ref(false)
  const sizeGuideOpen = ref(false)
  const cookieBannerShown = ref(false)
  const newsletterShown = ref(false)
  const cartDrawerOpen = ref(false)
  const mobileFilterOpen = ref(false)

  const recentlyViewed = ref<string[]>([])

  const hasRecentlyViewed = computed(() => recentlyViewed.value.length > 0)

  function openSearch() {
    searchOpen.value = true
  }

  function closeSearch() {
    searchOpen.value = false
  }

  function toggleSearch() {
    searchOpen.value = !searchOpen.value
  }

  function toggleMobileNav() {
    mobileNavOpen.value = !mobileNavOpen.value
  }

  function closeMobileNav() {
    mobileNavOpen.value = false
  }

  function openSizeGuide() {
    sizeGuideOpen.value = true
  }

  function closeSizeGuide() {
    sizeGuideOpen.value = false
  }

  function openCartDrawer() {
    cartDrawerOpen.value = true
  }

  function closeCartDrawer() {
    cartDrawerOpen.value = false
  }

  function toggleCartDrawer() {
    cartDrawerOpen.value = !cartDrawerOpen.value
  }

  function openMobileFilter() {
    mobileFilterOpen.value = true
  }

  function closeMobileFilter() {
    mobileFilterOpen.value = false
  }

  function dismissCookieBanner() {
    cookieBannerShown.value = true
    localStorage.setItem('cookies-accepted', 'true')
  }

  function dismissNewsletter() {
    newsletterShown.value = true
    localStorage.setItem('newsletter-dismissed', 'true')
  }

  function addRecentlyViewed(productId: string) {
    recentlyViewed.value = [
      productId,
      ...recentlyViewed.value.filter(id => id !== productId),
    ].slice(0, MAX_RECENTLY_VIEWED)
    persistRecentlyViewed()
  }

  function clearRecentlyViewed() {
    recentlyViewed.value = []
    persistRecentlyViewed()
  }

  function persistRecentlyViewed() {
    try {
      localStorage.setItem(RECENTLY_VIEWED_KEY, JSON.stringify(recentlyViewed.value))
    } catch (e) {
      console.error('Failed to persist recently viewed:', e)
    }
  }

  function hydrateUI() {
    cookieBannerShown.value = !!localStorage.getItem('cookies-accepted')
    newsletterShown.value = !!localStorage.getItem('newsletter-dismissed')

    try {
      const rv = localStorage.getItem(RECENTLY_VIEWED_KEY)
      if (rv) {
        recentlyViewed.value = JSON.parse(rv)
      }
    } catch (e) {
      console.error('Failed to hydrate recently viewed:', e)
      recentlyViewed.value = []
    }
  }

  return {
    searchOpen,
    mobileNavOpen,
    sizeGuideOpen,
    cookieBannerShown,
    newsletterShown,
    cartDrawerOpen,
    mobileFilterOpen,
    recentlyViewed,
    hasRecentlyViewed,
    openSearch,
    closeSearch,
    toggleSearch,
    toggleMobileNav,
    closeMobileNav,
    openSizeGuide,
    closeSizeGuide,
    openCartDrawer,
    closeCartDrawer,
    toggleCartDrawer,
    openMobileFilter,
    closeMobileFilter,
    dismissCookieBanner,
    dismissNewsletter,
    addRecentlyViewed,
    clearRecentlyViewed,
    hydrateUI,
  }
})
