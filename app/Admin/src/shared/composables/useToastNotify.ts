import { useToast } from 'primevue/usetoast';

/**
 * House-styled toast helpers. Requires <Toast /> mounted once in App.vue.
 */
export function useToastNotify() {
  const toast = useToast();

  const success = (detail: string, summary = 'Success') =>
    toast.add({ severity: 'success', summary, detail, life: 3000 });

  const error = (detail: string, summary = 'Error') =>
    toast.add({ severity: 'error', summary, detail, life: 5000 });

  const warn = (detail: string, summary = 'Warning') =>
    toast.add({ severity: 'warn', summary, detail, life: 4000 });

  const info = (detail: string, summary = 'Info') =>
    toast.add({ severity: 'info', summary, detail, life: 3000 });

  return { success, error, warn, info };
}
