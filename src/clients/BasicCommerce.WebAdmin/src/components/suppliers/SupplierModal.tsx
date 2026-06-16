import { useEffect, useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { Truck, X } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { Supplier, SupplierFormData } from '../../types/supplier';
import { useDistricts } from '../../hooks/useWarehouses';

type Props = {
  open: boolean;
  supplier: Supplier | null;
  onSave: (data: SupplierFormData) => void;
  onClose: () => void;
  isSaving?: boolean;
};

const EMPTY: SupplierFormData = {
  name: '',
  code: '',
  contactName: '',
  email: '',
  phone: '',
  addressLine1: '',
  addressLine2: '',
  city: '',
  district: '',
  postalCode: '',
  leadTimeDays: 0,
  notes: '',
};

export function SupplierModal({ open, supplier, onSave, onClose, isSaving }: Props) {
  const isEdit = supplier !== null;
  const [form, setForm] = useState<SupplierFormData>(EMPTY);
  const [errors, setErrors] = useState<Partial<Record<keyof SupplierFormData, string>>>({});
  const { data: districts = [] } = useDistricts();

  useEffect(() => {
    if (open) {
      setErrors({});
      setForm(
        isEdit
          ? {
              name: supplier.name,
              code: supplier.code,
              contactName: supplier.contactName ?? '',
              email: supplier.email ?? '',
              phone: supplier.phone ?? '',
              addressLine1: supplier.addressLine1 ?? '',
              addressLine2: '',
              city: supplier.city ?? '',
              district: '',
              postalCode: '',
              leadTimeDays: supplier.leadTimeDays,
              notes: supplier.notes ?? '',
            }
          : EMPTY,
      );
    }
  }, [open, supplier]);

  function set<K extends keyof SupplierFormData>(key: K, value: SupplierFormData[K]) {
    setForm((f) => ({ ...f, [key]: value }));
    setErrors((e) => ({ ...e, [key]: undefined }));
  }

  function validate(): boolean {
    const e: Partial<Record<keyof SupplierFormData, string>> = {};
    if (!form.name.trim()) e.name = 'Name is required.';
    if (!isEdit && !form.code.trim()) e.code = 'Code is required.';
    if (form.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email))
      e.email = 'Enter a valid email address.';
    if (form.leadTimeDays < 0) e.leadTimeDays = 'Lead time cannot be negative.';
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
              <Truck className="w-5 h-5 text-primary-700" />
              {isEdit ? 'Edit Supplier' : 'Add Supplier'}
            </Dialog.Title>
            <Dialog.Description className="sr-only">
              {isEdit ? `Edit supplier ${supplier?.name}` : 'Add a new supplier'}
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
                    placeholder="Acme Distributors"
                    disabled={isSaving}
                    className={inputCls(errors.name)}
                  />
                  {errors.name && <p className="mt-1 text-xs text-red-500">{errors.name}</p>}
                </div>
                <div className="col-span-2 sm:col-span-1">
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Code <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    value={form.code}
                    onChange={(e) => set('code', e.target.value.toUpperCase())}
                    placeholder="ACME"
                    disabled={isSaving || isEdit}
                    title={isEdit ? 'Supplier code cannot be changed' : undefined}
                    className={cn(inputCls(errors.code), 'font-mono', isEdit && 'opacity-60 cursor-not-allowed')}
                  />
                  {errors.code && <p className="mt-1 text-xs text-red-500">{errors.code}</p>}
                </div>
              </div>

              {/* Contact Name + Email */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Contact Name
                  </label>
                  <input
                    type="text"
                    value={form.contactName}
                    onChange={(e) => set('contactName', e.target.value)}
                    placeholder="John Smith"
                    disabled={isSaving}
                    className={inputCls()}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Email
                  </label>
                  <input
                    type="email"
                    value={form.email}
                    onChange={(e) => set('email', e.target.value)}
                    placeholder="supplier@example.com"
                    disabled={isSaving}
                    className={inputCls(errors.email)}
                  />
                  {errors.email && <p className="mt-1 text-xs text-red-500">{errors.email}</p>}
                </div>
              </div>

              {/* Phone + Lead Time */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Phone
                  </label>
                  <input
                    type="text"
                    value={form.phone}
                    onChange={(e) => set('phone', e.target.value)}
                    placeholder="+880..."
                    disabled={isSaving}
                    className={inputCls()}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Lead Time (days)
                  </label>
                  <input
                    type="number"
                    min={0}
                    value={form.leadTimeDays}
                    onChange={(e) => set('leadTimeDays', parseInt(e.target.value) || 0)}
                    disabled={isSaving}
                    className={inputCls(errors.leadTimeDays)}
                  />
                  {errors.leadTimeDays && <p className="mt-1 text-xs text-red-500">{errors.leadTimeDays}</p>}
                </div>
              </div>

              {/* Address */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Address Line 1
                </label>
                <input
                  type="text"
                  value={form.addressLine1}
                  onChange={(e) => set('addressLine1', e.target.value)}
                  placeholder="123 Main Street"
                  disabled={isSaving}
                  className={inputCls()}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Address Line 2
                </label>
                <input
                  type="text"
                  value={form.addressLine2}
                  onChange={(e) => set('addressLine2', e.target.value)}
                  placeholder="Suite, floor, etc. (optional)"
                  disabled={isSaving}
                  className={inputCls()}
                />
              </div>

              {/* City + District */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    City
                  </label>
                  <input
                    type="text"
                    value={form.city}
                    onChange={(e) => set('city', e.target.value)}
                    placeholder="Dhaka"
                    disabled={isSaving}
                    className={inputCls()}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    District
                  </label>
                  <select
                    value={form.district}
                    onChange={(e) => set('district', e.target.value)}
                    disabled={isSaving}
                    className={inputCls()}
                  >
                    <option value="">Select district…</option>
                    {districts.map((d) => (
                      <option key={d.code} value={d.code}>{d.name}</option>
                    ))}
                  </select>
                </div>
              </div>

              {/* Postal Code */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Postal Code
                  </label>
                  <input
                    type="text"
                    value={form.postalCode}
                    onChange={(e) => set('postalCode', e.target.value)}
                    placeholder="1212"
                    disabled={isSaving}
                    className={inputCls()}
                  />
                </div>
              </div>

              {/* Notes */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Notes
                </label>
                <textarea
                  rows={2}
                  value={form.notes}
                  onChange={(e) => set('notes', e.target.value)}
                  placeholder="Additional notes…"
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
                {isSaving ? 'Saving…' : isEdit ? 'Save Changes' : 'Create Supplier'}
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
