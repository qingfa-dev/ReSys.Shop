<template>
  <div class="faceted-filters">
    <div class="filters-header">
      <h3>Filters</h3>
      <button v-if="hasActiveFilters" class="clear-all" @click="clearAllFilters">Clear All</button>
    </div>

    <!-- Price Range Filter -->
    <div class="filter-section">
      <h4 @click="toggleSection('price')" class="filter-title">
        <i class="pi" :class="expandedSections.price ? 'pi-chevron-down' : 'pi-chevron-right'"></i>
        Price Range
      </h4>
      <div v-if="expandedSections.price" class="filter-content">
        <div class="price-inputs">
          <input
            v-model.number="priceRange[0]"
            type="number"
            placeholder="Min"
            @change="applyPriceFilter"
            class="price-input"
          />
          <span>—</span>
          <input
            v-model.number="priceRange[1]"
            type="number"
            placeholder="Max"
            @change="applyPriceFilter"
            class="price-input"
          />
        </div>
      </div>
    </div>

    <!-- Size Filter -->
    <div v-if="facets?.sizes && facets.sizes.length" class="filter-section">
      <h4 @click="toggleSection('sizes')" class="filter-title">
        <i class="pi" :class="expandedSections.sizes ? 'pi-chevron-down' : 'pi-chevron-right'"></i>
        Sizes ({{ facets.sizes.length }})
      </h4>
      <div v-if="expandedSections.sizes" class="filter-content">
        <div class="size-options">
          <label v-for="sizeOption in facets.sizes" :key="sizeOption.name">
            <input
              type="checkbox"
              :checked="isFilterActive('sizes', sizeOption.name)"
              @change="toggleSizeFilter(sizeOption.name)"
            />
            <span>{{ sizeOption.name }} ({{ sizeOption.count }})</span>
          </label>
        </div>
      </div>
    </div>

    <!-- Color Filter -->
    <div v-if="facets?.colors && facets.colors.length" class="filter-section">
      <h4 @click="toggleSection('colors')" class="filter-title">
        <i class="pi" :class="expandedSections.colors ? 'pi-chevron-down' : 'pi-chevron-right'"></i>
        Colors ({{ facets.colors.length }})
      </h4>
      <div v-if="expandedSections.colors" class="filter-content">
        <div class="color-options">
          <label v-for="colorOption in facets.colors" :key="colorOption.name">
            <input
              type="checkbox"
              :checked="isFilterActive('colors', colorOption.name)"
              @change="toggleColorFilter(colorOption.name)"
            />
            <span>{{ colorOption.name }} ({{ colorOption.count }})</span>
          </label>
        </div>
      </div>
    </div>

    <!-- Brand Filter -->
    <div v-if="facets?.brands && facets.brands.length" class="filter-section">
      <h4 @click="toggleSection('brands')" class="filter-title">
        <i class="pi" :class="expandedSections.brands ? 'pi-chevron-down' : 'pi-chevron-right'"></i>
        Brands ({{ facets.brands.length }})
      </h4>
      <div v-if="expandedSections.brands" class="filter-content">
        <div class="brand-options">
          <label v-for="brandOption in facets.brands.slice(0, 5)" :key="brandOption.name">
            <input
              type="checkbox"
              :checked="isFilterActive('brands', brandOption.name)"
              @change="toggleBrandFilter(brandOption.name)"
            />
            <span>{{ brandOption.name }} ({{ brandOption.count }})</span>
          </label>
          <button v-if="facets.brands.length > 5" class="show-more">
            Show {{ facets.brands.length - 5 }} more
          </button>
        </div>
      </div>
    </div>

    <!-- Rating Filter -->
    <div class="filter-section">
      <h4 @click="toggleSection('rating')" class="filter-title">
        <i class="pi" :class="expandedSections.rating ? 'pi-chevron-down' : 'pi-chevron-right'"></i>
        Rating
      </h4>
      <div v-if="expandedSections.rating" class="filter-content">
        <div class="rating-options">
          <label v-for="rating in [5, 4, 3, 2, 1]" :key="rating">
            <input type="radio" name="rating" @change="applyRatingFilter(rating)" />
            <span>
              <span v-for="i in 5" :key="i" class="star" :class="{ filled: i <= rating }">★</span>
              & Up
            </span>
          </label>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import { useSearch } from "../composables/useSearch";

const { currentFilters, updateFilter, clearFilters: clearAllFiltersStore } = useSearch();

const expandedSections = ref({
  price: false,
  sizes: true,
  colors: true,
  brands: true,
  rating: false,
});

const priceRange = ref<[number, number]>([0, 1000]);

