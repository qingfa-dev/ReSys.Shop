import { useToast as usePrimeToast } from 'primevue/usetoast'

export function useToast() {
  const toast = usePrimeToast()
  const showToast = (
    severity: 'success' | 'info' | 'warn' | 'error',
    summary: string,
    detail: string,
    life = 3000,
  ) => {
    toast.add({ severity, summary, detail, life })
  }
  return { showToast }
}
