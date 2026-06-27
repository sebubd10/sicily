import { useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import {
  ArrowLeft, RotateCcw, ShoppingCart, ChevronRight,
  AlertCircle, Loader2, Package, AlertTriangle,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { LineItem, ReturnReason, DamageDisposition, ReturnLineItemPayload } from '../types/transaction';
import { useTransactionDetail, useCreateReturn } from '../hooks/useTransactions';

const REFUND_METHODS = [
  { value: 'Cash',         label: 'Cash' },
  { value: 'Card',         label: 'Card' },
  { value: 'BKash',        label: 'bKash' },
  { value: 'Nagad',        label: 'Nagad' },
  { value: 'Credit',       label: 'Credit Account' },
  { value: 'GiftCard',     label: 'Gift Card' },
  { value: 'RewardPoints', label: 'Reward Points' },
];

const RETURN_REASONS: { value: ReturnReason; label: string }[] = [
  { value: 'CustomerChangedMind', label: 'Customer changed mind' },
  { value: 'Defective',          label: 'Defective / damaged' },
  { value: 'WrongItem',          label: 'Wrong item' },
  { value: 'Expired',            label: 'Expired' },
  { value: 'Other',              label: 'Other' },
];

const DISPOSITIONS: { value: DamageDisposition; label: string; desc: string }[] = [
  { value: 'RestoreToStock', label: 'Restore to stock',    desc: 'Item goes back to saleable inventory' },
  { value: 'WriteOff',       label: 'Write off (damage)',  desc: 'Item is damaged and removed from stock' },
];

function fmtMoney(n: number) {
  return `৳${n.toLocaleString('en-BD', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

interface ReturnItemState {
  selected: boolean;
  qty: number;
  reason: ReturnReason;
  disposition: DamageDisposition;
}

export default function TransactionReturnPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: txn, isLoading, isError } = useTransactionDetail(id ?? null);
  const { mutate, isPending } = useCreateReturn();

  const [refundMethod, setRefundMethod] = useState('Cash');
  const [notes, setNotes]               = useState('');
  const [submitError, setSubmitError]   = useState('');
  const [itemState, setItemState]       = useState<Record<string, ReturnItemState>>({});

  function getItemState(itemId: string, maxQty: number): ReturnItemState {
    return itemState[itemId] ?? {
      selected: false, qty: maxQty,
      reason: 'Other', disposition: 'RestoreToStock',
    };
  }

  function updateItem(itemId: string, patch: Partial<ReturnItemState>, maxQty: number) {
    setItemState(prev => ({
      ...prev,
      [itemId]: { ...getItemState(itemId, maxQty), ...patch },
    }));
  }

  const activeItems = txn?.lineItems.filter(l => !l.isVoided) ?? [];
  const selectedItems = activeItems.filter(l => getItemState(l.id, l.quantity).selected);

  const returnTotal = selectedItems.reduce((sum, l) => {
    const s = getItemState(l.id, l.quantity);
    return sum + l.unitPrice * s.qty;
  }, 0);

  function handleSubmit() {
    if (selectedItems.length === 0) {
      setSubmitError('Please select at least one item to return.');
      return;
    }

    const payload: ReturnLineItemPayload[] = selectedItems.map(l => {
      const s = getItemState(l.id, l.quantity);
      return {
        originalLineItemId: l.id,
        quantity: s.qty,
        returnReason: s.reason,
        damageDisposition: s.disposition,
      };
    });

    setSubmitError('');
    mutate(
      { id: id!, payload: { items: payload, refundMethod, notes: notes || undefined } },
      {
        onSuccess: (returnTxn) => navigate(`/sales/transactions/${returnTxn.id}`),
        onError: (e) => setSubmitError(extractApiError(e)),
      },
    );
  }

  if (isLoading) {
    return (
      <div className="flex flex-col items-center justify-center h-64 gap-3">
        <Loader2 className="w-8 h-8 animate-spin text-primary-600" />
        <p className="text-sm text-gray-500 dark:text-gray-400">Loading transaction…</p>
      </div>
    );
  }

  if (isError || !txn) {
    return (
      <div className="flex flex-col items-center justify-center h-64 gap-3">
        <AlertCircle className="w-8 h-8 text-red-500" />
        <p className="text-sm text-gray-600 dark:text-gray-400">Transaction not found or failed to load.</p>
        <button onClick={() => navigate(-1)}
          className="text-sm text-primary-600 hover:underline flex items-center gap-1">
          <ArrowLeft className="w-3.5 h-3.5" /> Back
        </button>
      </div>
    );
  }

  if (txn.status !== 'Completed') {
    return (
      <div className="flex flex-col items-center justify-center h-64 gap-3">
        <AlertTriangle className="w-8 h-8 text-amber-500" />
        <p className="text-sm text-gray-600 dark:text-gray-400">Only completed transactions can be returned.</p>
        <Link to={`/sales/transactions/${id}`}
          className="text-sm text-primary-600 hover:underline flex items-center gap-1">
          <ArrowLeft className="w-3.5 h-3.5" /> Back to transaction
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto space-y-5">
      {/* Breadcrumb */}
      <nav className="flex items-center gap-2 text-sm text-gray-500 dark:text-gray-400">
        <Link to="/sales/transactions" className="hover:text-primary-600 transition-colors flex items-center gap-1">
          <ShoppingCart className="w-3.5 h-3.5" />
          Transactions
        </Link>
        <ChevronRight className="w-3.5 h-3.5" />
        <Link to={`/sales/transactions/${id}`}
          className="hover:text-primary-600 transition-colors font-mono text-gray-700 dark:text-gray-300">
          {txn.transactionNumber}
        </Link>
        <ChevronRight className="w-3.5 h-3.5" />
        <span className="text-gray-700 dark:text-gray-300">Process Return</span>
      </nav>

      {/* Header */}
      <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm p-6">
        <div className="flex items-center gap-3">
          <div className="p-2.5 bg-orange-50 dark:bg-orange-900/20 rounded-xl">
            <RotateCcw className="w-5 h-5 text-orange-600 dark:text-orange-400" />
          </div>
          <div>
            <h1 className="text-xl font-bold text-gray-900 dark:text-white">Process Return</h1>
            <p className="text-sm text-gray-500 dark:text-gray-400 font-mono">{txn.transactionNumber}</p>
          </div>
        </div>
        {txn.customerName && (
          <p className="mt-3 text-sm text-gray-600 dark:text-gray-400">
            Customer: <span className="font-medium text-gray-900 dark:text-white">{txn.customerName}</span>
          </p>
        )}
      </div>

      {/* Item selection */}
      <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
        <div className="px-5 py-4 border-b border-gray-100 dark:border-gray-800">
          <h2 className="text-sm font-semibold text-gray-900 dark:text-white flex items-center gap-2">
            <Package className="w-4 h-4 text-primary-600" />
            Select Items to Return
          </h2>
          <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">Check each item you want to include in this return</p>
        </div>

        <div className="divide-y divide-gray-100 dark:divide-gray-800">
          {activeItems.length === 0 && (
            <div className="px-5 py-10 text-center text-sm text-gray-400">No returnable items</div>
          )}
          {activeItems.map((item: LineItem) => {
            const s = getItemState(item.id, item.quantity);
            return (
              <div key={item.id} className={cn(
                'p-5 transition-colors',
                s.selected ? 'bg-orange-50/50 dark:bg-orange-900/10' : 'hover:bg-gray-50 dark:hover:bg-gray-800/30',
              )}>
                <div className="flex items-start gap-4">
                  {/* Checkbox */}
                  <input
                    type="checkbox"
                    checked={s.selected}
                    onChange={(e) => updateItem(item.id, { selected: e.target.checked }, item.quantity)}
                    className="mt-0.5 w-4 h-4 rounded border-gray-300 text-primary-600 focus:ring-primary-500 cursor-pointer"
                  />

                  {/* Item info */}
                  <div className="flex-1 min-w-0">
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <p className="font-medium text-gray-900 dark:text-white">{item.productName}</p>
                        <p className="text-xs text-gray-500 dark:text-gray-400 font-mono">{item.productSku}</p>
                      </div>
                      <div className="text-right flex-shrink-0">
                        <p className="font-semibold text-gray-900 dark:text-white">{fmtMoney(item.lineTotal)}</p>
                        <p className="text-xs text-gray-500 dark:text-gray-400">{item.quantity} × {fmtMoney(item.unitPrice)}</p>
                      </div>
                    </div>

                    {/* Return config — only show when selected */}
                    {s.selected && (
                      <div className="mt-4 grid grid-cols-1 sm:grid-cols-3 gap-4">
                        {/* Qty */}
                        <div>
                          <label className="text-xs font-medium text-gray-600 dark:text-gray-400 mb-1 block">Return Qty</label>
                          <input
                            type="number"
                            min={1}
                            max={item.quantity}
                            value={s.qty}
                            onChange={(e) => updateItem(item.id, { qty: Math.min(Number(e.target.value), item.quantity) }, item.quantity)}
                            className="w-full px-3 py-1.5 text-sm border border-gray-300 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
                          />
                          <p className="text-xs text-gray-400 mt-0.5">Max: {item.quantity}</p>
                        </div>

                        {/* Reason */}
                        <div>
                          <label className="text-xs font-medium text-gray-600 dark:text-gray-400 mb-1 block">Return Reason</label>
                          <select
                            value={s.reason}
                            onChange={(e) => updateItem(item.id, { reason: e.target.value as ReturnReason }, item.quantity)}
                            className="w-full px-3 py-1.5 text-sm border border-gray-300 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
                          >
                            {RETURN_REASONS.map(r => <option key={r.value} value={r.value}>{r.label}</option>)}
                          </select>
                        </div>

                        {/* Disposition */}
                        <div>
                          <label className="text-xs font-medium text-gray-600 dark:text-gray-400 mb-1 block">Stock Disposition</label>
                          <div className="space-y-1.5">
                            {DISPOSITIONS.map(d => (
                              <label key={d.value} className="flex items-start gap-2 cursor-pointer group">
                                <input
                                  type="radio"
                                  name={`disposition-${item.id}`}
                                  value={d.value}
                                  checked={s.disposition === d.value}
                                  onChange={() => updateItem(item.id, { disposition: d.value }, item.quantity)}
                                  className="mt-0.5 w-3.5 h-3.5 text-primary-600 focus:ring-primary-500"
                                />
                                <div>
                                  <p className="text-xs font-medium text-gray-700 dark:text-gray-300 group-hover:text-primary-600">{d.label}</p>
                                  <p className="text-xs text-gray-400">{d.desc}</p>
                                </div>
                              </label>
                            ))}
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Refund config */}
      <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm p-6 space-y-4">
        <h2 className="text-sm font-semibold text-gray-900 dark:text-white">Refund Details</h2>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-1 block">Refund Method *</label>
            <select
              value={refundMethod}
              onChange={(e) => setRefundMethod(e.target.value)}
              className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              {REFUND_METHODS.map(m => <option key={m.value} value={m.value}>{m.label}</option>)}
            </select>
          </div>
          <div>
            <label className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-1 block">Notes</label>
            <input
              type="text"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Optional notes about this return…"
              className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
          </div>
        </div>

        {/* Return summary */}
        {selectedItems.length > 0 && (
          <div className="p-4 bg-orange-50 dark:bg-orange-900/20 border border-orange-200 dark:border-orange-800 rounded-xl">
            <p className="text-sm font-medium text-orange-800 dark:text-orange-300">
              Return summary: <span className="font-bold">{selectedItems.length} item{selectedItems.length === 1 ? '' : 's'}</span>
              {' '}— Refund amount: <span className="font-bold">{fmtMoney(returnTotal)}</span>
            </p>
          </div>
        )}

        {submitError && (
          <div className="flex items-center gap-2 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-xl">
            <AlertCircle className="w-4 h-4 text-red-500 flex-shrink-0" />
            <p className="text-sm text-red-700 dark:text-red-400">{submitError}</p>
          </div>
        )}

        <div className="flex items-center justify-end gap-3 pt-1">
          <Link
            to={`/sales/transactions/${id}`}
            className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 rounded-xl transition"
          >
            Cancel
          </Link>
          <button
            onClick={handleSubmit}
            disabled={isPending || selectedItems.length === 0}
            className="flex items-center gap-2 px-5 py-2 text-sm font-medium text-white bg-orange-600 hover:bg-orange-700 rounded-xl transition disabled:opacity-40"
          >
            {isPending && <Loader2 className="w-4 h-4 animate-spin" />}
            <RotateCcw className="w-4 h-4" />
            Process Return
          </button>
        </div>
      </div>
    </div>
  );
}
