export function slugify(input: string): string {
  return input
    .toLowerCase()
    .normalize('NFKD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
}

export function humanize(input: string): string {
  return input.replace(/[-_]+/g, ' ').replace(/\b\w/g, (c) => c.toUpperCase())
}
