<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { useRoute } from "vue-router";
import { usePageTitle } from "@/shared/composables/usePageTitle";
import { useFilters } from "../composables/useFilters";
import { useTaxonomy } from "../composables/useTaxonomy";
import { useProducts } from "../composables/useProducts";
import ProductCard from "../components/ProductCard.vue";
import ShopFilterPanel from "../components/ShopFilterPanel.vue";
import type { PageState } from "primevue/paginator";

usePageTitle("Shop");

const filters = useFilters();
const taxonomy = useTaxonomy();
const productList = useProducts();
const route = useRoute();

// Layout: Grid/list presentation toggle for the product rail
const layout = ref<"grid" | "list">("grid");
const layoutOptions = [
  { value: "grid", icon: "pi pi-th-large", label: "Grid view" },
  { value: "list", icon: "pi pi-bars", label: "List view" },
];

// Grid: Column density per layout mode — list mode shows one wide card per row
const gridClass = computed(() =>
  layout.value === "grid"
    ? "grid grid-cols-2 gap-4 lg:grid-cols-3 xl:grid-cols-4"
    : "grid grid-cols-1 gap-4",
);

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

// Pagination: Zero-based first index for the Paginator from the 1-based store page
const first = computed(() => (productList.page - 1) * productList.pageSize);

// Page: Forward Paginator page state to the composable and refetch
function onPage(event: PageState): void {
  productList.goToPage(event.page + 1);
}

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
    <!-- Section: Catalog Grid — sticky filter aside and product rail -->
    <div class="grid grid-cols-1 gap-8 lg:grid-cols-[16rem_1fr]">
      <!-- Aside: Filter panel sticky on desktop; hidden below lg -->
      <aside class="hidden self-start lg:block">
        <div class="sticky top-24 max-h-[calc(100vh-6rem)] overflow-y-auto pr-1">
          <ShopFilterPanel />
        </div>
      </aside>

      <!-- Main: Toolbar, product grid, empty state and pagination -->
      <div class="min-w-0">
        <Toolbar class="mb-6 border-none bg-transparent p-0">
          <template #start>
            <div class="flex items-center gap-3">
              <!-- Toggle: Open the filter drawer on mobile -->
              <Button
                class="lg:hidden"
                icon="pi pi-filter"
                :badge="
                  filters.activeFilterCount > 0 ? String(filters.activeFilterCount) : undefined
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

        <!-- Loading: Skeleton cards while the first page fetches -->
        <div v-if="productList.loading && productList.items.length === 0" :class="gridClass">
          <div v-for="n in 8" :key="n" class="space-y-3">
            <Skeleton class="aspect-square w-full rounded-xl" />
            <Skeleton width="70%" height="1rem" />
            <Skeleton width="40%" height="1rem" />
          </div>
        </div>

        <!-- Grid: Product cards in the active layout mode -->
        <div v-else-if="productList.items.length > 0" :class="gridClass">
          <ProductCard v-for="product in productList.items" :key="product.id" :product="product" />
        </div>

        <!-- Empty State: No products match the active filters -->
        <div
          v-else-if="!productList.isInitialLoad"
          class="flex flex-col items-center gap-3 py-16 text-center"
        >
          <Message severity="warn" :closable="false">
            No products found. Try adjusting or clearing your filters.
          </Message>
          <Button label="Clear filters" variant="text" @click="filters.clearFilters()" />
        </div>

        <!-- Pagination: Bound to the product list store paging state -->
        <Paginator
          v-if="productList.totalCount > 0"
          class="mt-8"
          :rows="productList.pageSize"
          :totalRecords="productList.totalCount"
          :first="first"
          @page="onPage"
        />
      </div>
    </div>

    <!-- Drawer: Same filter panel on mobile, opened from the toolbar toggle -->
    <Drawer v-model:visible="filtersOpen" position="left" header="Filters">
      <ShopFilterPanel />
    </Drawer>
  </div>
</template>
