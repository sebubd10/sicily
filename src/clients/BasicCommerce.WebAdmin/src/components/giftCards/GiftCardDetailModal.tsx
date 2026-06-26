import { useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import {
  X, Gift, RefreshCw, XCircle, Loader2, AlertCircle, CheckCircle,
  ArrowUpCircle, ArrowDownCircle, Plus, Clock, Copy, Check,
} from 'lucide-react';
import { cn, extractApiError } from '../../lib/utils';
import { useGiftCardDetail, useReloadGiftCard, useCancelGiftCard } from '../../hooks/useGiftCards';
import type { GiftCardTransactionItem } from '../../types/giftCard';

type Props = {
  giftCardId: string | null;
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
    day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit',
  });
}

const STATUS_STYLES: Record<string, string> = {
  Active:    'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400',
  Depleted:  'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400',
  Expired:   'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400',
  Cancelled: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400',
  Inactive:  'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400',
};

const TX_CONFIG: Record<string, { icon: React.ReactNode; color: string }> = {
  Issue:   { icon: <Plus className="w-3.5 h-3.5" />,            color: 'text-violet-600 dark:text-violet-400' },
  Redeem:  { icon: <ArrowUpCircle className="w-3.5 h-3.5" />,   color: 'text-red-600 dark:text-red-400' },
  Reload:  { icon: <ArrowDownCircle className="w-3.5 h-3.5" />, color: 'text-emerald-600 dark:text-emerald-400' },
  Refund:  { icon: <RefreshCw className="w-3.5 h-3.5" />,       color: 'text-blue-600 dark:text-blue-400' },
  Expire:  { icon: <Clock className="w-3.5 h-3.5" />,           color: 'text-amber-600 dark:text-amber-400' },
  Cancel:  { icon: <XCircle className="w-3.5 h-3.5" />,         color: 'text-red-500 dark:text-red-400' },
};

function BalanceBar({ balance, initial }: { balance: number; initial: number }) {
  const pct = initial > 0 ? Math.min(100, (balance / initial) * 100) : 0;
  const color = pct > 50 ? 'bg-emerald-500' : pct > 20 ? 'bg-amber-500' : 'bg-red-500';
  return (
    <div className="w-full bg-gray-100 dark:bg-gray-700 rounded-full h-2 overflow-hidden">
      <div className={cn('h-full rounded-full transition-all', color)} style={{ width: `${pct}%` }} />
    </div>
  );
}

function TxRow({ tx }: { tx: GiftCardTransactionItem }) {
  const cfg = TX_CONFIG[tx.transactionType] ?? TX_CONFIG.Issue;
  return (
    <tr className="hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors">
      <td className="px-4 py-2.5 text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">
        {fmtDateTime(tx.createdAt)}
      </td>
      <td className="px-4 py-2.5">
        <div className={cn('flex items-center gap-2 font-medium text-sm', cfg.color)}>
          {cfg.icon}
          {tx.transactionType}
        </div>
        {tx.notes && <p className="text-xs text-gray-400 mt-0.5">{tx.notes}</p>}
      </td>
      <td className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums whitespace-nowrap">
        <span className={tx.amount >= 0 ? 'text-emerald-600 dark:text-emerald-400' : 'text-red-600 dark:text-red-400'}>
          {tx.amount >= 0 ? '+' : ''}{fmtMoney(tx.amount)}
        </span>
      </td>
      <td className="px-4 py-2.5 text-right text-sm tabular-nums text-gray-700 dark:text-gray-300 whitespace-nowrap">
        {fmtMoney(tx.balanceAfter)}
      </td>
    </tr>
  );
}

