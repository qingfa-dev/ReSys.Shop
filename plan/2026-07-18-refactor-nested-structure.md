# Nested SPA Feature Structure Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restructure `app/Admin/src/features/` to mirror backend `Features/Admin/` nesting, extracting sub-features (prices, images, classifications, option-types under products) and correcting all `ServerResult`/`ServerPagedResult` types per backend endpoint audit.

**Architecture:** Each backend entity hierarchy becomes a nested directory tree. Each sub-feature gets the full pipeline: `schemas/ → types/ → api/ → services/ → stores/ → components/ → views/`. Return types are `ServerResult<T>` for single-object endpoints, `ServerPagedResult<T>` for paginated-list endpoints.

**Tech Stack:** Vue 3 + TypeScript 6, PrimeVue, Pinia, vee-validate + zod, Axios. Files are `.ts` (api/services/stores/types/schemas) and `.vue` (components/views).

## Global Constraints

- All API files use `apiClient` from `@/shared/api/http/api.client` (Axios instance)
- All result types from `@/shared/api/types/result.types` — `ServerResult<T>` (has `.value`) vs `ServerPagedResult<T>` (has `.items`, `.page`, `.pageSize`, `.totalCount`)
- All type files use `.ts` with PascalCase names and `.Type.ts` suffix convention
- All schema files use `.ts` with `.Schema.ts` suffix, zod-based
- All Vue components use `.Component.vue` suffix; views use `.View.vue` suffix
- Use `git mv` to preserve file history — never `mv` + re-add
- After every task, run `vue-tsc --build` to confirm zero type errors
- Commit after each task with conventional commit format

---
## File Structure

### Target structure after all phases (catalog/products shown as example):

```
features/catalog/
├── catalog.routes.ts
├── _tests/catalog.api.spec.ts
├── dashboard/
│   ├── services/catalog-dashboard.service.ts
│   ├── stores/catalog-dashboard.store.ts
│   ├── types/catalog-dashboard.types.ts
│   └── views/CatalogDashboard.View.vue
├── option-types/
│   ├── api/option-type.api.ts
│   ├── schemas/OptionType.Schema.ts
│   ├── services/option-type.service.ts
│   ├── stores/option-type.store.ts
│   ├── types/{Parameters,Query,Request,Response}.Type.ts
│   ├── views/{Form,List,Manager}.View.vue
│   ├── tests/{schema,store}.spec.ts
│   └── option-values/             (nested sub-feature)
│       ├── api/option-value.api.ts
│       └── ...
├── products/
│   ├── api/product.api.ts         (product CRUD + activate/discontinue)
│   ├── schemas/{Create,Update}Product.Schema.ts
│   ├── services/product.service.ts
│   ├── stores/product.store.ts
│   ├── types/{Parameters,Query,Request,Response}.Type.ts
│   ├── views/{Form,List}.View.vue
│   ├── components/                (product-level components only)
│   ├── tests/product.store.spec.ts
│   ├── classifications/           (Products/Classifications)
│   │   ├── api/product-classification.api.ts
│   │   ├── schemas/ProductClassification.Schema.ts
│   │   ├── services/classification.service.ts
│   │   ├── types/
│   │   └── components/ProductClassificationManager.Component.vue
│   ├── option-types/              (Products/OptionTypes)
│   │   ├── api/product-option-type.api.ts
│   │   ├── services/product-option-type.service.ts
│   │   ├── types/
│   │   └── components/ProductOptionTypeManager.Component.vue
│   └── variants/                  (Products/Variants)
│       ├── api/variant.api.ts     (variant CRUD only)
│       ├── schemas/Variant.Schema.ts
│       ├── services/variant.service.ts
│       ├── types/{Parameters,Query,Request,Response}.Type.ts
│       ├── components/
│       │   ├── ProductVariantManager.Component.vue
│       │   ├── VariantFormDialog.Component.vue
│       │   ├── VariantGenerationDialog.Component.vue
│       │   ├── ProductImageManager.Component.vue
│       │   ├── ProductInventoryManager.Component.vue
│       │   └── images/
│       │       ├── ProductImageList.Component.vue
│       │       └── ProductImageUploader.Component.vue
│       ├── prices/                 (Products/Variants/Prices)
│       │   ├── api/price.api.ts
│       │   ├── services/price.service.ts
│       │   ├── types/Price.Response.Type.ts
│       │   └── components/
│       └── images/                 (Products/Variants/Images)
│           ├── api/image.api.ts
│           ├── services/image.service.ts
│           ├── types/Image.Response.Type.ts
│           └── components/
├── taxonomies/
│   ├── api/taxonomy.api.ts
│   ├── ...
│   └── taxa/                      (Taxonomies/Taxons)
│       ├── api/taxon.api.ts
│       ├── ...
│       └── rules/                 (Taxonomies/Taxons/Rules)
│           └── api/taxon-rule.api.ts (inline in taxon.api currently)
└── ── (end)
```

### Files to create new:

| File | Purpose |
|------|---------|
| `products/classifications/api/product-classification.api.ts` | Extract from `product.api.ts` — `getClassifications`, `syncClassifications` |
| `products/classifications/services/classification.service.ts` | Service wrapper |
| `products/option-types/api/product-option-type.api.ts` | Extract from `product.api.ts` — `getOptionTypes`, `syncOptionTypes` |
| `products/option-types/services/product-option-type.service.ts` | Service wrapper |
| `products/variants/prices/api/price.api.ts` | Extract from `variant.api.ts` — `listPrices`, `setPrice`, `deletePrice`, `syncPrices` |
| `products/variants/prices/types/Price.Response.Type.ts` | `PriceRecord` interface |
| `products/variants/prices/services/price.service.ts` | Service wrapper |
| `products/variants/images/api/image.api.ts` | Extract from `variant.api.ts` — `listByVariant`, `upload`, `update`, `delete` |
| `products/variants/images/types/Image.Response.Type.ts` | `VariantImage` interface |
| `products/variants/images/services/image.service.ts` | Service wrapper |
| `profile/addresses/api/address.api.ts` | New scaffolding for Profile/Addresses sub-feature |
| `profile/addresses/types/Address.Response.Type.ts` | `AddressDetail` interface |
| `profile/addresses/services/address.service.ts` | Service wrapper |

