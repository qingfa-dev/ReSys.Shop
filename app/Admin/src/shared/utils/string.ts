export function capitalize(value: string): string {
  if (!value) return value
  return value.charAt(0).toUpperCase() + value.slice(1)
}

export function truncate(text: string, length: number, suffix = '...'): string {
  if (text.length <= length) return text
  const trimmed = text.slice(0, length)
  const lastSpace = trimmed.lastIndexOf(' ')
  return (lastSpace > 0 ? trimmed.slice(0, lastSpace) : trimmed) + suffix
}

export function slugify(text: string): string {
  return text
    .toLowerCase()
    .trim()
    .replace(/[^\w\s-]/g, '')
    .replace(/[\s_]+/g, '-')
    .replace(/-+/g, '-')
    .replace(/^-|-$/g, '')
}

export function toCamelCase(str: string): string {
  return str.charAt(0).toLowerCase() + str.slice(1).replace(/_([a-z])/g, (_, c) => c.toUpperCase())
}
