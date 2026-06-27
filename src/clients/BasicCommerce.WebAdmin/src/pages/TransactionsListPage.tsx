import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import {
  ShoppingCart, Search, X, RefreshCw, Eye, Loader2,
  AlertCircle, ArrowUpCircle, ArrowDownCircle, TrendingUp,
  Receipt, Ban, RotateCcw, Clock, CheckCircle2, Plus,
} from 'lucide-react';
import { cn } from '../lib/utils';
import type { TransactionStatus, TransactionType, TransactionSummary } from '../types/transaction';
import { useTransactions } from '../hooks/useTransactions';
import { useStoreList } from '../hooks/useStoreAdmin';
import { Pagination } from '../components/ui/Pagination';

const PAGE_SIZE = 20;

const STATUS_CONFIG: Record<TransactionStatus, { label: string; dot: string; badge: string; icon: React.ReactNode }> = {
  Open:      { label: 'Open',      dot: 'bg-sky-500',     badge: 'bg-sky-50 text-sky-700 dark:bg-sky-900/30 dark:text-sky-400',      icon: <Clock className="w-3 h-3" /> },
  Completed: { label: 'Completed', dot: 'bg-emerald-500', badge: 'bg-emerald-50 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400', icon: <CheckCircle2 className="w-3 h-3" /> },
  Voided:    { label: 'Voided',    dot: 'bg-red-500',     badge: 'bg-red-50 text-red-700 dark:bg-red-900/30 dark:text-red-400',      icon: <Ban className="w-3 h-3" /> },
  Suspended: { label: 'Suspended', dot: 'bg-amber-500',   badge: 'bg-amber-50 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400', icon: <Clock className="w-3 h-3" /> },
  Refunded:  { label: 'Refunded',  dot: 'bg-violet-500',  badge: 'bg-violet-50 text-violet-700 dark:bg-violet-900/30 dark:text-violet-400', icon: <RotateCcw className="w-3 h-3" /> },
};

const TYPE_CONFIG: Record<TransactionType, { label: string; badge: string }> = {
  Sale:       { label: 'Sale',       badge: 'bg-blue-50 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400' },
  Return:     { label: 'Return',     badge: 'bg-orange-50 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400' },
  Exchange:   { label: 'Exchange',   badge: 'bg-teal-50 text-teal-700 dark:bg-teal-900/30 dark:text-teal-400' },
  CreditSale: { label: 'Credit',     badge: 'bg-purple-50 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400' },
};

