import {
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import {
  TrendingUp, ShoppingBag, AlertTriangle, Users,
  ArrowUpRight, ArrowDownRight,
} from 'lucide-react';
import { formatCurrency } from '../lib/utils';

// ── Mock data ──────────────────────────────────────────────────────────────────
const salesData = [
  { day: 'Mon', revenue: 124500 },
  { day: 'Tue', revenue: 98700 },
  { day: 'Wed', revenue: 156800 },
  { day: 'Thu', revenue: 142300 },
  { day: 'Fri', revenue: 189600 },
  { day: 'Sat', revenue: 234100 },
  { day: 'Sun', revenue: 201400 },
];

const topProducts = [
  { rank: 1, name: 'Fresh Whole Milk (1L)', category: 'Dairy',      sold: 482, revenue: 48200 },
  { rank: 2, name: 'Basmati Rice (5kg)',    category: 'Grains',     sold: 319, revenue: 95700 },
  { rank: 3, name: 'Sunflower Oil (1L)',    category: 'Cooking Oil', sold: 278, revenue: 41700 },
  { rank: 4, name: 'Bread (White Loaf)',    category: 'Bakery',     sold: 256, revenue: 25600 },
  { rank: 5, name: 'Eggs (12 pcs)',         category: 'Poultry',    sold: 241, revenue: 48200 },
];

const kpis = [
  {
    label: "Today's Revenue",
    value: formatCurrency(201400),
    change: 6.8,
    icon: TrendingUp,
    color: 'text-primary-700 bg-primary-50 dark:bg-primary-900/20 dark:text-primary-300',
  },
  {
    label: 'Transactions',
    value: '1,248',
    change: 3.2,
    icon: ShoppingBag,
    color: 'text-emerald-700 bg-emerald-50 dark:bg-emerald-900/20 dark:text-emerald-300',
  },
  {
    label: 'Low Stock Alerts',
    value: '14',
    change: -2,
    icon: AlertTriangle,
    color: 'text-amber-700 bg-amber-50 dark:bg-amber-900/20 dark:text-amber-300',
    negative: true,
  },
  {
    label: 'Active Customers',
    value: '3,891',
    change: 1.4,
    icon: Users,
    color: 'text-violet-700 bg-violet-50 dark:bg-violet-900/20 dark:text-violet-300',
  },
];

export default function DashboardPage() {
  return (
    <div className="space-y-6">
      {/* Page title */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Dashboard</h1>
        <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
          Thursday, 8 May 2026 · Agora Supermarkets
        </p>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
        {kpis.map((kpi) => (
          <div
            key={kpi.label}
            className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 p-5 flex items-start gap-4 shadow-sm"
          >
            <div className={`p-2.5 rounded-lg ${kpi.color}`}>
              <kpi.icon className="w-5 h-5" />
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-xs text-gray-500 dark:text-gray-400 truncate">{kpi.label}</p>
              <p className="text-2xl font-bold text-gray-900 dark:text-white mt-0.5">{kpi.value}</p>
              <div className={`flex items-center gap-1 mt-1 text-xs font-medium ${
                kpi.negative
                  ? kpi.change < 0 ? 'text-emerald-600' : 'text-red-500'
                  : kpi.change >= 0 ? 'text-emerald-600' : 'text-red-500'
              }`}>
                {kpi.change >= 0
                  ? <ArrowUpRight className="w-3.5 h-3.5" />
                  : <ArrowDownRight className="w-3.5 h-3.5" />
                }
                {Math.abs(kpi.change)}% vs yesterday
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Chart + Top Products */}
      <div className="grid grid-cols-1 xl:grid-cols-3 gap-4">
        {/* Sales Chart */}
        <div className="xl:col-span-2 bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 p-5 shadow-sm">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h2 className="text-base font-semibold text-gray-900 dark:text-white">Revenue — Last 7 Days</h2>
              <p className="text-xs text-gray-500 dark:text-gray-400">All stores combined</p>
            </div>
            <span className="text-xs font-medium px-2.5 py-1 bg-primary-50 dark:bg-primary-900/20 text-primary-700 dark:text-primary-300 rounded-full">
              BDT
            </span>
          </div>
          <ResponsiveContainer width="100%" height={220}>
            <AreaChart data={salesData} margin={{ top: 4, right: 4, left: 0, bottom: 0 }}>
              <defs>
                <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%"  stopColor="#1565C0" stopOpacity={0.15} />
                  <stop offset="95%" stopColor="#1565C0" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
              <XAxis dataKey="day" tick={{ fontSize: 12, fill: '#9CA3AF' }} axisLine={false} tickLine={false} />
              <YAxis
                tick={{ fontSize: 11, fill: '#9CA3AF' }}
                axisLine={false}
                tickLine={false}
                tickFormatter={(v) => `৳${(v / 1000).toFixed(0)}k`}
              />
              <Tooltip
                formatter={(v: number) => [`৳${v.toLocaleString()}`, 'Revenue']}
                contentStyle={{ borderRadius: 8, border: '1px solid #E5E7EB', fontSize: 12 }}
              />
              <Area
                type="monotone"
                dataKey="revenue"
                stroke="#1565C0"
                strokeWidth={2}
                fill="url(#colorRevenue)"
                dot={{ r: 3, fill: '#1565C0' }}
                activeDot={{ r: 5 }}
              />
            </AreaChart>
          </ResponsiveContainer>
        </div>

        {/* Top Products */}
        <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 p-5 shadow-sm">
          <h2 className="text-base font-semibold text-gray-900 dark:text-white mb-1">Top Products</h2>
          <p className="text-xs text-gray-500 dark:text-gray-400 mb-4">By units sold today</p>
          <div className="space-y-3">
            {topProducts.map((p) => (
              <div key={p.rank} className="flex items-center gap-3">
                <span className="w-5 h-5 flex-shrink-0 rounded-full bg-primary-100 dark:bg-primary-900/30 text-primary-700 dark:text-primary-300 text-xs font-bold flex items-center justify-center">
                  {p.rank}
                </span>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-800 dark:text-gray-200 truncate">{p.name}</p>
                  <p className="text-xs text-gray-400">{p.category}</p>
                </div>
                <div className="text-right flex-shrink-0">
                  <p className="text-sm font-semibold text-gray-900 dark:text-white">{p.sold}</p>
                  <p className="text-xs text-gray-400">units</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
