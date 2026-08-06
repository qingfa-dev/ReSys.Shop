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
  const filtered = taxonomyId
    ? taxons.filter(t => t.taxonomyId === taxonomyId)
    : taxons

  const byParent = new Map<string | null, StoreTaxonListItemResponse[]>()
  for (const t of filtered) {
    const key = t.parentId ?? null
    const list = byParent.get(key) ?? []
    list.push(t)
    byParent.set(key, list)
  }

  function sortSiblings(items: StoreTaxonListItemResponse[]): TaxonTreeNode[] {
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

  return sortSiblings(byParent.get(null) ?? [])
}
