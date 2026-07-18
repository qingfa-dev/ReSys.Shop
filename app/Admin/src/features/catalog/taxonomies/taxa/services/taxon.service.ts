import { taxonRepository } from '../repositories/taxon.repository'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { ProductSummary } from '@/features/catalog/products/types/Product.Response.Type'

export const taxonService = {
  list: taxonRepository.listByTaxonomyId,
  getTree: taxonRepository.getTree,
  getById: taxonRepository.getById,
  create: taxonRepository.create,
  update: taxonRepository.update,
  delete: taxonRepository.delete,
  reposition: taxonRepository.reposition,
  restore: taxonRepository.restore,
  getRules: taxonRepository.listRules,
  addRule: taxonRepository.createRule,
  updateRule: taxonRepository.updateRule,
  deleteRule: taxonRepository.deleteRule,
  syncRules: taxonRepository.syncRules,
  regenerateProducts: taxonRepository.regenerateProducts,
  getProductPreview: async (_taxonId: string, _params: Record<string, unknown>): Promise<ServerResult<{ items: ProductSummary[]; totalCount: number }>> => ({
    isSuccess: true,
    statusCode: 200,
    errors: [],
    message: null,
    metadata: null,
    value: { items: [], totalCount: 0 },
  }),
}
