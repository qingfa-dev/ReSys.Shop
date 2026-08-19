import type { ToastServiceMethods } from 'primevue/toastservice'

// State: PrimeVue toast instance — set once at app bootstrap
let toast: ToastServiceMethods | null = null

// Initialize: Inject PrimeVue toast service at app startup
export function setNotifyToast(t: ToastServiceMethods): void {
  toast = t
}

// Notify: Display error toast — 5s auto-dismiss for server errors
export function notifyError(summary: string, detail?: string): void {
  toast?.add({ severity: 'error', summary, detail, life: 5000 })
}

// Notify: Display success toast — 3s auto-dismiss
export function notifySuccess(summary: string, detail?: string): void {
  toast?.add({ severity: 'success', summary, detail, life: 3000 })
}

// Notify: Display info toast — 3s auto-dismiss
export function notifyInfo(summary: string, detail?: string): void {
  toast?.add({ severity: 'info', summary, detail, life: 3000 })
}

// Notify: Display warning toast — 5s auto-dismiss for actionable warnings
export function notifyWarn(summary: string, detail?: string): void {
  toast?.add({ severity: 'warn', summary, detail, life: 5000 })
}
