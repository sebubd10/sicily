import { useEffect, useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { Package, X, ChevronRight } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { Product, ProductFormData } from '../../types/product';
import { UNIT_TYPES } from '../../types/product';
import { useVatRates, useAllCategoriesFlat, useAllManufacturers, useAllTags } from '../../hooks/useProducts';
import { ProductImageManager } from './ProductImageManager';

type Props = {
  open: boolean;
  product: Product | null;
  onSave: (data: ProductFormData) => void;
  onClose: () => void;
  isSaving?: boolean;
};

type Tab = 'basic' | 'pricing' | 'settings' | 'images';

const EMPTY: ProductFormData = {
  sku: '', barcode: '', plu: '', name: '', nameBn: '', description: '',
  categoryId: '', manufacturerId: '', price: '', costPrice: '',
  vatRateId: '', unitType: 'Each', unitLabel: '',
  isWeightBased: false, isPerishable: false, isAgeRestricted: false,
  ageRestrictionYears: '', isEbtEligible: false, trackInventory: true,
  reorderLevel: '0', imageUrl: '', tagIds: [],
};

export function ProductModal({ open, product, onSave, onClose, isSaving }: Props) {
  const isEdit = product !== null;
  const [tab, setTab] = useState<Tab>('basic');
  const [form, setForm] = useState<ProductFormData>(EMPTY);
  const [errors, setErrors] = useState<Partial<Record<keyof ProductFormData, string>>>({});

  const { data: vatRates = [] } = useVatRates();
  const { data: categories = [] } = useAllCategoriesFlat();
  const { data: manufacturers = [] } = useAllManufacturers();
  const { data: allTags = [] } = useAllTags();

  useEffect(() => {
    if (open) {
      setErrors({});
      setTab('basic');
      if (isEdit && product) {
        setForm({
          sku: product.sku,
          barcode: product.barcode,
          plu: product.plu ?? '',
          name: product.name,
          nameBn: product.nameBn,
          description: product.description ?? '',
          categoryId: product.categoryId,
          manufacturerId: product.manufacturerId ?? '',
          price: String(product.price),
          costPrice: product.costPrice != null ? String(product.costPrice) : '',
          vatRateId: product.vatRateId,
          unitType: product.unitType,
          unitLabel: product.unitLabel ?? '',
          isWeightBased: product.isWeightBased,
          isPerishable: product.isPerishable,
          isAgeRestricted: product.isAgeRestricted,
          ageRestrictionYears: product.ageRestrictionYears != null ? String(product.ageRestrictionYears) : '',
          isEbtEligible: product.isEbtEligible,
          trackInventory: product.trackInventory,
          reorderLevel: String(product.reorderLevel),
          imageUrl: product.imageUrl ?? '',
          tagIds: product.tags.map((t) => t.id),
        });
      } else {
        const defaultVat = vatRates.find((v) => v.isDefault);
        setForm({ ...EMPTY, vatRateId: defaultVat?.id ?? '' });
      }
    }
  }, [open, product]);

  function set<K extends keyof ProductFormData>(key: K, value: ProductFormData[K]) {
    setForm((f) => ({ ...f, [key]: value }));
    setErrors((e) => ({ ...e, [key]: undefined }));
  }

  function validate(): boolean {
    const e: Partial<Record<keyof ProductFormData, string>> = {};
    if (!form.name.trim()) e.name = 'Name is required.';
    if (!form.nameBn.trim()) e.nameBn = 'Bengali name is required.';
    if (!isEdit && !form.sku.trim()) e.sku = 'SKU is required.';
    if (!isEdit && !form.barcode.trim()) e.barcode = 'Barcode is required.';
    if (!form.categoryId) e.categoryId = 'Category is required.';
    if (!form.vatRateId) e.vatRateId = 'VAT rate is required.';
    const price = parseFloat(form.price);
    if (isNaN(price) || price <= 0) e.price = 'Price must be greater than 0.';
    if (form.costPrice && isNaN(parseFloat(form.costPrice))) e.costPrice = 'Enter a valid cost price.';
    if (form.isAgeRestricted) {
      const age = parseInt(form.ageRestrictionYears);
      if (isNaN(age) || age < 0 || age > 25)
        e.ageRestrictionYears = 'Enter age between 0 and 25.';
    }
    setErrors(e);
    if (Object.keys(e).length > 0) {
      if (e.name || e.nameBn || e.sku || e.barcode || e.categoryId || e.manufacturerId) setTab('basic');
      else if (e.price || e.costPrice || e.vatRateId) setTab('pricing');
      else if (e.ageRestrictionYears) setTab('settings');
    }
    return Object.keys(e).length === 0;
  }

  function handleSubmit(ev: React.FormEvent) {
    ev.preventDefault();
    if (validate()) onSave(form);
  }

  function toggleTag(id: string) {
    set('tagIds', form.tagIds.includes(id)
      ? form.tagIds.filter((t) => t !== id)
      : [...form.tagIds, id]);
  }

  const inputCls = (err?: string) => cn(
    'w-full rounded-lg border px-3 py-2 text-sm bg-white dark:bg-gray-900',
    'text-gray-900 dark:text-white placeholder-gray-400',
    'focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 transition',
    err ? 'border-red-400 dark:border-red-500' : 'border-gray-200 dark:border-gray-700',
  );

  const tabs: { id: Tab; label: string }[] = [
    { id: 'basic',    label: 'Basic Info' },
    { id: 'pricing',  label: 'Pricing' },
    { id: 'settings', label: 'Details & Flags' },
    ...(isEdit ? [{ id: 'images' as Tab, label: 'Images' }] : []),
  ];

  const hasError = (t: Tab) => {
    if (t === 'basic')    return !!(errors.name || errors.nameBn || errors.sku || errors.barcode || errors.categoryId);
    if (t === 'pricing')  return !!(errors.price || errors.costPrice || errors.vatRateId);
    if (t === 'settings') return !!errors.ageRestrictionYears;
    return false;
  };

  return (
    <Dialog.Root open={open} onOpenChange={(o) => !o && !isSaving && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <Dialog.Content
          className={cn(
            'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
            'w-full max-w-2xl rounded-xl bg-white dark:bg-gray-900',
            'border border-gray-200 dark:border-gray-700 shadow-xl focus:outline-none',
            'data-[state=open]:animate-in data-[state=closed]:animate-out',
            'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
            'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
            'data-[state=closed]:slide-out-to-left-1/2 data-[state=closed]:slide-out-to-top-48%',
            'data-[state=open]:slide-in-from-left-1/2 data-[state=open]:slide-in-from-top-48%',
          )}
        >
          {/* Header */}
          <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 px-6 py-4">
            <Dialog.Title className="flex items-center gap-2 text-base font-semibold text-gray-900 dark:text-white">
              <Package className="w-5 h-5 text-primary-700" />
              {isEdit ? `Edit Product — ${product?.sku}` : 'Add New Product'}
            </Dialog.Title>
            <Dialog.Close disabled={isSaving} className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none disabled:opacity-40">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          {/* Tabs */}
          <div className="flex border-b border-gray-200 dark:border-gray-700 px-6 bg-gray-50 dark:bg-gray-800/50">
            {tabs.map((t) => (
              <button
                key={t.id}
                type="button"
                onClick={() => setTab(t.id)}
                className={cn(
                  'flex items-center gap-1.5 px-3 py-3 text-sm font-medium border-b-2 transition-colors -mb-px',
                  tab === t.id
                    ? 'border-primary-700 text-primary-700 dark:text-primary-400'
                    : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200',
                  hasError(t.id) && 'text-red-500 dark:text-red-400',
                )}
              >
                {t.label}
                {hasError(t.id) && <span className="w-1.5 h-1.5 rounded-full bg-red-500 flex-shrink-0" />}
              </button>
            ))}
          </div>

          <form onSubmit={handleSubmit} noValidate>
            <div className="px-6 py-5 max-h-[58vh] overflow-y-auto">

              {/* ── Tab: Basic Info ─────────────────────────────────────────── */}
              {tab === 'basic' && (
                <div className="space-y-4">
                  {/* SKU + Barcode + PLU */}
                  <div className="grid grid-cols-3 gap-3">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                        SKU {!isEdit && <span className="text-red-500">*</span>}
                      </label>
                      <input
                        type="text"
                        value={form.sku}
                        onChange={(e) => set('sku', e.target.value.toUpperCase())}
                        placeholder="SKU-001"
                        disabled={isSaving || isEdit}
                        className={cn(inputCls(errors.sku), isEdit && 'opacity-60 cursor-not-allowed')}
                      />
                      {errors.sku && <p className="mt-1 text-xs text-red-500">{errors.sku}</p>}
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                        Barcode {!isEdit && <span className="text-red-500">*</span>}
                      </label>
                      <input
                        type="text"
                        value={form.barcode}
                        onChange={(e) => set('barcode', e.target.value)}
                        placeholder="1234567890"
                        disabled={isSaving || isEdit}
                        className={cn(inputCls(errors.barcode), isEdit && 'opacity-60 cursor-not-allowed')}
                      />
                      {errors.barcode && <p className="mt-1 text-xs text-red-500">{errors.barcode}</p>}
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">PLU</label>
                      <input
                        type="text"
                        value={form.plu}
                        onChange={(e) => set('plu', e.target.value)}
                        placeholder="4001"
                        disabled={isSaving}
                        className={inputCls()}
                      />
                    </div>
                  </div>

                  {/* Name EN + Name BN */}
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                        Name (English) <span className="text-red-500">*</span>
                      </label>
                      <input
                        type="text"
                        value={form.name}
                        onChange={(e) => set('name', e.target.value)}
                        placeholder="Product Name"
                        disabled={isSaving}
                        className={inputCls(errors.name)}
                      />
                      {errors.name && <p className="mt-1 text-xs text-red-500">{errors.name}</p>}
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                        Name (Bengali) <span className="text-red-500">*</span>
                      </label>
                      <input
                        type="text"
                        value={form.nameBn}
                        onChange={(e) => set('nameBn', e.target.value)}
                        placeholder="পণ্যের নাম"
                        disabled={isSaving}
                        className={inputCls(errors.nameBn)}
                      />
                      {errors.nameBn && <p className="mt-1 text-xs text-red-500">{errors.nameBn}</p>}
                    </div>
                  </div>

                  {/* Category + Manufacturer */}
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                        Category <span className="text-red-500">*</span>
                      </label>
                      <select
                        value={form.categoryId}
                        onChange={(e) => set('categoryId', e.target.value)}
                        disabled={isSaving}
                        className={inputCls(errors.categoryId)}
                      >
                        <option value="">Select category…</option>
                        {categories.map((c) => (
                          <option key={c.id} value={c.id}>
                            {c.parentCategoryId ? '  ↳ ' : ''}{c.name}
                          </option>
                        ))}
                      </select>
                      {errors.categoryId && <p className="mt-1 text-xs text-red-500">{errors.categoryId}</p>}
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Manufacturer</label>
                      <select
                        value={form.manufacturerId}
                        onChange={(e) => set('manufacturerId', e.target.value)}
                        disabled={isSaving}
                        className={inputCls()}
                      >
                        <option value="">None</option>
                        {manufacturers.map((m) => (
                          <option key={m.id} value={m.id}>{m.name}</option>
                        ))}
                      </select>
                    </div>
                  </div>

                  {/* Description */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Description</label>
                    <textarea
                      value={form.description}
                      onChange={(e) => set('description', e.target.value)}
                      rows={3}
                      placeholder="Optional product description…"
                      disabled={isSaving}
                      className={cn(inputCls(), 'resize-none')}
                    />
                  </div>

                  {/* Tags */}
                  {allTags.length > 0 && (
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Tags</label>
                      <div className="flex flex-wrap gap-2">
                        {allTags.map((t) => {
                          const selected = form.tagIds.includes(t.id);
                          return (
                            <button
                              key={t.id}
                              type="button"
                              onClick={() => toggleTag(t.id)}
                              disabled={isSaving}
                              className={cn(
                                'px-2.5 py-1 rounded-full text-xs font-medium border transition-colors',
                                selected
                                  ? 'bg-primary-100 dark:bg-primary-900/30 border-primary-400 text-primary-700 dark:text-primary-300'
                                  : 'bg-gray-100 dark:bg-gray-800 border-gray-200 dark:border-gray-700 text-gray-600 dark:text-gray-400 hover:border-gray-400',
                              )}
                            >
                              {selected && '✓ '}{t.name}
                            </button>
                          );
                        })}
                      </div>
                    </div>
                  )}
                </div>
              )}

              {/* ── Tab: Pricing ────────────────────────────────────────────── */}
              {tab === 'pricing' && (
                <div className="space-y-4">
                  <div className="grid grid-cols-3 gap-3">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                        Selling Price <span className="text-red-500">*</span>
                      </label>
                      <input
                        type="number"
                        min="0"
                        step="0.01"
                        value={form.price}
                        onChange={(e) => set('price', e.target.value)}
                        placeholder="0.00"
                        disabled={isSaving}
                        className={inputCls(errors.price)}
                      />
                      {errors.price && <p className="mt-1 text-xs text-red-500">{errors.price}</p>}
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Cost Price</label>
                      <input
                        type="number"
                        min="0"
                        step="0.01"
                        value={form.costPrice}
                        onChange={(e) => set('costPrice', e.target.value)}
                        placeholder="0.00"
                        disabled={isSaving}
                        className={inputCls(errors.costPrice)}
                      />
                      {errors.costPrice && <p className="mt-1 text-xs text-red-500">{errors.costPrice}</p>}
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                        VAT Rate <span className="text-red-500">*</span>
                      </label>
                      <select
                        value={form.vatRateId}
                        onChange={(e) => set('vatRateId', e.target.value)}
                        disabled={isSaving}
                        className={inputCls(errors.vatRateId)}
                      >
                        <option value="">Select VAT…</option>
                        {vatRates.map((v) => (
                          <option key={v.id} value={v.id}>{v.name} ({v.rate}%)</option>
                        ))}
                      </select>
                      {errors.vatRateId && <p className="mt-1 text-xs text-red-500">{errors.vatRateId}</p>}
                    </div>
                  </div>

                  {/* Image URL */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Primary Image URL
                      <span className="ml-2 text-xs text-gray-400 font-normal">(for the product card thumbnail)</span>
                    </label>
                    <input
                      type="url"
                      value={form.imageUrl}
                      onChange={(e) => set('imageUrl', e.target.value)}
                      placeholder="https://example.com/product.jpg"
                      disabled={isSaving}
                      className={inputCls()}
                    />
                  </div>

                  {form.price && form.costPrice && parseFloat(form.price) > 0 && parseFloat(form.costPrice) > 0 && (
                    <div className="bg-emerald-50 dark:bg-emerald-900/20 border border-emerald-200 dark:border-emerald-800 rounded-lg px-4 py-3">
                      <p className="text-sm text-emerald-700 dark:text-emerald-300">
                        <span className="font-medium">Margin:</span>{' '}
                        {(((parseFloat(form.price) - parseFloat(form.costPrice)) / parseFloat(form.price)) * 100).toFixed(1)}%
                        {' '}({(parseFloat(form.price) - parseFloat(form.costPrice)).toFixed(2)} per unit)
                      </p>
                    </div>
                  )}
                </div>
              )}

              {/* ── Tab: Settings ───────────────────────────────────────────── */}
              {tab === 'settings' && (
                <div className="space-y-5">
                  {/* Unit Type + Label + Reorder */}
                  <div className="grid grid-cols-3 gap-3">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Unit Type</label>
                      <select
                        value={form.unitType}
                        onChange={(e) => set('unitType', e.target.value)}
                        disabled={isSaving}
                        className={inputCls()}
                      >
                        {UNIT_TYPES.map((u) => <option key={u} value={u}>{u}</option>)}
                      </select>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Unit Label</label>
                      <input
                        type="text"
                        value={form.unitLabel}
                        onChange={(e) => set('unitLabel', e.target.value)}
                        placeholder="kg / L / pcs"
                        disabled={isSaving}
                        className={inputCls()}
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Reorder Level</label>
                      <input
                        type="number"
                        min="0"
                        value={form.reorderLevel}
                        onChange={(e) => set('reorderLevel', e.target.value)}
                        placeholder="0"
                        disabled={isSaving}
                        className={inputCls()}
                      />
                    </div>
                  </div>

                  {/* Boolean flags */}
                  <div className="space-y-3">
                    <p className="text-sm font-medium text-gray-700 dark:text-gray-300">Product Flags</p>
                    <div className="grid grid-cols-2 gap-3">
                      {([
                        ['isWeightBased',  'Weight-Based Pricing'],
                        ['isPerishable',   'Perishable Item'],
                        ['isEbtEligible',  'EBT Eligible'],
                        ['trackInventory', 'Track Inventory'],
                      ] as [keyof ProductFormData, string][]).map(([key, label]) => (
                        <label key={key} className="flex items-center gap-3 p-3 border border-gray-200 dark:border-gray-700 rounded-lg cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors">
                          <div
                            onClick={() => set(key, !form[key] as ProductFormData[typeof key])}
                            className={cn(
                              'w-9 h-5 rounded-full transition-colors relative flex-shrink-0',
                              form[key] ? 'bg-primary-700' : 'bg-gray-300 dark:bg-gray-600',
                            )}
                          >
                            <span className={cn(
                              'absolute top-0.5 w-4 h-4 rounded-full bg-white shadow transition-all',
                              form[key] ? 'left-4' : 'left-0.5',
                            )} />
                          </div>
                          <span className="text-sm text-gray-700 dark:text-gray-300">{label}</span>
                        </label>
                      ))}
                    </div>
                  </div>

                  {/* Age restriction */}
                  <div className="border border-gray-200 dark:border-gray-700 rounded-lg p-4 space-y-3">
                    <label className="flex items-center gap-3 cursor-pointer">
                      <div
                        onClick={() => set('isAgeRestricted', !form.isAgeRestricted)}
                        className={cn(
                          'w-9 h-5 rounded-full transition-colors relative flex-shrink-0',
                          form.isAgeRestricted ? 'bg-amber-500' : 'bg-gray-300 dark:bg-gray-600',
                        )}
                      >
                        <span className={cn(
                          'absolute top-0.5 w-4 h-4 rounded-full bg-white shadow transition-all',
                          form.isAgeRestricted ? 'left-4' : 'left-0.5',
                        )} />
                      </div>
                      <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Age Restricted Product</span>
                    </label>
                    {form.isAgeRestricted && (
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                          Minimum Age (years)
                        </label>
                        <input
                          type="number"
                          min="0"
                          max="25"
                          value={form.ageRestrictionYears}
                          onChange={(e) => set('ageRestrictionYears', e.target.value)}
                          placeholder="18"
                          disabled={isSaving}
                          className={cn(inputCls(errors.ageRestrictionYears), 'max-w-[140px]')}
                        />
                        {errors.ageRestrictionYears && (
                          <p className="mt-1 text-xs text-red-500">{errors.ageRestrictionYears}</p>
                        )}
                      </div>
                    )}
                  </div>
                </div>
              )}

              {/* ── Tab: Images ─────────────────────────────────────────────── */}
              {tab === 'images' && product && (
                <ProductImageManager productId={product.id} />
              )}
            </div>

            {tab !== 'images' && (
              <div className="flex justify-between items-center px-6 py-4 border-t border-gray-200 dark:border-gray-700">
                <div className="flex items-center gap-1 text-xs text-gray-400">
                  {tabs.filter((t) => t.id !== 'images').map((t, i, arr) => (
                    <span key={t.id} className="flex items-center gap-1">
                      <button
                        type="button"
                        onClick={() => setTab(t.id)}
                        className={cn(
                          'transition-colors',
                          tab === t.id ? 'text-primary-700 font-medium' : 'hover:text-gray-600',
                        )}
                      >
                        {t.label}
                      </button>
                      {i < arr.length - 1 && <ChevronRight className="w-3 h-3" />}
                    </span>
                  ))}
                </div>
                <div className="flex gap-3">
                  <button
                    type="button"
                    onClick={onClose}
                    disabled={isSaving}
                    className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-40"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={isSaving}
                    className="px-4 py-2 text-sm font-medium text-white bg-primary-800 hover:bg-primary-900 rounded-lg transition-colors disabled:opacity-50 flex items-center gap-2"
                  >
                    {isSaving && (
                      <svg className="w-4 h-4 animate-spin" viewBox="0 0 24 24" fill="none">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
                      </svg>
                    )}
                    {isSaving ? 'Saving…' : isEdit ? 'Save Changes' : 'Create Product'}
                  </button>
                </div>
              </div>
            )}

            {tab === 'images' && (
              <div className="flex justify-end px-6 py-4 border-t border-gray-200 dark:border-gray-700">
                <button
                  type="button"
                  onClick={onClose}
                  className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
                >
                  Done
                </button>
              </div>
            )}
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
