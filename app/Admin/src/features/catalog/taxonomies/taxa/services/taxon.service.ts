import { catalogApi } from '../../../services/catalog.api'

export const taxonService = {
  list: catalogApi.taxonomies.listTaxons,
  getTree: catalogApi.taxonomies.getTaxonTree,
  getById: catalogApi.taxonomies.getTaxonById,
  create: catalogApi.taxonomies.createTaxon,
  update: catalogApi.taxonomies.updateTaxon,
  delete: catalogApi.taxonomies.deleteTaxon,
  reposition: catalogApi.taxonomies.repositionTaxon,
  restore: catalogApi.taxonomies.restoreTaxon,
  getRules: catalogApi.taxonomies.listTaxonRules,
  addRule: catalogApi.taxonomies.createTaxonRule,
  updateRule: catalogApi.taxonomies.updateTaxonRule,
  deleteRule: catalogApi.taxonomies.deleteTaxonRule,
  syncRules: catalogApi.taxonomies.syncTaxonRules,
  regenerateProducts: catalogApi.taxonomies.regenerateTaxonProducts,
  getProductPreview: async (_taxonId: string, _params: Record<string, unknown>) => ({
    success: true as const,
    data: { items: [], total_count: 0 },
  }),
}
