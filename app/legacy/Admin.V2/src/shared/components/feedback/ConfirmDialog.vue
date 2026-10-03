<template>
  <!--
    Global confirm surface — mount once in App.vue.
    Trigger via the shared/composables/useConfirm.ts wrapper, e.g.:

      const { confirmDelete } = useConfirm();
      confirmDelete({ target: 'this product', onAccept: () => deleteProduct(id) });
  -->
  <ConfirmDialog>
    <template #container="{ message, acceptCallback, rejectCallback }">
      <div class="flex flex-col items-center gap-4 rounded-border bg-white p-6 dark:bg-surface-900">
        <div
          class="flex h-12 w-12 items-center justify-center rounded-full"
          :class="message.severity === 'error' ? 'bg-red-100 dark:bg-red-400/10' : 'bg-orange-100 dark:bg-orange-400/10'"
        >
          <i
            :class="[message.icon || 'pi pi-exclamation-triangle', message.severity === 'error' ? 'text-red-500' : 'text-orange-500']"
            class="text-xl"
          />
        </div>
        <div class="text-center">
          <p class="font-semibold text-surface-900 dark:text-surface-0">{{ message.header }}</p>
          <p class="mt-1 text-sm text-surface-500">{{ message.message }}</p>
        </div>
        <div class="flex w-full gap-2">
          <Button class="flex-1" label="Cancel" severity="secondary" outlined @click="rejectCallback" />
          <Button
            class="flex-1"
            :label="message.acceptLabel || 'Confirm'"
            :severity="message.severity === 'error' ? 'danger' : 'warn'"
            @click="acceptCallback"
          />
        </div>
      </div>
    </template>
  </ConfirmDialog>
</template>

<script setup lang="ts">
import ConfirmDialog from 'primevue/confirmdialog';
import Button from 'primevue/button';
</script>
