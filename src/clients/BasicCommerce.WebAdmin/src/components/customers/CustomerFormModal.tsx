import { useState, useEffect } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { UserPlus, X, AlertCircle } from 'lucide-react';
import { cn, extractApiError } from '../../lib/utils';
import type { Customer, CustomerFormState } from '../../types/customer';
import { useRegisterCustomer, useUpdateCustomer } from '../../hooks/useCustomers';

type Props = {
  open: boolean;
  onClose: () => void;
  editCustomer?: Customer | null;
};

const EMPTY: CustomerFormState = {
  name: '',
  email: '',
  phone: '',
  creditLimit: '0',
  addressLine1: '',
  city: '',
  district: '',
  postalCode: '',
};

const inputCls =
  'w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 ' +
  'px-3 py-2 text-sm text-gray-900 dark:text-gray-100 placeholder-gray-400 ' +
  'focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent';

const labelCls = 'block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1';

function toForm(c: Customer): CustomerFormState {
  return {
    name: c.name,
    email: c.email ?? '',
    phone: c.phone ?? '',
    creditLimit: String(c.creditLimit),
    addressLine1: c.addressLine1 ?? '',
    city: c.city ?? '',
    district: '',
    postalCode: '',
  };
}

export function CustomerFormModal({ open, onClose, editCustomer }: Props) {
  const [form, setForm] = useState<CustomerFormState>(EMPTY);
  const [errors, setErrors] = useState<Partial<Record<keyof CustomerFormState, string>>>({});

  const { mutate: register, isPending: registering, error: regError } = useRegisterCustomer();
  const { mutate: update, isPending: updating, error: updateError } = useUpdateCustomer();

  const isSaving = registering || updating;
  const isEdit = !!editCustomer;
  const apiError = regError ? extractApiError(regError) : updateError ? extractApiError(updateError) : null;

  useEffect(() => {
    if (open) {
      setForm(editCustomer ? toForm(editCustomer) : EMPTY);
      setErrors({});
    }
  }, [open, editCustomer]);

  function set(field: keyof CustomerFormState, value: string) {
    setForm((f) => ({ ...f, [field]: value }));
    setErrors((e) => ({ ...e, [field]: undefined }));
  }

  function validate(): boolean {
    const e: Partial<Record<keyof CustomerFormState, string>> = {};
    if (!form.name.trim()) e.name = 'Name is required';
    if (form.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email))
      e.email = 'Enter a valid email';
    if (form.phone && !/^\+?[0-9\s\-]{7,20}$/.test(form.phone))
      e.phone = 'Enter a valid phone number';
    if (!isEdit && Number(form.creditLimit) < 0)
      e.creditLimit = 'Credit limit cannot be negative';
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  function handleSubmit() {
    if (!validate()) return;
    const payload = {
      name: form.name.trim(),
      email: form.email.trim() || null,
      phone: form.phone.trim() || null,
      addressLine1: form.addressLine1.trim() || null,
      city: form.city.trim() || null,
      district: form.district.trim() || null,
      postalCode: form.postalCode.trim() || null,
    };

    if (isEdit) {
      update({ id: editCustomer.id, payload }, { onSuccess: onClose });
    } else {
      register(
        { ...payload, creditLimit: Number(form.creditLimit) || 0 },
        { onSuccess: onClose },
      );
    }
  }

  return (
    <Dialog.Root open={open} onOpenChange={(v) => !v && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 bg-black/50 backdrop-blur-sm z-40 animate-in fade-in-0 duration-200" />
        <Dialog.Content className="fixed left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 z-50 w-full max-w-lg bg-white dark:bg-gray-900 rounded-2xl shadow-2xl flex flex-col max-h-[90vh] overflow-hidden">
          {/* Header */}
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-gray-800">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-xl bg-indigo-100 dark:bg-indigo-900/30">
                <UserPlus className="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
              </div>
              <Dialog.Title className="text-base font-semibold text-gray-900 dark:text-gray-100">
                {isEdit ? 'Edit Customer' : 'New Customer'}
              </Dialog.Title>
            </div>
            <Dialog.Close className="p-1.5 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          {/* Body */}
          <div className="flex-1 overflow-y-auto px-6 py-4 space-y-4">
            {apiError && (
              <div className="flex items-start gap-2 p-3 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800">
                <AlertCircle className="w-4 h-4 text-red-500 flex-shrink-0 mt-0.5" />
                <p className="text-sm text-red-700 dark:text-red-300">{apiError}</p>
              </div>
            )}

            {/* Name */}
            <div>
              <label className={labelCls}>Full Name *</label>
              <input
                className={cn(inputCls, errors.name && 'border-red-400 focus:ring-red-500')}
                placeholder="e.g. Rahim Uddin"
                value={form.name}
                onChange={(e) => set('name', e.target.value)}
              />
              {errors.name && <p className="text-xs text-red-500 mt-1">{errors.name}</p>}
            </div>

            {/* Phone + Email */}
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className={labelCls}>Phone</label>
                <input
                  className={cn(inputCls, errors.phone && 'border-red-400 focus:ring-red-500')}
                  placeholder="+8801700000000"
                  value={form.phone}
                  onChange={(e) => set('phone', e.target.value)}
                />
                {errors.phone && <p className="text-xs text-red-500 mt-1">{errors.phone}</p>}
              </div>
              <div>
                <label className={labelCls}>Email</label>
                <input
                  type="email"
                  className={cn(inputCls, errors.email && 'border-red-400 focus:ring-red-500')}
                  placeholder="example@email.com"
                  value={form.email}
                  onChange={(e) => set('email', e.target.value)}
                />
                {errors.email && <p className="text-xs text-red-500 mt-1">{errors.email}</p>}
              </div>
            </div>

            {/* Credit limit — only on create */}
            {!isEdit && (
              <div>
                <label className={labelCls}>Credit Limit (৳)</label>
                <div className="relative">
                  <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm">৳</span>
                  <input
                    type="number"
                    min="0"
                    className={cn(inputCls, 'pl-8', errors.creditLimit && 'border-red-400 focus:ring-red-500')}
                    placeholder="0"
                    value={form.creditLimit}
                    onChange={(e) => set('creditLimit', e.target.value)}
                  />
                </div>
                {errors.creditLimit && (
                  <p className="text-xs text-red-500 mt-1">{errors.creditLimit}</p>
                )}
                <p className="text-xs text-gray-400 mt-1">Leave 0 for no credit account</p>
              </div>
            )}

            {/* Address section */}
            <div className="pt-2 border-t border-gray-100 dark:border-gray-800">
              <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-3">
                Address (optional)
              </p>
              <div className="space-y-3">
                <div>
                  <label className={labelCls}>Address Line 1</label>
                  <input
                    className={inputCls}
                    placeholder="House/Road"
                    value={form.addressLine1}
                    onChange={(e) => set('addressLine1', e.target.value)}
                  />
                </div>
                <div className="grid grid-cols-3 gap-3">
                  <div>
                    <label className={labelCls}>City</label>
                    <input
                      className={inputCls}
                      placeholder="Dhaka"
                      value={form.city}
                      onChange={(e) => set('city', e.target.value)}
                    />
                  </div>
                  <div>
                    <label className={labelCls}>District</label>
                    <input
                      className={inputCls}
                      placeholder="Dhaka"
                      value={form.district}
                      onChange={(e) => set('district', e.target.value)}
                    />
                  </div>
                  <div>
                    <label className={labelCls}>Postal Code</label>
                    <input
                      className={inputCls}
                      placeholder="1205"
                      value={form.postalCode}
                      onChange={(e) => set('postalCode', e.target.value)}
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Footer */}
          <div className="flex items-center justify-end gap-3 px-6 py-4 border-t border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100 font-medium transition-colors"
            >
              Cancel
            </button>
            <button
              type="button"
              onClick={handleSubmit}
              disabled={isSaving}
              className="flex items-center gap-2 px-5 py-2 rounded-lg bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-sm font-medium transition-colors"
            >
              {isSaving ? 'Saving…' : isEdit ? 'Save Changes' : 'Register Customer'}
            </button>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
