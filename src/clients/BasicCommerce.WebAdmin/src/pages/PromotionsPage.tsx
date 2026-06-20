import { useState } from 'react';
import {
  Plus, Tag, Percent, DollarSign, Gift, ShoppingCart,
  Play, Pause, XCircle, ChevronLeft, ChevronRight,
  AlertCircle, Loader2, CalendarDays,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { PromotionStatus, PromotionSummary, PromotionType } from '../types/promotion';
import {
  usePromotions,
  useActivatePromotion,
  usePausePromotion,
  useCancelPromotion,
} from '../hooks/usePromotions';
import { CreatePromotionModal } from '../components/promotions/CreatePromotionModal';
import { ConfirmDialog } from '../components/ui/ConfirmDialog';

const PAGE_SIZE = 15;

type StatusFilter = 'All' | PromotionStatus;

const STATUS_FILTERS: StatusFilter[] = [
  'All', 'Active', 'Draft', 'Paused', 'Expired', 'Cancelled',
];

const STATUS_CONFIG: Record<PromotionStatus, { label: string; cls: string }> = {
  Draft: {
    label: 'Draft',
    cls: 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300',
  },
  Active: {
    label: 'Active',
    cls: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400',
  },
  Paused: {
    label: 'Paused',
    cls: 'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400',
  },
  Expired: {
    label: 'Expired',
    cls: 'bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-400',
  },
  Cancelled: {
    label: 'Cancelled',
    cls: 'bg-red-100 text-red-600 dark:bg-red-900/20 dark:text-red-400',
  },
};

const TYPE_CONFIG: Record<PromotionType, { label: string; icon: React.ReactNode; cls: string }> = {
  PercentageOff: {
    label: 'Pct Off',
    icon: <Percent className="w-3 h-3" />,
    cls: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300',
  },
  FixedAmountOff: {
    label: 'Fixed',
    icon: <DollarSign className="w-3 h-3" />,
    cls: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300',
  },
  BuyXGetYFree: {
    label: 'BXGY',
    icon: <Gift className="w-3 h-3" />,
    cls: 'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-300',
  },
  CategoryPercentageOff: {
    label: 'Category',
    icon: <Tag className="w-3 h-3" />,
    cls: 'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-300',
  },
  CartDiscount: {
    label: 'Cart',
    icon: <ShoppingCart className="w-3 h-3" />,
    cls: 'bg-rose-100 text-rose-700 dark:bg-rose-900/30 dark:text-rose-300',
  },
};

function discountSummary(p: PromotionSummary): string {
  const type = p.type as PromotionType;
  if (type === 'PercentageOff' || type === 'CategoryPercentageOff' || type === 'CartDiscount') {
    if (p.discountPercentage) return `${p.discountPercentage}% off`;
    if (p.discountAmount) return `৳${p.discountAmount} off`;
  }
  if (type === 'FixedAmountOff') {
    if (p.discountAmount) return `৳${p.discountAmount} off`;
  }
  if (type === 'BuyXGetYFree') return 'Buy X Get Y Free';
  return '—';
}

function fmtDateShort(d: string | null): string {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

export default function PromotionsPage() {
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('All');
  const [page, setPage] = useState(1);
  const [createOpen, setCreateOpen] = useState(false);
  const [cancelTarget, setCancelTarget] = useState<PromotionSummary | null>(null);

  const { data, isLoading, isError, error } = usePromotions({
    status: statusFilter === 'All' ? undefined : statusFilter,
    page,
    pageSize: PAGE_SIZE,
  });

  const { mutate: activate, isPending: activating } = useActivatePromotion();
  const { mutate: pause, isPending: pausing } = usePausePromotion();
  const { mutate: cancel, isPending: cancelling } = useCancelPromotion();

  const items = data?.items ?? [];
  const total = data?.totalCount ?? 0;
  const totalPages = Math.ceil(total / PAGE_SIZE) || 1;
  const apiError = isError ? extractApiError(error) : null;

  function handleStatusFilter(s: StatusFilter) {
    setStatusFilter(s);
    setPage(1);
  }

  return (
    <div className="flex flex-col gap-6 p-6 min-h-0">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100">Promotions</h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Manage discounts, deals, and special offers
          </p>
        </div>
        <button
          onClick={() => setCreateOpen(true)}
          className="flex items-center gap-2 px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium shadow-sm transition-colors"
        >
          <Plus className="w-4 h-4" />
          New Promotion
        </button>
      </div>

      {/* Status filter tabs */}
      <div className="flex gap-1 bg-gray-100 dark:bg-gray-800 rounded-xl p-1 w-fit">
        {STATUS_FILTERS.map((s) => (
          <button
            key={s}
            onClick={() => handleStatusFilter(s)}
            className={cn(
              'px-3 py-1.5 rounded-lg text-sm font-medium transition-all',
              statusFilter === s
                ? 'bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 shadow-sm'
                : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200',
            )}
          >
            {s}
          </button>
        ))}
      </div>

      {/* Content */}
      {isLoading ? (
        <div className="flex items-center justify-center py-24">
          <Loader2 className="w-8 h-8 text-indigo-500 animate-spin" />
        </div>
      ) : apiError ? (
        <div className="flex items-center gap-3 p-4 rounded-xl bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800">
          <AlertCircle className="w-5 h-5 text-red-500 flex-shrink-0" />
          <p className="text-sm text-red-700 dark:text-red-300">{apiError}</p>
        </div>
      ) : items.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-24 text-center">
          <div className="p-4 rounded-2xl bg-gray-100 dark:bg-gray-800 mb-4">
            <Tag className="w-8 h-8 text-gray-400" />
          </div>
          <p className="text-gray-600 dark:text-gray-400 font-medium">No promotions found</p>
          <p className="text-gray-400 dark:text-gray-500 text-sm mt-1">
            {statusFilter === 'All'
              ? 'Create your first promotion to get started'
              : `No ${statusFilter.toLowerCase()} promotions`}
          </p>
          {statusFilter === 'All' && (
            <button
              onClick={() => setCreateOpen(true)}
              className="mt-4 flex items-center gap-2 px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium transition-colors"
            >
              <Plus className="w-4 h-4" />
              New Promotion
            </button>
          )}
        </div>
      ) : (
        <>
          {/* Table */}
          <div className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-100 dark:border-gray-800 overflow-hidden shadow-sm">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50">
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Promotion
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Type
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Discount
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Period
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="px-4 py-3 text-right text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50 dark:divide-gray-800">
                  {items.map((p) => {
                    const status = p.promotionStatus as PromotionStatus;
                    const type = p.type as PromotionType;
                    const typeCfg = TYPE_CONFIG[type] ?? TYPE_CONFIG.PercentageOff;
                    const statusCfg = STATUS_CONFIG[status] ?? STATUS_CONFIG.Draft;
                    const canActivate = status === 'Draft' || status === 'Paused';
                    const canPause = status === 'Active';
                    const canCancel = status !== 'Cancelled' && status !== 'Expired';

                    return (
                      <tr key={p.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors">
                        <td className="px-4 py-3">
                          <p className="font-medium text-gray-900 dark:text-gray-100">{p.name}</p>
                        </td>
                        <td className="px-4 py-3">
                          <span className={cn(
                            'inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium',
                            typeCfg.cls,
                          )}>
                            {typeCfg.icon}
                            {typeCfg.label}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-gray-700 dark:text-gray-300 font-medium">
                          {discountSummary(p)}
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-1.5 text-gray-500 dark:text-gray-400 text-xs">
                            <CalendarDays className="w-3.5 h-3.5 flex-shrink-0" />
                            <span>
                              {fmtDateShort(p.startsAt)}
                              {p.endsAt ? ` → ${fmtDateShort(p.endsAt)}` : ' → No end'}
                            </span>
                          </div>
                        </td>
                        <td className="px-4 py-3">
                          <span className={cn(
                            'inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium',
                            statusCfg.cls,
                          )}>
                            {statusCfg.label}
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex items-center justify-end gap-1">
                            {canActivate && (
                              <button
                                title="Activate"
                                onClick={() => activate(p.id)}
                                disabled={activating}
                                className="p-1.5 rounded-lg text-emerald-600 dark:text-emerald-400 hover:bg-emerald-50 dark:hover:bg-emerald-900/20 disabled:opacity-40 transition-colors"
                              >
                                <Play className="w-4 h-4 fill-current" />
                              </button>
                            )}
                            {canPause && (
                              <button
                                title="Pause"
                                onClick={() => pause(p.id)}
                                disabled={pausing}
                                className="p-1.5 rounded-lg text-amber-600 dark:text-amber-400 hover:bg-amber-50 dark:hover:bg-amber-900/20 disabled:opacity-40 transition-colors"
                              >
                                <Pause className="w-4 h-4 fill-current" />
                              </button>
                            )}
                            {canCancel && (
                              <button
                                title="Cancel"
                                onClick={() => setCancelTarget(p)}
                                className="p-1.5 rounded-lg text-red-500 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
                              >
                                <XCircle className="w-4 h-4" />
                              </button>
                            )}
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>

          {/* Pagination */}
          <div className="flex items-center justify-between text-sm text-gray-500 dark:text-gray-400">
            <span>
              {total} promotion{total !== 1 ? 's' : ''}
              {statusFilter !== 'All' && ` · ${statusFilter}`}
            </span>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="p-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 disabled:opacity-40 transition-colors"
              >
                <ChevronLeft className="w-4 h-4" />
              </button>
              <span className="text-xs">
                Page {page} of {totalPages}
              </span>
              <button
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
                className="p-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 disabled:opacity-40 transition-colors"
              >
                <ChevronRight className="w-4 h-4" />
              </button>
            </div>
          </div>
        </>
      )}

      {/* Create modal */}
      <CreatePromotionModal open={createOpen} onClose={() => setCreateOpen(false)} />

      {/* Cancel confirm */}
      <ConfirmDialog
        open={!!cancelTarget}
        title="Cancel Promotion"
        message={
          cancelTarget
            ? `Are you sure you want to cancel "${cancelTarget.name}"? This cannot be undone.`
            : ''
        }
        confirmLabel="Cancel Promotion"
        variant="danger"
        loading={cancelling}
        onConfirm={() => {
          if (!cancelTarget) return;
          cancel(cancelTarget.id, { onSuccess: () => setCancelTarget(null) });
        }}
        onClose={() => setCancelTarget(null)}
      />
    </div>
  );
}
