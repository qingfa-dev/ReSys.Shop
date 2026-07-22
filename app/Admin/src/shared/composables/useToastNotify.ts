import { useToast } from './useToast'

export function useToastNotify() {
  const { showToast } = useToast()

  const success = (detail: string, summary = 'Success') =>
    showToast('success', summary, detail)

  const error = (detail: string, summary = 'Error') =>
    showToast('error', summary, detail, 5000)

  const warn = (detail: string, summary = 'Warning') =>
    showToast('warn', summary, detail, 4000)

  const info = (detail: string, summary = 'Info') =>
    showToast('info', summary, detail)

  return { success, error, warn, info }
}