export function GiftCardDetailModal({ giftCardId, open, onClose }: Props) {
  const [tab, setTab] = useState<'overview' | 'history'>('overview');
  const [reloadAmount, setReloadAmount] = useState('');
  const [reloadNotes, setReloadNotes] = useState('');
  const [cancelReason, setCancelReason] = useState('');
  const [showCancelForm, setShowCancelForm] = useState(false);
  const [reloadSuccess, setReloadSuccess] = useState('');
  const [copied, setCopied] = useState(false);

  const { data: card, isLoading, isError, error: fetchError } = useGiftCardDetail(
    open ? giftCardId : null,
  );
  const { mutate: reload, isPending: reloading, error: reloadError } = useReloadGiftCard();
  const { mutate: cancel, isPending: cancelling, error: cancelError } = useCancelGiftCard();

  const reloadErr = reloadError ? extractApiError(reloadError) : null;
  const cancelErr = cancelError ? extractApiError(cancelError) : null;
  const fetchErr = isError ? extractApiError(fetchError) : null;

  function handleClose() {
    setTab('overview');
    setReloadAmount('');
    setReloadNotes('');
    setCancelReason('');
    setShowCancelForm(false);
    setReloadSuccess('');
    setCopied(false);
    onClose();
  }

  function copyCode() {
    if (!card) return;
    navigator.clipboard.writeText(card.code);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }

  function handleReload(e: React.FormEvent) {
    e.preventDefault();
    if (!giftCardId) return;
    const amt = parseFloat(reloadAmount);
    if (isNaN(amt) || amt <= 0) return;
    reload(
      { id: giftCardId, amount: amt, notes: reloadNotes.trim() || null },
      {
        onSuccess: () => {
          setReloadAmount('');
          setReloadNotes('');
          setReloadSuccess(`৳${amt.toLocaleString()} added successfully.`);
          setTimeout(() => setReloadSuccess(''), 4000);
        },
      },
    );
  }

  function handleCancel(e: React.FormEvent) {
    e.preventDefault();
    if (!giftCardId) return;
    cancel(
      { id: giftCardId, reason: cancelReason.trim() || null },
      { onSuccess: handleClose },
    );
  }

  const canReload = card && (card.cardStatus === 'Active' || card.cardStatus === 'Depleted');
  const canCancel = card && card.cardStatus !== 'Cancelled';

  const sortedTx = card ? [...card.transactions].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
  ) : [];

  return (
    <Dialog.Root open={open} onOpenChange={(v) => !v && handleClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 bg-black/50 backdrop-blur-sm z-40 animate-in fade-in-0 duration-200" />
        <Dialog.Content className="fixed left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 z-50 w-full max-w-2xl bg-white dark:bg-gray-900 rounded-2xl shadow-2xl flex flex-col max-h-[92vh]">

          {/* Header */}
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-gray-800">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-xl bg-violet-100 dark:bg-violet-900/30">
                <Gift className="w-5 h-5 text-violet-600 dark:text-violet-400" />
              </div>
              <div>
                <Dialog.Title className="text-base font-semibold text-gray-900 dark:text-gray-100">
                  Gift Card Details
                </Dialog.Title>
                {card && (
                  <div className="flex items-center gap-2 mt-0.5">
                    <button
                      onClick={copyCode}
                      className="flex items-center gap-1 font-mono text-xs text-gray-500 dark:text-gray-400 hover:text-violet-600 dark:hover:text-violet-400 transition-colors"
                    >
                      {card.code}
                      {copied ? <Check className="w-3 h-3 text-emerald-500" /> : <Copy className="w-3 h-3" />}
                    </button>
                    <span className={cn('text-xs px-2 py-0.5 rounded-full font-medium', STATUS_STYLES[card.cardStatus] ?? STATUS_STYLES.Inactive)}>
                      {card.cardStatus}
                    </span>
                  </div>
                )}
              </div>
            </div>
            <Dialog.Close onClick={handleClose}
              className="p-1.5 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          {/* Tabs */}
          <div className="flex gap-0.5 px-6 pt-3 border-b border-gray-100 dark:border-gray-800">
            {(['overview', 'history'] as const).map((t) => (
              <button
                key={t}
                onClick={() => setTab(t)}
                className={cn(
                  'px-4 py-2 text-sm font-medium capitalize rounded-t-lg transition-colors',
                  tab === t
                    ? 'text-violet-700 dark:text-violet-400 border-b-2 border-violet-600 dark:border-violet-400'
                    : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200',
                )}
              >
                {t === 'history' ? `History (${card?.transactions.length ?? 0})` : 'Overview'}
              </button>
            ))}
          </div>

          {/* Body */}
          <div className="flex-1 overflow-y-auto px-6 py-5">
            {isLoading && (
              <div className="flex items-center justify-center py-16">
                <Loader2 className="w-6 h-6 text-violet-500 animate-spin" />
              </div>
            )}
            {fetchErr && (
              <div className="flex items-center gap-2 p-3 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-sm text-red-700 dark:text-red-300">
                <AlertCircle className="w-4 h-4 flex-shrink-0" />{fetchErr}
              </div>
            )}

            {card && tab === 'overview' && (
              <div className="space-y-5">
                {/* Balance card */}
                <div className="bg-gradient-to-br from-violet-50 to-purple-50 dark:from-violet-900/20 dark:to-purple-900/20 rounded-xl border border-violet-100 dark:border-violet-800/50 p-4 space-y-3">
                  <div className="flex items-end justify-between">
                    <div>
                      <p className="text-xs text-violet-600 dark:text-violet-400 font-medium">Current Balance</p>
                      <p className="text-3xl font-bold text-gray-900 dark:text-gray-100 tabular-nums mt-0.5">
                        {fmtMoney(card.balance)}
                      </p>
                    </div>
                    <div className="text-right">
                      <p className="text-xs text-gray-400">of {fmtMoney(card.initialBalance)}</p>
                      <p className="text-xs text-gray-400">
                        {card.initialBalance > 0
                          ? `${((card.balance / card.initialBalance) * 100).toFixed(0)}% remaining`
                          : ''}
                      </p>
                    </div>
                  </div>
                  <BalanceBar balance={card.balance} initial={card.initialBalance} />
                </div>

                {/* Info grid */}
                <div className="grid grid-cols-2 gap-3">
                  {[
                    { label: 'Initial Value', value: fmtMoney(card.initialBalance) },
                    { label: 'Expiry Date', value: fmtDate(card.expiryDate), warn: card.expiryDate ? new Date(card.expiryDate) < new Date(Date.now() + 7 * 86400000) : false },
                    { label: 'Issued', value: fmtDate(card.createdAt) },
                    { label: 'Transactions', value: String(card.transactions.length) },
                  ].map(({ label, value, warn }) => (
                    <div key={label} className="bg-gray-50 dark:bg-gray-800 rounded-xl px-4 py-3">
                      <p className="text-xs text-gray-400">{label}</p>
                      <p className={cn('text-sm font-semibold mt-0.5', warn ? 'text-amber-600 dark:text-amber-400' : 'text-gray-900 dark:text-gray-100')}>
                        {value}
                      </p>
                    </div>
                  ))}
                </div>

                {card.notes && (
                  <div className="text-xs text-gray-500 dark:text-gray-400 bg-gray-50 dark:bg-gray-800 rounded-lg px-4 py-2.5 italic">
                    {card.notes}
                  </div>
                )}

                {/* Reload section */}
                {canReload && (
                  <div className="rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">
                    <div className="px-4 py-2.5 bg-emerald-50 dark:bg-emerald-900/20 border-b border-emerald-100 dark:border-emerald-800">
                      <p className="text-xs font-semibold text-emerald-700 dark:text-emerald-400 uppercase tracking-wider">
                        Top-up Balance
                      </p>
                    </div>
                    <form onSubmit={handleReload} className="px-4 py-4 space-y-3">
                      {reloadErr && (
                        <div className="flex items-center gap-2 text-xs text-red-600 dark:text-red-400">
                          <AlertCircle className="w-3.5 h-3.5 flex-shrink-0" />{reloadErr}
                        </div>
                      )}
                      {reloadSuccess && (
                        <div className="flex items-center gap-2 text-xs text-emerald-600 dark:text-emerald-400">
                          <CheckCircle className="w-3.5 h-3.5 flex-shrink-0" />{reloadSuccess}
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
                              type="number" min="1" step="1" required
                              placeholder="0"
                              value={reloadAmount}
                              onChange={(e) => setReloadAmount(e.target.value)}
                              className="w-full pl-8 pr-3 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
                            />
                          </div>
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">
                            Notes
                          </label>
                          <input
                            placeholder="Optional"
                            value={reloadNotes}
                            onChange={(e) => setReloadNotes(e.target.value)}
                            className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
                          />
                        </div>
                      </div>
                      {reloadAmount && parseFloat(reloadAmount) > 0 && (
                        <p className="text-xs text-gray-400">
                          New balance: {fmtMoney(card.balance + parseFloat(reloadAmount))}
                        </p>
                      )}
                      <button
                        type="submit"
                        disabled={reloading || !reloadAmount}
                        className="flex items-center gap-2 px-4 py-2 rounded-lg bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white text-sm font-medium transition-colors"
                      >
                        {reloading ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <RefreshCw className="w-3.5 h-3.5" />}
                        {reloading ? 'Adding…' : 'Add Funds'}
                      </button>
                    </form>
                  </div>
                )}

                {/* Cancel section */}
                {canCancel && (
                  <div className="rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">
                    <div className="px-4 py-2.5 bg-red-50 dark:bg-red-900/20 border-b border-red-100 dark:border-red-800">
                      <p className="text-xs font-semibold text-red-700 dark:text-red-400 uppercase tracking-wider">
                        Cancel Card
                      </p>
                    </div>
                    <div className="px-4 py-3">
                      {!showCancelForm ? (
                        <div className="flex items-center justify-between">
                          <p className="text-xs text-gray-500 dark:text-gray-400">
                            Permanently disable this gift card. This cannot be undone.
                          </p>
                          <button
                            onClick={() => setShowCancelForm(true)}
                            className="flex items-center gap-1.5 text-xs text-red-600 dark:text-red-400 hover:underline font-medium"
                          >
                            <XCircle className="w-3.5 h-3.5" />Cancel Card
                          </button>
                        </div>
                      ) : (
                        <form onSubmit={handleCancel} className="space-y-3">
                          {cancelErr && (
                            <div className="flex items-center gap-2 text-xs text-red-600 dark:text-red-400">
                              <AlertCircle className="w-3.5 h-3.5 flex-shrink-0" />{cancelErr}
                            </div>
                          )}
                          <textarea
                            rows={2}
                            placeholder="Cancellation reason (optional)"
                            value={cancelReason}
                            onChange={(e) => setCancelReason(e.target.value)}
                            className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-red-500 resize-none"
                          />
                          <div className="flex gap-2">
                            <button type="button" onClick={() => setShowCancelForm(false)}
                              className="px-3 py-1.5 text-sm text-gray-600 dark:text-gray-400 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors">
                              Keep Card
                            </button>
                            <button type="submit" disabled={cancelling}
                              className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-red-600 hover:bg-red-700 disabled:opacity-50 text-white text-sm font-medium transition-colors">
                              {cancelling ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <XCircle className="w-3.5 h-3.5" />}
                              {cancelling ? 'Cancelling…' : 'Confirm Cancel'}
                            </button>
                          </div>
                        </form>
                      )}
                    </div>
                  </div>
                )}
              </div>
            )}

            {/* History tab */}
            {card && tab === 'history' && (
              <div>
                {sortedTx.length === 0 ? (
                  <div className="text-center py-10 text-gray-400 text-sm">No transactions yet</div>
                ) : (
                  <div className="rounded-xl border border-gray-100 dark:border-gray-800 overflow-hidden">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="bg-gray-50 dark:bg-gray-800/50 border-b border-gray-100 dark:border-gray-800">
                          {['Date', 'Type', 'Amount', 'Balance After'].map((h) => (
                            <th key={h} className={cn(
                              'px-4 py-2.5 text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider',
                              h === 'Amount' || h === 'Balance After' ? 'text-right' : 'text-left',
                            )}>{h}</th>
                          ))}
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-50 dark:divide-gray-800">
                        {sortedTx.map((tx) => <TxRow key={tx.id} tx={tx} />)}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="px-6 py-3 border-t border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50 flex justify-end">
            <Dialog.Close onClick={handleClose}
              className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100 font-medium transition-colors">
              Close
            </Dialog.Close>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
