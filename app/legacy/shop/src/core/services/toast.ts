import { ref } from 'vue'

export interface Toast {
  id: number
  severity: 'success' | 'info' | 'warn' | 'error'
  summary: string
  detail?: string
  life?: number
  closable?: boolean
}

const toasts = ref<Toast[]>([])
let nextId = 0

export const toastService = {
  success(summary: string, detail?: string, life = 3000) {
    this.add({ severity: 'success', summary, detail, life })
  },

  info(summary: string, detail?: string, life = 3000) {
    this.add({ severity: 'info', summary, detail, life })
  },

  warn(summary: string, detail?: string, life = 5000) {
    this.add({ severity: 'warn', summary, detail, life })
  },

  error(summary: string, detail?: string, life = 5000) {
    this.add({ severity: 'error', summary, detail, life })
  },

  add(toast: Omit<Toast, 'id'>) {
    const id = nextId++
    toasts.value.push({ ...toast, id })

    if (toast.life && toast.life > 0) {
      setTimeout(() => this.remove(id), toast.life)
    }

    return id
  },

  remove(id: number) {
    const index = toasts.value.findIndex(t => t.id === id)
    if (index > -1) {
      toasts.value.splice(index, 1)
    }
  },

  clear() {
    toasts.value = []
  },

  getToasts() {
    return toasts
  },
}

export function useToast() {
  return toastService
}
