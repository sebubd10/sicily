import { useState, useEffect, useMemo } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import {
  ShoppingCart, ChevronRight, Plus, Minus,
  Trash2, Loader2, AlertCircle,
  Receipt, ScanLine, Store as StoreIcon, Monitor, CheckCircle2, Delete,
  SplitSquareHorizontal, ArrowLeftRight, BadgePercent, Star,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { ProductListItem } from '../types/product';
import type { Customer } from '../types/customer';
import { useStoreList } from '../hooks/useStoreAdmin';
import { useTerminals } from '../hooks/useTerminals';
import * as transactionsApi from '../api/transactionsApi';
import { useCartPreview } from '../hooks/useTransactions';
import { useCheckoutRewardPoints } from '../hooks/useRewardPoints';
import { useQueryClient } from '@tanstack/react-query';
import { PaymentMethodIcon } from '../components/transactions/PaymentMethodIcon';
import { ProductSearch, CustomerPicker, RewardPointsStrip } from '../components/transactions/CheckoutWidgets';
import { SplitPaymentPanel, type DraftPayment } from '../components/transactions/SplitPaymentPanel';
import { PAYMENT_METHODS } from '../lib/paymentMethods';

/* ── Types & constants ────────────────────────────────────────────────────── */

interface DraftItem {
  tempId: string;
  productId: string;
  productName: string;
  productSku: string;
  quantity: number;
  unitPrice: number;
  taxRate: number;
}

const SETUP_STORAGE_KEY = 'pos-new-txn-setup';

function fmtMoney(n: number) {
  return `৳${n.toLocaleString('en-BD', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

function fmtMoneyShort(n: number) {
  return `৳${n.toLocaleString('en-BD', { maximumFractionDigits: 0 })}`;
}

/* ── Main page ────────────────────────────────────────────────────────────── */

export default function TransactionNewPage() {
  const navigate = useNavigate();
  const qc       = useQueryClient();

  // Persisted register setup — cashiers shouldn't reselect these every sale
  const saved = useMemo(() => {
    try { return JSON.parse(localStorage.getItem(SETUP_STORAGE_KEY) ?? '{}'); }
    catch { return {}; }
  }, []);

  const [storeId,      setStoreId]      = useState<string>(saved.storeId ?? '');
  const [terminalId,   setTerminalId]   = useState<string>(saved.terminalId ?? '');
  const [customer,     setCustomer]     = useState<Customer | null>(null);
  const [items,        setItems]        = useState<DraftItem[]>([]);
  const [payMethod,    setPayMethod]    = useState('Cash');
  const [tendered,     setTendered]     = useState('');
  const [payRef,       setPayRef]       = useState('');
  const [giftCardCode, setGiftCardCode] = useState('');
  const [mobileNum,    setMobileNum]    = useState('');
  const [splitMode,    setSplitMode]    = useState(false);
  const [splitPayments, setSplitPayments] = useState<DraftPayment[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError,  setSubmitError]  = useState('');

  const { data: stores    = [] } = useStoreList();
  const { data: terminals = [] } = useTerminals();

  const activeStores   = stores.filter((s) => s.status === 'Active');
  const storeTerminals = terminals.filter(
    (t) => t.status === 'Active' && (!storeId || t.storeId === storeId),
  );

  useEffect(() => {
    localStorage.setItem(SETUP_STORAGE_KEY, JSON.stringify({ storeId, terminalId }));
  }, [storeId, terminalId]);

  /* ── Cart ops ── */

  function addProduct(p: ProductListItem) {
    setItems((prev) => {
      const existing = prev.find((i) => i.productId === p.id);
      if (existing) {
        return prev.map((i) =>
          i.productId === p.id ? { ...i, quantity: i.quantity + 1 } : i);
      }
      return [...prev, {
        tempId:      crypto.randomUUID(),
        productId:   p.id,
        productName: p.name,
        productSku:  p.sku,
        quantity:    1,
        unitPrice:   p.price,
        taxRate:     p.vatRate,
      }];
    });
  }

  function updateQty(tempId: string, qty: number) {
    if (qty < 1 || Number.isNaN(qty)) return;
    setItems((prev) => prev.map((i) => (i.tempId === tempId ? { ...i, quantity: qty } : i)));
  }

  function removeItem(tempId: string) {
    setItems((prev) => prev.filter((i) => i.tempId !== tempId));
  }

  /* ── Totals ── */

  // Prices come from the server (same promotion + VAT logic as the real sale), so the
  // amount the cashier quotes is exactly what gets charged.
  const previewItems = useMemo(
    () => items.map((i) => ({ productId: i.productId, quantity: i.quantity })),
    [items],
  );
  const { data: preview, isError: previewError } = useCartPreview(storeId, previewItems);

  const previewLines = new Map((preview?.lines ?? []).map((l) => [l.productId, l]));
  // keepPreviousData can briefly hand back pricing for the previous cart — only trust
  // the preview when it matches the cart line for line.
  const pricingReady = items.length === 0 || (
    !!preview
    && preview.lines.length === items.length
    && items.every((i) => previewLines.get(i.productId)?.quantity === i.quantity)
  );

  const lineItems = items.map((i) => {
    const p = pricingReady ? previewLines.get(i.productId) : undefined;
    if (p) {
      return {
        ...i,
        lineSubTotal: p.lineSubTotal,
        discount: p.discountAmount,
        promotionName: p.promotionName,
        taxAmt: p.taxAmount,
        lineTotal: p.lineTotal,
      };
    }
    const lineSubTotal = i.unitPrice * i.quantity;
    const taxAmt       = lineSubTotal * (i.taxRate / 100);
    return {
      ...i, lineSubTotal, discount: 0, promotionName: null as string | null,
      taxAmt, lineTotal: lineSubTotal + taxAmt,
    };
  });
  const subTotal      = lineItems.reduce((s, i) => s + i.lineSubTotal, 0);
  const discountTotal = lineItems.reduce((s, i) => s + i.discount, 0);
  const taxTotal      = lineItems.reduce((s, i) => s + i.taxAmt, 0);
  const total         = pricingReady && preview ? preview.total : subTotal - discountTotal + taxTotal;
  const itemCount     = items.reduce((s, i) => s + i.quantity, 0);

  const isCash        = payMethod === 'Cash';
  const needsMobile   = ['BKash', 'Nagad', 'Rocket'].includes(payMethod);
  const needsGiftCard = payMethod === 'GiftCard';

  const tenderedNum = parseFloat(tendered) || 0;
  const changeDue   = isCash && tenderedNum > total ? tenderedNum - total : 0;
  const cashShort   = isCash && tendered !== '' && tenderedNum < total;

  // Quick tender suggestions: exact + sensible round-ups above the total
  const quickTenders = useMemo(() => {
    if (total <= 0) return [];
    const ups = [
      Math.ceil(total / 100) * 100,
      Math.ceil(total / 500) * 500,
      Math.ceil(total / 1000) * 1000,
    ].filter((v) => v > total);
    return [...new Set(ups)].slice(0, 3);
  }, [total]);

  const splitPaidSoFar  = splitPayments.reduce((s, p) => s + p.amount, 0);
  const splitBalanceDue = Math.round((total - splitPaidSoFar) * 100) / 100;

  /* ── Reward points ── */

  const pointsPaying = splitMode
    ? splitPayments.filter((p) => p.method === 'RewardPoints').reduce((s, p) => s + p.amount, 0)
    : payMethod === 'RewardPoints' ? total : 0;

  const { data: points } = useCheckoutRewardPoints({
    customerId:   customer?.id ?? null,
    storeId,
    orderTotal:   items.length > 0 ? total : 0,
    pointsAmount: pointsPaying,
  });

  const pointsMaxAmount = !customer
    ? 0
    : points ? (points.programEnabled ? points.maxRedeemableAmount : 0) : undefined;
  const pointsCoverWholeSale = pointsMaxAmount !== undefined && pointsMaxAmount >= total - 0.005;

  const disabledMethods: Record<string, string> = {};
  if (!customer) {
    disabledMethods.Credit       = 'Attach a customer to sell on credit';
    disabledMethods.RewardPoints = 'Attach a customer to pay with points';
  } else if (points && !points.programEnabled) {
    disabledMethods.RewardPoints = 'The reward points programme is turned off';
  } else if (points && points.maxRedeemableAmount <= 0) {
    disabledMethods.RewardPoints =
      points.availablePoints > 0 && points.availablePoints < points.minimumPointsToUse
        ? `Needs at least ${points.minimumPointsToUse} points to redeem`
        : 'No points available to use on this sale';
  }

  // Detaching the customer (or the customer having no usable points) must not leave a
  // points/credit payment selected.
  useEffect(() => {
    if (disabledMethods[payMethod]) setPayMethod('Cash');
    setSplitPayments((prev) => {
      const kept = prev.filter((p) => !disabledMethods[p.method]);
      return kept.length === prev.length ? prev : kept;
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [customer?.id, disabledMethods.RewardPoints, disabledMethods.Credit]);

  const setupOk   = !!storeId && !!terminalId;
  const canSubmit = setupOk && items.length > 0 && !isSubmitting && pricingReady
    && (splitMode
      ? splitPayments.length > 0 && splitBalanceDue <= 0.005
      : !(needsMobile && !mobileNum) && !(needsGiftCard && !giftCardCode) && !cashShort
        && !(payMethod === 'RewardPoints' && !pointsCoverWholeSale));

  function toggleSplitMode() {
    setSplitMode((v) => !v);
    setSplitPayments([]);
    setSubmitError('');
  }

  /** Points can't cover the whole sale: use the max in points and pay the rest another way. */
  function splitWithPoints() {
    if (!pointsMaxAmount) return;
    setSplitMode(true);
    setPayMethod('Cash');
    setSplitPayments([{ tempId: crypto.randomUUID(), method: 'RewardPoints', amount: pointsMaxAmount }]);
    setSubmitError('');
  }

  /* ── Submit ── */

  async function handleSubmit() {
    if (!canSubmit) return;
    setIsSubmitting(true);
    setSubmitError('');

    const paymentsToSend: DraftPayment[] = splitMode
      ? splitPayments
      : [{
          tempId:       'single',
          method:       payMethod,
          // For cash send the tendered amount so the backend records change due
          amount:       isCash && tenderedNum > total ? tenderedNum : total,
          reference:    payRef || undefined,
          mobileNumber: needsMobile ? mobileNum : undefined,
          giftCardCode: needsGiftCard ? giftCardCode : undefined,
        }];

    let txnId: string | null = null;
    try {
      const txn = await transactionsApi.createTransaction({
        storeId, terminalId, customerId: customer?.id,
      });
      txnId = txn.id;

      for (const item of items) {
        await transactionsApi.addLineItem(txn.id, item.productId, item.quantity);
      }

      for (const p of paymentsToSend) {
        await transactionsApi.addPayment(
          txn.id, p.method, p.amount, p.reference, p.giftCardCode, p.mobileNumber,
        );
      }

      await transactionsApi.completeTransaction(txn.id);

      qc.invalidateQueries({ queryKey: ['transactions'] });
      qc.invalidateQueries({ queryKey: ['reward-points'] });
      qc.invalidateQueries({ queryKey: ['customers'] });
      navigate(`/sales/transactions/${txn.id}`);
    } catch (e) {
      const base = extractApiError(e);
      setSubmitError(txnId
        ? `${base} The draft transaction was saved — you can find and void it in the transaction list.`
        : base);
      setIsSubmitting(false);
    }
  }

  const compactSelect = 'text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg pl-8 pr-3 py-1.5 text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 cursor-pointer transition appearance-none';

  return (
    <div className="max-w-7xl mx-auto space-y-4">
      {/* Top bar: breadcrumb + register setup */}
      <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-3">
        <nav className="flex items-center gap-2 text-sm text-gray-500 dark:text-gray-400">
          <Link to="/sales/transactions" className="hover:text-primary-600 transition-colors flex items-center gap-1">
            <ShoppingCart className="w-3.5 h-3.5" />
            Transactions
          </Link>
          <ChevronRight className="w-3.5 h-3.5" />
          <span className="text-gray-900 dark:text-white font-semibold flex items-center gap-1.5">
            <Receipt className="w-4 h-4 text-primary-600" />
            New Sale
          </span>
        </nav>

        {/* Register setup — compact, persisted */}
        <div className="flex items-center gap-2">
          <div className="relative">
            <StoreIcon className="absolute left-2.5 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-gray-400 pointer-events-none" />
            <select
              value={storeId}
              onChange={(e) => { setStoreId(e.target.value); setTerminalId(''); }}
              className={cn(compactSelect, !storeId && 'border-amber-300 dark:border-amber-700')}
            >
              <option value="">Store…</option>
              {activeStores.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
            </select>
          </div>
          <div className="relative">
            <Monitor className="absolute left-2.5 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-gray-400 pointer-events-none" />
            <select
              value={terminalId}
              onChange={(e) => setTerminalId(e.target.value)}
              disabled={!storeId}
              className={cn(compactSelect, 'disabled:opacity-50', storeId && !terminalId && 'border-amber-300 dark:border-amber-700')}
            >
              <option value="">Terminal…</option>
              {storeTerminals.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
            </select>
          </div>
          {setupOk && (
            <span className="hidden sm:flex items-center gap-1 text-xs text-emerald-600 dark:text-emerald-400 font-medium">
              <CheckCircle2 className="w-3.5 h-3.5" /> Ready
            </span>
          )}
        </div>
      </div>

      {/* Main layout */}
      <div className="grid grid-cols-1 lg:grid-cols-8 gap-4 items-start">

        {/* ── LEFT: search + cart ── */}
        <div className="lg:col-span-5 space-y-4">
          <ProductSearch onSelect={addProduct} />

          {/* Cart */}
          <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
            <div className="px-5 py-3.5 border-b border-gray-100 dark:border-gray-800 flex items-center justify-between">
              <h3 className="text-sm font-semibold text-gray-900 dark:text-white flex items-center gap-2">
                <ShoppingCart className="w-4 h-4 text-primary-600" />
                Cart
                {items.length > 0 && (
                  <span className="px-2 py-0.5 text-xs font-bold bg-primary-100 dark:bg-primary-900/40 text-primary-700 dark:text-primary-400 rounded-full tabular-nums">
                    {itemCount} {itemCount === 1 ? 'item' : 'items'}
                  </span>
                )}
              </h3>
              {items.length > 0 && (
                <button onClick={() => setItems([])}
                  className="flex items-center gap-1 text-xs font-medium text-gray-400 hover:text-red-600 transition-colors">
                  <Delete className="w-3.5 h-3.5" /> Clear
                </button>
              )}
            </div>

            {items.length === 0 ? (
              <div className="px-6 py-20 text-center">
                <div className="w-16 h-16 mx-auto mb-4 rounded-2xl bg-gray-50 dark:bg-gray-800 flex items-center justify-center">
                  <ScanLine className="w-7 h-7 text-gray-300 dark:text-gray-600" />
                </div>
                <p className="text-sm font-medium text-gray-500 dark:text-gray-400">Cart is empty</p>
                <p className="text-xs text-gray-400 mt-1">Scan a barcode or search above — items appear here instantly.</p>
              </div>
            ) : (
              <div className="divide-y divide-gray-100 dark:divide-gray-800">
                {lineItems.map((item) => (
                  <div key={item.tempId}
                    className="flex items-center gap-4 px-5 py-3.5 hover:bg-gray-50/70 dark:hover:bg-gray-800/40 transition-colors">
                    {/* Name + SKU */}
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-semibold text-gray-900 dark:text-white truncate">{item.productName}</p>
                      <p className="text-xs text-gray-400 font-mono mt-0.5">
                        {item.productSku}
                        <span className="font-sans text-gray-400"> · {fmtMoney(item.unitPrice)} each</span>
                        {item.taxRate > 0 && <span className="font-sans"> · {item.taxRate}% VAT</span>}
                      </p>
                      {item.promotionName && item.discount > 0 && (
                        <span className="inline-flex items-center gap-1 mt-1 px-1.5 py-0.5 rounded-md text-[11px] font-semibold bg-emerald-50 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400">
                          <BadgePercent className="w-3 h-3" />
                          {item.promotionName} · −{fmtMoney(item.discount)}
                        </span>
                      )}
                    </div>

                    {/* Qty stepper — large hit targets */}
                    <div className="flex items-center bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl overflow-hidden flex-shrink-0">
                      <button
                        onClick={() => item.quantity <= 1 ? removeItem(item.tempId) : updateQty(item.tempId, item.quantity - 1)}
                        className="px-3 py-2.5 text-gray-500 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
                        title={item.quantity <= 1 ? 'Remove' : 'Decrease'}
                      >
                        {item.quantity <= 1
                          ? <Trash2 className="w-4 h-4 text-red-400" />
                          : <Minus className="w-4 h-4" />}
                      </button>
                      <input
                        type="number"
                        min={1}
                        value={item.quantity}
                        onChange={(e) => updateQty(item.tempId, Number(e.target.value))}
                        className="w-12 py-2 text-center text-sm font-bold bg-transparent text-gray-900 dark:text-white focus:outline-none tabular-nums [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
                      />
                      <button
                        onClick={() => updateQty(item.tempId, item.quantity + 1)}
                        className="px-3 py-2.5 text-gray-500 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
                        title="Increase"
                      >
                        <Plus className="w-4 h-4" />
                      </button>
                    </div>

                    {/* Line total */}
                    <div className="w-24 text-right flex-shrink-0">
                      {item.discount > 0 && (
                        <p className="text-xs text-gray-400 line-through tabular-nums">
                          {fmtMoney(item.lineSubTotal * (1 + item.taxRate / 100))}
                        </p>
                      )}
                      <p className="text-sm font-bold text-gray-900 dark:text-white tabular-nums">{fmtMoney(item.lineTotal)}</p>
                      {item.taxAmt > 0 && (
                        <p className="text-xs text-gray-400 tabular-nums">incl. {fmtMoney(item.taxAmt)} VAT</p>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* ── RIGHT: checkout panel (sticky) ── */}
        <div className="lg:col-span-3 lg:sticky lg:top-4 space-y-4">

          {/* Customer */}
          <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm p-4">
            <CustomerPicker value={customer} onChange={setCustomer} />
            {customer && <RewardPointsStrip points={points} hasItems={items.length > 0} />}
          </div>

          {/* Checkout */}
          <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
            {/* Totals */}
            <div className="p-5 space-y-2.5">
              <div className="flex justify-between text-sm">
                <span className="text-gray-500 dark:text-gray-400">Subtotal</span>
                <span className="tabular-nums font-medium text-gray-700 dark:text-gray-300">{fmtMoney(subTotal)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className={cn(
                  'flex items-center gap-1.5',
                  discountTotal > 0 ? 'text-emerald-600 dark:text-emerald-400 font-medium' : 'text-gray-500 dark:text-gray-400',
                )}>
                  <BadgePercent className="w-3.5 h-3.5" />
                  Discount
                  {discountTotal > 0 && (
                    <span className="text-xs font-normal text-gray-400">
                      ({lineItems.filter((i) => i.discount > 0).length} promo
                      {lineItems.filter((i) => i.discount > 0).length === 1 ? '' : 's'})
                    </span>
                  )}
                </span>
                <span className={cn(
                  'tabular-nums font-medium',
                  discountTotal > 0 ? 'text-emerald-600 dark:text-emerald-400' : 'text-gray-700 dark:text-gray-300',
                )}>
                  {discountTotal > 0 ? `−${fmtMoney(discountTotal)}` : fmtMoney(0)}
                </span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500 dark:text-gray-400">VAT</span>
                <span className="tabular-nums font-medium text-gray-700 dark:text-gray-300">{fmtMoney(taxTotal)}</span>
              </div>
              <div className="h-px bg-gray-100 dark:bg-gray-800 my-1" />
              <div className="flex items-baseline justify-between">
                <span className="text-sm font-bold text-gray-900 dark:text-white uppercase tracking-wide">Total</span>
                <span className="text-3xl font-extrabold tabular-nums text-gray-900 dark:text-white">{fmtMoney(total)}</span>
              </div>
              {items.length > 0 && !pricingReady && (
                <p className="flex items-center justify-end gap-1.5 text-xs text-gray-400">
                  {previewError
                    ? <><AlertCircle className="w-3 h-3 text-red-500" /> Couldn't fetch prices — check the store selection</>
                    : <><Loader2 className="w-3 h-3 animate-spin" /> Updating prices…</>}
                </p>
              )}
            </div>

            {/* Payment */}
            <div className="px-5 pb-4">
              <div className="flex items-center justify-between mb-2">
                <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Payment</p>
                {items.length > 0 && (
                  <button
                    onClick={toggleSplitMode}
                    className="flex items-center gap-1 text-xs font-semibold text-primary-600 dark:text-primary-400 hover:text-primary-800 dark:hover:text-primary-300 transition-colors"
                  >
                    {splitMode
                      ? <><ArrowLeftRight className="w-3 h-3" /> Single payment</>
                      : <><SplitSquareHorizontal className="w-3 h-3" /> Split payment</>
                    }
                  </button>
                )}
              </div>

              {splitMode ? (
                <SplitPaymentPanel
                  total={total}
                  payments={splitPayments}
                  onAdd={(p) => setSplitPayments((prev) => [...prev, p])}
                  disabledMethods={disabledMethods}
                  pointsMaxAmount={pointsMaxAmount}
                  onRemove={(id) => setSplitPayments((prev) => prev.filter((p) => p.tempId !== id))}
                />
              ) : (
                <div className="flex flex-wrap gap-2">
                  {PAYMENT_METHODS.map((m) => (
                    <button
                      key={m.value}
                      onClick={() => {
                        setPayMethod(m.value);
                        setGiftCardCode(''); setMobileNum(''); setTendered('');
                      }}
                      disabled={!!disabledMethods[m.value]}
                      title={disabledMethods[m.value] ?? m.hint}
                      className={cn(
                        'flex items-center gap-1.5 pl-2 pr-3 py-1.5 rounded-full border-2 text-xs font-semibold transition-all',
                        'disabled:opacity-40 disabled:cursor-not-allowed',
                        payMethod === m.value
                          ? 'border-primary-600 bg-primary-50 dark:bg-primary-900/20 text-primary-700 dark:text-primary-400 shadow-sm'
                          : 'border-gray-200 dark:border-gray-700 text-gray-500 dark:text-gray-400 hover:border-gray-300 dark:hover:border-gray-600 hover:text-gray-700 dark:hover:text-gray-300',
                      )}
                    >
                      <PaymentMethodIcon method={m.value} size={18} />
                      {m.label}
                    </button>
                  ))}
                </div>
              )}
            </div>

            {/* Cash tendering — single-payment mode only */}
            {!splitMode && isCash && total > 0 && (
              <div className="px-5 pb-4 space-y-2">
                <label className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider block">
                  Cash received
                </label>
                <div className="relative">
                  <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 font-semibold">৳</span>
                  <input
                    type="number"
                    inputMode="decimal"
                    min={0}
                    value={tendered}
                    onChange={(e) => setTendered(e.target.value)}
                    placeholder={total.toFixed(2)}
                    className={cn(
                      'w-full pl-8 pr-3 py-2.5 text-lg font-bold tabular-nums border-2 rounded-xl bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-4 transition',
                      cashShort
                        ? 'border-red-300 dark:border-red-800 focus:border-red-500 focus:ring-red-500/10'
                        : 'border-gray-200 dark:border-gray-700 focus:border-primary-500 focus:ring-primary-500/10',
                    )}
                  />
                </div>
                <div className="flex gap-1.5 flex-wrap">
                  <button
                    onClick={() => setTendered(total.toFixed(2))}
                    className="px-3 py-1.5 text-xs font-bold rounded-lg bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700 transition"
                  >
                    Exact
                  </button>
                  {quickTenders.map((v) => (
                    <button
                      key={v}
                      onClick={() => setTendered(String(v))}
                      className="px-3 py-1.5 text-xs font-bold tabular-nums rounded-lg bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700 transition"
                    >
                      {fmtMoneyShort(v)}
                    </button>
                  ))}
                </div>
                {cashShort && (
                  <p className="text-xs font-medium text-red-600 dark:text-red-400">
                    Short by {fmtMoney(total - tenderedNum)}
                  </p>
                )}
                {changeDue > 0 && (
                  <div className="flex items-center justify-between px-3.5 py-2.5 bg-emerald-50 dark:bg-emerald-900/20 border border-emerald-200 dark:border-emerald-800 rounded-xl">
                    <span className="text-sm font-semibold text-emerald-700 dark:text-emerald-400">Change due</span>
                    <span className="text-xl font-extrabold tabular-nums text-emerald-700 dark:text-emerald-400">{fmtMoney(changeDue)}</span>
                  </div>
                )}
              </div>
            )}

            {/* Paying with points — single-payment mode */}
            {!splitMode && payMethod === 'RewardPoints' && total > 0 && points && (
              <div className="px-5 pb-4">
                {pointsCoverWholeSale ? (
                  <div className="flex items-center justify-between px-3.5 py-2.5 rounded-xl bg-amber-50 dark:bg-amber-900/15 border border-amber-200 dark:border-amber-800">
                    <span className="text-sm text-amber-800 dark:text-amber-300">
                      Uses <b className="tabular-nums">{Math.ceil(total / points.exchangeRate).toLocaleString()}</b> of {points.availablePoints.toLocaleString()} pts
                    </span>
                    <Star className="w-4 h-4 text-amber-500 fill-amber-400" />
                  </div>
                ) : (
                  <div className="p-3 rounded-xl bg-amber-50 dark:bg-amber-900/15 border border-amber-200 dark:border-amber-800 space-y-2">
                    <p className="text-sm text-amber-800 dark:text-amber-300">
                      Points can cover <b className="tabular-nums">{fmtMoney(pointsMaxAmount ?? 0)}</b> of this {fmtMoney(total)} sale.
                    </p>
                    <button
                      onClick={splitWithPoints}
                      disabled={!pointsMaxAmount}
                      className="w-full flex items-center justify-center gap-1.5 px-3 py-2 text-sm font-semibold rounded-lg bg-amber-500 hover:bg-amber-600 text-white transition disabled:opacity-40"
                    >
                      <SplitSquareHorizontal className="w-4 h-4" />
                      Use {fmtMoney(pointsMaxAmount ?? 0)} in points, pay the rest
                    </button>
                  </div>
                )}
              </div>
            )}

            {/* Conditional payment fields — single-payment mode only */}
            {!splitMode && needsMobile && (
              <div className="px-5 pb-4">
                <label className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-1.5 block">
                  Mobile number *
                </label>
                <input
                  type="tel"
                  value={mobileNum}
                  onChange={(e) => setMobileNum(e.target.value)}
                  placeholder="01XXXXXXXXX"
                  className="w-full px-3 py-2.5 text-sm font-medium border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 rounded-xl text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
                />
              </div>
            )}
            {!splitMode && needsGiftCard && (
              <div className="px-5 pb-4">
                <label className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-1.5 block">
                  Gift card code *
                </label>
                <input
                  type="text"
                  value={giftCardCode}
                  onChange={(e) => setGiftCardCode(e.target.value.toUpperCase())}
                  placeholder="GC-XXXX-XXXX"
                  className="w-full px-3 py-2.5 text-sm font-mono font-medium border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 rounded-xl text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500 uppercase"
                />
              </div>
            )}
            {!splitMode && !isCash && (
              <div className="px-5 pb-4">
                <input
                  type="text"
                  value={payRef}
                  onChange={(e) => setPayRef(e.target.value)}
                  placeholder="Reference / note (optional)"
                  className="w-full px-3 py-2 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 rounded-xl text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500 placeholder-gray-400"
                />
              </div>
            )}

            {/* Errors & hints */}
            {submitError && (
              <div className="mx-5 mb-4 flex items-start gap-2 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-xl">
                <AlertCircle className="w-4 h-4 text-red-500 flex-shrink-0 mt-0.5" />
                <p className="text-xs text-red-700 dark:text-red-400">{submitError}</p>
              </div>
            )}
            {!setupOk && (
              <div className="mx-5 mb-4 flex items-center gap-2 p-3 bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800 rounded-xl">
                <AlertCircle className="w-4 h-4 text-amber-500 flex-shrink-0" />
                <p className="text-xs text-amber-700 dark:text-amber-400">Select store and terminal (top right) to start selling.</p>
              </div>
            )}
            {setupOk && splitMode && splitBalanceDue > 0.005 && items.length > 0 && (
              <div className="mx-5 mb-4 flex items-center gap-2 p-3 bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800 rounded-xl">
                <AlertCircle className="w-4 h-4 text-amber-500 flex-shrink-0" />
                <p className="text-xs text-amber-700 dark:text-amber-400">
                  Add {fmtMoney(splitBalanceDue)} more in payments to complete the sale.
                </p>
              </div>
            )}

            {/* Charge button */}
            <div className="p-5 pt-0">
              <button
                onClick={handleSubmit}
                disabled={!canSubmit || items.length === 0}
                className={cn(
                  'w-full flex items-center justify-center gap-2.5 px-5 py-4 rounded-2xl text-base font-bold text-white transition-all shadow-md',
                  canSubmit && items.length > 0
                    ? 'bg-emerald-600 hover:bg-emerald-700 active:scale-[0.99] shadow-emerald-600/20'
                    : 'bg-gray-300 dark:bg-gray-700 cursor-not-allowed shadow-none',
                )}
              >
                {isSubmitting
                  ? <><Loader2 className="w-5 h-5 animate-spin" /> Processing…</>
                  : <>Charge {fmtMoney(total)}</>
                }
              </button>
              <Link
                to="/sales/transactions"
                className="block w-full text-center mt-2 px-5 py-2 text-sm font-medium text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition"
              >
                Cancel sale
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
