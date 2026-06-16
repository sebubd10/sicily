import { useState, useEffect } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { PackageCheck, X, AlertCircle } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { PurchaseOrderDetail } from '../../types/purchaseOrder';

type Props = {
  open: boolean;
  po: PurchaseOrderDetail | null;
  onSave: (items: { productId: string; receivedQuantity: number }[], notes?: string) => void;
  onClose: () => void;
  isSaving?: boolean;
  error?: string | null;
};

export function ReceiveModal({ open, po, onSave, onClose, isSaving, error }: Props) {
  const [quantities, setQuantities] = useState<Record<string, number>>({});
  const [notes, setNotes] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});

  const pendingItems = po?.items.filter((i) => i.remainingQuantity > 0) ?? [];

  useEffect(() => {
    if (open && po) {
      const init: Record<string, number> = {};
      po.items
        .filter((i) => i.remainingQuantity > 0)
        .forEach((i) => { init[i.productId] = i.remainingQuantity; });
      setQuantities(init);
      setNotes('');
      setErrors({});
    }
  }, [open, po]);

  function setQty(productId: string, val: number) {
    setQuantities((q) => ({ ...q, [productId]: val }));
    setErrors((e) => ({ ...e, [productId]: '' }));
  }

  function validate(): boolean {
    const e: Record<string, string> = {};
    let hasAny = false;
    pendingItems.forEach((item) => {
      const qty = quantities[item.productId] ?? 0;
      if (qty < 0) e[item.productId] = 'Cannot be negative';
      else if (qty > item.remainingQuantity) e[item.productId] = `Max is ${item.remainingQuantity}`;
      if (qty > 0) hasAny = true;
    });
    if (!hasAny) e['_general'] = 'Enter at least one received quantity greater than zero.';
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  function handleSubmit(ev: React.FormEvent) {
    ev.preventDefault();
    if (!validate()) return;
    const items = pendingItems
      .map((i) => ({ productId: i.productId, receivedQuantity: quantities[i.productId] ?? 0 }))
      .filter((i) => i.receivedQuantity > 0);
    onSave(items, notes.trim() || undefined);
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
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <Dialog.Content
          className={cn(
            'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
            'w-full max-w-2xl rounded-xl bg-white dark:bg-gray-900',
            'border border-gray-200 dark:border-gray-700 shadow-2xl focus:outline-none',
            'data-[state=open]:animate-in data-[state=closed]:animate-out',
            'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
            'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
          )}
        >
          <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 px-6 py-4">
            <Dialog.Title className="flex items-center gap-2 text-base font-semibold text-gray-900 dark:text-white">
              <PackageCheck className="w-5 h-5 text-emerald-600" />
              Record Receipt{po ? ` · ${po.orderNumber}` : ''}
            </Dialog.Title>
            <Dialog.Description className="sr-only">
              Record items received against this purchase order
            </Dialog.Description>
            <Dialog.Close
              disabled={isSaving}
              className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none disabled:opacity-40"
            >
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          <form onSubmit={handleSubmit} noValidate>
            <div className="px-6 py-5 space-y-4 max-h-[65vh] overflow-y-auto">

              {/* API error */}
              {error && (
                <div className="flex items-center gap-2 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 px-4 py-3 text-sm text-red-600 dark:text-red-400">
                  <AlertCircle className="w-4 h-4 flex-shrink-0" />
                  {error}
                </div>
              )}

              {/* Validation banner */}
              {errors['_general'] && (
                <div className="flex items-center gap-2 rounded-lg bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800 px-4 py-3 text-sm text-amber-700 dark:text-amber-400">
                  <AlertCircle className="w-4 h-4 flex-shrink-0" />
                  {errors['_general']}
                </div>
              )}

              {/* Items table */}
              <div className="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                      {['Product', 'SKU', 'Ordered', 'Received', 'Remaining', 'Receive Now'].map((h) => (
                        <th key={h} className="px-3 py-2.5 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider whitespace-nowrap">
                          {h}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                    {pendingItems.length === 0 ? (
                      <tr>
                        <td colSpan={6} className="px-4 py-8 text-center text-sm text-gray-400">
                          No pending items to receive.
                        </td>
                      </tr>
                    ) : (
                      pendingItems.map((item) => (
                        <tr key={item.productId} className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
                          <td className="px-3 py-2.5 font-medium text-gray-900 dark:text-white">{item.productName}</td>
                          <td className="px-3 py-2.5 font-mono text-xs text-gray-500 dark:text-gray-400">{item.sku}</td>
                          <td className="px-3 py-2.5 text-center text-gray-600 dark:text-gray-400">{item.orderedQuantity}</td>
                          <td className="px-3 py-2.5 text-center text-emerald-600 dark:text-emerald-400">{item.receivedQuantity}</td>
                          <td className="px-3 py-2.5 text-center font-medium text-amber-600 dark:text-amber-400">{item.remainingQuantity}</td>
                          <td className="px-3 py-2.5 w-32">
                            <input
                              type="number"
                              min={0}
                              max={item.remainingQuantity}
                              step={1}
                              value={quantities[item.productId] ?? item.remainingQuantity}
                              onChange={(e) => setQty(item.productId, Number(e.target.value) || 0)}
                              disabled={isSaving}
                              className={cn(
                                'w-full rounded border px-2 py-1 text-sm text-center',
                                'bg-white dark:bg-gray-900 text-gray-900 dark:text-white',
                                'focus:outline-none focus:ring-1 focus:ring-primary-500 transition',
                                errors[item.productId]
                                  ? 'border-red-400 dark:border-red-500'
                                  : 'border-gray-200 dark:border-gray-700',
                              )}
                            />
                            {errors[item.productId] && (
                              <p className="mt-0.5 text-xs text-red-500 text-center">{errors[item.productId]}</p>
                            )}
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>

              {/* Notes */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Receipt Notes
                </label>
                <textarea
                  rows={2}
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  placeholder="Optional notes about this receipt…"
                  disabled={isSaving}
                  className={cn(inputCls(), 'resize-none')}
                />
              </div>
            </div>

            <div className="flex justify-end gap-3 px-6 py-4 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50 rounded-b-xl">
              <button
                type="button"
                onClick={onClose}
                disabled={isSaving}
                className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors disabled:opacity-40"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isSaving || pendingItems.length === 0}
                className="flex items-center gap-2 px-5 py-2 text-sm font-medium text-white bg-emerald-700 hover:bg-emerald-800 rounded-lg transition-colors disabled:opacity-50"
              >
                {isSaving && (
                  <svg className="w-4 h-4 animate-spin" viewBox="0 0 24 24" fill="none">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
                  </svg>
                )}
                {isSaving ? 'Saving…' : 'Confirm Receipt'}
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
