<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useToast } from '@/shared/composables/toast.use';
import { useConfirm } from 'primevue/useconfirm';
import { roleService } from '../../services/role.service';
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
import type { RoleSummary } from '../../types/user.domain.types';
import type { DataTablePageEvent } from 'primevue/datatable';

const router = useRouter();
const { showToast } = useToast();
const confirm = useConfirm();

const roles = ref<RoleSummary[]>([]);
const loading = ref(false);
const totalRecords = ref(0);
const query = ref({
    page: 1,
    pageSize: 20
});

onMounted(() => {
    fetchRoles();
});

async function fetchRoles() {
    loading.value = true;
    try {
        const res = await roleService.list(query.value);
        if (res.isSuccess && res.value) {
            roles.value = res.value;
            totalRecords.value = res.value.length || 0;
        }
    } finally {
        loading.value = false;
    }
}

const onPage = (event: DataTablePageEvent) => {
    query.value.page = event.page !== undefined ? event.page + 1 : 1;
    query.value.pageSize = event.rows;
    fetchRoles();
};

const confirmDelete = (role: RoleSummary) => {
    confirm.require({
        message: `Are you sure you want to delete the role "${role.displayName || role.name}"? This cannot be undone.`,
        header: 'Delete Role',
        icon: 'pi pi-exclamation-triangle',
        acceptClass: 'p-button-danger',
        accept: async () => {
            const res = await roleService.delete(role.id);
            if (res.isSuccess) {
                showToast('success', 'Deleted', 'Role deleted successfully');
                fetchRoles();
            }
        }
    });
};
</script>

<template>
    <div class="p-6">
        <AppBreadcrumb :locales="{ titles: { list: 'Roles' }, descriptions: { list: 'Manage system roles and access control.' } }" />
        <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
            <template #title>
                <div class="flex items-center justify-between p-4">
                    <div class="flex flex-col gap-1">
                        <div class="flex items-center gap-3">
                            <span class="text-xl font-bold">Roles</span>
                            <Badge :value="totalRecords" severity="info" />
                        </div>
                        <span class="text-sm text-surface-500">Manage system roles and access control.</span>
                    </div>
                    <Button label="Create Role" icon="pi pi-plus" severity="primary" class="rounded-xl" @click="router.push({ name: 'role-create' })" />
                </div>
            </template>
            <template #content>
                <DataTable 
                    :value="roles" 
                    :loading="loading" 
                    lazy 
                    paginator 
                    :rows="query.pageSize" 
                    :totalRecords="totalRecords" 
                    @page="onPage"
                    dataKey="id"
                    rowHover
                    scrollable
                    stripedRows
                    showGridlines
                >
                    <Column field="name" header="Role Name" sortable>
                        <template #body="{ data }">
                            <div class="flex flex-col">
                                <span class="font-bold">{{ data.displayName || data.name }}</span>
                                <small class="font-mono text-[10px] text-surface-500">{{ data.name }}</small>
                            </div>
                        </template>
                    </Column>

                    <Column field="priority" header="Priority" sortable>
                        <template #body="{ data }">
                            <Badge :value="data.priority" severity="info" />
                        </template>
                    </Column>

                    <Column field="userCount" header="Users">
                        <template #body="{ data }">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-users text-surface-400"></i>
                                <span>{{ data.userCount }}</span>
                            </div>
                        </template>
                    </Column>
                    
                    <Column header="Type">
                        <template #body="{ data }">
                            <Tag v-if="data.isSystem" value="System" severity="warning" icon="pi pi-lock" rounded />
                            <Tag v-else value="Custom" severity="secondary" rounded />
                            <Tag v-if="data.isDefault" value="Default" severity="success" class="ml-2" rounded />
                        </template>
                    </Column>

                    <Column header="Actions" class="w-48 text-right">
                        <template #body="{ data }">
                            <div class="flex justify-end gap-1">
                                <Button icon="pi pi-shield" text rounded v-tooltip.top="'Permissions'" @click="router.push({ name: 'role-permissions', params: { id: data.id } })" />
                                <Button icon="pi pi-pencil" text rounded severity="secondary" @click="router.push({ name: 'role-edit', params: { id: data.id } })" />
                                <Button icon="pi pi-trash" text rounded severity="danger" :disabled="data.isSystem" @click="confirmDelete(data)" />
                            </div>
                        </template>
                    </Column>
                </DataTable>
            </template>
        </Card>
    </div>
</template>
