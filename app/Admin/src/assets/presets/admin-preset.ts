import { definePreset } from '@primevue/themes'
import Aura from '@primevue/themes/aura'

export const AdminPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '{indigo.50}',
      100: '{indigo.100}',
      200: '{indigo.200}',
      300: '{indigo.300}',
      400: '{indigo.400}',
      500: '{indigo.500}',
      600: '{indigo.600}',
      700: '{indigo.700}',
      800: '{indigo.800}',
      900: '{indigo.900}',
      950: '{indigo.950}',
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
