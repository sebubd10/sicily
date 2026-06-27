import { useState, useEffect } from 'react';
import {
  UserPlus, Search, Users, Eye, Pencil, Power, PowerOff, Trash2,
  ChevronLeft, ChevronRight, AlertCircle, Loader2,
  Phone, Mail, CreditCard, Star, X,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { Customer } from '../types/customer';
import {
  useCustomers, useDeactivateCustomer, useActivateCustomer, useDeleteCustomer,
} from '../hooks/useCustomers';
import { CustomerFormModal } from '../components/customers/CustomerFormModal';
import { CustomerDetailModal } from '../components/customers/CustomerDetailModal';
import { ConfirmDialog } from '../components/ui/ConfirmDialog';

const PAGE_SIZE = 20;

type StatusFilter = 'All' | 'Active' | 'Inactive';
type ConfirmAction =
  | { type: 'activate' | 'deactivate'; customer: Customer }
  | { type: 'delete'; customer: Customer }
  | null;

const STATUS_FILTERS: StatusFilter[] = ['All', 'Active', 'Inactive'];

function useDebounce<T>(value: T, delay: number): T {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const t = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(t);
  }, [value, delay]);
  return debounced;
}

function fmtMoney(n: number) {
  return `৳${n.toLocaleString('en-BD', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`;
}

