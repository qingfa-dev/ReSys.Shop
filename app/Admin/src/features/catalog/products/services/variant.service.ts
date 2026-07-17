import { catalogApi } from '../../services/catalog.api'

export const variantService = {
    list: catalogApi.variants.list,
    getById: catalogApi.variants.getById,
    create: catalogApi.variants.create,
    update: catalogApi.variants.update,
    delete: catalogApi.variants.delete,
    listByProductId: catalogApi.variants.listByProductId,
    setMaster: catalogApi.variants.setMaster,
    updateOptionValues: catalogApi.variants.updateOptionValues,
}
