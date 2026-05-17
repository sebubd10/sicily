import { useEffect, useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { ShieldCheck, X } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { UserType, UserTypeFormData } from '../../types/userType';

type Props = {
  open: boolean;
  userType: UserType | null;
  onSave: (data: UserTypeFormData) => void;
  onClose: () => void;
  isSaving?: boolean;
};

const EMPTY: UserTypeFormData = {
  name: '',
  description: '',
  sortOrder: 1,
  color: '',
};

const PRESET_COLORS = [
  '#1565C0', '#2E7D32', '#6A1B9A', '#C62828', '#E65100',
  '#00838F', '#4527A0', '#283593', '#558B2F', '#F57F17',
];

export function UserTypeModal({ open, userType, onSave, onClose, isSaving }: Props) {
  const isEdit = userType !== null;
  const [form, setForm] = useState<UserTypeFormData>(EMPTY);
  const [errors, setErrors] = useState<Partial<Record<keyof UserTypeFormData, string>>>({});

  useEffect(() => {
    if (open) {
      setErrors({});
      setForm(
        isEdit
          ? {
              name: userType.name,
              description: userType.description ?? '',
              sortOrder: userType.sortOrder,
              color: userType.color ?? '',
            }
          : EMPTY,
      );
    }
  }, [open, userType]);

  function set<K extends keyof UserTypeFormData>(key: K, value: UserTypeFormData[K]) {
    setForm((f) => ({ ...f, [key]: value }));
    setErrors((e) => ({ ...e, [key]: undefined }));
  }

  function validate(): boolean {
    const e: Partial<Record<keyof UserTypeFormData, string>> = {};
    if (!form.name.trim()) e.name = 'Name is required.';
    else if (form.name.length > 100) e.name = 'Name must be 100 characters or less.';
    if (form.sortOrder < 0) e.sortOrder = 'Sort order must be 0 or greater.';
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
      err
        ? 'border-red-400 dark:border-red-500'
        : 'border-gray-200 dark:border-gray-700',
    );

  return (
    <Dialog.Root open={open} onOpenChange={(o) => !o && !isSaving && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <Dialog.Content
          className={cn(
            'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
            'w-full max-w-lg rounded-xl bg-white dark:bg-gray-900',
            'border border-gray-200 dark:border-gray-700 shadow-xl',
            'focus:outline-none',
            'data-[state=open]:animate-in data-[state=closed]:animate-out',
            'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
            'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
            'data-[state=closed]:slide-out-to-left-1/2 data-[state=closed]:slide-out-to-top-48%',
            'data-[state=open]:slide-in-from-left-1/2 data-[state=open]:slide-in-from-top-48%',
          )}
        >
          <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 px-6 py-4">
            <Dialog.Title className="flex items-center gap-2 text-base font-semibold text-gray-900 dark:text-white">
              <ShieldCheck className="w-5 h-5 text-primary-700" />
              {isEdit ? 'Edit User Type' : 'Add User Type'}
            </Dialog.Title>
            <Dialog.Close
              disabled={isSaving}
              className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 disabled:opacity-40"
            >
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          <form onSubmit={handleSubmit} noValidate>
            <div className="px-6 py-5 space-y-4">

              {isEdit && userType?.isSystem && (
                <div className="flex items-center gap-2 px-3 py-2 rounded-lg bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800 text-xs text-amber-700 dark:text-amber-300">
                  <span className="font-medium">System type</span> — some fields may be restricted.
                </div>
              )}

              {/* Name */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={form.name}
                  onChange={(e) => set('name', e.target.value)}
                  placeholder="e.g. Warehouse Lead"
                  disabled={isSaving}
                  className={inputCls(errors.name)}
                />
                {errors.name && <p className="mt-1 text-xs text-red-500">{errors.name}</p>}
              </div>

              {/* Description */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Description
                </label>
                <textarea
                  value={form.description}
                  onChange={(e) => set('description', e.target.value)}
                  placeholder="Brief description of this role profile..."
                  rows={3}
                  disabled={isSaving}
                  className={cn(inputCls(), 'resize-none')}
                />
              </div>

              {/* Sort Order */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Sort Order
                </label>
                <input
                  type="number"
                  min={0}
                  value={form.sortOrder}
                  onChange={(e) => set('sortOrder', Number(e.target.value))}
                  disabled={isSaving}
                  className={inputCls(errors.sortOrder)}
                />
                {errors.sortOrder && <p className="mt-1 text-xs text-red-500">{errors.sortOrder}</p>}
              </div>

              {/* Color */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Color <span className="text-xs text-gray-400">(optional — used for badges)</span>
                </label>
                <div className="flex items-center gap-2 flex-wrap">
                  {PRESET_COLORS.map((c) => (
                    <button
                      key={c}
                      type="button"
                      onClick={() => set('color', form.color === c ? '' : c)}
                      disabled={isSaving}
                      className={cn(
                        'w-6 h-6 rounded-full ring-offset-2 ring-offset-white dark:ring-offset-gray-900 transition-all',
                        form.color === c ? 'ring-2 ring-gray-700 scale-110' : 'hover:scale-110',
                      )}
                      style={{ backgroundColor: c }}
                      title={c}
                    />
                  ))}
                  <div className="flex items-center gap-1.5">
                    <input
                      type="color"
                      value={form.color || '#000000'}
                      onChange={(e) => set('color', e.target.value)}
                      disabled={isSaving}
                      className="w-7 h-7 rounded cursor-pointer border border-gray-200 dark:border-gray-700 p-0.5"
                      title="Custom color"
                    />
                    {form.color && (
                      <button
                        type="button"
                        onClick={() => set('color', '')}
                        disabled={isSaving}
                        className="text-xs text-gray-400 hover:text-gray-600 dark:hover:text-gray-200"
                      >
                        Clear
                      </button>
                    )}
                  </div>
                </div>
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
                {isSaving ? 'Saving…' : isEdit ? 'Save Changes' : 'Create'}
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
