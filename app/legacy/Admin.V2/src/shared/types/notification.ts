export type NotificationType = 'order_status' | 'payment_status' | 'stock_alert' | 'system'

export interface Notification {
  id: string
  type: NotificationType
  title: string
  message: string
  linkRoute?: string
  isRead: boolean
  createdAt: string
}
