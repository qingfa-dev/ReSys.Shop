<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useUserStore } from '../store/user.store';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import ConfirmDialog from '@/shared/components/overlays/ConfirmDialog.vue';
import { useFormatter } from '@/common/composables/formatter.use';
import { useI18n } from 'vue-i18n';
import PageShell from '@/shared/components/navigation/PageShell.vue';
import PageHeader from '@/shared/components/navigation/PageHeader.vue';
import { FilterMatchMode } from '@primevue/core/api';
import type { DataTablePageEvent, DataTableSortEvent, DataTableFilterMeta } from 'primevue/datatable';
import { getFilterValue } from '@/common/api/types/filter.types';

const store = useUserStore();
const { admins, loading, totalRecords, query } = storeToRefs(store);
const router = useRouter();
const { formatDate } = useFormatter();
const { t } = useI18n();

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
        pageSize: event.rows,
    });
};

const onSort = (event: DataTableSortEvent) => {
    store.fetchAdmins({
        sort: [event.sortOrder === -1 ? `-${event.sortField as string}` : event.sortField as string],
        page: 1,
    });
};

const onFilter = () => {
    const globalValue = getFilterValue(filters.value, 'global') as string | null;
    store.fetchAdmins({
        search: globalValue || undefined,
        searchFields: globalValue ? ['UserName', 'Email', 'FirstName', 'LastName'] : undefined,
        page: 1,
    });
};

const clearFilters = () => {
    filters.value = {
        global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    };
    onFilter();
};

const deleteAdmin = async (userId: string) => {
  await store.deleteAdmin(userId);
};
</script>

<template>
  <PageShell maxWidth="7xl">
    <PageHeader :title="t('users.titles.list')" :description="t('users.descriptions.list')">
      <template #badge>
        <Badge :value="totalRecords" severity="info" />
      </template>
      <template #actions>
        <Button :label="t('users.actions.new')" icon="pi pi-user-plus" severity="primary" class="rounded-xl" />
      </template>
    </PageHeader>
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
          :rows="query.pageSize || 10"
          :first="((query.page || 1) - 1) * (query.pageSize || 10)"
          :sortField="query.sort?.[0]?.replace(/^-/, '')"
          :sortOrder="query.sort?.[0]?.startsWith('-') ? -1 : 1"
          dataKey="id"
          filterDisplay="menu"
          removableSort
          scrollable
          rowHover
          stripedRows
          showGridlines
        >
          <template #header>
            <div class="flex items-center justify-between gap-4">
              <IconField iconPosition="left" class="w-full md:w-72">
                <InputIcon class="pi pi-search" />
                <InputText
                  v-model="(filters.global as { value: string | null }).value"
                  :placeholder="t('users.placeholders.search')"
                  @keyup.enter="onFilter"
                  class="w-full rounded-xl"
                />
              </IconField>
              <Button
                type="button"
                icon="pi pi-filter-slash"
                :label="t('users.table.clear_filter')"
                outlined
                @click="clearFilters"
                class="rounded-xl"
              />
            </div>
          </template>

          <template #empty>
            <div class="flex flex-col items-center justify-center py-20 text-surface-400">
              <i class="mb-4 text-6xl pi pi-users opacity-20"></i>
              <p class="text-xl font-medium">{{ t('users.messages.empty_list') }}</p>
            </div>
          </template>

          <Column field="fullName" :header="t('users.table.user')">
            <template #body="{ data }">
              <div class="flex flex-col">
                <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.fullName || 'Incomplete Profile' }}</span>
                <small class="font-mono text-[10px] text-surface-500 uppercase tracking-widest">{{ data.email }}</small>
              </div>
            </template>
          </Column>

          <Column field="isActive" :header="t('users.table.status')">
            <template #body="{ data }">
              <Tag :value="data.isActive ? 'Active' : 'Inactive'" :severity="data.isActive ? 'success' : 'secondary'" rounded class="font-bold px-3" />
            </template>
          </Column>

          <Column field="createdAtUtc" :header="t('users.table.joined')" sortable>
            <template #body="{ data }">
              <span class="text-sm">{{ formatDate(data.createdAtUtc) }}</span>
            </template>
          </Column>

          <Column :header="t('users.table.actions')" class="w-32 text-right" frozen alignFrozen="right">
            <template #body="{ data }">
              <div class="flex justify-end gap-1">
                <Button icon="pi pi-eye" severity="secondary" text rounded @click="router.push({ name: 'users.staff.detail', params: { id: data.id } })" />
                <Button icon="pi pi-pencil" severity="secondary" text rounded />
                <ConfirmDialog
                  :header="t('users.confirm.delete_header')"
                  :message="t('users.confirm.delete_message').replace('{email}', data.email)"
                  :accept-label="t('users.confirm.accept_label')"
                  :reject-label="t('users.confirm.reject_label')"
                  @confirm="deleteAdmin(data.id)" />
              </div>
            </template>
          </Column>
        </DataTable>
  </PageShell>
</template>
