import { defineStore } from 'pinia';
import { ref } from 'vue';
import { useToast } from '@/common/composables/toast.use';
import { useI18n } from 'vue-i18n';
import { userService } from '../services/user.service';
import type { AdminUserSummary, CustomerSummary } from '../types/user.response.type'
import type { UserQuery } from '../types/user.query.type'
import type { CreateAdminUserRequest, UpdateAdminUserRequest } from '../types/user.request.type';

export const useUserStore = defineStore('user', () => {
  const { showToast } = useToast();
  const { t } = useI18n();

  // --- STATE ---
  const admins = ref<AdminUserSummary[]>([]);
  const customers = ref<CustomerSummary[]>([]);
  const currentCustomer = ref<CustomerSummary | null>(null);
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
      if (result.isSuccess && result.items) {
        admins.value = result.items;
        totalRecords.value = result.totalCount || 0;
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
      if (result.isSuccess && result.items) {
        customers.value = result.items;
        totalRecords.value = result.totalCount || 0;
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  async function fetchCustomerById(id: string) {
    loading.value = true;
    try {
      const result = await userService.getById(id);
      if (result.isSuccess && result.value) {
        currentCustomer.value = {
          id: result.value.id,
          email: result.value.email,
          firstName: result.value.firstName,
          lastName: result.value.lastName,
          fullName: result.value.fullName,
          phoneNumber: result.value.phoneNumber,
          ordersCount: 0,
          totalSpent: 0,
          isActive: result.value.isActive,
          createdAtUtc: result.value.createdAtUtc,
        };
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
    currentCustomer,
    loading,
    submitting,
    error,
    query,
    totalRecords,
    fetchAdmins,
    fetchCustomers,
    fetchCustomerById,
    createAdmin,
    deleteAdmin
  };
});
