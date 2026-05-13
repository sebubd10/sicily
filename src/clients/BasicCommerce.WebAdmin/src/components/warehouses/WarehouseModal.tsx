import { useEffect, useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { Warehouse, X } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { Warehouse as WarehouseType, WarehouseFormData } from '../../types/warehouse';
import { useDistricts } from '../../hooks/useWarehouses';

type Props = {
  open: boolean;
  warehouse: WarehouseType | null;
  onSave: (data: WarehouseFormData) => void;
  onClose: () => void;
  isSaving?: boolean;
};

const EMPTY: WarehouseFormData = {
  name: '',
  code: '',
  addressLine1: '',
  addressLine2: '',
  city: '',
  district: '',
  postalCode: '',
  country: 'BD',
  phone: '',
  email: '',
  isDefault: false,
};

export function WarehouseModal({ open, warehouse, onSave, onClose, isSaving }: Props) {
  const isEdit = warehouse !== null;
  const [form, setForm] = useState<WarehouseFormData>(EMPTY);
  const [errors, setErrors] = useState<Partial<Record<keyof WarehouseFormData, string>>>({});

  const { data: districts = [] } = useDistricts();

  useEffect(() => {
    if (open) {
      setErrors({});
      setForm(
        isEdit
          ? {
              name: warehouse.name,
              code: warehouse.code,
              addressLine1: warehouse.addressLine1,
              addressLine2: warehouse.addressLine2 ?? '',
              city: warehouse.city,
              district: warehouse.district,
              postalCode: warehouse.postalCode,
              country: warehouse.country,
              phone: warehouse.phone ?? '',
              email: warehouse.email ?? '',
              isDefault: warehouse.isDefault,
            }
          : EMPTY,
      );
    }
  }, [open, warehouse]);

  function set<K extends keyof WarehouseFormData>(key: K, value: WarehouseFormData[K]) {
    setForm((f) => ({ ...f, [key]: value }));
    setErrors((e) => ({ ...e, [key]: undefined }));
  }

  function validate(): boolean {
    const e: Partial<Record<keyof WarehouseFormData, string>> = {};
    if (!form.name.trim())        e.name        = 'Name is required.';
    if (!isEdit && !form.code.trim()) e.code    = 'Code is required.';
    if (!form.addressLine1.trim()) e.addressLine1 = 'Address is required.';
    if (!form.city.trim())        e.city        = 'City is required.';
    if (!form.district)           e.district    = 'District is required.';
    if (!form.postalCode.trim())  e.postalCode  = 'Postal code is required.';
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
            'w-full max-w-xl rounded-xl bg-white dark:bg-gray-900',
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
              <Warehouse className="w-5 h-5 text-primary-700" />
              {isEdit ? 'Edit Warehouse' : 'Add Warehouse'}
            </Dialog.Title>
            <Dialog.Close disabled={isSaving} className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 disabled:opacity-40">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          <form onSubmit={handleSubmit} noValidate>
            <div className="px-6 py-5 space-y-4 max-h-[65vh] overflow-y-auto">

              {/* Name + Code */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Name <span className="text-red-500">*</span>
                  </label>
                  <input type="text" value={form.name} onChange={(e) => set('name', e.target.value)}
                    placeholder="Main Warehouse" disabled={isSaving} className={inputCls(errors.name)} />
                  {errors.name && <p className="mt-1 text-xs text-red-500">{errors.name}</p>}
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Code {!isEdit && <span className="text-red-500">*</span>}
                    {isEdit && <span className="text-xs text-gray-400 ml-1">(cannot change)</span>}
                  </label>
                  <input type="text" value={form.code}
                    onChange={(e) => set('code', e.target.value.toUpperCase())}
                    placeholder="WH-01" disabled={isSaving || isEdit} className={inputCls(errors.code)} />
                  {errors.code && <p className="mt-1 text-xs text-red-500">{errors.code}</p>}
                </div>
              </div>

              {/* Address */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Address Line 1 <span className="text-red-500">*</span>
                </label>
                <input type="text" value={form.addressLine1} onChange={(e) => set('addressLine1', e.target.value)}
                  placeholder="123 Industrial Zone" disabled={isSaving} className={inputCls(errors.addressLine1)} />
                {errors.addressLine1 && <p className="mt-1 text-xs text-red-500">{errors.addressLine1}</p>}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Address Line 2
                </label>
                <input type="text" value={form.addressLine2} onChange={(e) => set('addressLine2', e.target.value)}
                  placeholder="Unit B, Block 3" disabled={isSaving} className={inputCls()} />
              </div>

              {/* City / District */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    City <span className="text-red-500">*</span>
                  </label>
                  <input type="text" value={form.city} onChange={(e) => set('city', e.target.value)}
                    placeholder="Dhaka" disabled={isSaving} className={inputCls(errors.city)} />
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

              {/* Postal / Country */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Postal Code <span className="text-red-500">*</span>
                  </label>
                  <input type="text" value={form.postalCode} onChange={(e) => set('postalCode', e.target.value)}
                    placeholder="1216" disabled={isSaving} className={inputCls(errors.postalCode)} />
                  {errors.postalCode && <p className="mt-1 text-xs text-red-500">{errors.postalCode}</p>}
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Country
                  </label>
                  <input type="text" value={form.country} onChange={(e) => set('country', e.target.value)}
                    placeholder="BD" maxLength={2} disabled={isSaving} className={inputCls()} />
                </div>
              </div>

              {/* Phone / Email */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Phone</label>
                  <input type="tel" value={form.phone} onChange={(e) => set('phone', e.target.value)}
                    placeholder="+880 1700 000000" disabled={isSaving} className={inputCls()} />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Email</label>
                  <input type="email" value={form.email} onChange={(e) => set('email', e.target.value)}
                    placeholder="wh@store.com" disabled={isSaving} className={inputCls()} />
                </div>
              </div>

              {/* Default toggle — create only */}
              {!isEdit && (
                <label className="flex items-center gap-3 cursor-pointer select-none">
                  <div
                    onClick={() => set('isDefault', !form.isDefault)}
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
                  <span className="text-sm text-gray-700 dark:text-gray-300">Set as default warehouse</span>
                </label>
              )}
            </div>

            <div className="flex justify-end gap-3 px-6 py-4 border-t border-gray-200 dark:border-gray-700">
              <button type="button" onClick={onClose} disabled={isSaving}
                className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-40">
                Cancel
              </button>
              <button type="submit" disabled={isSaving}
                className="px-4 py-2 text-sm font-medium text-white bg-primary-800 hover:bg-primary-900 rounded-lg transition-colors disabled:opacity-50 flex items-center gap-2">
                {isSaving && <svg className="w-4 h-4 animate-spin" viewBox="0 0 24 24" fill="none"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" /></svg>}
                {isSaving ? 'Saving…' : isEdit ? 'Save Changes' : 'Create Warehouse'}
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
