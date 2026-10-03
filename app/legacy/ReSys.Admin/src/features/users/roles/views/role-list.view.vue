<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useToast } from '@/shared/composables/toast.use';
import { useConfirm } from 'primevue/useconfirm';
import { roleService } from '../../services/role.service';
import type { RoleSummary } from '../../types/user.types';
import type { DataTablePageEvent } from 'primevue/datatable';

const router = useRouter();
const { showToast } = useToast();
const confirm = useConfirm();

const roles = ref<RoleSummary[]>([]);
const loading = ref(false);
const totalRecords = ref(0);
const query = ref({
    page: 1,
    page_size: 20
});

onMounted(() => {
    fetchRoles();
});

async function fetchRoles() {
    loading.value = true;
    try {
        const res = await roleService.listRoles(query.value);
        if (res.success && res.data) {
            roles.value = res.data.items;
            totalRecords.value = res.data.total_count || 0;
        }
    } finally {
        loading.value = false;
    }
}

const onPage = (event: DataTablePageEvent) => {
    query.value.page = event.page !== undefined ? event.page + 1 : 1;
    query.value.page_size = event.rows;
    fetchRoles();
};

const confirmDelete = (role: RoleSummary) => {
    confirm.require({
        message: `Are you sure you want to delete the role "${role.display_name || role.name}"? This cannot be undone.`,
        header: 'Delete Role',
        icon: 'pi pi-exclamation-triangle',
        acceptClass: 'p-button-danger',
        accept: async () => {
            const res = await roleService.deleteRole(role.id);
            if (res.success) {
                showToast('success', 'Deleted', 'Role deleted successfully');
                fetchRoles();
            }
        }
    });
};
</script>

<template>
    <div class="card p-6">
        <div class="flex justify-between items-center mb-8">
            <div>
                <h1 class="text-3xl font-black uppercase tracking-tighter text-surface-900 dark:text-surface-0">Roles</h1>
                <p class="text-surface-500">Manage system roles and access control.</p>
            </div>
            <Button label="Create Role" icon="pi pi-plus" @click="router.push({ name: 'role-create' })" class="px-6 rounded-xl shadow-lg" />
        </div>

        <div class="overflow-hidden border border-surface-100 dark:border-surface-800 rounded-2xl shadow-sm">
            <DataTable 
                :value="roles" 
                :loading="loading" 
                lazy 
                paginator 
                :rows="query.page_size" 
                :totalRecords="totalRecords" 
                @page="onPage"
                dataKey="id"
                rowHover
            >
                <Column field="name" header="Role Name" sortable>
                    <template #body="{ data }">
                        <div class="flex flex-col">
                            <span class="font-bold">{{ data.display_name || data.name }}</span>
                            <small class="font-mono text-[10px] text-surface-500">{{ data.name }}</small>
                        </div>
                    </template>
                </Column>

                <Column field="priority" header="Priority" sortable>
                    <template #body="{ data }">
                        <Badge :value="data.priority" severity="info" />
                    </template>
                </Column>

                <Column field="user_count" header="Users">
                    <template #body="{ data }">
                         <div class="flex items-center gap-2">
                            <i class="pi pi-users text-surface-400"></i>
                            <span>{{ data.user_count }}</span>
                        </div>
                    </template>
                </Column>
                
                 <Column header="Type">
                    <template #body="{ data }">
                        <Tag v-if="data.is_system_role" value="System" severity="warning" icon="pi pi-lock" rounded />
                        <Tag v-else value="Custom" severity="secondary" rounded />
                        <Tag v-if="data.is_default" value="Default" severity="success" class="ml-2" rounded />
                    </template>
                </Column>

                <Column header="Actions" class="w-48 text-right">
                    <template #body="{ data }">
                        <div class="flex justify-end gap-1">
                            <Button icon="pi pi-shield" text rounded v-tooltip.top="'Permissions'" @click="router.push({ name: 'role-permissions', params: { id: data.id } })" />
                            <Button icon="pi pi-pencil" text rounded severity="secondary" @click="router.push({ name: 'role-edit', params: { id: data.id } })" />
                            <Button icon="pi pi-trash" text rounded severity="danger" :disabled="data.is_system_role" @click="confirmDelete(data)" />
                        </div>
                    </template>
                </Column>
            </DataTable>
        </div>
    </div>
</template>