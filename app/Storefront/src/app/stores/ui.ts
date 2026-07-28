import { defineStore } from 'pinia'
import { ref, watch } from 'vue'

export const useUIStore = defineStore('ui', () => {
  const mobileMenuOpen = ref(false)
  const searchOpen = ref(false)
  const cartOpen = ref(false)
  const cookieBannerDismissed = ref(false)
  const newsletterDismissed = ref(false)

  function openMobileMenu() {
    mobileMenuOpen.value = true
  }

  function closeMobileMenu() {
    mobileMenuOpen.value = false
  }

  function toggleMobileMenu() {
    mobileMenuOpen.value = !mobileMenuOpen.value
  }

  function openSearch() {
    searchOpen.value = true
  }

  function closeSearch() {
    searchOpen.value = false
  }

  function toggleCart() {
    cartOpen.value = !cartOpen.value
  }

  function dismissCookieBanner() {
    cookieBannerDismissed.value = true
    localStorage.setItem('cookies-accepted', 'true')
  }

  function dismissNewsletter() {
    newsletterDismissed.value = true
    localStorage.setItem('newsletter-dismissed', 'true')
  }

  watch(mobileMenuOpen, (isOpen) => {
    document.body.style.overflow = isOpen ? 'hidden' : ''
  })

  function hydrate() {
    cookieBannerDismissed.value = !!localStorage.getItem('cookies-accepted')
    newsletterDismissed.value = !!localStorage.getItem('newsletter-dismissed')
  }

  return {
    mobileMenuOpen,
    searchOpen,
    cartOpen,
    cookieBannerDismissed,
    newsletterDismissed,
    openMobileMenu,
    closeMobileMenu,
    toggleMobileMenu,
    openSearch,
    closeSearch,
    toggleCart,
    dismissCookieBanner,
    dismissNewsletter,
    hydrate,
  }
})
