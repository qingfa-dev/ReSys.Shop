import { useConfirm as usePrimeConfirm } from 'primevue/useconfirm';

interface ConfirmOptions {
  target?: string;
  onAccept: () => void;
  onReject?: () => void;
}

/**
 * House-styled confirm helpers. Pairs with shared/components/feedback/ConfirmDialog.vue
 * which must be mounted once (in App.vue) to render the actual dialog surface.
 */
export function useConfirm() {
  const confirm = usePrimeConfirm();

  const confirmDelete = ({ target = 'this item', onAccept, onReject }: ConfirmOptions) => {
    confirm.require({
      header: 'Delete confirmation',
      message: `Are you sure you want to delete ${target}? This action cannot be undone.`,
      icon: 'pi pi-trash',
      acceptLabel: 'Delete',
      accept: onAccept,
      reject: onReject,
    });
  };

  const confirmAction = ({ target = 'this action', onAccept, onReject }: ConfirmOptions) => {
    confirm.require({
      header: 'Please confirm',
      message: `Are you sure you want to proceed with ${target}?`,
      icon: 'pi pi-question-circle',
      acceptLabel: 'Confirm',
      accept: onAccept,
      reject: onReject,
    });
  };

  return { confirmDelete, confirmAction };
}
