import type { Directive, DirectiveBinding } from 'vue'

interface ClickOutsideElement extends HTMLElement {
  __clickOutsideHandler?: (event: MouseEvent) => void
}

export const clickOutside: Directive = {
  mounted(el: ClickOutsideElement, binding: DirectiveBinding) {
    const handler = binding.value
    const exceptSelectors = (binding.arg || '').split(',').filter(Boolean)

    el.__clickOutsideHandler = (event: MouseEvent) => {
      const target = event.target as Node | null
      if (!target) return

      if (exceptSelectors.length > 0 && target instanceof Element) {
        for (const sel of exceptSelectors) {
          if (target.closest(sel.trim())) return
        }
      }

      if (!el.contains(target)) {
        handler(event)
      }
    }
    document.addEventListener('click', el.__clickOutsideHandler)
  },
  unmounted(el: ClickOutsideElement) {
    if (el.__clickOutsideHandler) {
      document.removeEventListener('click', el.__clickOutsideHandler)
    }
  },
}
