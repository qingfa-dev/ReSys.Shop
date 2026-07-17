import { catalogApi } from '../../services/catalog.api'

export const variantService = {
    getById: catalogApi.variants.getById,
    listByProductId: catalogApi.variants.listByProductId,
    create: catalogApi.variants.create,
    update: catalogApi.variants.update,
    delete: catalogApi.variants.delete,
    updateOptionValues: catalogApi.variants.syncOptionValues,
}
