import { useState, useEffect } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { BadgeDollarSign, X, Loader2, AlertCircle } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { SupplierReturnDetail } from '../../types/supplierReturn';

type Props = {
  open: boolean;
  sr: SupplierReturnDetail | null;
  isSaving?: boolean;
  error?: string | null;
  onSave: (creditAmount: number, creditNoteReference?: string) => void;
  onClose: () => void;
};

export function ReceiveCreditModal({ open, sr, isSaving, error, onSave, onClose }: Props) {
  const [amount, setAmount] = useState('');
  const [reference, setReference] = useState('');
  const [amtErr, setAmtErr] = useState('');

  useEffect(() => {
    if (open) {
      setAmount(sr?.expectedCreditAmount?.toString() ?? '');
      setReference('');
      setAmtErr('');
    }
  }, [open, sr]);

  function handleSubmit(ev: React.FormEvent) {
    ev.preventDefault();
    const n = parseFloat(amount);
    if (!amount || isNaN(n) || n < 0) { setAmtErr('Enter a valid credit amount'); return; }
    onSave(n, reference.trim() || undefined);
  }

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
        <Dialog.Overlay className="fixed inset-0 z-[60] bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <Dialog.Content className={cn(
          'fixed left-1/2 top-1/2 z-[60] -translate-x-1/2 -translate-y-1/2',
          'w-full max-w-md rounded-xl bg-white dark:bg-gray-900',
          'border border-gray-200 dark:border-gray-700 shadow-2xl focus:outline-none',
          'data-[state=open]:animate-in data-[state=closed]:animate-out',
          'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
          'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
        )}>
          <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 px-6 py-4">
            <Dialog.Title className="flex items-center gap-2.5 text-base font-semibold text-gray-900 dark:text-white">
              <BadgeDollarSign className="w-5 h-5 text-emerald-600 flex-shrink-0" />
              Record Credit Received
            </Dialog.Title>
            <Dialog.Description className="sr-only">Record supplier credit for return {sr?.returnNumber}</Dialog.Description>
            <Dialog.Close disabled={isSaving} className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none disabled:opacity-40">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          <form onSubmit={handleSubmit} noValidate>
            <div className="px-6 py-5 space-y-4">
              {error && (
                <div className="flex items-center gap-2 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 px-4 py-3 text-sm text-red-600 dark:text-red-400">
                  <AlertCircle className="w-4 h-4 flex-shrink-0" />
                  {error}
                </div>
              )}

              {sr?.expectedCreditAmount && (
                <div className="rounded-lg bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 px-4 py-3">
                  <p className="text-xs text-blue-600 dark:text-blue-400 font-medium mb-0.5">Expected Credit</p>
                  <p className="text-sm font-semibold text-blue-700 dark:text-blue-300">
                    {sr.expectedCreditAmount.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                  </p>
                </div>
              )}

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Credit Amount Received <span className="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  min={0}
                  step="0.01"
                  value={amount}
                  onChange={(e) => { setAmount(e.target.value); setAmtErr(''); }}
                  placeholder="0.00"
                  disabled={isSaving}
                  className={fieldCls(amtErr)}
                />
                {amtErr && <p className="mt-1 text-xs text-red-500">{amtErr}</p>}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Credit Note Reference <span className="text-xs text-gray-400">(optional)</span>
                </label>
                <input
                  type="text"
                  value={reference}
                  onChange={(e) => setReference(e.target.value)}
                  placeholder="CN-2024-0001"
                  disabled={isSaving}
                  className={fieldCls()}
                />
              </div>
            </div>

            <div className="flex justify-end gap-3 px-6 py-4 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50 rounded-b-xl">
              <button type="button" onClick={onClose} disabled={isSaving} className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors disabled:opacity-40">
                Cancel
              </button>
              <button type="submit" disabled={isSaving} className="flex items-center gap-2 px-5 py-2 text-sm font-medium text-white bg-emerald-700 hover:bg-emerald-800 rounded-lg transition-colors disabled:opacity-50">
                {isSaving && <Loader2 className="w-4 h-4 animate-spin" />}
                Confirm Credit
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
