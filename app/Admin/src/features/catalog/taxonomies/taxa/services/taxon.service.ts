import { taxonRepository } from '../../../repository/taxon.repository'
import type { ServerResult } from '@/shared/api/types/result.types'

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
  getProductPreview: async (_taxonId: string, _params: Record<string, unknown>): Promise<ServerResult<{ items: any[]; total_count: number }>> => ({
    isSuccess: true,
    statusCode: 200,
    errors: [],
    message: null,
    metadata: null,
    value: { items: [], total_count: 0 },
  }),
}
