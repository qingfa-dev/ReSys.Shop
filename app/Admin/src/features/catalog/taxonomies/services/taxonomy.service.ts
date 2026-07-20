import { taxonomyRepository } from '../api/taxonomy.api'

export const taxonomyService = {
  list: taxonomyRepository.list,
  getById: taxonomyRepository.getById,
  create: taxonomyRepository.create,
  update: taxonomyRepository.update,
  delete: taxonomyRepository.delete,
}
