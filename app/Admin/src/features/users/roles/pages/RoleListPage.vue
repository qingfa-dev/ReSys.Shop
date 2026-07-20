<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useToast } from '@/common/composables/toast.use';
import { useConfirm } from 'primevue/useconfirm';
import { useI18n } from 'vue-i18n';
import { roleRepository } from '../api/role.api';
import PageShell from '@/shared/components/navigation/PageShell.vue';
import PageHeader from '@/shared/components/navigation/PageHeader.vue';
import type { RoleSummary } from '../types/role.response';
import type { DataTablePageEvent } from 'primevue/datatable';

const router = useRouter();
const { showToast } = useToast();
const { t } = useI18n();
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
        const res = await roleRepository.list(query.value);
        if (res.isSuccess && res.items) {
            roles.value = res.items;
            totalRecords.value = res.totalCount || 0;
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
            const res = await roleRepository.delete(role.id);
            if (res.isSuccess) {
                showToast('success', t('common.deleted'), t('roles.messages.delete_success'));
                fetchRoles();
            }
        }
    });
};
</script>

<template>
    <PageShell maxWidth="7xl">
        <PageHeader :title="t('roles.titles.list')" description="Manage system roles and access control.">
            <template #badge>
                <Badge :value="totalRecords" severity="info" />
            </template>
            <template #actions>
                <Button :label="t('roles.actions.create')" icon="pi pi-plus" severity="primary" class="rounded-xl" @click="router.push({ name: 'users.roles.create' })" />
            </template>
        </PageHeader>
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
                    <Column field="name" :header="t('roles.table.name')" sortable>
                        <template #body="{ data }">
                            <div class="flex flex-col">
                                <span class="font-bold">{{ data.displayName || data.name }}</span>
                                <small class="font-mono text-[10px] text-surface-500">{{ data.name }}</small>
                            </div>
                        </template>
                    </Column>

                    <Column field="priority" :header="t('roles.table.priority')" sortable>
                        <template #body="{ data }">
                            <Badge :value="data.priority" severity="info" />
                        </template>
                    </Column>

                    <Column field="userCount" :header="t('roles.table.users')">
                        <template #body="{ data }">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-users text-surface-400"></i>
                                <span>{{ data.userCount }}</span>
                            </div>
                        </template>
                    </Column>
                    
                    <Column :header="t('roles.table.type')">
                        <template #body="{ data }">
                            <Tag v-if="data.isSystem" value="System" severity="warning" icon="pi pi-lock" rounded />
                            <Tag v-else value="Custom" severity="secondary" rounded />
                            <Tag v-if="data.isDefault" value="Default" severity="success" class="ml-2" rounded />
                        </template>
                    </Column>

                    <Column :header="t('roles.table.actions')" class="w-48 text-right">
                        <template #body="{ data }">
                            <div class="flex justify-end gap-1">
                                <Button icon="pi pi-shield" text rounded v-tooltip.top="'Permissions'" @click="router.push({ name: 'users.roles.permissions', params: { id: data.id } })" />
                                <Button icon="pi pi-pencil" text rounded severity="secondary" @click="router.push({ name: 'users.roles.edit', params: { id: data.id } })" />
                                <Button icon="pi pi-trash" text rounded severity="danger" :disabled="data.isSystem" @click="confirmDelete(data)" />
                            </div>
                        </template>
                    </Column>
                </DataTable>
    </PageShell>
</template>
