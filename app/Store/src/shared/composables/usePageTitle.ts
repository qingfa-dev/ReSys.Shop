import { watchEffect } from 'vue'

const SUFFIX = ' | ReSys.Shop'

export function usePageTitle(title: string | (() => string)): void {
  const resolved = typeof title === 'function' ? title : () => title
  watchEffect(() => {
    document.title = `${resolved()}${SUFFIX}`
  })
}
