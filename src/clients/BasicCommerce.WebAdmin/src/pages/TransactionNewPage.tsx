import { useState, useRef, useEffect, useMemo } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import {
  ShoppingCart, ChevronRight, X, Plus, Minus,
  Trash2, User, CreditCard, Loader2, AlertCircle, Package,
  Receipt, ScanLine, Banknote, Smartphone, Gift, Star,
  Wallet, Store as StoreIcon, Monitor, CheckCircle2, Delete,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { ProductListItem } from '../types/product';
import type { Customer } from '../types/customer';
import { useStoreList } from '../hooks/useStoreAdmin';
import { useTerminals } from '../hooks/useTerminals';
import { useProductSearch } from '../hooks/useProducts';
import { useCustomerSearch } from '../hooks/useCustomers';
import * as transactionsApi from '../api/transactionsApi';
import { useQueryClient } from '@tanstack/react-query';

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

const PAYMENT_METHODS: {
  value: string; label: string; icon: React.ReactNode; hint?: string;
}[] = [
  { value: 'Cash',         label: 'Cash',          icon: <Banknote className="w-5 h-5" /> },
  { value: 'Card',         label: 'Card',          icon: <CreditCard className="w-5 h-5" /> },
  { value: 'BKash',        label: 'bKash',         icon: <Smartphone className="w-5 h-5" /> },
  { value: 'Nagad',        label: 'Nagad',         icon: <Smartphone className="w-5 h-5" /> },
  { value: 'Rocket',       label: 'Rocket',        icon: <Smartphone className="w-5 h-5" /> },
  { value: 'Credit',       label: 'Credit',        icon: <Wallet className="w-5 h-5" />,  hint: 'Customer account' },
  { value: 'GiftCard',     label: 'Gift Card',     icon: <Gift className="w-5 h-5" /> },
  { value: 'RewardPoints', label: 'Points',        icon: <Star className="w-5 h-5" />,    hint: 'Loyalty points' },
];

const SETUP_STORAGE_KEY = 'pos-new-txn-setup';

function fmtMoney(n: number) {
  return `৳${n.toLocaleString('en-BD', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

function fmtMoneyShort(n: number) {
  return `৳${n.toLocaleString('en-BD', { maximumFractionDigits: 0 })}`;
}

/* ── Product search (keyboard-first, scanner-friendly) ────────────────────── */

function ProductSearch({ onSelect }: { onSelect: (p: ProductListItem) => void }) {
  const [term, setTerm]           = useState('');
  const [open, setOpen]           = useState(false);
  const [highlight, setHighlight] = useState(0);
  const wrapRef  = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const { data: rawResults = [], isFetching } = useProductSearch(term);
  const results = rawResults.filter((p) => p.status === 'Active');

  // Auto-focus on mount — cashier can start typing / scanning immediately
  useEffect(() => { inputRef.current?.focus(); }, []);

  useEffect(() => {
    function handler(e: MouseEvent) {
      if (wrapRef.current && !wrapRef.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  useEffect(() => { setHighlight(0); }, [term]);

  function pick(p: ProductListItem) {
    onSelect(p);
    setTerm('');
    setOpen(false);
    inputRef.current?.focus();
  }

  function handleKeyDown(e: React.KeyboardEvent) {
    if (!open || results.length === 0) {
      // Enter with an exact SKU (barcode scanner) — pick the exact match if present
      if (e.key === 'Enter' && term && results.length === 0) e.preventDefault();
      return;
    }
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      setHighlight((h) => Math.min(h + 1, results.length - 1));
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      setHighlight((h) => Math.max(h - 1, 0));
    } else if (e.key === 'Enter') {
      e.preventDefault();
      // Barcode scanners type the code and send Enter — prefer an exact SKU match
      const exact = results.find((p) => p.sku.toLowerCase() === term.trim().toLowerCase());
      pick(exact ?? results[highlight] ?? results[0]);
    } else if (e.key === 'Escape') {
      setOpen(false);
    }
  }

  return (
    <div ref={wrapRef} className="relative">
      <div className="relative">
        <ScanLine className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-primary-500 pointer-events-none" />
        <input
          ref={inputRef}
          value={term}
          onChange={(e) => { setTerm(e.target.value); setOpen(true); }}
          onFocus={() => term.length >= 2 && setOpen(true)}
          onKeyDown={handleKeyDown}
          placeholder="Scan barcode or search product name / SKU…"
          className="w-full pl-12 pr-12 py-3.5 text-base border-2 border-primary-200 dark:border-primary-900 bg-white dark:bg-gray-900 rounded-2xl text-gray-900 dark:text-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 placeholder-gray-400 shadow-sm transition"
        />
        {isFetching
          ? <Loader2 className="absolute right-4 top-1/2 -translate-y-1/2 w-5 h-5 animate-spin text-gray-400" />
          : term && (
            <button onClick={() => { setTerm(''); setOpen(false); inputRef.current?.focus(); }}
              className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 p-0.5">
              <X className="w-5 h-5" />
            </button>
          )
        }
      </div>

      {open && term.length >= 2 && (
        <div className="absolute top-full left-0 right-0 z-50 mt-2 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-2xl shadow-xl overflow-hidden">
          {results.length === 0 ? (
            <div className="px-4 py-8 text-center text-sm text-gray-400">
              {isFetching ? 'Searching…' : `No products match “${term}”`}
            </div>
          ) : (
            <>
              {results.map((p, idx) => (
                <button
                  key={p.id}
                  onClick={() => pick(p)}
                  onMouseEnter={() => setHighlight(idx)}
                  className={cn(
                    'w-full flex items-center gap-3 px-4 py-3 transition-colors text-left',
                    idx === highlight
                      ? 'bg-primary-50 dark:bg-primary-900/20'
                      : 'hover:bg-gray-50 dark:hover:bg-gray-700',
                  )}
                >
                  <div className="w-10 h-10 bg-gray-100 dark:bg-gray-700 rounded-xl flex items-center justify-center flex-shrink-0 overflow-hidden">
                    {p.imageUrl
                      ? <img src={p.imageUrl} alt="" className="w-full h-full object-cover" />
                      : <Package className="w-5 h-5 text-gray-400" />
                    }
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-gray-900 dark:text-white truncate">{p.name}</p>
                    <p className="text-xs text-gray-500 dark:text-gray-400 font-mono">{p.sku}</p>
                  </div>
                  <div className="text-right flex-shrink-0">
                    <p className="text-sm font-bold text-gray-900 dark:text-white tabular-nums">{fmtMoney(p.price)}</p>
                    {p.vatRate > 0 && <p className="text-xs text-gray-400">+{p.vatRate}% VAT</p>}
                  </div>
                </button>
              ))}
              <div className="px-4 py-2 bg-gray-50 dark:bg-gray-900/60 border-t border-gray-100 dark:border-gray-700 flex items-center gap-3 text-xs text-gray-400">
                <span><kbd className="px-1 py-0.5 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-600 rounded">↑↓</kbd> navigate</span>
                <span><kbd className="px-1 py-0.5 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-600 rounded">Enter</kbd> add to cart</span>
              </div>
            </>
          )}
        </div>
      )}
    </div>
  );
}

/* ── Customer picker ──────────────────────────────────────────────────────── */

function CustomerPicker({
  value, onChange,
}: { value: Customer | null; onChange: (c: Customer | null) => void }) {
  const [term, setTerm] = useState('');
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  const { data: results = [], isFetching } = useCustomerSearch(term);

  useEffect(() => {
    function handler(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  if (value) {
    return (
      <div className="flex items-center gap-3 px-3 py-2.5 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-xl">
        <div className="w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900/40 flex items-center justify-center flex-shrink-0">
          <User className="w-4 h-4 text-blue-600 dark:text-blue-400" />
        </div>
        <div className="flex-1 min-w-0">
          <p className="text-sm font-semibold text-blue-900 dark:text-blue-300 truncate">{value.name}</p>
          <p className="text-xs text-blue-500 dark:text-blue-400">
            {value.phone ?? '—'}
            {(value.loyaltyPoints ?? 0) > 0 && ` · ${value.loyaltyPoints} pts`}
          </p>
        </div>
        <button onClick={() => onChange(null)}
          className="p-1 text-blue-400 hover:text-blue-600 transition-colors" title="Remove customer">
          <X className="w-4 h-4" />
        </button>
      </div>
    );
  }

  return (
    <div ref={ref} className="relative">
      <div className="relative">
        <User className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
        <input
          value={term}
          onChange={(e) => { setTerm(e.target.value); setOpen(true); }}
          onFocus={() => term.length >= 2 && setOpen(true)}
          placeholder="Walk-in — search to attach customer"
          className="w-full pl-9 pr-4 py-2.5 text-sm border border-dashed border-gray-300 dark:border-gray-600 bg-transparent rounded-xl text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-solid placeholder-gray-400 transition"
        />
        {isFetching && (
          <Loader2 className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 animate-spin text-gray-400" />
        )}
      </div>

      {open && term.length >= 2 && results.length > 0 && (
        <div className="absolute top-full left-0 right-0 z-50 mt-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl shadow-xl overflow-hidden max-h-56 overflow-y-auto">
          {results.map((c: Customer) => (
            <button
              key={c.id}
              onClick={() => { onChange(c); setTerm(''); setOpen(false); }}
              className="w-full flex items-center justify-between gap-3 px-4 py-2.5 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors text-left"
            >
              <div className="min-w-0">
                <p className="text-sm font-medium text-gray-900 dark:text-white truncate">{c.name}</p>
                <p className="text-xs text-gray-500 dark:text-gray-400">{c.phone}</p>
              </div>
              {(c.loyaltyPoints ?? 0) > 0 && (
                <span className="text-xs text-amber-600 dark:text-amber-400 font-semibold whitespace-nowrap">
                  {c.loyaltyPoints} pts
                </span>
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  );
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

  const lineItems = items.map((i) => {
    const lineSubTotal = i.unitPrice * i.quantity;
    const taxAmt       = lineSubTotal * (i.taxRate / 100);
    return { ...i, lineSubTotal, taxAmt, lineTotal: lineSubTotal + taxAmt };
  });
  const subTotal  = lineItems.reduce((s, i) => s + i.lineSubTotal, 0);
  const taxTotal  = lineItems.reduce((s, i) => s + i.taxAmt, 0);
  const total     = subTotal + taxTotal;
  const itemCount = items.reduce((s, i) => s + i.quantity, 0);

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

  const setupOk   = !!storeId && !!terminalId;
  const canSubmit = setupOk && items.length > 0 && !isSubmitting
    && !(needsMobile && !mobileNum)
    && !(needsGiftCard && !giftCardCode)
    && !cashShort;

  /* ── Submit ── */

  async function handleSubmit() {
    if (!canSubmit) return;
    setIsSubmitting(true);
    setSubmitError('');

    let txnId: string | null = null;
    try {
      const txn = await transactionsApi.createTransaction({
        storeId, terminalId, customerId: customer?.id,
      });
      txnId = txn.id;

      for (const item of items) {
        await transactionsApi.addLineItem(txn.id, item.productId, item.quantity);
      }

      // For cash send the tendered amount so the backend records change due
      const payAmount = isCash && tenderedNum > total ? tenderedNum : total;
      await transactionsApi.addPayment(
        txn.id, payMethod, payAmount,
        payRef || undefined,
        needsGiftCard ? giftCardCode : undefined,
      );

      await transactionsApi.completeTransaction(txn.id);

      qc.invalidateQueries({ queryKey: ['transactions'] });
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
                      <p className="text-sm font-bold text-gray-900 dark:text-white tabular-nums">{fmtMoney(item.lineTotal)}</p>
                      {item.taxAmt > 0 && (
                        <p className="text-xs text-gray-400 tabular-nums">incl. {fmtMoney(item.taxAmt)} tax</p>
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
                <span className="text-gray-500 dark:text-gray-400">VAT</span>
                <span className="tabular-nums font-medium text-gray-700 dark:text-gray-300">{fmtMoney(taxTotal)}</span>
              </div>
              <div className="h-px bg-gray-100 dark:bg-gray-800 my-1" />
              <div className="flex items-baseline justify-between">
                <span className="text-sm font-bold text-gray-900 dark:text-white uppercase tracking-wide">Total</span>
                <span className="text-3xl font-extrabold tabular-nums text-gray-900 dark:text-white">{fmtMoney(total)}</span>
              </div>
            </div>

            {/* Payment method grid */}
            <div className="px-5 pb-4">
              <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-2">Payment</p>
              <div className="grid grid-cols-3 gap-2">
                {PAYMENT_METHODS.map((m) => (
                  <button
                    key={m.value}
                    onClick={() => {
                      setPayMethod(m.value);
                      setGiftCardCode(''); setMobileNum(''); setTendered('');
                    }}
                    title={m.hint}
                    className={cn(
                      'flex flex-col items-center gap-1 px-2 py-2.5 rounded-xl border-2 text-xs font-semibold transition-all',
                      payMethod === m.value
                        ? 'border-primary-600 bg-primary-50 dark:bg-primary-900/20 text-primary-700 dark:text-primary-400 shadow-sm'
                        : 'border-gray-200 dark:border-gray-700 text-gray-500 dark:text-gray-400 hover:border-gray-300 dark:hover:border-gray-600 hover:text-gray-700 dark:hover:text-gray-300',
                    )}
                  >
                    {m.icon}
                    {m.label}
                  </button>
                ))}
              </div>
            </div>

            {/* Cash tendering */}
            {isCash && total > 0 && (
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

            {/* Conditional payment fields */}
            {needsMobile && (
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
            {needsGiftCard && (
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
            {!isCash && (
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