const facets = ref({
  brands: [
    { name: "Brand A", count: 45 },
    { name: "Brand B", count: 32 },
    { name: "Brand C", count: 28 },
    { name: "Brand D", count: 19 },
    { name: "Brand E", count: 15 },
    { name: "Brand F", count: 12 },
  ],
  colors: [
    { name: "Black", count: 120 },
    { name: "White", count: 98 },
    { name: "Navy", count: 87 },
    { name: "Red", count: 45 },
  ],
  sizes: [
    { name: "XS", count: 24 },
    { name: "S", count: 56 },
    { name: "M", count: 89 },
    { name: "L", count: 75 },
    { name: "XL", count: 42 },
  ],
});

const hasActiveFilters = computed(() => Object.keys(currentFilters).length > 0);

function toggleSection(section: string) {
  expandedSections.value[section as keyof typeof expandedSections.value] =
    !expandedSections.value[section as keyof typeof expandedSections.value];
}

function isFilterActive(type: string, value: string): boolean {
  const filters = currentFilters;
  if (type === "brands") {
    return filters.brand?.includes(value) || false;
  }
  if (type === "sizes") {
    return filters.sizes?.includes(value) || false;
  }
  if (type === "colors") {
    return filters.colors?.includes(value) || false;
  }
  return false;
}

function toggleSizeFilter(size: string) {
  const current = currentFilters.sizes || [];
  const updated = current.includes(size)
    ? current.filter((s: string) => s !== size)
    : [...current, size];
  updateFilter("sizes", updated.length > 0 ? updated : undefined);
}

function toggleColorFilter(color: string) {
  const current = currentFilters.colors || [];
  const updated = current.includes(color)
    ? current.filter((c: string) => c !== color)
    : [...current, color];
  updateFilter("colors", updated.length > 0 ? updated : undefined);
}

function toggleBrandFilter(brand: string) {
  const current = currentFilters.brand || [];
  const updated = current.includes(brand)
    ? current.filter((b: string) => b !== brand)
    : [...current, brand];
  updateFilter("brand", updated.length > 0 ? updated : undefined);
}

function applyPriceFilter() {
  updateFilter("priceRange", priceRange.value);
}

function applyRatingFilter(rating: number) {
  updateFilter("rating", rating);
}

function clearAllFilters() {
  clearAllFiltersStore();
  priceRange.value = [0, 1000];
}
</script>

<style scoped lang="scss">
.faceted-filters {
  background: var(--color-surface);
  border-radius: var(--radius-lg);
  padding: 1.5rem;

  .filters-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;
    padding-bottom: 1rem;
    border-bottom: 2px solid var(--color-border-light);

    h3 {
      margin: 0;
      font-size: var(--font-size-lg);
    }

    .clear-all {
      background: none;
      border: none;
      color: var(--color-primary);
      cursor: pointer;
      text-decoration: underline;
      font-size: var(--font-size-sm);

      &:hover {
        text-decoration: none;
      }
    }
  }

  .filter-section {
    margin-bottom: 1.5rem;

    .filter-title {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin: 0;
      font-size: var(--font-size-base);
      font-weight: var(--font-weight-medium);
      cursor: pointer;
      padding: 0.5rem 0;
      user-select: none;

      &:hover {
        color: var(--color-primary);
      }

      i {
        font-size: var(--font-size-sm);
        transition: transform var(--transition-fast);
      }
    }

    .filter-content {
      margin-top: 0.75rem;
      padding-left: 1.5rem;
      animation: slideDown 0.2s ease;

      @keyframes slideDown {
        from {
          opacity: 0;
          transform: translateY(-10px);
        }
        to {
          opacity: 1;
          transform: translateY(0);
        }
      }
    }
  }

  .price-inputs {
    display: flex;
    align-items: center;
    gap: 0.5rem;

    .price-input {
      flex: 1;
      padding: 0.5rem;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      font-size: var(--font-size-sm);

      &:focus {
        outline: none;
        border-color: var(--color-primary);
      }
    }
  }

  .size-options,
  .color-options,
  .brand-options,
  .rating-options {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;

    label {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      cursor: pointer;
      font-size: var(--font-size-sm);

      input {
        cursor: pointer;
      }

      &:hover {
        color: var(--color-primary);
      }

      .star {
        font-size: 0.75rem;
        color: #ddd;

        &.filled {
          color: #ffc107;
        }
      }
    }

    .show-more {
      background: none;
      border: none;
      color: var(--color-primary);
      cursor: pointer;
      text-align: left;
      font-size: var(--font-size-sm);
      padding: 0.5rem 0;

      &:hover {
        text-decoration: underline;
      }
    }
  }
}
</style>
