import { useMemo, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import {
  ShoppingCart, ChevronRight, Loader2, AlertCircle, Repeat, Undo2, PackagePlus, Plus, Minus, Trash2,
  BadgePercent,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { ProductListItem } from '../types/product';
import type { DamageDisposition, LineItem, ReturnReason } from '../types/transaction';
import { useTransactionDetail, useCartPreview, useExchangeTransaction } from '../hooks/useTransactions';
import { ProductSearch } from '../components/transactions/CheckoutWidgets';
import { PaymentMethodIcon } from '../components/transactions/PaymentMethodIcon';
import { MOBILE_WALLET_METHODS, paymentMethodLabel } from '../lib/paymentMethods';

const SETTLEMENT_METHODS = ['Cash', 'Card', 'BKash', 'Nagad', 'Rocket'];

const RETURN_REASONS: { value: ReturnReason; label: string }[] = [
  { value: 'CustomerChangedMind', label: 'Changed mind' },
  { value: 'Defective',           label: 'Defective' },
  { value: 'WrongItem',           label: 'Wrong item' },
  { value: 'Expired',             label: 'Expired' },
  { value: 'Other',               label: 'Other' },
];

function fmtMoney(n: number) {
  return `৳${n.toLocaleString('en-BD', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}
const round2 = (n: number) => Math.round(n * 100) / 100;

/** What the customer paid for `qty` units of an original line (mirrors the backend). */
function refundFor(line: LineItem, qty: number) {
  const gross    = line.unitPrice * qty;
  const discount = line.quantity > 0 ? round2(line.discountAmount * qty / line.quantity) : 0;
  const tax      = round2((gross - discount) * line.taxRate / 100);
  return gross - discount + tax;
}

interface ReturnState { selected: boolean; qty: number; reason: ReturnReason; disposition: DamageDisposition }
interface NewItem { productId: string; name: string; sku: string; quantity: number }

export default function TransactionExchangePage() {
  const { id = '' } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: txn, isLoading, isError } = useTransactionDetail(id || null);
  const exchange = useExchangeTransaction();

  const [returns, setReturns]   = useState<Record<string, ReturnState>>({});
  const [newItems, setNewItems] = useState<NewItem[]>([]);
  const [method, setMethod]     = useState('Cash');
  const [mobile, setMobile]     = useState('');
  const [notes, setNotes]       = useState('');
  const [error, setError]       = useState('');

  const previewItems = useMemo(
    () => newItems.map((i) => ({ productId: i.productId, quantity: i.quantity })),
    [newItems],
  );
  const { data: preview, isFetching: pricing } = useCartPreview(txn?.storeId ?? '', previewItems);

  if (isLoading) {
    return <div className="flex justify-center py-24"><Loader2 className="w-8 h-8 animate-spin text-primary-600" /></div>;
  }
  if (isError || !txn) return <p className="text-center py-24 text-sm text-gray-500">Sale not found.</p>;

  if (txn.status !== 'Completed') {
    return (
      <div className="max-w-lg mx-auto mt-16 text-center space-y-3">
        <p className="text-gray-700 dark:text-gray-300 font-medium">Only completed sales can be exchanged.</p>
        <p className="text-sm text-gray-500">
          {txn.status === 'Refunded' ? 'This sale already has a return or exchange against it.' : `This sale is ${txn.status.toLowerCase()}.`}
        </p>
        <Link to={`/sales/transactions/${txn.id}`} className="text-sm text-primary-600 hover:underline">Back to the sale</Link>
      </div>
    );
  }

  const lines = txn.lineItems.filter((l) => !l.isVoided);
  const stateFor = (l: LineItem): ReturnState =>
    returns[l.id] ?? { selected: false, qty: l.quantity, reason: 'Other', disposition: 'RestoreToStock' };
  const update = (l: LineItem, patch: Partial<ReturnState>) =>
    setReturns((r) => ({ ...r, [l.id]: { ...stateFor(l), ...patch } }));

  const selected    = lines.filter((l) => stateFor(l).selected);
  const returnValue = selected.reduce((s, l) => s + refundFor(l, stateFor(l).qty), 0);

  const previewReady = newItems.length > 0 && !!preview && preview.lines.length === newItems.length
    && newItems.every((i) => preview.lines.find((pl) => pl.productId === i.productId)?.quantity === i.quantity);
  const newTotal   = previewReady ? preview!.total : 0;
  const difference = round2(newTotal - returnValue);

  const needsMobile = MOBILE_WALLET_METHODS.includes(method) && Math.abs(difference) > 0.005;
  const canSubmit   = selected.length > 0 && previewReady && !pricing && !exchange.isPending
    && !(needsMobile && !mobile.trim());

  function addProduct(p: ProductListItem) {
    setNewItems((items) => items.some((i) => i.productId === p.id)
      ? items.map((i) => i.productId === p.id ? { ...i, quantity: i.quantity + 1 } : i)
      : [...items, { productId: p.id, name: p.name, sku: p.sku, quantity: 1 }]);
  }
  const setQty = (pid: string, q: number) =>
    setNewItems((items) => q < 1 ? items.filter((i) => i.productId !== pid)
      : items.map((i) => i.productId === pid ? { ...i, quantity: q } : i));

  function submit() {
    if (!txn) return;
    setError('');
    exchange.mutate({
      id: txn.id,
      payload: {
        returnItems: selected.map((l) => {
          const s = stateFor(l);
          return { originalLineItemId: l.id, quantity: s.qty, returnReason: s.reason, damageDisposition: s.disposition };
        }),
        newItems: previewItems,
        settlementMethod: method,
        settlementMobileNumber: needsMobile ? mobile.trim() : null,
        notes: notes.trim() || null,
      },
    }, {
      onSuccess: (r) => navigate(`/sales/transactions/${r.replacement.id}`),
      onError: (e) => setError(extractApiError(e)),
    });
  }

  const card = 'bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm';
  const selectCls = 'px-2 py-1.5 text-xs border border-gray-300 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white';

  return (
    <div className="max-w-7xl mx-auto space-y-4">
      <nav className="flex items-center gap-2 text-sm text-gray-500 dark:text-gray-400">
        <Link to="/sales/transactions" className="hover:text-primary-600 flex items-center gap-1">
          <ShoppingCart className="w-3.5 h-3.5" /> Transactions
        </Link>
        <ChevronRight className="w-3.5 h-3.5" />
        <Link to={`/sales/transactions/${txn.id}`} className="hover:text-primary-600 font-mono">{txn.transactionNumber}</Link>
        <ChevronRight className="w-3.5 h-3.5" />
        <span className="text-gray-900 dark:text-white font-semibold flex items-center gap-1.5">
          <Repeat className="w-4 h-4 text-primary-600" /> Exchange
        </span>
      </nav>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 items-start">
        {/* Coming back */}
        <div className={card}>
          <div className="px-5 py-3.5 border-b border-gray-100 dark:border-gray-800 flex items-center justify-between">
            <h3 className="text-sm font-semibold text-gray-900 dark:text-white flex items-center gap-2">
              <Undo2 className="w-4 h-4 text-orange-500" /> Coming back
            </h3>
            <span className="text-sm font-bold tabular-nums text-orange-600 dark:text-orange-400">{fmtMoney(returnValue)}</span>
          </div>
          <div className="divide-y divide-gray-100 dark:divide-gray-800">
            {lines.map((l) => {
              const s = stateFor(l);
              return (
                <div key={l.id} className={cn('p-4 space-y-2', s.selected && 'bg-orange-50/50 dark:bg-orange-900/10')}>
                  <label className="flex items-start gap-3 cursor-pointer">
                    <input type="checkbox" checked={s.selected} onChange={(e) => update(l, { selected: e.target.checked })}
                      className="mt-1 w-4 h-4 rounded border-gray-300 text-primary-600" />
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-gray-900 dark:text-white truncate">{l.productName}</p>
                      <p className="text-xs text-gray-500">{l.quantity} bought · {fmtMoney(refundFor(l, l.quantity))} paid</p>
                    </div>
                  </label>
                  {s.selected && (
                    <div className="flex flex-wrap items-center gap-2 pl-7">
                      <input type="number" min={1} max={l.quantity} value={s.qty}
                        onChange={(e) => update(l, { qty: Math.min(Math.max(1, Number(e.target.value) || 1), l.quantity) })}
                        className={cn(selectCls, 'w-16')} />
                      <select value={s.reason} onChange={(e) => update(l, { reason: e.target.value as ReturnReason })} className={selectCls}>
                        {RETURN_REASONS.map((r) => <option key={r.value} value={r.value}>{r.label}</option>)}
                      </select>
                      <select value={s.disposition} onChange={(e) => update(l, { disposition: e.target.value as DamageDisposition })} className={selectCls}>
                        <option value="RestoreToStock">Back to stock</option>
                        <option value="WriteOff">Write off (damaged)</option>
                      </select>
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </div>

        {/* Going out */}
        <div className="space-y-3">
          <ProductSearch onSelect={addProduct} />
          <div className={card}>
            <div className="px-5 py-3.5 border-b border-gray-100 dark:border-gray-800 flex items-center justify-between">
              <h3 className="text-sm font-semibold text-gray-900 dark:text-white flex items-center gap-2">
                <PackagePlus className="w-4 h-4 text-emerald-600" /> Replacement items
              </h3>
              <span className="text-sm font-bold tabular-nums text-emerald-600 dark:text-emerald-400 flex items-center gap-1.5">
                {pricing && <Loader2 className="w-3 h-3 animate-spin" />}
                {fmtMoney(newTotal)}
              </span>
            </div>
            {newItems.length === 0 ? (
              <p className="px-5 py-10 text-center text-sm text-gray-400">Search above to add the items the customer takes instead.</p>
            ) : (
              <div className="divide-y divide-gray-100 dark:divide-gray-800">
                {newItems.map((i) => {
                  const pl = previewReady ? preview!.lines.find((x) => x.productId === i.productId) : undefined;
                  return (
                    <div key={i.productId} className="flex items-center gap-3 px-5 py-3">
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium text-gray-900 dark:text-white truncate">{i.name}</p>
                        <p className="text-xs text-gray-400 font-mono">{i.sku}</p>
                        {pl && pl.discountAmount > 0 && (
                          <span className="inline-flex items-center gap-1 mt-1 px-1.5 py-0.5 rounded-md text-[11px] font-semibold bg-emerald-50 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400">
                            <BadgePercent className="w-3 h-3" /> {pl.promotionName} · −{fmtMoney(pl.discountAmount)}
                          </span>
                        )}
                      </div>
                      <div className="flex items-center border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden">
                        <button onClick={() => setQty(i.productId, i.quantity - 1)} className="px-2 py-1.5 text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800">
                          {i.quantity <= 1 ? <Trash2 className="w-3.5 h-3.5 text-red-400" /> : <Minus className="w-3.5 h-3.5" />}
                        </button>
                        <span className="w-8 text-center text-sm font-bold tabular-nums">{i.quantity}</span>
                        <button onClick={() => setQty(i.productId, i.quantity + 1)} className="px-2 py-1.5 text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800">
                          <Plus className="w-3.5 h-3.5" />
                        </button>
                      </div>
                      <span className="w-20 text-right text-sm font-bold tabular-nums">{pl ? fmtMoney(pl.lineTotal) : '…'}</span>
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Settlement */}
      <div className={cn(card, 'p-5 space-y-4')}>
        <div className="grid grid-cols-3 gap-3 text-center">
          <Stat label="Coming back" value={fmtMoney(returnValue)} />
          <Stat label="Going out" value={fmtMoney(newTotal)} />
          <Stat
            label={difference > 0.005 ? 'Customer pays' : difference < -0.005 ? 'Refund to customer' : 'Difference'}
            value={fmtMoney(Math.abs(difference))}
            accent={difference > 0.005 ? 'text-amber-600 dark:text-amber-400' : difference < -0.005 ? 'text-emerald-600 dark:text-emerald-400' : undefined}
          />
        </div>

        {Math.abs(difference) > 0.005 && (
          <div className="space-y-2">
            <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              {difference > 0 ? 'Customer pays the difference by' : 'Refund the difference by'}
            </p>
            <div className="flex flex-wrap gap-2">
              {SETTLEMENT_METHODS.map((m) => (
                <button key={m} onClick={() => setMethod(m)}
                  className={cn('flex items-center gap-1.5 pl-2 pr-3 py-1.5 rounded-full border-2 text-xs font-semibold',
                    method === m ? 'border-primary-600 bg-primary-50 dark:bg-primary-900/20 text-primary-700 dark:text-primary-400'
                      : 'border-gray-200 dark:border-gray-700 text-gray-500 dark:text-gray-400')}>
                  <PaymentMethodIcon method={m} size={16} /> {paymentMethodLabel(m)}
                </button>
              ))}
            </div>
            {needsMobile && (
              <input value={mobile} onChange={(e) => setMobile(e.target.value)} placeholder="Mobile number *"
                className="w-60 px-3 py-2 text-sm border border-gray-300 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white" />
            )}
          </div>
        )}

        <input value={notes} onChange={(e) => setNotes(e.target.value)} placeholder="Note (optional)"
          className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white" />

        {error && (
          <div className="flex items-start gap-2 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-xl">
            <AlertCircle className="w-4 h-4 text-red-500 flex-shrink-0 mt-0.5" />
            <p className="text-sm text-red-700 dark:text-red-400">{error}</p>
          </div>
        )}

        <div className="flex justify-end gap-3">
          <Link to={`/sales/transactions/${txn.id}`} className="px-4 py-2 text-sm font-medium bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 rounded-xl">
            Cancel
          </Link>
          <button onClick={submit} disabled={!canSubmit}
            className="flex items-center gap-2 px-5 py-2 text-sm font-semibold text-white bg-primary-700 hover:bg-primary-800 rounded-xl disabled:opacity-40">
            {exchange.isPending ? <Loader2 className="w-4 h-4 animate-spin" /> : <Repeat className="w-4 h-4" />}
            Complete exchange
          </button>
        </div>
      </div>
    </div>
  );
}

function Stat({ label, value, accent }: { label: string; value: string; accent?: string }) {
  return (
    <div className="p-3 rounded-xl bg-gray-50 dark:bg-gray-800">
      <p className="text-xs text-gray-500 dark:text-gray-400">{label}</p>
      <p className={cn('text-lg font-extrabold tabular-nums text-gray-900 dark:text-white', accent)}>{value}</p>
    </div>
  );
}