### Files to move (git mv) — complete list:

**Phase 1 — Variant types/schemas/components into `products/variants/`:**
- `products/types/Variant.Parameters.Type.ts` → `products/variants/types/`
- `products/types/Variant.Query.Type.ts` → `products/variants/types/`
- `products/types/Variant.Request.Type.ts` → `products/variants/types/`
- `products/types/Variant.Response.Type.ts` → `products/variants/types/`
- `products/schemas/Variant.Schema.ts` → `products/variants/schemas/`
- `products/components/ProductVariantManager.Component.vue` → `products/variants/components/`
- `products/components/VariantFormDialog.Component.vue` → `products/variants/components/`
- `products/components/dialogs/VariantGenerationDialog.Component.vue` → `products/variants/components/`
- `products/components/images/` (entire dir) → `products/variants/components/images/`
- `products/components/ProductImageManager.Component.vue` → `products/variants/components/`
- `products/components/ProductInventoryManager.Component.vue` → `products/variants/components/`

**Phase 3 — Classification/option-type components/schemas into sub-features:**
- `products/schemas/ProductClassification.Schema.ts` → `products/classifications/schemas/`
- `products/components/ProductClassificationManager.Component.vue` → `products/classifications/components/`
- `products/components/ProductOptionTypeManager.Component.vue` → `products/option-types/components/`

**Phases 4-6 — Inventory/Ordering/Location flat → nested (listed inline in phase tasks)**

---

### Task N: 1 — Create variant sub-feature directories

**Files:**
- Create: `products/variants/types/`
- Create: `products/variants/schemas/`
- Create: `products/variants/stores/`
- Create: `products/variants/components/`
- Create: `products/variants/__tests__/`

**Interfaces:** N/A — scaffolding only

- [ ] **Step 1: Create all directories**

```bash
BASE=app/Admin/src/features/catalog/products/variants
mkdir -p $BASE/types $BASE/schemas $BASE/stores $BASE/components $BASE/__tests__
```

- [ ] **Step 2: Verify directories exist**

```bash
ls -d $BASE/{types,schemas,stores,components,__tests__}
```

Expected output — 5 directory paths printed, no errors.

- [ ] **Step 3: Commit**

```bash
git add $BASE/{types,schemas,stores,components,__tests__}/.gitkeep 2>/dev/null || \
  (touch $BASE/types/.gitkeep && git add $BASE/types/.gitkeep $BASE/schemas/.gitkeep $BASE/stores/.gitkeep $BASE/components/.gitkeep $BASE/__tests__/.gitkeep)
git commit -m "chore(admin): create variant sub-feature directories"
```

Note: We don't need `.gitkeep` files — `git mv` will create the directories automatically when we move files into them. Skip the commit here and fold it into Task 2.

---

### Task N: 2 — Move variant type files into `products/variants/types/`

**Files:**
- Move: `products/types/Variant.Parameters.Type.ts` → `products/variants/types/`
- Move: `products/types/Variant.Query.Type.ts` → `products/variants/types/`
- Move: `products/types/Variant.Request.Type.ts` → `products/variants/types/`
- Move: `products/types/Variant.Response.Type.ts` → `products/variants/types/`
- Modify: `products/variants/types/Variant.Request.Type.ts` (fix import path to schema)
- Modify: `products/variants/types/Variant.Parameters.Type.ts` (fix import path to schema)
- Modify: `products/types/Product.Response.Type.ts` (fix import path to VariantSummary)

**Interfaces:**
- Consumes: `products/variants/schemas/Variant.Schema` (moved in Task 3)
- Produces: `VariantParameters`, `VariantQuery`, `CreateVariantRequest`, `UpdateVariantRequest`, `VariantSummary`, `VariantDetail` at new paths

- [ ] **Step 1: Move files with git mv**

```bash
BASE=app/Admin/src/features/catalog/products
git mv $BASE/types/Variant.Parameters.Type.ts $BASE/variants/types/
git mv $BASE/types/Variant.Query.Type.ts $BASE/variants/types/
git mv $BASE/types/Variant.Request.Type.ts $BASE/variants/types/
git mv $BASE/types/Variant.Response.Type.ts $BASE/variants/types/
```

- [ ] **Step 2: Fix import path in `Variant.Request.Type.ts`**

Before: `import type { VariantParameters } from '../schemas/Variant.Schema'`
After: `import type { VariantParameters } from '../../schemas/Variant.Schema'`

```bash
cat > $BASE/variants/types/Variant.Request.Type.ts << 'EOF'
import type { VariantParameters } from '../../schemas/Variant.Schema'
export type CreateVariantRequest = VariantParameters & { productId?: string }
export type UpdateVariantRequest = Partial<CreateVariantRequest>
EOF
```

- [ ] **Step 3: Fix import path in `Variant.Parameters.Type.ts`**

Before: `import type { VariantParameters } from '../schemas/Variant.Schema'`
After: `import type { VariantParameters } from '../../schemas/Variant.Schema'`

```bash
cat > $BASE/variants/types/Variant.Parameters.Type.ts << 'EOF'
import type { VariantParameters } from '../../schemas/Variant.Schema'
export type { VariantParameters }
EOF
```

- [ ] **Step 4: Fix import path in `Product.Response.Type.ts`**

Read the file first to confirm line 27:
```bash
head -27 $BASE/types/Product.Response.Type.ts | tail -1
```

Expected: `import type { VariantSummary } from './Variant.Response.Type'`

Replace with:

```bash
cat > /tmp/fix_import.py << 'PYEOF'
content = open("$BASE/types/Product.Response.Type.ts").read()
content = content.replace(
    "import type { VariantSummary } from './Variant.Response.Type'",
    "import type { VariantSummary } from './variants/types/Variant.Response.Type'"
)
open("$BASE/types/Product.Response.Type.ts", "w").write(content)
PYEOF
python3 /tmp/fix_import.py
```

