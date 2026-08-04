import { ref } from 'vue'
import type DataTable from 'primevue/datatable'

export function useDataTableExport() {
  const dt = ref<InstanceType<typeof DataTable>>()

  function exportCSV() {
    dt.value?.exportCSV()
  }

  return { dt, exportCSV }
}
