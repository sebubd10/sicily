import { useState } from 'react';
import {
  Plus, Eye, XCircle, Loader2, AlertCircle,
  ShoppingCart, ChevronLeft, ChevronRight,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import {
  PO_STATUS_CONFIG,
  type PurchaseOrderStatus,
  type PurchaseOrderSummary,
  type CreatePurchaseOrderFormData,
} from '../types/purchaseOrder';
import {
  usePurchaseOrders,
  usePurchaseOrderDetail,
  useCreatePurchaseOrder,
  useCancelPurchaseOrder,
  useReceivePurchaseOrder,
} from '../hooks/usePurchaseOrders';
import { useSuppliers } from '../hooks/useSuppliers';
import { useWarehouses } from '../hooks/useWarehouses';
import { CreatePurchaseOrderModal } from '../components/purchaseOrders/CreatePurchaseOrderModal';
import { PurchaseOrderDetailModal } from '../components/purchaseOrders/PurchaseOrderDetailModal';
import { ReceiveModal } from '../components/purchaseOrders/ReceiveModal';
import { ConfirmDialog } from '../components/ui/ConfirmDialog';

const PAGE_SIZE = 15;

type StatusFilter = 'All' | PurchaseOrderStatus;

const STATUS_FILTERS: StatusFilter[] = [
  'All', 'Draft', 'Submitted', 'PartiallyReceived', 'Received', 'Cancelled',
];

const STATUS_FILTER_LABELS: Record<StatusFilter, string> = {
  All: 'All',
  Draft: 'Draft',
  Submitted: 'Submitted',
  PartiallyReceived: 'Partially Received',
  Received: 'Received',
  Cancelled: 'Cancelled',
};

function fmtDate(d: string | null | undefined): string {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

const CANCELLABLE_STATUSES: PurchaseOrderStatus[] = ['Draft', 'Submitted', 'PartiallyReceived'];

export default function PurchaseOrdersPage() {
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('All');
  const [supplierFilter, setSupplierFilter] = useState('');
  const [warehouseFilter, setWarehouseFilter] = useState('');

  const [createOpen, setCreateOpen]       = useState(false);
  const [detailId, setDetailId]           = useState<string | null>(null);
  const [receivePoId, setReceivePoId]     = useState<string | null>(null);
  const [cancelPo, setCancelPo]           = useState<PurchaseOrderSummary | null>(null);
  const [cancelError, setCancelError]     = useState<string | null>(null);
  const [receiveError, setReceiveError]   = useState<string | null>(null);

  const { data, isLoading, isError, isFetching } = usePurchaseOrders({
    page,
    pageSize: PAGE_SIZE,
    status: statusFilter !== 'All' ? statusFilter : undefined,
    supplierId: supplierFilter || undefined,
    warehouseId: warehouseFilter || undefined,
  });

  const { data: suppliers = [] } = useSuppliers();
  const { data: warehousesData }  = useWarehouses();
  const warehouses = (warehousesData?.items ?? []) as { id: string; name: string }[];

  const { data: receivePo } = usePurchaseOrderDetail(receivePoId);

  const createMutation  = useCreatePurchaseOrder();
  const cancelMutation  = useCancelPurchaseOrder();
  const receiveMutation = useReceivePurchaseOrder();

  const totalPages = data ? Math.ceil(data.totalCount / PAGE_SIZE) : 1;

  function changePage(next: number) {
    setPage(next);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  function handleStatusFilter(s: StatusFilter) { setStatusFilter(s); setPage(1); }
  function handleSupplierFilter(id: string)    { setSupplierFilter(id); setPage(1); }
  function handleWarehouseFilter(id: string)   { setWarehouseFilter(id); setPage(1); }

  async function handleCreate(form: CreatePurchaseOrderFormData) {
    await createMutation.mutateAsync(form);
    setCreateOpen(false);
  }

  async function handleCancel() {
    if (!cancelPo) return;
    try {
      setCancelError(null);
      await cancelMutation.mutateAsync(cancelPo.id);
      setCancelPo(null);
    } catch (err) {
      setCancelError(extractApiError(err));
    }
  }

  async function handleReceive(
    items: { productId: string; receivedQuantity: number }[],
    notes?: string,
  ) {
    if (!receivePoId) return;
    try {
      setReceiveError(null);
      await receiveMutation.mutateAsync({ id: receivePoId, items, notes });
      setReceivePoId(null);
    } catch (err) {
      setReceiveError(extractApiError(err));
    }
  }

  const pos = data?.items ?? [];

  const selectCls =
    'text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg px-3 py-2 text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 cursor-pointer transition';

  return (
    <div className="space-y-5">
      {/* Page header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <ShoppingCart className="w-6 h-6 text-primary-700" />
            Purchase Orders
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Manage supplier purchase orders and stock receiving
          </p>
        </div>
        <button
          onClick={() => setCreateOpen(true)}
          className="flex items-center gap-2 px-4 py-2 bg-primary-800 hover:bg-primary-900 text-white text-sm font-medium rounded-lg transition-colors shadow-sm self-start"
        >
          <Plus className="w-4 h-4" />
          New Purchase Order
        </button>
      </div>

      {/* Filters */}
      <div className="flex flex-col gap-3">
        {/* Status pill-button group */}
        <div className="flex items-center gap-1 bg-gray-100 dark:bg-gray-800 rounded-lg p-1 w-fit flex-wrap">
          {STATUS_FILTERS.map((opt) => (
            <button
              key={opt}
              onClick={() => handleStatusFilter(opt)}
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

        {/* Secondary filters row */}
        <div className="flex flex-wrap items-center gap-3">
          <select
            value={supplierFilter}
            onChange={(e) => handleSupplierFilter(e.target.value)}
            className={selectCls}
          >
            <option value="">All Suppliers</option>
            {suppliers.map((s) => (
              <option key={s.id} value={s.id}>{s.name}</option>
            ))}
          </select>

          <select
            value={warehouseFilter}
            onChange={(e) => handleWarehouseFilter(e.target.value)}
            className={selectCls}
          >
            <option value="">All Warehouses</option>
            {warehouses.map((w) => (
              <option key={w.id} value={w.id}>{w.name}</option>
            ))}
          </select>

          {isFetching && !isLoading && (
            <Loader2 className="w-4 h-4 animate-spin text-gray-400" />
          )}

          {data && (
            <span className="text-xs text-gray-400 ml-auto">
              {data.totalCount} order{data.totalCount !== 1 ? 's' : ''}
            </span>
          )}
        </div>
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
        {isError ? (
          <div className="flex items-center gap-2 px-6 py-12 text-red-500">
            <AlertCircle className="w-5 h-5 flex-shrink-0" />
            <span className="text-sm">Failed to load purchase orders. Please refresh and try again.</span>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                    {['Order #', 'Supplier', 'Warehouse', 'Order Date', 'Expected', 'Items', 'Total', 'Status', 'Actions'].map((h) => (
                      <th
                        key={h}
                        className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider whitespace-nowrap"
                      >
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
                  ) : pos.length === 0 ? (
                    <tr>
                      <td colSpan={9} className="px-4 py-14 text-center">
                        <ShoppingCart className="w-8 h-8 text-gray-300 dark:text-gray-600 mx-auto mb-2" />
                        <p className="text-sm text-gray-400">No purchase orders found.</p>
                      </td>
                    </tr>
                  ) : (
                    pos.map((po) => {
                      const cfg = PO_STATUS_CONFIG[po.status];
                      const cancellable = CANCELLABLE_STATUSES.includes(po.status);
                      return (
                        <tr
                          key={po.id}
                          className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors cursor-pointer"
                          onClick={() => setDetailId(po.id)}
                        >
                          <td className="px-4 py-3 font-mono text-xs font-semibold text-primary-700 dark:text-primary-400 whitespace-nowrap">
                            {po.orderNumber}
                          </td>
                          <td className="px-4 py-3 text-gray-800 dark:text-gray-200 whitespace-nowrap">
                            {po.supplierName}
                          </td>
                          <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                            {po.warehouseName}
                          </td>
                          <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                            {fmtDate(po.orderDate)}
                          </td>
                          <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                            {po.expectedDate
                              ? fmtDate(po.expectedDate)
                              : <span className="text-gray-300 dark:text-gray-600">—</span>}
                          </td>
                          <td className="px-4 py-3 text-center text-gray-500 dark:text-gray-400 tabular-nums">
                            {po.itemsCount}
                          </td>
                          <td className="px-4 py-3 text-right font-medium text-gray-800 dark:text-gray-200 whitespace-nowrap tabular-nums">
                            {po.currency}{' '}
                            {po.totalAmount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                          </td>
                          <td className="px-4 py-3">
                            <span className={cn('inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium', cfg.color)}>
                              <span className={cn('w-1.5 h-1.5 rounded-full mr-1.5', cfg.dot)} />
                              {cfg.label}
                            </span>
                          </td>
                          <td
                            className="px-4 py-3"
                            onClick={(e) => e.stopPropagation()}
                          >
                            <div className="flex items-center gap-1">
                              <button
                                title="View Details"
                                onClick={() => setDetailId(po.id)}
                                className="p-1.5 rounded-lg text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors"
                              >
                                <Eye className="w-3.5 h-3.5" />
                              </button>
                              {cancellable && (
                                <button
                                  title="Cancel Order"
                                  onClick={() => setCancelPo(po)}
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
                    ? `${data.totalCount} order${data.totalCount !== 1 ? 's' : ''}`
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
                      const p = totalPages <= 7
                        ? i + 1
                        : page <= 4
                          ? i + 1
                          : page >= totalPages - 3
                            ? totalPages - 6 + i
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
      <CreatePurchaseOrderModal
        open={createOpen}
        onSave={handleCreate}
        onClose={() => setCreateOpen(false)}
        isSaving={createMutation.isPending}
      />

      {/* Detail modal */}
      <PurchaseOrderDetailModal
        open={!!detailId}
        poId={detailId}
        onClose={() => setDetailId(null)}
        onReceive={(id) => { setDetailId(null); setReceivePoId(id); }}
      />

      {/* Receive modal */}
      <ReceiveModal
        open={!!receivePoId}
        po={receivePo ?? null}
        onSave={handleReceive}
        onClose={() => { setReceivePoId(null); setReceiveError(null); }}
        isSaving={receiveMutation.isPending}
        error={receiveError}
      />

      {/* Cancel confirm */}
      <ConfirmDialog
        open={!!cancelPo}
        title="Cancel Purchase Order"
        message={cancelPo ? `Cancel order ${cancelPo.orderNumber}? This action cannot be undone.` : ''}
        confirmLabel="Cancel Order"
        variant="danger"
        loading={cancelMutation.isPending}
        error={cancelError}
        onConfirm={handleCancel}
        onClose={() => { setCancelPo(null); setCancelError(null); }}
      />
    </div>
  );
}