Or use `sed`:
```bash
sed -i "s|from './Variant.Response.Type'|from './variants/types/Variant.Response.Type'|" $BASE/types/Product.Response.Type.ts
```

- [ ] **Step 5: Run typecheck to verify**

```bash
pnpm run type-check 2>&1 | head -20
```

Expected: Zero `Cannot find module` errors for variant paths. Other pre-existing errors may remain.

- [ ] **Step 6: Commit**

```bash
git add $BASE/variants/types/ $BASE/types/Product.Response.Type.ts
git commit -m "refactor(admin): move variant types into products/variants/types/"
```

---

### Task N: 3 — Move variant schema into `products/variants/schemas/`

**Files:**
- Move: `products/schemas/Variant.Schema.ts` → `products/variants/schemas/`

**Interfaces:**
- Consumes: zod (from `zod` package)
- Produces: `VariantSchema` zod object at `products/variants/schemas/Variant.Schema.ts`

- [ ] **Step 1: Move file**

```bash
BASE=app/Admin/src/features/catalog/products
git mv $BASE/schemas/Variant.Schema.ts $BASE/variants/schemas/
```

- [ ] **Step 2: Verify no import changes needed**

The schema file itself has no relative imports — it only imports `z` from `'zod'`. No changes needed.

```bash
cat $BASE/variants/schemas/Variant.Schema.ts
```

Expected: Just `import { z } from 'zod'` and the schema definition.

- [ ] **Step 3: Run typecheck**

```bash
pnpm run type-check 2>&1 | head -20
```

Expected: Zero new errors.

- [ ] **Step 4: Commit**

```bash
git add $BASE/variants/schemas/
git commit -m "refactor(admin): move Variant.Schema into products/variants/schemas/"
```

---

### Task N: 4 — Move variant-related Vue components into `products/variants/components/`

**Files:**
- Move: `products/components/ProductVariantManager.Component.vue` → `products/variants/components/`
- Move: `products/components/VariantFormDialog.Component.vue` → `products/variants/components/`
- Move: `products/components/dialogs/VariantGenerationDialog.Component.vue` → `products/variants/components/`
- Move: `products/components/ProductImageManager.Component.vue` → `products/variants/components/`
- Move: `products/components/ProductInventoryManager.Component.vue` → `products/variants/components/`
- Move: `products/components/images/` → `products/variants/components/images/`
- Modify: All moved `.vue` files (fix relative import paths)

**Interfaces:** N/A — only path changes

- [ ] **Step 1: Move all variant-related components**

```bash
BASE=app/Admin/src/features/catalog/products
git mv $BASE/components/ProductVariantManager.Component.vue $BASE/variants/components/
git mv $BASE/components/VariantFormDialog.Component.vue $BASE/variants/components/
git mv $BASE/components/dialogs/VariantGenerationDialog.Component.vue $BASE/variants/components/
git mv $BASE/components/ProductImageManager.Component.vue $BASE/variants/components/
git mv $BASE/components/ProductInventoryManager.Component.vue $BASE/variants/components/
git mv $BASE/components/images/ $BASE/variants/components/images/
```

- [ ] **Step 2: Remove empty `dialogs/` dir**

```bash
rmdir $BASE/components/dialogs/ 2>/dev/null || echo "dialogs/ not empty (check)"
ls $BASE/components/
```

Expected: `ProductClassificationManager`, `ProductOptionTypeManager` remain — these stay at products level.

- [ ] **Step 3: Fix imports in `ProductVariantManager.Component.vue`**

Read current imports:
```bash
head -14 $BASE/variants/components/ProductVariantManager.Component.vue
```

Current (already partially correct from previous plan):
```
import { variantService } from '../variants/services/variant.service';   ✅
import VariantGenerationDialog from './dialogs/VariantGenerationDialog.Component.vue';  ❌ now at ./VariantGenerationDialog
import VariantFormDialog from './VariantFormDialog.Component.vue';  ✅
import type { VariantSummary, VariantDetail } from '../types/Variant.Response.Type';  ❌ now at ./types/Variant.Response.Type
import type { CreateVariantRequest } from '../types/Variant.Request.Type';  ❌ now at ./types/Variant.Request.Type
```

Fix each:
```bash
sed -i \
  -e "s|from './dialogs/VariantGenerationDialog.Component.vue'|from './VariantGenerationDialog.Component.vue'|" \
  -e "s|from '../types/Variant.Response.Type'|from '../variants/types/Variant.Response.Type'|" \
  -e "s|from '../types/Variant.Request.Type'|from '../variants/types/Variant.Request.Type'|" \
  $BASE/variants/components/ProductVariantManager.Component.vue
```

- [ ] **Step 4: Fix imports in `VariantFormDialog.Component.vue`**

Current imports:
```bash
head -14 $BASE/variants/components/VariantFormDialog.Component.vue
```

Shows:
```
import { productService } from '../services/product.service';  ✅ (stays at products level)
import type { VariantDetail } from '../types/Variant.Response.Type';  ❌
import type { CreateVariantRequest } from '../types/Variant.Request.Type';  ❌
```

The component is now at `variants/components/VariantFormDialog.Component.vue`. Types are at `variants/types/`. So `../types/` = `variants/types/` — that's correct! No change needed for the `../types/` paths.

But wait — the component imports `productService` from `'../services/product.service'` which resolves to `variants/services/product.service` — WRONG. It should be `../../services/product.service`.

```bash
sed -i \
  -e "s|from '../services/product.service'|from '../../services/product.service'|" \
  -e "s|from '../types/Variant.Response.Type'|from '../types/Variant.Response.Type'|" \
  -e "s|from '../types/Variant.Request.Type'|from '../types/Variant.Request.Type'|" \
  $BASE/variants/components/VariantFormDialog.Component.vue
```

Actually `../types/` from `variants/components/` = `variants/types/` — correct! Only the productService import needs fixing.

```bash
sed -i "s|from '../services/product.service'|from '../../services/product.service'|" \
  $BASE/variants/components/VariantFormDialog.Component.vue
```

