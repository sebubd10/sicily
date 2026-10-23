import { useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import {
  ArrowLeft, ShoppingCart, CheckCircle2, Ban, Clock, RotateCcw,
  User, Store, Terminal, CreditCard, AlertCircle, Loader2,
  Receipt, Package, Copy, Check, XCircle, RefreshCw,
  ChevronRight, AlertTriangle, Star, Pencil, FilePenLine, Repeat, History,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { Transaction, TransactionStatus, TransactionType, LineItem, Payment } from '../types/transaction';
import { useTransactionDetail, useVoidTransaction, useTransactionHistory, useTransactionMutation } from '../hooks/useTransactions';
import * as transactionsApi from '../api/transactionsApi';
import type { TransactionAuditAction } from '../types/transaction';
import { PaymentMethodIcon } from '../components/transactions/PaymentMethodIcon';
import { useStoreList } from '../hooks/useStoreAdmin';
import { useTerminals } from '../hooks/useTerminals';

/* ── helpers ──────────────────────────────────────────────────────────────── */

function fmtMoney(n: number) {
  return `৳${n.toLocaleString('en-BD', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

function fmtDate(d: string | null | undefined) {
  if (!d) return '—';
  return new Date(d).toLocaleString('en-GB', {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
}

const STATUS_CONFIG: Record<TransactionStatus, { label: string; cls: string; dot: string; icon: React.ReactNode }> = {
  Open:      { label: 'Open',      dot: 'bg-sky-500',     cls: 'bg-sky-50 text-sky-700 dark:bg-sky-900/30 dark:text-sky-400 border border-sky-200 dark:border-sky-800',     icon: <Clock className="w-4 h-4" /> },
  Completed: { label: 'Completed', dot: 'bg-emerald-500', cls: 'bg-emerald-50 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400 border border-emerald-200 dark:border-emerald-800', icon: <CheckCircle2 className="w-4 h-4" /> },
  Voided:    { label: 'Voided',    dot: 'bg-red-500',     cls: 'bg-red-50 text-red-700 dark:bg-red-900/30 dark:text-red-400 border border-red-200 dark:border-red-800',      icon: <Ban className="w-4 h-4" /> },
  Suspended: { label: 'Suspended', dot: 'bg-amber-500',   cls: 'bg-amber-50 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400 border border-amber-200 dark:border-amber-800', icon: <Clock className="w-4 h-4" /> },
  Refunded:  { label: 'Refunded',  dot: 'bg-violet-500',  cls: 'bg-violet-50 text-violet-700 dark:bg-violet-900/30 dark:text-violet-400 border border-violet-200 dark:border-violet-800', icon: <RotateCcw className="w-4 h-4" /> },
};

const TYPE_LABELS: Record<TransactionType, string> = {
  Sale: 'Sale', Return: 'Return', Exchange: 'Exchange', CreditSale: 'Credit Sale',
};

const PAYMENT_METHOD_LABELS: Record<string, string> = {
  Cash: 'Cash', Card: 'Card', BKash: 'bKash', Nagad: 'Nagad',
  Rocket: 'Rocket', Ebt: 'EBT', Credit: 'Credit Account',
  GiftCard: 'Gift Card', SslCommerz: 'SSLCommerz',
  AamarPay: 'AamarPay', RewardPoints: 'Reward Points', ExchangeCredit: 'Exchange Credit',
};

const PAYMENT_STATUS_STYLES: Record<string, string> = {
  Approved:    'text-emerald-600 dark:text-emerald-400',
  Pending:     'text-amber-600 dark:text-amber-400',
  Declined:    'text-red-600 dark:text-red-400',
  Refunded:    'text-violet-600 dark:text-violet-400',
  PendingSync: 'text-sky-600 dark:text-sky-400',
};

/* ── Void Modal ───────────────────────────────────────────────────────────── */

function VoidModal({
  txn, mode, onClose,
}: { txn: Transaction; mode: 'void' | 'rering'; onClose: () => void }) {
  const [reason, setReason] = useState('');
  const [error, setError]   = useState('');
  const voidTxn = useVoidTransaction();
  const reRing  = useTransactionMutation((r: string) => transactionsApi.voidAndReRing(txn.id, r));
  const navigate = useNavigate();
  const isPending = voidTxn.isPending || reRing.isPending;
  const completed = txn.status === 'Completed';

  function submit() {
    if (!reason.trim()) { setError('Please provide a reason.'); return; }
    setError('');
    if (mode === 'rering') {
      reRing.mutate(reason.trim(), {
        onSuccess: (draft) => { onClose(); navigate(`/sales/transactions/${draft.id}/edit`); },
        onError: (e) => setError(extractApiError(e)),
      });
    } else {
      voidTxn.mutate({ id: txn.id, reason: reason.trim() }, {
        onSuccess: () => onClose(),
        onError: (e) => setError(extractApiError(e)),
      });
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="bg-white dark:bg-gray-900 rounded-2xl shadow-xl w-full max-w-md p-6 space-y-4">
        <div className="flex items-center gap-3">
          <div className="p-2 bg-red-50 dark:bg-red-900/20 rounded-xl">
            <AlertTriangle className="w-5 h-5 text-red-600 dark:text-red-400" />
          </div>
          <div>
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
              {mode === 'rering' ? 'Void & re-ring' : 'Void transaction'}
            </h2>
            <p className="text-sm text-gray-500 dark:text-gray-400">{txn.transactionNumber}</p>
          </div>
        </div>

        <ul className="text-sm text-gray-600 dark:text-gray-400 space-y-1 list-disc pl-5">
          <li>This sale is permanently voided — it can't be undone.</li>
          <li>Gift card balance, credit charges and redeemed points are given back.</li>
          {completed && <li>Stock is put back and points earned on it are taken back.</li>}
          {completed && <li>Only allowed while the till session that took it is still open.</li>}
          {mode === 'rering' && (
            <li className="text-gray-900 dark:text-white font-medium">
              A new draft opens with the same customer and items (at today's prices) for you to fix and take payment.
            </li>
          )}
        </ul>

        <div className="space-y-1">
          <label className="text-sm font-medium text-gray-700 dark:text-gray-300">Reason *</label>
          <textarea
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            rows={3}
            placeholder={mode === 'rering' ? 'e.g. Wrong quantity rung up' : 'Explain why this transaction is being voided…'}
            className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-red-500 resize-none"
          />
          {error && <p className="text-xs text-red-500">{error}</p>}
        </div>

        <div className="flex gap-2 justify-end pt-1">
          <button onClick={onClose} disabled={isPending}
            className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 rounded-lg transition disabled:opacity-40">
            Cancel
          </button>
          <button onClick={submit} disabled={isPending || !reason.trim()}
            className="px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-lg transition disabled:opacity-40 flex items-center gap-2">
            {isPending && <Loader2 className="w-3.5 h-3.5 animate-spin" />}
            {mode === 'rering' ? 'Void & re-ring' : 'Void transaction'}
          </button>
        </div>
      </div>
    </div>
  );
}

/* ── TxnNumberCell with copy ──────────────────────────────────────────────── */

function TxnNumber({ num }: { num: string }) {
  const [copied, setCopied] = useState(false);
  function copy() {
    navigator.clipboard.writeText(num);
    setCopied(true);
    setTimeout(() => setCopied(false), 1800);
  }
  return (
    <div className="flex items-center gap-2">
      <span className="font-mono text-xl font-bold text-gray-900 dark:text-white">{num}</span>
      <button onClick={copy} className="text-gray-400 hover:text-primary-600 transition-colors p-0.5" title="Copy">
        {copied ? <Check className="w-4 h-4 text-emerald-500" /> : <Copy className="w-4 h-4" />}
      </button>
    </div>
  );
}

/* ── Main Page ────────────────────────────────────────────────────────────── */

type TabKey = 'items' | 'payments' | 'history';

export default function TransactionDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [tab, setTab]           = useState<TabKey>('items');
  const [voidMode, setVoidMode] = useState<'void' | 'rering' | null>(null);

  const { data: txn, isLoading, isError, refetch, isFetching } = useTransactionDetail(id ?? null);
  const { data: stores = [] }    = useStoreList();
  const { data: terminals = [] } = useTerminals();
  const { data: history = [] }   = useTransactionHistory(id ?? null);

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

  const statusCfg = STATUS_CONFIG[txn.status] ?? STATUS_CONFIG.Open;
  const store     = stores.find((s) => s.id === txn.storeId);
  const terminal  = terminals.find((t) => t.id === txn.terminalId);
  const activeItems = txn.lineItems.filter(l => !l.isVoided);
  const voidedItems = txn.lineItems.filter(l => l.isVoided);

  const btn = 'flex items-center gap-2 px-3.5 py-2 text-sm font-medium border rounded-xl transition';
  const isDraft     = txn.status === 'Open' || txn.status === 'Suspended';
  const isCompleted = txn.status === 'Completed';
  const canVoid     = isDraft || isCompleted;
  const canCorrect  = isCompleted || txn.status === 'Refunded';

  return (
    <div className="space-y-5 max-w-5xl mx-auto">
      {/* Breadcrumb */}
      <nav className="flex items-center gap-2 text-sm text-gray-500 dark:text-gray-400">
        <Link to="/sales/transactions" className="hover:text-primary-600 transition-colors flex items-center gap-1">
          <ShoppingCart className="w-3.5 h-3.5" />
          Transactions
        </Link>
        <ChevronRight className="w-3.5 h-3.5" />
        <span className="font-mono text-gray-700 dark:text-gray-300">{txn.transactionNumber}</span>
      </nav>

      {/* Header card */}
      <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm p-6">
        <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4">
          <div className="space-y-2">
            <TxnNumber num={txn.transactionNumber} />
            <div className="flex items-center gap-2 flex-wrap">
              {/* Status */}
              <span className={cn('inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-sm font-medium', statusCfg.cls)}>
                {statusCfg.icon}
                {statusCfg.label}
              </span>
              {/* Type */}
              <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300">
                {TYPE_LABELS[txn.type] ?? txn.type}
              </span>
              {/* Original TXN link */}
              {txn.originalTransactionId && (
                <Link
                  to={`/sales/transactions/${txn.originalTransactionId}`}
                  className="inline-flex items-center gap-1 px-3 py-1 rounded-full text-sm font-medium text-orange-700 dark:text-orange-400 bg-orange-50 dark:bg-orange-900/20 border border-orange-200 dark:border-orange-800 hover:underline"
                >
                  <RotateCcw className="w-3.5 h-3.5" />
                  Original TXN
                </Link>
              )}
            </div>
            <p className="text-xs text-gray-500 dark:text-gray-400">
              Created {fmtDate(txn.createdAt)}
              {txn.completedAt && ` · Completed ${fmtDate(txn.completedAt)}`}
              {txn.voidedAt && ` · Voided ${fmtDate(txn.voidedAt)}`}
            </p>
          </div>

          {/* Action buttons */}
          <div className="flex flex-wrap items-center justify-end gap-2">
            <button
              onClick={() => refetch()} disabled={isFetching}
              className="p-2 rounded-lg text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 transition disabled:opacity-40"
              title="Refresh"
            >
              <RefreshCw className={cn('w-4 h-4', isFetching && 'animate-spin')} />
            </button>
            {isDraft && (
              <button
                onClick={() => navigate(`/sales/transactions/${txn.id}/edit`)}
                className={cn(btn, 'text-white bg-primary-700 hover:bg-primary-800 border-primary-700')}
              >
                <Pencil className="w-4 h-4" /> Resume sale
              </button>
            )}
            {canCorrect && (
              <button onClick={() => navigate(`/sales/transactions/${txn.id}/correct`)}
                className={cn(btn, 'text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-900 border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800')}>
                <FilePenLine className="w-4 h-4" /> Correct
              </button>
            )}
            {isCompleted && (
              <>
                <button onClick={() => navigate(`/sales/transactions/${txn.id}/exchange`)}
                  className={cn(btn, 'text-sky-700 dark:text-sky-400 bg-sky-50 dark:bg-sky-900/20 border-sky-200 dark:border-sky-800 hover:bg-sky-100')}>
                  <Repeat className="w-4 h-4" /> Exchange
                </button>
                <button onClick={() => navigate(`/sales/transactions/${txn.id}/return`)}
                  className={cn(btn, 'text-orange-700 dark:text-orange-400 bg-orange-50 dark:bg-orange-900/20 border-orange-200 dark:border-orange-800 hover:bg-orange-100')}>
                  <RotateCcw className="w-4 h-4" /> Return
                </button>
                <button onClick={() => setVoidMode('rering')}
                  className={cn(btn, 'text-red-700 dark:text-red-400 bg-white dark:bg-gray-900 border-red-200 dark:border-red-800 hover:bg-red-50')}>
                  <RefreshCw className="w-4 h-4" /> Void &amp; re-ring
                </button>
              </>
            )}
            {canVoid && (
              <button onClick={() => setVoidMode('void')}
                className={cn(btn, 'text-red-700 dark:text-red-400 bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800 hover:bg-red-100')}>
                <XCircle className="w-4 h-4" /> Void
              </button>
            )}
          </div>
        </div>

        {/* Void reason banner */}
        {txn.voidReason && (
          <div className="mt-4 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-xl">
            <p className="text-sm text-red-700 dark:text-red-400">
              <span className="font-semibold">Void reason:</span> {txn.voidReason}
            </p>
          </div>
        )}
      </div>

      {/* Info grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {/* Customer */}
        <InfoCard icon={<User className="w-4 h-4 text-blue-600" />} label="Customer">
          {txn.customerName
            ? <p className="font-medium text-gray-900 dark:text-white">{txn.customerName}</p>
            : <p className="text-gray-400 italic text-sm">Walk-in customer</p>
          }
          {txn.customerId && (txn.pointsEarned > 0 || txn.pointsRedeemed > 0) && (
            <div className="mt-2 flex flex-wrap gap-1.5">
              {txn.pointsEarned > 0 && (
                <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400">
                  <Star className="w-3 h-3" /> +{txn.pointsEarned.toLocaleString()} pts earned
                </span>
              )}
              {txn.pointsRedeemed > 0 && (
                <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-semibold bg-amber-50 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400">
                  <Star className="w-3 h-3" /> {txn.pointsRedeemed.toLocaleString()} pts redeemed
                </span>
              )}
            </div>
          )}
        </InfoCard>

        {/* Store */}
        <InfoCard icon={<Store className="w-4 h-4 text-emerald-600" />} label="Store">
          <NamedRef name={store?.name} code={store?.code} id={txn.storeId} />
        </InfoCard>

        {/* Terminal */}
        <InfoCard icon={<Terminal className="w-4 h-4 text-violet-600" />} label="Terminal">
          <NamedRef name={terminal?.name} code={terminal?.code} id={txn.terminalId} />
        </InfoCard>
      </div>

      {/* Financial Summary */}
      <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-100 dark:border-gray-800">
          <h3 className="text-sm font-semibold text-gray-900 dark:text-white flex items-center gap-2">
            <CreditCard className="w-4 h-4 text-primary-600" />
            Financial Summary
          </h3>
        </div>
        <div className="p-6 grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
          <FinLine label="Subtotal"  value={fmtMoney(txn.subTotal)} />
          <FinLine label="Tax"       value={fmtMoney(txn.taxTotal)} />
          <FinLine label="Discounts" value={`-${fmtMoney(txn.discountTotal)}`} highlight="text-orange-600 dark:text-orange-400" />
          <FinLine label="Total"     value={fmtMoney(txn.total)} highlight="text-gray-900 dark:text-white font-bold text-base" />
          <FinLine label="Paid"      value={fmtMoney(txn.amountPaid)} highlight="text-emerald-600 dark:text-emerald-400" />
          <FinLine label="Change"    value={fmtMoney(txn.changeDue)} />
        </div>
      </div>

      {/* Tabs */}
      <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
        <div className="flex border-b border-gray-200 dark:border-gray-700">
          {([
            { key: 'items',    label: `Items (${txn.lineItems.length})`,    icon: <Package className="w-4 h-4" /> },
            { key: 'payments', label: `Payments (${txn.payments.length})`,  icon: <CreditCard className="w-4 h-4" /> },
            { key: 'history',  label: 'History',                             icon: <History className="w-4 h-4" /> },
          ] as { key: TabKey; label: string; icon: React.ReactNode }[]).map((t) => (
            <button
              key={t.key}
              onClick={() => setTab(t.key)}
              className={cn(
                'flex items-center gap-1.5 px-5 py-3.5 text-sm font-medium transition-colors border-b-2 -mb-px',
                tab === t.key
                  ? 'text-primary-700 dark:text-primary-400 border-primary-600'
                  : 'text-gray-500 dark:text-gray-400 border-transparent hover:text-gray-700 dark:hover:text-gray-300',
              )}
            >
              {t.icon}
              {t.label}
            </button>
          ))}
        </div>

        {/* Items tab */}
        {tab === 'items' && (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                  {['Product', 'SKU', 'Qty', 'Unit Price', 'Tax', 'Discount', 'Total', 'Promo'].map(h => (
                    <th key={h} className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider whitespace-nowrap">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                {activeItems.map((item: LineItem) => (
                  <tr key={item.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
                    <td className="px-4 py-3 font-medium text-gray-900 dark:text-white">
                      {item.productName}
                      {item.isPriceOverridden && (
                        <span className="ml-1.5 text-xs text-amber-600 dark:text-amber-400 font-normal">(price override)</span>
                      )}
                      {item.returnReason && (
                        <div className="text-xs text-orange-600 dark:text-orange-400 mt-0.5">Return: {item.returnReason}</div>
                      )}
                    </td>
                    <td className="px-4 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">{item.productSku}</td>
                    <td className="px-4 py-3 tabular-nums text-gray-700 dark:text-gray-300">{item.quantity}</td>
                    <td className="px-4 py-3 tabular-nums text-gray-700 dark:text-gray-300">{fmtMoney(item.unitPrice)}</td>
                    <td className="px-4 py-3 tabular-nums text-gray-500 dark:text-gray-400">{fmtMoney(item.taxAmount)}</td>
                    <td className="px-4 py-3 tabular-nums text-orange-600 dark:text-orange-400">{item.discountAmount > 0 ? `-${fmtMoney(item.discountAmount)}` : '—'}</td>
                    <td className="px-4 py-3 tabular-nums font-semibold text-gray-900 dark:text-white">{fmtMoney(item.lineTotal)}</td>
                    <td className="px-4 py-3 text-xs text-violet-600 dark:text-violet-400">{item.appliedPromotionName ?? '—'}</td>
                  </tr>
                ))}
                {voidedItems.length > 0 && (
                  <>
                    <tr>
                      <td colSpan={8} className="px-4 py-2 bg-gray-50 dark:bg-gray-800 text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Voided Items
                      </td>
                    </tr>
                    {voidedItems.map((item: LineItem) => (
                      <tr key={item.id} className="opacity-50">
                        <td className="px-4 py-3 text-gray-500 line-through">{item.productName}</td>
                        <td className="px-4 py-3 font-mono text-xs text-gray-400">{item.productSku}</td>
                        <td className="px-4 py-3 tabular-nums text-gray-400">{item.quantity}</td>
                        <td className="px-4 py-3 tabular-nums text-gray-400">{fmtMoney(item.unitPrice)}</td>
                        <td colSpan={4} className="px-4 py-3 text-xs text-red-500">Voided</td>
                      </tr>
                    ))}
                  </>
                )}
                {txn.lineItems.length === 0 && (
                  <tr>
                    <td colSpan={8} className="px-4 py-10 text-center text-sm text-gray-400">No items</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}

        {/* Payments tab */}
        {tab === 'payments' && (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                  {['Method', 'Amount', 'Status', 'Reference'].map(h => (
                    <th key={h} className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                {txn.payments.map((p: Payment) => (
                  <tr key={p.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
                    <td className="px-4 py-3 font-medium text-gray-900 dark:text-white flex items-center gap-2">
                      <PaymentMethodIcon method={p.method} size={16} />
                      {PAYMENT_METHOD_LABELS[p.method] ?? p.method}
                    </td>
                    <td className="px-4 py-3 tabular-nums font-semibold text-gray-900 dark:text-white">{fmtMoney(p.amount)}</td>
                    <td className="px-4 py-3">
                      <span className={cn('text-xs font-medium', PAYMENT_STATUS_STYLES[p.status] ?? 'text-gray-500')}>
                        {p.status}
                      </span>
                    </td>
                    <td className="px-4 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">
                      {p.reference ?? '—'}
                    </td>
                  </tr>
                ))}
                {txn.payments.length === 0 && (
                  <tr>
                    <td colSpan={4} className="px-4 py-10 text-center text-sm text-gray-400">No payments recorded</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}

        {/* History tab */}
        {tab === 'history' && (history.length > 0 ? <HistoryList entries={history} /> : (
          <div className="p-6">
            <div className="relative pl-6 space-y-6">
              <div className="absolute left-2.5 top-0 bottom-0 w-px bg-gray-200 dark:bg-gray-700" />

              <TimelineEvent
                icon={<Receipt className="w-3.5 h-3.5 text-primary-600" />}
                dot="bg-primary-600"
                title="Transaction created"
                time={fmtDate(txn.createdAt)}
              />

              {txn.completedAt && (
                <TimelineEvent
                  icon={<CheckCircle2 className="w-3.5 h-3.5 text-emerald-600" />}
                  dot="bg-emerald-500"
                  title="Completed"
                  time={fmtDate(txn.completedAt)}
                  sub={`Total: ${fmtMoney(txn.total)}`}
                />
              )}

              {txn.voidedAt && (
                <TimelineEvent
                  icon={<XCircle className="w-3.5 h-3.5 text-red-600" />}
                  dot="bg-red-500"
                  title="Voided"
                  time={fmtDate(txn.voidedAt)}
                  sub={txn.voidReason ?? undefined}
                />
              )}

              {txn.status === 'Refunded' && (
                <TimelineEvent
                  icon={<RotateCcw className="w-3.5 h-3.5 text-violet-600" />}
                  dot="bg-violet-500"
                  title="Return processed"
                  time="—"
                />
              )}
            </div>
          </div>
        ))}
      </div>

      {/* Notes */}
      {txn.notes && (
        <div className="bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800 rounded-xl p-4">
          <p className="text-sm text-amber-800 dark:text-amber-300">
            <span className="font-semibold">Notes: </span>{txn.notes}
          </p>
        </div>
      )}

      {/* Void Modal */}
      {voidMode && <VoidModal txn={txn} mode={voidMode} onClose={() => setVoidMode(null)} />}
    </div>
  );
}

/* ── Sub-components ───────────────────────────────────────────────────────── */

function InfoCard({ icon, label, children }: {
  icon: React.ReactNode; label: string; children: React.ReactNode;
}) {
  return (
    <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm p-4">
      <div className="flex items-center gap-2 mb-2">
        {icon}
        <span className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">{label}</span>
      </div>
      {children}
    </div>
  );
}

const AUDIT_STYLE: Partial<Record<TransactionAuditAction, string>> = {
  Created: 'bg-primary-500', Completed: 'bg-emerald-500', Voided: 'bg-red-500', ReRung: 'bg-red-400',
  Returned: 'bg-orange-500', Exchanged: 'bg-sky-500', PaymentAdded: 'bg-emerald-400',
  PaymentRemoved: 'bg-amber-500', PaymentMethodCorrected: 'bg-amber-500', NotesChanged: 'bg-gray-400',
  CustomerChanged: 'bg-blue-500', Suspended: 'bg-amber-400', Resumed: 'bg-sky-400',
};

function HistoryList({ entries }: {
  entries: { id: string; action: TransactionAuditAction; summary: string; reason: string | null; userName: string | null; createdAt: string }[];
}) {
  return (
    <ol className="p-6 relative space-y-5">
      <div className="absolute left-[29px] top-8 bottom-8 w-px bg-gray-200 dark:bg-gray-700" />
      {entries.map((e) => (
        <li key={e.id} className="relative flex gap-4">
          <span className={cn('relative z-10 mt-1.5 w-2.5 h-2.5 rounded-full ring-4 ring-white dark:ring-gray-900 flex-shrink-0',
            AUDIT_STYLE[e.action] ?? 'bg-gray-400')} />
          <div className="min-w-0">
            <p className="text-sm text-gray-900 dark:text-white">{e.summary}</p>
            {e.reason && <p className="text-xs text-gray-600 dark:text-gray-300 mt-0.5">Reason: {e.reason}</p>}
            <p className="text-xs text-gray-400 mt-0.5">
              {fmtDate(e.createdAt)}{e.userName && ` · ${e.userName}`}
            </p>
          </div>
        </li>
      ))}
    </ol>
  );
}

function NamedRef({ name, code, id }: { name?: string; code?: string; id: string }) {
  if (!name) {
    return <p className="font-mono text-xs text-gray-400" title={id}>{id.slice(0, 8)}…</p>;
  }
  return (
    <div>
      <p className="font-medium text-gray-900 dark:text-white">{name}</p>
      {code && <p className="text-xs text-gray-500 dark:text-gray-400 font-mono">{code}</p>}
    </div>
  );
}

function FinLine({ label, value, highlight }: { label: string; value: string; highlight?: string }) {
  return (
    <div className="text-center">
      <p className="text-xs text-gray-500 dark:text-gray-400 mb-1">{label}</p>
      <p className={cn('tabular-nums font-semibold text-sm text-gray-700 dark:text-gray-300', highlight)}>{value}</p>
    </div>
  );
}

function TimelineEvent({ icon, dot, title, time, sub }: {
  icon: React.ReactNode; dot: string; title: string; time: string; sub?: string;
}) {
  return (
    <div className="relative flex gap-4">
      <div className={cn('absolute -left-4 top-1 w-3.5 h-3.5 rounded-full border-2 border-white dark:border-gray-900 flex-shrink-0', dot)} />
      <div className="flex items-center gap-2">
        {icon}
        <div>
          <p className="text-sm font-medium text-gray-900 dark:text-white">{title}</p>
          <p className="text-xs text-gray-500 dark:text-gray-400">{time}</p>
          {sub && <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">{sub}</p>}
        </div>
      </div>
    </div>
  );
}
