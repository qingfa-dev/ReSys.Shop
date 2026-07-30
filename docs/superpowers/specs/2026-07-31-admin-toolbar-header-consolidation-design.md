# Admin Toolbar & Header Consolidation

**Date**: 2026-07-31  
**Scope**: Admin SPA — 2 prototype views (CountriesList + CountryDetail), then replicate to 5 remaining pairs  
**Decision**: Merge list page standalone `<Toolbar>` into DataTable `#header` slot. Move detail page actions into a top toolbar row, drop inner section heading.

## Motivation

- List pages currently split toolbar actions and search between a standalone `<Toolbar>` and DataTable `#header` — they are visually separate and redundant.
- Detail pages have inner section headings ("Country Details") that repeat information already in the page title, and sticky bottom action bars that add unnecessary chrome.
- Consolidating into a single consistent header pattern reduces visual noise and matches modern admin panel conventions (linear scan: title → filters → data).

## Architecture

### List page pattern (CountriesList prototype)

**Current:**
```
<div class="...">
  <div>Countries title + description</div>
  <Toolbar>
    <template #start>New Country, Delete</template>
    <template #end>Export</template>
  </Toolbar>
  <DataTable>
    <template #header>Search input + Clear button</template>
  </DataTable>
</div>
```

**Target:**
```
<div class="flex flex-col h-full p-4">
  <div class="flex-none flex flex-col gap-4">
    <div>
      <div class="font-semibold text-xl">Countries</div>
      <p class="text-muted-color mt-1">Manage supported countries</p>
    </div>
  </div>
  <div class="flex-1 min-h-0 mt-4">
    <DataTable>
      <template #header>
        <div class="flex justify-between items-center">
          <div class="flex items-center gap-2">
            <FloatLabel variant="on">
              <IconField>
                <InputIcon class="pi pi-search" />
                <InputText v-model="searchTerm" placeholder="Search countries..." />
              </IconField>
              <label>Search</label>
            </FloatLabel>
            <Button label="Clear" outlined @click="clearSearch" />
          </div>
          <div class="flex items-center gap-2">
            <Button label="New Country" icon="pi pi-plus" severity="primary" @click="navigateToNew" />
            <Button label="Reload" icon="pi pi-sync" severity="secondary" @click="refresh" />
            <Button label="Export" icon="pi pi-upload" severity="secondary" @click="exportCSV" />
          </div>
        </div>
      </template>
    </DataTable>
  </div>
</div>
```

**Key rules:**
- No standalone `<Toolbar>` — everything in `#header`
- Left side: `FloatLabel variant="on"` "Search" label + `IconField` with `InputIcon pi-search` + `InputText` + Clear button
- Right side: "New Country" (plus, primary) | "Reload" (sync, secondary) | "Export" (upload, secondary)
- Batch delete button removed from toolbar (row-level delete remains via row action buttons)
- `selectedItems` ref stays for row selection (delete via row icon)
- Column menu filters stay as-is (`filter-display="menu"`) — no expand/collapse constraint

### Detail page pattern (CountryDetail prototype)

**Target:**
```
<div class="flex flex-col h-full p-4">
  <div class="flex-none flex justify-between items-start gap-4 mb-4">
    <div>
      <div class="font-semibold text-xl">{{ pageTitle }}</div>
      <p v-if="pageDescription" class="text-muted-color mt-1">{{ pageDescription }}</p>
    </div>
    <div class="flex items-center gap-2 shrink-0">
      <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="loading" form="country-form" />
      <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
    </div>
  </div>

  <div class="flex-1 min-h-0 overflow-auto">
    <Card>
      <template #content>
        <Form id="country-form" v-slot="$form" :key="String(formLoaded)" :resolver="countryResolver" :initial-values="form" class="flex flex-col gap-4" @submit="onSubmit">
          <FormField>...fields...</FormField>
        </Form>
      </template>
    </Card>
  </div>
</div>
```

**Key rules:**
- Remove inner section heading ("Country Details") from Card
- Remove sticky bottom action bar from inside Form
- Action buttons (Save, Cancel) live in top toolbar row, right side of title
- Save button uses `form="country-form"` attribute + `type="submit"` to trigger Form submission from outside
- Give Form an `id` attribute so the external submit button can target it

### Tabbed detail variant

Same top toolbar pattern. Form gets an `id`. Tabs and fields stay as-is inside Form.

```
<div class="flex flex-col h-full p-4">
  <div class="flex-none flex justify-between items-start gap-4 mb-4">
    <div>title + description</div>
    <div class="flex items-center gap-2 shrink-0">
      <Button label="Save" form="tab-form" ... />
      <Button label="Cancel" ... />
    </div>
  </div>
  <div class="flex-1 min-h-0 overflow-auto">
    <Card>
      <template #content>
        <Form id="tab-form" ...>
          <Tabs>...</Tabs>
        </Form>
      </template>
    </Card>
  </div>
</div>
```

## Component Changes

### CountriesList.vue
- Remove `Toolbar` import
- Remove standalone `<Toolbar>` block
- Move New Country + Reload + Export to `#header` right side
- Remove batch Delete toolbar button (row delete icon stays)
- Replace `#header` search with `FloatLabel variant="on"` + `IconField` + `InputIcon` + `InputText` + Clear button on left

### CountryDetail.vue
- Remove "Country Details" inner heading from Card
- Remove sticky bottom action bar from Form
- Add `id="country-form"` to `<Form>`
- Add Save/Cancel buttons to top toolbar row using `flex justify-between`
- Save button uses `form="country-form"` + `type="submit"`

## Data Flow

No changes. Form submission, router navigation, API calls remain identical.

## Error Handling

No changes. Form validation errors render via `Message` per field as before. API errors via `useApiErrorHandler`.

## Replication Plan (after prototype)

After CountriesList + CountryDetail are verified, replicate to:

| List | Detail | Module |
|------|--------|--------|
| StatesList | StateDetail | Location |
| ProductsList | ProductDetail | Catalog |
| OptionTypesList | OptionTypeDetail | Catalog |
| TaxonomiesList | TaxonomyDetail | Catalog |
| TaxonsList | TaxonDetail | Catalog |

## Verification

```bash
cd app/Admin
pnpm run type-check
pnpm run test:unit -- run
```

All 570 tests must pass. No type errors.
