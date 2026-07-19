import { imageApi } from '../api/image.api'

export const imageService = {
  listByVariant: imageApi.listByVariant,
  upload: imageApi.upload,
  update: imageApi.update,
  delete: imageApi.delete,
}
