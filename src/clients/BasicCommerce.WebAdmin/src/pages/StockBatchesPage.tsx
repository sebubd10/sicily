import { useState, useMemo } from 'react';
import {
  Layers, Store as StoreIcon, RefreshCw, Plus, Trash2,
  AlertTriangle, AlertCircle, Loader2, Clock, CheckCircle,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { StockBatch, ReceiveBatchRequest } from '../types/inventory';
import { getBatchStatus } from '../types/inventory';
import {
  useStockBatches, useExpiringBatches,
  useReceiveBatch, useExpireBatches, useDeleteStockBatch,
} from '../hooks/useInventory';
import { useStores } from '../hooks/useWarehouses';
import { ReceiveBatchModal } from '../components/inventory/ReceiveBatchModal';
import { ConfirmDialog } from '../components/ui/ConfirmDialog';
import { Pagination } from '../components/ui/Pagination';

function fmtDate(d: string | null | undefined): string {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

function daysUntil(d: string): number {
  return Math.ceil((new Date(d).getTime() - Date.now()) / (1000 * 60 * 60 * 24));
}

const STATUS_CONFIG = {
  active:   { label: 'Active',         dot: 'bg-emerald-500', text: 'text-emerald-700 dark:text-emerald-400', bg: 'bg-emerald-50 dark:bg-emerald-900/30' },
  expiring: { label: 'Expiring Soon',  dot: 'bg-amber-500',   text: 'text-amber-700 dark:text-amber-400',     bg: 'bg-amber-50 dark:bg-amber-900/30' },
  expired:  { label: 'Expired',        dot: 'bg-red-500',     text: 'text-red-700 dark:text-red-400',         bg: 'bg-red-50 dark:bg-red-900/30' },
  depleted: { label: 'Depleted',       dot: 'bg-gray-400',    text: 'text-gray-500 dark:text-gray-400',       bg: 'bg-gray-100 dark:bg-gray-800' },
};

function StatusBadge({ batch }: { batch: StockBatch }) {
  const status = getBatchStatus(batch);
  const cfg = STATUS_CONFIG[status];
  return (
    <span className={cn('inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full text-xs font-medium', cfg.bg, cfg.text)}>
      <span className={cn('w-1.5 h-1.5 rounded-full flex-shrink-0', cfg.dot)} />
      {cfg.label}
      {status === 'expiring' && batch.expiryDate && (
        <span className="opacity-75">· {daysUntil(batch.expiryDate)}d</span>
      )}
    </span>
  );
}

export default function StockBatchesPage() {
  const [storeId, setStoreId] = useState('');
  const [page, setPage] = useState(1);
  const [includeExpired, setIncludeExpired] = useState(false);
  const [showReceive, setShowReceive] = useState(false);
  const [receiveError, setReceiveError] = useState<string | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<StockBatch | null>(null);
  const [deleteError, setDeleteError] = useState<string | null>(null);
  const [expireError, setExpireError] = useState<string | null>(null);

  const PAGE_SIZE = 20;
  const { data: stores = [] } = useStores();
  const { data: batchData, isLoading, isError, isFetching, refetch } = useStockBatches(
    storeId || null, { includeExpired, page, pageSize: PAGE_SIZE },
  );
  const { data: expiringSoon = [] } = useExpiringBatches(storeId || null, 7);

  const receiveMutation = useReceiveBatch();
  const expireMutation  = useExpireBatches();
  const deleteMutation  = useDeleteStockBatch();

  const batches    = batchData?.items ?? [];
  const totalCount = batchData?.totalCount ?? 0;
  const totalPages = Math.ceil(totalCount / PAGE_SIZE);

  const stats = useMemo(() => {
    const active   = batches.filter((b) => getBatchStatus(b) === 'active').length;
    const expired  = batches.filter((b) => getBatchStatus(b) === 'expired').length;
    const depleted = batches.filter((b) => getBatchStatus(b) === 'depleted').length;
    return { active, expiring: expiringSoon.length, expired: expired + depleted };
  }, [batches, expiringSoon]);

  const selectedStore = stores.find((s) => s.id === storeId);
  const hasExpiredBatches = batches.some((b) => getBatchStatus(b) === 'expired');

  async function handleReceive(data: ReceiveBatchRequest) {
    if (!storeId) return;
    try {
      setReceiveError(null);
      await receiveMutation.mutateAsync({ storeId, request: data });
      setShowReceive(false);
    } catch (err) {
      setReceiveError(extractApiError(err));
    }
  }

  async function handleExpire() {
    if (!storeId) return;
    try {
      setExpireError(null);
      const count = await expireMutation.mutateAsync({ storeId });
      if (count === 0) setExpireError('No expired batches found to process.');
    } catch (err) {
      setExpireError(extractApiError(err));
    }
  }

  async function handleDelete() {
    if (!deleteTarget) return;
    try {
      setDeleteError(null);
      await deleteMutation.mutateAsync({ batchId: deleteTarget.id, storeId });
      setDeleteTarget(null);
    } catch (err) {
      setDeleteError(extractApiError(err));
    }
  }

  const selectCls = 'text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg px-3 py-2 text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 cursor-pointer transition';

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <Layers className="w-6 h-6 text-primary-700" />
            Stock Batches
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Track perishable stock by batch, lot number, and expiry date
          </p>
        </div>

        <div className="flex items-center gap-2 flex-wrap">
          <StoreIcon className="w-4 h-4 text-gray-400 flex-shrink-0" />
          <select value={storeId} onChange={(e) => { setStoreId(e.target.value); setPage(1); }} className={selectCls}>
            <option value="">Select store…</option>
            {stores.filter((s) => s.status === 'Active').map((s) => (
              <option key={s.id} value={s.id}>{s.name}</option>
            ))}
          </select>
          {storeId && (
            <>
              <button
                onClick={() => refetch()} disabled={isFetching}
                className="p-2 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors disabled:opacity-40"
                title="Refresh"
              >
                <RefreshCw className={cn('w-4 h-4', isFetching && 'animate-spin')} />
              </button>
              <button
                onClick={() => { setShowReceive(true); setReceiveError(null); }}
                className="flex items-center gap-1.5 px-3 py-2 text-sm font-medium text-white bg-primary-800 hover:bg-primary-900 rounded-lg transition-colors"
              >
                <Plus className="w-4 h-4" />
                Receive Batch
              </button>
            </>
          )}
        </div>
      </div>

      {/* No store selected */}
      {!storeId && (
        <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm flex flex-col items-center justify-center py-20">
          <StoreIcon className="w-10 h-10 text-gray-300 dark:text-gray-600 mb-3" />
          <p className="text-sm font-medium text-gray-500 dark:text-gray-400">Select a store to view stock batches</p>
          <p className="text-xs text-gray-400 mt-1">Choose a store from the dropdown above</p>
        </div>
      )}

      {storeId && (
        <>
          {/* Stats */}
          <div className="grid grid-cols-3 gap-4">
            <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm p-4">
              <div className="flex items-center gap-2 mb-1">
                <CheckCircle className="w-4 h-4 text-emerald-500" />
                <span className="text-xs text-gray-500 dark:text-gray-400 font-medium">Active Batches</span>
              </div>
              <p className="text-2xl font-bold text-gray-900 dark:text-white">{stats.active}</p>
              <p className="text-xs text-gray-400 mt-0.5">on this page</p>
            </div>
            <div className="bg-white dark:bg-gray-900 rounded-xl border border-amber-200 dark:border-amber-800 shadow-sm p-4">
              <div className="flex items-center gap-2 mb-1">
                <Clock className="w-4 h-4 text-amber-500" />
                <span className="text-xs text-amber-600 dark:text-amber-400 font-medium">Expiring in 7 Days</span>
              </div>
              <p className="text-2xl font-bold text-amber-700 dark:text-amber-400">{stats.expiring}</p>
              <p className="text-xs text-gray-400 mt-0.5">across all batches</p>
            </div>
            <div className="bg-white dark:bg-gray-900 rounded-xl border border-red-200 dark:border-red-800 shadow-sm p-4">
              <div className="flex items-center gap-2 mb-1">
                <AlertTriangle className="w-4 h-4 text-red-500" />
                <span className="text-xs text-red-600 dark:text-red-400 font-medium">Expired / Depleted</span>
              </div>
              <p className="text-2xl font-bold text-red-700 dark:text-red-400">{stats.expired}</p>
              <p className="text-xs text-gray-400 mt-0.5">on this page</p>
            </div>
          </div>

          {/* Toolbar */}
          <div className="flex flex-wrap items-center gap-3">
            {/* Include expired toggle */}
            <label className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 cursor-pointer select-none">
              <div
                onClick={() => { setIncludeExpired((v) => !v); setPage(1); }}
                className={cn(
                  'w-9 h-5 rounded-full transition-colors relative flex-shrink-0',
                  includeExpired ? 'bg-primary-700' : 'bg-gray-300 dark:bg-gray-600',
                )}
              >
                <span className={cn(
                  'absolute top-0.5 w-4 h-4 rounded-full bg-white shadow transition-all',
                  includeExpired ? 'left-4' : 'left-0.5',
                )} />
              </div>
              Show expired & depleted
            </label>

            {/* Expire all button */}
            {hasExpiredBatches && (
              <button
                onClick={handleExpire}
                disabled={expireMutation.isPending}
                className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-red-700 dark:text-red-400 border border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-900/20 hover:bg-red-100 dark:hover:bg-red-900/30 rounded-lg transition-colors disabled:opacity-50"
              >
                {expireMutation.isPending
                  ? <Loader2 className="w-3.5 h-3.5 animate-spin" />
                  : <AlertTriangle className="w-3.5 h-3.5" />}
                Process Expired Batches
              </button>
            )}

            {expireError && (
              <span className="text-xs text-red-500">{expireError}</span>
            )}

            {(isFetching && !isLoading) && <Loader2 className="w-4 h-4 animate-spin text-gray-400" />}

            <span className="text-xs text-gray-400 ml-auto">
              {totalCount} batch{totalCount === 1 ? '' : 'es'} in {selectedStore?.name ?? 'store'}
            </span>
          </div>

          {/* Table */}
          <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
            {isError ? (
              <div className="flex items-center gap-2 px-6 py-12 text-red-500">
                <AlertCircle className="w-5 h-5 flex-shrink-0" />
                <span className="text-sm">Failed to load batches. Please refresh.</span>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                      {[
                        'Product', 'Lot / Batch #', 'Received', 'Expiry Date',
                        'Received Qty', 'Remaining', 'Unit Cost', 'Status', 'Actions',
                      ].map((h) => (
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
                      : batches.length === 0
                        ? (
                          <tr>
                            <td colSpan={9} className="px-4 py-14 text-center">
                              <Layers className="w-8 h-8 text-gray-300 dark:text-gray-600 mx-auto mb-2" />
                              <p className="text-sm text-gray-400">
                                {includeExpired
                                  ? `No batches recorded for ${selectedStore?.name ?? 'this store'} yet.`
                                  : 'No active batches. Toggle "Show expired" or receive a new batch.'}
                              </p>
                            </td>
                          </tr>
                        )
                        : batches.map((batch) => {
                            const status = getBatchStatus(batch);
                            const isActionable = status !== 'depleted' && !batch.isExpired;
                            return (
                              <tr
                                key={batch.id}
                                className={cn(
                                  'hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors',
                                  status === 'expired'  && 'bg-red-50/30 dark:bg-red-900/10',
                                  status === 'expiring' && 'bg-amber-50/30 dark:bg-amber-900/10',
                                  status === 'depleted' && 'opacity-60',
                                )}
                              >
                                {/* Product */}
                                <td className="px-4 py-3">
                                  <p className="font-medium text-gray-900 dark:text-white leading-tight">{batch.productName}</p>
                                  <p className="text-xs font-mono text-gray-400 mt-0.5">{batch.productSku}</p>
                                </td>

                                {/* Lot # */}
                                <td className="px-4 py-3 font-mono text-sm text-gray-600 dark:text-gray-300 whitespace-nowrap">
                                  {batch.lotNumber
                                    ? <span className="px-2 py-0.5 bg-gray-100 dark:bg-gray-800 rounded text-xs">{batch.lotNumber}</span>
                                    : <span className="text-gray-300 dark:text-gray-600">—</span>}
                                </td>

                                {/* Received date */}
                                <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap text-xs">
                                  {fmtDate(batch.createdAt)}
                                </td>

                                {/* Expiry date */}
                                <td className="px-4 py-3 whitespace-nowrap text-xs">
                                  {batch.expiryDate
                                    ? (
                                      <span className={cn(
                                        'font-medium',
                                        status === 'expired'  && 'text-red-600 dark:text-red-400',
                                        status === 'expiring' && 'text-amber-600 dark:text-amber-400',
                                        status === 'active'   && 'text-gray-600 dark:text-gray-300',
                                        status === 'depleted' && 'text-gray-400',
                                      )}>
                                        {fmtDate(batch.expiryDate)}
                                      </span>
                                    )
                                    : <span className="text-gray-300 dark:text-gray-600">—</span>}
                                </td>

                                {/* Received qty */}
                                <td className="px-4 py-3 text-left text-gray-600 dark:text-gray-300 whitespace-nowrap tabular-nums">
                                  {batch.receivedQuantity.toLocaleString(undefined, { maximumFractionDigits: 2 })}
                                </td>

                                {/* Remaining qty */}
                                <td className="px-4 py-3 text-left whitespace-nowrap tabular-nums">
                                  <span className={cn(
                                    'font-semibold',
                                    batch.remainingQuantity <= 0   && 'text-gray-400',
                                    batch.remainingQuantity > 0 && status === 'active'   && 'text-emerald-600 dark:text-emerald-400',
                                    batch.remainingQuantity > 0 && status === 'expiring' && 'text-amber-600 dark:text-amber-400',
                                    batch.remainingQuantity > 0 && status === 'expired'  && 'text-red-600 dark:text-red-400',
                                  )}>
                                    {batch.remainingQuantity.toLocaleString(undefined, { maximumFractionDigits: 2 })}
                                  </span>
                                </td>

                                {/* Unit cost */}
                                <td className="px-4 py-3 text-left text-gray-500 dark:text-gray-400 whitespace-nowrap tabular-nums text-xs">
                                  {batch.unitCost != null
                                    ? batch.unitCost.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
                                    : <span className="text-gray-300 dark:text-gray-600">—</span>}
                                </td>

                                {/* Status */}
                                <td className="px-4 py-3 whitespace-nowrap">
                                  <StatusBadge batch={batch} />
                                </td>

                                {/* Actions */}
                                <td className="px-4 py-3">
                                  <button
                                    title={isActionable
                                      ? 'Cannot delete — batch has remaining stock'
                                      : 'Delete batch'}
                                    disabled={isActionable || deleteMutation.isPending}
                                    onClick={() => { setDeleteTarget(batch); setDeleteError(null); }}
                                    className={cn(
                                      'p-1.5 rounded-lg transition-colors',
                                      isActionable
                                        ? 'text-gray-200 dark:text-gray-700 cursor-not-allowed'
                                        : 'text-gray-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20',
                                    )}
                                  >
                                    <Trash2 className="w-3.5 h-3.5" />
                                  </button>
                                </td>
                              </tr>
                            );
                          })
                    }
                  </tbody>
                </table>
              </div>
            )}

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="px-4 py-3 border-t border-gray-100 dark:border-gray-800">
                <Pagination
                  page={page}
                  totalPages={totalPages}
                  onPageChange={setPage}
                />
              </div>
            )}
          </div>
        </>
      )}

      {/* Receive batch modal */}
      <ReceiveBatchModal
        open={showReceive}
        storeId={storeId}
        onSave={handleReceive}
        onClose={() => { setShowReceive(false); setReceiveError(null); }}
        isSaving={receiveMutation.isPending}
        error={receiveError}
      />

      {/* Delete confirm dialog */}
      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Stock Batch"
        message={
          deleteTarget
            ? `Delete batch${deleteTarget.lotNumber ? ` "${deleteTarget.lotNumber}"` : ''} for ${deleteTarget.productName}? This action cannot be undone.`
            : ''
        }
        confirmLabel="Delete Batch"
        variant="danger"
        loading={deleteMutation.isPending}
        error={deleteError}
        onConfirm={handleDelete}
        onClose={() => { setDeleteTarget(null); setDeleteError(null); }}
      />
    </div>
  );
}
