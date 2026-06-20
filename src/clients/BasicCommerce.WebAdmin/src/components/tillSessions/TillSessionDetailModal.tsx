import * as Dialog from '@radix-ui/react-dialog';
import {
  X, Monitor, Store as StoreIcon, User as UserIcon,
  ArrowDownCircle, ArrowUpCircle, BarChart2, Loader2,
  TrendingDown, AlertCircle,
} from 'lucide-react';
import { cn } from '../../lib/utils';
import type { TillReport } from '../../types/tillSession';
import { useTillSession, useXReport } from '../../hooks/useTillSessions';

type Props = {
  sessionId: string | null;
  open: boolean;
  onClose: () => void;
};

function fmt(n: number | null | undefined): string {
  if (n == null) return '—';
  return n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function fmtDateTime(d: string | null | undefined): string {
  if (!d) return '—';
  return new Date(d).toLocaleString('en-GB', {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
}

function sessionDuration(start: string, end: string | null): string {
  const ms = (end ? new Date(end).getTime() : Date.now()) - new Date(start).getTime();
  const h = Math.floor(ms / 3_600_000);
  const m = Math.floor((ms % 3_600_000) / 60_000);
  return `${h}h ${m}m`;
}

function ReportSection({ report }: { report: TillReport }) {
  const totalSales = report.cashSalesTotal + report.cardSalesTotal + report.giftCardSalesTotal;

  const rows: { label: string; value: string; highlight?: 'green' | 'red'; muted?: boolean }[] = [
    { label: 'Opening Float',   value: fmt(report.openingFloat) },
    { label: 'Cash Sales',      value: fmt(report.cashSalesTotal) },
    { label: 'Card Sales',      value: fmt(report.cardSalesTotal) },
    { label: 'Gift Card Sales', value: fmt(report.giftCardSalesTotal) },
    { label: 'Total Sales',     value: fmt(totalSales), highlight: 'green' },
    {
      label: 'Total Refunds',
      value: fmt(report.totalRefunds),
      highlight: report.totalRefunds > 0 ? 'red' : undefined,
      muted: report.totalRefunds === 0,
    },
    { label: 'Petty Cash In',   value: fmt(report.pettyCashIn),  muted: report.pettyCashIn === 0 },
    { label: 'Petty Cash Out',  value: fmt(report.pettyCashOut), muted: report.pettyCashOut === 0 },
    { label: 'Expected Cash',   value: fmt(report.expectedCash) },
  ];

  if (report.isFinal) {
    rows.push({ label: 'Actual Cash', value: fmt(report.actualCash) });
    const v = report.variance ?? 0;
    rows.push({
      label: 'Variance',
      value: report.variance != null ? (v > 0 ? '+' : '') + fmt(report.variance) : '—',
      highlight: v > 0 ? 'green' : v < 0 ? 'red' : undefined,
    });
  }

  return (
    <div className="rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">
      <div className="bg-gray-50 dark:bg-gray-800 px-4 py-2.5 border-b border-gray-200 dark:border-gray-700">
        <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
          {report.isFinal ? 'Z-Report (Final)' : 'X-Report (Mid-Shift)'}
        </p>
      </div>
      <div className="divide-y divide-gray-100 dark:divide-gray-800">
        {rows.map(({ label, value, highlight, muted }) => (
          <div key={label} className="flex items-center justify-between px-4 py-2.5">
            <span className={cn(
              'text-sm',
              muted ? 'text-gray-400 dark:text-gray-600' : 'text-gray-600 dark:text-gray-300',
            )}>
              {label}
            </span>
            <span className={cn(
              'text-sm font-semibold tabular-nums',
              highlight === 'green' && 'text-emerald-600 dark:text-emerald-400',
              highlight === 'red'   && 'text-red-600 dark:text-red-400',
              !highlight && (muted ? 'text-gray-400 dark:text-gray-600' : 'text-gray-900 dark:text-white'),
            )}>
              {value}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}

export function TillSessionDetailModal({ sessionId, open, onClose }: Props) {
  const { data: session, isLoading, isError } = useTillSession(open ? sessionId : null);
  const isOpen = session?.sessionStatus === 'Open';

  const { data: xReport, isFetching: xFetching } = useXReport(
    open && isOpen && sessionId ? sessionId : null,
  );

  return (
    <Dialog.Root open={open} onOpenChange={(o) => !o && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <Dialog.Content className={cn(
          'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
          'w-full max-w-xl rounded-xl bg-white dark:bg-gray-900',
          'border border-gray-200 dark:border-gray-700 shadow-xl focus:outline-none',
          'data-[state=open]:animate-in data-[state=closed]:animate-out',
          'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
          'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
          'data-[state=closed]:slide-out-to-left-1/2 data-[state=closed]:slide-out-to-top-48%',
          'data-[state=open]:slide-in-from-left-1/2 data-[state=open]:slide-in-from-top-48%',
        )}>
          {/* Header */}
          <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 px-6 py-4">
            <Dialog.Title className="flex items-center gap-2 text-base font-semibold text-gray-900 dark:text-white">
              <Monitor className="w-5 h-5 text-primary-700" />
              Till Session
              {session && (
                <span className={cn(
                  'ml-1 px-2 py-0.5 rounded-full text-xs font-medium',
                  isOpen
                    ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400'
                    : 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400',
                )}>
                  {session.sessionStatus}
                </span>
              )}
            </Dialog.Title>
            <Dialog.Close className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          <Dialog.Description className="sr-only">Till session financial detail</Dialog.Description>

          <div className="px-6 py-5 max-h-[75vh] overflow-y-auto">
            {/* Loading */}
            {isLoading && (
              <div className="flex items-center justify-center gap-2 py-16 text-gray-400">
                <Loader2 className="w-5 h-5 animate-spin" />
                <span className="text-sm">Loading session…</span>
              </div>
            )}

            {/* Error */}
            {isError && (
              <div className="flex items-center gap-2 text-red-500 py-8">
                <AlertCircle className="w-5 h-5 flex-shrink-0" />
                <span className="text-sm">Failed to load session details.</span>
              </div>
            )}

            {session && (
              <div className="space-y-5">
                {/* Meta cards */}
                <div className="grid grid-cols-2 gap-3">
                  {[
                    { icon: StoreIcon,  label: 'Store',      value: session.storeId.slice(0, 8) + '…' },
                    { icon: Monitor,    label: 'Terminal',   value: session.terminalId.slice(0, 8) + '…' },
                    { icon: UserIcon,   label: 'Opened By',  value: session.openedBy.slice(0, 8) + '…' },
                    { icon: BarChart2,  label: 'Duration',   value: sessionDuration(session.openedAt, session.closedAt) },
                  ].map(({ icon: Icon, label, value }) => (
                    <div key={label} className="flex items-start gap-2 rounded-lg bg-gray-50 dark:bg-gray-800 px-3 py-3">
                      <Icon className="w-4 h-4 text-gray-400 mt-0.5 flex-shrink-0" />
                      <div className="min-w-0">
                        <p className="text-xs text-gray-400 mb-0.5">{label}</p>
                        <p className="text-sm font-medium text-gray-900 dark:text-white truncate">{value}</p>
                      </div>
                    </div>
                  ))}
                </div>

                {/* Timing */}
                <div className="flex gap-6 text-sm">
                  <div>
                    <p className="text-xs text-gray-400 mb-0.5">Opened</p>
                    <p className="text-gray-700 dark:text-gray-300">{fmtDateTime(session.openedAt)}</p>
                  </div>
                  {session.closedAt && (
                    <div>
                      <p className="text-xs text-gray-400 mb-0.5">Closed</p>
                      <p className="text-gray-700 dark:text-gray-300">{fmtDateTime(session.closedAt)}</p>
                    </div>
                  )}
                </div>

                {/* Notes */}
                {session.notes && (
                  <div className="rounded-lg border border-gray-200 dark:border-gray-700 px-4 py-3">
                    <p className="text-xs text-gray-400 mb-1">Notes</p>
                    <p className="text-sm text-gray-700 dark:text-gray-300">{session.notes}</p>
                  </div>
                )}

                {/* Financial summary (closed) */}
                {!isOpen && (
                  <div className="rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">
                    <div className="bg-gray-50 dark:bg-gray-800 px-4 py-2.5 border-b border-gray-200 dark:border-gray-700">
                      <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Financial Summary
                      </p>
                    </div>
                    <div className="grid grid-cols-3 divide-x divide-gray-100 dark:divide-gray-800">
                      {[
                        { label: 'Opening Float',  value: fmt(session.openingFloat) },
                        { label: 'Closing Balance', value: fmt(session.closingBalance) },
                        {
                          label: 'Variance',
                          value: session.closingVariance != null
                            ? (session.closingVariance > 0 ? '+' : '') + fmt(session.closingVariance)
                            : '—',
                          color: session.closingVariance == null ? 'text-gray-900 dark:text-white'
                            : session.closingVariance > 0  ? 'text-emerald-600 dark:text-emerald-400'
                            : session.closingVariance < 0  ? 'text-red-600 dark:text-red-400'
                            : 'text-gray-900 dark:text-white',
                        },
                      ].map(({ label, value, color }) => (
                        <div key={label} className="px-4 py-3 text-center">
                          <p className="text-xs text-gray-400 mb-1">{label}</p>
                          <p className={cn('text-base font-semibold tabular-nums', color ?? 'text-gray-900 dark:text-white')}>
                            {value}
                          </p>
                        </div>
                      ))}
                    </div>
                  </div>
                )}

                {/* X-Report (open session) */}
                {isOpen && (
                  xFetching
                    ? (
                      <div className="flex items-center gap-2 text-sm text-gray-400 py-4 justify-center">
                        <Loader2 className="w-4 h-4 animate-spin" />
                        Loading X-Report…
                      </div>
                    )
                    : xReport && <ReportSection report={xReport} />
                )}

                {/* Petty cash */}
                {session.pettyTransactions.length > 0 ? (
                  <div className="rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">
                    <div className="bg-gray-50 dark:bg-gray-800 px-4 py-2.5 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between">
                      <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Petty Cash
                      </p>
                      <div className="flex items-center gap-3 text-xs">
                        {session.pettyTransactions.filter((p) => p.type === 'CashIn').length > 0 && (
                          <span className="text-emerald-600 dark:text-emerald-400 flex items-center gap-1">
                            <ArrowDownCircle className="w-3 h-3" />
                            In: {fmt(session.pettyTransactions.filter((p) => p.type === 'CashIn').reduce((s, p) => s + p.amount, 0))}
                          </span>
                        )}
                        {session.pettyTransactions.filter((p) => p.type === 'CashOut').length > 0 && (
                          <span className="text-red-600 dark:text-red-400 flex items-center gap-1">
                            <ArrowUpCircle className="w-3 h-3" />
                            Out: {fmt(session.pettyTransactions.filter((p) => p.type === 'CashOut').reduce((s, p) => s + p.amount, 0))}
                          </span>
                        )}
                      </div>
                    </div>
                    <div className="divide-y divide-gray-100 dark:divide-gray-800">
                      {session.pettyTransactions.map((p) => (
                        <div key={p.id} className="flex items-center px-4 py-2.5 gap-3">
                          {p.type === 'CashIn'
                            ? <ArrowDownCircle className="w-4 h-4 text-emerald-500 flex-shrink-0" />
                            : <ArrowUpCircle   className="w-4 h-4 text-red-500 flex-shrink-0" />}
                          <div className="flex-1 min-w-0">
                            <p className="text-sm text-gray-700 dark:text-gray-300 truncate">{p.reason}</p>
                            <p className="text-xs text-gray-400">
                              {new Date(p.createdAt).toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' })}
                            </p>
                          </div>
                          <span className={cn(
                            'text-sm font-semibold tabular-nums flex-shrink-0',
                            p.type === 'CashIn'
                              ? 'text-emerald-600 dark:text-emerald-400'
                              : 'text-red-600 dark:text-red-400',
                          )}>
                            {p.type === 'CashIn' ? '+' : '−'}{fmt(p.amount)}
                          </span>
                        </div>
                      ))}
                    </div>
                  </div>
                ) : (
                  <div className="flex items-center gap-2 text-sm text-gray-400 rounded-lg border border-gray-100 dark:border-gray-800 px-4 py-3">
                    <TrendingDown className="w-4 h-4 flex-shrink-0" />
                    No petty cash transactions recorded.
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="flex justify-end px-6 py-4 border-t border-gray-200 dark:border-gray-700">
            <button
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
            >
              Close
            </button>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
