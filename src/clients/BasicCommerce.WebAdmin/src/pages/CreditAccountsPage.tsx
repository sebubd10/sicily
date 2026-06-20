import { useState } from 'react';
import {
  CreditCard, Search, X, ChevronLeft, ChevronRight,
  AlertCircle, Loader2, Eye, ArrowDownCircle, Users,
  TrendingUp, Wallet, CheckCircle, Plus,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { CreditAccountSummary } from '../types/customer';
import { useAllCreditAccounts, useRecordCreditPayment } from '../hooks/useCustomers';
import { CreditAccountDetailModal } from '../components/customers/CreditAccountDetailModal';
import { AddCreditAccountModal } from '../components/customers/AddCreditAccountModal';
import * as Dialog from '@radix-ui/react-dialog';

const PAGE_SIZE = 20;

type BalanceFilter = 'all' | 'outstanding' | 'settled';

const BALANCE_FILTERS: { id: BalanceFilter; label: string }[] = [
  { id: 'all', label: 'All Accounts' },
  { id: 'outstanding', label: 'Outstanding' },
  { id: 'settled', label: 'Settled' },
];

function fmtMoney(n: number, compact = false) {
  if (compact && n >= 1_000_000)
    return `৳${(n / 1_000_000).toFixed(1)}M`;
  if (compact && n >= 1_000)
    return `৳${(n / 1_000).toFixed(1)}K`;
  return `৳${n.toLocaleString('en-BD', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

function fmtDate(d: string | null | undefined) {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-GB', {
    day: '2-digit', month: 'short', year: 'numeric',
  });
}

function UtilBar({ used, limit }: { used: number; limit: number }) {
  const pct = limit > 0 ? Math.min(100, (used / limit) * 100) : 0;
  const color =
    pct >= 90 ? 'bg-red-500' :
    pct >= 70 ? 'bg-amber-500' :
    'bg-emerald-500';
  return (
    <div className="flex items-center gap-2">
      <div className="flex-1 h-1.5 bg-gray-100 dark:bg-gray-700 rounded-full overflow-hidden">
        <div className={cn('h-full rounded-full', color)} style={{ width: `${pct}%` }} />
      </div>
      <span className="text-xs tabular-nums text-gray-400 w-9 text-right">
        {pct.toFixed(0)}%
      </span>
    </div>
  );
}

/* ── Quick payment dialog ───────────────────────────────────── */
function QuickPaymentDialog({
  account,
  onClose,
}: {
  account: CreditAccountSummary | null;
  onClose: () => void;
}) {
  const [amount, setAmount] = useState('');
  const [reference, setReference] = useState('');
  const [done, setDone] = useState(false);

  const { mutate: recordPayment, isPending, error } = useRecordCreditPayment();
  const apiError = error ? extractApiError(error) : null;

  function handleClose() {
    setAmount('');
    setReference('');
    setDone(false);
    onClose();
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!account) return;
    const amt = parseFloat(amount);
    if (isNaN(amt) || amt <= 0) return;
    recordPayment(
      { creditAccountId: account.id, amount: amt, reference: reference.trim() || null },
      {
        onSuccess: () => {
          setDone(true);
          setTimeout(handleClose, 1800);
        },
      },
    );
  }

  return (
    <Dialog.Root open={!!account} onOpenChange={(v) => !v && handleClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50 animate-in fade-in-0 duration-150" />
        <Dialog.Content className="fixed left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 z-50 w-full max-w-sm bg-white dark:bg-gray-900 rounded-2xl shadow-2xl">
          <div className="px-6 py-4 border-b border-gray-100 dark:border-gray-800 flex items-center justify-between">
            <div>
              <Dialog.Title className="text-sm font-semibold text-gray-900 dark:text-gray-100">
                Record Payment
              </Dialog.Title>
              {account && (
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                  {account.customerName} · {account.storeName}
                </p>
              )}
            </div>
            <Dialog.Close onClick={handleClose} className="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          <form onSubmit={handleSubmit} className="px-6 py-5 space-y-4">
            {done ? (
              <div className="flex flex-col items-center py-4 gap-2 text-emerald-600 dark:text-emerald-400">
                <CheckCircle className="w-8 h-8" />
                <p className="text-sm font-medium">Payment recorded!</p>
              </div>
            ) : (
              <>
                {account && (
                  <div className="flex items-center justify-between text-xs text-gray-500 dark:text-gray-400 bg-gray-50 dark:bg-gray-800 rounded-lg px-3 py-2.5">
                    <span>Outstanding</span>
                    <span className="font-bold text-red-600 dark:text-red-400">
                      {fmtMoney(account.outstandingBalance)}
                    </span>
                  </div>
                )}

                {apiError && (
                  <div className="flex items-center gap-2 text-xs text-red-600 dark:text-red-400">
                    <AlertCircle className="w-3.5 h-3.5" />{apiError}
                  </div>
                )}

                <div>
                  <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">
                    Payment Amount *
                  </label>
                  <div className="relative">
                    <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">৳</span>
                    <input
                      autoFocus
                      type="number"
                      min="0.01"
                      step="0.01"
                      required
                      className="w-full pl-8 pr-3 py-2.5 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
                      placeholder="0.00"
                      value={amount}
                      onChange={(e) => setAmount(e.target.value)}
                    />
                  </div>
                  {amount && parseFloat(amount) > 0 && account && (
                    <p className="text-xs text-gray-400 mt-1">
                      Remaining: {fmtMoney(Math.max(0, account.outstandingBalance - parseFloat(amount)))}
                    </p>
                  )}
                </div>

                <div>
                  <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">
                    Reference / Cheque No.
                  </label>
                  <input
                    className="w-full px-3 py-2.5 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
                    placeholder="Optional"
                    value={reference}
                    onChange={(e) => setReference(e.target.value)}
                  />
                </div>

                <div className="flex gap-2 pt-1">
                  <button
                    type="button"
                    onClick={handleClose}
                    className="flex-1 py-2.5 rounded-lg text-sm text-gray-600 dark:text-gray-400 border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={isPending || !amount}
                    className="flex-1 flex items-center justify-center gap-2 py-2.5 rounded-lg bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white text-sm font-medium transition-colors"
                  >
                    {isPending ? <Loader2 className="w-4 h-4 animate-spin" /> : <ArrowDownCircle className="w-4 h-4" />}
                    {isPending ? 'Saving…' : 'Record'}
                  </button>
                </div>
              </>
            )}
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}

/* ── Main page ──────────────────────────────────────────────── */
export default function CreditAccountsPage() {
  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [balanceFilter, setBalanceFilter] = useState<BalanceFilter>('all');
  const [page, setPage] = useState(1);
  const [detailId, setDetailId] = useState<string | null>(null);
  const [payTarget, setPayTarget] = useState<CreditAccountSummary | null>(null);
  const [addOpen, setAddOpen] = useState(false);

  // Debounce search
  const [searchTimer, setSearchTimer] = useState<ReturnType<typeof setTimeout> | null>(null);
  function handleSearch(v: string) {
    setSearch(v);
    if (searchTimer) clearTimeout(searchTimer);
    setSearchTimer(setTimeout(() => { setDebouncedSearch(v); setPage(1); }, 350));
  }

  const hasBalance =
    balanceFilter === 'outstanding' ? true :
    balanceFilter === 'settled' ? false :
    undefined;

  const { data, isLoading, isError, error } = useAllCreditAccounts({
    term: debouncedSearch || undefined,
    hasBalance,
    page,
    pageSize: PAGE_SIZE,
  });

  const items = data?.items ?? [];
  const total = data?.totalCount ?? 0;
  const totalPages = Math.ceil(total / PAGE_SIZE) || 1;
  const totalOutstanding = data?.totalOutstanding ?? 0;
  const totalCredit = data?.totalCreditExtended ?? 0;
  const netAvailable = totalCredit - totalOutstanding;
  const apiError = isError ? extractApiError(error) : null;

  return (
    <div className="flex flex-col gap-6 p-6 min-h-0">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100">Credit Accounts</h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Track customer credit balances, record payments, and manage credit limits
          </p>
        </div>
        <button
          onClick={() => setAddOpen(true)}
          className="flex items-center gap-2 px-4 py-2 rounded-xl bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium shadow-sm transition-colors"
        >
          <Plus className="w-4 h-4" />
          New Credit Account
        </button>
      </div>

      {/* Stat cards */}
      <div className="grid grid-cols-2 xl:grid-cols-4 gap-4">
        {[
          {
            label: 'Total Accounts',
            value: isLoading ? '…' : total.toLocaleString(),
            icon: <Users className="w-5 h-5" />,
            bg: 'bg-indigo-100 dark:bg-indigo-900/30',
            ic: 'text-indigo-600 dark:text-indigo-400',
            val: 'text-gray-900 dark:text-gray-100',
          },
          {
            label: 'Total Outstanding',
            value: isLoading ? '…' : fmtMoney(totalOutstanding, true),
            icon: <CreditCard className="w-5 h-5" />,
            bg: 'bg-red-100 dark:bg-red-900/30',
            ic: 'text-red-600 dark:text-red-400',
            val: totalOutstanding > 0 ? 'text-red-600 dark:text-red-400' : 'text-gray-900 dark:text-gray-100',
          },
          {
            label: 'Credit Extended',
            value: isLoading ? '…' : fmtMoney(totalCredit, true),
            icon: <TrendingUp className="w-5 h-5" />,
            bg: 'bg-amber-100 dark:bg-amber-900/30',
            ic: 'text-amber-600 dark:text-amber-400',
            val: 'text-gray-900 dark:text-gray-100',
          },
          {
            label: 'Net Available',
            value: isLoading ? '…' : fmtMoney(netAvailable, true),
            icon: <Wallet className="w-5 h-5" />,
            bg: 'bg-emerald-100 dark:bg-emerald-900/30',
            ic: 'text-emerald-600 dark:text-emerald-400',
            val: 'text-emerald-700 dark:text-emerald-300',
          },
        ].map(({ label, value, icon, bg, ic, val }) => (
          <div
            key={label}
            className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-4 shadow-sm"
          >
            <div className="flex items-center gap-3">
              <div className={cn('p-2.5 rounded-xl', bg)}>
                <span className={ic}>{icon}</span>
              </div>
              <div>
                <p className="text-xs text-gray-500 dark:text-gray-400">{label}</p>
                <p className={cn('text-xl font-bold', val)}>{value}</p>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Filter bar */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center gap-3">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            className="w-full pl-9 pr-9 py-2 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            placeholder="Search by customer name or code…"
            value={search}
            onChange={(e) => handleSearch(e.target.value)}
          />
          {search && (
            <button
              onClick={() => handleSearch('')}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
            >
              <X className="w-3.5 h-3.5" />
            </button>
          )}
        </div>

        <div className="flex gap-1 bg-gray-100 dark:bg-gray-800 rounded-xl p-1">
          {BALANCE_FILTERS.map((f) => (
            <button
              key={f.id}
              onClick={() => { setBalanceFilter(f.id); setPage(1); }}
              className={cn(
                'px-3 py-1.5 rounded-lg text-sm font-medium transition-all whitespace-nowrap',
                balanceFilter === f.id
                  ? 'bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 shadow-sm'
                  : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200',
              )}
            >
              {f.label}
            </button>
          ))}
        </div>
      </div>

      {/* Content */}
      {isLoading ? (
        <div className="flex items-center justify-center py-24">
          <Loader2 className="w-8 h-8 text-indigo-500 animate-spin" />
        </div>
      ) : apiError ? (
        <div className="flex items-center gap-3 p-4 rounded-xl bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800">
          <AlertCircle className="w-5 h-5 text-red-500 flex-shrink-0" />
          <p className="text-sm text-red-700 dark:text-red-300">{apiError}</p>
        </div>
      ) : items.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-24 text-center">
          <div className="p-4 rounded-2xl bg-gray-100 dark:bg-gray-800 mb-4">
            <CreditCard className="w-8 h-8 text-gray-400" />
          </div>
          <p className="text-gray-600 dark:text-gray-400 font-medium">No credit accounts found</p>
          <p className="text-gray-400 dark:text-gray-500 text-sm mt-1">
            {search
              ? `No results for "${search}"`
              : balanceFilter !== 'all'
                ? `No ${balanceFilter === 'outstanding' ? 'accounts with outstanding balance' : 'settled accounts'}`
                : 'Credit accounts are created when a customer is given a credit limit'}
          </p>
        </div>
      ) : (
        <>
          <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 overflow-hidden shadow-sm">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50">
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Customer</th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Store</th>
                    <th className="px-4 py-3 text-right text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Credit Limit</th>
                    <th className="px-4 py-3 text-right text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Outstanding</th>
                    <th className="px-4 py-3 text-right text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Available</th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider min-w-[120px]">Utilisation</th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Last Payment</th>
                    <th className="px-4 py-3 text-right text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50 dark:divide-gray-800">
                  {items.map((acc) => {
                    const usedPct = acc.creditLimit > 0
                      ? Math.min(100, (acc.outstandingBalance / acc.creditLimit) * 100)
                      : 0;
                    const isOverused = usedPct >= 90;
                    const isWarning = usedPct >= 70 && usedPct < 90;

                    return (
                      <tr
                        key={acc.id}
                        className="hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors"
                      >
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-2.5">
                            <div className="w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center flex-shrink-0">
                              <span className="text-xs font-bold text-blue-600 dark:text-blue-400">
                                {acc.customerName[0]?.toUpperCase()}
                              </span>
                            </div>
                            <div>
                              <p className="font-medium text-gray-900 dark:text-gray-100">
                                {acc.customerName}
                              </p>
                              <p className="text-xs text-gray-400">{acc.customerCode}</p>
                            </div>
                          </div>
                        </td>
                        <td className="px-4 py-3 text-gray-600 dark:text-gray-400 text-sm">
                          {acc.storeName}
                        </td>
                        <td className="px-4 py-3 text-right font-medium text-gray-900 dark:text-gray-100 tabular-nums">
                          {fmtMoney(acc.creditLimit)}
                        </td>
                        <td className="px-4 py-3 text-right tabular-nums">
                          <span className={cn(
                            'font-semibold',
                            acc.outstandingBalance > 0
                              ? isOverused
                                ? 'text-red-600 dark:text-red-400'
                                : isWarning
                                  ? 'text-amber-600 dark:text-amber-400'
                                  : 'text-gray-900 dark:text-gray-100'
                              : 'text-gray-400 dark:text-gray-500',
                          )}>
                            {acc.outstandingBalance > 0 ? fmtMoney(acc.outstandingBalance) : '—'}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-right font-medium text-emerald-600 dark:text-emerald-400 tabular-nums">
                          {fmtMoney(acc.availableCredit)}
                        </td>
                        <td className="px-4 py-3 min-w-[140px]">
                          <UtilBar used={acc.outstandingBalance} limit={acc.creditLimit} />
                        </td>
                        <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">
                          {fmtDate(acc.lastPaymentAt)}
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex items-center justify-end gap-1">
                            {acc.outstandingBalance > 0 && (
                              <button
                                title="Record Payment"
                                onClick={() => setPayTarget(acc)}
                                className="p-1.5 rounded-lg text-emerald-600 dark:text-emerald-400 hover:bg-emerald-50 dark:hover:bg-emerald-900/20 transition-colors"
                              >
                                <ArrowDownCircle className="w-4 h-4" />
                              </button>
                            )}
                            <button
                              title="View Details"
                              onClick={() => setDetailId(acc.id)}
                              className="p-1.5 rounded-lg text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
                            >
                              <Eye className="w-4 h-4" />
                            </button>
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
              {total} account{total !== 1 ? 's' : ''}
              {debouncedSearch && ` matching "${debouncedSearch}"`}
              {balanceFilter !== 'all' && ` · ${balanceFilter}`}
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

      {/* Detail modal */}
      <CreditAccountDetailModal
        creditAccountId={detailId}
        open={!!detailId}
        onClose={() => setDetailId(null)}
      />

      {/* Quick payment dialog */}
      <QuickPaymentDialog
        account={payTarget}
        onClose={() => setPayTarget(null)}
      />

      {/* Add credit account modal */}
      <AddCreditAccountModal
        open={addOpen}
        onClose={() => setAddOpen(false)}
      />
    </div>
  );
}
