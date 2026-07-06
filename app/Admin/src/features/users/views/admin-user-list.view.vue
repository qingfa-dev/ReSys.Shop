<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useUserStore } from '../stores/user.store';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { useConfirm } from 'primevue/useconfirm';
import { useFormatter } from '@/shared/composables/formatter.use';
import { userLocales as t } from '../locales/user.locales';
import AppBreadcrumb from '@/shared/components/breadcrumb.component.vue';
import { FilterMatchMode } from '@primevue/core/api';
import type { DataTablePageEvent, DataTableSortEvent, DataTableFilterMeta } from 'primevue/datatable';
import type { AdminUserSummary } from '../types/user.types';

const store = useUserStore();
const { admins, loading, totalRecords, query } = storeToRefs(store);
const router = useRouter();
const confirm = useConfirm();
const { formatDate } = useFormatter();

const filters = ref<DataTableFilterMeta>({
  global: { value: query.value.search || null, matchMode: FilterMatchMode.CONTAINS },
});

onMounted(() => {
    loadUsers();
});

const loadUsers = async () => {
    await store.fetchAdmins();
};

const onPage = (event: DataTablePageEvent) => {
    store.fetchAdmins({
        page: event.page !== undefined ? event.page + 1 : 1,
        page_size: event.rows,
    });
};

const onSort = (event: DataTableSortEvent) => {
    store.fetchAdmins({
        sort_by: event.sortField as string,
        is_descending: event.sortOrder === -1,
        page: 1,
    });
};

const onFilter = () => {
    const globalFilter = filters.value.global as { value: string | null };
    store.fetchAdmins({
        search: globalFilter.value || undefined,
        page: 1,
    });
};

const clearFilters = () => {
    filters.value = {
        global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    };
    onFilter();
};

const confirmDelete = (user: AdminUserSummary) => {
    const messageStr = (t.confirm?.delete_message as string).replace('{email}', user.email);

    confirm.require({
        message: messageStr,
        header: t.confirm?.delete_header as string,
        icon: 'pi pi-exclamation-triangle',
        rejectLabel: t.confirm?.reject_label as string,
        acceptProps: {
            label: t.confirm?.accept_label as string,
            severity: 'danger',
        },
        accept: async () => {
            await store.deleteAdmin(user.id);
        }
    });
};
</script>

<template>
  <div class="p-6">
    <AppBreadcrumb :locales="t" />
    <div class="flex flex-col items-start justify-between gap-4 mb-8 md:flex-row md:items-center">
      <div>
        <h2 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50">
          {{ t.titles.list }}
        </h2>
        <div class="flex items-center gap-2 mt-1">
          <span class="text-surface-500 dark:text-surface-400">
            {{ t.descriptions?.list }}
          </span>
          <Badge :value="totalRecords" severity="info" class="ml-2"></Badge>
        </div>
      </div>
      <div class="flex w-full gap-3 md:w-auto">
        <Button
          :label="t.actions.new"
          icon="pi pi-user-plus"
          class="px-4 shadow-lg rounded-xl"
        />
      </div>
    </div>

    <div class="overflow-hidden border shadow-sm bg-surface-0 dark:bg-surface-900 rounded-2xl border-surface-100 dark:border-surface-800">
      <DataTable
        v-model:filters="filters"
        :value="admins"
        :loading="loading"
        :totalRecords="totalRecords"
        :lazy="true"
        @page="onPage"
        @sort="onSort"
        @filter="onFilter"
        :paginator="true"
        :rows="query.page_size || 10"
        :first="((query.page || 1) - 1) * (query.page_size || 10)"
        :sortField="query.sort_by"
        :sortOrder="query.is_descending ? -1 : 1"
        dataKey="id"
        filterDisplay="menu"
        removableSort
        scrollable
        rowHover
      >
        <template #header>
          <div class="flex flex-col items-center justify-between gap-4 md:flex-row">
            <IconField iconPosition="left" class="w-full md:w-72">
              <InputIcon class="pi pi-search" />
              <InputText
                v-model="(filters.global as any).value"
                :placeholder="t.placeholders?.search"
                @keyup.enter="onFilter"
                class="w-full rounded-xl"
              />
            </IconField>

            <Button
              type="button"
              icon="pi pi-filter-slash"
              :label="t.table?.clear_filter"
              outlined
              @click="clearFilters"
              class="w-full rounded-xl md:w-auto"
            />
          </div>
        </template>

        <template #empty>
          <div class="flex flex-col items-center justify-center py-20 text-surface-400">
            <i class="mb-4 text-6xl pi pi-users opacity-20"></i>
            <p class="text-xl font-medium">{{ t.messages?.empty_list }}</p>
          </div>
        </template>

        <Column field="full_name" :header="t.table?.user" sortable>
            <template #body="{ data }">
                <div class="flex flex-col">
                    <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.full_name || 'Incomplete Profile' }}</span>
                    <small class="font-mono text-[10px] text-surface-500 uppercase tracking-widest">{{ data.email }}</small>
                </div>
            </template>
        </Column>

        <Column field="role_names" :header="t.table?.roles">
            <template #body="{ data }">
                <div class="flex gap-1">
                    <Tag v-for="r in data.role_names" :key="r" :value="r" severity="info" class="text-[9px] font-black uppercase" />
                </div>
            </template>
        </Column>

        <Column field="is_active" :header="t.table?.status">
            <template #body="{ data }">
                <Tag :value="data.is_active ? 'Active' : 'Inactive'" :severity="data.is_active ? 'success' : 'secondary'" rounded class="font-bold px-3" />
            </template>
        </Column>

        <Column field="created_at" :header="t.table?.joined" sortable>
            <template #body="{ data }">
                <span class="text-sm">{{ formatDate(data.created_at) }}</span>
            </template>
        </Column>

        <Column :header="t.table?.actions" class="w-32 text-right" frozen alignFrozen="right">
          <template #body="{ data }">
            <div class="flex justify-end gap-1">
              <Button icon="pi pi-eye" severity="secondary" text rounded @click="router.push({ name: 'admin-user-detail', params: { id: data.id } })" />
              <Button icon="pi pi-pencil" severity="secondary" text rounded />
              <Button icon="pi pi-trash" severity="danger" text rounded @click="confirmDelete(data)" />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>

<style scoped>
:deep(.p-datatable-header) {
  background: transparent;
  padding: 1rem;
}
:deep(.p-datatable-thead > tr > th) {
  background: var(--p-content-background);
  color: var(--p-text-color);
  font-size: 0.875rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.025em;
  padding: 1rem 1.5rem;
  border-bottom: 2px solid var(--p-primary-color);
}
:deep(.p-datatable-tbody > tr > td) {
  padding: 1rem 1.5rem;
  border-bottom: 1px solid var(--p-content-border-color);
}
</style>