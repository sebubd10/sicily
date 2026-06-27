import { useState, useRef, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import {
  ShoppingCart, ChevronRight, Search, X, Plus, Minus,
  Trash2, User, CreditCard, CheckCircle2, Loader2,
  AlertCircle, Package, Receipt, Tag,
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

/* ── Types ────────────────────────────────────────────────────────────────── */

interface DraftItem {
  tempId: string;
  productId: string;
  productName: string;
  productSku: string;
  quantity: number;
  unitPrice: number;
  taxRate: number;
}

const PAYMENT_METHODS = [
  { value: 'Cash',         label: 'Cash' },
  { value: 'Card',         label: 'Card' },
  { value: 'BKash',        label: 'bKash' },
  { value: 'Nagad',        label: 'Nagad' },
  { value: 'Credit',       label: 'Credit Account' },
  { value: 'GiftCard',     label: 'Gift Card' },
  { value: 'RewardPoints', label: 'Reward Points' },
];

function fmtMoney(n: number) {
  return `৳${n.toLocaleString('en-BD', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

/* ── Product Search Dropdown ──────────────────────────────────────────────── */

function ProductSearchDropdown({
  onSelect,
}: { onSelect: (p: ProductListItem) => void }) {
  const [term, setTerm] = useState('');
  const [open, setOpen]  = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  const { data: results = [], isFetching } = useProductSearch(term);

  useEffect(() => {
    function handler(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  function handleSelect(p: ProductListItem) {
    onSelect(p);
    setTerm('');
    setOpen(false);
  }

  return (
    <div ref={ref} className="relative">
      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
        <input
          value={term}
          onChange={(e) => { setTerm(e.target.value); setOpen(true); }}
          onFocus={() => term.length >= 2 && setOpen(true)}
          placeholder="Search product by name or SKU…"
          className="w-full pl-9 pr-9 py-2.5 text-sm border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 rounded-xl text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500 placeholder-gray-400"
        />
        {isFetching
          ? <Loader2 className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 animate-spin text-gray-400" />
          : term && (
            <button onClick={() => { setTerm(''); setOpen(false); }}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600">
              <X className="w-4 h-4" />
            </button>
          )
        }
      </div>

      {open && term.length >= 2 && (
        <div className="absolute top-full left-0 right-0 z-50 mt-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl shadow-lg overflow-hidden">
          {results.length === 0
            ? (
              <div className="px-4 py-6 text-center text-sm text-gray-400">
                {isFetching ? 'Searching…' : 'No products found'}
              </div>
            )
            : results.filter(p => p.status === 'Active').map((p) => (
              <button
                key={p.id}
                onClick={() => handleSelect(p)}
                className="w-full flex items-center gap-3 px-4 py-3 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors text-left"
              >
                <div className="w-8 h-8 bg-gray-100 dark:bg-gray-700 rounded-lg flex items-center justify-center flex-shrink-0">
                  <Package className="w-4 h-4 text-gray-500 dark:text-gray-400" />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-900 dark:text-white truncate">{p.name}</p>
                  <p className="text-xs text-gray-500 dark:text-gray-400 font-mono">{p.sku}</p>
                </div>
                <div className="text-right flex-shrink-0">
                  <p className="text-sm font-semibold text-gray-900 dark:text-white">{fmtMoney(p.price)}</p>
                  {p.vatRate > 0 && <p className="text-xs text-gray-400">+{p.vatRate}% VAT</p>}
                </div>
              </button>
            ))
          }
        </div>
      )}
    </div>
  );
}

/* ── Customer Search Dropdown ─────────────────────────────────────────────── */

function CustomerSearchDropdown({
  value,
  onChange,
}: { value: Customer | null; onChange: (c: Customer | null) => void }) {
  const [term, setTerm] = useState('');
  const [open, setOpen]  = useState(false);
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
      <div className="flex items-center gap-2 px-3 py-2 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-xl">
        <User className="w-4 h-4 text-blue-600 dark:text-blue-400 flex-shrink-0" />
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium text-blue-800 dark:text-blue-300 truncate">{value.name}</p>
          {value.phone && <p className="text-xs text-blue-500 dark:text-blue-400">{value.phone}</p>}
        </div>
        <button onClick={() => onChange(null)}
          className="text-blue-400 hover:text-blue-600 transition-colors">
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
          placeholder="Search customer (optional)…"
          className="w-full pl-9 pr-4 py-2 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 rounded-xl text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500 placeholder-gray-400"
        />
        {isFetching && (
          <Loader2 className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 animate-spin text-gray-400" />
        )}
      </div>

      {open && term.length >= 2 && results.length > 0 && (
        <div className="absolute top-full left-0 right-0 z-50 mt-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl shadow-lg overflow-hidden max-h-48 overflow-y-auto">
          {results.map((c: Customer) => (
            <button
              key={c.id}
              onClick={() => { onChange(c); setTerm(''); setOpen(false); }}
              className="w-full flex items-center gap-3 px-4 py-2.5 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors text-left"
            >
              <div>
                <p className="text-sm font-medium text-gray-900 dark:text-white">{c.name}</p>
                <p className="text-xs text-gray-500 dark:text-gray-400">{c.phone}</p>
              </div>
              {(c.loyaltyPoints ?? 0) > 0 && (
                <span className="ml-auto text-xs text-amber-600 dark:text-amber-400 font-medium">
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

/* ── Main Page ────────────────────────────────────────────────────────────── */

export default function TransactionNewPage() {
  const navigate    = useNavigate();
  const qc          = useQueryClient();

  const [storeId,      setStoreId]      = useState('');
  const [terminalId,   setTerminalId]   = useState('');
  const [customer,     setCustomer]     = useState<Customer | null>(null);
  const [items,        setItems]        = useState<DraftItem[]>([]);
  const [payMethod,    setPayMethod]    = useState('Cash');
  const [payRef,       setPayRef]       = useState('');
  const [giftCardCode, setGiftCardCode] = useState('');
  const [mobileNum,    setMobileNum]    = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError,  setSubmitError]  = useState('');

  const { data: stores    = [] } = useStoreList();
  const { data: terminals = [] } = useTerminals();

  const storeTerminals = storeId
    ? terminals.filter((t) => t.storeId === storeId && t.status === 'Active')
    : terminals.filter((t) => t.status === 'Active');

  function handleStoreChange(id: string) {
    setStoreId(id);
    setTerminalId('');
  }

  function addProduct(p: ProductListItem) {
    setItems((prev) => {
      const existing = prev.find((i) => i.productId === p.id);
      if (existing) {
        return prev.map((i) => i.productId === p.id ? { ...i, quantity: i.quantity + 1 } : i);
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
    if (qty < 1) return;
    setItems((prev) => prev.map((i) => i.tempId === tempId ? { ...i, quantity: qty } : i));
  }

  function removeItem(tempId: string) {
    setItems((prev) => prev.filter((i) => i.tempId !== tempId));
  }

  // Computed totals
  const lineItems = items.map((i) => {
    const lineSubTotal = i.unitPrice * i.quantity;
    const taxAmt       = lineSubTotal * (i.taxRate / 100);
    return { ...i, lineSubTotal, taxAmt, lineTotal: lineSubTotal + taxAmt };
  });
  const subTotal   = lineItems.reduce((s, i) => s + i.lineSubTotal, 0);
  const taxTotal   = lineItems.reduce((s, i) => s + i.taxAmt, 0);
  const total      = subTotal + taxTotal;

  const needsMobile    = ['BKash', 'Nagad', 'Rocket'].includes(payMethod);
  const needsGiftCard  = payMethod === 'GiftCard';
  const canSubmit = storeId && terminalId && items.length > 0;

  async function handleSubmit() {
    if (!storeId || !terminalId || items.length === 0) return;
    setIsSubmitting(true);
    setSubmitError('');

    let txnId: string | null = null;
    try {
      // 1. Create transaction
      const txn = await transactionsApi.createTransaction({
        storeId,
        terminalId,
        customerId: customer?.id,
      });
      txnId = txn.id;

      // 2. Add items
      for (const item of items) {
        await transactionsApi.addLineItem(txn.id, item.productId, item.quantity);
      }

      // 3. Add payment
      await transactionsApi.addPayment(
        txn.id, payMethod, total,
        payRef || undefined,
        needsGiftCard ? giftCardCode : undefined,
      );

      // 4. Complete
      await transactionsApi.completeTransaction(txn.id);

      qc.invalidateQueries({ queryKey: ['transactions'] });
      navigate(`/sales/transactions/${txn.id}`);
    } catch (e) {
      setSubmitError(extractApiError(e));
      if (txnId) {
        setSubmitError(
          extractApiError(e) + ' The draft transaction was saved — you can find and void it in the transaction list.',
        );
      }
      setIsSubmitting(false);
    }
  }

  const selectCls = 'w-full text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-xl px-3 py-2.5 text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 cursor-pointer transition';

  return (
    <div className="max-w-6xl mx-auto space-y-5">
      {/* Breadcrumb */}
      <nav className="flex items-center gap-2 text-sm text-gray-500 dark:text-gray-400">
        <Link to="/sales/transactions" className="hover:text-primary-600 transition-colors flex items-center gap-1">
          <ShoppingCart className="w-3.5 h-3.5" />
          Transactions
        </Link>
        <ChevronRight className="w-3.5 h-3.5" />
        <span className="text-gray-700 dark:text-gray-300 font-medium">New Transaction</span>
      </nav>

      {/* Header */}
      <div className="flex items-center gap-3">
        <div className="p-2.5 bg-primary-50 dark:bg-primary-900/20 rounded-xl">
          <Receipt className="w-5 h-5 text-primary-600" />
        </div>
        <div>
          <h1 className="text-xl font-bold text-gray-900 dark:text-white">New Transaction</h1>
          <p className="text-sm text-gray-500 dark:text-gray-400">Create a manual sale from the backoffice</p>
        </div>
      </div>

      {/* Setup bar */}
      <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm p-4">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div>
            <label className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-1.5 block">Store *</label>
            <select value={storeId} onChange={(e) => handleStoreChange(e.target.value)} className={selectCls}>
              <option value="">Select store…</option>
              {stores.filter((s) => s.status === 'Active').map((s) => (
                <option key={s.id} value={s.id}>{s.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-1.5 block">Terminal *</label>
            <select
              value={terminalId}
              onChange={(e) => setTerminalId(e.target.value)}
              disabled={!storeId || storeTerminals.length === 0}
              className={selectCls}
            >
              <option value="">Select terminal…</option>
              {storeTerminals.map((t) => (
                <option key={t.id} value={t.id}>{t.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-1.5 block">Customer</label>
            <CustomerSearchDropdown value={customer} onChange={setCustomer} />
          </div>
        </div>
      </div>

      {/* Main body — two columns */}
      <div className="grid grid-cols-1 lg:grid-cols-5 gap-5">

        {/* Left: product search + items */}
        <div className="lg:col-span-3 space-y-4">
          {/* Product search */}
          <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm p-4">
            <label className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-2 block">
              Add Products
            </label>
            <ProductSearchDropdown onSelect={addProduct} />
            <p className="text-xs text-gray-400 mt-2">Type at least 2 characters to search. Click a result to add it to the cart.</p>
          </div>

          {/* Items list */}
          <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
            <div className="px-4 py-3 border-b border-gray-100 dark:border-gray-800 flex items-center justify-between">
              <h3 className="text-sm font-semibold text-gray-900 dark:text-white flex items-center gap-2">
                <ShoppingCart className="w-4 h-4 text-primary-600" />
                Items ({items.length})
              </h3>
              {items.length > 0 && (
                <button onClick={() => setItems([])}
                  className="text-xs text-red-500 hover:text-red-700 transition-colors">
                  Clear all
                </button>
              )}
            </div>

            {items.length === 0 ? (
              <div className="px-4 py-12 text-center">
                <Package className="w-8 h-8 text-gray-200 dark:text-gray-700 mx-auto mb-2" />
                <p className="text-sm text-gray-400">No items added yet.</p>
                <p className="text-xs text-gray-400 mt-0.5">Use the product search above to add items.</p>
              </div>
            ) : (
              <div className="divide-y divide-gray-100 dark:divide-gray-800">
                {lineItems.map((item) => (
                  <div key={item.tempId} className="flex items-center gap-3 px-4 py-3 group hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-gray-900 dark:text-white truncate">{item.productName}</p>
                      <p className="text-xs text-gray-500 dark:text-gray-400 font-mono">{item.productSku}</p>
                    </div>

                    {/* Qty controls */}
                    <div className="flex items-center gap-1.5 border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden">
                      <button
                        onClick={() => updateQty(item.tempId, item.quantity - 1)}
                        disabled={item.quantity <= 1}
                        className="p-1 text-gray-500 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-30 transition-colors"
                      >
                        <Minus className="w-3.5 h-3.5" />
                      </button>
                      <input
                        type="number"
                        min={1}
                        value={item.quantity}
                        onChange={(e) => updateQty(item.tempId, Number(e.target.value))}
                        className="w-10 text-center text-sm bg-transparent text-gray-900 dark:text-white focus:outline-none"
                      />
                      <button
                        onClick={() => updateQty(item.tempId, item.quantity + 1)}
                        className="p-1 text-gray-500 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
                      >
                        <Plus className="w-3.5 h-3.5" />
                      </button>
                    </div>

                    {/* Unit price */}
                    <div className="text-right w-20 flex-shrink-0">
                      <p className="text-sm font-semibold text-gray-900 dark:text-white tabular-nums">{fmtMoney(item.lineTotal)}</p>
                      <p className="text-xs text-gray-400 tabular-nums">{fmtMoney(item.unitPrice)} each</p>
                    </div>

                    <button
                      onClick={() => removeItem(item.tempId)}
                      className="opacity-0 group-hover:opacity-100 p-1 text-red-400 hover:text-red-600 transition-all"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Right: Summary + Payment */}
        <div className="lg:col-span-2 space-y-4">
          {/* Totals */}
          <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm p-5 space-y-3">
            <h3 className="text-sm font-semibold text-gray-900 dark:text-white flex items-center gap-2">
              <Tag className="w-4 h-4 text-primary-600" />
              Order Summary
            </h3>
            <div className="space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-gray-500 dark:text-gray-400">Subtotal</span>
                <span className="tabular-nums text-gray-700 dark:text-gray-300">{fmtMoney(subTotal)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500 dark:text-gray-400">Tax</span>
                <span className="tabular-nums text-gray-700 dark:text-gray-300">{fmtMoney(taxTotal)}</span>
              </div>
              <div className="h-px bg-gray-100 dark:bg-gray-800" />
              <div className="flex justify-between">
                <span className="font-bold text-gray-900 dark:text-white">Total</span>
                <span className="text-xl font-bold tabular-nums text-primary-700 dark:text-primary-400">{fmtMoney(total)}</span>
              </div>
            </div>
          </div>

          {/* Payment */}
          <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm p-5 space-y-4">
            <h3 className="text-sm font-semibold text-gray-900 dark:text-white flex items-center gap-2">
              <CreditCard className="w-4 h-4 text-primary-600" />
              Payment
            </h3>

            <div className="space-y-3">
              {/* Method */}
              <div>
                <label className="text-xs font-medium text-gray-600 dark:text-gray-400 mb-1 block">Method</label>
                <select
                  value={payMethod}
                  onChange={(e) => { setPayMethod(e.target.value); setGiftCardCode(''); setMobileNum(''); }}
                  className="w-full text-sm border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 rounded-xl px-3 py-2 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
                >
                  {PAYMENT_METHODS.map((m) => (
                    <option key={m.value} value={m.value}>{m.label}</option>
                  ))}
                </select>
              </div>

              {/* Mobile number for mobile wallets */}
              {needsMobile && (
                <div>
                  <label className="text-xs font-medium text-gray-600 dark:text-gray-400 mb-1 block">Mobile Number *</label>
                  <input
                    type="tel"
                    value={mobileNum}
                    onChange={(e) => setMobileNum(e.target.value)}
                    placeholder="01XXXXXXXXX"
                    className="w-full text-sm border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 rounded-xl px-3 py-2 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
                  />
                </div>
              )}

              {/* Gift card code */}
              {needsGiftCard && (
                <div>
                  <label className="text-xs font-medium text-gray-600 dark:text-gray-400 mb-1 block">Gift Card Code *</label>
                  <input
                    type="text"
                    value={giftCardCode}
                    onChange={(e) => setGiftCardCode(e.target.value.toUpperCase())}
                    placeholder="GC-XXXX-XXXX"
                    className="w-full text-sm font-mono border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 rounded-xl px-3 py-2 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500 uppercase"
                  />
                </div>
              )}

              {/* Reference */}
              <div>
                <label className="text-xs font-medium text-gray-600 dark:text-gray-400 mb-1 block">Reference / Note</label>
                <input
                  type="text"
                  value={payRef}
                  onChange={(e) => setPayRef(e.target.value)}
                  placeholder="Optional reference…"
                  className="w-full text-sm border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 rounded-xl px-3 py-2 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
                />
              </div>

              {/* Amount display */}
              <div className="p-3 bg-gray-50 dark:bg-gray-800 rounded-xl">
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600 dark:text-gray-400">Amount to charge</span>
                  <span className="text-lg font-bold tabular-nums text-gray-900 dark:text-white">{fmtMoney(total)}</span>
                </div>
              </div>
            </div>
          </div>

          {/* Validation hints */}
          {(!storeId || !terminalId) && (
            <div className="flex items-center gap-2 p-3 bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800 rounded-xl">
              <AlertCircle className="w-4 h-4 text-amber-500 flex-shrink-0" />
              <p className="text-xs text-amber-700 dark:text-amber-400">Select a store and terminal to continue.</p>
            </div>
          )}
          {storeId && terminalId && items.length === 0 && (
            <div className="flex items-center gap-2 p-3 bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800 rounded-xl">
              <AlertCircle className="w-4 h-4 text-amber-500 flex-shrink-0" />
              <p className="text-xs text-amber-700 dark:text-amber-400">Add at least one product to proceed.</p>
            </div>
          )}

          {submitError && (
            <div className="flex items-start gap-2 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-xl">
              <AlertCircle className="w-4 h-4 text-red-500 flex-shrink-0 mt-0.5" />
              <p className="text-xs text-red-700 dark:text-red-400">{submitError}</p>
            </div>
          )}

          {/* Actions */}
          <div className="flex flex-col gap-2">
            <button
              onClick={handleSubmit}
              disabled={!canSubmit || isSubmitting || (needsMobile && !mobileNum) || (needsGiftCard && !giftCardCode)}
              className="w-full flex items-center justify-center gap-2 px-5 py-3 text-sm font-semibold text-white bg-emerald-600 hover:bg-emerald-700 rounded-xl transition disabled:opacity-40 shadow-sm"
            >
              {isSubmitting
                ? <Loader2 className="w-4 h-4 animate-spin" />
                : <CheckCircle2 className="w-4 h-4" />
              }
              {isSubmitting ? 'Processing…' : `Complete Sale — ${fmtMoney(total)}`}
            </button>
            <Link
              to="/sales/transactions"
              className="w-full text-center px-5 py-2.5 text-sm font-medium text-gray-600 dark:text-gray-400 bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 rounded-xl transition"
            >
              Cancel
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
