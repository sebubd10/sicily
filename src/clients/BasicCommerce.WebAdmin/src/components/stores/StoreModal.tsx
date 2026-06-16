import { useEffect, useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { Store as StoreIcon, X } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { Store, StoreFormData } from '../../types/store';
import { useDistricts } from '../../hooks/useWarehouses';

type Props = {
  open: boolean;
  store: Store | null;
  onSave: (data: StoreFormData) => void;
  onClose: () => void;
  isSaving?: boolean;
};

const EMPTY: StoreFormData = {
  name: '',
  code: '',
  addressLine1: '',
  addressLine2: '',
  city: '',
  district: '',
  postalCode: '',
  phone: '',
  email: '',
  openingTime: '08:00',
  closingTime: '22:00',
};

export function StoreModal({ open, store, onSave, onClose, isSaving }: Props) {
  const isEdit = store !== null;
  const [form, setForm] = useState<StoreFormData>(EMPTY);
  const [errors, setErrors] = useState<Partial<Record<keyof StoreFormData, string>>>({});
  const { data: districts = [] } = useDistricts();

  useEffect(() => {
    if (open) {
      setErrors({});
      setForm(
        isEdit
          ? {
              name: store.name,
              code: store.code,
              addressLine1: store.addressLine1,
              addressLine2: store.addressLine2 ?? '',
              city: store.city,
              district: store.district,
              postalCode: store.postalCode,
              phone: store.phone ?? '',
              email: store.email ?? '',
              openingTime: store.openingTime,
              closingTime: store.closingTime,
            }
          : EMPTY,
      );
    }
  }, [open, store]);

  function set<K extends keyof StoreFormData>(key: K, value: string) {
    setForm((f) => ({ ...f, [key]: value }));
    setErrors((e) => ({ ...e, [key]: undefined }));
  }

  function validate(): boolean {
    const e: Partial<Record<keyof StoreFormData, string>> = {};
    if (!form.name.trim()) e.name = 'Name is required.';
    if (!isEdit && !form.code.trim()) e.code = 'Code is required.';
    if (!isEdit && form.code && !/^[A-Za-z0-9_-]+$/.test(form.code)) e.code = 'Code must be alphanumeric.';
    if (!form.addressLine1.trim()) e.addressLine1 = 'Address is required.';
    if (!form.city.trim()) e.city = 'City is required.';
    if (!form.district.trim()) e.district = 'District is required.';
    if (!form.postalCode.trim()) e.postalCode = 'Postal code is required.';
    if (form.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email))
      e.email = 'Enter a valid email address.';
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
              <StoreIcon className="w-5 h-5 text-primary-700" />
              {isEdit ? 'Edit Store' : 'Add Store'}
            </Dialog.Title>
            <Dialog.Description className="sr-only">
              {isEdit ? `Edit store ${store?.name}` : 'Add a new store'}
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
                    placeholder="Downtown Branch"
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
                    placeholder="DTN"
                    disabled={isSaving || isEdit}
                    title={isEdit ? 'Store code cannot be changed' : undefined}
                    className={cn(inputCls(errors.code), 'font-mono', isEdit && 'opacity-60 cursor-not-allowed')}
                  />
                  {errors.code && <p className="mt-1 text-xs text-red-500">{errors.code}</p>}
                </div>
              </div>

              {/* Address line 1 + 2 */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Address Line 1 <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={form.addressLine1}
                  onChange={(e) => set('addressLine1', e.target.value)}
                  placeholder="123 Main Street"
                  disabled={isSaving}
                  className={inputCls(errors.addressLine1)}
                />
                {errors.addressLine1 && <p className="mt-1 text-xs text-red-500">{errors.addressLine1}</p>}
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
                    City <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    value={form.city}
                    onChange={(e) => set('city', e.target.value)}
                    placeholder="Dhaka"
                    disabled={isSaving}
                    className={inputCls(errors.city)}
                  />
                  {errors.city && <p className="mt-1 text-xs text-red-500">{errors.city}</p>}
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    District <span className="text-red-500">*</span>
                  </label>
                  <select
                    value={form.district}
                    onChange={(e) => set('district', e.target.value)}
                    disabled={isSaving}
                    className={inputCls(errors.district)}
                  >
                    <option value="">Select district…</option>
                    {districts.map((d) => (
                      <option key={d.code} value={d.code}>{d.name}</option>
                    ))}
                  </select>
                  {errors.district && <p className="mt-1 text-xs text-red-500">{errors.district}</p>}
                </div>
              </div>

              {/* Postal Code + Phone */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Postal Code <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    value={form.postalCode}
                    onChange={(e) => set('postalCode', e.target.value)}
                    placeholder="1212"
                    disabled={isSaving}
                    className={inputCls(errors.postalCode)}
                  />
                  {errors.postalCode && <p className="mt-1 text-xs text-red-500">{errors.postalCode}</p>}
                </div>
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
              </div>

              {/* Email */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Email
                </label>
                <input
                  type="email"
                  value={form.email}
                  onChange={(e) => set('email', e.target.value)}
                  placeholder="store@example.com"
                  disabled={isSaving}
                  className={inputCls(errors.email)}
                />
                {errors.email && <p className="mt-1 text-xs text-red-500">{errors.email}</p>}
              </div>

              {/* Opening + Closing time (edit only - new stores get defaults) */}
              {isEdit && (
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Opening Time
                    </label>
                    <input
                      type="time"
                      value={form.openingTime}
                      onChange={(e) => set('openingTime', e.target.value)}
                      disabled={isSaving}
                      className={inputCls()}
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Closing Time
                    </label>
                    <input
                      type="time"
                      value={form.closingTime}
                      onChange={(e) => set('closingTime', e.target.value)}
                      disabled={isSaving}
                      className={inputCls()}
                    />
                  </div>
                </div>
              )}
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
                {isSaving ? 'Saving…' : isEdit ? 'Save Changes' : 'Create Store'}
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
