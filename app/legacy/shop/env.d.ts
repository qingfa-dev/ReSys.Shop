/// <reference types="vite/client" />

declare module '@/composables' {
  export * from './app/composables/index'
}

declare module '@/components/layout' {
  import type { AppHeader, AppFooter, MobileNav } from './app/components/layout/index'
  export { AppHeader, AppFooter, MobileNav }
}

declare module '@/components/ui/NewsletterForm.vue' {
  import type { DefineComponent } from 'vue'
  const NewsletterForm: DefineComponent<{}, {}, any>
  export default NewsletterForm
}

declare module '@/components/ui/ScrollToTop.vue' {
  import type { DefineComponent } from 'vue'
  const ScrollToTop: DefineComponent<{}, {}, any>
  export default ScrollToTop
}
