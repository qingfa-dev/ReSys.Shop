<script setup lang="ts">
import { ref, watch, onMounted, watchEffect } from "vue";
import { useRouter, useRoute } from "vue-router";
import Select from "primevue/select";
import SelectButton from "primevue/selectbutton";
import Paginator from "primevue/paginator";
import Button from "primevue/button";
import Drawer from "primevue/drawer";
import Toolbar from "primevue/toolbar";
import { useCatalog } from "../composables/useCatalog";
import { useCart } from "@/features/ordering/composables/useCart";
import { useWishlistStore } from "../store/wishlist";
import SearchBar from "../components/search/SearchBar.vue";
import ShopFilters from "../components/search/ShopFilters.vue";
import ProductCard from "../components/product/ProductCard.vue";
import type { Product, ProductColor, ProductSize, ProductImage } from "../types";

function getProductImage(product: Product): string {
  const firstImage = product.images?.[0];
  if (!firstImage) return "";
  if (typeof firstImage === "string") return firstImage;
  return firstImage.url || "";
}

const mockColors: ProductColor[] = [
  { id: "col-black", name: "Black", hex: "#1a1a1a" },
  { id: "col-white", name: "White", hex: "#ffffff" },
  { id: "col-navy", name: "Navy", hex: "#1e3a5f" },
  { id: "col-gray", name: "Gray", hex: "#6b7280" },
  { id: "col-red", name: "Red", hex: "#dc2626" },
];

const mockSizes: ProductSize[] = [
  { id: "sz-xs", name: "XS", stock: 10 },
  { id: "sz-s", name: "S", stock: 15 },
  { id: "sz-m", name: "M", stock: 20 },
  { id: "sz-l", name: "L", stock: 15 },
  { id: "sz-xl", name: "XL", stock: 8 },
];

const mockBrands = [
  { name: "ReSyShop Originals", slug: "resyshop-originals" },
  { name: "Premium Basics", slug: "premium-basics" },
  { name: "Urban Style", slug: "urban-style" },
  { name: "Eco Wear", slug: "eco-wear" },
];

function getProductColors(product: Product): ProductColor[] {
  return mockColors.slice(0, 3 + Math.floor(Math.random() * 3));
}

function getProductSizes(product: Product): ProductSize[] {
  return mockSizes.slice(0, 3 + Math.floor(Math.random() * 3));
}

const router = useRouter();
const route = useRoute();

const {
  products,
  loading,
  error,
  pagination,
  loadProducts,
  loadCategories,
  categories,
  search,
  sortBy,
  filterByCategory,
  filterByPrice,
  filterBySize,
  filterByColor,
  filterByBrand,
  clearFilters,
  goToPage,
} = useCatalog();

const { addToCart: addToCartStore } = useCart();
const wishlistStore = useWishlistStore();

const sortOptions = ref([
  { label: "Newest", value: "newest" },
  { label: "Price: Low to High", value: "price-asc" },
  { label: "Price: High to Low", value: "price-desc" },
]);

const selectedSort = ref("newest");
const searchQuery = ref(route.query.q ? String(route.query.q) : "");
const viewMode = ref<"grid" | "list">("grid");
const selectedCategory = ref<string | null>(null);
const showFilters = ref(true);
const filterDrawerOpen = ref(false);
const priceMin = ref<number | null>(null);
const priceMax = ref<number | null>(null);

const isMobile = ref(false);

function checkMobile() {
  isMobile.value = window.innerWidth < 1024;
}

onMounted(() => {
  checkMobile();
  window.addEventListener("resize", checkMobile);
  loadCategories();
  loadProducts();
});

watch(isMobile, (mobile) => {
  if (!mobile) {
    showFilters.value = true;
    filterDrawerOpen.value = false;
  }
});

function toggleFilters() {
  if (isMobile.value) {
    filterDrawerOpen.value = !filterDrawerOpen.value;
  } else {
    showFilters.value = !showFilters.value;
  }
}

let priceDebounceTimer: ReturnType<typeof setTimeout> | null = null;

function debounce<T extends (...args: any[]) => any>(fn: T, delay: number): T {
  return ((...args: Parameters<T>) => {
    if (priceDebounceTimer) clearTimeout(priceDebounceTimer);
    priceDebounceTimer = setTimeout(() => fn(...args), delay);
  }) as T;
}

const debouncedPriceFilter = debounce(() => {
  if (priceMin.value !== null || priceMax.value !== null) {
    filterByPrice(priceMin.value || 0, priceMax.value || 999999);
  }
}, 300);

onMounted(async () => {
  await loadCategories();
  if (searchQuery.value) {
    search(searchQuery.value);
  } else {
    await loadProducts();
  }
});

