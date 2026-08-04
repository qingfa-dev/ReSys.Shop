import { ENDPOINTS } from '@/shared/constants/api'
import { env } from '@config/env'

export function getImageUrl(imageId: string): string {
  return `${env.apiUrl}/${ENDPOINTS.images}/${imageId}`
}
