<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Button from 'primevue/button'
import Skeleton from 'primevue/skeleton'
import Message from 'primevue/message'
import { useAuthStore } from '@/features/identity/store/auth'
import { httpClient } from '@/core/http'

interface SessionInfo {
  id: string
  device?: string
  ipAddress?: string
  lastActivity?: string
  isCurrent?: boolean
  createdAt?: string
}

const router = useRouter()
const authStore = useAuthStore()

const sessions = ref<SessionInfo[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    const response = await httpClient.get('/api/storefront/identity/auth/sessions')
    if (response.data?.isSuccess && response.data?.data) {
      const data = response.data.data
      sessions.value = Array.isArray(data) ? data : [data]
    } else if (response.data?.data) {
      const data = response.data.data
      sessions.value = Array.isArray(data) ? data : [data]
    }
  } catch {
    error.value = 'Failed to load sessions.'
  } finally {
    loading.value = false
  }
})

async function handleLogout() {
  await authStore.logout()
  router.push('/')
}

function formatDate(dateStr?: string): string {
  if (!dateStr) return 'Unknown'
  return new Date(dateStr).toLocaleDateString('en-US', {
    year: 'numeric', month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })
}
</script>

<template>
  <div class="sessions-view">
    <h1>Sessions</h1>
    <p class="subtitle">Manage your active sessions across devices.</p>

    <!-- Loading -->
    <div v-if="loading" class="skeleton-list">
      <Skeleton v-for="i in 2" :key="i" width="100%" height="80px" />
    </div>

    <!-- Error -->
    <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>

    <!-- Session list -->
    <div v-if="!loading && !error" class="session-list">
      <div v-if="sessions.length === 0" class="empty-state">
        <i class="pi pi-desktop"></i>
        <p>No session data available.</p>
      </div>
      <div v-for="session in sessions" :key="session.id" class="session-card"
           :class="{ current: session.isCurrent }">
        <div class="session-icon">
          <i :class="session.isCurrent ? 'pi pi-desktop' : 'pi pi-mobile'" />
        </div>
        <div class="session-info">
          <div class="session-header">
            <strong>{{ session.device || 'Unknown Device' }}</strong>
            <span v-if="session.isCurrent" class="current-badge">Current</span>
          </div>
          <span class="session-meta">
            {{ session.ipAddress || 'Unknown IP' }} &middot;
            Last active: {{ formatDate(session.lastActivity) }}
          </span>
        </div>
      </div>
    </div>

    <div class="actions">
      <Button label="Logout" icon="pi pi-sign-out" severity="danger" @click="handleLogout" />
    </div>
  </div>
</template>

<style scoped lang="scss">
.sessions-view {
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
  gap: 1rem;
}

.session-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-bottom: 2rem;
}

.session-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 1.25rem;
  border: 1px solid var(--color-border-light);
  border-radius: var(--radius-md);

  &.current {
    border-color: var(--color-primary);
    background: rgba(var(--color-primary-rgb), 0.04);
  }
}

.session-icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  background: var(--color-surface-ground);
  display: flex;
  align-items: center;
  justify-content: center;

  i { font-size: 1.25rem; color: var(--color-text-secondary); }
}

.session-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.session-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;

  strong { font-size: var(--font-size-base); }
}

.current-badge {
  padding: 0.125rem 0.5rem;
  background: var(--color-primary);
  color: white;
  border-radius: var(--radius-full);
  font-size: var(--font-size-xs);
}

.session-meta {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
}

.actions {
  display: flex;
  justify-content: flex-end;
}

.empty-state {
  text-align: center;
  padding: 3rem 1rem;
  color: var(--color-text-secondary);

  i { font-size: 3rem; margin-bottom: 1rem; }
}
</style>
