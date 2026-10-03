import { ref } from "vue";
import type { ServerResult, ServerPagedResult } from "@/common/api/types/result.types";
import type { ServerQueryingParameters } from "@/common/api/types/query.types";

type PagedFetchResult<T> = ServerPagedResult<T> | ServerResult<T[]>;

function isPaged<T>(r: PagedFetchResult<T>): r is ServerPagedResult<T> {
  return "items" in r && "totalCount" in r;
}

export function usePagedList<
  TItem,
  TParams extends ServerQueryingParameters = ServerQueryingParameters,
>(
  fetchFn: (params: TParams) => Promise<PagedFetchResult<TItem>>,
  defaultParams?: Partial<TParams>,
) {
  const items = ref<TItem[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);
  const totalRecords = ref(0);
  const params = ref<TParams>({
    page: 1,
    pageSize: 10,
    ...defaultParams,
  } as TParams);

  async function fetch(overrides?: Partial<TParams>) {
    loading.value = true;
    error.value = null;

    if (overrides) {
      params.value = { ...params.value, ...overrides };
    }

    try {
      const result = await fetchFn(params.value);
      if (result.isSuccess) {
        if (isPaged(result)) {
          items.value = result.items;
          totalRecords.value = result.totalCount || 0;
        } else if (result.value) {
          items.value = result.value;
          totalRecords.value = result.value.length || 0;
        }
      } else {
        error.value = result.errors?.[0]?.message || "Failed to fetch";
      }
      return result;
    } catch {
      error.value = "An unexpected error occurred";
    } finally {
      loading.value = false;
    }
  }

  function setPage(page: number) {
    params.value.page = page;
    return fetch();
  }

  function setSort(sort: string[]) {
    params.value.sort = sort;
    return fetch();
  }

  function setSearch(search: string, searchFields?: string[]) {
    params.value.search = search;
    params.value.searchFields = searchFields;
    return fetch();
  }

  function refresh() {
    return fetch();
  }

  return {
    items,
    loading,
    error,
    totalRecords,
    params,
    fetch,
    setPage,
    setSort,
    setSearch,
    refresh,
  };
}
