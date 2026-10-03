<template>
  <div class="notification-bell">
    <button
      class="bell-button"
      @click="isOpen = !isOpen"
      :title="`${unreadCount} new notifications`"
    >
      <i class="pi pi-bell"></i>
      <span v-if="unreadCount > 0" class="badge">{{ unreadCount > 9 ? "9+" : unreadCount }}</span>
    </button>

    <div v-if="isOpen" class="notification-dropdown">
      <div class="dropdown-header">
        <h3>Notifications</h3>
        <button v-if="unreadCount > 0" class="mark-all-btn" @click="markAllAsRead">
          Mark all as read
        </button>
      </div>

      <div class="notification-list">
        <div v-if="notifications.length === 0" class="empty-state">
          <p>No notifications yet</p>
        </div>

        <div
          v-for="notification in notifications"
          :key="notification.id"
          class="notification-item"
          :class="{ 'is-unread': !notification.read }"
          @click="handleNotificationClick(notification)"
        >
          <div class="notification-icon">
            <i :class="getIconClass(notification.type)"></i>
          </div>

          <div class="notification-content">
            <h4>{{ notification.title }}</h4>
            <p>{{ notification.message }}</p>
            <span class="time">{{ formatTime(notification.createdAt) }}</span>
          </div>

          <button
            class="remove-btn"
            @click.stop="removeNotification(notification.id)"
            title="Remove"
          >
            ×
          </button>
        </div>
      </div>

      <div class="dropdown-footer">
        <a href="/notifications" class="view-all">View all notifications</a>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useNotifications } from "../composables/useNotifications";

const { notifications, unreadCount, markAsRead, markAllAsRead, removeNotification } =
  useNotifications();

const isOpen = ref(false);

function getIconClass(type: string): string {
  const iconMap: Record<string, string> = {
    "order-confirmed": "pi pi-check-circle",
    "order-shipped": "pi pi-shopping-bag",
    "order-delivered": "pi pi-home",
    "price-drop": "pi pi-tag",
    "in-stock": "pi pi-exclamation-circle",
    "new-arrival": "pi pi-star",
    promotion: "pi pi-gift",
    system: "pi pi-info-circle",
  };
  return iconMap[type] || "pi pi-bell";
}

function formatTime(date: string): string {
  const now = new Date();
  const time = new Date(date);
  const diff = now.getTime() - time.getTime();
  const minutes = Math.floor(diff / 60000);
  const hours = Math.floor(diff / 3600000);
  const days = Math.floor(diff / 86400000);

  if (minutes < 1) return "just now";
  if (minutes < 60) return `${minutes}m ago`;
  if (hours < 24) return `${hours}h ago`;
  if (days < 7) return `${days}d ago`;

  return time.toLocaleDateString();
}

function handleNotificationClick(notification: any) {
  if (!notification.read) {
    markAsRead(notification.id);
  }
  if (notification.actionUrl) {
    window.location.href = notification.actionUrl;
  }
}
</script>

<style scoped lang="scss">
.notification-bell {
  position: relative;

  .bell-button {
    position: relative;
    background: none;
    border: none;
    font-size: 1.5rem;
    cursor: pointer;
    color: var(--color-text);
    transition: color var(--transition-fast);

    &:hover {
      color: var(--color-primary);
    }

    .badge {
      position: absolute;
      top: -5px;
      right: -5px;
      background: #f44;
      color: white;
      border-radius: 50%;
      width: 20px;
      height: 20px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.75rem;
      font-weight: var(--font-weight-medium);
    }
  }

  .notification-dropdown {
    position: absolute;
    top: 100%;
    right: 0;
    width: 350px;
    max-height: 500px;
    background: white;
    border: 1px solid var(--color-border);
    border-radius: var(--radius-xl);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    z-index: 1000;
    margin-top: 0.5rem;
    overflow-y: auto;

    .dropdown-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem;
      border-bottom: 1px solid var(--color-border-light);

      h3 {
        margin: 0;
        font-size: var(--font-size-lg);
      }

      .mark-all-btn {
        background: none;
        border: none;
        color: var(--color-primary);
        cursor: pointer;
        font-size: var(--font-size-sm);

        &:hover {
          text-decoration: underline;
        }
      }
    }

    .notification-list {
      .empty-state {
        padding: 2rem 1rem;
        text-align: center;
        color: var(--color-text-secondary);
      }

      .notification-item {
        display: flex;
        gap: 1rem;
        padding: 1rem;
        border-bottom: 1px solid var(--color-border-light);
        cursor: pointer;
        transition: background var(--transition-fast);

        &:hover {
          background: var(--color-surface);
        }

        &.is-unread {
          background: #f0f8ff;

          &::before {
            content: "";
            position: absolute;
            left: 0;
            width: 3px;
            height: 100%;
            background: var(--color-primary);
          }
        }

        .notification-icon {
          flex-shrink: 0;
          width: 40px;
          height: 40px;
          background: var(--color-surface);
          border-radius: 50%;
          display: flex;
          align-items: center;
          justify-content: center;
          color: var(--color-primary);
        }

        .notification-content {
          flex: 1;

          h4 {
            margin: 0 0 0.25rem;
            font-size: var(--font-size-sm);
          }

          p {
            margin: 0 0 0.5rem;
            font-size: 0.875rem;
            color: var(--color-text-secondary);
          }

          .time {
            font-size: 0.75rem;
            color: #999;
          }
        }

        .remove-btn {
          flex-shrink: 0;
          background: none;
          border: none;
          font-size: 1.5rem;
          color: #ccc;
          cursor: pointer;

          &:hover {
            color: #f44;
          }
        }
      }
    }

    .dropdown-footer {
      padding: 1rem;
      border-top: 1px solid var(--color-border-light);
      text-align: center;

      .view-all {
        color: var(--color-primary);
        text-decoration: none;
        font-size: var(--font-size-sm);

        &:hover {
          text-decoration: underline;
        }
      }
    }
  }
}
</style>