watch(
  () => route.query.q,
  (newQuery) => {
    const q = newQuery ? String(newQuery) : "";
    searchQuery.value = q;
    if (q) {
      search(q);
    } else {
      loadProducts();
    }
  },
);

watch([priceMin, priceMax], () => {
  debouncedPriceFilter();
});

watch(selectedSort, (sort) => {
  sortBy(sort as "newest" | "price-asc" | "price-desc");
});

watch(selectedCategory, (category) => {
  if (category) {
    filterByCategory(category);
  } else {
    clearFilters();
  }
});

function handleSearch(query: string) {
  searchQuery.value = query;
  if (query) {
    search(query);
  } else {
    loadProducts();
  }
}

function handleFilterChange(filters: any) {
  if (filters.category) {
    selectedCategory.value = filters.category;
  }
  if (filters.priceMin !== null || filters.priceMax !== null) {
    priceMin.value = filters.priceMin;
    priceMax.value = filters.priceMax;
    filterByPrice(filters.priceMin || 0, filters.priceMax || 999999);
  }
  if (filters.sizes.length > 0) {
    filters.sizes.forEach((size: string) => filterBySize(size));
  }
  if (filters.colors.length > 0) {
    filters.colors.forEach((color: string) => filterByColor(color));
  }
  if (filters.brands.length > 0) {
    filters.brands.forEach((brand: string) => filterByBrand(brand));
  }
  loadProducts();
}

function handleClearFilters() {
  selectedCategory.value = null;
  priceMin.value = null;
  priceMax.value = null;
  clearFilters();
  loadProducts();
}

function onPageChange(event: { page: number }) {
  goToPage(event.page + 1);
}

async function handleAddToCart(product: Product, colorId?: string, sizeId?: string) {
  const image = getProductImage(product);
  const selectedOptions = [colorId, sizeId].filter(Boolean).join(" / ");
  const variantLabel = selectedOptions ? ` (${selectedOptions})` : "";
  await addToCartStore(product.id, product.name + variantLabel, image, 1, product.price);
}

function handleAddToWishlist(product: Product) {
  wishlistStore.toggle({
    id: `temp-${product.id}`,
    productId: product.id,
    name: product.name,
    slug: product.slug,
    price: product.price,
    compareAtPrice: product.compareAtPrice,
    image: getProductImage(product),
    brand: {
      id: product.category?.id || "unknown",
      name: product.category?.name || "Unknown",
      slug: product.category?.slug || "unknown",
    },
    addedAt: new Date().toISOString(),
  });
}

function handleProductClick(product: Product) {
  router.push({ name: "product-detail", params: { id: product.id } });
}
</script>

<template>
  <div class="shop-view">
    <div class="shop-header">
      <h1>Shop All</h1>
      <p>Discover our complete collection</p>
    </div>

    <Toolbar class="shop-toolbar">
      <template #start>
        <Button
          label="Filters"
          icon="pi pi-sliders-h"
          :outlined="!showFilters"
          @click="toggleFilters"
          class="filter-toggle-btn"
        />
      </template>

      <template #center>
        <SearchBar :placeholder="'Search products...'" @search="handleSearch" />
      </template>

      <template #end>
        <div class="toolbar-filters">
          <Select
            v-model="selectedCategory"
            :options="[{ name: 'All', slug: '' }, ...categories]"
            optionLabel="name"
            optionValue="slug"
            placeholder="Category"
            class="toolbar-select"
          />

          <Select
            v-model="selectedSort"
            :options="sortOptions"
            optionLabel="label"
            optionValue="value"
            placeholder="Sort"
            class="toolbar-select"
          />

          <SelectButton
            v-model="viewMode"
            :options="[
              { label: 'Grid', value: 'grid', icon: 'pi pi-th-large' },
              { label: 'List', value: 'list', icon: 'pi pi-list' },
            ]"
            optionLabel="label"
            optionValue="value"
          />
        </div>
      </template>
    </Toolbar>

    <div v-if="error" class="error-message">
      <i class="pi pi-exclamation-circle"></i>
      <span>{{ error }}</span>
      <button @click="loadProducts()">Retry</button>
    </div>

    <div class="shop-content" :class="{ 'filters-hidden': !showFilters }">
      <ShopFilters
        v-if="!isMobile && showFilters"
        :categories="categories"
        :colors="mockColors"
        :sizes="mockSizes"
        :brands="mockBrands"
        @filter-change="handleFilterChange"
        @clear="handleClearFilters"
      />

      <Drawer
        v-model:visible="filterDrawerOpen"
        position="left"
        :header="'Filters'"
        class="filter-drawer"
      >
        <ShopFilters
          :categories="categories"
          :colors="mockColors"
          :sizes="mockSizes"
          :brands="mockBrands"
          @filter-change="
            (filters) => {
              handleFilterChange(filters);
              filterDrawerOpen = false;
            }
          "
          @clear="handleClearFilters"
        />
      </Drawer>

      <main class="shop-main">
        <div v-if="loading" class="loading-state">
          <i class="pi pi-spin pi-spinner"></i>
          <span>Loading products...</span>
        </div>

        <div v-else-if="products.length === 0" class="empty-state">
          <i class="pi pi-inbox"></i>
          <h3>No products found</h3>
          <p>Try adjusting your search or filters</p>
          <button @click="handleClearFilters">Clear Filters</button>
        </div>

        <template v-else>
          <div class="product-grid" :class="{ 'list-view': viewMode === 'list' }">
            <ProductCard
              v-for="product in products"
              :key="product.id"
              :product="product"
              :variant="viewMode"
              :show-actions="true"
              :colors="getProductColors(product)"
              :sizes="getProductSizes(product)"
              @add-to-cart="handleAddToCart"
              @add-to-wishlist="handleAddToWishlist"
              @click="handleProductClick"
            />
          </div>

          <Paginator
            :first="(pagination.page - 1) * pagination.pageSize"
            :rows="pagination.pageSize"
            :totalRecords="pagination.total"
            :rowsPerPageOptions="[12, 24, 48]"
            @page="onPageChange"
            class="shop-paginator"
          />
        </template>
      </main>
    </div>
  </div>
