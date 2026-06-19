import { useEffect, useRef, useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { Layers, X, Search, AlertCircle } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { ReceiveBatchRequest } from '../../types/inventory';
import { useProducts } from '../../hooks/useProducts';

type Props = {
  open: boolean;
  storeId: string;
  onSave: (data: ReceiveBatchRequest) => void;
  onClose: () => void;
  isSaving?: boolean;
  error?: string | null;
};

type FormData = {
  productId: string;
  productLabel: string;
  quantity: string;
  expiryDate: string;
  lotNumber: string;
  unitCost: string;
  reference: string;
  notes: string;
};

const EMPTY: FormData = {
  productId: '', productLabel: '', quantity: '',
  expiryDate: '', lotNumber: '', unitCost: '',
  reference: '', notes: '',
};

export function ReceiveBatchModal({ open, storeId, onSave, onClose, isSaving, error }: Props) {
  const [form, setForm] = useState<FormData>(EMPTY);
  const [errors, setErrors] = useState<Partial<Record<keyof FormData, string>>>({});
  const [productSearch, setProductSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [productDropOpen, setProductDropOpen] = useState(false);
  const productRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (open) { setForm(EMPTY); setErrors({}); setProductSearch(''); }
  }, [open]);

  useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(productSearch), 300);
    return () => clearTimeout(t);
  }, [productSearch]);

  useEffect(() => {
    if (!productDropOpen) return;
    function handler(e: MouseEvent) {
      if (!productRef.current?.contains(e.target as Node)) setProductDropOpen(false);
    }
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, [productDropOpen]);

  const { data: productData, isFetching: productFetching } = useProducts({
    page: 1, pageSize: 20, search: debouncedSearch || undefined,
  });
  const productItems = productData?.items ?? [];

  function set<K extends keyof FormData>(k: K, v: FormData[K]) {
    setForm((f) => ({ ...f, [k]: v }));
    setErrors((e) => ({ ...e, [k]: undefined }));
  }

  function validate(): boolean {
    const e: Partial<Record<keyof FormData, string>> = {};
    if (!form.productId) e.productId = 'Select a product.';
    const qty = parseFloat(form.quantity);
    if (!form.quantity || isNaN(qty) || qty <= 0) e.quantity = 'Quantity must be greater than 0.';
    if (form.unitCost && (isNaN(parseFloat(form.unitCost)) || parseFloat(form.unitCost) <= 0))
      e.unitCost = 'Enter a valid unit cost.';
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  function handleSubmit(ev: React.FormEvent) {
    ev.preventDefault();
    if (!validate()) return;
    onSave({
      productId: form.productId,
      quantity: parseFloat(form.quantity),
      expiryDate: form.expiryDate || null,
      lotNumber: form.lotNumber.trim() || null,
      unitCost: form.unitCost ? parseFloat(form.unitCost) : null,
      reference: form.reference.trim() || null,
      notes: form.notes.trim() || null,
    });
  }

  const inputCls = (err?: string) => cn(
    'w-full rounded-lg border px-3 py-2 text-sm bg-white dark:bg-gray-900',
    'text-gray-900 dark:text-white placeholder-gray-400',
    'focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 transition',
    err ? 'border-red-400 dark:border-red-500' : 'border-gray-200 dark:border-gray-700',
  );

  return (
    <Dialog.Root open={open} onOpenChange={(o) => !o && !isSaving && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <Dialog.Content className={cn(
          'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
          'w-full max-w-lg rounded-xl bg-white dark:bg-gray-900',
          'border border-gray-200 dark:border-gray-700 shadow-xl focus:outline-none',
          'data-[state=open]:animate-in data-[state=closed]:animate-out',
          'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
          'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
          'data-[state=closed]:slide-out-to-left-1/2 data-[state=closed]:slide-out-to-top-48%',
          'data-[state=open]:slide-in-from-left-1/2 data-[state=open]:slide-in-from-top-48%',
        )}>
          {/* Header */}
          <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 px-6 py-4">
            <Dialog.Title className="flex items-center gap-2 text-base font-semibold text-gray-900 dark:text-white">
              <Layers className="w-5 h-5 text-primary-700" />
              Receive Stock Batch
            </Dialog.Title>
            <Dialog.Close disabled={isSaving} className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none disabled:opacity-40">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          <Dialog.Description className="sr-only">Receive a new perishable stock batch</Dialog.Description>

          <form onSubmit={handleSubmit} noValidate>
            <div className="px-6 py-5 space-y-4 max-h-[70vh] overflow-y-auto">

              {/* Error banner */}
              {error && (
                <div className="flex items-start gap-2 rounded-lg border border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-900/20 px-3 py-2.5">
                  <AlertCircle className="w-4 h-4 text-red-600 dark:text-red-400 flex-shrink-0 mt-0.5" />
                  <p className="text-xs text-red-700 dark:text-red-300">{error}</p>
                </div>
              )}

              {/* Product search */}
              <div ref={productRef}>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Product <span className="text-red-500">*</span>
                  <span className="ml-2 text-xs text-gray-400 font-normal">(perishable products only)</span>
                </label>
                {form.productId ? (
                  <div className={cn(
                    'flex items-center justify-between rounded-lg border px-3 py-2 text-sm',
                    'bg-primary-50 dark:bg-primary-900/20 border-primary-200 dark:border-primary-700',
                  )}>
                    <span className="font-medium text-primary-700 dark:text-primary-300">{form.productLabel}</span>
                    <button
                      type="button"
                      onClick={() => { set('productId', ''); set('productLabel', ''); }}
                      disabled={isSaving}
                      className="text-primary-400 hover:text-primary-600 dark:hover:text-primary-200 ml-2 flex-shrink-0"
                    >
                      <X className="w-3.5 h-3.5" />
                    </button>
                  </div>
                ) : (
                  <div className="relative">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-gray-400" />
                    <input
                      type="text"
                      value={productSearch}
                      onChange={(e) => { setProductSearch(e.target.value); setProductDropOpen(true); }}
                      onFocus={() => setProductDropOpen(true)}
                      placeholder="Search by name or SKU…"
                      disabled={isSaving}
                      className={cn(inputCls(errors.productId), 'pl-8')}
                    />
                    {productDropOpen && (
                      <ul className="absolute z-50 mt-1 w-full max-h-48 overflow-y-auto rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 shadow-lg py-1">
                        {productFetching && (
                          <li className="px-3 py-2 text-sm text-gray-400 italic">Searching…</li>
                        )}
                        {!productFetching && productItems.length === 0 && (
                          <li className="px-3 py-2 text-sm text-gray-400 italic">No products found.</li>
                        )}
                        {!productFetching && productItems.map((p) => (
                          <li key={p.id}>
                            <button
                              type="button"
                              onMouseDown={(e) => {
                                e.preventDefault();
                                set('productId', p.id);
                                set('productLabel', `${p.name} (${p.sku})`);
                                setProductSearch('');
                                setProductDropOpen(false);
                              }}
                              className="w-full text-left px-3 py-2 text-sm hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
                            >
                              <span className="font-medium text-gray-800 dark:text-gray-200">{p.name}</span>
                              <span className="ml-2 text-xs text-gray-400 font-mono">{p.sku}</span>
                            </button>
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>
                )}
                {errors.productId && <p className="mt-1 text-xs text-red-500">{errors.productId}</p>}
              </div>

              {/* Quantity + Expiry Date */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Quantity <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="number" min="0.01" step="0.01"
                    value={form.quantity}
                    onChange={(e) => set('quantity', e.target.value)}
                    placeholder="0"
                    disabled={isSaving}
                    className={inputCls(errors.quantity)}
                  />
                  {errors.quantity && <p className="mt-1 text-xs text-red-500">{errors.quantity}</p>}
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Expiry Date
                  </label>
                  <input
                    type="date"
                    value={form.expiryDate}
                    onChange={(e) => set('expiryDate', e.target.value)}
                    disabled={isSaving}
                    className={inputCls()}
                  />
                </div>
              </div>

              {/* Lot Number + Unit Cost */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Lot / Batch Number</label>
                  <input
                    type="text"
                    value={form.lotNumber}
                    onChange={(e) => set('lotNumber', e.target.value)}
                    placeholder="LOT-2024-001"
                    disabled={isSaving}
                    className={inputCls()}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Unit Cost</label>
                  <input
                    type="number" min="0.01" step="0.01"
                    value={form.unitCost}
                    onChange={(e) => set('unitCost', e.target.value)}
                    placeholder="0.00"
                    disabled={isSaving}
                    className={inputCls(errors.unitCost)}
                  />
                  {errors.unitCost && <p className="mt-1 text-xs text-red-500">{errors.unitCost}</p>}
                </div>
              </div>

              {/* Reference */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Reference</label>
                <input
                  type="text"
                  value={form.reference}
                  onChange={(e) => set('reference', e.target.value)}
                  placeholder="Delivery note / PO number…"
                  disabled={isSaving}
                  className={inputCls()}
                />
              </div>

              {/* Notes */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Notes</label>
                <textarea
                  value={form.notes}
                  onChange={(e) => set('notes', e.target.value)}
                  rows={2}
                  placeholder="Optional notes…"
                  disabled={isSaving}
                  className={cn(inputCls(), 'resize-none')}
                />
              </div>
            </div>

            {/* Footer */}
            <div className="flex justify-end gap-3 px-6 py-4 border-t border-gray-200 dark:border-gray-700">
              <button
                type="button" onClick={onClose} disabled={isSaving}
                className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-40"
              >
                Cancel
              </button>
              <button
                type="submit" disabled={isSaving}
                className="px-4 py-2 text-sm font-medium text-white bg-primary-800 hover:bg-primary-900 rounded-lg transition-colors disabled:opacity-50 flex items-center gap-2"
              >
                {isSaving && (
                  <svg className="w-4 h-4 animate-spin" viewBox="0 0 24 24" fill="none">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
                  </svg>
                )}
                {isSaving ? 'Saving…' : 'Receive Batch'}
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
