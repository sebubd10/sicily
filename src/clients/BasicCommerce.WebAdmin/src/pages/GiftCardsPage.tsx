import { useState } from 'react';
import {
  Gift, Search, X, ChevronLeft, ChevronRight, AlertCircle, Loader2,
  Eye, RefreshCw, XCircle, Plus, Filter, Copy, Check,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { GiftCardSummary, GiftCardStatus } from '../types/giftCard';
import { useGiftCards } from '../hooks/useGiftCards';
import { useStoreList } from '../hooks/useStoreAdmin';
import { IssueGiftCardModal } from '../components/giftCards/IssueGiftCardModal';
import { GiftCardDetailModal } from '../components/giftCards/GiftCardDetailModal';

const PAGE_SIZE = 20;

type StatusFilter = 'All' | GiftCardStatus;

const STATUS_FILTERS: StatusFilter[] = ['All', 'Active', 'Depleted', 'Expired', 'Cancelled', 'Inactive'];

const STATUS_STYLES: Record<string, string> = {
  Active:    'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400',
  Depleted:  'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400',
  Expired:   'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400',
  Cancelled: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400',
  Inactive:  'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400',
};

const STATUS_DOTS: Record<string, string> = {
  Active:    'bg-emerald-500',
  Depleted:  'bg-blue-500',
  Expired:   'bg-amber-500',
  Cancelled: 'bg-red-500',
  Inactive:  'bg-gray-400',
};

function fmtMoney(n: number) {
  return `৳${n.toLocaleString('en-BD', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`;
}

function fmtDate(d: string | null | undefined) {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

function isExpiringSoon(d: string | null | undefined) {
  if (!d) return false;
  const diff = new Date(d).getTime() - Date.now();
  return diff > 0 && diff < 7 * 86400000;
}

function CodeCell({ code }: { code: string }) {
  const [copied, setCopied] = useState(false);
  function copy(e: React.MouseEvent) {
    e.stopPropagation();
    navigator.clipboard.writeText(code);
    setCopied(true);
    setTimeout(() => setCopied(false), 1800);
  }
  return (
    <div className="flex items-center gap-2">
      <span className="font-mono text-xs text-gray-700 dark:text-gray-300">{code}</span>
      <button onClick={copy} className="text-gray-400 hover:text-violet-600 transition-colors p-0.5">
        {copied ? <Check className="w-3.5 h-3.5 text-emerald-500" /> : <Copy className="w-3.5 h-3.5" />}
      </button>
    </div>
  );
}

function StatCard({
  label, value, sub, color,
}: {
  label: string; value: string | number; sub?: string; color: string;
}) {
  return (
    <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-4 shadow-sm">
      <p className="text-xs text-gray-500 dark:text-gray-400">{label}</p>
      <p className={cn('text-2xl font-bold mt-0.5 tabular-nums', color)}>{value}</p>
      {sub && <p className="text-xs text-gray-400 mt-0.5">{sub}</p>}
    </div>
  );
}

export default function GiftCardsPage() {
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('All');
  const [storeFilter, setStoreFilter] = useState('');
  const [page, setPage] = useState(1);
  const [issueOpen, setIssueOpen] = useState(false);
  const [detailId, setDetailId] = useState<string | null>(null);

  const { data, isLoading, isError, error, isFetching } = useGiftCards({
    status: statusFilter === 'All' ? undefined : statusFilter,
    storeId: storeFilter || undefined,
    page,
    pageSize: PAGE_SIZE,
  });

  const { data: stores = [] } = useStoreList();

  const items = data?.items ?? [];
  const total = data?.totalCount ?? 0;
  const totalPages = Math.ceil(total / PAGE_SIZE) || 1;
  const apiError = isError ? extractApiError(error) : null;

  // Client-side search by code (since API doesn't support code search directly)
  const filtered = search
    ? items.filter((c) =>
        c.code.toLowerCase().includes(search.toLowerCase()),
      )
    : items;

  // Stats from current page items
  const activeItems   = items.filter((c) => c.cardStatus === 'Active');
  const depletedItems = items.filter((c) => c.cardStatus === 'Depleted');
  const activeBalance = activeItems.reduce((s, c) => s + c.balance, 0);

  const storeMap = new Map(stores.map((s) => [s.id, s.name]));

  function handleSearch(v: string) { setSearch(v); }

  function changeFilter(s: StatusFilter) {
    setStatusFilter(s);
    setPage(1);
    setSearch('');
  }

  function changeStore(s: string) {
    setStoreFilter(s);
    setPage(1);
    setSearch('');
  }

  return (
    <div className="flex flex-col gap-6 p-6 min-h-0">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100 flex items-center gap-2">
            <Gift className="w-6 h-6 text-violet-600 dark:text-violet-400" />
            Gift Cards
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Issue, reload, and manage prepaid gift cards
          </p>
        </div>
        <button
          onClick={() => setIssueOpen(true)}
          className="flex items-center gap-2 px-4 py-2 rounded-xl bg-violet-600 hover:bg-violet-700 text-white text-sm font-medium shadow-sm transition-colors"
        >
          <Plus className="w-4 h-4" />
          Issue Gift Card
        </button>
      </div>

      {/* Stat cards */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label="Total Cards"
          value={isLoading ? '…' : total.toLocaleString()}
          color="text-gray-900 dark:text-gray-100"
        />
        <StatCard
          label="Active Cards"
          value={isLoading ? '…' : activeItems.length}
          sub={statusFilter === 'All' ? 'on this page' : undefined}
          color="text-emerald-600 dark:text-emerald-400"
        />
        <StatCard
          label="Depleted"
          value={isLoading ? '…' : depletedItems.length}
          sub={statusFilter === 'All' ? 'on this page' : undefined}
          color="text-blue-600 dark:text-blue-400"
        />
        <StatCard
          label="Active Balance"
          value={isLoading ? '…' : fmtMoney(activeBalance)}
          sub="on this page"
          color="text-violet-600 dark:text-violet-400"
        />
      </div>

      {/* Toolbar */}
      <div className="flex flex-col sm:flex-row gap-3 items-start sm:items-center">
        {/* Search */}
        <div className="relative max-w-xs w-full">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            className="w-full pl-9 pr-9 py-2 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-violet-500"
            placeholder="Filter by card code…"
            value={search}
            onChange={(e) => handleSearch(e.target.value)}
          />
          {search && (
            <button onClick={() => setSearch('')}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600">
              <X className="w-3.5 h-3.5" />
            </button>
          )}
        </div>

        {/* Store filter */}
        <div className="flex items-center gap-1.5">
          <Filter className="w-4 h-4 text-gray-400 flex-shrink-0" />
          <select
            value={storeFilter}
            onChange={(e) => changeStore(e.target.value)}
            className="px-3 py-2 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-violet-500"
          >
            <option value="">All Stores</option>
            {stores.map((s) => (
              <option key={s.id} value={s.id}>{s.name}</option>
            ))}
          </select>
        </div>

        {isFetching && !isLoading && <Loader2 className="w-4 h-4 animate-spin text-gray-400" />}
      </div>

      {/* Status tabs */}
      <div className="flex gap-1 bg-gray-100 dark:bg-gray-800 rounded-xl p-1 flex-wrap">
        {STATUS_FILTERS.map((s) => (
          <button
            key={s}
            onClick={() => changeFilter(s)}
            className={cn(
              'px-3 py-1.5 rounded-lg text-sm font-medium transition-all whitespace-nowrap',
              statusFilter === s
                ? 'bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 shadow-sm'
                : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200',
            )}
          >
            {s}
          </button>
        ))}
      </div>

      {/* Content */}
      {isLoading ? (
        <div className="flex items-center justify-center py-24">
          <Loader2 className="w-8 h-8 text-violet-500 animate-spin" />
        </div>
      ) : apiError ? (
        <div className="flex items-center gap-3 p-4 rounded-xl bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800">
          <AlertCircle className="w-5 h-5 text-red-500 flex-shrink-0" />
          <p className="text-sm text-red-700 dark:text-red-300">{apiError}</p>
        </div>
      ) : filtered.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-24 text-center">
          <div className="p-4 rounded-2xl bg-violet-50 dark:bg-violet-900/20 mb-4">
            <Gift className="w-8 h-8 text-violet-400" />
          </div>
          <p className="text-gray-600 dark:text-gray-400 font-medium">No gift cards found</p>
          <p className="text-gray-400 dark:text-gray-500 text-sm mt-1">
            {search
              ? `No card matching "${search}"`
              : statusFilter !== 'All'
              ? `No ${statusFilter.toLowerCase()} gift cards`
              : 'Issue your first gift card to get started'}
          </p>
          {!search && statusFilter === 'All' && (
            <button
              onClick={() => setIssueOpen(true)}
              className="mt-4 flex items-center gap-2 px-4 py-2 rounded-xl bg-violet-600 hover:bg-violet-700 text-white text-sm font-medium"
            >
              <Plus className="w-4 h-4" />Issue Gift Card
            </button>
          )}
        </div>
      ) : (
        <>
          <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 shadow-sm overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50">
                    {['Code', 'Store', 'Balance', 'Status', 'Expiry', 'Issued', 'Actions'].map((h) => (
                      <th key={h}
                        className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider whitespace-nowrap">
                        {h}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50 dark:divide-gray-800">
                  {filtered.map((c: GiftCardSummary) => {
                    const expiringSoon = isExpiringSoon(c.expiryDate);
                    return (
                      <tr
                        key={c.id}
                        className="hover:bg-violet-50/40 dark:hover:bg-violet-900/10 transition-colors cursor-pointer"
                        onClick={() => setDetailId(c.id)}
                      >
                        {/* Code */}
                        <td className="px-4 py-3" onClick={(e) => e.stopPropagation()}>
                          <CodeCell code={c.code} />
                        </td>

                        {/* Store */}
                        <td className="px-4 py-3 text-sm text-gray-600 dark:text-gray-400 whitespace-nowrap">
                          {storeMap.get(c.storeId) ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                        </td>

                        {/* Balance */}
                        <td className="px-4 py-3">
                          <span className={cn(
                            'text-sm font-bold tabular-nums',
                            c.balance > 0 ? 'text-gray-900 dark:text-gray-100' : 'text-gray-400',
                          )}>
                            {fmtMoney(c.balance)}
                          </span>
                        </td>

                        {/* Status */}
                        <td className="px-4 py-3">
                          <span className={cn(
                            'inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium',
                            STATUS_STYLES[c.cardStatus] ?? STATUS_STYLES.Inactive,
                          )}>
                            <span className={cn('w-1.5 h-1.5 rounded-full', STATUS_DOTS[c.cardStatus] ?? 'bg-gray-400')} />
                            {c.cardStatus}
                          </span>
                        </td>

                        {/* Expiry */}
                        <td className="px-4 py-3 whitespace-nowrap">
                          {c.expiryDate ? (
                            <span className={cn(
                              'text-sm',
                              expiringSoon ? 'text-amber-600 dark:text-amber-400 font-medium' : 'text-gray-500 dark:text-gray-400',
                            )}>
                              {expiringSoon && '⚠ '}{fmtDate(c.expiryDate)}
                            </span>
                          ) : (
                            <span className="text-gray-300 dark:text-gray-600 text-sm">No expiry</span>
                          )}
                        </td>

                        {/* Issued */}
                        <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400 whitespace-nowrap">
                          {fmtDate(c.createdAt)}
                        </td>

                        {/* Actions */}
                        <td className="px-4 py-3" onClick={(e) => e.stopPropagation()}>
                          <div className="flex items-center gap-1">
                            <button
                              title="View details"
                              onClick={() => setDetailId(c.id)}
                              className="p-1.5 rounded-lg text-gray-400 hover:text-violet-600 hover:bg-violet-50 dark:hover:bg-violet-900/20 transition-colors"
                            >
                              <Eye className="w-4 h-4" />
                            </button>
                            {(c.cardStatus === 'Active' || c.cardStatus === 'Depleted') && (
                              <button
                                title="Reload"
                                onClick={() => setDetailId(c.id)}
                                className="p-1.5 rounded-lg text-gray-400 hover:text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/20 transition-colors"
                              >
                                <RefreshCw className="w-4 h-4" />
                              </button>
                            )}
                            {c.cardStatus !== 'Cancelled' && (
                              <button
                                title="Cancel card"
                                onClick={() => setDetailId(c.id)}
                                className="p-1.5 rounded-lg text-gray-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
                              >
                                <XCircle className="w-4 h-4" />
                              </button>
                            )}
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>

          {/* Pagination */}
          <div className="flex items-center justify-between text-sm text-gray-500 dark:text-gray-400">
            <span>
              {total} card{total !== 1 ? 's' : ''}
              {statusFilter !== 'All' && ` · ${statusFilter}`}
              {storeFilter && ` · ${storeMap.get(storeFilter) ?? 'store'}`}
              {search && ` · filtered`}
            </span>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="p-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 disabled:opacity-40 transition-colors"
              >
                <ChevronLeft className="w-4 h-4" />
              </button>
              <span className="text-xs">Page {page} of {totalPages}</span>
              <button
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
                className="p-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 disabled:opacity-40 transition-colors"
              >
                <ChevronRight className="w-4 h-4" />
              </button>
            </div>
          </div>
        </>
      )}

      {/* Modals */}
      <IssueGiftCardModal open={issueOpen} onClose={() => setIssueOpen(false)} />
      <GiftCardDetailModal
        giftCardId={detailId}
        open={!!detailId}
        onClose={() => setDetailId(null)}
      />
    </div>
  );
}
