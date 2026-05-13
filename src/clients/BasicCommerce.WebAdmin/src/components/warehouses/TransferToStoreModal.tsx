import { useEffect, useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { ArrowRightLeft, X } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { WarehouseStockLevel, Store } from '../../types/warehouse';

type Props = {
  open: boolean;
  warehouseId: string;
  stockItem: WarehouseStockLevel | null;
  stores: Store[];
  onSave: (storeId: string, quantity: number, notes: string) => void;
  onClose: () => void;
  isSaving?: boolean;
};

export function TransferToStoreModal({
  open, warehouseId, stockItem, stores, onSave, onClose, isSaving,
}: Props) {
  const [storeId, setStoreId] = useState('');
  const [quantity, setQuantity] = useState('');
  const [notes, setNotes] = useState('');
  const [errors, setErrors] = useState<{ storeId?: string; quantity?: string }>({});

  useEffect(() => {
    if (open) {
      setStoreId('');
      setQuantity('');
      setNotes('');
      setErrors({});
    }
  }, [open]);

  function validate(): boolean {
    const e: { storeId?: string; quantity?: string } = {};
    if (!storeId) e.storeId = 'Select a store.';
    const qty = parseFloat(quantity);
    if (!quantity || isNaN(qty) || qty <= 0) e.quantity = 'Enter a valid positive quantity.';
    else if (stockItem && qty > stockItem.availableQuantity)
      e.quantity = `Cannot exceed available stock (${stockItem.availableQuantity}).`;
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  function handleSubmit(ev: React.FormEvent) {
    ev.preventDefault();
    if (validate()) onSave(storeId, parseFloat(quantity), notes);
  }

  const inputCls = (err?: string) =>
    cn(
      'w-full rounded-lg border px-3 py-2 text-sm bg-white dark:bg-gray-900',
      'text-gray-900 dark:text-white placeholder-gray-400',
      'focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 transition',
      err ? 'border-red-400 dark:border-red-500' : 'border-gray-200 dark:border-gray-700',
    );

  return (
    <Dialog.Root open={open} onOpenChange={(o) => !o && !isSaving && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-[60] bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <Dialog.Content
          className={cn(
            'fixed left-1/2 top-1/2 z-[60] -translate-x-1/2 -translate-y-1/2',
            'w-full max-w-md rounded-xl bg-white dark:bg-gray-900',
            'border border-gray-200 dark:border-gray-700 shadow-xl focus:outline-none',
            'data-[state=open]:animate-in data-[state=closed]:animate-out',
            'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
            'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
            'data-[state=closed]:slide-out-to-left-1/2 data-[state=closed]:slide-out-to-top-48%',
            'data-[state=open]:slide-in-from-left-1/2 data-[state=open]:slide-in-from-top-48%',
          )}
        >
          <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 px-6 py-4">
            <Dialog.Title className="flex items-center gap-2 text-base font-semibold text-gray-900 dark:text-white">
              <ArrowRightLeft className="w-5 h-5 text-primary-700" />
              Transfer to Store
            </Dialog.Title>
            <Dialog.Close disabled={isSaving} className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none disabled:opacity-40">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          <form onSubmit={handleSubmit} noValidate>
            <div className="px-6 py-5 space-y-4">

              {/* Product info */}
              {stockItem && (
                <div className="p-3 bg-primary-50 dark:bg-primary-900/20 rounded-lg border border-primary-200 dark:border-primary-800">
                  <p className="text-sm font-medium text-gray-900 dark:text-white">{stockItem.productName}</p>
                  <p className="text-xs text-gray-500 dark:text-gray-400 font-mono mt-0.5">{stockItem.sku}</p>
                  <div className="mt-2 flex gap-4 text-xs">
                    <span className="text-gray-600 dark:text-gray-400">
                      In stock: <strong>{stockItem.quantity}</strong>
                    </span>
                    <span className="text-gray-600 dark:text-gray-400">
                      Reserved: <strong>{stockItem.reservedQuantity}</strong>
                    </span>
                    <span className="text-emerald-700 dark:text-emerald-300 font-medium">
                      Available: <strong>{stockItem.availableQuantity}</strong>
                    </span>
                  </div>
                </div>
              )}

              {/* Store */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Destination Store <span className="text-red-500">*</span>
                </label>
                <select value={storeId} onChange={(e) => { setStoreId(e.target.value); setErrors((p) => ({ ...p, storeId: undefined })); }}
                  disabled={isSaving} className={inputCls(errors.storeId)}>
                  <option value="">Select a store…</option>
                  {stores.filter((s) => s.status === 'Active').map((s) => (
                    <option key={s.id} value={s.id}>{s.name} ({s.code})</option>
                  ))}
                </select>
                {errors.storeId && <p className="mt-1 text-xs text-red-500">{errors.storeId}</p>}
              </div>

              {/* Quantity */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Quantity <span className="text-red-500">*</span>
                </label>
                <input type="number" min={0.01} step={0.01}
                  value={quantity} onChange={(e) => { setQuantity(e.target.value); setErrors((p) => ({ ...p, quantity: undefined })); }}
                  placeholder={stockItem ? `Max ${stockItem.availableQuantity}` : '0'}
                  disabled={isSaving} className={inputCls(errors.quantity)} />
                {errors.quantity && <p className="mt-1 text-xs text-red-500">{errors.quantity}</p>}
              </div>

              {/* Notes */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Notes</label>
                <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2}
                  placeholder="Optional reason or reference…" disabled={isSaving}
                  className={cn(inputCls(), 'resize-none')} />
              </div>
            </div>

            <div className="flex justify-end gap-3 px-6 py-4 border-t border-gray-200 dark:border-gray-700">
              <button type="button" onClick={onClose} disabled={isSaving}
                className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-40">
                Cancel
              </button>
              <button type="submit" disabled={isSaving}
                className="px-4 py-2 text-sm font-medium text-white bg-primary-800 hover:bg-primary-900 rounded-lg transition-colors disabled:opacity-50 flex items-center gap-2">
                {isSaving && <svg className="w-4 h-4 animate-spin" viewBox="0 0 24 24" fill="none"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" /></svg>}
                {isSaving ? 'Transferring…' : 'Transfer'}
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
