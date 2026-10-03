import { definePreset } from '@primevue/themes'
import Aura from '@primevue/themes/aura'

export const AdminPreset = definePreset(Aura, {
  semantic: {
      primary: {
        50: '{emerald.50}',
        100: '{emerald.100}',
        200: '{emerald.200}',
        300: '{emerald.300}',
        400: '{emerald.400}',
        500: '{emerald.500}',
        600: '{emerald.600}',
        700: '{emerald.700}',
        800: '{emerald.800}',
        900: '{emerald.900}',
        950: '{emerald.950}',
      },
    colorScheme: {
      light: {
        surface: { 0: '#ffffff', 50: '{slate.50}', 100: '{slate.100}', 200: '{slate.200}' },
        content: { background: '#ffffff', borderColor: '{slate.200}' },
      },
      dark: {
        surface: { 0: '#0f172a', 50: '{slate.900}', 100: '{slate.800}', 200: '{slate.700}' },
        content: { background: '{slate.900}', borderColor: '{slate.700}' },
      },
    },
  },
  components: {
    button: { root: { borderRadius: '0.5rem' } },
    datatable: {
      root: { borderColor: '{content.border.color}' },
      headerCell: { fontWeight: '600' },
    },
    card: { root: { borderRadius: '0.75rem' } },
    tag: { root: { borderRadius: '9999px' } },
  },
})
