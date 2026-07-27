import type { Directive } from 'vue'

export const autofocus: Directive = {
  mounted(el: HTMLElement) {
    if (el instanceof HTMLInputElement || el instanceof HTMLTextAreaElement || el instanceof HTMLSelectElement) {
      el.focus()
    } else {
      const input = el.querySelector('input, textarea, select') as HTMLElement | null
      input?.focus()
    }
  },
}
