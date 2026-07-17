<script setup lang="ts">
import { onMounted, computed } from 'vue';
import { useReportStore } from '../stores/report.store';
import { storeToRefs } from 'pinia';
import { useFormatter } from '@/shared/composables/formatter.use';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const store = useReportStore();
const { sales, inventory, catalog, activities, is_loading } = storeToRefs(store);
const { formatCurrency, formatDate } = useFormatter();

onMounted(() => {
    store.fetchDashboardData();
});

const chartData = computed(() => {
    if (!sales.value?.trendHistory) return null;
    
    return {
        labels: sales.value.trendHistory.map(h => new Date(h.date).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })),
        datasets: [
            {
                label: 'Revenue',
                data: sales.value.trendHistory.map(h => h.revenue),
                fill: true,
                borderColor: '#3b82f6',
                tension: 0.4,
                backgroundColor: 'rgba(59, 130, 246, 0.1)'
            }
        ]
    };
});

const chartOptions = {
    maintainAspectRatio: false,
    aspectRatio: 0.6,
    plugins: {
        legend: {
            display: false
        }
    },
    scales: {
        x: {
            grid: {
                display: false
            }
        },
        y: {
            grid: {
                color: 'rgba(0,0,0,0.05)'
            }
        }
    }
};

const getStatusSeverity = (status: string) => {
    switch (status?.toLowerCase()) {
        case 'complete': return 'success';
        case 'processing': return 'info';
        case 'canceled': return 'danger';
        default: return 'secondary';
    }
};

const recentOrders = computed(() => {
    return activities.value?.filter(a => a.type === 'Order') || [];
});

const recentActivity = computed(() => {
    return activities.value || [];
});

const getActivityIcon = (type: string) => {
    return type === 'Order' ? 'pi pi-shopping-bag' : 'pi pi-box';
};

const getActivityColor = (type: string) => {
    return type === 'Order' ? 'text-blue-500' : 'text-orange-500';
};
</script>

