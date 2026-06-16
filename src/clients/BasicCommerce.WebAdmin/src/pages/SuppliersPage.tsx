import { useState, useMemo } from 'react';
import {
  Search, Plus, Pencil, PowerOff, Power, Trash2, Loader2,
  AlertCircle, Truck,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { Supplier, SupplierFormData } from '../types/supplier';
import { SupplierModal } from '../components/suppliers/SupplierModal';
import { ConfirmDialog } from '../components/ui/ConfirmDialog';
import {
  useSuppliers,
  useSupplierDetail,
  useCreateSupplier,
  useUpdateSupplier,
  useActivateSupplier,
  useDeactivateSupplier,
  useDeleteSupplier,
} from '../hooks/useSuppliers';

type StatusFilter = 'All' | 'Active' | 'Inactive';

type ConfirmState =
  | { type: 'deactivate'; supplier: Supplier }
  | { type: 'activate';   supplier: Supplier }
  | { type: 'delete';     supplier: Supplier }
  | null;

export default function SuppliersPage() {
  const [search, setSearch]             = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('Active');

  const [modalOpen, setModalOpen] = useState(false);
  const [editId, setEditId]       = useState<string | null>(null);
  const [confirm, setConfirm]     = useState<ConfirmState>(null);
  const [confirmError, setConfirmError] = useState<string | null>(null);

  const { data: suppliers = [], isLoading, isError, isFetching } = useSuppliers();
  const { data: editSupplier, isFetching: isFetchingEdit } = useSupplierDetail(editId);

  const createMutation     = useCreateSupplier();
  const updateMutation     = useUpdateSupplier();
  const activateMutation   = useActivateSupplier();
  const deactivateMutation = useDeactivateSupplier();
  const deleteMutation     = useDeleteSupplier();

  const isMutating =
    createMutation.isPending ||
    updateMutation.isPending ||
    activateMutation.isPending ||
    deactivateMutation.isPending ||
    deleteMutation.isPending;

  const filtered = useMemo(() => {
    let list = suppliers;
    if (statusFilter !== 'All') list = list.filter((s) => s.status === statusFilter);
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter(
        (s) =>
          s.name.toLowerCase().includes(q) ||
          s.code.toLowerCase().includes(q) ||
          (s.contactName ?? '').toLowerCase().includes(q) ||
          (s.email ?? '').toLowerCase().includes(q),
      );
    }
    return list;
  }, [suppliers, search, statusFilter]);

  const stats = useMemo(() => ({
    total:  suppliers.length,
    active: suppliers.filter((s) => s.status === 'Active').length,
  }), [suppliers]);

  function openAdd() { setEditId(null); setModalOpen(true); }
  function openEdit(s: Supplier) { setEditId(s.id); setModalOpen(true); }

  function handleToggleStatus(s: Supplier) {
    setConfirm(
      s.status === 'Active'
        ? { type: 'deactivate', supplier: s }
        : { type: 'activate',   supplier: s },
    );
  }

  async function handleSave(form: SupplierFormData) {
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
      if (confirm.type === 'deactivate') await deactivateMutation.mutateAsync(confirm.supplier.id);
      if (confirm.type === 'activate')   await activateMutation.mutateAsync(confirm.supplier.id);
      if (confirm.type === 'delete')     await deleteMutation.mutateAsync(confirm.supplier.id);
      setConfirm(null);
    } catch (err) {
      setConfirmError(extractApiError(err));
    }
  }

  const confirmProps = (() => {
    if (!confirm) return null;
    const name = confirm.supplier.name;
    if (confirm.type === 'deactivate') return {
      title: 'Deactivate Supplier',
      message: `"${name}" will be hidden from purchasing workflows.`,
      confirmLabel: 'Deactivate',
      variant: 'warning' as const,
    };
    if (confirm.type === 'delete') return {
      title: 'Delete Supplier',
      message: `"${name}" will be permanently removed. This cannot be undone.`,
      confirmLabel: 'Delete',
      variant: 'danger' as const,
    };
    return {
      title: 'Activate Supplier',
      message: `"${name}" will be made active again.`,
      confirmLabel: 'Activate',
      variant: 'warning' as const,
    };
  })();

  const STATUS_OPTIONS: StatusFilter[] = ['All', 'Active', 'Inactive'];

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <Truck className="w-6 h-6 text-primary-700" /> Suppliers
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Manage suppliers and their contact details
          </p>
        </div>
        <button
          onClick={openAdd}
          className="flex items-center gap-2 px-4 py-2 bg-primary-800 hover:bg-primary-900 text-white text-sm font-medium rounded-lg transition-colors shadow-sm self-start"
        >
          <Plus className="w-4 h-4" /> Add Supplier
        </button>
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
            placeholder="Search suppliers..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-9 pr-4 py-2 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-gray-900 dark:text-white placeholder-gray-400"
          />
        </div>

        {/* Status radio group */}
        <div className="flex items-center gap-1 bg-gray-100 dark:bg-gray-800 rounded-lg p-1">
          {STATUS_OPTIONS.map((opt) => (
            <button
              key={opt}
              onClick={() => setStatusFilter(opt)}
              className={cn(
                'px-3 py-1 text-sm font-medium rounded-md transition-colors',
                statusFilter === opt
                  ? 'bg-white dark:bg-gray-700 text-gray-900 dark:text-white shadow-sm'
                  : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300',
              )}
            >
              {opt}
            </button>
          ))}
        </div>

        {isFetching && !isLoading && (
          <Loader2 className="w-4 h-4 animate-spin text-gray-400 self-center" />
        )}
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
        {isError ? (
          <div className="flex items-center gap-2 px-6 py-12 text-red-500">
            <AlertCircle className="w-5 h-5 flex-shrink-0" />
            <span className="text-sm">Failed to load suppliers. Please refresh and try again.</span>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                    {['#', 'Name', 'Code', 'Contact', 'Email / Phone', 'City', 'Lead Time', 'Status', 'Actions'].map((h) => (
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
                        No suppliers found.
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
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                          {s.contactName ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                        </td>
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400">
                          {s.email || s.phone ? (
                            <div className="flex flex-col">
                              {s.email && <span className="text-xs">{s.email}</span>}
                              {s.phone && <span className="text-xs">{s.phone}</span>}
                            </div>
                          ) : (
                            <span className="text-gray-300 dark:text-gray-600">—</span>
                          )}
                        </td>
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                          {s.city ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                        </td>
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                          {s.leadTimeDays > 0 ? `${s.leadTimeDays}d` : <span className="text-gray-300 dark:text-gray-600">—</span>}
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
                            <button
                              title="Delete"
                              onClick={() => setConfirm({ type: 'delete', supplier: s })}
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
              {filtered.length} of {suppliers.length} suppliers
            </div>
          </>
        )}
      </div>

      {/* Create / Edit Modal */}
      <SupplierModal
        open={modalOpen && (!editId || !!editSupplier)}
        supplier={editSupplier ?? null}
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
