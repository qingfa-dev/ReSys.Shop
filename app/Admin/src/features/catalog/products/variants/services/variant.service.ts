import { variantRepository } from '../api/variant.api'

export const variantService = {
    getById: variantRepository.getById,
    listByProductId: variantRepository.listByProductId,
    create: variantRepository.create,
    update: variantRepository.update,
    delete: variantRepository.delete,
    updateOptionValues: variantRepository.syncOptionValues,
}