<template>
    <div class="flex flex-col gap-8">
        <!-- Page Header -->
        <Card class="border-none shadow-none bg-transparent">
            <template #content>
                <div class="flex items-center justify-between">
                    <div>
                        <h1 class="text-3xl font-bold text-surface-900 dark:text-surface-0">{{ t('navigation.dashboard') }}</h1>
                        <p class="mt-1 text-surface-500 dark:text-surface-400">Real-time performance overview</p>
                    </div>
                    <Button
                        icon="pi pi-refresh"
                        severity="secondary"
                        text
                        rounded
                        :loading="is_loading"
                        @click="store.fetchDashboardData()"
                    />
                </div>
            </template>
        </Card>

        <!-- Revenue Chart -->
        <Card v-if="chartData" class="border border-surface-100 dark:border-surface-800 shadow-sm">
            <template #title>
                <div class="flex items-center justify-between">
                    <span class="text-lg font-semibold text-surface-700 dark:text-surface-100">Revenue Trend (30 Days)</span>
                    <i class="pi pi-chart-line text-primary text-xl"></i>
                </div>
            </template>
            <template #content>
                <div class="h-64">
                    <Chart type="line" :data="chartData" :options="chartOptions" class="h-full" />
                </div>
            </template>
        </Card>

        <!-- Stats Row -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            <Card class="border border-surface-100 dark:border-surface-800 shadow-sm">
                <template #content>
                    <div class="flex items-start justify-between">
                        <div class="flex flex-col gap-2">
                            <span class="text-sm font-medium text-surface-500 dark:text-surface-400">Total Orders</span>
                            <span class="text-3xl font-bold text-surface-900 dark:text-surface-0">{{ sales?.orderCount || 0 }}</span>
                        </div>
                        <div class="flex items-center justify-center w-12 h-12 rounded-lg bg-primary/10">
                            <i class="pi pi-shopping-bag text-xl text-primary"></i>
                        </div>
                    </div>
                </template>
            </Card>

            <Card class="border border-surface-100 dark:border-surface-800 shadow-sm">
                <template #content>
                    <div class="flex items-start justify-between">
                        <div class="flex flex-col gap-2">
                            <span class="text-sm font-medium text-surface-500 dark:text-surface-400">{{ t('reports.labels.revenue') }}</span>
                            <span class="text-3xl font-bold text-surface-900 dark:text-surface-0">{{ formatCurrency(sales?.totalRevenue || 0) }}</span>
                        </div>
                        <div class="flex items-center justify-center w-12 h-12 rounded-lg bg-primary/10">
                            <i class="pi pi-dollar text-xl text-primary"></i>
                        </div>
                    </div>
                </template>
            </Card>

            <Card class="border border-surface-100 dark:border-surface-800 shadow-sm">
                <template #content>
                    <div class="flex items-start justify-between">
                        <div class="flex flex-col gap-2">
                            <span class="text-sm font-medium text-surface-500 dark:text-surface-400">Active Products</span>
                            <span class="text-3xl font-bold text-surface-900 dark:text-surface-0">{{ catalog?.activeProducts || 0 }}</span>
                        </div>
                        <div class="flex items-center justify-center w-12 h-12 rounded-lg bg-primary/10">
                            <i class="pi pi-box text-xl text-primary"></i>
                        </div>
                    </div>
                </template>
            </Card>

            <Card class="border border-surface-100 dark:border-surface-800 shadow-sm">
                <template #content>
                    <div class="flex items-start justify-between">
                        <div class="flex flex-col gap-2">
                            <span class="text-sm font-medium text-surface-500 dark:text-surface-400">Pending Fulfillment</span>
                            <span class="text-3xl font-bold text-surface-900 dark:text-surface-0">{{ inventory?.lowStockCount || 0 }}</span>
                        </div>
                        <div class="flex items-center justify-center w-12 h-12 rounded-lg bg-primary/10">
                            <i class="pi pi-truck text-xl text-primary"></i>
                        </div>
                    </div>
                </template>
            </Card>
        </div>

        <!-- Content Grid: Recent Orders + Activity Feed -->
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <!-- Recent Orders DataTable -->
            <div class="lg:col-span-2">
                <Card class="border border-surface-100 dark:border-surface-800 shadow-sm">
                    <template #title>
                        <div class="flex items-center justify-between">
                            <span class="text-lg font-semibold text-surface-700 dark:text-surface-100">Recent Orders</span>
                            <Button
                                icon="pi pi-arrow-right"
                                severity="secondary"
                                text
                                rounded
                                class="p-0"
                            />
                        </div>
                    </template>
                    <template #content>
                        <DataTable
                            :value="recentOrders"
                            :loading="is_loading"
                            stripedRows
                            rowHover
                            responsiveLayout="scroll"
                            class="p-datatable-sm -mx-4 -mb-4"
                        >
                            <Column field="title" :header="t('ordering.table.order_number')" :style="{ minWidth: '12rem' }">
                                <template #body="{ data }">
                                    <div class="flex items-center gap-3">
                                        <Avatar
                                            :icon="getActivityIcon(data.type)"
                                            class="bg-primary-50 dark:bg-primary-500/10 text-primary"
                                            size="small"
                                            shape="circle"
                                        />
                                        <div>
                                            <span class="font-medium text-surface-900 dark:text-surface-0">{{ data.title }}</span>
                                            <p class="text-xs text-surface-500 dark:text-surface-400 mt-0.5">{{ data.description }}</p>
                                        </div>
                                    </div>
                                </template>
                            </Column>
                            <Column field="timestamp" :header="t('ordering.table.date')" :style="{ minWidth: '8rem' }">
                                <template #body="{ data }">
                                    <span class="text-sm text-surface-600 dark:text-surface-300">{{ formatDate(data.timestamp) }}</span>
                                </template>
                            </Column>
                            <Column field="status" :header="t('ordering.table.status')" :style="{ minWidth: '8rem' }">
                                <template #body="{ data }">
                                    <Tag
                                        :value="data.status"
                                        :severity="getStatusSeverity(data.status)"
                                        rounded
                                        class="text-xs font-semibold px-2"
                                    />
                                </template>
                            </Column>
                        </DataTable>
                        <div
                            v-if="recentOrders.length === 0 && !is_loading"
                            class="flex flex-col items-center justify-center py-12 text-surface-400 dark:text-surface-500"
                        >
                            <i class="pi pi-shopping-bag text-4xl mb-3"></i>
                            <span class="text-sm font-medium">No recent orders</span>
                        </div>
                    </template>
                </Card>
            </div>

            <!-- Activity Feed -->
            <div class="lg:col-span-1">
                <Card class="border border-surface-100 dark:border-surface-800 shadow-sm">
                    <template #title>
                        <span class="text-lg font-semibold text-surface-700 dark:text-surface-100">{{ t('reports.labels.activity') }}</span>
                    </template>
                    <template #content>
                        <div class="flex flex-col gap-4 -mx-4 -mb-4">
                            <div
                                v-for="item in recentActivity"
                                :key="item.id"
                                class="flex items-start gap-3 px-4 py-3 border-b border-surface-100 dark:border-surface-800 last:border-b-0"
                            >
                                <Avatar
                                    :icon="getActivityIcon(item.type)"
                                    :class="[
                                        'flex-shrink-0',
                                        'bg-primary/10 text-primary'
                                    ]"
                                    size="small"
                                    shape="circle"
                                />
                                <div class="flex-1 min-w-0">
                                    <p class="text-sm font-medium text-surface-900 dark:text-surface-0 truncate">{{ item.title }}</p>
                                    <p class="text-xs text-surface-500 dark:text-surface-400 mt-0.5 truncate">{{ item.description }}</p>
                                </div>
                                <div class="flex flex-col items-end gap-1 flex-shrink-0">
                                    <Tag
                                        :value="item.status"
                                        :severity="getStatusSeverity(item.status)"
                                        rounded
                                        class="text-[10px] font-semibold px-1.5"
                                    />
                                    <small class="text-[10px] text-surface-400 dark:text-surface-500 font-medium uppercase">{{ formatDate(item.timestamp) }}</small>
                                </div>
                            </div>
                            <div
                                v-if="recentActivity.length === 0 && !is_loading"
                                class="flex flex-col items-center justify-center py-8 text-surface-400 dark:text-surface-500"
                            >
                                <i class="pi pi-history text-3xl mb-2"></i>
                                <span class="text-sm font-medium">No recent activity</span>
                            </div>
                        </div>
                    </template>
                </Card>
            </div>
        </div>
    </div>
</template>