- [ ] **Step 5: Fix imports in `VariantGenerationDialog.Component.vue`**

Current:
```bash
head -12 $BASE/variants/components/VariantGenerationDialog.Component.vue
```

Current imports:
```
import { useProductStore } from '../../stores/product.store';  ✅ (was '../../stores/', still correct from variants/components/)
import { productService } from '../../services/product.service';  ✅
import { variantService } from '../../variants/services/variant.service';  ❌ now at ./services/variant.service
```

File is at `variants/components/VariantGenerationDialog.Component.vue`.
- `../../stores/product.store` = `products/stores/product.store` ✅
- `../../services/product.service` = `products/services/product.service` ✅
- `../../variants/services/variant.service` — should be `../services/variant.service` (it's now at same level)

```bash
sed -i "s|from '../../variants/services/variant.service'|from '../services/variant.service'|" \
  $BASE/variants/components/VariantGenerationDialog.Component.vue
```

- [ ] **Step 6: Fix imports in `ProductImageManager.Component.vue`**

Current:
```bash
head -12 $BASE/variants/components/ProductImageManager.Component.vue
```

Current imports:
```
import ProductImageUploader from './images/ProductImageUploader.Component.vue';  ✅ (relative stays same)
import ProductImageList from './images/ProductImageList.Component.vue';  ✅
import { productService } from '../services/product.service';  ❌ now depth is different
import type { ProductImage } from '../types/Product.Response.Type';  ❌
```

File is now at `variants/components/ProductImageManager.Component.vue`.
- `./images/ProductImageUploader` = `variants/components/images/` ✅
- `../services/product.service` = `variants/services/product.service` — WRONG, should be `../../services/product.service`
- `../types/Product.Response.Type` = `variants/types/Product.Response.Type` — WRONG, should be `../../types/Product.Response.Type`

```bash
sed -i \
  -e "s|from '../services/product.service'|from '../../services/product.service'|" \
  -e "s|from '../types/Product.Response.Type'|from '../../types/Product.Response.Type'|" \
  $BASE/variants/components/ProductImageManager.Component.vue
```

- [ ] **Step 7: Fix imports in `ProductInventoryManager.Component.vue`**

Current:
```bash
head -12 $BASE/variants/components/ProductInventoryManager.Component.vue
```

Current imports:
```
import { inventoryService } from '@/features/inventories/services/inventory.service';  ✅ (absolute path)
import { variantService } from '../variants/services/variant.service';  ❌
```

File is now at `variants/components/ProductInventoryManager.Component.vue`.
- `../variants/services/variant.service` = `products/variants/services/variant.service` ❌ — at this depth, `../variants/` from `variants/components/` goes up to `variants/` then back down to `variants/services/`. That's wrong.

`../` from `variants/components/` = `variants/`. So `../variants/` = `variants/variants/`. Need `../services/` = `variants/services/`.

```bash
sed -i "s|from '../variants/services/variant.service'|from '../services/variant.service'|" \
  $BASE/variants/components/ProductInventoryManager.Component.vue
```

- [ ] **Step 8: Fix imports in image components**

```bash
head -5 $BASE/variants/components/images/ProductImageList.Component.vue
```

Current: `import type { ProductImage } from '../../types/Product.Response.Type'`
File is at `variants/components/images/ProductImageList.Component.vue`.
`../../types/` = `variants/types/` — but ProductImage is at `products/types/`. Need `../../../types/`.

```bash
sed -i "s|from '../../types/Product.Response.Type'|from '../../../types/Product.Response.Type'|" \
  $BASE/variants/components/images/ProductImageList.Component.vue

sed -i "s|from '../../types/Product.Response.Type'|from '../../../types/Product.Response.Type'|" \
  $BASE/variants/components/images/ProductImageUploader.Component.vue
```

- [ ] **Step 9: Verify typecheck**

```bash
pnpm run type-check 2>&1 | grep "Cannot find module"
```

Expected: zero matches.

- [ ] **Step 10: Commit**

```bash
git add $BASE/variants/components/ $BASE/components/
git commit -m "refactor(admin): move variant components into products/variants/components/"
```

---

### Task N: 5 — Extract price API, types, and service from variant.api.ts

**Files:**
- Create: `products/variants/prices/api/price.api.ts`
- Create: `products/variants/prices/types/Price.Response.Type.ts`
- Create: `products/variants/prices/services/price.service.ts`
- Modify: `products/variants/api/variant.api.ts` (remove price methods)
- Modify: `products/variants/services/variant.service.ts` (remove price delegations)

**Interfaces:**
- Consumes: `variantApi` from `variant.api.ts` (before extraction)
- Produces: `priceApi` at `prices/api/price.api.ts` with `listPrices`, `setPrice`, `deletePrice`, `syncPrices`

- [ ] **Step 1: Create directories**

```bash
BASE=app/Admin/src/features/catalog/products/variants
mkdir -p $BASE/prices/api $BASE/prices/types $BASE/prices/services
```

- [ ] **Step 2: Create `Price.Response.Type.ts`**

```bash
cat > $BASE/prices/types/Price.Response.Type.ts << 'EOF'
export interface PriceRecord {
  id: string
  amount: number
  currency: string
}
EOF
```

- [ ] **Step 3: Create `price.api.ts`**

```bash
cat > $BASE/prices/api/price.api.ts << 'EOF'
import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ServerPagedResult, ServerResult } from '@/shared/api/types/result.types'
import type { PriceRecord } from '../types/Price.Response.Type'

export const priceApi = {
  listPrices: (variantId: string): Promise<ServerPagedResult<PriceRecord>> =>
    apiClient.get(`${CATALOG}/variants/${variantId}/prices`).then(res => res.data as ServerPagedResult<PriceRecord>),

  setPrice: (variantId: string, data: { amount: number; currency: string }): Promise<ServerResult<PriceRecord>> =>
    apiClient.post(`${CATALOG}/variants/${variantId}/prices`, data).then(res => res.data as ServerResult<PriceRecord>),

  deletePrice: (variantId: string, priceId: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/variants/${variantId}/prices/${priceId}`).then(res => res.data as ServerResult<void>),

  syncPrices: (variantId: string, prices: Array<{ amount: number; currency: string }>): Promise<ServerResult<void>> =>
    apiClient.post(`${CATALOG}/variants/${variantId}/prices/sync`, prices).then(res => res.data as ServerResult<void>),
}
EOF
```

Note: `listPrices` uses `ServerPagedResult<PriceRecord>` per backend audit (backend returns `PagedResult<Response>`).

- [ ] **Step 4: Create `price.service.ts`**

```bash
cat > $BASE/prices/services/price.service.ts << 'EOF'
import { priceApi } from '../api/price.api'

