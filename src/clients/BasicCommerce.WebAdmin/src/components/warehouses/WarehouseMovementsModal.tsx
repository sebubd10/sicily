import { useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { Activity, X, Loader2 } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { Warehouse } from '../../types/warehouse';
import { useWarehouseMovements } from '../../hooks/useWarehouses';

type Props = {
  open: boolean;
  warehouse: Warehouse | null;
  onClose: () => void;
};

const MOVEMENT_COLORS: Record<string, string> = {
  PurchaseOrderReceipt: 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300',
  TransferToStore:      'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300',
  Adjustment:           'bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300',
  WriteOff:             'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400',
  Return:               'bg-violet-100 dark:bg-violet-900/30 text-violet-700 dark:text-violet-300',
};

const MOVEMENT_LABELS: Record<string, string> = {
  PurchaseOrderReceipt: 'PO Receipt',
  TransferToStore:      'Transfer Out',
  Adjustment:           'Adjustment',
  WriteOff:             'Write-off',
  Return:               'Return',
};

export function WarehouseMovementsModal({ open, warehouse, onClose }: Props) {
  const [from, setFrom] = useState('');
  const [to, setTo]     = useState('');

  const params = {
    from: from || undefined,
    to: to || undefined,
    limit: 200,
  };

  const { data: movements = [], isLoading, isError, refetch } = useWarehouseMovements(
    open && warehouse ? warehouse.id : null,
    params,
  );

  function handleDateChange(key: 'from' | 'to', val: string) {
    if (key === 'from') setFrom(val);
    else setTo(val);
  }

  return (
    <Dialog.Root open={open} onOpenChange={(o) => !o && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <Dialog.Content
          className={cn(
            'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
            'w-full max-w-4xl rounded-xl bg-white dark:bg-gray-900',
            'border border-gray-200 dark:border-gray-700 shadow-xl focus:outline-none flex flex-col',
            'max-h-[85vh]',
            'data-[state=open]:animate-in data-[state=closed]:animate-out',
            'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
            'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
            'data-[state=closed]:slide-out-to-left-1/2 data-[state=closed]:slide-out-to-top-48%',
            'data-[state=open]:slide-in-from-left-1/2 data-[state=open]:slide-in-from-top-48%',
          )}
        >
          {/* Header */}
          <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 px-6 py-4 flex-shrink-0">
            <Dialog.Title className="flex items-center gap-2 text-base font-semibold text-gray-900 dark:text-white">
              <Activity className="w-5 h-5 text-primary-700" />
              Stock Movements
              {warehouse && (
                <span className="text-sm font-normal text-gray-500 dark:text-gray-400">
                  — {warehouse.name}
                </span>
              )}
            </Dialog.Title>
            <Dialog.Close className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          {/* Date filter toolbar */}
          <div className="px-6 py-3 border-b border-gray-100 dark:border-gray-800 flex items-center gap-3 flex-shrink-0 flex-wrap">
            <div className="flex items-center gap-2">
              <label className="text-xs font-medium text-gray-500 dark:text-gray-400">From</label>
              <input
                type="date"
                value={from}
                onChange={(e) => handleDateChange('from', e.target.value)}
                className="text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg px-2 py-1 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
              />
            </div>
            <div className="flex items-center gap-2">
              <label className="text-xs font-medium text-gray-500 dark:text-gray-400">To</label>
              <input
                type="date"
                value={to}
                onChange={(e) => handleDateChange('to', e.target.value)}
                className="text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg px-2 py-1 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
              />
            </div>
            {(from || to) && (
              <button
                onClick={() => { setFrom(''); setTo(''); }}
                className="text-xs text-gray-500 hover:text-gray-700 dark:hover:text-gray-200 underline"
              >
                Clear
              </button>
            )}
            <span className="ml-auto text-xs text-gray-400">
              Showing latest {movements.length} records (max 200)
            </span>
          </div>

          {/* Table */}
          <div className="flex-1 overflow-y-auto min-h-0">
            {isLoading ? (
              <div className="flex items-center justify-center py-16">
                <Loader2 className="w-6 h-6 animate-spin text-gray-400" />
              </div>
            ) : isError ? (
              <div className="px-6 py-12 text-center text-sm text-red-500">Failed to load movements.</div>
            ) : movements.length === 0 ? (
              <div className="px-6 py-12 text-center text-sm text-gray-400">No movements found for this period.</div>
            ) : (
              <table className="w-full text-sm">
                <thead className="sticky top-0 bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                  <tr>
                    {['Date', 'Type', 'Product', 'SKU', 'Qty', 'Before', 'After', 'Reference', 'Notes'].map((h) => (
                      <th key={h} className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider whitespace-nowrap">{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                  {movements.map((m) => {
                    const isOut = m.movementType === 'TransferToStore' || m.movementType === 'WriteOff';
                    return (
                      <tr key={m.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                        <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">
                          {new Date(m.createdAt).toLocaleDateString()}<br />
                          <span className="text-gray-400">{new Date(m.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
                        </td>
                        <td className="px-4 py-3">
                          <span className={cn(
                            'text-xs font-medium px-2 py-0.5 rounded-full',
                            MOVEMENT_COLORS[m.movementType] ?? 'bg-gray-100 text-gray-600',
                          )}>
                            {MOVEMENT_LABELS[m.movementType] ?? m.movementType}
                          </span>
                        </td>
                        <td className="px-4 py-3 font-medium text-gray-900 dark:text-white max-w-[160px] truncate">{m.productName}</td>
                        <td className="px-4 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">{m.productSku}</td>
                        <td className={cn('px-4 py-3 font-medium text-right', isOut ? 'text-red-600 dark:text-red-400' : 'text-emerald-700 dark:text-emerald-300')}>
                          {isOut ? '−' : '+'}{Math.abs(m.quantity)}
                        </td>
                        <td className="px-4 py-3 text-right text-gray-500 dark:text-gray-400">{m.quantityBefore}</td>
                        <td className="px-4 py-3 text-right text-gray-700 dark:text-gray-300 font-medium">{m.quantityAfter}</td>
                        <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400 max-w-[100px] truncate">
                          {m.reference ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                        </td>
                        <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400 max-w-[120px] truncate">
                          {m.notes ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            )}
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
