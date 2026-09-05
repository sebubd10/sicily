import { useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import {
  ArrowLeft, ShoppingCart, CheckCircle2, Ban, Clock, RotateCcw,
  User, Store, Terminal, CreditCard, AlertCircle, Loader2,
  Receipt, Package, Copy, Check, XCircle, RefreshCw,
  ChevronRight, AlertTriangle,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { Transaction, TransactionStatus, TransactionType, LineItem, Payment } from '../types/transaction';
import { useTransactionDetail, useVoidTransaction } from '../hooks/useTransactions';
import { PaymentMethodIcon } from '../components/transactions/PaymentMethodIcon';

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
  AamarPay: 'AamarPay', RewardPoints: 'Reward Points',
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
  txn, onClose,
}: { txn: Transaction; onClose: () => void }) {
  const [reason, setReason] = useState('');
  const [error, setError]   = useState('');
  const { mutate, isPending } = useVoidTransaction();
  const navigate = useNavigate();

  function submit() {
    if (!reason.trim()) { setError('Please provide a reason.'); return; }
    setError('');
    mutate({ id: txn.id, reason }, {
      onSuccess: () => { onClose(); navigate('/sales/transactions'); },
      onError: (e) => setError(extractApiError(e)),
    });
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="bg-white dark:bg-gray-900 rounded-2xl shadow-xl w-full max-w-md p-6 space-y-4">
        <div className="flex items-center gap-3">
          <div className="p-2 bg-red-50 dark:bg-red-900/20 rounded-xl">
            <AlertTriangle className="w-5 h-5 text-red-600 dark:text-red-400" />
          </div>
          <div>
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Void Transaction</h2>
            <p className="text-sm text-gray-500 dark:text-gray-400">{txn.transactionNumber}</p>
          </div>
        </div>

        <p className="text-sm text-gray-600 dark:text-gray-400">
          This action cannot be undone. The transaction will be permanently voided.
        </p>

        <div className="space-y-1">
          <label className="text-sm font-medium text-gray-700 dark:text-gray-300">Reason *</label>
          <textarea
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            rows={3}
            placeholder="Explain why this transaction is being voided…"
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
            Void Transaction
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

type TabKey = 'items' | 'payments' | 'timeline';

export default function TransactionDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [tab, setTab]           = useState<TabKey>('items');
  const [showVoid, setShowVoid] = useState(false);

  const { data: txn, isLoading, isError, refetch, isFetching } = useTransactionDetail(id ?? null);

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
  const activeItems = txn.lineItems.filter(l => !l.isVoided);
  const voidedItems = txn.lineItems.filter(l => l.isVoided);

  const canVoid   = txn.status === 'Open' || txn.status === 'Suspended';
  const canReturn = txn.status === 'Completed';

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
          <div className="flex items-center gap-2">
            <button
              onClick={() => refetch()} disabled={isFetching}
              className="p-2 rounded-lg text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 transition disabled:opacity-40"
              title="Refresh"
            >
              <RefreshCw className={cn('w-4 h-4', isFetching && 'animate-spin')} />
            </button>
            {canReturn && (
              <button
                onClick={() => navigate(`/sales/transactions/${txn.id}/return`)}
                className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-orange-700 dark:text-orange-400 bg-orange-50 dark:bg-orange-900/20 border border-orange-200 dark:border-orange-800 hover:bg-orange-100 dark:hover:bg-orange-900/40 rounded-xl transition"
              >
                <RotateCcw className="w-4 h-4" />
                Process Return
              </button>
            )}
            {canVoid && (
              <button
                onClick={() => setShowVoid(true)}
                className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-red-700 dark:text-red-400 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 hover:bg-red-100 dark:hover:bg-red-900/40 rounded-xl transition"
              >
                <XCircle className="w-4 h-4" />
                Void
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
        </InfoCard>

        {/* Store */}
        <InfoCard icon={<Store className="w-4 h-4 text-emerald-600" />} label="Store">
          <p className="font-mono text-xs text-gray-600 dark:text-gray-400">{txn.storeId.slice(0, 8)}…</p>
        </InfoCard>

        {/* Terminal */}
        <InfoCard icon={<Terminal className="w-4 h-4 text-violet-600" />} label="Terminal">
          <p className="font-mono text-xs text-gray-600 dark:text-gray-400">{txn.terminalId.slice(0, 8)}…</p>
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
            { key: 'timeline', label: 'Timeline',                            icon: <Clock className="w-4 h-4" /> },
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

        {/* Timeline tab */}
        {tab === 'timeline' && (
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
        )}
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
      {showVoid && <VoidModal txn={txn} onClose={() => setShowVoid(false)} />}
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
