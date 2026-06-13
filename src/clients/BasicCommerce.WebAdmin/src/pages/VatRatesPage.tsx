import { useState, useMemo } from 'react';
import {
  Search, Plus, Pencil, PowerOff, Power, Trash2, Loader2,
  AlertCircle, Percent, Star,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { VatRate, VatRateFormData } from '../types/vatRate';
import { VatRateModal } from '../components/vatRates/VatRateModal';
import { ConfirmDialog } from '../components/ui/ConfirmDialog';
import {
  useVatRates,
  useVatRateDetail,
  useCreateVatRate,
  useUpdateVatRate,
  useActivateVatRate,
  useDeactivateVatRate,
  useDeleteVatRate,
} from '../hooks/useVatRates';

type ConfirmState =
  | { type: 'deactivate'; vatRate: VatRate }
  | { type: 'activate';   vatRate: VatRate }
  | { type: 'delete';     vatRate: VatRate }
  | null;

export default function VatRatesPage() {
  const [search, setSearch]             = useState('');
  const [showInactive, setShowInactive] = useState(false);

  const [modalOpen, setModalOpen] = useState(false);
  const [editId, setEditId]       = useState<string | null>(null);
  const [confirm, setConfirm]     = useState<ConfirmState>(null);

  const { data: vatRates = [], isLoading, isError, isFetching } = useVatRates();
  const { data: editVatRate, isFetching: isFetchingEdit } = useVatRateDetail(editId);

  const createMutation     = useCreateVatRate();
  const updateMutation     = useUpdateVatRate();
  const activateMutation   = useActivateVatRate();
  const deactivateMutation = useDeactivateVatRate();
  const deleteMutation     = useDeleteVatRate();

  const [confirmError, setConfirmError] = useState<string | null>(null);

  const isMutating =
    createMutation.isPending ||
    updateMutation.isPending ||
    activateMutation.isPending ||
    deactivateMutation.isPending ||
    deleteMutation.isPending;

  const filtered = useMemo(() => {
    let list = vatRates;
    if (!showInactive) list = list.filter((v) => v.status === 'Active');
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter(
        (v) =>
          v.name.toLowerCase().includes(q) ||
          v.code.toLowerCase().includes(q),
      );
    }
    return list;
  }, [vatRates, search, showInactive]);

  const stats = useMemo(() => ({
    total:  vatRates.length,
    active: vatRates.filter((v) => v.status === 'Active').length,
  }), [vatRates]);

  function openAdd() { setEditId(null); setModalOpen(true); }
  function openEdit(v: VatRate) { setEditId(v.id); setModalOpen(true); }

  function handleToggleStatus(v: VatRate) {
    setConfirm(
      v.status === 'Active'
        ? { type: 'deactivate', vatRate: v }
        : { type: 'activate',   vatRate: v },
    );
  }

  async function handleSave(form: VatRateFormData) {
    if (editId) {
      await updateMutation.mutateAsync({ id: editId, form });
    } else {
      await createMutation.mutateAsync(form);
    }
    setModalOpen(false);
    setEditId(null);
  }

  async function executeConfirm() {
    if (!confirm) return;
    try {
      setConfirmError(null);
      if (confirm.type === 'deactivate') await deactivateMutation.mutateAsync(confirm.vatRate.id);
      if (confirm.type === 'activate')   await activateMutation.mutateAsync(confirm.vatRate.id);
      if (confirm.type === 'delete')     await deleteMutation.mutateAsync(confirm.vatRate.id);
      setConfirm(null);
    } catch (err) {
      setConfirmError(extractApiError(err));
    }
  }

  const confirmProps = (() => {
    if (!confirm) return null;
    const name = confirm.vatRate.name;
    if (confirm.type === 'deactivate') return {
      title: 'Deactivate VAT Rate',
      message: `"${name}" will no longer be available for new products.`,
      confirmLabel: 'Deactivate',
      variant: 'warning' as const,
    };
    if (confirm.type === 'delete') return {
      title: 'Delete VAT Rate',
      message: `"${name}" will be permanently removed. This cannot be undone.`,
      confirmLabel: 'Delete',
      variant: 'danger' as const,
    };
    return {
      title: 'Activate VAT Rate',
      message: `"${name}" will be made active again.`,
      confirmLabel: 'Activate',
      variant: 'warning' as const,
    };
  })();

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <Percent className="w-6 h-6 text-primary-700" /> VAT Rates
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Manage VAT rate configurations applied to products
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={openAdd}
            className="flex items-center gap-2 px-4 py-2 bg-primary-800 hover:bg-primary-900 text-white text-sm font-medium rounded-lg transition-colors shadow-sm"
          >
            <Plus className="w-4 h-4" /> Add VAT Rate
          </button>
        </div>
      </div>

      {/* Stats */}
      <div className="flex flex-wrap gap-3">
        {[
          { label: 'Total',    value: stats.total,  color: 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300' },
          { label: 'Active',   value: stats.active, color: 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300' },
          { label: 'Inactive', value: stats.total - stats.active, color: 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400' },
        ].map((s) => (
          <span key={s.label} className={cn('px-3 py-1 rounded-full text-sm font-medium', s.color)}>
            {s.label}: <strong>{s.value}</strong>
          </span>
        ))}
      </div>

      {/* Toolbar */}
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            placeholder="Search VAT rates..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-9 pr-4 py-2 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-gray-900 dark:text-white placeholder-gray-400"
          />
        </div>
        <label className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 cursor-pointer select-none">
          <div
            onClick={() => setShowInactive((v) => !v)}
            className={cn(
              'w-9 h-5 rounded-full transition-colors relative',
              showInactive ? 'bg-primary-700' : 'bg-gray-300 dark:bg-gray-600',
            )}
          >
            <span className={cn(
              'absolute top-0.5 w-4 h-4 rounded-full bg-white shadow transition-all',
              showInactive ? 'left-4' : 'left-0.5',
            )} />
          </div>
          Show inactive
        </label>
        {isFetching && !isLoading && (
          <Loader2 className="w-4 h-4 animate-spin text-gray-400 self-center" />
        )}
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
        {isError ? (
          <div className="flex items-center gap-2 px-6 py-12 text-red-500">
            <AlertCircle className="w-5 h-5 flex-shrink-0" />
            <span className="text-sm">Failed to load VAT rates. Please refresh and try again.</span>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                    {['#', 'Name', 'Code', 'Rate', 'Default', 'Status', 'Actions'].map((h) => (
                      <th
                        key={h}
                        className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider whitespace-nowrap"
                      >
                        {h}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                  {isLoading ? (
                    Array.from({ length: 4 }).map((_, i) => (
                      <tr key={i} className="animate-pulse">
                        {Array.from({ length: 7 }).map((_, j) => (
                          <td key={j} className="px-4 py-3">
                            <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-full" />
                          </td>
                        ))}
                      </tr>
                    ))
                  ) : filtered.length === 0 ? (
                    <tr>
                      <td colSpan={7} className="px-4 py-12 text-center text-sm text-gray-400">
                        No VAT rates found.
                      </td>
                    </tr>
                  ) : (
                    filtered.map((v, idx) => (
                      <tr key={v.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                        <td className="px-4 py-3 text-gray-400 text-xs">{idx + 1}</td>
                        <td className="px-4 py-3 font-medium text-gray-900 dark:text-white whitespace-nowrap">
                          {v.name}
                        </td>
                        <td className="px-4 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">
                          {v.code}
                        </td>
                        <td className="px-4 py-3 text-gray-700 dark:text-gray-300">
                          {v.rate}%
                        </td>
                        <td className="px-4 py-3">
                          {v.isDefault ? (
                            <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-primary-100 dark:bg-primary-900/30 text-primary-700 dark:text-primary-300">
                              <Star className="w-3 h-3 fill-current" /> Default
                            </span>
                          ) : (
                            <span className="text-gray-300 dark:text-gray-600">—</span>
                          )}
                        </td>
                        <td className="px-4 py-3">
                          <span className={cn(
                            'inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium',
                            v.status === 'Active'
                              ? 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300'
                              : 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400',
                          )}>
                            <span className={cn(
                              'w-1.5 h-1.5 rounded-full mr-1.5',
                              v.status === 'Active' ? 'bg-emerald-500' : 'bg-red-400',
                            )} />
                            {v.status}
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-1">
                            <button
                              title="Edit"
                              onClick={() => openEdit(v)}
                              disabled={isMutating || isFetchingEdit}
                              className="p-1.5 rounded-lg text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors disabled:opacity-40"
                            >
                              {isFetchingEdit && editId === v.id
                                ? <Loader2 className="w-3.5 h-3.5 animate-spin" />
                                : <Pencil className="w-3.5 h-3.5" />}
                            </button>
                            <button
                              title={v.status === 'Active' ? 'Deactivate' : 'Activate'}
                              onClick={() => handleToggleStatus(v)}
                              disabled={isMutating}
                              className={cn(
                                'p-1.5 rounded-lg transition-colors disabled:opacity-40',
                                v.status === 'Active'
                                  ? 'text-gray-400 hover:text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-900/20'
                                  : 'text-gray-400 hover:text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/20',
                              )}
                            >
                              {v.status === 'Active'
                                ? <PowerOff className="w-3.5 h-3.5" />
                                : <Power    className="w-3.5 h-3.5" />}
                            </button>
                            <button
                              title="Delete"
                              onClick={() => setConfirm({ type: 'delete', vatRate: v })}
                              disabled={isMutating}
                              className="p-1.5 rounded-lg text-gray-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-40"
                            >
                              <Trash2 className="w-3.5 h-3.5" />
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>

            <div className="px-6 py-3 border-t border-gray-100 dark:border-gray-800 text-xs text-gray-400">
              {filtered.length} of {vatRates.length} VAT rates
            </div>
          </>
        )}
      </div>

      {/* Create / Edit Modal */}
      <VatRateModal
        open={modalOpen && (!editId || !!editVatRate)}
        vatRate={editVatRate ?? null}
        onSave={handleSave}
        onClose={() => { setModalOpen(false); setEditId(null); }}
        isSaving={createMutation.isPending || updateMutation.isPending}
      />

      {/* Confirm Dialog */}
      {confirmProps && (
        <ConfirmDialog
          open={!!confirm}
          title={confirmProps.title}
          message={confirmProps.message}
          confirmLabel={confirmProps.confirmLabel}
          variant={confirmProps.variant}
          loading={isMutating}
          error={confirmError}
          onConfirm={executeConfirm}
          onClose={() => { setConfirm(null); setConfirmError(null); }}
        />
      )}
    </div>
  );
}
