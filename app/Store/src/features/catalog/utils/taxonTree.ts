import type { StoreTaxonListItemResponse, TaxonTreeNode } from '../types/taxon'

/**
 * Build a recursive tree from a flat taxon list.
 * Only includes taxons belonging to the given taxonomyId (if provided).
 * Roots are items with `parentId === null`.
 */
export function buildTaxonTree(
  taxons: StoreTaxonListItemResponse[],
  taxonomyId?: string,
): TaxonTreeNode[] {
  // Transform: Filter by taxonomy if provided — allows building trees for a single taxonomy
  const filtered = taxonomyId
    ? taxons.filter(t => t.taxonomyId === taxonomyId)
    : taxons

  // Group: Index taxons by parentId for O(1) child lookup during recursion
  const byParent = new Map<string | null, StoreTaxonListItemResponse[]>()
  for (const t of filtered) {
    const key = t.parentId ?? null
    const list = byParent.get(key) ?? []
    list.push(t)
    byParent.set(key, list)
  }

  function sortSiblings(items: StoreTaxonListItemResponse[]): TaxonTreeNode[] {
    // Sort: Preserve display order defined by backend position field
    return items
      .sort((a, b) => a.position - b.position)
      .map(t => ({
        id: t.id,
        name: t.name,
        presentation: t.presentation,
        permalink: t.permalink,
        depth: t.depth,
        hasChildren: (t.childrenCount ?? 0) > 0,
        children: sortSiblings(byParent.get(t.id) ?? []),
      }))
  }

  // Build: Start from root nodes (parentId === null)
  return sortSiblings(byParent.get(null) ?? [])
}
