import { useState, useMemo } from 'react';
import {
  Activity, Loader2, AlertCircle, Store as StoreIcon,
  RefreshCw, ChevronUp, ChevronDown, Minus, Filter,
} from 'lucide-react';
import { cn } from '../lib/utils';
import {
  MOVEMENT_TYPE_CONFIG, ALL_MOVEMENT_TYPES, type StockMovementType,
} from '../types/inventory';
import { useStockMovements } from '../hooks/useInventory';
import { useStores } from '../hooks/useWarehouses';
import { useStockLevels } from '../hooks/useInventory';

const LIMIT_OPTIONS = [50, 100, 200, 500];

function fmtDateTime(d: string): string {
  return new Date(d).toLocaleString('en-GB', {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
}

function fmtQty(n: number): string {
  return n.toLocaleString(undefined, { maximumFractionDigits: 2 });
}

function MovementTypeBadge({ type }: { type: string }) {
  const cfg = MOVEMENT_TYPE_CONFIG[type as StockMovementType];
  if (!cfg) {
    return (
      <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400">
        {type}
      </span>
    );
  }
  return (
    <span className={cn('inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium whitespace-nowrap', cfg.color)}>
      <span className={cn('w-1.5 h-1.5 rounded-full flex-shrink-0', cfg.dot)} />
      {cfg.label}
    </span>
  );
}

function DeltaCell({ before, after }: { before: number; after: number }) {
  const delta = after - before;
  if (delta > 0) {
    return (
      <span className="inline-flex items-center gap-0.5 text-emerald-600 dark:text-emerald-400 font-semibold tabular-nums">
        <ChevronUp className="w-3.5 h-3.5" />+{fmtQty(delta)}
      </span>
    );
  }
  if (delta < 0) {
    return (
      <span className="inline-flex items-center gap-0.5 text-red-600 dark:text-red-400 font-semibold tabular-nums">
        <ChevronDown className="w-3.5 h-3.5" />{fmtQty(delta)}
      </span>
    );
  }
  return (
    <span className="inline-flex items-center gap-0.5 text-gray-400 tabular-nums">
      <Minus className="w-3 h-3" />0
    </span>
  );
}

export default function StockMovementsPage() {
  const [storeId,      setStoreId]      = useState('');
  const [productId,    setProductId]    = useState('');
  const [typeFilter,   setTypeFilter]   = useState<StockMovementType | ''>('');
  const [from,         setFrom]         = useState('');
  const [to,           setTo]           = useState('');
  const [limit,        setLimit]        = useState(100);

  const { data: stores = [] } = useStores();
  const { data: stock = [] }  = useStockLevels(storeId || null);

  const { data: movements = [], isLoading, isError, refetch, isFetching } = useStockMovements(
    storeId || null,
    {
      productId: productId || undefined,
      from:      from || undefined,
      to:        to || undefined,
      limit,
    },
  );

  const filteredMovements = useMemo(() => {
    if (!typeFilter) return movements;
    return movements.filter((m) => m.type === typeFilter);
  }, [movements, typeFilter]);

  const typeCounts = useMemo(() => {
    const counts: Record<string, number> = {};
    for (const m of movements) counts[m.type] = (counts[m.type] ?? 0) + 1;
    return counts;
  }, [movements]);

  const selectCls = 'text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg px-3 py-2 text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 cursor-pointer transition';
  const inputCls  = 'text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg px-3 py-2 text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 transition';

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <Activity className="w-6 h-6 text-primary-700" />
            Stock Movements
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Full audit trail of all inventory changes
          </p>
        </div>
        <div className="flex items-center gap-2">
          <StoreIcon className="w-4 h-4 text-gray-400 flex-shrink-0" />
          <select value={storeId} onChange={(e) => { setStoreId(e.target.value); setProductId(''); }} className={selectCls}>
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

      {!storeId ? (
        <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm flex flex-col items-center justify-center py-20">
          <StoreIcon className="w-10 h-10 text-gray-300 dark:text-gray-600 mb-3" />
          <p className="text-sm font-medium text-gray-500 dark:text-gray-400">Select a store to view movement history</p>
          <p className="text-xs text-gray-400 mt-1">Choose a store from the dropdown above</p>
        </div>
      ) : (
        <>
          {/* Filters */}
          <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm p-4 space-y-3">
            <div className="flex items-center gap-2 text-xs font-semibold text-gray-400 uppercase tracking-wider">
              <Filter className="w-3.5 h-3.5" />
              Filters
            </div>
            <div className="flex flex-wrap gap-3">
              {/* Date range */}
              <div className="flex items-center gap-2">
                <label className="text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">From</label>
                <input type="date" value={from} onChange={(e) => setFrom(e.target.value)} className={inputCls} />
              </div>
              <div className="flex items-center gap-2">
                <label className="text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">To</label>
                <input type="date" value={to} onChange={(e) => setTo(e.target.value)} className={inputCls} />
              </div>

              {/* Product filter */}
              <select value={productId} onChange={(e) => setProductId(e.target.value)} className={selectCls}>
                <option value="">All Products</option>
                {stock.map((s) => (
                  <option key={s.productId} value={s.productId}>{s.productName} — {s.sku}</option>
                ))}
              </select>

              {/* Limit */}
              <div className="flex items-center gap-2">
                <label className="text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">Show</label>
                <select value={limit} onChange={(e) => setLimit(Number(e.target.value))} className={selectCls}>
                  {LIMIT_OPTIONS.map((l) => <option key={l} value={l}>{l} records</option>)}
                </select>
              </div>

              {(isFetching && !isLoading) && <Loader2 className="w-4 h-4 animate-spin text-gray-400 self-center" />}
            </div>

            {/* Type filter pills */}
            <div className="flex flex-wrap gap-1.5">
              <button
                onClick={() => setTypeFilter('')}
                className={cn('px-3 py-1 text-xs font-medium rounded-full transition-colors',
                  !typeFilter ? 'bg-gray-800 dark:bg-gray-200 text-white dark:text-gray-900'
                              : 'bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700')}>
                All ({movements.length})
              </button>
              {ALL_MOVEMENT_TYPES.filter((t) => typeCounts[t]).map((t) => {
                const cfg = MOVEMENT_TYPE_CONFIG[t];
                return (
                  <button key={t} onClick={() => setTypeFilter(typeFilter === t ? '' : t)}
                    className={cn('px-3 py-1 text-xs font-medium rounded-full transition-colors',
                      typeFilter === t ? cn(cfg.color, 'ring-2 ring-offset-1 ring-current') : cn(cfg.color, 'opacity-70 hover:opacity-100'))}>
                    {cfg.label} ({typeCounts[t]})
                  </button>
                );
              })}
            </div>
          </div>

          {/* Table */}
          <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
            {isError ? (
              <div className="flex items-center gap-2 px-6 py-12 text-red-500">
                <AlertCircle className="w-5 h-5 flex-shrink-0" />
                <span className="text-sm">Failed to load movements. Please refresh.</span>
              </div>
            ) : (
              <>
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                        {['Date & Time', 'Type', 'Product', 'Change', 'Before → After', 'Reference', 'Related Store', 'Recorded By'].map((h) => (
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
                              {Array.from({ length: 8 }).map((_, j) => (
                                <td key={j} className="px-4 py-3">
                                  <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-full" />
                                </td>
                              ))}
                            </tr>
                          ))
                        : filteredMovements.length === 0
                          ? (
                            <tr>
                              <td colSpan={8} className="px-4 py-14 text-center">
                                <Activity className="w-8 h-8 text-gray-300 dark:text-gray-600 mx-auto mb-2" />
                                <p className="text-sm text-gray-400">
                                  {movements.length === 0 ? 'No stock movements recorded yet.' : 'No movements match the selected filters.'}
                                </p>
                              </td>
                            </tr>
                          )
                          : filteredMovements.map((m) => (
                            <tr key={m.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                              <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">
                                {fmtDateTime(m.createdAt)}
                              </td>
                              <td className="px-4 py-3 whitespace-nowrap">
                                <MovementTypeBadge type={m.type} />
                              </td>
                              <td className="px-4 py-3">
                                <p className="font-medium text-gray-900 dark:text-white leading-tight">{m.productName}</p>
                                <p className="text-xs font-mono text-gray-400 mt-0.5">{m.productSku}</p>
                              </td>
                              <td className="px-4 py-3 whitespace-nowrap">
                                <DeltaCell before={m.quantityBefore} after={m.quantityAfter} />
                              </td>
                              <td className="px-4 py-3 text-gray-500 dark:text-gray-400 tabular-nums whitespace-nowrap text-xs">
                                {fmtQty(m.quantityBefore)}
                                <span className="mx-1.5 text-gray-300 dark:text-gray-600">→</span>
                                {fmtQty(m.quantityAfter)}
                              </td>
                              <td className="px-4 py-3 max-w-[160px]">
                                {m.reference && (
                                  <p className="text-xs font-mono text-gray-600 dark:text-gray-400 truncate">{m.reference}</p>
                                )}
                                {m.notes && (
                                  <p className="text-xs text-gray-400 truncate" title={m.notes}>{m.notes}</p>
                                )}
                                {!m.reference && !m.notes && <span className="text-gray-300 dark:text-gray-600">—</span>}
                              </td>
                              <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">
                                {m.relatedStoreName ?? '—'}
                              </td>
                              <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">
                                {m.recordedByName}
                              </td>
                            </tr>
                          ))
                      }
                    </tbody>
                  </table>
                </div>

                {/* Footer */}
                {!isLoading && (
                  <div className="px-6 py-3 border-t border-gray-100 dark:border-gray-800 text-xs text-gray-400 flex items-center justify-between">
                    <span>
                      {filteredMovements.length === movements.length
                        ? `${movements.length} movement${movements.length !== 1 ? 's' : ''}`
                        : `${filteredMovements.length} of ${movements.length} movements`}
                    </span>
                    {movements.length >= limit && (
                      <span className="text-amber-500 dark:text-amber-400">
                        Results limited to {limit} — increase the limit or narrow the date range to see more.
                      </span>
                    )}
                  </div>
                )}
              </>
            )}
          </div>
        </>
      )}
    </div>
  );
}
