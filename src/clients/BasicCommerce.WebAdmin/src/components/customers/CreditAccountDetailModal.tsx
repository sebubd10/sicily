import { useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import {
  X, CreditCard, ArrowDownCircle, ArrowUpCircle,
  AlertCircle, Loader2, CheckCircle, User, Store, BadgeCheck,
} from 'lucide-react';
import { cn, extractApiError } from '../../lib/utils';
import { useCreditAccountDetail, useRecordCreditPayment } from '../../hooks/useCustomers';

type Props = {
  creditAccountId: string | null;
  open: boolean;
  onClose: () => void;
};

function fmtMoney(n: number) {
  return `৳${n.toLocaleString('en-BD', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

function fmtDate(d: string | null | undefined) {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

function fmtDateTime(d: string) {
  return new Date(d).toLocaleString('en-GB', {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
}

function UtilBar({ used, limit }: { used: number; limit: number }) {
  const pct = limit > 0 ? Math.min(100, (used / limit) * 100) : 0;
  const color = pct >= 90 ? 'bg-red-500' : pct >= 70 ? 'bg-amber-500' : 'bg-emerald-500';
  return (
    <div className="w-full bg-gray-100 dark:bg-gray-700 rounded-full h-1.5 overflow-hidden">
      <div className={cn('h-full rounded-full transition-all', color)} style={{ width: `${pct}%` }} />
    </div>
  );
}

export function CreditAccountDetailModal({ creditAccountId, open, onClose }: Props) {
  const [amount, setAmount] = useState('');
  const [reference, setReference] = useState('');
  const [success, setSuccess] = useState('');

  const { data: account, isLoading, isError, error: fetchError } = useCreditAccountDetail(
    open ? creditAccountId : null,
  );
  const { mutate: recordPayment, isPending, error: payError } = useRecordCreditPayment();

  const apiError = payError ? extractApiError(payError) : null;
  const fetchErr = isError ? extractApiError(fetchError) : null;

  function handleClose() {
    setAmount('');
    setReference('');
    setSuccess('');
    onClose();
  }

  function handlePayment(e: React.FormEvent) {
    e.preventDefault();
    if (!creditAccountId) return;
    const amt = parseFloat(amount);
    if (isNaN(amt) || amt <= 0) return;
    recordPayment(
      { creditAccountId, amount: amt, reference: reference.trim() || null },
      {
        onSuccess: () => {
          setAmount('');
          setReference('');
          setSuccess(`Payment of ${fmtMoney(amt)} recorded successfully.`);
          setTimeout(() => setSuccess(''), 4000);
        },
      },
    );
  }

  const usedPct = account
    ? account.creditLimit > 0
      ? Math.min(100, (account.outstandingBalance / account.creditLimit) * 100)
      : 0
    : 0;

  return (
    <Dialog.Root open={open} onOpenChange={(v) => !v && handleClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 bg-black/50 backdrop-blur-sm z-40 animate-in fade-in-0 duration-200" />
        <Dialog.Content className="fixed left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 z-50 w-full max-w-2xl bg-white dark:bg-gray-900 rounded-2xl shadow-2xl flex flex-col max-h-[90vh]">

          {/* Header */}
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-gray-800">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-xl bg-blue-100 dark:bg-blue-900/30">
                <CreditCard className="w-5 h-5 text-blue-600 dark:text-blue-400" />
              </div>
              <div>
                <Dialog.Title className="text-base font-semibold text-gray-900 dark:text-gray-100">
                  Credit Account
                </Dialog.Title>
                {account && (
                  <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                    {account.customerName} · {account.storeName}
                  </p>
                )}
              </div>
            </div>
            <Dialog.Close
              onClick={handleClose}
              className="p-1.5 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
            >
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          {/* Body */}
          <div className="flex-1 overflow-y-auto px-6 py-5 space-y-5">
            {isLoading && (
              <div className="flex items-center justify-center py-16">
                <Loader2 className="w-6 h-6 text-indigo-500 animate-spin" />
              </div>
            )}

            {fetchErr && (
              <div className="flex items-center gap-2 p-3 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800">
                <AlertCircle className="w-4 h-4 text-red-500 flex-shrink-0" />
                <p className="text-sm text-red-700 dark:text-red-300">{fetchErr}</p>
              </div>
            )}

            {account && (
              <>
                {/* Context chips */}
                <div className="flex items-center gap-3 flex-wrap">
                  <div className="flex items-center gap-1.5 text-xs text-gray-500 dark:text-gray-400 bg-gray-100 dark:bg-gray-800 px-2.5 py-1 rounded-full">
                    <User className="w-3 h-3" />
                    {account.customerName}
                    <span className="text-gray-400">·</span>
                    {account.customerCode}
                  </div>
                  <div className="flex items-center gap-1.5 text-xs text-gray-500 dark:text-gray-400 bg-gray-100 dark:bg-gray-800 px-2.5 py-1 rounded-full">
                    <Store className="w-3 h-3" />
                    {account.storeName}
                  </div>
                  <span className={cn(
                    'text-xs px-2.5 py-1 rounded-full font-medium',
                    account.status === 'Active'
                      ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400'
                      : 'bg-gray-100 text-gray-500 dark:bg-gray-800',
                  )}>
                    {account.status}
                  </span>
                </div>

                {/* Credit utilisation */}
                <div className="bg-gray-50 dark:bg-gray-800 rounded-xl p-4 space-y-3">
                  <div className="flex items-center justify-between text-xs text-gray-500 dark:text-gray-400">
                    <span>Credit utilisation</span>
                    <span className={cn(
                      'font-semibold',
                      usedPct >= 90 ? 'text-red-600 dark:text-red-400' :
                      usedPct >= 70 ? 'text-amber-600 dark:text-amber-400' :
                      'text-emerald-600 dark:text-emerald-400',
                    )}>
                      {usedPct.toFixed(1)}%
                    </span>
                  </div>
                  <UtilBar used={account.outstandingBalance} limit={account.creditLimit} />
                  <div className="grid grid-cols-3 gap-3 pt-1">
                    {[
                      { label: 'Credit Limit', value: fmtMoney(account.creditLimit), color: 'text-gray-900 dark:text-gray-100' },
                      { label: 'Outstanding', value: fmtMoney(account.outstandingBalance), color: account.outstandingBalance > 0 ? 'text-red-600 dark:text-red-400' : 'text-gray-900 dark:text-gray-100' },
                      { label: 'Available', value: fmtMoney(account.availableCredit), color: 'text-emerald-600 dark:text-emerald-400' },
                    ].map(({ label, value, color }) => (
                      <div key={label} className="text-center">
                        <p className="text-xs text-gray-400 mb-0.5">{label}</p>
                        <p className={cn('text-base font-bold tabular-nums', color)}>{value}</p>
                      </div>
                    ))}
                  </div>
                  <p className="text-xs text-gray-400 text-center">
                    Last payment: {fmtDate(account.lastPaymentAt)}
                  </p>
                </div>

                {/* Record Payment */}
                {account.outstandingBalance > 0 && (
                  <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 overflow-hidden">
                    <div className="px-4 py-3 bg-emerald-50 dark:bg-emerald-900/20 border-b border-emerald-100 dark:border-emerald-800">
                      <p className="text-xs font-semibold text-emerald-700 dark:text-emerald-400 uppercase tracking-wider">
                        Record Payment
                      </p>
                    </div>
                    <form onSubmit={handlePayment} className="px-4 py-4 space-y-3">
                      {apiError && (
                        <div className="flex items-center gap-2 text-xs text-red-600 dark:text-red-400">
                          <AlertCircle className="w-3.5 h-3.5 flex-shrink-0" />
                          {apiError}
                        </div>
                      )}
                      {success && (
                        <div className="flex items-center gap-2 text-xs text-emerald-600 dark:text-emerald-400">
                          <CheckCircle className="w-3.5 h-3.5 flex-shrink-0" />
                          {success}
                        </div>
                      )}
                      <div className="grid grid-cols-2 gap-3">
                        <div>
                          <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">
                            Amount *
                          </label>
                          <div className="relative">
                            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm">৳</span>
                            <input
                              type="number"
                              min="0.01"
                              step="0.01"
                              required
                              className="w-full pl-8 pr-3 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
                              placeholder="0.00"
                              value={amount}
                              onChange={(e) => setAmount(e.target.value)}
                            />
                          </div>
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">
                            Reference / Cheque No.
                          </label>
                          <input
                            className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
                            placeholder="Optional"
                            value={reference}
                            onChange={(e) => setReference(e.target.value)}
                          />
                        </div>
                      </div>
                      {amount && parseFloat(amount) > 0 && (
                        <p className="text-xs text-gray-400">
                          Remaining after payment: {fmtMoney(Math.max(0, account.outstandingBalance - parseFloat(amount)))}
                        </p>
                      )}
                      <button
                        type="submit"
                        disabled={isPending || !amount}
                        className="flex items-center gap-2 px-4 py-2 rounded-lg bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white text-sm font-medium transition-colors"
                      >
                        {isPending
                          ? <Loader2 className="w-3.5 h-3.5 animate-spin" />
                          : <ArrowDownCircle className="w-3.5 h-3.5" />}
                        {isPending ? 'Recording…' : 'Record Payment'}
                      </button>
                    </form>
                  </div>
                )}

                {/* Transaction history */}
                <div>
                  <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-3">
                    Transaction History
                    <span className="ml-2 font-normal normal-case text-gray-400">
                      (last {account.recentHistory.length} entries)
                    </span>
                  </p>

                  {account.recentHistory.length === 0 ? (
                    <div className="text-center py-8 text-gray-400 dark:text-gray-500 text-sm">
                      No transactions yet
                    </div>
                  ) : (
                    <div className="rounded-xl border border-gray-100 dark:border-gray-800 overflow-hidden">
                      <table className="w-full text-sm">
                        <thead>
                          <tr className="bg-gray-50 dark:bg-gray-800/50 border-b border-gray-100 dark:border-gray-800">
                            <th className="px-4 py-2.5 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Date</th>
                            <th className="px-4 py-2.5 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Description</th>
                            <th className="px-4 py-2.5 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Reference</th>
                            <th className="px-4 py-2.5 text-right text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Amount</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-50 dark:divide-gray-800">
                          {account.recentHistory.map((tx) => {
                            const isPayment = tx.type === 'Payment';
                            return (
                              <tr key={tx.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors">
                                <td className="px-4 py-2.5 text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">
                                  {fmtDateTime(tx.createdAt)}
                                </td>
                                <td className="px-4 py-2.5">
                                  <div className="flex items-center gap-2">
                                    {isPayment
                                      ? <ArrowDownCircle className="w-3.5 h-3.5 text-emerald-500 flex-shrink-0" />
                                      : <ArrowUpCircle className="w-3.5 h-3.5 text-red-500 flex-shrink-0" />}
                                    <span className="text-sm text-gray-800 dark:text-gray-200">
                                      {tx.description}
                                    </span>
                                  </div>
                                </td>
                                <td className="px-4 py-2.5 text-xs text-gray-400 dark:text-gray-500">
                                  {tx.reference ?? '—'}
                                </td>
                                <td className="px-4 py-2.5 text-right font-semibold tabular-nums whitespace-nowrap">
                                  <span className={isPayment ? 'text-emerald-600 dark:text-emerald-400' : 'text-red-600 dark:text-red-400'}>
                                    {isPayment ? '-' : '+'}{fmtMoney(tx.amount)}
                                  </span>
                                </td>
                              </tr>
                            );
                          })}
                        </tbody>
                      </table>
                    </div>
                  )}
                </div>
              </>
            )}
          </div>

          {/* Footer */}
          <div className="px-6 py-3 border-t border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50 flex justify-end">
            <Dialog.Close
              onClick={handleClose}
              className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100 font-medium transition-colors"
            >
              Close
            </Dialog.Close>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