</template>

<style scoped lang="scss">
.shop-view {
  max-width: 1400px;
  margin: 0 auto;
  padding: 2rem;
}

.shop-header {
  text-align: center;
  padding: 2rem 0;

  h1 {
    font-size: var(--font-size-4xl);
    margin-bottom: 0.5rem;
  }

  p {
    color: var(--color-text-muted);
    font-size: var(--font-size-lg);
  }
}

.shop-toolbar {
  border-bottom: 1px solid var(--color-border-light);
  margin-bottom: 1.5rem;
  padding: 0;
  background: var(--color-surface);
  
  :deep(.p-toolbar-content) {
    padding: 1rem 0;
    gap: 1rem;
  }
  
  :deep(.p-toolbar-start) {
    flex: 0 0 auto;
  }
  
  :deep(.p-toolbar-center) {
    flex: 1 1 auto;
    justify-content: center;
  }
  
  :deep(.p-toolbar-end) {
    flex: 0 0 auto;
  }
}

.toolbar-filters {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.filter-toggle-btn {
  // Always visible
}

.filter-drawer {
  :deep(.p-drawer) {
    width: 300px;
    max-width: 85vw;
  }
}

.price-range {
  display: flex;
  align-items: center;
  gap: 0.25rem;

  .price-input {
    width: 60px;
    padding: 0.375rem 0.5rem;
    border: 1px solid var(--color-border);
    border-radius: var(--radius-md);
    font-size: var(--font-size-sm);

    &:focus {
      outline: none;
      border-color: var(--color-primary);
    }
  }

  span {
    color: var(--color-text-muted);
    font-size: var(--font-size-sm);
  }
}

.toolbar-select {
  min-width: 130px;
}

.error-message {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 1rem;
  background: var(--color-surface);
  border: 1px solid var(--color-danger);
  border-radius: var(--radius-md);
  margin-bottom: 2rem;
  color: var(--color-danger);

  button {
    margin-left: auto;
    padding: 0.5rem 1rem;
    background: var(--color-danger);
    color: white;
    border: none;
    border-radius: var(--radius-md);
    cursor: pointer;
  }
}

.shop-content {
  display: grid;
  grid-template-columns: 260px 1fr;
  gap: 2rem;
  align-items: start;
  overflow-x: hidden;
  transition: grid-template-columns 0.3s ease;

  @media (max-width: 1024px) {
    grid-template-columns: 1fr;
  }
}

.shop-content.filters-hidden {
  grid-template-columns: 1fr;
}

.shop-main {
  width: 100%;
}

.loading-state,
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 2rem;
  text-align: center;

  i {
    font-size: 3rem;
    color: var(--color-text-muted);
    margin-bottom: 1rem;
  }

  h3 {
    font-size: var(--font-size-xl);
    margin-bottom: 0.5rem;
  }

  p {
    color: var(--color-text-muted);
  }

  button {
    margin-top: 1rem;
    padding: 0.75rem 1.5rem;
    background: var(--color-primary);
    color: white;
    border: none;
    border-radius: var(--radius-md);
    cursor: pointer;
  }
}

.product-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.5rem;

  @media (max-width: 1200px) {
    grid-template-columns: repeat(2, 1fr);
  }

  @media (max-width: 640px) {
    grid-template-columns: 1fr;
  }

  &.list-view {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
}

.shop-paginator {
  margin-top: 2rem;
  justify-content: center;
}
</style>
