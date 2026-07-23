import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from '@/shared/composables/useToast'
import { TaxonomyApi } from '../api/taxonomy.api'
import { TaxonApi } from '../api/taxon.api'

export function useTaxonomy() {
  const toast = useToast()
  const route = useRoute()
  const router = useRouter()

  const id = computed(() => route.params.id as string | undefined)
  const mode = computed<'create' | 'view' | 'edit'>(() => {
    if (!id.value) return 'create'
    return route.name?.toString().endsWith('.edit') ? 'edit' : 'view'
  })

  return { id, mode, route, router, toast, api: TaxonomyApi, taxonApi: TaxonApi }
}
