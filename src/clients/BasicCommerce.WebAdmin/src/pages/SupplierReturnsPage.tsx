import { useState } from 'react';
import {
  Plus, Eye, XCircle, Loader2, AlertCircle,
  RotateCcw, ChevronLeft, ChevronRight, Truck, BadgeDollarSign,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import {
  RETURN_STATUS_CONFIG,
  type SupplierReturnStatus,
  type SupplierReturnSummary,
  type CreateSupplierReturnFormData,
} from '../types/supplierReturn';
import {
  useSupplierReturns,
  useCreateSupplierReturn,
  useCancelSupplierReturn,
} from '../hooks/useSupplierReturns';
import { useSuppliers } from '../hooks/useSuppliers';
import { useStores } from '../hooks/useWarehouses';
import { useProducts } from '../hooks/useProducts';
import { CreateSupplierReturnModal } from '../components/supplierReturns/CreateSupplierReturnModal';
import { SupplierReturnDetailModal } from '../components/supplierReturns/SupplierReturnDetailModal';
import { ConfirmDialog } from '../components/ui/ConfirmDialog';

const PAGE_SIZE = 15;

type StatusFilter = 'All' | SupplierReturnStatus;

const STATUS_FILTERS: StatusFilter[] = [
  'All', 'Draft', 'Submitted', 'Shipped', 'CreditReceived', 'Cancelled',
];

const STATUS_FILTER_LABELS: Record<StatusFilter, string> = {
  All: 'All',
  Draft: 'Draft',
  Submitted: 'Submitted',
  Shipped: 'Shipped',
  CreditReceived: 'Credit Received',
  Cancelled: 'Cancelled',
};

function fmtDate(d: string | null | undefined): string {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

function fmtMoney(n: number | null | undefined): string {
  if (n == null) return '—';
  return n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

const CANCELLABLE: SupplierReturnStatus[] = ['Draft', 'Submitted'];

export default function SupplierReturnsPage() {
  const [page, setPage]                 = useState(1);
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('All');
  const [supplierFilter, setSupplierFilter] = useState('');

  const [createOpen, setCreateOpen]   = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);
  const [detailId, setDetailId]       = useState<string | null>(null);
  const [cancelSr, setCancelSr]       = useState<SupplierReturnSummary | null>(null);
  const [cancelError, setCancelError] = useState<string | null>(null);

  const { data, isLoading, isError, isFetching } = useSupplierReturns({
    page,
    pageSize: PAGE_SIZE,
    status: statusFilter !== 'All' ? statusFilter : undefined,
    supplierId: supplierFilter || undefined,
  });

  const { data: suppliers = [] } = useSuppliers();
  const { data: stores = [] }    = useStores();
  const { data: productsList }   = useProducts({ page: 1, pageSize: 500 });
  const products = (productsList?.items ?? []).map((p) => ({
    id: p.id, name: p.name, sku: p.sku,
  }));

  const createMutation = useCreateSupplierReturn();
  const cancelMutation = useCancelSupplierReturn();

  const totalPages = data ? Math.ceil(data.totalCount / PAGE_SIZE) : 1;

  function changePage(next: number) { setPage(next); window.scrollTo({ top: 0, behavior: 'smooth' }); }

  async function handleCreate(form: CreateSupplierReturnFormData) {
    try {
      setCreateError(null);
      const created = await createMutation.mutateAsync(form);
      setCreateOpen(false);
      setDetailId(created.id);
    } catch (err) {
      setCreateError(extractApiError(err));
    }
  }

  async function handleCancel() {
    if (!cancelSr) return;
    try {
      setCancelError(null);
      await cancelMutation.mutateAsync(cancelSr.id);
      setCancelSr(null);
    } catch (err) {
      setCancelError(extractApiError(err));
    }
  }

  const returns = data?.items ?? [];

  const selectCls =
    'text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg px-3 py-2 text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 cursor-pointer transition';

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <RotateCcw className="w-6 h-6 text-primary-700" />
            Supplier Returns
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Manage returns to suppliers and track credit notes
          </p>
        </div>
        <button
          onClick={() => setCreateOpen(true)}
          className="flex items-center gap-2 px-4 py-2 bg-primary-800 hover:bg-primary-900 text-white text-sm font-medium rounded-lg transition-colors shadow-sm self-start"
        >
          <Plus className="w-4 h-4" />
          New Return
        </button>
      </div>

      {/* Filters */}
      <div className="flex flex-col gap-3">
        <div className="flex items-center gap-1 bg-gray-100 dark:bg-gray-800 rounded-lg p-1 w-fit flex-wrap">
          {STATUS_FILTERS.map((opt) => (
            <button
              key={opt}
              onClick={() => { setStatusFilter(opt); setPage(1); }}
              className={cn(
                'px-3 py-1 text-sm font-medium rounded-md transition-colors whitespace-nowrap',
                statusFilter === opt
                  ? 'bg-white dark:bg-gray-700 text-gray-900 dark:text-white shadow-sm'
                  : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300',
              )}
            >
              {STATUS_FILTER_LABELS[opt]}
            </button>
          ))}
        </div>

        <div className="flex flex-wrap items-center gap-3">
          <select
            value={supplierFilter}
            onChange={(e) => { setSupplierFilter(e.target.value); setPage(1); }}
            className={selectCls}
          >
            <option value="">All Suppliers</option>
            {suppliers.map((s) => (
              <option key={s.id} value={s.id}>{s.name}</option>
            ))}
          </select>

          {isFetching && !isLoading && <Loader2 className="w-4 h-4 animate-spin text-gray-400" />}

          {data && (
            <span className="text-xs text-gray-400 ml-auto">
              {data.totalCount} return{data.totalCount !== 1 ? 's' : ''}
            </span>
          )}
        </div>
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
        {isError ? (
          <div className="flex items-center gap-2 px-6 py-12 text-red-500">
            <AlertCircle className="w-5 h-5 flex-shrink-0" />
            <span className="text-sm">Failed to load supplier returns. Please refresh.</span>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                    {['Return #', 'Supplier', 'Status', 'Return Value', 'Expected Credit', 'Actual Credit', 'Created', 'Shipped', 'Actions'].map((h) => (
                      <th key={h} className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider whitespace-nowrap">
                        {h}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                  {isLoading ? (
                    Array.from({ length: 5 }).map((_, i) => (
                      <tr key={i} className="animate-pulse">
                        {Array.from({ length: 9 }).map((_, j) => (
                          <td key={j} className="px-4 py-3">
                            <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-full" />
                          </td>
                        ))}
                      </tr>
                    ))
                  ) : returns.length === 0 ? (
                    <tr>
                      <td colSpan={9} className="px-4 py-14 text-center">
                        <RotateCcw className="w-8 h-8 text-gray-300 dark:text-gray-600 mx-auto mb-2" />
                        <p className="text-sm text-gray-400">No supplier returns found.</p>
                      </td>
                    </tr>
                  ) : (
                    returns.map((sr) => {
                      const cfg = RETURN_STATUS_CONFIG[sr.status];
                      const cancellable = CANCELLABLE.includes(sr.status);
                      return (
                        <tr
                          key={sr.id}
                          className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors cursor-pointer"
                          onClick={() => setDetailId(sr.id)}
                        >
                          <td className="px-4 py-3 font-mono text-xs font-semibold text-primary-700 dark:text-primary-400 whitespace-nowrap">
                            {sr.returnNumber}
                          </td>
                          <td className="px-4 py-3 text-gray-800 dark:text-gray-200 whitespace-nowrap">
                            {sr.supplierName}
                          </td>
                          <td className="px-4 py-3">
                            <span className={cn('inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium', cfg.color)}>
                              <span className={cn('w-1.5 h-1.5 rounded-full mr-1.5', cfg.dot)} />
                              {cfg.label}
                            </span>
                          </td>
                          <td className="px-4 py-3 text-right font-medium text-gray-800 dark:text-gray-200 tabular-nums whitespace-nowrap">
                            {fmtMoney(sr.totalReturnValue)}
                          </td>
                          <td className="px-4 py-3 text-right text-blue-600 dark:text-blue-400 tabular-nums whitespace-nowrap">
                            {fmtMoney(sr.expectedCreditAmount)}
                          </td>
                          <td className="px-4 py-3 text-right text-emerald-600 dark:text-emerald-400 tabular-nums whitespace-nowrap">
                            {fmtMoney(sr.actualCreditAmount)}
                          </td>
                          <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                            {fmtDate(sr.createdAt)}
                          </td>
                          <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                            {sr.status === 'Shipped' || sr.status === 'CreditReceived'
                              ? <span className="flex items-center gap-1"><Truck className="w-3.5 h-3.5 text-purple-500" />{fmtDate(sr.shippedAt)}</span>
                              : <span className="text-gray-300 dark:text-gray-600">—</span>}
                          </td>
                          <td className="px-4 py-3" onClick={(e) => e.stopPropagation()}>
                            <div className="flex items-center gap-1">
                              <button
                                title="View Details"
                                onClick={() => setDetailId(sr.id)}
                                className="p-1.5 rounded-lg text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors"
                              >
                                <Eye className="w-3.5 h-3.5" />
                              </button>
                              {sr.status === 'Shipped' && (
                                <span title="Shipped — awaiting credit">
                                  <BadgeDollarSign className="w-3.5 h-3.5 text-purple-400" />
                                </span>
                              )}
                              {cancellable && (
                                <button
                                  title="Cancel Return"
                                  onClick={() => setCancelSr(sr)}
                                  className="p-1.5 rounded-lg text-gray-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
                                >
                                  <XCircle className="w-3.5 h-3.5" />
                                </button>
                              )}
                            </div>
                          </td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            {data && data.totalCount > 0 && (
              <div className="flex items-center justify-between px-6 py-3 border-t border-gray-100 dark:border-gray-800">
                <span className="text-xs text-gray-400">
                  {data.totalCount <= PAGE_SIZE
                    ? `${data.totalCount} return${data.totalCount !== 1 ? 's' : ''}`
                    : `Page ${page} of ${totalPages} · ${data.totalCount} total`}
                </span>
                {totalPages > 1 && (
                  <div className="flex items-center gap-1">
                    <button
                      onClick={() => changePage(Math.max(1, page - 1))}
                      disabled={page === 1 || isFetching}
                      className="p-1.5 rounded-lg text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800 disabled:opacity-40 transition-colors"
                    >
                      <ChevronLeft className="w-4 h-4" />
                    </button>
                    {Array.from({ length: Math.min(totalPages, 7) }, (_, i) => {
                      const p = totalPages <= 7 ? i + 1
                        : page <= 4 ? i + 1
                        : page >= totalPages - 3 ? totalPages - 6 + i
                        : page - 3 + i;
                      return (
                        <button
                          key={p}
                          onClick={() => changePage(p)}
                          className={cn(
                            'w-8 h-8 text-sm rounded-lg transition-colors',
                            page === p
                              ? 'bg-primary-800 text-white font-semibold'
                              : 'text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800',
                          )}
                        >
                          {p}
                        </button>
                      );
                    })}
                    <button
                      onClick={() => changePage(Math.min(totalPages, page + 1))}
                      disabled={page === totalPages || isFetching}
                      className="p-1.5 rounded-lg text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800 disabled:opacity-40 transition-colors"
                    >
                      <ChevronRight className="w-4 h-4" />
                    </button>
                  </div>
                )}
              </div>
            )}
          </>
        )}
      </div>

      {/* Create modal */}
      <CreateSupplierReturnModal
        open={createOpen}
        suppliers={suppliers}
        stores={stores}
        isSaving={createMutation.isPending}
        error={createError}
        onSave={handleCreate}
        onClose={() => { setCreateOpen(false); setCreateError(null); }}
      />

      {/* Detail modal */}
      <SupplierReturnDetailModal
        open={!!detailId}
        srId={detailId}
        products={products}
        onClose={() => setDetailId(null)}
      />

      {/* Cancel confirm */}
      <ConfirmDialog
        open={!!cancelSr}
        title="Cancel Supplier Return"
        message={cancelSr ? `Cancel return ${cancelSr.returnNumber}? This cannot be undone.` : ''}
        confirmLabel="Cancel Return"
        variant="danger"
        loading={cancelMutation.isPending}
        error={cancelError}
        onConfirm={handleCancel}
        onClose={() => { setCancelSr(null); setCancelError(null); }}
      />
    </div>
  );
}
