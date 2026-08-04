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

  const success = (detail: string, summary = 'Success') =>
    showToast('success', summary, detail)
  const error = (detail: string, summary = 'Error') =>
    showToast('error', summary, detail, 5000)
  const warn = (detail: string, summary = 'Warning') =>
    showToast('warn', summary, detail, 4000)
  const info = (detail: string, summary = 'Info') =>
    showToast('info', summary, detail)

  return { showToast, success, error, warn, info }
}
