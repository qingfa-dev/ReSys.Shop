export type Metadata = Record<string, unknown>

export function metadataValue<T = unknown>(meta: Metadata, key: string): T | undefined {
  const val = meta[key]
  return val as T | undefined
}

export function setMetadataValue(meta: Metadata, key: string, value: unknown): void {
  meta[key] = value
}
