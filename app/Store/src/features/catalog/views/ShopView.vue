<script setup lang="ts">
import { onMounted, ref, watch } from "vue";
import { useRoute } from "vue-router";
import { usePageTitle } from "@/shared/composables/usePageTitle";
import { useFilters } from "../composables/useFilters";
import { useTaxonomy } from "../composables/useTaxonomy";
import { useProducts } from "../composables/useProducts";
import ProductGridCard from "../components/ProductGridCard.vue";
import ProductListItem from "../components/ProductListItem.vue";
import ShopFilterPanel from "../components/ShopFilterPanel.vue";

usePageTitle("Shop");

const filters = useFilters();
const taxonomy = useTaxonomy();
const productList = useProducts();
const route = useRoute();

// Layout: Grid/list presentation toggle for the DataView
const layout = ref<"grid" | "list">("grid");
const layoutOptions = [
  { value: "grid", icon: "pi pi-th-large", label: "Grid view" },
  { value: "list", icon: "pi pi-bars", label: "List view" },
];

// Sort: Select options mirroring the backend's allowed sort fields
const sortOptions = [
  { value: "-CreatedAtUtc", label: "Newest" },
  { value: "CreatedAtUtc", label: "Oldest" },
  { value: "Name", label: "Name A-Z" },
  { value: "-Name", label: "Name Z-A" },
  { value: "Price", label: "Price: Low to High" },
  { value: "-Price", label: "Price: High to Low" },
];

// Mobile: Drawer visibility for the filter panel below lg
const filtersOpen = ref(false);

// Pagination: Zero-based first index for DataView from the 1-based composable page
const first = ref(0);

// Page: Forward DataView page event to the composable and refetch
function onPage(event: { page: number }): void {
  productList.goToPage(event.page + 1);
}

// Sync: Keep first index aligned with composable page
watch(
  () => productList.page,
  (page) => { first.value = (page - 1) * productList.pageSize; },
  { immediate: true },
);

// Restore: Pre-populate filters from ?taxon= and ?q= route query on mount
function applyRouteQuery(): void {
  const taxon = route.query.taxon;
  const ids = Array.isArray(taxon) ? taxon : taxon ? [taxon] : [];
  for (const id of ids) {
    if (id && !filters.selectedTaxonIds.includes(id)) filters.toggleTaxon(id);
  }
  const query = route.query.q;
  if (typeof query === "string" && query.length > 0) filters.setSearch(query);
}

// Watch: Re-apply route query filters on in-page navigations (e.g. category tag clicks)
watch(() => route.query, applyRouteQuery);

onMounted(() => {
  // Load: Taxonomy and option metadata — composables guard duplicate fetches
  void taxonomy.loadTaxonomyGroups();
  void taxonomy.loadOptionTypes();
  // Restore: Apply route query filters before the first fetch
  applyRouteQuery();
  // Fetch: Initial product page — skip if the home rail already loaded one
  if (productList.isInitialLoad) void productList.fetch();
  else void productList.refresh();
});
</script>

<template>
  <div class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
    <!-- Section: Catalog Grid — sticky filter aside and DataView product rail -->
    <div class="grid grid-cols-1 gap-8 lg:grid-cols-[16rem_1fr]">
      <!-- Aside: Filter panel sticky on desktop; hidden below lg -->
      <aside class="hidden self-start lg:block">
        <div class="sticky top-24 max-h-[calc(100vh-6rem)] overflow-y-auto pr-1">
          <ShopFilterPanel />
        </div>
      </aside>

      <!-- Main: DataView with header, grid/list slots, and built-in pagination -->
      <div class="min-w-0">
        <DataView
          :value="productList.items"
          :layout="layout"
          lazy
          paginator
          :rows="productList.pageSize"
          :first="first"
          :totalRecords="productList.totalCount"
          dataKey="id"
          @page="onPage"
        >
          <!-- Header: Toolbar with filter toggle, count, layout toggle, sort select -->
          <template #header>
            <Toolbar class="mb-6 border-none bg-transparent p-0">
              <template #start>
                <div class="flex items-center gap-3">
                  <Button
                    class="lg:hidden"
                    icon="pi pi-filter"
                    :badge="
                      filters.activeFilterCount > 0
                        ? String(filters.activeFilterCount)
                        : undefined
                    "
                    aria-label="Open filters"
                    @click="filtersOpen = true"
                  />
                  <Tag :value="`${productList.totalCount} products`" severity="secondary" />
                </div>
              </template>
              <template #end>
                <div class="flex items-center gap-2">
                  <SelectButton
                    v-model="layout"
                    :options="layoutOptions"
                    optionLabel="label"
                    optionValue="value"
                    :allowEmpty="false"
                    aria-label="View layout"
                  >
                    <template #option="{ option }">
                      <i :class="option.icon" :aria-label="option.label" />
                    </template>
                  </SelectButton>
                  <Select
                    :modelValue="filters.sortField"
                    :options="sortOptions"
                    optionLabel="label"
                    optionValue="value"
                    class="w-48"
                    aria-label="Sort products"
                    @change="filters.setSort($event.value)"
                  />
                </div>
              </template>
            </Toolbar>
          </template>

          <!-- Grid: Product cards in responsive grid layout -->
          <template #grid="{ items }">
            <!-- Loading: Skeleton cards while the first page fetches -->
            <div v-if="productList.loading && items.length === 0" class="grid grid-cols-2 gap-4 lg:grid-cols-3 xl:grid-cols-4">
              <div v-for="n in 8" :key="n" class="space-y-3">
                <Skeleton class="aspect-square w-full rounded-xl" />
                <Skeleton width="70%" height="1rem" />
                <Skeleton width="40%" height="1rem" />
              </div>
            </div>
            <div v-else class="grid grid-cols-2 gap-4 lg:grid-cols-3 xl:grid-cols-4">
              <ProductGridCard v-for="product in items" :key="product.id" :product="product" />
            </div>
          </template>

          <!-- List: Product list items in single-column layout -->
          <template #list="{ items }">
            <!-- Loading: Skeleton cards while the first page fetches -->
            <div v-if="productList.loading && items.length === 0" class="flex flex-col gap-4">
              <div v-for="n in 8" :key="n" class="flex gap-4 rounded-lg border border-surface-200 p-4">
                <Skeleton class="h-32 w-32 shrink-0 rounded-lg sm:h-40 sm:w-40" />
                <div class="flex flex-1 flex-col justify-center gap-3">
                  <Skeleton width="70%" height="1.25rem" />
                  <Skeleton width="50%" height="0.875rem" />
                  <Skeleton width="30%" height="1rem" />
                </div>
              </div>
            </div>
            <div v-else class="flex flex-col gap-3">
              <ProductListItem v-for="product in items" :key="product.id" :product="product" />
            </div>
          </template>

          <!-- Empty: No products match the active filters -->
          <template #empty>
            <div
              v-if="!productList.isInitialLoad"
              class="flex flex-col items-center gap-3 py-16 text-center"
            >
              <Message severity="warn" :closable="false">
                No products found. Try adjusting or clearing your filters.
              </Message>
              <Button label="Clear filters" variant="text" @click="filters.clearFilters()" />
            </div>
          </template>
        </DataView>
      </div>
    </div>

    <!-- Drawer: Same filter panel on mobile, opened from the toolbar toggle -->
    <Drawer v-model:visible="filtersOpen" position="left" header="Filters">
      <ShopFilterPanel />
    </Drawer>
  </div>
</template>
