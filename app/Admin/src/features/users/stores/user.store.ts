import { defineStore } from 'pinia';
import { ref } from 'vue';
import { useToast } from '@/shared/composables/toast.use';
import { useI18n } from 'vue-i18n';
import { userService } from '../services/user.service';
import type { AdminUserSummary, CustomerSummary } from '../types/User.Response.Type'
import type { UserQuery } from '../types/User.Query.Type'
import type { CreateAdminUserRequest, UpdateAdminUserRequest } from '../types/User.Request.Type';

export const useUserStore = defineStore('user', () => {
  const { showToast } = useToast();
  const { t } = useI18n();

  // --- STATE ---
  const admins = ref<AdminUserSummary[]>([]);
  const customers = ref<CustomerSummary[]>([]);
  const loading = ref(false);
  const submitting = ref(false);
  const error = ref<string | null>(null);

  const query = ref<UserQuery>({
    page: 1,
    pageSize: 10,
    search: '',
    sort: ['-createdAtUtc']
  });

  const totalRecords = ref(0);

  // --- ACTIONS ---
  async function fetchAdmins(params: UserQuery = {}) {
    loading.value = true;
    query.value = { ...query.value, ...params };
    try {
      const result = await userService.list(query.value);
      if (result.isSuccess && result.value) {
        admins.value = result.value;
        totalRecords.value = result.value.length || 0;
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  async function fetchCustomers(params: UserQuery = {}) {
    loading.value = true;
    query.value = { ...query.value, ...params };
    try {
      const result = await userService.listCustomers(query.value);
      if (result.isSuccess && result.value) {
        customers.value = result.value;
        totalRecords.value = result.value.length || 0;
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  async function createAdmin(data: CreateAdminUserRequest) {
    submitting.value = true;
    try {
      const result = await userService.create(data);
      if (result.isSuccess) {
        showToast('success', t('common.created'), t('users.messages.create_success'));
        await fetchAdmins();
      }
      return result;
    } finally {
      submitting.value = false;
    }
  }

  async function deleteAdmin(id: string) {
    loading.value = true;
    try {
      const result = await userService.delete(id);
      if (result.isSuccess) {
        showToast('success', t('common.deleted'), t('users.messages.delete_success'));
        await fetchAdmins();
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  return {
    admins,
    customers,
    loading,
    submitting,
    error,
    query,
    totalRecords,
    fetchAdmins,
    fetchCustomers,
    createAdmin,
    deleteAdmin
  };
});
