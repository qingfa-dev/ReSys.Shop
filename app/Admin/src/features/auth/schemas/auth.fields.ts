import { z } from 'zod'

export type TFunction = (key: string) => string

export class AuthFields {
  constructor(private t: TFunction) {}

  email() {
    return z.string().email(this.t('auth.validation.email.invalid'))
  }

  userName() {
    return z
      .string()
      .min(3, this.t('auth.validation.userName.min_length'))
      .max(50, this.t('auth.validation.userName.max_length'))
  }

  password() {
    return z
      .string()
      .min(8, this.t('auth.validation.password.min_length'))
      .max(128, this.t('auth.validation.password.max_length'))
      .regex(/[A-Z]/, this.t('auth.validation.password.uppercase'))
      .regex(/[a-z]/, this.t('auth.validation.password.lowercase'))
      .regex(/[0-9]/, this.t('auth.validation.password.digit'))
      .regex(/[^A-Za-z0-9]/, this.t('auth.validation.password.special'))
  }

  credential() {
    return z
      .string()
      .min(1, this.t('auth.validation.credential.required'))
      .max(255, this.t('auth.validation.credential.max_length'))
  }

  firstName() {
    return z.string().min(1, this.t('auth.validation.firstName.required'))
  }

  lastName() {
    return z.string().max(50, this.t('auth.validation.lastName.max_length')).optional()
  }

  phone() {
    return z.string().optional()
  }

  confirmPassword() {
    return z.string().min(1, this.t('auth.validation.confirmPassword.required'))
  }

  currentPassword() {
    return z
      .string()
      .min(1, this.t('auth.validation.currentPassword.required'))
      .max(128, this.t('auth.validation.currentPassword.max_length'))
  }

  acceptTerm() {
    return z.literal(true, {
      errorMap: () => ({ message: this.t('auth.validation.acceptTerms.required') }),
    })
  }
}

export function createFields(t: TFunction) {
  return new AuthFields(t)
}