export const priceService = {
  listPrices: priceApi.listPrices,
  setPrice: priceApi.setPrice,
  deletePrice: priceApi.deletePrice,
  syncPrices: priceApi.syncPrices,
}
EOF
```

- [ ] **Step 5: Update `variant.api.ts` — remove price methods**

Read current file:
```bash
cat $BASE/api/variant.api.ts
```

Current content:
```typescript
import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { VariantDetail, VariantSummary } from '../../types/Variant.Response.Type'
import type { CreateVariantRequest, UpdateVariantRequest } from '../../types/Variant.Request.Type'

interface PriceRecord {
  id: string
  amount: number
  currency: string
}

interface VariantImage {
  id: string
  variantId: string
  url: string
  alt: string | null
  position: number
  role: number
  fileSize: number | null
  isDefault: boolean
}

export const variantRepository = {
  getById: (id: string): Promise<ServerResult<VariantDetail>> =>
    apiClient.get(`${CATALOG}/variants/${id}`).then(res => res.data as ServerResult<VariantDetail>),

  listByProductId: (productId: string): Promise<ServerResult<VariantSummary[]>> =>
    apiClient.get(`${CATALOG}/products/${productId}/variants`).then(res => res.data as ServerResult<VariantSummary[]>),

  create: (productId: string, data: CreateVariantRequest): Promise<ServerResult<VariantDetail>> =>
    apiClient.post(`${CATALOG}/products/${productId}/variants`, data).then(res => res.data as ServerResult<VariantDetail>),

  update: (id: string, data: UpdateVariantRequest): Promise<ServerResult<VariantDetail>> =>
    apiClient.put(`${CATALOG}/variants/${id}`, data).then(res => res.data as ServerResult<VariantDetail>),

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/variants/${id}`).then(res => res.data as ServerResult<void>),

  listPrices: (variantId: string): Promise<ServerResult<PriceRecord[]>> =>
    apiClient.get(`${CATALOG}/variants/${variantId}/prices`).then(res => res.data as ServerResult<PriceRecord[]>),

  setPrice: (variantId: string, data: { amount: number; currency: string }): Promise<ServerResult<PriceRecord>> =>
    apiClient.post(`${CATALOG}/variants/${variantId}/prices`, data).then(res => res.data as ServerResult<PriceRecord>),

  deletePrice: (variantId: string, priceId: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/variants/${variantId}/prices/${priceId}`).then(res => res.data as ServerResult<void>),

  syncPrices: (variantId: string, prices: Array<{ amount: number; currency: string }>): Promise<ServerResult<void>> =>
    apiClient.post(`${CATALOG}/variants/${variantId}/prices/sync`, prices).then(res => res.data as ServerResult<void>),

  syncOptionValues: (variantId: string, optionValueIds: string[]): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/variants/${variantId}/option-values/sync`, { optionValueIds }).then(res => res.data as ServerResult<void>),

  listImages: (variantId: string): Promise<ServerResult<VariantImage[]>> =>
    apiClient.get(`${CATALOG}/variants/${variantId}/images`).then(res => res.data as ServerResult<VariantImage[]>),

  uploadImage: (variantId: string, file: File, role?: number): Promise<ServerResult<VariantImage>> => {
    const formData = new FormData()
    formData.append('file', file)
    let url = `${CATALOG}/variants/${variantId}/images`
    if (role !== undefined) url += `?role=${role}`
    return apiClient.post(url, formData, { headers: { 'Content-Type': 'multipart/form-data' } }).then(res => res.data as ServerResult<VariantImage>)
  },

  deleteImage: (imageId: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/variants/images/${imageId}`).then(res => res.data as ServerResult<void>),

  updateImage: (imageId: string, data: { alt?: string; role?: number }): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/variants/images/${imageId}`, data).then(res => res.data as ServerResult<void>),
}
```

We need to:
1. Remove `PriceRecord` interface (moved to `prices/types/`)
2. Keep `VariantImage` interface (moved to `images/types/` in next task)
3. Remove price methods (`listPrices`, `setPrice`, `deletePrice`, `syncPrices`)
4. Keep image methods (extracted in Task 6)
5. Fix type import paths
6. Add `ServerPagedResult` import

Replace content:

```bash
cat > $BASE/api/variant.api.ts << 'EOF'
import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { VariantDetail, VariantSummary } from '../types/Variant.Response.Type'
import type { CreateVariantRequest, UpdateVariantRequest } from '../types/Variant.Request.Type'

export const variantRepository = {
  getById: (id: string): Promise<ServerResult<VariantDetail>> =>
    apiClient.get(`${CATALOG}/variants/${id}`).then(res => res.data as ServerResult<VariantDetail>),

  listByProductId: (productId: string): Promise<ServerResult<VariantSummary[]>> =>
    apiClient.get(`${CATALOG}/products/${productId}/variants`).then(res => res.data as ServerResult<VariantSummary[]>),

  create: (productId: string, data: CreateVariantRequest): Promise<ServerResult<VariantDetail>> =>
    apiClient.post(`${CATALOG}/products/${productId}/variants`, data).then(res => res.data as ServerResult<VariantDetail>),

  update: (id: string, data: UpdateVariantRequest): Promise<ServerResult<VariantDetail>> =>
    apiClient.put(`${CATALOG}/variants/${id}`, data).then(res => res.data as ServerResult<VariantDetail>),

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/variants/${id}`).then(res => res.data as ServerResult<void>),

  syncOptionValues: (variantId: string, optionValueIds: string[]): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/variants/${variantId}/option-values/sync`, { optionValueIds }).then(res => res.data as ServerResult<void>),
}
EOF
```