export default function CustomersPage() {
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('All');
  const [page, setPage] = useState(1);
  const [createOpen, setCreateOpen] = useState(false);
  const [editCustomer, setEditCustomer] = useState<Customer | null>(null);
  const [detailCustomer, setDetailCustomer] = useState<Customer | null>(null);
  const [confirmAction, setConfirmAction] = useState<ConfirmAction>(null);
  const [confirmError, setConfirmError] = useState<string | null>(null);

  const debouncedSearch = useDebounce(search, 350);

  const { data, isLoading, isError, error } = useCustomers({
    term: debouncedSearch || undefined,
    page,
    pageSize: PAGE_SIZE,
  });

  const { mutate: deactivate, isPending: deactivating } = useDeactivateCustomer();
  const { mutate: activate, isPending: activating } = useActivateCustomer();
  const { mutate: deleteCustomer, isPending: deleting } = useDeleteCustomer();
  const isMutating = deactivating || activating || deleting;

  const items = data?.items ?? [];
  const total = data?.totalCount ?? 0;
  const totalPages = Math.ceil(total / PAGE_SIZE) || 1;
  const apiError = isError ? extractApiError(error) : null;

  // Client-side status filter (server doesn't support status filter yet)
  const filtered = statusFilter === 'All' ? items : items.filter((c) => c.status === statusFilter);

  const activeCount = items.filter((c) => c.status === 'Active').length;
  const inactiveCount = items.filter((c) => c.status === 'Inactive').length;

  function handleSearch(v: string) {
    setSearch(v);
    setPage(1);
  }

  function openDetail(c: Customer) {
    setDetailCustomer(c);
  }

  function openEdit(c: Customer) {
    setDetailCustomer(null);
    setEditCustomer(c);
  }

  function handleToggleStatus(c: Customer) {
    setConfirmError(null);
    setConfirmAction({ type: c.status === 'Active' ? 'deactivate' : 'activate', customer: c });
  }

  function executeConfirm() {
    if (!confirmAction) return;
    setConfirmError(null);
    const done = {
      onSuccess: () => setConfirmAction(null),
      onError: (err: unknown) => setConfirmError(extractApiError(err)),
    };
    if (confirmAction.type === 'deactivate') deactivate(confirmAction.customer.id, done);
    else if (confirmAction.type === 'activate') activate(confirmAction.customer.id, done);
    else deleteCustomer(confirmAction.customer.id, done);
  }

  return (
    <div className="flex flex-col gap-6 p-6 min-h-0">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100">Customers</h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Manage customer accounts, credit limits, and loyalty points
          </p>
        </div>
        <button
          onClick={() => setCreateOpen(true)}
          className="flex items-center gap-2 px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium shadow-sm transition-colors"
        >
          <UserPlus className="w-4 h-4" />
          New Customer
        </button>
      </div>

      {/* Stat cards */}
      <div className="grid grid-cols-3 gap-4">
        <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-4 shadow-sm">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-xl bg-indigo-100 dark:bg-indigo-900/30">
              <Users className="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
            </div>
            <div>
              <p className="text-xs text-gray-500 dark:text-gray-400">Total</p>
              <p className="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {isLoading ? '…' : total}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-4 shadow-sm">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-xl bg-emerald-100 dark:bg-emerald-900/30">
              <Users className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
            </div>
            <div>
              <p className="text-xs text-gray-500 dark:text-gray-400">Active</p>
              <p className="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {isLoading ? '…' : activeCount}
              </p>
            </div>
          </div>
        </div>
        <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 p-4 shadow-sm">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-xl bg-gray-100 dark:bg-gray-800">
              <CreditCard className="w-5 h-5 text-gray-500 dark:text-gray-400" />
            </div>
            <div>
              <p className="text-xs text-gray-500 dark:text-gray-400">With Credit</p>
              <p className="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {isLoading ? '…' : items.filter((c) => c.creditLimit > 0).length}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Search + filter bar */}
      <div className="flex items-center gap-3">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            className="w-full pl-9 pr-9 py-2 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            placeholder="Search by name, code, phone, email…"
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
          {STATUS_FILTERS.map((s) => (
            <button
              key={s}
              onClick={() => setStatusFilter(s)}
              className={cn(
                'px-3 py-1.5 rounded-lg text-sm font-medium transition-all',
                statusFilter === s
                  ? 'bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 shadow-sm'
                  : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200',
              )}
            >
              {s}
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
      ) : filtered.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-24 text-center">
          <div className="p-4 rounded-2xl bg-gray-100 dark:bg-gray-800 mb-4">
            <Users className="w-8 h-8 text-gray-400" />
          </div>
          <p className="text-gray-600 dark:text-gray-400 font-medium">No customers found</p>
          <p className="text-gray-400 dark:text-gray-500 text-sm mt-1">
            {search ? `No results for "${search}"` : 'Register your first customer to get started'}
          </p>
          {!search && (
            <button
              onClick={() => setCreateOpen(true)}
              className="mt-4 flex items-center gap-2 px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium"
            >
              <UserPlus className="w-4 h-4" />
              New Customer
            </button>
          )}
        </div>
      ) : (
        <>
          {/* Table */}
          <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 overflow-hidden shadow-sm">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50">
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Customer
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Contact
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Loyalty Pts
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Credit Limit
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Outstanding
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50 dark:divide-gray-800">
                  {filtered.map((c) => (
                    <tr
                      key={c.id}
                      className="hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors cursor-pointer"
                      onClick={() => openDetail(c)}
                    >
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-3">
                          <div className="w-8 h-8 rounded-full bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center flex-shrink-0">
                            <span className="text-xs font-bold text-indigo-600 dark:text-indigo-400">
                              {c.name[0].toUpperCase()}
                            </span>
                          </div>
                          <div>
                            <p className="font-medium text-gray-900 dark:text-gray-100">{c.name}</p>
                            <p className="text-xs text-gray-400">{c.code}</p>
                          </div>
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <div className="space-y-0.5">
                          {c.phone && (
                            <div className="flex items-center gap-1.5 text-xs text-gray-500 dark:text-gray-400">
                              <Phone className="w-3 h-3" />
                              {c.phone}
                            </div>
                          )}
                          {c.email && (
                            <div className="flex items-center gap-1.5 text-xs text-gray-500 dark:text-gray-400">
                              <Mail className="w-3 h-3" />
                              <span className="truncate max-w-[140px]">{c.email}</span>
                            </div>
                          )}
                          {!c.phone && !c.email && (
                            <span className="text-xs text-gray-300 dark:text-gray-600">—</span>
                          )}
                        </div>
                      </td>
                      <td className="px-4 py-3 text-left">
                        {c.loyaltyPoints > 0 ? (
                          <div className="flex items-center justify-start gap-1">
                            <Star className="w-3.5 h-3.5 text-amber-400 fill-amber-400" />
                            <span className="font-medium text-gray-900 dark:text-gray-100">
                              {c.loyaltyPoints.toLocaleString()}
                            </span>
                          </div>
                        ) : (
                          <span className="text-gray-300 dark:text-gray-600">—</span>
                        )}
                      </td>
                      <td className="px-4 py-3 text-left font-medium text-gray-900 dark:text-gray-100">
                        {c.creditLimit > 0 ? fmtMoney(c.creditLimit) : (
                          <span className="text-gray-300 dark:text-gray-600">—</span>
                        )}
                      </td>
                      <td className="px-4 py-3 text-left">
                        {c.currentBalance > 0 ? (
                          <span className="font-medium text-red-600 dark:text-red-400">
                            {fmtMoney(c.currentBalance)}
                          </span>
                        ) : (
                          <span className="text-gray-300 dark:text-gray-600">—</span>
                        )}
                      </td>
                      <td className="px-4 py-3">
                        <span className={cn(
                          'inline-flex px-2 py-0.5 rounded-full text-xs font-medium',
                          c.status === 'Active'
                            ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400'
                            : 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400',
                        )}>
                          {c.status}
                        </span>
                      </td>
                      <td className="px-4 py-3">
                        <div
                          className="flex items-center justify-start gap-1"
                          onClick={(e) => e.stopPropagation()}
                        >
                          <button
                            title="View"
                            onClick={() => openDetail(c)}
                            className="p-1.5 rounded-lg text-gray-400 hover:text-gray-700 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
                          >
                            <Eye className="w-4 h-4" />
                          </button>
                          <button
                            title="Edit"
                            onClick={() => openEdit(c)}
                            disabled={isMutating}
                            className="p-1.5 rounded-lg text-gray-400 hover:text-indigo-600 hover:bg-indigo-50 dark:hover:bg-indigo-900/20 transition-colors disabled:opacity-40"
                          >
                            <Pencil className="w-4 h-4" />
                          </button>
                          <button
                            title={c.status === 'Active' ? 'Deactivate' : 'Activate'}
                            onClick={() => handleToggleStatus(c)}
                            disabled={isMutating}
                            className={cn(
                              'p-1.5 rounded-lg transition-colors disabled:opacity-40',
                              c.status === 'Active'
                                ? 'text-gray-400 hover:text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-900/20'
                                : 'text-gray-400 hover:text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/20',
                            )}
                          >
                            {c.status === 'Active'
                              ? <PowerOff className="w-4 h-4" />
                              : <Power className="w-4 h-4" />}
                          </button>
                          {c.status === 'Inactive' && (
                            <button
                              title="Delete customer"
                              onClick={() => {
                                setConfirmError(null);
                                setConfirmAction({ type: 'delete', customer: c });
                              }}
                              disabled={isMutating}
                              className="p-1.5 rounded-lg text-gray-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-40"
                            >
                              <Trash2 className="w-4 h-4" />
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Pagination */}
          <div className="flex items-center justify-between text-sm text-gray-500 dark:text-gray-400">
            <span>
              {total} customer{total !== 1 ? 's' : ''}
              {debouncedSearch && ` matching "${debouncedSearch}"`}
            </span>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="p-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 disabled:opacity-40 transition-colors"
              >
                <ChevronLeft className="w-4 h-4" />
              </button>
              <span className="text-xs">
                Page {page} of {totalPages}
              </span>
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
      <CustomerFormModal
        open={createOpen}
        onClose={() => setCreateOpen(false)}
      />

      <CustomerFormModal
        open={!!editCustomer}
        onClose={() => setEditCustomer(null)}
        editCustomer={editCustomer}
      />

      <CustomerDetailModal
        customer={detailCustomer}
        open={!!detailCustomer}
        onClose={() => setDetailCustomer(null)}
        onEdit={() => {
          if (detailCustomer) openEdit(detailCustomer);
        }}
      />

      <ConfirmDialog
        open={!!confirmAction}
        title={
          confirmAction?.type === 'delete'
            ? 'Delete Customer'
            : confirmAction?.type === 'activate'
            ? 'Activate Customer'
            : 'Deactivate Customer'
        }
        message={
          confirmAction?.type === 'delete'
            ? `Permanently delete "${confirmAction.customer.name}"? Their record will be hidden from all lists. Transaction history is preserved but the customer cannot transact. This cannot be undone.`
            : confirmAction?.type === 'activate'
            ? `"${confirmAction.customer.name}" will be restored and able to transact again.`
            : `Deactivate "${confirmAction?.customer.name}"? They will no longer be able to transact.`
        }
        confirmLabel={
          confirmAction?.type === 'delete'
            ? 'Delete'
            : confirmAction?.type === 'activate'
            ? 'Activate'
            : 'Deactivate'
        }
        variant={confirmAction?.type === 'delete' ? 'danger' : 'warning'}
        loading={isMutating}
        error={confirmError}
        onConfirm={executeConfirm}
        onClose={() => { setConfirmAction(null); setConfirmError(null); }}
      />
    </div>
  );
}
