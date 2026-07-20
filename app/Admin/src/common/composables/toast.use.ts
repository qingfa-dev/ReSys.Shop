import { ref } from 'vue';

export interface ToastMessage {
  severity: 'success' | 'info' | 'warn' | 'error';
  summary: string;
  detail: string;
  life?: number;
}

export const toastBus = ref<ToastMessage | null>(null);

export function useToast() {
  const showToast = (severity: ToastMessage['severity'], summary: string, detail: string, life = 3000) => {
    toastBus.value = { severity, summary, detail, life };
  };

  return {
    showToast,
    toastBus
  };
}
