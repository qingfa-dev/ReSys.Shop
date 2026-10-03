export function enumToOptions<T extends Record<string, string>>(
  enumObj: T
): { label: string; value: T[keyof T] }[] {
  return (Object.values(enumObj) as string[]).map((value) => ({
    label: value.replace(/([A-Z])/g, ' $1').trim(),
    value: value as T[keyof T],
  }))
}

export function enumLabel<T extends Record<string, string>>(
  enumObj: T,
  value: T[keyof T] | undefined
): string {
  if (!value) return ''
  return value.replace(/([A-Z])/g, ' $1').trim()
}
