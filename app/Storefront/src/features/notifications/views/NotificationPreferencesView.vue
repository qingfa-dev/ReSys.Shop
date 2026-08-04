<script setup lang="ts">
import { ref, onMounted } from 'vue'
import Button from 'primevue/button'
import ToggleSwitch from 'primevue/toggleswitch'
import Skeleton from 'primevue/skeleton'
import Message from 'primevue/message'
import { useToast } from 'primevue/usetoast'
import { notificationsService } from '../services/notifications.service'
import type { NotificationPreference } from '../repositories/notifications.repository.interface'

const toast = useToast()
const preferences = ref<NotificationPreference[]>([])
const loading = ref(true)
const saving = ref(false)
const error = ref<string | null>(null)

onMounted(async () => {
  const result = await notificationsService.getPreferences()
  loading.value = false
  if (result.isSuccess && result.data) {
    preferences.value = result.data
  } else {
    error.value = result.message || 'Failed to load preferences'
  }
})

async function handleSave() {
  saving.value = true
  error.value = null
  const result = await notificationsService.updateAllPreferences(preferences.value)
  saving.value = false
  if (result.isSuccess) {
    toast.add({ severity: 'success', summary: 'Saved', detail: 'Notification preferences updated.', life: 3000 })
  } else {
    error.value = result.message || 'Failed to save preferences'
  }
}
</script>

<template>
  <div class="notifications-view">
    <h1>Notification Preferences</h1>
    <p class="subtitle">Choose how you want to be notified.</p>

    <!-- Loading -->
    <div v-if="loading" class="skeleton-list">
      <Skeleton v-for="i in 3" :key="i" width="100%" height="56px" />
    </div>

    <!-- Error -->
    <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>

    <!-- Toggle table -->
    <div v-if="!loading && !error" class="prefs-table">
      <div class="table-header">
        <span class="header-category">Category</span>
        <span class="header-channel">Email</span>
        <span class="header-channel">SMS</span>
      </div>
      <div v-if="preferences.length === 0" class="empty-state">
        <i class="pi pi-bell-slash"></i>
        <p>No notification preferences configured.</p>
      </div>
      <div v-for="pref in preferences" :key="pref.id" class="pref-row">
        <span class="pref-category">{{ pref.type }}</span>
        <ToggleSwitch v-model="pref.email" :input-id="'email-' + pref.id" />
        <ToggleSwitch v-model="pref.sms" :input-id="'sms-' + pref.id" />
      </div>
    </div>

    <div v-if="!loading" class="actions">
      <Button label="Save Preferences" icon="pi pi-check" :loading="saving" @click="handleSave" />
    </div>
  </div>
</template>

<style scoped lang="scss">
.notifications-view {
  max-width: 640px;
  margin: 0 auto;
  padding: 2rem;

  h1 {
    font-family: var(--font-display);
    font-size: var(--font-size-2xl);
    margin-bottom: 0.25rem;
  }

  .subtitle {
    color: var(--color-text-secondary);
    margin-bottom: 2rem;
  }
}

.skeleton-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-bottom: 2rem;
}

.prefs-table {
  border: 1px solid var(--color-border-light);
  border-radius: var(--radius-md);
  overflow: hidden;
  margin-bottom: 2rem;
}

.table-header {
  display: grid;
  grid-template-columns: 1fr 80px 80px;
  gap: 1rem;
  padding: 0.75rem 1.25rem;
  background: var(--color-surface-ground);
  font-weight: var(--font-weight-semibold);
  font-size: var(--font-size-sm);
  text-transform: uppercase;
  letter-spacing: 0.05em;

  .header-channel {
    text-align: center;
  }
}

.pref-row {
  display: grid;
  grid-template-columns: 1fr 80px 80px;
  gap: 1rem;
  align-items: center;
  padding: 0.75rem 1.25rem;
  border-top: 1px solid var(--color-border-light);

  .pref-category {
    font-weight: var(--font-weight-medium);
  }
}

.actions {
  display: flex;
  justify-content: flex-end;
}

.empty-state {
  text-align: center;
  padding: 3rem 1rem;
  color: var(--color-text-secondary);

  i { font-size: 2.5rem; margin-bottom: 0.75rem; }
}
</style>
