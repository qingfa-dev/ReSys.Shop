import { watchEffect } from 'vue'

const SUFFIX = ' | ReSys.Shop'

// Format: Set document title with brand suffix — reactive when title is a function
export function usePageTitle(title: string | (() => string)): void {
  const resolved = typeof title === 'function' ? title : () => title
  watchEffect(() => {
    document.title = `${resolved()}${SUFFIX}`
  })
}
