import { useToast } from 'primevue/usetoast'

// Cache: Wrap PrimeVue toast with consistent lifetimes per severity
export function useNotify() {
  const toast = useToast()
  return {
    error: (summary: string, detail?: string) => toast.add({ severity: 'error', summary, detail, life: 5000 }),
    success: (summary: string, detail?: string) => toast.add({ severity: 'success', summary, detail, life: 3000 }),
    info: (summary: string, detail?: string) => toast.add({ severity: 'info', summary, detail, life: 3000 }),
    warn: (summary: string, detail?: string) => toast.add({ severity: 'warn', summary, detail, life: 5000 }),
  }
}
