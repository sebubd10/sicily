import { useState, useMemo } from 'react';
import {
  Search, Plus, Pencil, PowerOff, Power, Loader2,
  AlertCircle, Package, Download, Filter, Image as ImageIcon,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { Product, ProductListItem, ProductFormData } from '../types/product';
import { ProductModal } from '../components/products/ProductModal';
import { ConfirmDialog } from '../components/ui/ConfirmDialog';
import { Pagination } from '../components/ui/Pagination';
import { exportProductsPdf } from '../api/productsApi';
import {
  useProducts,
  useProductDetail,
  useCreateProduct,
  useUpdateProduct,
  useActivateProduct,
  useDeactivateProduct,
  useAllCategoriesFlat,
} from '../hooks/useProducts';

type ConfirmState =
  | { type: 'deactivate'; product: ProductListItem }
  | { type: 'activate';   product: ProductListItem }
  | null;

const PAGE_SIZE_OPTIONS = [10, 20, 50];

export default function ProductsPage() {
  const [search, setSearch]             = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [searchTimer, setSearchTimer]   = useState<ReturnType<typeof setTimeout> | null>(null);
  const [categoryId, setCategoryId]     = useState('');
  const [showInactive, setShowInactive] = useState(false);
  const [page, setPage]                 = useState(1);
  const [pageSize, setPageSize]         = useState(20);
  const [isExporting, setIsExporting]   = useState(false);

  const [modalOpen, setModalOpen] = useState(false);
  const [editId, setEditId]       = useState<string | null>(null);
  const [confirm, setConfirm]     = useState<ConfirmState>(null);
  const [confirmError, setConfirmError] = useState<string | null>(null);

  const params = useMemo(() => ({
    page,
    pageSize,
    search: debouncedSearch || undefined,
    categoryId: categoryId || undefined,
    includeInactive: showInactive,
  }), [page, pageSize, debouncedSearch, categoryId, showInactive]);

  const { data, isLoading, isError, isFetching } = useProducts(params);
  const { data: editProduct, isFetching: isFetchingEdit } = useProductDetail(editId);
  const { data: categories = [] } = useAllCategoriesFlat();

  const createMutation     = useCreateProduct();
  const updateMutation     = useUpdateProduct();
  const activateMutation   = useActivateProduct();
  const deactivateMutation = useDeactivateProduct();

  const isMutating =
    createMutation.isPending ||
    updateMutation.isPending ||
    activateMutation.isPending ||
    deactivateMutation.isPending;

  const products   = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;
  const totalPages = data?.totalPages ?? 1;

  function handleSearch(val: string) {
    setSearch(val);
    if (searchTimer) clearTimeout(searchTimer);
    const t = setTimeout(() => {
      setDebouncedSearch(val);
      setPage(1);
    }, 350);
    setSearchTimer(t);
  }

  function handleCategoryChange(val: string) {
    setCategoryId(val);
    setPage(1);
  }

  function handleToggleInactive(val: boolean) {
    setShowInactive(val);
    setPage(1);
  }

  function openAdd() { setEditId(null); setModalOpen(true); }
  function openEdit(p: ProductListItem) { setEditId(p.id); setModalOpen(true); }

  function handleToggleStatus(p: ProductListItem) {
    setConfirm(p.status === 'Active'
      ? { type: 'deactivate', product: p }
      : { type: 'activate',   product: p });
  }

  async function handleSave(form: ProductFormData) {
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
      if (confirm.type === 'deactivate') await deactivateMutation.mutateAsync(confirm.product.id);
      if (confirm.type === 'activate')   await activateMutation.mutateAsync(confirm.product.id);
      setConfirm(null);
    } catch (err) {
      setConfirmError(extractApiError(err));
    }
  }

  async function handleExportPdf() {
    setIsExporting(true);
    try {
      await exportProductsPdf({
        search: debouncedSearch.trim() || undefined,
        includeInactive: showInactive,
        categoryId: categoryId || undefined,
      });
    } finally {
      setIsExporting(false);
    }
  }

  const confirmProps = (() => {
    if (!confirm) return null;
    const name = confirm.product.name;
    if (confirm.type === 'deactivate') return {
      title: 'Deactivate Product',
      message: `"${name}" will be hidden from sales.`,
      confirmLabel: 'Deactivate',
      variant: 'warning' as const,
    };
    return {
      title: 'Activate Product',
      message: `"${name}" will be made available for sale again.`,
      confirmLabel: 'Activate',
      variant: 'warning' as const,
    };
  })();

  const activeCount   = (data?.items ?? []).filter((p) => p.status === 'Active').length;
  const inactiveCount = (data?.items ?? []).length - activeCount;

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <Package className="w-6 h-6 text-primary-700" /> Products
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Manage your product catalogue
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={handleExportPdf}
            disabled={isExporting}
            title="Export current view as PDF"
            className="flex items-center gap-2 px-4 py-2 border border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 text-sm font-medium rounded-lg transition-colors disabled:opacity-50"
          >
            {isExporting ? <Loader2 className="w-4 h-4 animate-spin" /> : <Download className="w-4 h-4" />}
            {isExporting ? 'Generating…' : 'Export PDF'}
          </button>
          <button
            onClick={openAdd}
            className="flex items-center gap-2 px-4 py-2 bg-primary-800 hover:bg-primary-900 text-white text-sm font-medium rounded-lg transition-colors shadow-sm"
          >
            <Plus className="w-4 h-4" /> Add Product
          </button>
        </div>
      </div>

      {/* Stats */}
      <div className="flex flex-wrap gap-3">
        {[
          { label: 'Total',    value: totalCount, color: 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300' },
          { label: 'Active',   value: activeCount, color: 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300' },
          { label: 'Inactive', value: inactiveCount, color: 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400' },
        ].map((s) => (
          <span key={s.label} className={cn('px-3 py-1 rounded-full text-sm font-medium', s.color)}>
            {s.label}: <strong>{s.value}</strong>
          </span>
        ))}
      </div>

      {/* Toolbar */}
      <div className="flex flex-col sm:flex-row gap-3 flex-wrap">
        <div className="relative flex-1 min-w-[200px] max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            placeholder="Search by name, SKU or barcode…"
            value={search}
            onChange={(e) => handleSearch(e.target.value)}
            className="w-full pl-9 pr-4 py-2 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-gray-900 dark:text-white placeholder-gray-400"
          />
        </div>

        <div className="relative flex items-center gap-1.5">
          <Filter className="w-4 h-4 text-gray-400" />
          <select
            value={categoryId}
            onChange={(e) => handleCategoryChange(e.target.value)}
            className="py-2 pl-2 pr-8 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-gray-700 dark:text-gray-300 appearance-none"
          >
            <option value="">All categories</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>
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
            <span className="text-sm">Failed to load products. Please refresh and try again.</span>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                    {['#', 'Image', 'SKU', 'Name', 'Category', 'Price', 'VAT', 'Manufacturer', 'Status', 'Actions'].map((h) => (
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
                    Array.from({ length: 8 }).map((_, i) => (
                      <tr key={i} className="animate-pulse">
                        {Array.from({ length: 10 }).map((_, j) => (
                          <td key={j} className="px-4 py-3">
                            <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-full" />
                          </td>
                        ))}
                      </tr>
                    ))
                  ) : products.length === 0 ? (
                    <tr>
                      <td colSpan={10} className="px-4 py-12 text-center text-sm text-gray-400">
                        No products found.{' '}
                        {(search || categoryId) && (
                          <button
                            onClick={() => { handleSearch(''); setCategoryId(''); }}
                            className="text-primary-700 hover:underline"
                          >
                            Clear filters
                          </button>
                        )}
                      </td>
                    </tr>
                  ) : (
                    products.map((p, idx) => (
                      <tr key={p.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                        <td className="px-4 py-3 text-gray-400 text-xs">{(page - 1) * pageSize + idx + 1}</td>
                        <td className="px-4 py-3">
                          {p.imageUrl ? (
                            <img
                              src={p.imageUrl}
                              alt={p.name}
                              className="w-9 h-9 object-cover rounded-md border border-gray-100 dark:border-gray-700"
                              onError={(e) => { (e.target as HTMLImageElement).style.display = 'none'; }}
                            />
                          ) : (
                            <div className="w-9 h-9 rounded-md border border-gray-100 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 flex items-center justify-center">
                              <ImageIcon className="w-4 h-4 text-gray-300 dark:text-gray-600" />
                            </div>
                          )}
                        </td>
                        <td className="px-4 py-3 font-mono text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">
                          {p.sku}
                        </td>
                        <td className="px-4 py-3">
                          <div className="font-medium text-gray-900 dark:text-white whitespace-nowrap max-w-[180px] truncate">
                            {p.name}
                          </div>
                          {p.nameBn && (
                            <div className="text-xs text-gray-400 truncate max-w-[180px]">{p.nameBn}</div>
                          )}
                        </td>
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                          {p.categoryName}
                        </td>
                        <td className="px-4 py-3 text-gray-700 dark:text-gray-300 whitespace-nowrap font-medium">
                          {p.currency} {p.price.toFixed(2)}
                        </td>
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap text-xs">
                          {p.vatRate > 0 ? `${p.vatRate}%` : '0%'}
                        </td>
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap max-w-[120px] truncate">
                          {p.manufacturerName ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                        </td>
                        <td className="px-4 py-3">
                          <span className={cn(
                            'inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium',
                            p.status === 'Active'
                              ? 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300'
                              : 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400',
                          )}>
                            <span className={cn(
                              'w-1.5 h-1.5 rounded-full mr-1.5',
                              p.status === 'Active' ? 'bg-emerald-500' : 'bg-red-400',
                            )} />
                            {p.status}
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-1">
                            <button
                              title="Edit"
                              onClick={() => openEdit(p)}
                              disabled={isMutating || isFetchingEdit}
                              className="p-1.5 rounded-lg text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors disabled:opacity-40"
                            >
                              {isFetchingEdit && editId === p.id
                                ? <Loader2 className="w-3.5 h-3.5 animate-spin" />
                                : <Pencil className="w-3.5 h-3.5" />}
                            </button>
                            <button
                              title={p.status === 'Active' ? 'Deactivate' : 'Activate'}
                              onClick={() => handleToggleStatus(p)}
                              disabled={isMutating}
                              className={cn(
                                'p-1.5 rounded-lg transition-colors disabled:opacity-40',
                                p.status === 'Active'
                                  ? 'text-gray-400 hover:text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-900/20'
                                  : 'text-gray-400 hover:text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/20',
                              )}
                            >
                              {p.status === 'Active'
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

            <Pagination
              page={page}
              totalPages={totalPages}
              totalItems={totalCount}
              pageSize={pageSize}
              pageSizeOptions={PAGE_SIZE_OPTIONS}
              onPageChange={setPage}
              onPageSizeChange={(s) => { setPageSize(s); setPage(1); }}
            />
          </>
        )}
      </div>

      {/* Create / Edit Modal */}
      <ProductModal
        open={modalOpen && (!editId || !!editProduct)}
        product={editProduct ?? null}
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
