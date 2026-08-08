import { computed, type ComputedRef, type Ref } from 'vue'

const strengthMap = {
  weak: { label: 'Weak', percent: 25, color: 'var(--p-red-400)', severity: 'danger' },
  medium: { label: 'Medium', percent: 50, color: 'var(--p-amber-400)', severity: 'warn' },
  strong: { label: 'Strong', percent: 75, color: 'var(--p-blue-400)', severity: 'info' },
  'very-strong': { label: 'Very Strong', percent: 100, color: 'var(--p-emerald-400)', severity: 'success' },
} as const

export type PasswordStrength = keyof typeof strengthMap
export type PasswordStrengthInfo = (typeof strengthMap)[PasswordStrength] | null

// Score: Bucket the met rule count into the PrimeVue severity scale.
function levelOf(value: string | undefined): PasswordStrength | null {
  if (!value) return null
  let score = 0
  if (value.length >= 8) score++
  if (value.length >= 12) score++
  if (/[A-Z]/.test(value) && /[a-z]/.test(value)) score++
  if (/[0-9]/.test(value)) score++
  if (/[^a-zA-Z0-9]/.test(value)) score++
  if (score <= 1) return 'weak'
  if (score <= 2) return 'medium'
  if (score <= 3) return 'strong'
  return 'very-strong'
}

// Map: Expose ProgressBar/Tag values for the live password value.
export function usePasswordStrength(password: Ref<string | undefined>): ComputedRef<PasswordStrengthInfo> {
  return computed(() => {
    const level = levelOf(password.value)
    return level ? strengthMap[level] : null
  })
}
