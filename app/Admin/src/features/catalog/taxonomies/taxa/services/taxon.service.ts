import { taxonRepository } from '../api/taxon.api'

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
  getProductPreview: taxonRepository.getProductPreview,
}
