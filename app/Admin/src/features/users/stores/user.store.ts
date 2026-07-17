import { defineStore } from 'pinia';
import { ref } from 'vue';
import { useToast } from '@/shared/composables/toast.use';
import { userService } from '../services/user.service';
import type { AdminUserSummary, CustomerSummary } from '../types/user.domain.types'
import type { UserSearchParams, CreateAdminUserRequest, UpdateAdminUserRequest } from '../types/user.request.types';

export const useUserStore = defineStore('user', () => {
  const { showToast } = useToast();

  // --- STATE ---
  const admins = ref<AdminUserSummary[]>([]);
  const customers = ref<CustomerSummary[]>([]);
  const loading = ref(false);
  const submitting = ref(false);
  const error = ref<string | null>(null);

  const query = ref<UserSearchParams>({
    page: 1,
    pageSize: 10,
    search: '',
    sort: ['-createdAtUtc']
  });

  const totalRecords = ref(0);

  // --- ACTIONS ---
  async function fetchAdmins(params: UserSearchParams = {}) {
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

  async function fetchCustomers(params: UserSearchParams = {}) {
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
        showToast('success', 'Created', 'Staff account created');
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
        showToast('success', 'Deleted', 'Staff account removed');
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