function fmtMoney(n: number) {
  return `৳${n.toLocaleString('en-BD', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

function fmtDate(d: string) {
  return new Date(d).toLocaleString('en-GB', {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
}

function StatusBadge({ status }: { status: TransactionStatus }) {
  const cfg = STATUS_CONFIG[status] ?? STATUS_CONFIG.Open;
  return (
    <span className={cn('inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full text-xs font-medium', cfg.badge)}>
      <span className={cn('w-1.5 h-1.5 rounded-full flex-shrink-0', cfg.dot)} />
      {cfg.label}
    </span>
  );
}

function TypeBadge({ type }: { type: TransactionType }) {
  const cfg = TYPE_CONFIG[type] ?? TYPE_CONFIG.Sale;
  return (
    <span className={cn('inline-flex items-center px-2 py-0.5 rounded text-xs font-medium', cfg.badge)}>
      {cfg.label}
    </span>
  );
}

function StatCard({ label, value, sub, icon, color }: {
  label: string; value: string | number; sub?: string;
  icon: React.ReactNode; color: string;
}) {
  return (
    <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-4 shadow-sm">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-xs text-gray-500 dark:text-gray-400 font-medium">{label}</p>
          <p className={cn('text-2xl font-bold mt-1 tabular-nums', color)}>{value}</p>
          {sub && <p className="text-xs text-gray-400 mt-0.5">{sub}</p>}
        </div>
        <div className={cn('p-2 rounded-xl', color.replace('text-', 'bg-').replace('-700', '-50').replace('-400', '-900/20'))}>
          {icon}
        </div>
      </div>
    </div>
  );
}

const STATUS_TABS: { value: string; label: string }[] = [
  { value: '', label: 'All' },
  { value: 'Completed', label: 'Completed' },
  { value: 'Open', label: 'Open' },
  { value: 'Voided', label: 'Voided' },
  { value: 'Suspended', label: 'Suspended' },
  { value: 'Refunded', label: 'Refunded' },
];

const TYPE_OPTIONS: { value: string; label: string }[] = [
  { value: '', label: 'All types' },
  { value: 'Sale', label: 'Sale' },
  { value: 'Return', label: 'Return' },
  { value: 'Exchange', label: 'Exchange' },
  { value: 'CreditSale', label: 'Credit Sale' },
];

export default function TransactionsListPage() {
  const navigate = useNavigate();
  const [search, setSearch]     = useState('');
  const [term, setTerm]         = useState('');
  const [storeId, setStoreId]   = useState('');
  const [status, setStatus]     = useState('');
  const [txType, setTxType]     = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo]     = useState('');
  const [page, setPage]         = useState(1);

  const { data: stores = [] } = useStoreList();

  const { data, isLoading, isError, isFetching, refetch } = useTransactions({
    term: term || undefined,
    storeId: storeId || undefined,
    status: status || undefined,
    type: txType || undefined,
    from: dateFrom ? new Date(dateFrom).toISOString() : undefined,
    to: dateTo ? new Date(dateTo + 'T23:59:59').toISOString() : undefined,
    page,
    pageSize: PAGE_SIZE,
  });

  const items      = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;
  const totalPages = Math.ceil(totalCount / PAGE_SIZE);

  const revenue    = items.filter(t => t.status === 'Completed').reduce((s, t) => s + t.total, 0);
  const completed  = items.filter(t => t.status === 'Completed').length;
  const voided     = items.filter(t => t.status === 'Voided').length;

  function handleSearch(e: React.FormEvent) {
    e.preventDefault();
    setTerm(search);
    setPage(1);
  }

  function clearSearch() {
    setSearch('');
    setTerm('');
    setPage(1);
  }

  function handleStatusTab(val: string) {
    setStatus(val);
    setPage(1);
  }

  const selectCls = 'text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg px-3 py-2 text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 cursor-pointer transition';

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <ShoppingCart className="w-6 h-6 text-primary-700" />
            Transactions
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            All sales, returns, and exchanges processed across your stores
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={() => refetch()} disabled={isFetching}
            className="p-2 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors disabled:opacity-40"
            title="Refresh"
          >
            <RefreshCw className={cn('w-4 h-4', isFetching && 'animate-spin')} />
          </button>
          <Link
            to="/sales/transactions/new"
            className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-primary-700 hover:bg-primary-800 rounded-xl transition shadow-sm"
          >
            <Plus className="w-4 h-4" />
            New Transaction
          </Link>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
        <StatCard
          label="Total (this page)"
          value={items.length}
          sub={`of ${totalCount} total`}
          icon={<Receipt className="w-5 h-5 text-primary-600" />}
          color="text-primary-700 dark:text-primary-400"
        />
        <StatCard
          label="Revenue (this page)"
          value={`৳${revenue.toLocaleString('en-BD', { maximumFractionDigits: 0 })}`}
          sub="Completed only"
          icon={<TrendingUp className="w-5 h-5 text-emerald-600" />}
          color="text-emerald-700 dark:text-emerald-400"
        />
        <StatCard
          label="Completed"
          value={completed}
          icon={<ArrowUpCircle className="w-5 h-5 text-sky-600" />}
          color="text-sky-700 dark:text-sky-400"
        />
        <StatCard
          label="Voided"
          value={voided}
          icon={<ArrowDownCircle className="w-5 h-5 text-red-500" />}
          color="text-red-600 dark:text-red-400"
        />
      </div>

      {/* Status tabs */}
      <div className="flex gap-1 flex-wrap">
        {STATUS_TABS.map((tab) => (
          <button
            key={tab.value}
            onClick={() => handleStatusTab(tab.value)}
            className={cn(
              'px-3 py-1.5 text-sm font-medium rounded-lg transition-colors',
              status === tab.value
                ? 'bg-primary-700 text-white shadow-sm'
                : 'text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800',
            )}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        {/* Search */}
        <form onSubmit={handleSearch} className="flex items-center gap-2">
          <div className="relative">
            <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search TXN number…"
              className="pl-8 pr-8 py-2 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 w-52"
            />
            {search && (
              <button type="button" onClick={clearSearch}
                className="absolute right-2 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600">
                <X className="w-3.5 h-3.5" />
              </button>
            )}
          </div>
        </form>

        <select value={storeId} onChange={(e) => { setStoreId(e.target.value); setPage(1); }} className={selectCls}>
          <option value="">All stores</option>
          {stores.filter(s => s.status === 'Active').map(s => (
            <option key={s.id} value={s.id}>{s.name}</option>
          ))}
        </select>

        <select value={txType} onChange={(e) => { setTxType(e.target.value); setPage(1); }} className={selectCls}>
          {TYPE_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
        </select>

        <input
          type="date" value={dateFrom}
          onChange={(e) => { setDateFrom(e.target.value); setPage(1); }}
          className={selectCls}
          title="From date"
        />
        <input
          type="date" value={dateTo}
          onChange={(e) => { setDateTo(e.target.value); setPage(1); }}
          className={selectCls}
          title="To date"
        />

        {(isFetching && !isLoading) && <Loader2 className="w-4 h-4 animate-spin text-gray-400" />}

        <span className="text-xs text-gray-400 ml-auto">
          {totalCount.toLocaleString()} transaction{totalCount === 1 ? '' : 's'}
        </span>
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
        {isError ? (
          <div className="flex items-center gap-2 px-6 py-12 text-red-500">
            <AlertCircle className="w-5 h-5 flex-shrink-0" />
            <span className="text-sm">Failed to load transactions. Please refresh.</span>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                  {['Transaction #', 'Type', 'Customer', 'Store', 'Items', 'Total', 'Status', 'Date', ''].map((h) => (
                    <th key={h} className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider whitespace-nowrap">
                      {h}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                {isLoading
                  ? Array.from({ length: 8 }).map((_, i) => (
                      <tr key={i} className="animate-pulse">
                        {Array.from({ length: 9 }).map((_, j) => (
                          <td key={j} className="px-4 py-3">
                            <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-full" />
                          </td>
                        ))}
                      </tr>
                    ))
                  : items.length === 0
                    ? (
                      <tr>
                        <td colSpan={9} className="px-4 py-16 text-center">
                          <ShoppingCart className="w-8 h-8 text-gray-300 dark:text-gray-600 mx-auto mb-2" />
                          <p className="text-sm text-gray-400">No transactions found.</p>
                          <p className="text-xs text-gray-400 mt-0.5">Adjust the filters or check back later.</p>
                        </td>
                      </tr>
                    )
                    : items.map((t: TransactionSummary) => (
                        <tr
                          key={t.id}
                          onClick={() => navigate(`/sales/transactions/${t.id}`)}
                          className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors cursor-pointer group"
                        >
                          {/* TXN # */}
                          <td className="px-4 py-3">
                            <span className="font-mono text-xs font-semibold text-gray-900 dark:text-white group-hover:text-primary-700 dark:group-hover:text-primary-400 transition-colors">
                              {t.transactionNumber}
                            </span>
                          </td>

                          {/* Type */}
                          <td className="px-4 py-3">
                            <TypeBadge type={t.type} />
                          </td>

                          {/* Customer */}
                          <td className="px-4 py-3 text-gray-600 dark:text-gray-300">
                            {t.customerName
                              ? <span className="font-medium">{t.customerName}</span>
                              : <span className="text-gray-400 text-xs italic">Walk-in</span>
                            }
                          </td>

                          {/* Store */}
                          <td className="px-4 py-3 text-gray-600 dark:text-gray-300 whitespace-nowrap">
                            {t.storeName ?? <span className="text-gray-400 font-mono text-xs">{t.storeId.slice(0, 8)}…</span>}
                          </td>

                          {/* Items */}
                          <td className="px-4 py-3 text-center text-gray-600 dark:text-gray-400 tabular-nums">
                            {t.itemCount}
                          </td>

                          {/* Total */}
                          <td className="px-4 py-3 font-semibold tabular-nums text-gray-900 dark:text-white whitespace-nowrap">
                            {fmtMoney(t.total)}
                          </td>

                          {/* Status */}
                          <td className="px-4 py-3">
                            <StatusBadge status={t.status} />
                          </td>

                          {/* Date */}
                          <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">
                            {fmtDate(t.createdAt)}
                          </td>

                          {/* Action */}
                          <td className="px-4 py-3">
                            <button
                              onClick={(e) => { e.stopPropagation(); navigate(`/sales/transactions/${t.id}`); }}
                              title="View detail"
                              className="p-1.5 rounded-lg text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors"
                            >
                              <Eye className="w-3.5 h-3.5" />
                            </button>
                          </td>
                        </tr>
                      ))
                }
              </tbody>
            </table>
          </div>
        )}

        {totalPages > 1 && (
          <div className="px-4 py-3 border-t border-gray-100 dark:border-gray-800">
            <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
          </div>
        )}
      </div>
    </div>
  );
}
