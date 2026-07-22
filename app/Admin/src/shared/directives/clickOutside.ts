import type { Directive, DirectiveBinding } from 'vue'

interface ClickOutsideElement extends HTMLElement {
  __clickOutsideHandler?: (event: MouseEvent) => void
}

export const clickOutside: Directive = {
  mounted(el: ClickOutsideElement, binding: DirectiveBinding) {
    const handler = (event: MouseEvent) => {
      if (!(el === event.target || el.contains(event.target as Node))) {
        binding.value(event)
      }
    }
    el.__clickOutsideHandler = handler
    document.addEventListener('click', handler)
  },
  unmounted(el: ClickOutsideElement) {
    if (el.__clickOutsideHandler) {
      document.removeEventListener('click', el.__clickOutsideHandler)
    }
  },
}
