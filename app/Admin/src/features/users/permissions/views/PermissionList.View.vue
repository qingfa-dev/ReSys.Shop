<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { permissionService } from '../../services/permission.service';
import type { PermissionSummary } from '../../types/user.types';
import type { DataTablePageEvent } from 'primevue/datatable';

const permissions = ref<PermissionSummary[]>([]);
const loading = ref(false);
const totalRecords = ref(0);
const query = ref({
    page: 1,
    pageSize: 50,
    search: ''
});

onMounted(() => {
    fetchPermissions();
});

async function fetchPermissions() {
    loading.value = true;
    try {
        const res = await permissionService.list(query.value);
        if (res.success && res.data) {
            permissions.value = res.data;
            totalRecords.value = res.meta?.totalCount || 0;
        }
    } finally {
        loading.value = false;
    }
}

const onPage = (event: DataTablePageEvent) => {
    query.value.page = event.page !== undefined ? event.page + 1 : 1;
    query.value.pageSize = event.rows;
    fetchPermissions();
};
</script>

<template>
    <Card>
        <div class="flex justify-between items-center mb-8">
            <div>
                <h1 class="text-3xl font-black uppercase tracking-tighter text-surface-900 dark:text-surface-0">Permissions</h1>
                <p class="text-surface-500">View available system permissions.</p>
            </div>
        </div>

        <div class="overflow-hidden border border-surface-100 dark:border-surface-800 rounded-2xl shadow-sm">
            <DataTable 
                :value="permissions" 
                :loading="loading" 
                lazy 
                paginator 
                :rows="query.pageSize" 
                :totalRecords="totalRecords" 
                @page="onPage"
                rowGroupMode="subheader"
                groupRowsBy="module"
                sortMode="single"
                sortField="module"
                :sortOrder="1"
                stripedRows
                showGridlines
            >
                <template #groupheader="slotProps">
                    <div class="flex items-center gap-2 px-4 py-2 bg-surface-50 dark:bg-surface-900 border-y border-surface-200 dark:border-surface-700">
                        <i class="pi pi-box text-primary"></i>
                        <span class="font-bold text-surface-900 dark:text-surface-0">{{ slotProps.data.module }}</span>
                    </div>
                </template>
                
                <Column field="module" header="Module"></Column>

                <Column field="name" header="Permission Key" class="font-mono text-sm max-w-[200px] truncate"></Column>

                <Column field="displayName" header="Name">
                    <template #body="{ data }">
                        <span class="font-bold">{{ data.displayName }}</span>
                    </template>
                </Column>

                <Column field="description" header="Description" class="text-surface-500"></Column>
            </DataTable>
        </div>
</Card>
</template>
