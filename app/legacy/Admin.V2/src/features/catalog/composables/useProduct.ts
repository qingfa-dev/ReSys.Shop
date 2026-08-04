import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from '@/shared/composables/useToast'
import { ProductApi } from '../api/product.api'
import { ProductOptionTypeApi } from '../api/product-option-type.api'
import { ProductClassificationApi } from '../api/product-classification.api'

export function useProduct() {
  const toast = useToast()
  const route = useRoute()
  const router = useRouter()

  const id = computed(() => route.params.id as string | undefined)
  const mode = computed<'create' | 'view' | 'edit'>(() => {
    if (!id.value) return 'create'
    return route.name?.toString().endsWith('.edit') ? 'edit' : 'view'
  })

  return { id, mode, route, router, toast, api: ProductApi, optionTypeApi: ProductOptionTypeApi, classificationApi: ProductClassificationApi }
}
