import { useState, useEffect } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { X, ArrowRightLeft, Loader2, AlertCircle, Package } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { StockLevel } from '../../types/inventory';
import type { Store } from '../../types/warehouse';

type Props = {
  open: boolean;
  sourceStoreId: string;
  item: StockLevel | null;
  stores: Store[];
  isSaving: boolean;
  error: string | null;
  onSave: (destinationStoreId: string, quantity: number, notes?: string) => void;
  onClose: () => void;
};

const inputCls = (err?: string) =>
  cn(
    'w-full rounded-lg border px-3 py-2 text-sm bg-white dark:bg-gray-900',
    'text-gray-900 dark:text-white placeholder-gray-400',
    'focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 transition',
    err ? 'border-red-400 dark:border-red-500' : 'border-gray-200 dark:border-gray-700',
  );

export function TransferStockModal({ open, sourceStoreId, item, stores, isSaving, error, onSave, onClose }: Props) {
  const [destStoreId, setDestStoreId] = useState('');
  const [quantity,    setQuantity]    = useState('');
  const [notes,       setNotes]       = useState('');
  const [errs,        setErrs]        = useState<Record<string, string>>({});

  const otherStores = stores.filter((s) => s.id !== sourceStoreId && s.status === 'Active');

  useEffect(() => {
    if (open) {
      setDestStoreId(''); setQuantity(''); setNotes(''); setErrs({});
    }
  }, [open, item]);

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (!destStoreId) e.dest = 'Select a destination store';
    const q = parseFloat(quantity);
    if (!quantity || isNaN(q) || q <= 0) e.quantity = 'Enter quantity > 0';
    if (item && q > item.availableQuantity) e.quantity = `Cannot exceed available quantity (${item.availableQuantity})`;
    setErrs(e);
    return Object.keys(e).length === 0;
  }

  function handleSubmit(ev: React.FormEvent) {
    ev.preventDefault();
    if (!validate()) return;
    onSave(destStoreId, parseFloat(quantity), notes || undefined);
  }

  return (
    <Dialog.Root open={open} onOpenChange={(o) => !o && !isSaving && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <Dialog.Content className={cn(
          'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
          'w-full max-w-md rounded-xl bg-white dark:bg-gray-900',
          'border border-gray-200 dark:border-gray-700 shadow-xl focus:outline-none',
          'data-[state=open]:animate-in data-[state=closed]:animate-out',
          'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
          'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
        )}>
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200 dark:border-gray-700">
            <Dialog.Title className="flex items-center gap-3 text-base font-semibold text-gray-900 dark:text-white">
              <div className="flex items-center justify-center w-9 h-9 rounded-full bg-orange-100 dark:bg-orange-900/30 flex-shrink-0">
                <ArrowRightLeft className="w-5 h-5 text-orange-600" />
              </div>
              Transfer Stock
            </Dialog.Title>
            <Dialog.Close disabled={isSaving} className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none disabled:opacity-40">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          <form onSubmit={handleSubmit} noValidate>
            <div className="px-6 py-5 space-y-4">
              <Dialog.Description className="sr-only">Transfer stock between stores</Dialog.Description>

              {/* Product summary */}
              {item && (
                <div className="flex items-center gap-3 rounded-lg bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 px-4 py-3">
                  <Package className="w-4 h-4 text-gray-400 flex-shrink-0" />
                  <div className="min-w-0">
                    <p className="text-sm font-medium text-gray-900 dark:text-white truncate">{item.productName}</p>
                    <p className="text-xs text-gray-400 font-mono">{item.sku}</p>
                  </div>
                  <div className="ml-auto text-right flex-shrink-0">
                    <p className="text-xs text-gray-400">Available</p>
                    <p className="text-sm font-bold text-emerald-700 dark:text-emerald-400">{item.availableQuantity.toLocaleString()}</p>
                  </div>
                </div>
              )}

              <div>
                <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">
                  Destination Store <span className="text-red-500">*</span>
                </label>
                <select value={destStoreId}
                  onChange={(e) => { setDestStoreId(e.target.value); setErrs((x) => ({ ...x, dest: '' })); }}
                  className={inputCls(errs.dest)}>
                  <option value="">Select store…</option>
                  {otherStores.map((s) => (
                    <option key={s.id} value={s.id}>{s.name}</option>
                  ))}
                </select>
                {errs.dest && <p className="mt-1 text-xs text-red-500">{errs.dest}</p>}
                {otherStores.length === 0 && (
                  <p className="mt-1 text-xs text-amber-600 dark:text-amber-400">No other active stores available.</p>
                )}
              </div>

              <div>
                <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">
                  Quantity <span className="text-red-500">*</span>
                </label>
                <input type="number" min={0.01} step="0.01" value={quantity}
                  onChange={(e) => { setQuantity(e.target.value); setErrs((x) => ({ ...x, quantity: '' })); }}
                  placeholder="0.00" className={inputCls(errs.quantity)} />
                {errs.quantity && <p className="mt-1 text-xs text-red-500">{errs.quantity}</p>}
              </div>

              <div>
                <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Notes (optional)</label>
                <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2}
                  placeholder="Reason for transfer…" className={cn(inputCls(), 'resize-none')} />
              </div>

              {error && (
                <div className="flex items-center gap-2 rounded-lg border border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-900/20 px-3 py-2.5">
                  <AlertCircle className="w-4 h-4 text-red-600 dark:text-red-400 flex-shrink-0" />
                  <p className="text-xs text-red-700 dark:text-red-300">{error}</p>
                </div>
              )}
            </div>

            <div className="flex justify-end gap-3 px-6 py-4 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50 rounded-b-xl">
              <button type="button" onClick={onClose} disabled={isSaving}
                className="px-4 py-2 text-sm font-medium rounded-lg border border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-50">
                Cancel
              </button>
              <button type="submit" disabled={isSaving || otherStores.length === 0}
                className="px-4 py-2 text-sm font-medium rounded-lg text-white bg-orange-600 hover:bg-orange-700 transition-colors disabled:opacity-60 flex items-center gap-2">
                {isSaving && <Loader2 className="w-3.5 h-3.5 animate-spin" />}
                {isSaving ? 'Transferring…' : 'Transfer'}
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
