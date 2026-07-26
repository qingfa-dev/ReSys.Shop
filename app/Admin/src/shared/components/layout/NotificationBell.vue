<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useNotification } from '@/shared/composables/useNotification'
import type { Notification } from '@/shared/types'

const { t } = useI18n()
const router = useRouter()
const { unreadCount, recentItems, markRead, markAllRead } = useNotification()

function onItemClick(notification: Notification) {
  markRead(notification.id)
  if (notification.linkRoute) {
    router.push(notification.linkRoute)
  }
}
</script>

<template>
  <Popover>
    <template #activator="{ toggle }">
      <Button
        text
        rounded
        :badge="unreadCount > 0 ? String(unreadCount > 99 ? '99+' : unreadCount) : undefined"
        badge-severity="danger"
        @click="toggle"
      >
        <template #icon>
          <i :class="['pi pi-bell', unreadCount > 0 ? 'text-primary-500' : 'text-surface-500']" />
        </template>
      </Button>
    </template>

    <div class="w-80">
      <div class="flex items-center justify-between px-4 py-2 border-b border-surface-200 dark:border-surface-700">
        <span class="font-semibold">{{ t('notifications.title') }}</span>
        <Button
          v-if="unreadCount > 0"
          :label="t('notifications.markAllRead')"
          text
          size="small"
          @click="markAllRead"
        />
      </div>

      <div v-if="!recentItems.length" class="p-6 text-center text-surface-500">
        <i class="pi pi-inbox text-2xl mb-2" />
        <p class="text-sm">{{ t('notifications.empty') }}</p>
      </div>

      <div v-else class="divide-y divide-surface-200 dark:divide-surface-700">
        <div
          v-for="item in recentItems"
          :key="item.id"
          class="flex items-start gap-3 p-3 cursor-pointer hover:bg-surface-50 dark:hover:bg-surface-800"
          :class="{ 'bg-primary-50 dark:bg-primary-900/10': !item.isRead }"
          @click="onItemClick(item)"
        >
          <i
            :class="[
              'mt-0.5 text-xs',
              item.isRead ? 'pi pi-circle text-surface-300' : 'pi pi-circle-fill text-primary-500',
            ]"
          />
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium truncate">{{ item.title }}</p>
            <p class="text-xs text-surface-500 truncate">{{ item.message }}</p>
            <p class="text-xs text-surface-400 mt-0.5">{{ item.createdAt }}</p>
          </div>
        </div>
      </div>
    </div>
  </Popover>
</template>
