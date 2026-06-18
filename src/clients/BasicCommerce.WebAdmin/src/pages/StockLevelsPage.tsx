import { useState, useMemo } from 'react';
import {
  Package, Search, AlertTriangle, Plus, SlidersHorizontal,
  Trash2, Flag, ArrowRightLeft, Loader2, AlertCircle,
  RefreshCw, Store as StoreIcon, TrendingDown, BarChart3,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { StockLevel, StockActionType } from '../types/inventory';
import { useStockLevels, useReceiveStock, useAdjustStock, useWriteOffStock, useTransferStock, useSetLowStockThreshold } from '../hooks/useInventory';
import { useStores } from '../hooks/useWarehouses';
import { StockActionModal, type ActionPayload } from '../components/inventory/StockActionModal';
import { TransferStockModal } from '../components/inventory/TransferStockModal';

type StockFilter = 'All' | 'LowStock' | 'OutOfStock' | 'InStock';

function fmtDate(d: string | null | undefined): string {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

function QtyCell({ value, className }: { value: number; className?: string }) {
  return (
    <span className={cn('tabular-nums', className)}>
      {value.toLocaleString(undefined, { maximumFractionDigits: 2 })}
    </span>
  );
}

function StatusBadge({ item }: { item: StockLevel }) {
  if (item.isOutOfStock) {
    return (
      <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-red-50 dark:bg-red-900/30 text-red-700 dark:text-red-400">
        <span className="w-1.5 h-1.5 rounded-full bg-red-500" />
        Out of Stock
      </span>
    );
  }
  if (item.isLowStock) {
    return (
      <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-amber-50 dark:bg-amber-900/30 text-amber-700 dark:text-amber-400">
        <AlertTriangle className="w-3 h-3" />
        Low Stock
      </span>
    );
  }
  return (
    <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-emerald-50 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400">
      <span className="w-1.5 h-1.5 rounded-full bg-emerald-500" />
      In Stock
    </span>
  );
}

type ActiveModal =
  | { type: StockActionType; item: StockLevel }
  | { type: 'transfer'; item: StockLevel }
  | null;

export default function StockLevelsPage() {
  const [storeId, setStoreId]       = useState('');
  const [search, setSearch]         = useState('');
  const [stockFilter, setStockFilter] = useState<StockFilter>('All');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [activeModal, setActiveModal] = useState<ActiveModal>(null);
  const [modalError, setModalError]   = useState<string | null>(null);

  const { data: stores = [] }                             = useStores();
  const { data: stock = [], isLoading, isError, refetch, isFetching } = useStockLevels(storeId || null);

  const receiveMutation  = useReceiveStock();
  const adjustMutation   = useAdjustStock();
  const writeOffMutation = useWriteOffStock();
  const transferMutation = useTransferStock();
  const thresholdMutation = useSetLowStockThreshold();

  const isMutating = receiveMutation.isPending || adjustMutation.isPending ||
    writeOffMutation.isPending || transferMutation.isPending || thresholdMutation.isPending;

  const categories = useMemo(() => {
    const s = new Set(stock.map((i) => i.categoryName).filter(Boolean));
    return [...s].sort();
  }, [stock]);

  const filtered = useMemo(() => {
    let list = stock;
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter((i) =>
        i.productName.toLowerCase().includes(q) ||
        i.sku.toLowerCase().includes(q) ||
        i.barcode?.toLowerCase().includes(q),
      );
    }
    if (categoryFilter) list = list.filter((i) => i.categoryName === categoryFilter);
    if (stockFilter === 'LowStock')   list = list.filter((i) => i.isLowStock && !i.isOutOfStock);
    if (stockFilter === 'OutOfStock') list = list.filter((i) => i.isOutOfStock);
    if (stockFilter === 'InStock')    list = list.filter((i) => !i.isLowStock && !i.isOutOfStock);
    return list;
  }, [stock, search, categoryFilter, stockFilter]);

  const stats = useMemo(() => ({
    total:      stock.length,
    lowStock:   stock.filter((i) => i.isLowStock && !i.isOutOfStock).length,
    outOfStock: stock.filter((i) => i.isOutOfStock).length,
  }), [stock]);

  const selectedStore = stores.find((s) => s.id === storeId);

  async function handleAction(payload: ActionPayload) {
    if (!storeId || !activeModal) return;
    const productId = activeModal.item.productId;
    try {
      setModalError(null);
      if (payload.type === 'receive')   await receiveMutation.mutateAsync({ storeId, productId, quantity: payload.quantity, reference: payload.reference, notes: payload.notes });
      if (payload.type === 'adjust')    await adjustMutation.mutateAsync({ storeId, productId, newQuantity: payload.newQuantity, notes: payload.notes });
      if (payload.type === 'writeOff')  await writeOffMutation.mutateAsync({ storeId, productId, quantity: payload.quantity, reason: payload.reason });
      if (payload.type === 'threshold') await thresholdMutation.mutateAsync({ storeId, productId, threshold: payload.threshold });
      setActiveModal(null);
    } catch (err) {
      setModalError(extractApiError(err));
    }
  }

  async function handleTransfer(destStoreId: string, quantity: number, notes?: string) {
    if (!storeId || !activeModal) return;
    try {
      setModalError(null);
      await transferMutation.mutateAsync({
        sourceStoreId: storeId, destinationStoreId: destStoreId,
        productId: activeModal.item.productId, quantity, notes,
      });
      setActiveModal(null);
    } catch (err) {
      setModalError(extractApiError(err));
    }
  }

  const FILTER_OPTS: { value: StockFilter; label: string }[] = [
    { value: 'All', label: 'All' },
    { value: 'InStock', label: 'In Stock' },
    { value: 'LowStock', label: 'Low Stock' },
    { value: 'OutOfStock', label: 'Out of Stock' },
  ];

  const selectCls = 'text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg px-3 py-2 text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 cursor-pointer transition';

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <Package className="w-6 h-6 text-primary-700" />
            Stock Levels
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            View and manage inventory at each store
          </p>
        </div>

        {/* Store selector */}
        <div className="flex items-center gap-2">
          <StoreIcon className="w-4 h-4 text-gray-400 flex-shrink-0" />
          <select value={storeId} onChange={(e) => setStoreId(e.target.value)} className={selectCls}>
            <option value="">Select store…</option>
            {stores.filter((s) => s.status === 'Active').map((s) => (
              <option key={s.id} value={s.id}>{s.name}</option>
            ))}
          </select>
          {storeId && (
            <button onClick={() => refetch()} disabled={isFetching}
              className="p-2 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors disabled:opacity-40" title="Refresh">
              <RefreshCw className={cn('w-4 h-4', isFetching && 'animate-spin')} />
            </button>
          )}
        </div>
      </div>

      {/* No store selected */}
      {!storeId && (
        <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm flex flex-col items-center justify-center py-20">
          <StoreIcon className="w-10 h-10 text-gray-300 dark:text-gray-600 mb-3" />
          <p className="text-sm font-medium text-gray-500 dark:text-gray-400">Select a store to view stock levels</p>
          <p className="text-xs text-gray-400 mt-1">Choose a store from the dropdown above</p>
        </div>
      )}

      {storeId && (
        <>
          {/* Stats */}
          <div className="grid grid-cols-3 gap-4">
            <button onClick={() => setStockFilter('All')}
              className={cn('rounded-xl border p-4 text-left transition-colors cursor-pointer',
                stockFilter === 'All'
                  ? 'bg-primary-50 dark:bg-primary-900/20 border-primary-200 dark:border-primary-800'
                  : 'bg-white dark:bg-gray-900 border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800')}>
              <div className="flex items-center gap-2 mb-1">
                <BarChart3 className="w-4 h-4 text-gray-400" />
                <span className="text-xs text-gray-500 dark:text-gray-400 font-medium">Total SKUs</span>
              </div>
              <p className="text-2xl font-bold text-gray-900 dark:text-white">{stats.total}</p>
            </button>
            <button onClick={() => setStockFilter(stockFilter === 'LowStock' ? 'All' : 'LowStock')}
              className={cn('rounded-xl border p-4 text-left transition-colors cursor-pointer',
                stockFilter === 'LowStock'
                  ? 'bg-amber-50 dark:bg-amber-900/20 border-amber-200 dark:border-amber-800'
                  : 'bg-white dark:bg-gray-900 border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800')}>
              <div className="flex items-center gap-2 mb-1">
                <AlertTriangle className="w-4 h-4 text-amber-500" />
                <span className="text-xs text-amber-600 dark:text-amber-400 font-medium">Low Stock</span>
              </div>
              <p className="text-2xl font-bold text-amber-700 dark:text-amber-400">{stats.lowStock}</p>
            </button>
            <button onClick={() => setStockFilter(stockFilter === 'OutOfStock' ? 'All' : 'OutOfStock')}
              className={cn('rounded-xl border p-4 text-left transition-colors cursor-pointer',
                stockFilter === 'OutOfStock'
                  ? 'bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800'
                  : 'bg-white dark:bg-gray-900 border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800')}>
              <div className="flex items-center gap-2 mb-1">
                <TrendingDown className="w-4 h-4 text-red-500" />
                <span className="text-xs text-red-600 dark:text-red-400 font-medium">Out of Stock</span>
              </div>
              <p className="text-2xl font-bold text-red-700 dark:text-red-400">{stats.outOfStock}</p>
            </button>
          </div>

          {/* Filters */}
          <div className="flex flex-wrap items-center gap-3">
            {/* Search */}
            <div className="relative flex-1 min-w-[200px] max-w-xs">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-gray-400" />
              <input type="text" value={search} onChange={(e) => setSearch(e.target.value)}
                placeholder="Search by product or SKU…"
                className="w-full pl-8 pr-3 py-2 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-gray-900 dark:text-white placeholder-gray-400" />
            </div>

            {/* Category */}
            {categories.length > 0 && (
              <select value={categoryFilter} onChange={(e) => setCategoryFilter(e.target.value)} className={selectCls}>
                <option value="">All Categories</option>
                {categories.map((c) => <option key={c} value={c}>{c}</option>)}
              </select>
            )}

            {/* Status filter pills */}
            <div className="flex items-center gap-1 bg-gray-100 dark:bg-gray-800 rounded-lg p-1 flex-wrap">
              {FILTER_OPTS.map((opt) => (
                <button key={opt.value} onClick={() => setStockFilter(opt.value)}
                  className={cn('px-3 py-1 text-xs font-medium rounded-md transition-colors whitespace-nowrap',
                    stockFilter === opt.value
                      ? 'bg-white dark:bg-gray-700 text-gray-900 dark:text-white shadow-sm'
                      : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300')}>
                  {opt.label}
                </button>
              ))}
            </div>

            {(isFetching && !isLoading) && <Loader2 className="w-4 h-4 animate-spin text-gray-400" />}

            <span className="text-xs text-gray-400 ml-auto">
              {filtered.length} of {stock.length} products
            </span>
          </div>

          {/* Table */}
          <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
            {isError ? (
              <div className="flex items-center gap-2 px-6 py-12 text-red-500">
                <AlertCircle className="w-5 h-5 flex-shrink-0" />
                <span className="text-sm">Failed to load stock levels. Please refresh.</span>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                      {['Product', 'Category', 'Total', 'Reserved', 'Available', 'Threshold', 'Status', 'Last Counted', 'Actions'].map((h) => (
                        <th key={h} className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider whitespace-nowrap">
                          {h}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                    {isLoading
                      ? Array.from({ length: 6 }).map((_, i) => (
                          <tr key={i} className="animate-pulse">
                            {Array.from({ length: 9 }).map((_, j) => (
                              <td key={j} className="px-4 py-3">
                                <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-full" />
                              </td>
                            ))}
                          </tr>
                        ))
                      : filtered.length === 0
                        ? (
                          <tr>
                            <td colSpan={9} className="px-4 py-14 text-center">
                              <Package className="w-8 h-8 text-gray-300 dark:text-gray-600 mx-auto mb-2" />
                              <p className="text-sm text-gray-400">
                                {stock.length === 0
                                  ? `No stock has been recorded for ${selectedStore?.name ?? 'this store'} yet.`
                                  : 'No products match your filters.'}
                              </p>
                            </td>
                          </tr>
                        )
                        : filtered.map((item) => (
                          <tr key={item.productId} className={cn(
                            'hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors',
                            item.isOutOfStock && 'bg-red-50/30 dark:bg-red-900/10',
                            !item.isOutOfStock && item.isLowStock && 'bg-amber-50/30 dark:bg-amber-900/10',
                          )}>
                            <td className="px-4 py-3">
                              <p className="font-medium text-gray-900 dark:text-white leading-tight">{item.productName}</p>
                              <p className="text-xs font-mono text-gray-400 mt-0.5">{item.sku}</p>
                            </td>
                            <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">{item.categoryName || '—'}</td>
                            <td className="px-4 py-3 text-right text-gray-700 dark:text-gray-300 whitespace-nowrap">
                              <QtyCell value={item.quantity} />
                            </td>
                            <td className="px-4 py-3 text-right text-gray-400 whitespace-nowrap">
                              <QtyCell value={item.reservedQuantity} />
                            </td>
                            <td className="px-4 py-3 text-right whitespace-nowrap">
                              <QtyCell value={item.availableQuantity}
                                className={item.isOutOfStock ? 'text-red-600 dark:text-red-400 font-semibold' : 'text-emerald-700 dark:text-emerald-300 font-semibold'} />
                            </td>
                            <td className="px-4 py-3 text-right text-gray-400 whitespace-nowrap">
                              <QtyCell value={item.lowStockThreshold} />
                            </td>
                            <td className="px-4 py-3 whitespace-nowrap">
                              <StatusBadge item={item} />
                            </td>
                            <td className="px-4 py-3 text-gray-400 text-xs whitespace-nowrap">
                              {fmtDate(item.lastCountedAt)}
                            </td>
                            <td className="px-4 py-3">
                              <div className="flex items-center gap-0.5">
                                <ActionBtn title="Receive Stock" icon={<Plus className="w-3.5 h-3.5" />}
                                  color="text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/20"
                                  onClick={() => { setModalError(null); setActiveModal({ type: 'receive', item }); }} />
                                <ActionBtn title="Adjust Count" icon={<SlidersHorizontal className="w-3.5 h-3.5" />}
                                  color="text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-900/20"
                                  onClick={() => { setModalError(null); setActiveModal({ type: 'adjust', item }); }} />
                                <ActionBtn title="Write Off" icon={<Trash2 className="w-3.5 h-3.5" />}
                                  color="text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20"
                                  disabled={item.quantity <= 0}
                                  onClick={() => { setModalError(null); setActiveModal({ type: 'writeOff', item }); }} />
                                <ActionBtn title="Transfer to Store" icon={<ArrowRightLeft className="w-3.5 h-3.5" />}
                                  color="text-orange-600 hover:bg-orange-50 dark:hover:bg-orange-900/20"
                                  disabled={item.availableQuantity <= 0}
                                  onClick={() => { setModalError(null); setActiveModal({ type: 'transfer', item }); }} />
                                <ActionBtn title="Set Threshold" icon={<Flag className="w-3.5 h-3.5" />}
                                  color="text-amber-500 hover:bg-amber-50 dark:hover:bg-amber-900/20"
                                  onClick={() => { setModalError(null); setActiveModal({ type: 'threshold', item }); }} />
                              </div>
                            </td>
                          </tr>
                        ))
                    }
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </>
      )}

      {/* Action modals */}
      {activeModal && activeModal.type !== 'transfer' && (
        <StockActionModal
          open
          type={activeModal.type}
          storeId={storeId}
          item={activeModal.item}
          isSaving={isMutating}
          error={modalError}
          onSave={handleAction}
          onClose={() => { setActiveModal(null); setModalError(null); }}
        />
      )}

      {activeModal?.type === 'transfer' && (
        <TransferStockModal
          open
          sourceStoreId={storeId}
          item={activeModal.item}
          stores={stores}
          isSaving={transferMutation.isPending}
          error={modalError}
          onSave={handleTransfer}
          onClose={() => { setActiveModal(null); setModalError(null); }}
        />
      )}
    </div>
  );
}

function ActionBtn({
  title, icon, color, onClick, disabled,
}: {
  title: string;
  icon: React.ReactNode;
  color: string;
  onClick: () => void;
  disabled?: boolean;
}) {
  return (
    <button title={title} onClick={onClick} disabled={disabled}
      className={cn('p-1.5 rounded-lg text-gray-400 transition-colors disabled:opacity-30 disabled:cursor-not-allowed', color)}>
      {icon}
    </button>
  );
}
