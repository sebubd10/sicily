import { useState, useEffect, useRef } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { RotateCcw, X, Loader2, ChevronDown, Check } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { CreateSupplierReturnFormData } from '../../types/supplierReturn';
import type { Supplier } from '../../types/supplier';
import type { Store } from '../../types/store';

type Props = {
  open: boolean;
  suppliers: Supplier[];
  stores: Store[];
  isSaving?: boolean;
  error?: string | null;
  onSave: (form: CreateSupplierReturnFormData) => void;
  onClose: () => void;
};

const EMPTY: CreateSupplierReturnFormData = {
  supplierId: '',
  storeId: '',
  purchaseOrderId: '',
  notes: '',
};

export function CreateSupplierReturnModal({
  open, suppliers, stores, isSaving, error, onSave, onClose,
}: Props) {
  const [form, setForm] = useState(EMPTY);
  const [supplierQuery, setSupplierQuery] = useState('');
  const [supplierOpen, setSupplierOpen] = useState(false);
  const [errors, setErrors] = useState<Partial<Record<keyof CreateSupplierReturnFormData, string>>>({});
  const supplierRef = useRef<HTMLDivElement>(null);

  const filteredSuppliers = suppliers.filter((s) =>
    s.name.toLowerCase().includes(supplierQuery.toLowerCase()) ||
    s.code.toLowerCase().includes(supplierQuery.toLowerCase()),
  );

  useEffect(() => {
    if (open) { setForm(EMPTY); setSupplierQuery(''); setErrors({}); }
  }, [open]);

  useEffect(() => {
    function onClick(e: MouseEvent) {
      if (supplierRef.current && !supplierRef.current.contains(e.target as Node))
        setSupplierOpen(false);
    }
    document.addEventListener('mousedown', onClick);
    return () => document.removeEventListener('mousedown', onClick);
  }, []);

  function set(k: keyof CreateSupplierReturnFormData, v: string) {
    setForm((f) => ({ ...f, [k]: v }));
    setErrors((e) => ({ ...e, [k]: '' }));
  }

  function validate(): boolean {
    const e: typeof errors = {};
    if (!form.supplierId) e.supplierId = 'Supplier is required';
    if (!form.storeId) e.storeId = 'Store is required';
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  function handleSubmit(ev: React.FormEvent) {
    ev.preventDefault();
    if (!validate()) return;
    onSave(form);
  }

  const selectedSupplier = suppliers.find((s) => s.id === form.supplierId);

  const fieldCls = (err?: string) =>
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
        <Dialog.Content className={cn(
          'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
          'w-full max-w-lg rounded-xl bg-white dark:bg-gray-900',
          'border border-gray-200 dark:border-gray-700 shadow-2xl focus:outline-none',
          'data-[state=open]:animate-in data-[state=closed]:animate-out',
          'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
          'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
        )}>
          <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 px-6 py-4">
            <Dialog.Title className="flex items-center gap-2.5 text-base font-semibold text-gray-900 dark:text-white">
              <RotateCcw className="w-5 h-5 text-primary-700 flex-shrink-0" />
              New Supplier Return
            </Dialog.Title>
            <Dialog.Description className="sr-only">Create a new supplier return</Dialog.Description>
            <Dialog.Close disabled={isSaving} className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none disabled:opacity-40">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          <form onSubmit={handleSubmit} noValidate>
            <div className="px-6 py-5 space-y-4">
              {error && (
                <div className="rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 px-4 py-3 text-sm text-red-600 dark:text-red-400">
                  {error}
                </div>
              )}

              {/* Supplier combobox */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Supplier <span className="text-red-500">*</span>
                </label>
                <div ref={supplierRef} className="relative">
                  <button
                    type="button"
                    onClick={() => setSupplierOpen((o) => !o)}
                    disabled={isSaving}
                    className={cn(
                      fieldCls(errors.supplierId),
                      'flex items-center justify-between text-left',
                      !selectedSupplier && 'text-gray-400',
                    )}
                  >
                    <span>{selectedSupplier ? selectedSupplier.name : 'Search supplier…'}</span>
                    <ChevronDown className={cn('w-4 h-4 text-gray-400 transition-transform flex-shrink-0', supplierOpen && 'rotate-180')} />
                  </button>
                  {supplierOpen && (
                    <div className="absolute z-10 mt-1 w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 shadow-lg">
                      <div className="p-2 border-b border-gray-100 dark:border-gray-800">
                        <input
                          autoFocus
                          type="text"
                          value={supplierQuery}
                          onChange={(e) => setSupplierQuery(e.target.value)}
                          placeholder="Search…"
                          className="w-full rounded-md border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 px-3 py-1.5 text-sm text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:ring-1 focus:ring-primary-500"
                        />
                      </div>
                      <ul className="max-h-48 overflow-y-auto py-1">
                        {filteredSuppliers.length === 0 ? (
                          <li className="px-3 py-2 text-sm text-gray-400">No suppliers found</li>
                        ) : filteredSuppliers.map((s) => (
                          <li
                            key={s.id}
                            onClick={() => { set('supplierId', s.id); setSupplierQuery(''); setSupplierOpen(false); }}
                            className={cn(
                              'flex items-center gap-2 px-3 py-2 text-sm cursor-pointer transition-colors',
                              s.id === form.supplierId
                                ? 'bg-primary-50 dark:bg-primary-900/20 text-primary-700 dark:text-primary-400'
                                : 'text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800',
                            )}
                          >
                            {s.id === form.supplierId && <Check className="w-3.5 h-3.5 flex-shrink-0" />}
                            <span className={s.id === form.supplierId ? '' : 'ml-5'}>{s.name}</span>
                            <span className="ml-auto text-xs text-gray-400">{s.code}</span>
                          </li>
                        ))}
                      </ul>
                    </div>
                  )}
                </div>
                {errors.supplierId && <p className="mt-1 text-xs text-red-500">{errors.supplierId}</p>}
              </div>

              {/* Store */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Store <span className="text-red-500">*</span>
                </label>
                <select
                  value={form.storeId}
                  onChange={(e) => set('storeId', e.target.value)}
                  disabled={isSaving}
                  className={fieldCls(errors.storeId)}
                >
                  <option value="">Select store…</option>
                  {stores.map((s) => (
                    <option key={s.id} value={s.id}>{s.name} ({s.code})</option>
                  ))}
                </select>
                {errors.storeId && <p className="mt-1 text-xs text-red-500">{errors.storeId}</p>}
              </div>

              {/* Optional PO reference */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Purchase Order Reference <span className="text-xs text-gray-400">(optional)</span>
                </label>
                <input
                  type="text"
                  value={form.purchaseOrderId ?? ''}
                  onChange={(e) => set('purchaseOrderId', e.target.value)}
                  placeholder="PO-202406-XXXXX"
                  disabled={isSaving}
                  className={fieldCls()}
                />
              </div>

              {/* Notes */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Notes <span className="text-xs text-gray-400">(optional)</span>
                </label>
                <textarea
                  rows={2}
                  value={form.notes ?? ''}
                  onChange={(e) => set('notes', e.target.value)}
                  placeholder="Reason for return, batch numbers, etc."
                  disabled={isSaving}
                  className={cn(fieldCls(), 'resize-none')}
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
                disabled={isSaving}
                className="flex items-center gap-2 px-5 py-2 text-sm font-medium text-white bg-primary-800 hover:bg-primary-900 rounded-lg transition-colors disabled:opacity-50"
              >
                {isSaving && <Loader2 className="w-4 h-4 animate-spin" />}
                Create Return
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
