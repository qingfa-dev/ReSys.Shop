import 'vue-router'

declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth?: boolean
    guestOnly?: boolean
    title?: string
    subtitle?: string
    statusCode?: string | number
    description?: string
    icon?: string
    iconBgClass?: string
    image?: string
    buttonLabel?: string
    buttonTo?: string
    gradientColor?: string
    links?: Array<{
      icon: string
      title: string
      description: string
      to: string
    }>
  }
}
