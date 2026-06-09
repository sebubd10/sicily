import { useEffect, useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { Factory, X } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { Manufacturer, ManufacturerFormData } from '../../types/manufacturer';
import { useCountries } from '../../hooks/useManufacturers';

type Props = {
  open: boolean;
  manufacturer: Manufacturer | null;
  onSave: (data: ManufacturerFormData) => void;
  onClose: () => void;
  isSaving?: boolean;
};

const EMPTY: ManufacturerFormData = {
  name: '',
  code: '',
  country: '',
  website: '',
  contactEmail: '',
  notes: '',
};

export function ManufacturerModal({ open, manufacturer, onSave, onClose, isSaving }: Props) {
  const isEdit = manufacturer !== null;
  const [form, setForm] = useState<ManufacturerFormData>(EMPTY);
  const { data: countries = [] } = useCountries();
  const [errors, setErrors] = useState<Partial<Record<keyof ManufacturerFormData, string>>>({});

  useEffect(() => {
    if (open) {
      setErrors({});
      setForm(
        isEdit
          ? {
              name: manufacturer.name,
              code: manufacturer.code ?? '',
              country: manufacturer.country ?? '',
              website: manufacturer.website ?? '',
              contactEmail: manufacturer.contactEmail ?? '',
              notes: manufacturer.notes ?? '',
            }
          : EMPTY,
      );
    }
  }, [open, manufacturer]);

  function set<K extends keyof ManufacturerFormData>(key: K, value: string) {
    setForm((f) => ({ ...f, [key]: value }));
    setErrors((e) => ({ ...e, [key]: undefined }));
  }

  function validate(): boolean {
    const e: Partial<Record<keyof ManufacturerFormData, string>> = {};
    if (!form.name.trim()) e.name = 'Name is required.';
    if (form.contactEmail && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.contactEmail))
      e.contactEmail = 'Enter a valid email address.';
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
            'w-full max-w-lg rounded-xl bg-white dark:bg-gray-900',
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
              <Factory className="w-5 h-5 text-primary-700" />
              {isEdit ? 'Edit Manufacturer' : 'Add Manufacturer'}
            </Dialog.Title>
            <Dialog.Description className="sr-only">
              {isEdit ? `Edit manufacturer ${manufacturer?.name}` : 'Add a new manufacturer'}
            </Dialog.Description>
            <Dialog.Close disabled={isSaving} className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none disabled:opacity-40">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          <form onSubmit={handleSubmit} noValidate>
            <div className="px-6 py-5 space-y-4 max-h-[65vh] overflow-y-auto">

              {/* Name + Code */}
              <div className="grid grid-cols-2 gap-3">
                <div className="col-span-2 sm:col-span-1">
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Name <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    value={form.name}
                    onChange={(e) => set('name', e.target.value)}
                    placeholder="Acme Corp"
                    disabled={isSaving}
                    className={inputCls(errors.name)}
                  />
                  {errors.name && <p className="mt-1 text-xs text-red-500">{errors.name}</p>}
                </div>
                <div className="col-span-2 sm:col-span-1">
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Code
                  </label>
                  <input
                    type="text"
                    value={form.code}
                    onChange={(e) => set('code', e.target.value.toUpperCase())}
                    placeholder="ACME"
                    disabled={isSaving}
                    className={inputCls()}
                  />
                </div>
              </div>

              {/* Country + Email */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Country
                  </label>
                  <select
                    value={form.country}
                    onChange={(e) => set('country', e.target.value)}
                    disabled={isSaving}
                    className={inputCls()}
                  >
                    <option value="">Select country…</option>
                    {countries.map((c) => (
                      <option key={c.code} value={c.code}>{c.name}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Contact Email
                  </label>
                  <input
                    type="email"
                    value={form.contactEmail}
                    onChange={(e) => set('contactEmail', e.target.value)}
                    placeholder="contact@acme.com"
                    disabled={isSaving}
                    className={inputCls(errors.contactEmail)}
                  />
                  {errors.contactEmail && <p className="mt-1 text-xs text-red-500">{errors.contactEmail}</p>}
                </div>
              </div>

              {/* Website */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Website
                </label>
                <input
                  type="url"
                  value={form.website}
                  onChange={(e) => set('website', e.target.value)}
                  placeholder="https://acme.com"
                  disabled={isSaving}
                  className={inputCls()}
                />
              </div>

              {/* Notes */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Notes
                </label>
                <textarea
                  value={form.notes}
                  onChange={(e) => set('notes', e.target.value)}
                  rows={3}
                  placeholder="Optional internal notes…"
                  disabled={isSaving}
                  className={cn(inputCls(), 'resize-none')}
                />
              </div>
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
                {isSaving ? 'Saving…' : isEdit ? 'Save Changes' : 'Create Manufacturer'}
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