- [ ] **Step 6: Update `variant.service.ts` — remove price delegations**

Current:
```typescript
import { variantRepository } from '../api/variant.api'

export const variantService = {
    getById: variantRepository.getById,
    listByProductId: variantRepository.listByProductId,
    create: variantRepository.create,
    update: variantRepository.update,
    delete: variantRepository.delete,
    updateOptionValues: variantRepository.syncOptionValues,
}
```

No changes needed — price methods were not in the service. Verify:
```bash
cat $BASE/services/variant.service.ts
```

- [ ] **Step 7: Run typecheck**

```bash
pnpm run type-check 2>&1 | grep "Cannot find module"
```

Expected: zero matches.

- [ ] **Step 8: Commit**

```bash
git add $BASE/prices/ $BASE/api/variant.api.ts $BASE/services/variant.service.ts
git commit -m "refactor(admin): extract price sub-feature from variant.api.ts into products/variants/prices/"
```

---

### Task N: 6 — Extract image API, types, and service from variant.api.ts

**Files:**
- Create: `products/variants/images/api/image.api.ts`
- Create: `products/variants/images/types/Image.Response.Type.ts`
- Create: `products/variants/images/services/image.service.ts`
- Modify: `products/variants/api/variant.api.ts` (remove image methods — done in Task 5)
- Modify: `products/variants/services/variant.service.ts` (no change needed)

**Interfaces:**
- Produces: `imageApi` at `images/api/image.api.ts` with `listByVariant`, `upload`, `update`, `delete`

- [ ] **Step 1: Create directories**

```bash
BASE=app/Admin/src/features/catalog/products/variants
mkdir -p $BASE/images/api $BASE/images/types $BASE/images/services
```

- [ ] **Step 2: Create `Image.Response.Type.ts`**

```bash
cat > $BASE/images/types/Image.Response.Type.ts << 'EOF'
export interface VariantImage {
  id: string
  variantId: string
  url: string
  alt: string | null
  position: number
  role: number
  fileSize: number | null
  isDefault: boolean
}
EOF
```

- [ ] **Step 3: Create `image.api.ts`**

```bash
cat > $BASE/images/api/image.api.ts << 'EOF'
import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { VariantImage } from '../types/Image.Response.Type'

export const imageApi = {
  listByVariant: (variantId: string): Promise<ServerResult<VariantImage[]>> =>
    apiClient.get(`${CATALOG}/variants/${variantId}/images`).then(res => res.data as ServerResult<VariantImage[]>),

  upload: (variantId: string, file: File, role?: number): Promise<ServerResult<VariantImage>> => {
    const formData = new FormData()
    formData.append('file', file)
    let url = `${CATALOG}/variants/${variantId}/images`
    if (role !== undefined) url += `?role=${role}`
    return apiClient.post(url, formData, { headers: { 'Content-Type': 'multipart/form-data' } }).then(res => res.data as ServerResult<VariantImage>)
  },

  update: (imageId: string, data: { alt?: string; role?: number }): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/variants/images/${imageId}`, data).then(res => res.data as ServerResult<void>),

  delete: (imageId: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/variants/images/${imageId}`).then(res => res.data as ServerResult<void>),
}
EOF
```

- [ ] **Step 4: Create `image.service.ts`**

```bash
cat > $BASE/images/services/image.service.ts << 'EOF'
import { imageApi } from '../api/image.api'

export const imageService = {
  listByVariant: imageApi.listByVariant,
  upload: imageApi.upload,
  update: imageApi.update,
  delete: imageApi.delete,
}
EOF
```

- [ ] **Step 5: Run typecheck**

```bash
pnpm run type-check 2>&1 | grep "Cannot find module"
```

Expected: zero matches.

- [ ] **Step 6: Commit**

```bash
git add $BASE/images/
git commit -m "refactor(admin): extract image sub-feature from variant.api.ts into products/variants/images/"
```

---

### Task N: 7 — Fix imports in components that use priceApi/imageApi instead of variantApi

**Files:**
- Modify: `products/variants/components/ProductImageManager.Component.vue`
- Modify: `products/variants/components/ProductInventoryManager.Component.vue`

**Interfaces:**
- Consumes: `imageService` from `../images/services/image.service`, `priceService` from `../prices/services/price.service`

- [ ] **Step 1: Check `ProductImageManager.Component.vue` imports**

```bash
BASE=app/Admin/src/features/catalog/products/variants
head -12 $BASE/components/ProductImageManager.Component.vue
```

Current imports (after Task 4 fix):
```
import { productService } from '../../services/product.service';
import type { ProductImage } from '../../types/Product.Response.Type';
```

The component uses `apiClient directly` for image operations — check:
```bash
grep -n "apiClient\|variantService\|imageService\|priceService" $BASE/components/ProductImageManager.Component.vue | head -10
```

If it uses `apiClient` directly for calls that should go through `imageService`, update it. Let me check the full file:

```bash
grep -n "get\|post\|upload\|delete\|update\|variantService\|imageService" $BASE/components/ProductImageManager.Component.vue | head -20
```

For now, add an import for `imageService` and replace direct `apiClient` calls:

```bash
# Add import
sed -i "5 a import { imageService } from '../images/services/image.service';" \
  $BASE/components/ProductImageManager.Component.vue
```

- [ ] **Step 2: Check `ProductInventoryManager.Component.vue`**

```bash
grep -n "variantService\|priceService\|listPrices" $BASE/components/ProductInventoryManager.Component.vue | head -10
```

This component uses `variantService` for listing prices still. Update to use `priceService`:

```bash
# Add import after variantService import
sed -i "/variantService/a import { priceService } from '../prices/services/price.service';" \
  $BASE/components/ProductInventoryManager.Component.vue
```

- [ ] **Step 3: Run typecheck**

```bash
pnpm run type-check 2>&1 | grep "Cannot find module"
```

Expected: zero matches.

- [ ] **Step 4: Commit**

```bash
git add $BASE/components/
git commit -m "refactor(admin): update components to use extracted priceService and imageService"
```

---

### Task N: 8 — Extract classifications and option-types sub-features under products

**Files:**
- Create: `products/classifications/api/product-classification.api.ts`
- Create: `products/classifications/types/`
- Create: `products/classifications/services/classification.service.ts`
- Create: `products/option-types/api/product-option-type.api.ts`
- Create: `products/option-types/types/`
- Create: `products/option-types/services/product-option-type.service.ts`
- Move: `products/schemas/ProductClassification.Schema.ts` → `products/classifications/schemas/`
- Move: `products/components/ProductClassificationManager.Component.vue` → `products/classifications/components/`
- Move: `products/components/ProductOptionTypeManager.Component.vue` → `products/option-types/components/`
- Modify: `products/api/product.api.ts` (remove classification/option-type methods)
- Modify: `products/services/product.service.ts` (remove classification/option-type delegations)

- [ ] **Step 1: Create directories**

```bash
BASE=app/Admin/src/features/catalog/products
mkdir -p $BASE/classifications/api $BASE/classifications/types $BASE/classifications/services $BASE/classifications/schemas $BASE/classifications/components
mkdir -p $BASE/option-types/api $BASE/option-types/types $BASE/option-types/services $BASE/option-types/components
```

- [ ] **Step 2: Create `product-classification.api.ts`**

```bash
cat > $BASE/classifications/api/product-classification.api.ts << 'EOF'
import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { ProductClassification } from '../../types/Product.Response.Type'

export const productClassificationApi = {
  getClassifications: (productId: string): Promise<ServerResult<ProductClassification[]>> =>
    apiClient.get(`${CATALOG}/products/${productId}/classifications`).then(res => res.data as ServerResult<ProductClassification[]>),

  syncClassifications: (productId: string, data: { taxonIds: string[]; mainTaxonId?: string }): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/products/${productId}/classifications/sync`, data).then(res => res.data as ServerResult<void>),
}
EOF
```

- [ ] **Step 3: Create `classification.service.ts`**

```bash
cat > $BASE/classifications/services/classification.service.ts << 'EOF'
import { productClassificationApi } from '../api/product-classification.api'

