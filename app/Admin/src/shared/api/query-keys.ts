export function withFilters<TFilters extends Record<string, unknown>>(
  base: readonly unknown[],
  filters: TFilters,
): readonly unknown[] {
  return [...base, filters] as const
}

export function withId(base: readonly unknown[], id: string): readonly unknown[] {
  return [...base, id] as const
}
