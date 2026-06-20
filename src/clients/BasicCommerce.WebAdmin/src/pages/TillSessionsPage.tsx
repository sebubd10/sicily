import { useState } from 'react';
import {
  Monitor, RefreshCw, Eye, Loader2,
  AlertCircle, CheckCircle, Clock,
} from 'lucide-react';
import { cn } from '../lib/utils';
import type { TillSessionSummary } from '../types/tillSession';
import { useTillSessions } from '../hooks/useTillSessions';
import { useStoreList } from '../hooks/useStoreAdmin';
import { useTerminals } from '../hooks/useTerminals';
import { TillSessionDetailModal } from '../components/tillSessions/TillSessionDetailModal';
import { Pagination } from '../components/ui/Pagination';

function fmtDate(d: string | null | undefined): string {
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

function StatusBadge({ status }: { status: string }) {
  const isOpen = status === 'Open';
  return (
    <span className={cn(
      'inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full text-xs font-medium',
      isOpen
        ? 'bg-emerald-50 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400'
        : 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400',
    )}>
      <span className={cn(
        'w-1.5 h-1.5 rounded-full flex-shrink-0',
        isOpen ? 'bg-emerald-500' : 'bg-gray-400',
      )} />
      {status}
    </span>
  );
}

const PAGE_SIZE = 20;

export default function TillSessionsPage() {
  const [storeId,    setStoreId]    = useState('');
  const [terminalId, setTerminalId] = useState('');
  const [openOnly,   setOpenOnly]   = useState(false);
  const [page,       setPage]       = useState(1);
  const [detailId,   setDetailId]   = useState<string | null>(null);

  const { data: stores = [] }    = useStoreList();
  const { data: terminals = [] } = useTerminals();

  const storeTerminals = storeId
    ? terminals.filter((t) => t.storeId === storeId)
    : terminals;

  const { data, isLoading, isError, isFetching, refetch } = useTillSessions({
    storeId:    storeId    || undefined,
    terminalId: terminalId || undefined,
    openOnly,
    page,
    pageSize: PAGE_SIZE,
  });

  const sessions    = data?.items ?? [];
  const totalCount  = data?.totalCount ?? 0;
  const totalPages  = Math.ceil(totalCount / PAGE_SIZE);

  const openCount   = sessions.filter((s) => s.sessionStatus === 'Open').length;
  const closedCount = sessions.filter((s) => s.sessionStatus === 'Closed').length;

  function handleStoreChange(id: string) {
    setStoreId(id);
    setTerminalId('');
    setPage(1);
  }

  const selectCls = 'text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg px-3 py-2 text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500 cursor-pointer transition';

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <Monitor className="w-6 h-6 text-primary-700" />
            Till Sessions
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            View shift openings, closings, and cash movements by terminal
          </p>
        </div>

        <div className="flex items-center gap-2 flex-wrap">
          <button
            onClick={() => refetch()} disabled={isFetching}
            className="p-2 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors disabled:opacity-40"
            title="Refresh"
          >
            <RefreshCw className={cn('w-4 h-4', isFetching && 'animate-spin')} />
          </button>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        <select
          value={storeId}
          onChange={(e) => handleStoreChange(e.target.value)}
          className={selectCls}
        >
          <option value="">All stores</option>
          {stores.filter((s) => s.status === 'Active').map((s) => (
            <option key={s.id} value={s.id}>{s.name}</option>
          ))}
        </select>

        <select
          value={terminalId}
          onChange={(e) => { setTerminalId(e.target.value); setPage(1); }}
          disabled={storeTerminals.length === 0}
          className={selectCls}
        >
          <option value="">All terminals</option>
          {storeTerminals.map((t) => (
            <option key={t.id} value={t.id}>{t.name}</option>
          ))}
        </select>

        <label className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 cursor-pointer select-none">
          <div
            onClick={() => { setOpenOnly((v) => !v); setPage(1); }}
            className={cn(
              'w-9 h-5 rounded-full transition-colors relative flex-shrink-0',
              openOnly ? 'bg-primary-700' : 'bg-gray-300 dark:bg-gray-600',
            )}
          >
            <span className={cn(
              'absolute top-0.5 w-4 h-4 rounded-full bg-white shadow transition-all',
              openOnly ? 'left-4' : 'left-0.5',
            )} />
          </div>
          Open sessions only
        </label>

        {(isFetching && !isLoading) && <Loader2 className="w-4 h-4 animate-spin text-gray-400" />}

        <span className="text-xs text-gray-400 ml-auto">
          {totalCount} session{totalCount === 1 ? '' : 's'}
        </span>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-3 gap-4">
        <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm p-4">
          <div className="flex items-center gap-2 mb-1">
            <Monitor className="w-4 h-4 text-primary-600" />
            <span className="text-xs text-gray-500 dark:text-gray-400 font-medium">Total (this page)</span>
          </div>
          <p className="text-2xl font-bold text-gray-900 dark:text-white">{sessions.length}</p>
          <p className="text-xs text-gray-400 mt-0.5">of {totalCount} total</p>
        </div>
        <div className="bg-white dark:bg-gray-900 rounded-xl border border-emerald-200 dark:border-emerald-800 shadow-sm p-4">
          <div className="flex items-center gap-2 mb-1">
            <CheckCircle className="w-4 h-4 text-emerald-500" />
            <span className="text-xs text-emerald-600 dark:text-emerald-400 font-medium">Open</span>
          </div>
          <p className="text-2xl font-bold text-emerald-700 dark:text-emerald-400">{openCount}</p>
          <p className="text-xs text-gray-400 mt-0.5">active sessions</p>
        </div>
        <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm p-4">
          <div className="flex items-center gap-2 mb-1">
            <Clock className="w-4 h-4 text-gray-400" />
            <span className="text-xs text-gray-500 dark:text-gray-400 font-medium">Closed</span>
          </div>
          <p className="text-2xl font-bold text-gray-700 dark:text-gray-300">{closedCount}</p>
          <p className="text-xs text-gray-400 mt-0.5">completed sessions</p>
        </div>
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
        {isError ? (
          <div className="flex items-center gap-2 px-6 py-12 text-red-500">
            <AlertCircle className="w-5 h-5 flex-shrink-0" />
            <span className="text-sm">Failed to load till sessions. Please refresh.</span>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                  {['Terminal', 'Store', 'Opened By', 'Opening Float', 'Status', 'Opened At', 'Closed At', 'Duration', 'Actions'].map((h) => (
                    <th key={h} className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider whitespace-nowrap">
                      {h}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                {isLoading
                  ? Array.from({ length: 8 }).map((_, i) => (
                      <tr key={i} className="animate-pulse">
                        {Array.from({ length: 9 }).map((_, j) => (
                          <td key={j} className="px-4 py-3">
                            <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-full" />
                          </td>
                        ))}
                      </tr>
                    ))
                  : sessions.length === 0
                    ? (
                      <tr>
                        <td colSpan={9} className="px-4 py-16 text-center">
                          <Monitor className="w-8 h-8 text-gray-300 dark:text-gray-600 mx-auto mb-2" />
                          <p className="text-sm text-gray-400">No till sessions found.</p>
                          <p className="text-xs text-gray-400 mt-0.5">Adjust the filters or check back later.</p>
                        </td>
                      </tr>
                    )
                    : sessions.map((s: TillSessionSummary) => (
                        <tr
                          key={s.id}
                          className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
                        >
                          {/* Terminal */}
                          <td className="px-4 py-3">
                            <p className="font-medium text-gray-900 dark:text-white">
                              {s.terminalName ?? <span className="text-gray-400 font-mono text-xs">{s.terminalId.slice(0, 8)}…</span>}
                            </p>
                          </td>

                          {/* Store */}
                          <td className="px-4 py-3 text-gray-600 dark:text-gray-300">
                            {s.storeName ?? <span className="text-gray-400 font-mono text-xs">{s.storeId.slice(0, 8)}…</span>}
                          </td>

                          {/* Opened by */}
                          <td className="px-4 py-3 text-gray-600 dark:text-gray-300">
                            {s.openedByName ?? <span className="text-gray-400 font-mono text-xs">{s.openedBy.slice(0, 8)}…</span>}
                          </td>

                          {/* Opening float */}
                          <td className="px-4 py-3 tabular-nums text-gray-700 dark:text-gray-300 whitespace-nowrap">
                            {s.openingFloat.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                          </td>

                          {/* Status */}
                          <td className="px-4 py-3">
                            <StatusBadge status={s.sessionStatus} />
                          </td>

                          {/* Opened at */}
                          <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">
                            {fmtDate(s.openedAt)}
                          </td>

                          {/* Closed at */}
                          <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">
                            {fmtDate(s.closedAt)}
                          </td>

                          {/* Duration */}
                          <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap tabular-nums">
                            {sessionDuration(s.openedAt, s.closedAt)}
                          </td>

                          {/* Actions */}
                          <td className="px-4 py-3">
                            <button
                              onClick={() => setDetailId(s.id)}
                              title="View detail"
                              className="p-1.5 rounded-lg text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors"
                            >
                              <Eye className="w-3.5 h-3.5" />
                            </button>
                          </td>
                        </tr>
                      ))
                }
              </tbody>
            </table>
          </div>
        )}

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="px-4 py-3 border-t border-gray-100 dark:border-gray-800">
            <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
          </div>
        )}
      </div>

      {/* Detail modal */}
      <TillSessionDetailModal
        sessionId={detailId}
        open={!!detailId}
        onClose={() => setDetailId(null)}
      />
    </div>
  );
}
