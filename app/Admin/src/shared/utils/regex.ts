export const EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

export const PHONE = /^\+?[\d\s().-]{7,20}$/

export const SLUG = /^[a-z0-9]+(?:-[a-z0-9]+)*$/

export const URL = /^https?:\/\/[\w-]+(\.[\w-]+)+[/#?]?.*$/

export const STRONG_PASSWORD = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_\-+={}[\]|:;"'<>,.?/~`]).{8,}$/
