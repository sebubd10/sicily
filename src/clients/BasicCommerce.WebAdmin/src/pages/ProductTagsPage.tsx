import { useState } from 'react';
import { Search, Plus, Pencil, Trash2, Power, PowerOff, Tag, Loader2, AlertCircle } from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { TagDetail, TagFormData } from '../types/tag';
import { TagModal } from '../components/tags/TagModal';
import { ConfirmDialog } from '../components/ui/ConfirmDialog';
import { Pagination } from '../components/ui/Pagination';
import {
  useTags,
  useTagDetail,
  useCreateTag,
  useUpdateTag,
  useDeleteTag,
  useActivateTag,
  useDeactivateTag,
} from '../hooks/useTags';

const PAGE_SIZE_OPTIONS = [10, 20, 50];

type ConfirmState =
  | { type: 'delete';     tag: TagDetail }
  | { type: 'activate';   tag: TagDetail }
  | { type: 'deactivate'; tag: TagDetail }
  | null;

export default function ProductTagsPage() {
  // ── Filter / pagination state ─────────────────────────────────────────────
  const [search, setSearch] = useState('');
  const [page, setPage]     = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [showInactive, setShowInactive] = useState(false);

  // ── Modal / confirm state ─────────────────────────────────────────────────
  const [modalOpen, setModalOpen] = useState(false);
  const [editId, setEditId]       = useState<string | null>(null);
  const [confirm, setConfirm]     = useState<ConfirmState>(null);
  const [confirmError, setConfirmError] = useState<string | null>(null);

  const { data: editTag, isFetching: isFetchingEdit } = useTagDetail(editId);

  // ── Queries ───────────────────────────────────────────────────────────────
  const { data, isLoading, isError, isFetching } = useTags({
    page,
    pageSize,
    search: search.trim() || undefined,
    includeInactive: showInactive,
  });

  // ── Mutations ─────────────────────────────────────────────────────────────
  const createMutation     = useCreateTag();
  const updateMutation     = useUpdateTag();
  const deleteMutation     = useDeleteTag();
  const activateMutation   = useActivateTag();
  const deactivateMutation = useDeactivateTag();

  const isMutating =
    createMutation.isPending ||
    updateMutation.isPending ||
    deleteMutation.isPending ||
    activateMutation.isPending ||
    deactivateMutation.isPending;

  // ── Handlers ──────────────────────────────────────────────────────────────
  function openAdd() {
    setEditId(null);
    setModalOpen(true);
  }

  function openEdit(tag: TagDetail) {
    setEditId(tag.id);
    setModalOpen(true);
  }

  async function handleSave(form: TagFormData) {
    if (editId) {
      await updateMutation.mutateAsync({ id: editId, form });
    } else {
      await createMutation.mutateAsync(form);
    }
    setModalOpen(false);
    setEditId(null);
  }

  function handleToggleStatus(tag: TagDetail) {
    setConfirm(
      tag.status === 'Active'
        ? { type: 'deactivate', tag }
        : { type: 'activate',   tag },
    );
  }

  function handleToggleInactive(value: boolean) {
    setShowInactive(value);
    setPage(1);
  }

  async function executeConfirm() {
    if (!confirm) return;
    try {
      setConfirmError(null);
      if (confirm.type === 'delete')     await deleteMutation.mutateAsync(confirm.tag.id);
      if (confirm.type === 'activate')   await activateMutation.mutateAsync(confirm.tag.id);
      if (confirm.type === 'deactivate') await deactivateMutation.mutateAsync(confirm.tag.id);
      setConfirm(null);
    } catch (err) {
      setConfirmError(extractApiError(err));
    }
  }

  const rows     = data?.items ?? [];
  const total    = data?.totalCount ?? 0;
  const totPages = data?.totalPages ?? 1;

  // ── Render ────────────────────────────────────────────────────────────────
  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <Tag className="w-6 h-6 text-primary-700" /> Product Tags
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Manage tags used to label and filter products
          </p>
        </div>
        <button
          onClick={openAdd}
          className="flex items-center gap-2 px-4 py-2 bg-primary-800 hover:bg-primary-900 text-white text-sm font-medium rounded-lg transition-colors shadow-sm"
        >
          <Plus className="w-4 h-4" /> Add Tag
        </button>
      </div>

      {/* Summary chips */}
      <div className="flex flex-wrap gap-3">
        {[
          {
            label: 'Total',
            value: total,
            color: 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300',
          },
          {
            label: 'Active',
            value: rows.filter((t) => t.status === 'Active').length,
            color: 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300',
          },
          {
            label: 'Inactive',
            value: rows.filter((t) => t.status !== 'Active').length,
            color: 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400',
          },
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
            placeholder="Search tags..."
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="w-full pl-9 pr-4 py-2 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-gray-900 dark:text-white placeholder-gray-400"
          />
        </div>

        <label className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 cursor-pointer select-none">
          <div
            onClick={() => handleToggleInactive(!showInactive)}
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
            <span className="text-sm">Failed to load tags. Please refresh and try again.</span>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                    {['#', 'Name', 'Tagged Products', 'Status', 'Actions'].map((h) => (
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
                    Array.from({ length: Math.min(pageSize, 5) }).map((_, i) => (
                      <tr key={i} className="animate-pulse">
                        {Array.from({ length: 5 }).map((_, j) => (
                          <td key={j} className="px-4 py-3">
                            <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-full" />
                          </td>
                        ))}
                      </tr>
                    ))
                  ) : rows.length === 0 ? (
                    <tr>
                      <td colSpan={5} className="px-4 py-12 text-center text-sm text-gray-400">
                        {search ? 'No tags match your search.' : 'No tags yet. Add one to get started.'}
                      </td>
                    </tr>
                  ) : (
                    rows.map((tag, idx) => (
                      <tr
                        key={tag.id}
                        className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
                      >
                        <td className="px-4 py-3 text-gray-400">
                          {(page - 1) * pageSize + idx + 1}
                        </td>
                        <td className="px-4 py-3 font-medium text-gray-900 dark:text-white">
                          <span className="inline-flex items-center gap-1.5">
                            <span className="px-2 py-0.5 rounded-full bg-primary-50 dark:bg-primary-900/20 border border-primary-200 dark:border-primary-700 text-primary-700 dark:text-primary-300 text-xs font-medium">
                              {tag.name}
                            </span>
                          </span>
                        </td>
                        <td className="px-4 py-3 text-gray-600 dark:text-gray-400">
                          {tag.taggedProductsCount > 0 ? (
                            <span className="inline-flex items-center gap-1">
                              <span className="font-medium text-gray-900 dark:text-white">
                                {tag.taggedProductsCount}
                              </span>
                              <span className="text-xs text-gray-400">
                                {tag.taggedProductsCount === 1 ? 'product' : 'products'}
                              </span>
                            </span>
                          ) : (
                            <span className="text-gray-300 dark:text-gray-600">—</span>
                          )}
                        </td>
                        <td className="px-4 py-3">
                          <span
                            className={cn(
                              'inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium',
                              tag.status === 'Active'
                                ? 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300'
                                : 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400',
                            )}
                          >
                            <span
                              className={cn(
                                'w-1.5 h-1.5 rounded-full mr-1.5',
                                tag.status === 'Active' ? 'bg-emerald-500' : 'bg-red-400',
                              )}
                            />
                            {tag.status}
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-1">
                            <button
                              title="Edit"
                              onClick={() => openEdit(tag)}
                              disabled={isMutating || isFetchingEdit}
                              className="p-1.5 rounded-lg text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors disabled:opacity-40"
                            >
                              {isFetchingEdit && editId === tag.id ? (
                                <Loader2 className="w-3.5 h-3.5 animate-spin" />
                              ) : (
                                <Pencil className="w-3.5 h-3.5" />
                              )}
                            </button>
                            <button
                              title={tag.status === 'Active' ? 'Deactivate' : 'Activate'}
                              onClick={() => handleToggleStatus(tag)}
                              disabled={isMutating}
                              className={cn(
                                'p-1.5 rounded-lg transition-colors disabled:opacity-40',
                                tag.status === 'Active'
                                  ? 'text-gray-400 hover:text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-900/20'
                                  : 'text-gray-400 hover:text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/20',
                              )}
                            >
                              {tag.status === 'Active'
                                ? <PowerOff className="w-3.5 h-3.5" />
                                : <Power    className="w-3.5 h-3.5" />}
                            </button>
                            <button
                              title="Delete"
                              onClick={() => setConfirm({ type: 'delete', tag })}
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
      <TagModal
        open={modalOpen && (!editId || !!editTag)}
        tag={editTag ?? null}
        onSave={handleSave}
        onClose={() => { setModalOpen(false); setEditId(null); }}
        isSaving={createMutation.isPending || updateMutation.isPending}
      />

      {/* Confirm Delete */}
      {confirm?.type === 'delete' && (
        <ConfirmDialog
          open
          title="Delete Tag"
          message={
            confirm.tag.taggedProductsCount > 0
              ? `"${confirm.tag.name}" is used by ${confirm.tag.taggedProductsCount} ${confirm.tag.taggedProductsCount === 1 ? 'product' : 'products'}. Deleting it will remove it from all of them. This cannot be undone.`
              : `"${confirm.tag.name}" will be permanently removed. This cannot be undone.`
          }
          confirmLabel="Delete"
          variant="danger"
          loading={deleteMutation.isPending}
          error={confirmError}
          onConfirm={executeConfirm}
          onClose={() => { setConfirm(null); setConfirmError(null); }}
        />
      )}

      {/* Confirm Deactivate */}
      {confirm?.type === 'deactivate' && (
        <ConfirmDialog
          open
          title="Deactivate Tag"
          message={`"${confirm.tag.name}" will be marked inactive and hidden from active tag lists.`}
          confirmLabel="Deactivate"
          variant="warning"
          loading={deactivateMutation.isPending}
          error={confirmError}
          onConfirm={executeConfirm}
          onClose={() => { setConfirm(null); setConfirmError(null); }}
        />
      )}

      {/* Confirm Activate */}
      {confirm?.type === 'activate' && (
        <ConfirmDialog
          open
          title="Activate Tag"
          message={`"${confirm.tag.name}" will be made active again.`}
          confirmLabel="Activate"
          variant="warning"
          loading={activateMutation.isPending}
          error={confirmError}
          onConfirm={executeConfirm}
          onClose={() => { setConfirm(null); setConfirmError(null); }}
        />
      )}
    </div>
  );
}
