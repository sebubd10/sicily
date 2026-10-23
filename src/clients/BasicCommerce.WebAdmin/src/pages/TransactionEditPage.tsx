import { useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import {
  ShoppingCart, ChevronRight, Plus, Minus, Trash2, Loader2, AlertCircle, X,
  BadgePercent, PauseCircle, PlayCircle, CheckCircle2, User, Receipt, Lock,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { Customer } from '../types/customer';
import type { Transaction } from '../types/transaction';
import type { ProductListItem } from '../types/product';
import { useTransactionDetail, useTransactionMutation } from '../hooks/useTransactions';
import { useCheckoutRewardPoints } from '../hooks/useRewardPoints';
import * as transactionsApi from '../api/transactionsApi';
import { PaymentMethodIcon } from '../components/transactions/PaymentMethodIcon';
import { ProductSearch, CustomerPicker, RewardPointsStrip } from '../components/transactions/CheckoutWidgets';
import { PAYMENT_METHODS, MOBILE_WALLET_METHODS, paymentMethodLabel } from '../lib/paymentMethods';

function fmtMoney(n: number) {
  return `৳${n.toLocaleString('en-BD', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

/**
 * Resume an unfinished sale (Open or on hold): add/remove items, change quantities and the
 * customer, take or remove payments, then complete. Every change is saved immediately.
 */
export default function TransactionEditPage() {
  const { id = '' } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: txn, isLoading, isError } = useTransactionDetail(id || null);

  const [error, setError]       = useState('');
  const [method, setMethod]     = useState('Cash');
  const [amount, setAmount]     = useState('');
  const [mobile, setMobile]     = useState('');
  const [giftCode, setGiftCode] = useState('');

  const run = useTransactionMutation(async (fn: () => Promise<Transaction>) => fn());
  const busy = run.isPending;

  function act(fn: () => Promise<Transaction>, after?: () => void) {
    setError('');
    run.mutate(fn, {
      onSuccess: () => after?.(),
      onError: (e) => setError(extractApiError(e)),
    });
  }

  const activeLines = txn?.lineItems.filter((l) => !l.isVoided) ?? [];
  const payments    = txn?.payments.filter((p) => p.status === 'Approved') ?? [];
  const balanceDue  = txn ? Math.round((txn.total - txn.amountPaid) * 100) / 100 : 0;
  const pointsPaid  = payments.filter((p) => p.method === 'RewardPoints').reduce((s, p) => s + p.amount, 0);

  const { data: points } = useCheckoutRewardPoints({
    customerId:   txn?.customerId ?? null,
    storeId:      txn?.storeId ?? '',
    orderTotal:   txn?.total ?? 0,
    pointsAmount: pointsPaid,
  });

  if (isLoading) {
    return (
      <div className="flex flex-col items-center justify-center h-64 gap-3">
        <Loader2 className="w-8 h-8 animate-spin text-primary-600" />
        <p className="text-sm text-gray-500 dark:text-gray-400">Loading sale…</p>
      </div>
    );
  }
  if (isError || !txn) {
    return (
      <div className="flex flex-col items-center justify-center h-64 gap-3">
        <AlertCircle className="w-8 h-8 text-red-500" />
        <p className="text-sm text-gray-600 dark:text-gray-400">Sale not found or failed to load.</p>
        <Link to="/sales/transactions" className="text-sm text-primary-600 hover:underline">Back to transactions</Link>
      </div>
    );
  }

  const isOpen      = txn.status === 'Open';
  const isSuspended = txn.status === 'Suspended';

  if (!isOpen && !isSuspended) {
    return (
      <div className="max-w-lg mx-auto mt-16 text-center space-y-3">
        <Lock className="w-8 h-8 text-gray-400 mx-auto" />
        <p className="text-gray-700 dark:text-gray-300 font-medium">
          This sale is {txn.status.toLowerCase()} and can't be edited here.
        </p>
        <p className="text-sm text-gray-500 dark:text-gray-400">
          Completed sales are changed with a correction, return, exchange, or void &amp; re-ring.
        </p>
        <Link to={`/sales/transactions/${txn.id}`} className="inline-block text-sm text-primary-600 hover:underline">
          Open the sale
        </Link>
      </div>
    );
  }

  const disabledMethods: Record<string, string> = {};
  if (!txn.customerId) {
    disabledMethods.Credit       = 'Attach a customer to sell on credit';
    disabledMethods.RewardPoints = 'Attach a customer to pay with points';
  } else if (points && (!points.programEnabled || points.maxRedeemableAmount <= 0)) {
    disabledMethods.RewardPoints = !points.programEnabled
      ? 'The reward points programme is turned off'
      : 'No points available to use on this sale';
  }

  const needsMobile   = MOBILE_WALLET_METHODS.includes(method);
  const needsGiftCard = method === 'GiftCard';
  const amountNum     = parseFloat(amount) || 0;

  function pickMethod(m: string) {
    if (disabledMethods[m]) return;
    setMethod(m);
    setMobile(''); setGiftCode('');
    setAmount(Math.max(balanceDue, 0).toFixed(2));
  }

  function addPayment() {
    if (!txn) return;
    if (amountNum <= 0) { setError('Enter an amount greater than zero.'); return; }
    if (needsMobile && !mobile.trim()) { setError('Mobile number is required.'); return; }
    if (needsGiftCard && !giftCode.trim()) { setError('Gift card code is required.'); return; }
    act(() => transactionsApi.addPayment(txn.id, method, amountNum, undefined,
      needsGiftCard ? giftCode.trim() : undefined, needsMobile ? mobile.trim() : undefined),
    () => { setAmount(''); setMobile(''); setGiftCode(''); });
  }

  function onCustomer(c: Customer | null) {
    if (!txn) return;
    act(() => c ? transactionsApi.attachCustomer(txn.id, c.id) : transactionsApi.detachCustomer(txn.id));
  }

  function onProduct(p: ProductListItem) {
    if (!txn) return;
    const existing = activeLines.find((l) => l.productSku === p.sku && !l.isPriceOverridden);
    act(() => existing
      ? transactionsApi.changeLineItemQuantity(txn.id, existing.id, existing.quantity + 1)
      : transactionsApi.addLineItem(txn.id, p.id, 1));
  }

  const canComplete = isOpen && activeLines.length > 0 && balanceDue <= 0.005 && !busy;

  return (
    <div className="max-w-7xl mx-auto space-y-4">
      {/* Breadcrumb */}
      <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-3">
        <nav className="flex items-center gap-2 text-sm text-gray-500 dark:text-gray-400">
          <Link to="/sales/transactions" className="hover:text-primary-600 transition-colors flex items-center gap-1">
            <ShoppingCart className="w-3.5 h-3.5" /> Transactions
          </Link>
          <ChevronRight className="w-3.5 h-3.5" />
          <Link to={`/sales/transactions/${txn.id}`} className="hover:text-primary-600 font-mono">
            {txn.transactionNumber}
          </Link>
          <ChevronRight className="w-3.5 h-3.5" />
          <span className="text-gray-900 dark:text-white font-semibold flex items-center gap-1.5">
            <Receipt className="w-4 h-4 text-primary-600" /> Resume sale
          </span>
        </nav>
        <div className="flex items-center gap-2">
          {busy && <Loader2 className="w-4 h-4 animate-spin text-gray-400" />}
          {isOpen && (
            <button
              onClick={() => act(() => transactionsApi.suspendTransaction(txn.id))}
              disabled={busy}
              className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium rounded-lg border border-gray-200 dark:border-gray-700 text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 disabled:opacity-40"
            >
              <PauseCircle className="w-4 h-4" /> Put on hold
            </button>
          )}
        </div>
      </div>

      {isSuspended && (
        <div className="flex items-center justify-between gap-3 p-4 rounded-2xl bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800">
          <p className="text-sm text-amber-800 dark:text-amber-300">
            This sale is <b>on hold</b>. Resume it to make changes or take payment.
          </p>
          <button
            onClick={() => act(() => transactionsApi.recallTransaction(txn.id))}
            disabled={busy}
            className="flex items-center gap-1.5 px-4 py-2 text-sm font-semibold rounded-lg bg-amber-500 hover:bg-amber-600 text-white disabled:opacity-40"
          >
            <PlayCircle className="w-4 h-4" /> Resume
          </button>
        </div>
      )}

      {error && (
        <div className="flex items-start gap-2 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-xl">
          <AlertCircle className="w-4 h-4 text-red-500 flex-shrink-0 mt-0.5" />
          <p className="text-sm text-red-700 dark:text-red-400 flex-1">{error}</p>
          <button onClick={() => setError('')} className="text-red-400 hover:text-red-600"><X className="w-4 h-4" /></button>
        </div>
      )}

      <div className={cn('grid grid-cols-1 lg:grid-cols-8 gap-4 items-start', isSuspended && 'opacity-60 pointer-events-none')}>
        {/* Left: items */}
        <div className="lg:col-span-5 space-y-4">
          <ProductSearch onSelect={onProduct} />

          <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
            <div className="px-5 py-3.5 border-b border-gray-100 dark:border-gray-800">
              <h3 className="text-sm font-semibold text-gray-900 dark:text-white flex items-center gap-2">
                <ShoppingCart className="w-4 h-4 text-primary-600" /> Items ({activeLines.length})
              </h3>
            </div>
            {activeLines.length === 0 ? (
              <p className="px-6 py-16 text-center text-sm text-gray-400">No items — search above to add some.</p>
            ) : (
              <div className="divide-y divide-gray-100 dark:divide-gray-800">
                {activeLines.map((l) => (
                  <div key={l.id} className="flex items-center gap-4 px-5 py-3.5">
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-semibold text-gray-900 dark:text-white truncate">{l.productName}</p>
                      <p className="text-xs text-gray-400 font-mono mt-0.5">
                        {l.productSku}<span className="font-sans"> · {fmtMoney(l.unitPrice)} each</span>
                        {l.taxRate > 0 && <span className="font-sans"> · {l.taxRate}% VAT</span>}
                      </p>
                      {l.discountAmount > 0 && (
                        <span className="inline-flex items-center gap-1 mt-1 px-1.5 py-0.5 rounded-md text-[11px] font-semibold bg-emerald-50 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400">
                          <BadgePercent className="w-3 h-3" />
                          {l.appliedPromotionName ?? 'Discount'} · −{fmtMoney(l.discountAmount)}
                        </span>
                      )}
                    </div>

                    <div className="flex items-center bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl overflow-hidden flex-shrink-0">
                      <button
                        disabled={busy}
                        onClick={() => act(() => l.quantity <= 1
                          ? transactionsApi.voidLineItem(txn.id, l.id)
                          : transactionsApi.changeLineItemQuantity(txn.id, l.id, l.quantity - 1))}
                        className="px-3 py-2.5 text-gray-500 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-40"
                        title={l.quantity <= 1 ? 'Remove' : 'Decrease'}
                      >
                        {l.quantity <= 1 ? <Trash2 className="w-4 h-4 text-red-400" /> : <Minus className="w-4 h-4" />}
                      </button>
                      <span className="w-12 text-center text-sm font-bold text-gray-900 dark:text-white tabular-nums">
                        {l.quantity}
                      </span>
                      <button
                        disabled={busy}
                        onClick={() => act(() => transactionsApi.changeLineItemQuantity(txn.id, l.id, l.quantity + 1))}
                        className="px-3 py-2.5 text-gray-500 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-40"
                        title="Increase"
                      >
                        <Plus className="w-4 h-4" />
                      </button>
                    </div>

                    <div className="w-24 text-right flex-shrink-0">
                      <p className="text-sm font-bold text-gray-900 dark:text-white tabular-nums">
                        {fmtMoney(l.lineTotal - l.discountAmount + l.taxAmount)}
                      </p>
                      {l.taxAmount > 0 && <p className="text-xs text-gray-400 tabular-nums">incl. {fmtMoney(l.taxAmount)} VAT</p>}
                    </div>

                    <button
                      disabled={busy}
                      onClick={() => act(() => transactionsApi.voidLineItem(txn.id, l.id))}
                      className="p-1.5 text-gray-300 hover:text-red-500 disabled:opacity-40"
                      title="Remove item"
                    >
                      <X className="w-4 h-4" />
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Right: customer, totals, payments */}
        <div className="lg:col-span-3 lg:sticky lg:top-4 space-y-4">
          <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm p-4">
            {txn.customerId ? (
              <div className="flex items-center gap-3 px-3 py-2.5 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-xl">
                <User className="w-4 h-4 text-blue-600 dark:text-blue-400 flex-shrink-0" />
                <p className="flex-1 text-sm font-semibold text-blue-900 dark:text-blue-300 truncate">
                  {txn.customerName ?? 'Customer'}
                </p>
                <button onClick={() => onCustomer(null)} disabled={busy}
                  className="p-1 text-blue-400 hover:text-blue-600 disabled:opacity-40" title="Remove customer">
                  <X className="w-4 h-4" />
                </button>
              </div>
            ) : (
              <CustomerPicker value={null} onChange={onCustomer} />
            )}
            {txn.customerId && <RewardPointsStrip points={points} hasItems={activeLines.length > 0} />}
          </div>

          <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
            <div className="p-5 space-y-2.5 text-sm">
              <Row label="Subtotal" value={fmtMoney(txn.subTotal)} />
              <Row label="Discount" value={txn.discountTotal > 0 ? `−${fmtMoney(txn.discountTotal)}` : fmtMoney(0)}
                accent={txn.discountTotal > 0} />
              <Row label="VAT" value={fmtMoney(txn.taxTotal)} />
              <div className="h-px bg-gray-100 dark:bg-gray-800" />
              <div className="flex items-baseline justify-between">
                <span className="font-bold text-gray-900 dark:text-white uppercase tracking-wide">Total</span>
                <span className="text-3xl font-extrabold tabular-nums text-gray-900 dark:text-white">{fmtMoney(txn.total)}</span>
              </div>
            </div>

            {/* Payments taken */}
            <div className="px-5 pb-4 space-y-1.5">
              <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Payments</p>
              {payments.length === 0 && <p className="text-xs text-gray-400">None yet.</p>}
              {payments.map((p) => (
                <div key={p.id} className="flex items-center gap-2.5 px-3 py-2 bg-gray-50 dark:bg-gray-800 rounded-xl">
                  <PaymentMethodIcon method={p.method} size={18} />
                  <span className="text-sm font-medium text-gray-700 dark:text-gray-300 flex-1">{paymentMethodLabel(p.method)}</span>
                  <span className="text-sm font-bold tabular-nums text-gray-900 dark:text-white">{fmtMoney(p.amount)}</span>
                  <button
                    onClick={() => act(() => transactionsApi.removePayment(txn.id, p.id))}
                    disabled={busy || !isOpen}
                    className="p-0.5 text-gray-400 hover:text-red-600 disabled:opacity-40"
                    title="Remove payment (gives back gift card balance, credit or points)"
                  >
                    <X className="w-3.5 h-3.5" />
                  </button>
                </div>
              ))}
              <div className={cn(
                'flex items-center justify-between px-3.5 py-2.5 rounded-xl border mt-2',
                balanceDue > 0.005
                  ? 'bg-amber-50 dark:bg-amber-900/20 border-amber-200 dark:border-amber-800 text-amber-700 dark:text-amber-400'
                  : 'bg-emerald-50 dark:bg-emerald-900/20 border-emerald-200 dark:border-emerald-800 text-emerald-700 dark:text-emerald-400',
              )}>
                <span className="text-sm font-semibold">
                  {balanceDue > 0.005 ? 'Balance due' : balanceDue < -0.005 ? 'Change due' : 'Fully paid'}
                </span>
                <span className="text-lg font-extrabold tabular-nums">{fmtMoney(Math.abs(balanceDue))}</span>
              </div>
            </div>

            {/* Take a payment */}
            {balanceDue > 0.005 && activeLines.length > 0 && (
              <div className="mx-5 mb-4 p-3 border border-dashed border-gray-300 dark:border-gray-700 rounded-xl space-y-2.5">
                <div className="flex flex-wrap gap-1.5">
                  {PAYMENT_METHODS.map((m) => (
                    <button
                      key={m.value}
                      onClick={() => pickMethod(m.value)}
                      disabled={!!disabledMethods[m.value]}
                      title={disabledMethods[m.value] ?? m.hint}
                      className={cn(
                        'flex items-center gap-1.5 pl-1.5 pr-2.5 py-1 rounded-full border text-xs font-semibold transition-all disabled:opacity-40 disabled:cursor-not-allowed',
                        method === m.value
                          ? 'border-primary-600 bg-primary-50 dark:bg-primary-900/20 text-primary-700 dark:text-primary-400'
                          : 'border-gray-200 dark:border-gray-700 text-gray-500 dark:text-gray-400',
                      )}
                    >
                      <PaymentMethodIcon method={m.value} size={16} /> {m.label}
                    </button>
                  ))}
                </div>
                <div className="flex gap-2">
                  <div className="relative flex-1">
                    <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm font-semibold">৳</span>
                    <input
                      type="number" inputMode="decimal" min={0}
                      value={amount}
                      onChange={(e) => setAmount(e.target.value)}
                      placeholder={balanceDue.toFixed(2)}
                      className="w-full pl-7 pr-3 py-2 text-sm font-bold tabular-nums border border-gray-300 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
                    />
                  </div>
                  <button onClick={addPayment} disabled={busy || !isOpen}
                    className="flex items-center gap-1.5 px-4 py-2 text-sm font-semibold text-white bg-primary-700 hover:bg-primary-800 rounded-lg disabled:opacity-40">
                    <Plus className="w-4 h-4" /> Add
                  </button>
                </div>
                {needsMobile && (
                  <input type="tel" value={mobile} onChange={(e) => setMobile(e.target.value)} placeholder="Mobile number *"
                    className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white" />
                )}
                {needsGiftCard && (
                  <input type="text" value={giftCode} onChange={(e) => setGiftCode(e.target.value.toUpperCase())} placeholder="Gift card code *"
                    className="w-full px-3 py-2 text-sm font-mono uppercase border border-gray-300 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white" />
                )}
              </div>
            )}

            <div className="p-5 pt-0">
              <button
                onClick={() => act(() => transactionsApi.completeTransaction(txn.id),
                  () => navigate(`/sales/transactions/${txn.id}`))}
                disabled={!canComplete}
                className={cn(
                  'w-full flex items-center justify-center gap-2.5 px-5 py-4 rounded-2xl text-base font-bold text-white transition-all shadow-md',
                  canComplete ? 'bg-emerald-600 hover:bg-emerald-700' : 'bg-gray-300 dark:bg-gray-700 cursor-not-allowed shadow-none',
                )}
              >
                {busy ? <Loader2 className="w-5 h-5 animate-spin" /> : <CheckCircle2 className="w-5 h-5" />}
                Complete sale
              </button>
              <p className="text-center text-xs text-gray-400 mt-2">Changes are saved as you go.</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

function Row({ label, value, accent }: { label: string; value: string; accent?: boolean }) {
  return (
    <div className="flex justify-between">
      <span className={accent ? 'text-emerald-600 dark:text-emerald-400 font-medium' : 'text-gray-500 dark:text-gray-400'}>{label}</span>
      <span className={cn('tabular-nums font-medium', accent ? 'text-emerald-600 dark:text-emerald-400' : 'text-gray-700 dark:text-gray-300')}>{value}</span>
    </div>
  );
}
