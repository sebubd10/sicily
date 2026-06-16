import { useState, useEffect, useRef } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { ShoppingCart, X, Search, Plus, Trash2, Package } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { CreatePurchaseOrderFormData, CreatePOLineItem } from '../../types/purchaseOrder';
import { useWarehouses } from '../../hooks/useWarehouses';
import { useSuppliers } from '../../hooks/useSuppliers';
import { getProducts } from '../../api/productsApi';
import type { ProductListItem } from '../../types/product';

type Props = {
  open: boolean;
  onSave: (data: CreatePurchaseOrderFormData) => void;
  onClose: () => void;
  isSaving?: boolean;
};

const today = () => new Date().toISOString().slice(0, 10);

const EMPTY: CreatePurchaseOrderFormData = {
  supplierId: '',
  warehouseId: '',
  orderDate: today(),
  expectedDate: '',
  notes: '',
  currency: 'BDT',
  items: [],
};

export function CreatePurchaseOrderModal({ open, onSave, onClose, isSaving }: Props) {
  const [form, setForm] = useState<CreatePurchaseOrderFormData>(EMPTY);
  const [errors, setErrors] = useState<Partial<Record<string, string>>>({});
  const [productSearch, setProductSearch] = useState('');
  const [searchResults, setSearchResults] = useState<ProductListItem[]>([]);
  const [searching, setSearching] = useState(false);
  const [showResults, setShowResults] = useState(false);
  const searchRef = useRef<HTMLDivElement>(null);
  const searchTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const { data: suppliers = [] } = useSuppliers();
  const { data: warehousesData } = useWarehouses();
  const warehouses = warehousesData?.items ?? [];

  useEffect(() => {
    if (open) { setForm(EMPTY); setErrors({}); setProductSearch(''); setSearchResults([]); }
  }, [open]);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (searchRef.current && !searchRef.current.contains(e.target as Node))
        setShowResults(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  useEffect(() => {
    if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    if (!productSearch.trim()) { setSearchResults([]); return; }
    searchTimerRef.current = setTimeout(async () => {
      setSearching(true);
      try {
        const res = await getProducts({ page: 1, pageSize: 10, search: productSearch });
        setSearchResults(res.items ?? []);
        setShowResults(true);
      } finally {
        setSearching(false);
      }
    }, 300);
  }, [productSearch]);

  function set<K extends keyof CreatePurchaseOrderFormData>(
    key: K, value: CreatePurchaseOrderFormData[K],
  ) {
    setForm((f) => ({ ...f, [key]: value }));
    setErrors((e) => ({ ...e, [key]: undefined }));
  }

  function addProduct(p: ProductListItem) {
    if (form.items.some((i) => i.productId === p.id)) {
      setProductSearch('');
      setShowResults(false);
      return;
    }
    const newItem: CreatePOLineItem = {
      productId: p.id,
      productName: p.name,
      sku: p.sku,
      quantity: 1,
      unitCost: p.costPrice ?? 0,
    };
    setForm((f) => ({ ...f, items: [...f.items, newItem] }));
    setProductSearch('');
    setShowResults(false);
  }

  function updateItem(idx: number, field: 'quantity' | 'unitCost', value: number) {
    setForm((f) => ({
      ...f,
      items: f.items.map((item, i) => i === idx ? { ...item, [field]: value } : item),
    }));
  }

  function removeItem(idx: number) {
    setForm((f) => ({ ...f, items: f.items.filter((_, i) => i !== idx) }));
  }

  function validate() {
    const e: Record<string, string> = {};
    if (!form.supplierId) e.supplierId = 'Supplier is required.';
    if (!form.warehouseId) e.warehouseId = 'Warehouse is required.';
    if (!form.orderDate) e.orderDate = 'Order date is required.';
    if (form.items.length === 0) e.items = 'Add at least one product.';
    form.items.forEach((item, i) => {
      if (item.quantity <= 0) e[`qty_${i}`] = 'Qty must be > 0';
      if (item.unitCost < 0) e[`cost_${i}`] = 'Cost cannot be negative';
    });
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (validate()) onSave(form);
  }

  const inputCls = (err?: string) =>
    cn(
      'w-full rounded-lg border px-3 py-2 text-sm bg-white dark:bg-gray-900',
      'text-gray-900 dark:text-white placeholder-gray-400',
      'focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 transition',
      err ? 'border-red-400 dark:border-red-500' : 'border-gray-200 dark:border-gray-700',
    );

  const total = form.items.reduce((s, i) => s + i.quantity * i.unitCost, 0);

  return (
    <Dialog.Root open={open} onOpenChange={(o) => !o && !isSaving && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <Dialog.Content
          className={cn(
            'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
            'w-full max-w-3xl rounded-xl bg-white dark:bg-gray-900',
            'border border-gray-200 dark:border-gray-700 shadow-2xl focus:outline-none',
            'data-[state=open]:animate-in data-[state=closed]:animate-out',
            'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
            'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
          )}
        >
          {/* Header */}
          <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 px-6 py-4">
            <Dialog.Title className="flex items-center gap-2 text-base font-semibold text-gray-900 dark:text-white">
              <ShoppingCart className="w-5 h-5 text-primary-700" />
              New Purchase Order
            </Dialog.Title>
            <Dialog.Description className="sr-only">Create a new purchase order</Dialog.Description>
            <Dialog.Close
              disabled={isSaving}
              className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none disabled:opacity-40"
            >
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          <form onSubmit={handleSubmit} noValidate>
            <div className="max-h-[75vh] overflow-y-auto">
              {/* Order Header */}
              <div className="px-6 py-5 space-y-4 border-b border-gray-100 dark:border-gray-800">
                <p className="text-xs font-semibold uppercase tracking-wider text-gray-400">Order Details</p>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Supplier <span className="text-red-500">*</span>
                    </label>
                    <select
                      value={form.supplierId}
                      onChange={(e) => set('supplierId', e.target.value)}
                      disabled={isSaving}
                      className={inputCls(errors.supplierId)}
                    >
                      <option value="">Select supplier…</option>
                      {suppliers.filter(s => s.status === 'Active').map((s) => (
                        <option key={s.id} value={s.id}>{s.name}</option>
                      ))}
                    </select>
                    {errors.supplierId && <p className="mt-1 text-xs text-red-500">{errors.supplierId}</p>}
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Destination Warehouse <span className="text-red-500">*</span>
                    </label>
                    <select
                      value={form.warehouseId}
                      onChange={(e) => set('warehouseId', e.target.value)}
                      disabled={isSaving}
                      className={inputCls(errors.warehouseId)}
                    >
                      <option value="">Select warehouse…</option>
                      {warehouses.filter((w: any) => w.status === 'Active').map((w: any) => (
                        <option key={w.id} value={w.id}>{w.name}</option>
                      ))}
                    </select>
                    {errors.warehouseId && <p className="mt-1 text-xs text-red-500">{errors.warehouseId}</p>}
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Order Date <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="date"
                      value={form.orderDate}
                      onChange={(e) => set('orderDate', e.target.value)}
                      disabled={isSaving}
                      className={inputCls(errors.orderDate)}
                    />
                    {errors.orderDate && <p className="mt-1 text-xs text-red-500">{errors.orderDate}</p>}
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Expected Delivery Date
                    </label>
                    <input
                      type="date"
                      value={form.expectedDate}
                      onChange={(e) => set('expectedDate', e.target.value)}
                      disabled={isSaving}
                      className={inputCls()}
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Currency
                    </label>
                    <select
                      value={form.currency}
                      onChange={(e) => set('currency', e.target.value)}
                      disabled={isSaving}
                      className={inputCls()}
                    >
                      <option value="BDT">BDT – Bangladeshi Taka</option>
                      <option value="USD">USD – US Dollar</option>
                      <option value="EUR">EUR – Euro</option>
                      <option value="GBP">GBP – British Pound</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Notes
                    </label>
                    <input
                      type="text"
                      value={form.notes}
                      onChange={(e) => set('notes', e.target.value)}
                      placeholder="Optional notes…"
                      disabled={isSaving}
                      className={inputCls()}
                    />
                  </div>
                </div>
              </div>

              {/* Line Items */}
              <div className="px-6 py-5 space-y-4">
                <p className="text-xs font-semibold uppercase tracking-wider text-gray-400">Line Items</p>

                {/* Product search */}
                <div ref={searchRef} className="relative">
                  <div className="relative">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                    <input
                      type="text"
                      placeholder="Search and add products…"
                      value={productSearch}
                      onChange={(e) => { setProductSearch(e.target.value); setShowResults(true); }}
                      onFocus={() => searchResults.length > 0 && setShowResults(true)}
                      className="w-full pl-9 pr-4 py-2 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-gray-900 dark:text-white placeholder-gray-400"
                    />
                    {searching && (
                      <div className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 border-2 border-primary-500 border-t-transparent rounded-full animate-spin" />
                    )}
                  </div>
                  {showResults && searchResults.length > 0 && (
                    <div className="absolute z-10 w-full mt-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg shadow-lg max-h-52 overflow-y-auto">
                      {searchResults.map((p) => {
                        const alreadyAdded = form.items.some((i) => i.productId === p.id);
                        return (
                          <button
                            key={p.id}
                            type="button"
                            onClick={() => addProduct(p)}
                            disabled={alreadyAdded}
                            className={cn(
                              'w-full flex items-center justify-between px-4 py-2.5 text-left text-sm transition-colors',
                              alreadyAdded
                                ? 'opacity-40 cursor-not-allowed bg-gray-50 dark:bg-gray-900'
                                : 'hover:bg-gray-50 dark:hover:bg-gray-700',
                            )}
                          >
                            <div>
                              <p className="font-medium text-gray-900 dark:text-white">{p.name}</p>
                              <p className="text-xs text-gray-500 dark:text-gray-400">SKU: {p.sku}</p>
                            </div>
                            <div className="flex items-center gap-2">
                              {p.costPrice != null && (
                                <span className="text-xs text-gray-400">{form.currency} {p.costPrice.toFixed(2)}</span>
                              )}
                              {alreadyAdded
                                ? <span className="text-xs text-primary-600">Added</span>
                                : <Plus className="w-4 h-4 text-primary-600" />}
                            </div>
                          </button>
                        );
                      })}
                    </div>
                  )}
                </div>
                {errors.items && <p className="text-xs text-red-500">{errors.items}</p>}

                {/* Items table */}
                {form.items.length > 0 ? (
                  <div className="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                          {['Product', 'SKU', 'Qty', `Unit Cost (${form.currency})`, 'Total', ''].map((h) => (
                            <th key={h} className="px-3 py-2 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                              {h}
                            </th>
                          ))}
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                        {form.items.map((item, idx) => (
                          <tr key={item.productId} className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
                            <td className="px-3 py-2">
                              <div className="flex items-center gap-2">
                                <Package className="w-3.5 h-3.5 text-gray-400 flex-shrink-0" />
                                <span className="font-medium text-gray-900 dark:text-white">{item.productName}</span>
                              </div>
                            </td>
                            <td className="px-3 py-2 font-mono text-xs text-gray-500 dark:text-gray-400">{item.sku}</td>
                            <td className="px-3 py-2 w-24">
                              <input
                                type="number"
                                min={0.01}
                                step={0.01}
                                value={item.quantity}
                                onChange={(e) => updateItem(idx, 'quantity', parseFloat(e.target.value) || 0)}
                                className={cn(
                                  'w-full rounded border px-2 py-1 text-sm text-center',
                                  'bg-white dark:bg-gray-900 text-gray-900 dark:text-white',
                                  'focus:outline-none focus:ring-1 focus:ring-primary-500',
                                  errors[`qty_${idx}`]
                                    ? 'border-red-400'
                                    : 'border-gray-200 dark:border-gray-700',
                                )}
                              />
                            </td>
                            <td className="px-3 py-2 w-32">
                              <input
                                type="number"
                                min={0}
                                step={0.01}
                                value={item.unitCost}
                                onChange={(e) => updateItem(idx, 'unitCost', parseFloat(e.target.value) || 0)}
                                className={cn(
                                  'w-full rounded border px-2 py-1 text-sm text-right',
                                  'bg-white dark:bg-gray-900 text-gray-900 dark:text-white',
                                  'focus:outline-none focus:ring-1 focus:ring-primary-500',
                                  errors[`cost_${idx}`]
                                    ? 'border-red-400'
                                    : 'border-gray-200 dark:border-gray-700',
                                )}
                              />
                            </td>
                            <td className="px-3 py-2 text-right text-gray-700 dark:text-gray-300 font-medium whitespace-nowrap">
                              {(item.quantity * item.unitCost).toFixed(2)}
                            </td>
                            <td className="px-3 py-2">
                              <button
                                type="button"
                                onClick={() => removeItem(idx)}
                                className="p-1 rounded text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
                              >
                                <Trash2 className="w-3.5 h-3.5" />
                              </button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                      <tfoot>
                        <tr className="bg-gray-50 dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700">
                          <td colSpan={4} className="px-3 py-2 text-sm font-semibold text-gray-700 dark:text-gray-300 text-right">
                            Total ({form.items.length} item{form.items.length !== 1 ? 's' : ''})
                          </td>
                          <td className="px-3 py-2 text-right font-bold text-gray-900 dark:text-white whitespace-nowrap">
                            {form.currency} {total.toFixed(2)}
                          </td>
                          <td />
                        </tr>
                      </tfoot>
                    </table>
                  </div>
                ) : (
                  <div className="flex flex-col items-center justify-center py-10 border-2 border-dashed border-gray-200 dark:border-gray-700 rounded-lg text-gray-400">
                    <Package className="w-8 h-8 mb-2 opacity-40" />
                    <p className="text-sm">Search and add products above</p>
                  </div>
                )}
              </div>
            </div>

            {/* Footer */}
            <div className="flex items-center justify-between px-6 py-4 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50 rounded-b-xl">
              <p className="text-sm text-gray-500 dark:text-gray-400">
                Order will be created as <span className="font-medium text-gray-700 dark:text-gray-300">Draft</span>
              </p>
              <div className="flex gap-3">
                <button
                  type="button"
                  onClick={onClose}
                  disabled={isSaving}
                  className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors disabled:opacity-40"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={isSaving}
                  className="px-5 py-2 text-sm font-medium text-white bg-primary-800 hover:bg-primary-900 rounded-lg transition-colors disabled:opacity-50 flex items-center gap-2"
                >
                  {isSaving && (
                    <svg className="w-4 h-4 animate-spin" viewBox="0 0 24 24" fill="none">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
                    </svg>
                  )}
                  {isSaving ? 'Creating…' : 'Create Purchase Order'}
                </button>
              </div>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
