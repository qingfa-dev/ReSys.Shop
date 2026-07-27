#import "../../../template/ctu-styles.typ": context-callout
== FRONTEND TECHNOLOGY STACK

This section describes the technologies used to build the customer-facing website: Vue.js, Tailwind CSS, and supporting libraries.

=== Vue 3 and the Composition API

The frontend is built with *Vue 3*, a JavaScript framework for building user interfaces. Vue was chosen because:

- Gentle learning curve with good documentation
- Active community and ecosystem
- Works well for single-page applications (SPAs)

Vue 3 introduced the *Composition API*, which organizes code by logical concern rather than by component option:

```typescript
// Example: Image search composable
export function useImageSearch() {
  // Reactive state
  const searchResults = ref<Product[]>([]);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  // Business logic
  async function searchByImage(file: File) {
    isLoading.value = true;
    error.value = null;
    try {
      const formData = new FormData();
      formData.append('image', file);
      const response = await api.post('/search/image', formData);
      searchResults.value = response.data.products;
    } catch (e) {
      error.value = 'Search failed. Please try again.';
    } finally {
      isLoading.value = false;
    }
  }

  return { searchResults, isLoading, error, searchByImage };
}
```

This pattern keeps related code together and makes it reusable across components.

=== TypeScript for Type Safety

The project uses *TypeScript* instead of plain JavaScript. TypeScript adds static type checking, which helps catch errors during development:

```typescript
// Types help catch errors before runtime
interface Product {
  id: string;
  name: string;
  price: number;
  imageUrl: string;
}

// TypeScript warns if we try to access a non-existent property
const product: Product = await fetchProduct();
console.log(product.nmae); // Error: Property 'nmae' does not exist
```

For a larger project like this, TypeScript reduces bugs and makes refactoring safer.

=== State Management with Pinia

*Pinia* is Vue's official state management library. It stores data that needs to be shared across multiple components:

```typescript
// stores/search.ts
export const useSearchStore = defineStore('search', () => {
  const results = ref<Product[]>([]);
  const queryImage = ref<string | null>(null);

  function setResults(products: Product[]) {
    results.value = products;
  }

  return { results, queryImage, setResults };
});
```

When search results arrive, they are stored in Pinia. Any component can then access these results without making another API request.

=== Styling with Tailwind CSS

*Tailwind CSS* is a utility-first CSS framework. Instead of writing custom CSS classes, utility classes are applied directly to HTML:

```html
<!-- Traditional CSS approach -->
<button class="primary-button">Search</button>
<!-- Requires separate .primary-button CSS definition -->

<!-- Tailwind approach -->
<button class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
  Search
</button>
<!-- Styling is visible inline -->
```

Benefits for this project:
- Faster iteration by eliminating the need to switch between files
- Consistent spacing and colors via design tokens
- Smaller final CSS (unused styles are purged)

=== Vite for Development

*Vite* is a build tool that makes frontend development faster:

- *Hot Module Replacement (HMR):* Changes appear instantly in the browser
- *Fast builds:* Uses native ES modules during development
- *Optimized production builds:* Output is minified and tree-shaken

Vite significantly improved development speed compared to older bundlers like Webpack.

=== Image Upload Component

The visual search feature centers on the image upload component:

#figure(
  image("/images/ui/store/ui-store-visualsearch-empty.png", width: 100%),
  caption: [The image upload dialog before an image is selected],
)

Key features:
- *Drag and drop:* Users can drag images onto the component
- *Preview:* Uploaded image is shown before searching
- *Progress indicator:* Shows loading state during search
- *Error handling:* Displays helpful messages if something goes wrong

=== API Integration

The frontend communicates with the backend via HTTP requests:

```typescript
// api/search.ts
export async function searchByImage(file: File): Promise<SearchResult[]> {
  const formData = new FormData();
  formData.append('image', file);

  const response = await fetch('/api/search/image', {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    throw new Error('Search failed');
  }

  const data = await response.json();
  return data.products;
}
```

All API calls are centralized in an `api/` folder, making it easier to update endpoints if they change.

=== Summary of Frontend Libraries

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (center, left),
    [*Library*], [*Purpose*],
    [Vue 3], [Component framework],
    [TypeScript], [Static type checking],
    [Pinia], [State management],
    [Vue Router], [Page navigation],
    [Tailwind CSS], [Utility-first styling],
    [Vite], [Build tool and dev server],
    [PrimeVue], [UI component library],
  ),
  caption: [Key frontend libraries and their purposes],
)

These tools combine to create a responsive, maintainable user interface that provides a smooth visual search experience.


