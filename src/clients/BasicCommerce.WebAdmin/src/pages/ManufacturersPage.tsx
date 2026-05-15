import { useState, useMemo } from 'react';
import {
  Search, Plus, Pencil, PowerOff, Power, Loader2,
  AlertCircle, Factory, Download,
} from 'lucide-react';
import { cn } from '../lib/utils';
import type { Manufacturer, ManufacturerFormData } from '../types/manufacturer';
import { ManufacturerModal } from '../components/manufacturers/ManufacturerModal';
import { ConfirmDialog } from '../components/ui/ConfirmDialog';
import { exportManufacturersPdf } from '../api/manufacturersApi';
import {
  useManufacturers,
  useManufacturerDetail,
  useCreateManufacturer,
  useUpdateManufacturer,
  useActivateManufacturer,
  useDeactivateManufacturer,
} from '../hooks/useManufacturers';

type ConfirmState =
  | { type: 'deactivate'; manufacturer: Manufacturer }
  | { type: 'activate';   manufacturer: Manufacturer }
  | null;

export default function ManufacturersPage() {
  const [search, setSearch]             = useState('');
  const [showInactive, setShowInactive] = useState(false);
  const [isExporting, setIsExporting]   = useState(false);

  const [modalOpen, setModalOpen] = useState(false);
  const [editId, setEditId]       = useState<string | null>(null);
  const [confirm, setConfirm]     = useState<ConfirmState>(null);

  const { data: manufacturers = [], isLoading, isError, isFetching } = useManufacturers();
  const { data: editManufacturer, isFetching: isFetchingEdit } = useManufacturerDetail(editId);

  const createMutation     = useCreateManufacturer();
  const updateMutation     = useUpdateManufacturer();
  const activateMutation   = useActivateManufacturer();
  const deactivateMutation = useDeactivateManufacturer();

  const isMutating =
    createMutation.isPending ||
    updateMutation.isPending ||
    activateMutation.isPending ||
    deactivateMutation.isPending;

  const filtered = useMemo(() => {
    let list = manufacturers;
    if (!showInactive) list = list.filter((m) => m.status === 'Active');
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter(
        (m) =>
          m.name.toLowerCase().includes(q) ||
          (m.code ?? '').toLowerCase().includes(q) ||
          (m.country ?? '').toLowerCase().includes(q) ||
          (m.contactEmail ?? '').toLowerCase().includes(q),
      );
    }
    return list;
  }, [manufacturers, search, showInactive]);

  const stats = useMemo(() => ({
    total:  manufacturers.length,
    active: manufacturers.filter((m) => m.status === 'Active').length,
  }), [manufacturers]);

  function openAdd() { setEditId(null); setModalOpen(true); }
  function openEdit(m: Manufacturer) { setEditId(m.id); setModalOpen(true); }

  function handleToggleStatus(m: Manufacturer) {
    setConfirm(
      m.status === 'Active'
        ? { type: 'deactivate', manufacturer: m }
        : { type: 'activate',   manufacturer: m },
    );
  }

  async function handleSave(form: ManufacturerFormData) {
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
    if (confirm.type === 'deactivate') await deactivateMutation.mutateAsync(confirm.manufacturer.id);
    if (confirm.type === 'activate')   await activateMutation.mutateAsync(confirm.manufacturer.id);
    setConfirm(null);
  }

  async function handleExportPdf() {
    setIsExporting(true);
    try {
      await exportManufacturersPdf({
        search: search.trim() || undefined,
        includeInactive: showInactive,
      });
    } finally {
      setIsExporting(false);
    }
  }

  const confirmProps = (() => {
    if (!confirm) return null;
    const name = confirm.manufacturer.name;
    if (confirm.type === 'deactivate') return {
      title: 'Deactivate Manufacturer',
      message: `"${name}" will be hidden from product listings.`,
      confirmLabel: 'Deactivate',
      variant: 'warning' as const,
    };
    return {
      title: 'Activate Manufacturer',
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
            <Factory className="w-6 h-6 text-primary-700" /> Manufacturers
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Manage product manufacturers and brands
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={handleExportPdf}
            disabled={isExporting}
            title={search || showInactive ? 'Export current filter as PDF' : 'Export all manufacturers as PDF'}
            className="flex items-center gap-2 px-4 py-2 border border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 text-sm font-medium rounded-lg transition-colors disabled:opacity-50"
          >
            {isExporting ? <Loader2 className="w-4 h-4 animate-spin" /> : <Download className="w-4 h-4" />}
            {isExporting ? 'Generating…' : 'Export PDF'}
          </button>
          <button
            onClick={openAdd}
            className="flex items-center gap-2 px-4 py-2 bg-primary-800 hover:bg-primary-900 text-white text-sm font-medium rounded-lg transition-colors shadow-sm"
          >
            <Plus className="w-4 h-4" /> Add Manufacturer
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
            placeholder="Search manufacturers..."
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
            <span className="text-sm">Failed to load manufacturers. Please refresh and try again.</span>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                    {['#', 'Name', 'Code', 'Country', 'Contact Email', 'Website', 'Status', 'Actions'].map((h) => (
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
                    Array.from({ length: 5 }).map((_, i) => (
                      <tr key={i} className="animate-pulse">
                        {Array.from({ length: 8 }).map((_, j) => (
                          <td key={j} className="px-4 py-3">
                            <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-full" />
                          </td>
                        ))}
                      </tr>
                    ))
                  ) : filtered.length === 0 ? (
                    <tr>
                      <td colSpan={8} className="px-4 py-12 text-center text-sm text-gray-400">
                        No manufacturers found.
                      </td>
                    </tr>
                  ) : (
                    filtered.map((m, idx) => (
                      <tr key={m.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                        <td className="px-4 py-3 text-gray-400 text-xs">{idx + 1}</td>
                        <td className="px-4 py-3 font-medium text-gray-900 dark:text-white whitespace-nowrap">
                          {m.name}
                        </td>
                        <td className="px-4 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">
                          {m.code ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                        </td>
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                          {m.country ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                        </td>
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400">
                          {m.contactEmail
                            ? <a href={`mailto:${m.contactEmail}`} className="hover:text-primary-700 transition-colors">{m.contactEmail}</a>
                            : <span className="text-gray-300 dark:text-gray-600">—</span>}
                        </td>
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400 max-w-[160px] truncate">
                          {m.website
                            ? <a href={m.website} target="_blank" rel="noopener noreferrer" className="hover:text-primary-700 transition-colors">{m.website.replace(/^https?:\/\//, '')}</a>
                            : <span className="text-gray-300 dark:text-gray-600">—</span>}
                        </td>
                        <td className="px-4 py-3">
                          <span className={cn(
                            'inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium',
                            m.status === 'Active'
                              ? 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300'
                              : 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400',
                          )}>
                            <span className={cn(
                              'w-1.5 h-1.5 rounded-full mr-1.5',
                              m.status === 'Active' ? 'bg-emerald-500' : 'bg-red-400',
                            )} />
                            {m.status}
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-1">
                            <button
                              title="Edit"
                              onClick={() => openEdit(m)}
                              disabled={isMutating || isFetchingEdit}
                              className="p-1.5 rounded-lg text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors disabled:opacity-40"
                            >
                              {isFetchingEdit && editId === m.id
                                ? <Loader2 className="w-3.5 h-3.5 animate-spin" />
                                : <Pencil className="w-3.5 h-3.5" />}
                            </button>
                            <button
                              title={m.status === 'Active' ? 'Deactivate' : 'Activate'}
                              onClick={() => handleToggleStatus(m)}
                              disabled={isMutating}
                              className={cn(
                                'p-1.5 rounded-lg transition-colors disabled:opacity-40',
                                m.status === 'Active'
                                  ? 'text-gray-400 hover:text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-900/20'
                                  : 'text-gray-400 hover:text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/20',
                              )}
                            >
                              {m.status === 'Active'
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
              {filtered.length} of {manufacturers.length} manufacturers
            </div>
          </>
        )}
      </div>

      {/* Create / Edit Modal */}
      <ManufacturerModal
        open={modalOpen && (!editId || !!editManufacturer)}
        manufacturer={editManufacturer ?? null}
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
          onConfirm={executeConfirm}
          onClose={() => setConfirm(null)}
        />
      )}
    </div>
  );
}
