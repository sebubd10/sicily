import { useState, useMemo } from 'react';
import {
  Search, Plus, Pencil, PowerOff, Power, Loader2,
  AlertCircle, Store as StoreIcon, Clock, Monitor,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { Store, StoreFormData } from '../types/store';
import { StoreModal } from '../components/stores/StoreModal';
import { ConfirmDialog } from '../components/ui/ConfirmDialog';
import {
  useStoreList,
  useStoreDetail,
  useCreateStore,
  useUpdateStore,
  useActivateStore,
  useDeactivateStore,
} from '../hooks/useStoreAdmin';

type ConfirmState =
  | { type: 'deactivate'; store: Store }
  | { type: 'activate';   store: Store }
  | null;

export default function StoresPage() {
  const [search, setSearch]             = useState('');
  const [showInactive, setShowInactive] = useState(false);

  const [modalOpen, setModalOpen] = useState(false);
  const [editId, setEditId]       = useState<string | null>(null);
  const [confirm, setConfirm]     = useState<ConfirmState>(null);

  const { data: stores = [], isLoading, isError, isFetching } = useStoreList();
  const { data: editStore, isFetching: isFetchingEdit } = useStoreDetail(editId);

  const createMutation     = useCreateStore();
  const updateMutation     = useUpdateStore();
  const activateMutation   = useActivateStore();
  const deactivateMutation = useDeactivateStore();

  const [confirmError, setConfirmError] = useState<string | null>(null);

  const isMutating =
    createMutation.isPending ||
    updateMutation.isPending ||
    activateMutation.isPending ||
    deactivateMutation.isPending;

  const filtered = useMemo(() => {
    let list = stores;
    if (!showInactive) list = list.filter((s) => s.status === 'Active');
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter(
        (s) =>
          s.name.toLowerCase().includes(q) ||
          s.code.toLowerCase().includes(q) ||
          s.address.toLowerCase().includes(q),
      );
    }
    return list;
  }, [stores, search, showInactive]);

  const stats = useMemo(() => ({
    total:  stores.length,
    active: stores.filter((s) => s.status === 'Active').length,
  }), [stores]);

  function openAdd() { setEditId(null); setModalOpen(true); }
  function openEdit(s: Store) { setEditId(s.id); setModalOpen(true); }

  function handleToggleStatus(s: Store) {
    setConfirm(
      s.status === 'Active'
        ? { type: 'deactivate', store: s }
        : { type: 'activate',   store: s },
    );
  }

  async function handleSave(form: StoreFormData) {
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
      if (confirm.type === 'deactivate') await deactivateMutation.mutateAsync(confirm.store.id);
      if (confirm.type === 'activate')   await activateMutation.mutateAsync(confirm.store.id);
      setConfirm(null);
    } catch (err) {
      setConfirmError(extractApiError(err));
    }
  }

  const confirmProps = (() => {
    if (!confirm) return null;
    const name = confirm.store.name;
    if (confirm.type === 'deactivate') return {
      title: 'Deactivate Store',
      message: `"${name}" will be hidden from store selectors and dropdowns.`,
      confirmLabel: 'Deactivate',
      variant: 'warning' as const,
    };
    return {
      title: 'Activate Store',
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
            <StoreIcon className="w-6 h-6 text-primary-700" /> Stores
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Manage store locations and their details
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={openAdd}
            className="flex items-center gap-2 px-4 py-2 bg-primary-800 hover:bg-primary-900 text-white text-sm font-medium rounded-lg transition-colors shadow-sm"
          >
            <Plus className="w-4 h-4" /> Add Store
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
            placeholder="Search stores..."
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
            <span className="text-sm">Failed to load stores. Please refresh and try again.</span>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                    {['#', 'Name', 'Code', 'Address', 'Hours', 'Terminals', 'Contact', 'Status', 'Actions'].map((h) => (
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
                        {Array.from({ length: 9 }).map((_, j) => (
                          <td key={j} className="px-4 py-3">
                            <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-full" />
                          </td>
                        ))}
                      </tr>
                    ))
                  ) : filtered.length === 0 ? (
                    <tr>
                      <td colSpan={9} className="px-4 py-12 text-center text-sm text-gray-400">
                        No stores found.
                      </td>
                    </tr>
                  ) : (
                    filtered.map((s, idx) => (
                      <tr key={s.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                        <td className="px-4 py-3 text-gray-400 text-xs">{idx + 1}</td>
                        <td className="px-4 py-3 font-medium text-gray-900 dark:text-white whitespace-nowrap">
                          {s.name}
                        </td>
                        <td className="px-4 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">
                          {s.code}
                        </td>
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400 max-w-[220px] truncate">
                          {s.address}
                        </td>
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                          <span className="inline-flex items-center gap-1">
                            <Clock className="w-3.5 h-3.5" /> {s.openingTime}–{s.closingTime}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400">
                          <span className="inline-flex items-center gap-1">
                            <Monitor className="w-3.5 h-3.5" /> {s.terminalCount}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400">
                          {s.phone || s.email ? (
                            <div className="flex flex-col">
                              {s.phone && <span>{s.phone}</span>}
                              {s.email && <span className="text-xs">{s.email}</span>}
                            </div>
                          ) : (
                            <span className="text-gray-300 dark:text-gray-600">—</span>
                          )}
                        </td>
                        <td className="px-4 py-3">
                          <span className={cn(
                            'inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium',
                            s.status === 'Active'
                              ? 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300'
                              : 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400',
                          )}>
                            <span className={cn(
                              'w-1.5 h-1.5 rounded-full mr-1.5',
                              s.status === 'Active' ? 'bg-emerald-500' : 'bg-red-400',
                            )} />
                            {s.status}
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-1">
                            <button
                              title="Edit"
                              onClick={() => openEdit(s)}
                              disabled={isMutating || isFetchingEdit}
                              className="p-1.5 rounded-lg text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors disabled:opacity-40"
                            >
                              {isFetchingEdit && editId === s.id
                                ? <Loader2 className="w-3.5 h-3.5 animate-spin" />
                                : <Pencil className="w-3.5 h-3.5" />}
                            </button>
                            <button
                              title={s.status === 'Active' ? 'Deactivate' : 'Activate'}
                              onClick={() => handleToggleStatus(s)}
                              disabled={isMutating}
                              className={cn(
                                'p-1.5 rounded-lg transition-colors disabled:opacity-40',
                                s.status === 'Active'
                                  ? 'text-gray-400 hover:text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-900/20'
                                  : 'text-gray-400 hover:text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/20',
                              )}
                            >
                              {s.status === 'Active'
                                ? <PowerOff className="w-3.5 h-3.5" />
                                : <Power    className="w-3.5 h-3.5" />}
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
              {filtered.length} of {stores.length} stores
            </div>
          </>
        )}
      </div>

      {/* Create / Edit Modal */}
      <StoreModal
        open={modalOpen && (!editId || !!editStore)}
        store={editStore ?? null}
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
