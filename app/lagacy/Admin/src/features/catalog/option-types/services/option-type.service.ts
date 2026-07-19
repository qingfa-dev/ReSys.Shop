import { optionTypeRepository } from '../api/option-type.api'

export const optionTypeService = {
  list: optionTypeRepository.list,
  getById: optionTypeRepository.getById,
  create: optionTypeRepository.create,
  update: optionTypeRepository.update,
  delete: optionTypeRepository.delete,
}
