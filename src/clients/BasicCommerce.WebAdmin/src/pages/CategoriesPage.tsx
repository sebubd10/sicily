import { useState, useMemo } from 'react';
import { Search, Plus, Pencil, PowerOff, Layers, Trash2, Power, Loader2, AlertCircle, Download } from 'lucide-react';
import { exportCategoriesPdf } from '../api/categoriesApi';
import { cn } from '../lib/utils';
import type { Category, CategoryFormData } from '../types/category';
import { CategoryModal } from '../components/categories/CategoryModal';
import { ConfirmDialog } from '../components/ui/ConfirmDialog';
import { Pagination } from '../components/ui/Pagination';
import {
  useCategories,
  useAllActiveCategories,
  useCategoryDetail,
  useCreateCategory,
  useUpdateCategory,
  useActivateCategory,
  useDeactivateCategory,
  useDeleteCategory,
} from '../hooks/useCategories';

const PAGE_SIZE_OPTIONS = [10, 20, 50];

type ConfirmState =
  | { type: 'delete';     category: Category }
  | { type: 'deactivate'; category: Category }
  | { type: 'activate';   category: Category }
  | null;

export default function CategoriesPage() {
  // ── Filter / pagination state (drives the server query) ───────────────────────
  const [search, setSearch]             = useState('');
  const [showInactive, setShowInactive] = useState(false);
  const [page, setPage]                 = useState(1);
  const [pageSize, setPageSize]         = useState(10);

  // ── Modal / confirm state ─────────────────────────────────────────────────────
  const [modalOpen, setModalOpen] = useState(false);
  const [editId, setEditId]       = useState<string | null>(null);  // null = add mode
  const [confirm, setConfirm]     = useState<ConfirmState>(null);

  // Fetch full detail when an edit ID is set; opens the modal once data arrives
  const { data: editCategory, isFetching: isFetchingEdit } = useCategoryDetail(editId);

  // ── Server queries ────────────────────────────────────────────────────────────
  const { data, isLoading, isError, isFetching } = useCategories({
    page,
    pageSize,
    search: search.trim() || undefined,
    includeInactive: showInactive,
  });

  // All active categories — used for the parent dropdown + stats
  const { data: allCategories = [] } = useAllActiveCategories();

  // ── Mutations ─────────────────────────────────────────────────────────────────
  const createMutation     = useCreateCategory();
  const updateMutation     = useUpdateCategory();
  const activateMutation   = useActivateCategory();
  const deactivateMutation = useDeactivateCategory();
  const deleteMutation     = useDeleteCategory();

  const isMutating =
    createMutation.isPending ||
    updateMutation.isPending ||
    activateMutation.isPending ||
    deactivateMutation.isPending ||
    deleteMutation.isPending;

  // ── Stats computed from the all-active list ───────────────────────────────────
  const stats = useMemo(() => ({
    total:  data?.totalCount ?? 0,
    active: allCategories.filter((c) => c.status === 'Active').length,
    root:   allCategories.filter((c) => !c.parentCategoryId).length,
    sub:    allCategories.filter((c) => !!c.parentCategoryId).length,
  }), [data?.totalCount, allCategories]);

  // ── Handlers ──────────────────────────────────────────────────────────────────
  function openAdd() {
    setEditId(null);
    setModalOpen(true);
  }

  function openEdit(id: string) {
    setEditId(id);
    setModalOpen(true);
  }

  function handleToggleStatus(cat: Category) {
    setConfirm(cat.status === 'Active'
      ? { type: 'deactivate', category: cat }
      : { type: 'activate',   category: cat });
  }

  function handleDelete(cat: Category) {
    setConfirm({ type: 'delete', category: cat });
  }

  async function handleSave(form: CategoryFormData) {
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
    if (confirm.type === 'delete')     await deleteMutation.mutateAsync(confirm.category.id);
    if (confirm.type === 'deactivate') await deactivateMutation.mutateAsync(confirm.category.id);
    if (confirm.type === 'activate')   await activateMutation.mutateAsync(confirm.category.id);
    setConfirm(null);
  }

  // ── Confirm dialog props ──────────────────────────────────────────────────────
  const confirmProps = (() => {
    if (!confirm) return null;
    const name = confirm.category.name;
    if (confirm.type === 'delete') return {
      title: 'Delete Category',
      message: `"${name}" will be permanently removed. This cannot be undone.`,
      confirmLabel: 'Delete',
      variant: 'danger' as const,
    };
    if (confirm.type === 'deactivate') return {
      title: 'Deactivate Category',
      message: `"${name}" will be hidden from product listings and the POS.`,
      confirmLabel: 'Deactivate',
      variant: 'warning' as const,
    };
    return {
      title: 'Activate Category',
      message: `"${name}" will be made visible again in product listings and the POS.`,
      confirmLabel: 'Activate',
      variant: 'warning' as const,
    };
  })();

  // ── PDF export ───────────────────────────────────────────────────────────────
  const [isExporting, setIsExporting] = useState(false);

  async function handleExportPdf() {
    setIsExporting(true);
    try {
      await exportCategoriesPdf({
        search: search.trim() || undefined,
        includeInactive: showInactive,
      });
    } finally {
      setIsExporting(false);
    }
  }

  const rows     = data?.items ?? [];
  const total    = data?.totalCount ?? 0;
  const totPages = data?.totalPages ?? 1;

  // ── Render ────────────────────────────────────────────────────────────────────
  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <Layers className="w-6 h-6 text-primary-700" /> Categories
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Manage product category hierarchy
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={handleExportPdf}
            disabled={isExporting}
            title={search || showInactive ? 'Export current filter as PDF' : 'Export all categories as PDF'}
            className="flex items-center gap-2 px-4 py-2 border border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 text-sm font-medium rounded-lg transition-colors disabled:opacity-50"
          >
            {isExporting
              ? <Loader2 className="w-4 h-4 animate-spin" />
              : <Download className="w-4 h-4" />}
            {isExporting ? 'Generating…' : 'Export PDF'}
          </button>
          <button
            onClick={openAdd}
            className="flex items-center gap-2 px-4 py-2 bg-primary-800 hover:bg-primary-900 text-white text-sm font-medium rounded-lg transition-colors shadow-sm"
          >
            <Plus className="w-4 h-4" /> Add Category
          </button>
        </div>
      </div>

      {/* Summary chips */}
      <div className="flex flex-wrap gap-3">
        {[
          { label: 'Total',          value: stats.total,  color: 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300' },
          { label: 'Active',         value: stats.active, color: 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300' },
          { label: 'Root',           value: stats.root,   color: 'bg-primary-100 dark:bg-primary-900/30 text-primary-700 dark:text-primary-300' },
          { label: 'Sub-categories', value: stats.sub,    color: 'bg-violet-100 dark:bg-violet-900/30 text-violet-700 dark:text-violet-300' },
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
            placeholder="Search categories..."
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="w-full pl-9 pr-4 py-2 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-gray-900 dark:text-white placeholder-gray-400"
          />
        </div>
        <label className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 cursor-pointer select-none">
          <div
            onClick={() => { setShowInactive((v) => !v); setPage(1); }}
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
            <span className="text-sm">Failed to load categories. Please refresh and try again.</span>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                    {['#', 'Name (English)', 'Name (Bengali)', 'Parent Category', 'Sub-cats', 'Sort', 'Status', 'Actions'].map((h) => (
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
                    Array.from({ length: pageSize > 5 ? 5 : pageSize }).map((_, i) => (
                      <tr key={i} className="animate-pulse">
                        {Array.from({ length: 8 }).map((_, j) => (
                          <td key={j} className="px-4 py-3">
                            <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-full" />
                          </td>
                        ))}
                      </tr>
                    ))
                  ) : rows.length === 0 ? (
                    <tr>
                      <td colSpan={8} className="px-4 py-12 text-center text-sm text-gray-400">
                        No categories found.
                      </td>
                    </tr>
                  ) : (
                    rows.map((cat, idx) => (
                      <tr key={cat.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                        <td className="px-4 py-3 text-gray-400">
                          {(page - 1) * pageSize + idx + 1}
                        </td>
                        <td className="px-4 py-3 font-medium text-gray-900 dark:text-white whitespace-nowrap">
                          {cat.parentCategoryId && (
                            <span className="text-gray-300 dark:text-gray-600 mr-1">└</span>
                          )}
                          {cat.name}
                        </td>
                        <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{cat.nameBn}</td>
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                          {cat.parentCategoryName
                            ? <span className="px-2 py-0.5 bg-gray-100 dark:bg-gray-800 rounded text-xs">{cat.parentCategoryName}</span>
                            : <span className="text-gray-300 dark:text-gray-600">—</span>}
                        </td>
                        <td className="px-4 py-3 text-center text-gray-600 dark:text-gray-400">{cat.childCount}</td>
                        <td className="px-4 py-3 text-center text-gray-600 dark:text-gray-400">{cat.sortOrder}</td>
                        <td className="px-4 py-3">
                          <span className={cn(
                            'inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium',
                            cat.status === 'Active'
                              ? 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300'
                              : 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400',
                          )}>
                            <span className={cn(
                              'w-1.5 h-1.5 rounded-full mr-1.5',
                              cat.status === 'Active' ? 'bg-emerald-500' : 'bg-red-400',
                            )} />
                            {cat.status}
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-1">
                            <button
                              title="Edit"
                              onClick={() => openEdit(cat.id)}
                              disabled={isMutating || isFetchingEdit}
                              className="p-1.5 rounded-lg text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors disabled:opacity-40"
                            >
                              {isFetchingEdit && editId === cat.id
                                ? <Loader2 className="w-3.5 h-3.5 animate-spin" />
                                : <Pencil className="w-3.5 h-3.5" />}
                            </button>
                            <button
                              title={cat.status === 'Active' ? 'Deactivate' : 'Activate'}
                              onClick={() => handleToggleStatus(cat)}
                              disabled={isMutating}
                              className={cn(
                                'p-1.5 rounded-lg transition-colors disabled:opacity-40',
                                cat.status === 'Active'
                                  ? 'text-gray-400 hover:text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-900/20'
                                  : 'text-gray-400 hover:text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/20',
                              )}
                            >
                              {cat.status === 'Active'
                                ? <PowerOff className="w-3.5 h-3.5" />
                                : <Power    className="w-3.5 h-3.5" />}
                            </button>
                            <button
                              title="Delete"
                              onClick={() => handleDelete(cat)}
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

            <Pagination
              page={page}
              totalPages={totPages}
              totalItems={total}
              pageSize={pageSize}
              pageSizeOptions={PAGE_SIZE_OPTIONS}
              onPageChange={setPage}
              onPageSizeChange={(s) => { setPageSize(s); setPage(1); }}
            />
          </>
        )}
      </div>

      {/* Add / Edit Modal */}
      <CategoryModal
        open={modalOpen && (!editId || !!editCategory)}
        category={editCategory ?? null}
        allCategories={allCategories}
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
