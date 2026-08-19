<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useConfirm } from 'primevue/useconfirm'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useNotify } from '@/shared/composables/useNotify'
import { SessionApi } from '../services'
import type { SessionInfo } from '../types'

usePageTitle('Sessions')

// State: Session rows plus fetch status; authStore has no session state, so the
// view talks to SessionApi directly.
const sessions = ref<SessionInfo[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

// Services: ConfirmPopup anchors to the clicked button via the confirm service.
const confirm = useConfirm()
const notify = useNotify()

// Fetch: Load the session list on mount.
async function loadSessions(): Promise<void> {
  loading.value = true
  error.value = null
  const result = await SessionApi.getSessions()
  if (result.isSuccess && result.value) {
    sessions.value = result.value
  } else {
    error.value = 'Failed to load sessions'
  }
  loading.value = false
}

// Icon: Map a device name to the closest device glyph.
function deviceIcon(session: SessionInfo): string {
  const name = session.deviceName.toLowerCase()
  if (name.includes('phone') || name.includes('iphone')) return 'pi pi-mobile'
  if (name.includes('tablet') || name.includes('ipad')) return 'pi pi-tablet'
  return 'pi pi-desktop'
}

// Relative: Format last active as a compact human-readable age.
function formatRelativeTime(iso: string): string {
  const minutes = Math.round((Date.now() - new Date(iso).getTime()) / 60000)
  if (minutes < 1) return 'just now'
  if (minutes < 60) return `${minutes}m ago`
  const hours = Math.round(minutes / 60)
  if (hours < 24) return `${hours}h ago`
  const days = Math.round(hours / 24)
  return `${days}d ago`
}

// Confirm: Anchor the popup to the clicked button, revoke on accept.
function onRevoke(event: Event, session: SessionInfo): void {
  confirm.require({
    target: event.currentTarget as HTMLElement,
    header: 'End session?',
    message: `End all other active sessions, including ${session.deviceName}?`,
    icon: 'pi pi-exclamation-triangle',
    accept: () => void revokeSession(),
  })
}

// Revoke: Backend exposes no per-session endpoint, so revoking one other
// session revokes all; keep the current row after success.
async function revokeSession(): Promise<void> {
  const result = await SessionApi.revokeAll()
  if (result.isSuccess) {
    sessions.value = sessions.value.filter(s => s.isCurrent)
    notify.success('Other sessions revoked')
  } else {
    notify.error('Failed to revoke sessions')
  }
}

onMounted(() => void loadSessions())
</script>

<template>
  <Card>
    <template #title>Active Sessions</template>
    <template #content>
      <!-- Section: Loading State — skeleton rows while the list fetches -->
      <div v-if="loading" class="flex flex-col gap-3">
        <Skeleton v-for="i in 3" :key="i" height="4rem" />
      </div>

      <!-- Section: Error State — message and retry when the fetch fails -->
      <div v-else-if="error" class="flex flex-col items-center gap-4 py-8">
        <Message severity="error" :closable="false">{{ error }}</Message>
        <Button label="Retry" severity="secondary" outlined @click="loadSessions" />
      </div>

      <!-- Section: Data Table — sessions with device, IP and last-active columns -->
      <DataTable v-else :value="sessions" dataKey="id">
        <!-- Section: Table Columns -->
        <Column header="Device">
          <template #body="{ data }">
            <div class="flex items-center gap-3">
              <i :class="deviceIcon(data)" class="text-lg text-muted" />
              <span class="font-medium">{{ data.deviceName }}</span>
              <Tag v-if="data.isCurrent" value="This device" severity="info" rounded />
            </div>
          </template>
        </Column>
        <Column field="ipAddress" header="IP Address" />
        <Column header="Last Active">
          <template #body="{ data }">{{ formatRelativeTime(data.lastActivityAt) }}</template>
        </Column>

        <!-- Section: Row Actions — revoke ends other sessions after confirmation -->
        <Column header="Actions">
          <template #body="{ data }">
            <Button
              icon="pi pi-times"
              label="Revoke"
              size="small"
              severity="danger"
              variant="text"
              v-tooltip.bottom="'End this session'"
              :disabled="data.isCurrent"
              @click="onRevoke($event, data)"
            />
          </template>
        </Column>

        <!-- Section: Empty State -->
        <template #empty>
          <Message severity="info" :closable="false">No active sessions found.</Message>
        </template>
      </DataTable>

      <ConfirmPopup />
    </template>
  </Card>
</template>
