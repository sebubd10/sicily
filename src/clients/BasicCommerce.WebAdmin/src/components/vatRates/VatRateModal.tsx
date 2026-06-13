import { useEffect, useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { Percent, X } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { VatRate, VatRateFormData } from '../../types/vatRate';

type Props = {
  open: boolean;
  vatRate: VatRate | null;
  onSave: (data: VatRateFormData) => void;
  onClose: () => void;
  isSaving?: boolean;
};

const EMPTY: VatRateFormData = {
  name: '',
  code: '',
  rate: '',
  isDefault: false,
};

export function VatRateModal({ open, vatRate, onSave, onClose, isSaving }: Props) {
  const isEdit = vatRate !== null;
  const [form, setForm] = useState<VatRateFormData>(EMPTY);
  const [errors, setErrors] = useState<Partial<Record<keyof VatRateFormData, string>>>({});

  useEffect(() => {
    if (open) {
      setErrors({});
      setForm(
        isEdit
          ? {
              name: vatRate.name,
              code: vatRate.code,
              rate: String(vatRate.rate),
              isDefault: vatRate.isDefault,
            }
          : EMPTY,
      );
    }
  }, [open, vatRate]);

  function set<K extends keyof VatRateFormData>(key: K, value: VatRateFormData[K]) {
    setForm((f) => ({ ...f, [key]: value }));
    setErrors((e) => ({ ...e, [key]: undefined }));
  }

  function validate(): boolean {
    const e: Partial<Record<keyof VatRateFormData, string>> = {};
    if (!form.name.trim()) e.name = 'Name is required.';
    if (!form.code.trim()) e.code = 'Code is required.';
    const rate = Number(form.rate);
    if (form.rate.trim() === '' || Number.isNaN(rate)) e.rate = 'Rate is required.';
    else if (rate < 0 || rate > 100) e.rate = 'Rate must be between 0 and 100.';
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  function handleSubmit(ev: React.FormEvent) {
    ev.preventDefault();
    if (validate()) onSave(form);
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
              <Percent className="w-5 h-5 text-primary-700" />
              {isEdit ? 'Edit VAT Rate' : 'Add VAT Rate'}
            </Dialog.Title>
            <Dialog.Description className="sr-only">
              {isEdit ? `Edit VAT rate ${vatRate?.name}` : 'Add a new VAT rate'}
            </Dialog.Description>
            <Dialog.Close disabled={isSaving} className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none disabled:opacity-40">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          <form onSubmit={handleSubmit} noValidate>
            <div className="px-6 py-5 space-y-4">
              {/* Name */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={form.name}
                  onChange={(e) => set('name', e.target.value)}
                  placeholder="Standard Rate"
                  disabled={isSaving}
                  className={inputCls(errors.name)}
                />
                {errors.name && <p className="mt-1 text-xs text-red-500">{errors.name}</p>}
              </div>

              {/* Code + Rate */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Code <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    value={form.code}
                    onChange={(e) => set('code', e.target.value.toUpperCase())}
                    placeholder="STD"
                    disabled={isSaving}
                    className={cn(inputCls(errors.code), 'font-mono')}
                  />
                  {errors.code && <p className="mt-1 text-xs text-red-500">{errors.code}</p>}
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Rate (%) <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    max="100"
                    value={form.rate}
                    onChange={(e) => set('rate', e.target.value)}
                    placeholder="15"
                    disabled={isSaving}
                    className={inputCls(errors.rate)}
                  />
                  {errors.rate && <p className="mt-1 text-xs text-red-500">{errors.rate}</p>}
                </div>
              </div>

              {/* Is Default */}
              <label className="flex items-center gap-2 text-sm text-gray-700 dark:text-gray-300 cursor-pointer select-none">
                <div
                  onClick={() => !isSaving && set('isDefault', !form.isDefault)}
                  className={cn(
                    'w-9 h-5 rounded-full transition-colors relative',
                    form.isDefault ? 'bg-primary-700' : 'bg-gray-300 dark:bg-gray-600',
                  )}
                >
                  <span className={cn(
                    'absolute top-0.5 w-4 h-4 rounded-full bg-white shadow transition-all',
                    form.isDefault ? 'left-4' : 'left-0.5',
                  )} />
                </div>
                Set as default VAT rate
              </label>
            </div>

            <div className="flex justify-end gap-3 px-6 py-4 border-t border-gray-200 dark:border-gray-700">
              <button
                type="button"
                onClick={onClose}
                disabled={isSaving}
                className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-40"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isSaving}
                className="px-4 py-2 text-sm font-medium text-white bg-primary-800 hover:bg-primary-900 rounded-lg transition-colors disabled:opacity-50 flex items-center gap-2"
              >
                {isSaving && (
                  <svg className="w-4 h-4 animate-spin" viewBox="0 0 24 24" fill="none">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
                  </svg>
                )}
                {isSaving ? 'Saving…' : isEdit ? 'Save Changes' : 'Create VAT Rate'}
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
