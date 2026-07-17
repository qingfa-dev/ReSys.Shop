import type { FeatureLocales } from '@/shared/locales/locale.types'

export const authLocales: FeatureLocales = {
  titles: {
    login: 'Sign In',
    welcome: 'Welcome Back',
    app_name: 'ReSys.Shop',
    app_subtitle: 'Admin Control Panel',
  },
  labels: {
    credential: 'Email or Username',
    password: 'Password',
    remember_me: 'Remember me',
    forgot_password: 'Forgot password?',
    sign_in: 'Sign In',
  },
  placeholders: {
    credential: 'admin@resys.shop',
    password: '••••••••',
  },
  messages: {
    login_success: 'You have successfully logged in.',
    login_failed: 'Invalid credentials or server error.',
    validation_failed: 'Please check your input.',
    loading: 'Signing in...',
    copyright: '© {year} ReSys.Shop. All rights reserved.',
  },
  common: {
    success: 'Success',
    error: 'Error',
    warning: 'Warning',
  },
  actions: {
    sign_in: 'Sign In',
  }
}
