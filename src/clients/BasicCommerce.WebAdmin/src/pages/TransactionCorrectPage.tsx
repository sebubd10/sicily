import { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { ShoppingCart, ChevronRight, Loader2, AlertCircle, FilePenLine, Info, Star } from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { Customer } from '../types/customer';
import type { Transaction } from '../types/transaction';
import { useTransactionDetail, useTransactionMutation } from '../hooks/useTransactions';
import * as transactionsApi from '../api/transactionsApi';
import { CustomerPicker } from '../components/transactions/CheckoutWidgets';
import { PaymentMethodIcon } from '../components/transactions/PaymentMethodIcon';
import { MOBILE_WALLET_METHODS, paymentMethodLabel } from '../lib/paymentMethods';

/** Methods with no ledger behind them — the only ones that can be swapped after the sale. */
const SWAPPABLE = ['Cash', 'Card', 'BKash', 'Nagad', 'Rocket'];

function fmtMoney(n: number) {
  return `৳${n.toLocaleString('en-BD', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

/**
 * Non-financial corrections to a completed sale: notes, adding a forgotten customer, and
 * fixing the method a payment was recorded under. Anything that changes what was sold or
 * charged is a return, exchange or void instead.
 */
export default function TransactionCorrectPage() {
  const { id = '' } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: txn, isLoading, isError } = useTransactionDetail(id || null);

  const [notes, setNotes]         = useState('');
  const [customer, setCustomer]   = useState<Customer | null>(null);
  const [methodFix, setMethodFix] = useState<Record<string, { method: string; mobile: string }>>({});
  const [reason, setReason]       = useState('');
  const [error, setError]         = useState('');

  useEffect(() => { if (txn) setNotes(txn.notes ?? ''); }, [txn?.id]);   // eslint-disable-line react-hooks/exhaustive-deps

  const save = useTransactionMutation((t: Transaction) => {
    const paymentCorrections = Object.entries(methodFix)
      .filter(([pid, f]) => t.payments.find((p) => p.id === pid)?.method !== f.method)
      .map(([paymentId, f]) => ({
        paymentId,
        newMethod: f.method,
        mobileNumber: MOBILE_WALLET_METHODS.includes(f.method) ? f.mobile.trim() : null,
      }));
    return transactionsApi.correctTransaction(t.id, {
      reason: reason.trim(),
      updateNotes: (notes.trim() || null) !== (t.notes ?? null),
      notes: notes.trim() || null,
      attachCustomerId: customer?.id ?? null,
      paymentCorrections,
    });
  });

  if (isLoading) {
    return <div className="flex justify-center py-24"><Loader2 className="w-8 h-8 animate-spin text-primary-600" /></div>;
  }
  if (isError || !txn) {
    return <p className="text-center py-24 text-sm text-gray-500">Sale not found.</p>;
  }

  const isCompleted = txn.status === 'Completed';
  const correctable = isCompleted || txn.status === 'Refunded';
  const payments    = txn.payments.filter((p) => p.status === 'Approved');

  const notesChanged    = (notes.trim() || null) !== (txn.notes ?? null);
  const methodChanges   = Object.entries(methodFix)
    .filter(([pid, f]) => payments.find((p) => p.id === pid)?.method !== f.method);
  const missingMobile   = methodChanges.some(([, f]) => MOBILE_WALLET_METHODS.includes(f.method) && !f.mobile.trim());
  const hasChanges      = notesChanged || !!customer || methodChanges.length > 0;
  const canSave         = hasChanges && reason.trim().length > 0 && !missingMobile && !save.isPending;

  function submit() {
    if (!txn) return;
    setError('');
    save.mutate(txn, {
      onSuccess: () => navigate(`/sales/transactions/${txn.id}`),
      onError: (e) => setError(extractApiError(e)),
    });
  }

  const inputCls = 'w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500';

  return (
    <div className="max-w-3xl mx-auto space-y-5">
      <nav className="flex items-center gap-2 text-sm text-gray-500 dark:text-gray-400">
        <Link to="/sales/transactions" className="hover:text-primary-600 flex items-center gap-1">
          <ShoppingCart className="w-3.5 h-3.5" /> Transactions
        </Link>
        <ChevronRight className="w-3.5 h-3.5" />
        <Link to={`/sales/transactions/${txn.id}`} className="hover:text-primary-600 font-mono">{txn.transactionNumber}</Link>
        <ChevronRight className="w-3.5 h-3.5" />
        <span className="text-gray-900 dark:text-white font-semibold">Correct</span>
      </nav>

      <div className="flex items-center gap-3">
        <div className="p-2.5 bg-primary-50 dark:bg-primary-900/20 rounded-xl">
          <FilePenLine className="w-5 h-5 text-primary-600" />
        </div>
        <div>
          <h1 className="text-xl font-bold text-gray-900 dark:text-white">Correct sale</h1>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            For changes that don't affect what was sold or charged. Every change is logged with your reason.
          </p>
        </div>
      </div>

      {!correctable && (
        <div className="p-4 rounded-xl bg-gray-50 dark:bg-gray-800 text-sm text-gray-600 dark:text-gray-300">
          Only completed sales can be corrected. {txn.status === 'Open' || txn.status === 'Suspended'
            ? <Link to={`/sales/transactions/${txn.id}/edit`} className="text-primary-600 hover:underline">Resume this sale</Link>
            : null}
        </div>
      )}

      {correctable && (
        <>
          <div className="flex gap-2 p-3 rounded-xl bg-sky-50 dark:bg-sky-900/20 border border-sky-200 dark:border-sky-800 text-xs text-sky-800 dark:text-sky-300">
            <Info className="w-4 h-4 flex-shrink-0" />
            Wrong items, quantities or prices? Use{' '}
            <Link to={`/sales/transactions/${txn.id}/exchange`} className="font-semibold underline">Exchange</Link>,{' '}
            <Link to={`/sales/transactions/${txn.id}/return`} className="font-semibold underline">Return</Link>{' '}
            or Void &amp; re-ring from the sale page.
          </div>

          {/* Notes */}
          <Section title="Notes">
            <textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)}
              placeholder="Internal note about this sale" className={cn(inputCls, 'resize-none')} />
          </Section>

          {/* Customer */}
          <Section title="Customer">
            {txn.customerId ? (
              <p className="text-sm text-gray-700 dark:text-gray-300">
                <b>{txn.customerName ?? 'Customer'}</b> — a sale's customer can't be changed once it's completed.
              </p>
            ) : !isCompleted ? (
              <p className="text-sm text-gray-500">A customer can only be added to a sale with no returns against it.</p>
            ) : (
              <div className="space-y-2">
                <CustomerPicker value={customer} onChange={setCustomer} />
                <p className="text-xs text-gray-500 dark:text-gray-400 flex items-center gap-1">
                  <Star className="w-3 h-3 text-amber-500" />
                  The customer receives the reward points this sale would have earned.
                </p>
              </div>
            )}
          </Section>

          {/* Payment method fixes */}
          <Section title="Payment method">
            {!isCompleted || payments.length === 0 ? (
              <p className="text-sm text-gray-500">No payments can be changed on this sale.</p>
            ) : (
              <div className="space-y-2">
                {payments.map((p) => {
                  const swappable = SWAPPABLE.includes(p.method);
                  const fix = methodFix[p.id] ?? { method: p.method, mobile: '' };
                  return (
                    <div key={p.id} className="flex flex-wrap items-center gap-3 px-3 py-2.5 bg-gray-50 dark:bg-gray-800 rounded-xl">
                      <PaymentMethodIcon method={p.method} size={18} />
                      <span className="text-sm font-bold tabular-nums text-gray-900 dark:text-white w-24">{fmtMoney(p.amount)}</span>
                      {swappable ? (
                        <>
                          <select
                            value={fix.method}
                            onChange={(e) => setMethodFix((m) => ({ ...m, [p.id]: { ...fix, method: e.target.value } }))}
                            className={cn(inputCls, 'w-36 py-1.5')}
                          >
                            {SWAPPABLE.map((m) => <option key={m} value={m}>{paymentMethodLabel(m)}</option>)}
                          </select>
                          {MOBILE_WALLET_METHODS.includes(fix.method) && fix.method !== p.method && (
                            <input
                              value={fix.mobile}
                              onChange={(e) => setMethodFix((m) => ({ ...m, [p.id]: { ...fix, mobile: e.target.value } }))}
                              placeholder="Mobile number *"
                              className={cn(inputCls, 'w-40 py-1.5')}
                            />
                          )}
                        </>
                      ) : (
                        <span className="text-xs text-gray-500">
                          {paymentMethodLabel(p.method)} — can't be switched (use void or return)
                        </span>
                      )}
                    </div>
                  );
                })}
                <p className="text-xs text-gray-400">
                  Only while the till session that took this sale is still open; the amount can't change.
                </p>
              </div>
            )}
          </Section>

          {/* Reason */}
          <Section title="Reason *">
            <input value={reason} onChange={(e) => setReason(e.target.value)}
              placeholder="e.g. Customer forgot their loyalty card; paid by bKash not card"
              className={inputCls} />
          </Section>

          {error && (
            <div className="flex items-start gap-2 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-xl">
              <AlertCircle className="w-4 h-4 text-red-500 flex-shrink-0 mt-0.5" />
              <p className="text-sm text-red-700 dark:text-red-400">{error}</p>
            </div>
          )}

          <div className="flex justify-end gap-3">
            <Link to={`/sales/transactions/${txn.id}`}
              className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-800 rounded-xl">
              Cancel
            </Link>
            <button onClick={submit} disabled={!canSave}
              className="flex items-center gap-2 px-5 py-2 text-sm font-semibold text-white bg-primary-700 hover:bg-primary-800 rounded-xl disabled:opacity-40">
              {save.isPending && <Loader2 className="w-4 h-4 animate-spin" />}
              Save correction
            </button>
          </div>
        </>
      )}
    </div>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm p-5 space-y-3">
      <h2 className="text-sm font-semibold text-gray-900 dark:text-white">{title}</h2>
      {children}
    </div>
  );
}
