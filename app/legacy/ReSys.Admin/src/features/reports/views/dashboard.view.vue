<script setup lang="ts">
import { onMounted, computed } from 'vue';
import { useReportStore } from '../stores/report.store';
import { storeToRefs } from 'pinia';
import { useFormatter } from '@/shared/composables/formatter.use';

const store = useReportStore();
const { sales, inventory, catalog, activities, is_loading } = storeToRefs(store);
const { formatCurrency, formatDate } = useFormatter();

onMounted(() => {
    store.fetchDashboardData();
});

const chartData = computed(() => {
    if (!sales.value?.trend_history) return null;
    
    return {
        labels: sales.value.trend_history.map(h => new Date(h.date).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })),
        datasets: [
            {
                label: 'Revenue',
                data: sales.value.trend_history.map(h => h.revenue),
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

const getActivityIcon = (type: string) => {
    return type === 'Order' ? 'pi pi-shopping-bag' : 'pi pi-box';
};

const getActivityColor = (type: string) => {
    return type === 'Order' ? 'text-blue-500' : 'text-orange-500';
};
</script>

<template>
    <div class="flex flex-col gap-8">
        <div class="flex justify-between items-center mb-4">
            <div>
                <h1 class="text-4xl font-black uppercase tracking-tighter text-surface-900 dark:text-surface-0">Command Center</h1>
                <p class="text-surface-500">Real-time performance overview.</p>
            </div>
            <Button icon="pi pi-refresh" severity="secondary" text rounded @click="store.fetchDashboardData()" :loading="is_loading" />
        </div>

        <!-- Revenue Chart -->
        <div class="grid grid-cols-1 gap-6" v-if="chartData">
            <Card class="rounded-[2.5rem] shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
                <template #title>
                    <div class="flex justify-between items-center p-4">
                        <span class="text-lg font-black uppercase tracking-widest text-surface-400">Revenue Trend (30 Days)</span>
                        <div class="flex items-center gap-2">
                            <i class="pi pi-chart-line text-primary"></i>
                            <span class="text-sm font-bold text-primary">Live Data</span>
                        </div>
                    </div>
                </template>
                <template #content>
                    <div class="h-64 px-4 pb-4">
                        <Chart type="line" :data="chartData" :options="chartOptions" class="h-full" />
                    </div>
                </template>
            </Card>
        </div>

        <!-- 4 Main Widgets -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <div class="p-8 bg-surface-0 dark:bg-surface-900 rounded-[2rem] shadow-sm border border-surface-100 dark:border-surface-800 flex flex-col gap-4">
                <div class="flex justify-between items-start">
                    <div class="w-12 h-12 bg-blue-500/10 rounded-2xl flex items-center justify-center text-blue-500">
                        <i class="pi pi-dollar text-xl"></i>
                    </div>
                    <Badge v-if="sales?.revenue_trend_percentage" :value="`${sales.revenue_trend_percentage}%`" :severity="sales.revenue_trend_percentage >= 0 ? 'success' : 'danger'" class="font-bold" />
                </div>
                <div>
                    <p class="text-xs font-black uppercase tracking-widest text-surface-400 mb-1">Total Revenue</p>
                    <h3 class="text-3xl font-black text-surface-900 dark:text-surface-0">
                        {{ formatCurrency(sales?.total_revenue || 0) }}
                    </h3>
                </div>
            </div>

            <div class="p-8 bg-surface-0 dark:bg-surface-900 rounded-[2rem] shadow-sm border border-surface-100 dark:border-surface-800 flex flex-col gap-4">
                <div class="w-12 h-12 bg-purple-500/10 rounded-2xl flex items-center justify-center text-purple-500">
                    <i class="pi pi-shopping-bag text-xl"></i>
                </div>
                <div>
                    <p class="text-xs font-black uppercase tracking-widest text-surface-400 mb-1">Total Orders</p>
                    <h3 class="text-3xl font-black text-surface-900 dark:text-surface-0">{{ sales?.order_count || 0 }}</h3>
                </div>
            </div>

            <div class="p-8 bg-surface-0 dark:bg-surface-900 rounded-[2rem] shadow-sm border border-surface-100 dark:border-surface-800 flex flex-col gap-4">
                <div class="w-12 h-12 bg-green-500/10 rounded-2xl flex items-center justify-center text-green-500">
                    <i class="pi pi-objects-column text-xl"></i>
                </div>
                <div>
                    <p class="text-xs font-black uppercase tracking-widest text-surface-400 mb-1">Active Products</p>
                    <h3 class="text-3xl font-black text-surface-900 dark:text-surface-0">{{ catalog?.active_products || 0 }}</h3>
                </div>
            </div>

            <div class="p-8 bg-surface-0 dark:bg-surface-900 rounded-[2rem] shadow-sm border border-surface-100 dark:border-surface-800 flex flex-col gap-4">
                <div class="w-12 h-12 bg-orange-500/10 rounded-2xl flex items-center justify-center text-orange-500">
                    <i class="pi pi-exclamation-circle text-xl"></i>
                </div>
                <div>
                    <p class="text-xs font-black uppercase tracking-widest text-surface-400 mb-1">Low Stock Alerts</p>
                    <h3 class="text-3xl font-black text-surface-900 dark:text-surface-0 text-orange-500">{{ inventory?.low_stock_count || 0 }}</h3>
                </div>
            </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
            <!-- Recent Activity -->
            <div class="lg:col-span-2 p-8 bg-surface-0 dark:bg-surface-900 rounded-[2.5rem] shadow-sm border border-surface-100 dark:border-surface-800">
                <h3 class="font-black text-xl uppercase tracking-tight mb-8">Recent Activity</h3>
                <div class="flex flex-col gap-4">
                    <div v-for="item in activities" :key="item.id" class="flex items-center gap-4 p-4 bg-surface-50 dark:bg-surface-800/50 rounded-2xl border border-surface-100 dark:border-surface-700">
                        <div :class="['w-10 h-10 rounded-xl flex items-center justify-center bg-surface-0 dark:bg-surface-900 shadow-sm', getActivityColor(item.type)]">
                            <i :class="getActivityIcon(item.type)"></i>
                        </div>
                        <div class="flex-1">
                            <p class="font-bold text-sm">{{ item.title }}</p>
                            <p class="text-xs text-surface-500">{{ item.description }}</p>
                        </div>
                        <div class="text-right flex flex-col items-end gap-1">
                            <Tag :value="item.status" :severity="getStatusSeverity(item.status)" rounded class="text-[10px] font-black px-2" />
                            <small class="text-[10px] text-surface-400 uppercase font-bold">{{ formatDate(item.timestamp) }}</small>
                        </div>
                    </div>
                    <div v-if="activities.length === 0 && !is_loading" class="text-center py-10 text-surface-500">
                        No recent activity found.
                    </div>
                </div>
            </div>

            <!-- Stats Column -->
            <div class="lg:col-span-1 flex flex-col gap-8">
                <!-- Catalog Summary -->
                <div class="bg-surface-900 dark:bg-surface-950 text-surface-0 p-10 rounded-[3rem] shadow-xl flex flex-col">
                    <h3 class="text-lg font-black uppercase tracking-widest mb-8 text-primary">Catalog Overview</h3>
                    <div class="flex flex-col gap-6">
                        <div class="flex justify-between items-center">
                            <span class="text-surface-400 font-bold text-sm">Total Products</span>
                            <span class="text-xl font-black">{{ catalog?.total_products || 0 }}</span>
                        </div>
                        <div class="flex justify-between items-center">
                            <span class="text-surface-400 font-bold text-sm">Total Variants</span>
                            <span class="text-xl font-black">{{ catalog?.total_variants || 0 }}</span>
                        </div>
                        <div class="flex justify-between items-center">
                            <span class="text-surface-400 font-bold text-sm">Taxonomies</span>
                            <span class="text-xl font-black">{{ catalog?.total_taxonomies || 0 }}</span>
                        </div>
                        <Divider class="border-surface-800" />
                        <h4 class="text-xs font-black uppercase tracking-widest text-surface-500 mb-2">Recently Added</h4>
                        <div v-for="product in catalog?.recently_added" :key="product.id" class="flex flex-col gap-1">
                            <span class="font-bold text-sm">{{ product.name }}</span>
                            <small class="text-[10px] text-surface-500 uppercase">{{ formatDate(product.created_at) }}</small>
                        </div>
                    </div>
                </div>

                <!-- Inventory Health -->
                <div class="p-8 bg-surface-0 dark:bg-surface-900 rounded-[2.5rem] shadow-sm border border-surface-100 dark:border-surface-800">
                    <h3 class="font-black text-xl uppercase tracking-tight mb-6">Inventory Health</h3>
                    <div class="flex flex-col gap-4">
                        <div class="flex flex-col gap-2">
                            <div class="flex justify-between text-xs font-black uppercase tracking-widest text-surface-400">
                                <span>Stock Accuracy</span>
                                <span>{{ inventory?.stock_accuracy_percentage || 100 }}%</span>
                            </div>
                            <ProgressBar :value="inventory?.stock_accuracy_percentage || 100" :showValue="false" style="height: 6px" />
                        </div>
                        <div class="grid grid-cols-2 gap-4 mt-2">
                            <div class="p-4 bg-red-500/5 rounded-2xl border border-red-500/10">
                                <p class="text-[10px] font-black uppercase text-red-500/60 mb-1">Out of Stock</p>
                                <p class="text-2xl font-black text-red-500">{{ inventory?.out_of_stock_count || 0 }}</p>
                            </div>
                            <div class="p-4 bg-orange-500/5 rounded-2xl border border-orange-500/10">
                                <p class="text-[10px] font-black uppercase text-orange-500/60 mb-1">Low Stock</p>
                                <p class="text-2xl font-black text-orange-500">{{ inventory?.low_stock_count || 0 }}</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>