export const classificationService = {
  getClassifications: productClassificationApi.getClassifications,
  syncClassifications: productClassificationApi.syncClassifications,
}
EOF
```

- [ ] **Step 4: Create `product-option-type.api.ts`**

```bash
cat > $BASE/option-types/api/product-option-type.api.ts << 'EOF'
import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { OptionTypeDetail } from '../../../option-types/types/OptionType.Response.Type'

export const productOptionTypeApi = {
  getOptionTypes: (productId: string): Promise<ServerResult<OptionTypeDetail[]>> =>
    apiClient.get(`${CATALOG}/products/${productId}/option-types`).then(res => res.data as ServerResult<OptionTypeDetail[]>),

  syncOptionTypes: (productId: string, optionTypeIds: string[]): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/products/${productId}/option-types/sync`, { optionTypeIds }).then(res => res.data as ServerResult<void>),
}
EOF
```

- [ ] **Step 5: Create `product-option-type.service.ts`**

```bash
cat > $BASE/option-types/services/product-option-type.service.ts << 'EOF'
import { productOptionTypeApi } from '../api/product-option-type.api'

export const productOptionTypeService = {
  getOptionTypes: productOptionTypeApi.getOptionTypes,
  syncOptionTypes: productOptionTypeApi.syncOptionTypes,
}
EOF
```

- [ ] **Step 6: Move schemas and components**

```bash
git mv $BASE/schemas/ProductClassification.Schema.ts $BASE/classifications/schemas/
git mv $BASE/components/ProductClassificationManager.Component.vue $BASE/classifications/components/
git mv $BASE/components/ProductOptionTypeManager.Component.vue $BASE/option-types/components/
```

- [ ] **Step 7: Fix import in `ProductClassificationManager.Component.vue`**

File is now at `products/classifications/components/ProductClassificationManager.Component.vue`.

Current imports line 4: `import { useProductStore } from '../stores/product.store';`
This resolves to `classifications/stores/product.store` — WRONG. Should be `../../stores/product.store`.

```bash
sed -i "s|from '../stores/product.store'|from '../../stores/product.store'|" \
  $BASE/classifications/components/ProductClassificationManager.Component.vue
sed -i "s|from '../types/Product.Response.Type'|from '../../types/Product.Response.Type'|" \
  $BASE/classifications/components/ProductClassificationManager.Component.vue
```

- [ ] **Step 8: Fix import in `ProductOptionTypeManager.Component.vue`**

File is now at `products/option-types/components/ProductOptionTypeManager.Component.vue`.

Current: `import { productService } from '../services/product.service';`
Resolves to `option-types/services/product.service` — WRONG. Should be `../../services/product.service`.

```bash
sed -i "s|from '../services/product.service'|from '../../services/product.service'|" \
  $BASE/option-types/components/ProductOptionTypeManager.Component.vue
```

- [ ] **Step 9: Update `product.api.ts` — remove classification/option-type methods**

```bash
cat > $BASE/api/product.api.ts << 'EOF'
import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ServerPagedResult, ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { ProductDetail, ProductSummary } from '../types/Product.Response.Type'
import type { CreateProductRequest, UpdateProductRequest } from '../types/Product.Request.Type'

export const productRepository = {
  list: (params?: ServerQueryingParameters): Promise<ServerPagedResult<ProductSummary>> =>
    apiClient.get(`${CATALOG}/products`, { params }).then(res => res.data as ServerPagedResult<ProductSummary>),

  getById: (id: string): Promise<ServerResult<ProductDetail>> =>
    apiClient.get(`${CATALOG}/products/${id}`).then(res => res.data as ServerResult<ProductDetail>),

  create: (data: CreateProductRequest): Promise<ServerResult<ProductDetail>> =>
    apiClient.post(`${CATALOG}/products`, data).then(res => res.data as ServerResult<ProductDetail>),

  update: (id: string, data: UpdateProductRequest): Promise<ServerResult<ProductDetail>> =>
    apiClient.put(`${CATALOG}/products/${id}`, data).then(res => res.data as ServerResult<ProductDetail>),

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/products/${id}`).then(res => res.data as ServerResult<void>),

  activate: (id: string): Promise<ServerResult<void>> =>
    apiClient.patch(`${CATALOG}/products/${id}/activate`).then(res => res.data as ServerResult<void>),

  discontinue: (id: string): Promise<ServerResult<void>> =>
    apiClient.patch(`${CATALOG}/products/${id}/discontinue`).then(res => res.data as ServerResult<void>),
}
EOF
```

Note: `list` now returns `ServerPagedResult<ProductSummary>` per backend audit (backend returns `PagedResult<Response>`).

- [ ] **Step 10: Update `product.service.ts` — remove classification/option-type delegations**

```bash
cat > $BASE/services/product.service.ts << 'EOF'
import { productRepository } from '../api/product.api'

