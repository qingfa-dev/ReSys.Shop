export type NotificationType =
    | 'order-confirmed'
    | 'order-shipped'
    | 'order-delivered'
    | 'price-drop'
    | 'in-stock'
    | 'new-arrival'
    | 'promotion'
    | 'system'

export interface Notification {
    id: string
    userId: string
    type: NotificationType
    title: string
    message: string
    icon?: string
    actionUrl?: string
    read: boolean
    createdAt: string
    expiresAt?: string
}

export interface NotificationPreference {
    type: NotificationType
    channels: ('in-app' | 'email' | 'push')[]
    enabled: boolean
}

export interface NotificationPreferences {
    userId: string
    preferences: NotificationPreference[]
    unsubscribeToken?: string
}
