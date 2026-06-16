import { useEffect, useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { Monitor, X } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { Terminal, TerminalFormData } from '../../types/terminal';
import { TERMINAL_TYPES } from '../../types/terminal';
import { useStoreList } from '../../hooks/useStoreAdmin';

type Props = {
  open: boolean;
  terminal: Terminal | null;
  onSave: (data: TerminalFormData) => void;
  onClose: () => void;
  isSaving?: boolean;
};

const EMPTY: TerminalFormData = {
  storeId: '',
  name: '',
  code: '',
  type: 'Standard',
};

export const TYPE_LABELS: Record<string, string> = {
  Standard: 'Standard',
  SelfCheckout: 'Self-Checkout',
  MobilePOS: 'Mobile POS',
  KioskOrder: 'Kiosk Order',
};

export function TerminalModal({ open, terminal, onSave, onClose, isSaving }: Props) {
  const isEdit = terminal !== null;
  const [form, setForm] = useState<TerminalFormData>(EMPTY);
  const [errors, setErrors] = useState<Partial<Record<keyof TerminalFormData, string>>>({});
  const { data: stores = [] } = useStoreList();

  useEffect(() => {
    if (open) {
      setErrors({});
      setForm(
        isEdit
          ? {
              storeId: terminal.storeId,
              name: terminal.name,
              code: terminal.code,
              type: (terminal.type as TerminalFormData['type']) ?? 'Standard',
            }
          : { ...EMPTY, storeId: stores.find((s) => s.status === 'Active')?.id ?? '' },
      );
    }
  }, [open, terminal]);

  function set<K extends keyof TerminalFormData>(key: K, value: TerminalFormData[K]) {
    setForm((f) => ({ ...f, [key]: value }));
    setErrors((e) => ({ ...e, [key]: undefined }));
  }

  function validate(): boolean {
    const e: Partial<Record<keyof TerminalFormData, string>> = {};
    if (!form.storeId) e.storeId = 'Store is required.';
    if (!form.name.trim()) e.name = 'Name is required.';
    if (!form.code.trim()) e.code = 'Code is required.';
    else if (!/^[A-Za-z0-9_-]+$/.test(form.code)) e.code = 'Code must be alphanumeric.';
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
              <Monitor className="w-5 h-5 text-primary-700" />
              {isEdit ? 'Edit Terminal' : 'Add Terminal'}
            </Dialog.Title>
            <Dialog.Description className="sr-only">
              {isEdit ? `Edit terminal ${terminal?.name}` : 'Add a new terminal'}
            </Dialog.Description>
            <Dialog.Close disabled={isSaving} className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none disabled:opacity-40">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          <form onSubmit={handleSubmit} noValidate>
            <div className="px-6 py-5 space-y-4">
              {/* Store */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Store <span className="text-red-500">*</span>
                </label>
                <select
                  value={form.storeId}
                  onChange={(e) => set('storeId', e.target.value)}
                  disabled={isSaving}
                  className={inputCls(errors.storeId)}
                >
                  <option value="">Select store…</option>
                  {stores.map((s) => (
                    <option key={s.id} value={s.id}>{s.name} ({s.code})</option>
                  ))}
                </select>
                {errors.storeId && <p className="mt-1 text-xs text-red-500">{errors.storeId}</p>}
              </div>

              {/* Name + Code */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Name <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    value={form.name}
                    onChange={(e) => set('name', e.target.value)}
                    placeholder="Till 1"
                    disabled={isSaving}
                    className={inputCls(errors.name)}
                  />
                  {errors.name && <p className="mt-1 text-xs text-red-500">{errors.name}</p>}
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Code <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    value={form.code}
                    onChange={(e) => set('code', e.target.value.toUpperCase())}
                    placeholder="T1"
                    disabled={isSaving}
                    className={cn(inputCls(errors.code), 'font-mono')}
                  />
                  {errors.code && <p className="mt-1 text-xs text-red-500">{errors.code}</p>}
                </div>
              </div>

              {/* Type */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Type
                </label>
                <select
                  value={form.type}
                  onChange={(e) => set('type', e.target.value as TerminalFormData['type'])}
                  disabled={isSaving}
                  className={inputCls()}
                >
                  {TERMINAL_TYPES.map((t) => (
                    <option key={t} value={t}>{TYPE_LABELS[t]}</option>
                  ))}
                </select>
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
                {isSaving ? 'Saving…' : isEdit ? 'Save Changes' : 'Create Terminal'}
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