export const productService = {
  list: productRepository.list,
  getById: productRepository.getById,
  create: productRepository.create,
  update: productRepository.update,
  delete: productRepository.delete,
  activate: productRepository.activate,
  discontinue: productRepository.discontinue,
}
EOF
```

- [ ] **Step 11: Run typecheck**

```bash
pnpm run type-check 2>&1 | grep "Cannot find module"
```

Expected: zero matches.

- [ ] **Step 12: Commit**

```bash
git add $BASE/classifications/ $BASE/option-types/ $BASE/api/product.api.ts $BASE/services/product.service.ts $BASE/components/
git commit -m "refactor(admin): extract classifications and option-types sub-features under products/"
```

---

### Task N: 9-20 — [Outline for remaining phases]

Remaining tasks follow the same pattern as Tasks 1-8. For brevity, the pattern is:

**Phase 4 (Inventories):** Move flat files from `inventories/schemas/`, `inventories/types/`, `inventories/views/` into each sub-feature dir (`stock-items/`, `stock-locations/`, `stock-movements/`, `stock-transfers/`, `inventory-units/`). Split `inventories/services/inventory.service.ts` into per-sub-feature services. Update all import paths.

**Phase 5 (Ordering):** Move `ordering/schemas/`, `ordering/types/`, `ordering/views/` into `ordering/orders/`. Move `ordering/services/order.service.ts` and `ordering/stores/order.store.ts` into `ordering/orders/`. Move `ordering/components/` into `ordering/orders/components/`.

**Phase 6 (Location):** Move `location/schemas/`, `location/services/`, `location/stores/`, `location/types/`, `location/views/` into `location/countries/` and `location/states/`.

**Phase 7 (Users):** Move `users/services/role.service.ts` → `users/roles/services/`, `users/services/permission.service.ts` → `users/permissions/services/`.

**Phase 8 (Profile):** Create `profile/addresses/` with API, types, and service scaffolding.

**Phase 9 (Type corrections):** Audit all 23 API files — change `ServerResult<T>` to `ServerPagedResult<T>` for list endpoints where backend returns `PagedResult<T>`.

**Phase 10 (Verification):** `rg` checks, lint, typecheck, unit tests.

---

## 3. Alternatives

- **ALT-001**: Keep current flat + semi-nested structure — rejected because backend uses deep nesting; frontend should mirror for maintainability
- **ALT-002**: Use barrel `index.ts` re-exports instead of moving files — simpler but breaks one-to-one file-to-endpoint mapping, creates indirection
- **ALT-003**: Do Result/PagedResult corrections in a separate plan — included here because type signatures live in same `api/` files being moved

## 4. Dependencies

- **DEP-001**: Builds on `plan/refactor-repo-to-api-1.md` (already completed): `repositories/` → `api/` rename + variants under products
- **DEP-002**: Tasks within each phase are sequential (moving files breaks imports mid-phase)
- **DEP-003**: Phases 1-8 are independent of each other (different feature modules) — can parallelize
- **DEP-004**: Phase 9 (type corrections) must run after all file moves settle

## 5. Files

- **~110 files moved** (git mv) across catalog, inventories, ordering, location, users
- **~20 files created** (price/image/classification/option-type API extraction, address scaffolding)
- **~50 import updates** across moved `.vue` and `.ts` files

## 6. Testing

- **TEST-001**: `rg '\.repository\.ts' app/Admin/src/` — zero matches
- **TEST-002**: `rg '/repositories/' app/Admin/src/'` — zero matches
- **TEST-003**: `pnpm run lint` — must pass (same pre-existing errors only)
- **TEST-004**: `vue-tsc --build` — zero `Cannot find module` errors
- **TEST-005**: `pnpm run test:unit` — same pre-existing failures only (no new regressions)
- **TEST-006**: Each `GET list` endpoint in `api/*.api.ts` uses `ServerPagedResult<T>` where backend returns `PagedResult<T>`

## 7. Risks & Assumptions

- **RISK-001**: Vue components may reference types via `../types/` relative paths that break after moves — mitigated by systematic sed updates in each task
- **RISK-002**: Route files (`catalog.routes.ts`, `inventory.routes.ts`, etc.) use `@/` path aliases for view imports — these use absolute paths so views can be moved without updating routes
- **RISK-003**: `Product.Response.Type.ts` imports `VariantSummary` — after moving variant types, this import path must be updated (handled in Task 2)
- **RISK-004**: Some `.vue` files may use `apiClient` directly (circumventing service layer) — these won't benefit from the extraction but also won't break
- **ASSUMPTION-001**: No code outside `app/Admin/src/` imports from these paths (Store SPA is separate)

## 8. Related Specifications / Further Reading

- Backend endpoint structure: `service/Api/src/Module/*/Features/Admin/` — hierarchical nesting by entity
- Backend `Result<T>` vs `PagedResult<T>` audit: See endpoint table in `plan/refactor-nested-structure-2.md` §1
- `plan/refactor-repo-to-api-1.md` — previous plan (completed): `repositories/` → `api/` rename
- `plan/refactor-nested-structure-2.md` — high-level task table (superseded by this detailed plan)
