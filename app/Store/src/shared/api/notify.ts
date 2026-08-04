// Ported from app/Admin — keep in sync.
import type { ToastServiceMethods } from 'primevue/toastservice'

let toast: ToastServiceMethods | null = null

export function setNotifyToast(t: ToastServiceMethods): void {
  toast = t
}

export function notifyError(summary: string, detail?: string): void {
  toast?.add({ severity: 'error', summary, detail, life: 5000 })
}

export function notifySuccess(summary: string, detail?: string): void {
  toast?.add({ severity: 'success', summary, detail, life: 3000 })
}

export function notifyInfo(summary: string, detail?: string): void {
  toast?.add({ severity: 'info', summary, detail, life: 3000 })
}

export function notifyWarn(summary: string, detail?: string): void {
  toast?.add({ severity: 'warn', summary, detail, life: 5000 })
}
