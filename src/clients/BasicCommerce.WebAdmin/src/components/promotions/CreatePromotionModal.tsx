import { useState, useEffect, useRef } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import {
  X, Percent, DollarSign, Gift, Tag, ShoppingCart,
  AlertCircle, Search, ChevronDown, Check,
} from 'lucide-react';
import { cn, extractApiError } from '../../lib/utils';
import type { CreatePromotionForm, PromotionType } from '../../types/promotion';
import { useCreatePromotion } from '../../hooks/usePromotions';
import { useAllActiveCategories } from '../../hooks/useCategories';
import { useStoreList } from '../../hooks/useStoreAdmin';
import { getProducts } from '../../api/productsApi';
import type { ProductListItem } from '../../types/product';

type Props = {
  open: boolean;
  onClose: () => void;
};

interface TypeCard {
  type: PromotionType;
  label: string;
  description: string;
  icon: React.ReactNode;
  color: string;
}

const TYPE_CARDS: TypeCard[] = [
  {
    type: 'PercentageOff',
    label: 'Percentage Off',
    description: '% discount on a specific product',
    icon: <Percent className="w-5 h-5" />,
    color: 'blue',
  },
  {
    type: 'FixedAmountOff',
    label: 'Fixed Amount Off',
    description: 'Fixed ৳ discount on a specific product',
    icon: <DollarSign className="w-5 h-5" />,
    color: 'emerald',
  },
  {
    type: 'BuyXGetYFree',
    label: 'Buy X Get Y Free',
    description: 'Buy X units and get Y free',
    icon: <Gift className="w-5 h-5" />,
    color: 'purple',
  },
  {
    type: 'CategoryPercentageOff',
    label: 'Category Discount',
    description: '% discount on all products in a category',
    icon: <Tag className="w-5 h-5" />,
    color: 'amber',
  },
  {
    type: 'CartDiscount',
    label: 'Cart Discount',
    description: '% or ৳ off when cart meets minimum value',
    icon: <ShoppingCart className="w-5 h-5" />,
    color: 'rose',
  },
];

const TYPE_COLORS: Record<string, string> = {
  blue: 'border-blue-500 bg-blue-50 dark:bg-blue-900/20 text-blue-700 dark:text-blue-300',
  emerald: 'border-emerald-500 bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-300',
  purple: 'border-purple-500 bg-purple-50 dark:bg-purple-900/20 text-purple-700 dark:text-purple-300',
  amber: 'border-amber-500 bg-amber-50 dark:bg-amber-900/20 text-amber-700 dark:text-amber-300',
  rose: 'border-rose-500 bg-rose-50 dark:bg-rose-900/20 text-rose-700 dark:text-rose-300',
};

const EMPTY_FORM: CreatePromotionForm = {
  name: '',
  description: '',
  type: 'PercentageOff',
  productId: '',
  categoryId: '',
  storeId: '',
  discountPercentage: '',
  discountAmount: '',
  buyQuantity: '',
  getQuantity: '',
  minimumCartValue: '',
  couponCode: '',
  requiresCoupon: false,
  startsAt: '',
  endsAt: '',
  maxUses: '',
};

const inputCls =
  'w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 ' +
  'px-3 py-2 text-sm text-gray-900 dark:text-gray-100 placeholder-gray-400 ' +
  'focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent';

const labelCls = 'block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1';

export function CreatePromotionModal({ open, onClose }: Props) {
  const [step, setStep] = useState<'type' | 'details'>('type');
  const [form, setForm] = useState<CreatePromotionForm>(EMPTY_FORM);
  const [errors, setErrors] = useState<Partial<Record<keyof CreatePromotionForm, string>>>({});

  // Product search
  const [productQuery, setProductQuery] = useState('');
  const [productResults, setProductResults] = useState<ProductListItem[]>([]);
  const [productSearching, setProductSearching] = useState(false);
  const [productOpen, setProductOpen] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState<ProductListItem | null>(null);
  const productRef = useRef<HTMLDivElement>(null);
  const productTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const { data: categories = [] } = useAllActiveCategories();
  const { data: stores = [] } = useStoreList();
  const { mutate: createPromotion, isPending, error: saveError } = useCreatePromotion();

  const errorMsg = saveError ? extractApiError(saveError) : null;

  useEffect(() => {
    if (!open) {
      setStep('type');
      setForm(EMPTY_FORM);
      setErrors({});
      setProductQuery('');
      setSelectedProduct(null);
      setProductResults([]);
    }
  }, [open]);

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (productRef.current && !productRef.current.contains(e.target as Node))
        setProductOpen(false);
    }
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  function set(field: keyof CreatePromotionForm, value: string | boolean) {
    setForm((f) => ({ ...f, [field]: value }));
    setErrors((e) => ({ ...e, [field]: undefined }));
  }

  function handleProductSearch(q: string) {
    setProductQuery(q);
    setProductOpen(true);
    if (q !== selectedProduct?.name) {
      set('productId', '');
      setSelectedProduct(null);
    }
    if (productTimerRef.current) clearTimeout(productTimerRef.current);
    if (q.length < 2) { setProductResults([]); return; }
    productTimerRef.current = setTimeout(async () => {
      setProductSearching(true);
      try {
        const res = await getProducts({ search: q, page: 1, pageSize: 10 });
        setProductResults(res.items);
      } finally {
        setProductSearching(false);
      }
    }, 300);
  }

  function selectProduct(p: ProductListItem) {
    setSelectedProduct(p);
    setProductQuery(p.name);
    set('productId', p.id);
    setProductOpen(false);
    setProductResults([]);
  }

  function validateDetails(): boolean {
    const e: Partial<Record<keyof CreatePromotionForm, string>> = {};
    if (!form.name.trim()) e.name = 'Name is required';

    if (form.type === 'PercentageOff' || form.type === 'FixedAmountOff' || form.type === 'BuyXGetYFree') {
      if (!form.productId) e.productId = 'Select a product';
    }
    if (form.type === 'CategoryPercentageOff') {
      if (!form.categoryId) e.categoryId = 'Select a category';
    }
    if (form.type === 'PercentageOff' || form.type === 'CategoryPercentageOff') {
      if (!form.discountPercentage || Number(form.discountPercentage) <= 0)
        e.discountPercentage = 'Enter a valid discount %';
    }
    if (form.type === 'FixedAmountOff') {
      if (!form.discountAmount || Number(form.discountAmount) <= 0)
        e.discountAmount = 'Enter a valid discount amount';
    }
    if (form.type === 'BuyXGetYFree') {
      if (!form.buyQuantity || Number(form.buyQuantity) < 1) e.buyQuantity = 'Required';
      if (!form.getQuantity || Number(form.getQuantity) < 1) e.getQuantity = 'Required';
    }
    if (form.type === 'CartDiscount') {
      if (!form.discountPercentage && !form.discountAmount)
        e.discountPercentage = 'Enter discount % or amount';
      if (!form.minimumCartValue || Number(form.minimumCartValue) <= 0)
        e.minimumCartValue = 'Enter minimum cart value';
    }

    setErrors(e);
    return Object.keys(e).length === 0;
  }

  function handleSubmit() {
    if (!validateDetails()) return;

    const payload = {
      name: form.name.trim(),
      description: form.description.trim() || null,
      type: form.type,
      productId: form.productId || null,
      categoryId: form.categoryId || null,
      storeId: form.storeId || null,
      discountPercentage: form.discountPercentage ? Number(form.discountPercentage) : null,
      discountAmount: form.discountAmount ? Number(form.discountAmount) : null,
      buyQuantity: form.buyQuantity ? Number(form.buyQuantity) : null,
      getQuantity: form.getQuantity ? Number(form.getQuantity) : null,
      minimumCartValue: form.minimumCartValue ? Number(form.minimumCartValue) : null,
      couponCode: form.couponCode.trim() || null,
      requiresCoupon: form.requiresCoupon,
      startsAt: form.startsAt || null,
      endsAt: form.endsAt || null,
      maxUses: form.maxUses ? Number(form.maxUses) : null,
    };

    createPromotion(payload, { onSuccess: onClose });
  }

  const selectedTypeCard = TYPE_CARDS.find((c) => c.type === form.type)!;
  const needsProduct = ['PercentageOff', 'FixedAmountOff', 'BuyXGetYFree'].includes(form.type);
  const needsCategory = form.type === 'CategoryPercentageOff';
  const needsPercent = ['PercentageOff', 'CategoryPercentageOff', 'CartDiscount'].includes(form.type);
  const needsAmount = ['FixedAmountOff', 'CartDiscount'].includes(form.type);
  const needsBXGY = form.type === 'BuyXGetYFree';
  const needsMinCart = form.type === 'CartDiscount';

  return (
    <Dialog.Root open={open} onOpenChange={(v) => !v && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 bg-black/50 backdrop-blur-sm z-40 animate-in fade-in-0 duration-200" />
        <Dialog.Content
          className={cn(
            'fixed left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 z-50',
            'w-full bg-white dark:bg-gray-900 rounded-2xl shadow-2xl',
            'flex flex-col max-h-[92vh] overflow-hidden',
            step === 'type' ? 'max-w-2xl' : 'max-w-xl',
          )}
        >
          {/* Header */}
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-gray-800">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-xl bg-indigo-100 dark:bg-indigo-900/30">
                <Tag className="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
              </div>
              <div>
                <Dialog.Title className="text-base font-semibold text-gray-900 dark:text-gray-100">
                  {step === 'type' ? 'Choose Promotion Type' : 'Promotion Details'}
                </Dialog.Title>
                {step === 'details' && (
                  <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                    {selectedTypeCard.label} — {selectedTypeCard.description}
                  </p>
                )}
              </div>
            </div>
            <Dialog.Close
              className="p-1.5 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
            >
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          {/* Step indicator */}
          <div className="flex px-6 pt-3 gap-2">
            {(['type', 'details'] as const).map((s, i) => (
              <div key={s} className="flex items-center gap-2">
                {i > 0 && <div className="w-8 h-px bg-gray-200 dark:bg-gray-700" />}
                <div className={cn(
                  'flex items-center gap-1.5 text-xs font-medium',
                  step === s ? 'text-indigo-600 dark:text-indigo-400' : 'text-gray-400 dark:text-gray-600',
                )}>
                  <div className={cn(
                    'w-5 h-5 rounded-full flex items-center justify-center text-[10px] font-bold',
                    step === s
                      ? 'bg-indigo-600 text-white'
                      : (s === 'type' && step === 'details')
                        ? 'bg-green-500 text-white'
                        : 'bg-gray-200 dark:bg-gray-700 text-gray-500',
                  )}>
                    {s === 'type' && step === 'details' ? <Check className="w-3 h-3" /> : i + 1}
                  </div>
                  {s === 'type' ? 'Type' : 'Details'}
                </div>
              </div>
            ))}
          </div>

          {/* Body */}
          <div className="flex-1 overflow-y-auto px-6 py-4">
            {/* Step 1: Type selector */}
            {step === 'type' && (
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                {TYPE_CARDS.map((card) => {
                  const isSelected = form.type === card.type;
                  return (
                    <button
                      key={card.type}
                      type="button"
                      onClick={() => set('type', card.type)}
                      className={cn(
                        'relative flex items-start gap-3 p-4 rounded-xl border-2 text-left transition-all',
                        isSelected
                          ? TYPE_COLORS[card.color]
                          : 'border-gray-200 dark:border-gray-700 hover:border-gray-300 dark:hover:border-gray-600 bg-white dark:bg-gray-800',
                      )}
                    >
                      <div className={cn(
                        'p-2 rounded-lg flex-shrink-0',
                        isSelected
                          ? 'bg-white/50 dark:bg-black/20'
                          : 'bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300',
                      )}>
                        {card.icon}
                      </div>
                      <div>
                        <p className={cn(
                          'text-sm font-semibold',
                          isSelected ? '' : 'text-gray-800 dark:text-gray-200',
                        )}>
                          {card.label}
                        </p>
                        <p className={cn(
                          'text-xs mt-0.5',
                          isSelected ? 'opacity-80' : 'text-gray-500 dark:text-gray-400',
                        )}>
                          {card.description}
                        </p>
                      </div>
                      {isSelected && (
                        <Check className="absolute top-3 right-3 w-4 h-4" />
                      )}
                    </button>
                  );
                })}
              </div>
            )}

            {/* Step 2: Details */}
            {step === 'details' && (
              <div className="space-y-4">
                {errorMsg && (
                  <div className="flex items-start gap-2 p-3 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800">
                    <AlertCircle className="w-4 h-4 text-red-500 flex-shrink-0 mt-0.5" />
                    <p className="text-sm text-red-700 dark:text-red-300">{errorMsg}</p>
                  </div>
                )}

                {/* Name */}
                <div>
                  <label className={labelCls}>Promotion Name *</label>
                  <input
                    className={cn(inputCls, errors.name && 'border-red-400 focus:ring-red-500')}
                    placeholder="e.g. Summer Sale 10% Off"
                    value={form.name}
                    onChange={(e) => set('name', e.target.value)}
                  />
                  {errors.name && <p className="text-xs text-red-500 mt-1">{errors.name}</p>}
                </div>

                {/* Description */}
                <div>
                  <label className={labelCls}>Description (optional)</label>
                  <textarea
                    rows={2}
                    className={cn(inputCls, 'resize-none')}
                    placeholder="Customer-facing description"
                    value={form.description}
                    onChange={(e) => set('description', e.target.value)}
                  />
                </div>

                {/* Product search */}
                {needsProduct && (
                  <div>
                    <label className={labelCls}>Product *</label>
                    <div ref={productRef} className="relative">
                      <div className="relative">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-gray-400" />
                        <input
                          className={cn(inputCls, 'pl-9', errors.productId && 'border-red-400')}
                          placeholder="Search products…"
                          value={productQuery}
                          onChange={(e) => handleProductSearch(e.target.value)}
                          onFocus={() => productResults.length > 0 && setProductOpen(true)}
                        />
                        {form.productId && (
                          <Check className="absolute right-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-green-500" />
                        )}
                      </div>
                      {productOpen && (productSearching || productResults.length > 0) && (
                        <div className="absolute top-full left-0 right-0 mt-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg shadow-lg z-50 max-h-44 overflow-y-auto">
                          {productSearching && (
                            <div className="px-3 py-2 text-xs text-gray-500">Searching…</div>
                          )}
                          {productResults.map((p) => (
                            <button
                              key={p.id}
                              type="button"
                              onMouseDown={(e) => { e.preventDefault(); selectProduct(p); }}
                              className="w-full flex items-center gap-2 px-3 py-2 text-sm hover:bg-gray-50 dark:hover:bg-gray-700 text-left"
                            >
                              <span className="text-gray-900 dark:text-gray-100 font-medium">{p.name}</span>
                              <span className="text-gray-400 text-xs">{p.sku}</span>
                            </button>
                          ))}
                        </div>
                      )}
                    </div>
                    {errors.productId && <p className="text-xs text-red-500 mt-1">{errors.productId}</p>}
                  </div>
                )}

                {/* Category */}
                {needsCategory && (
                  <div>
                    <label className={labelCls}>Category *</label>
                    <div className="relative">
                      <select
                        className={cn(inputCls, 'appearance-none pr-8', errors.categoryId && 'border-red-400')}
                        value={form.categoryId}
                        onChange={(e) => set('categoryId', e.target.value)}
                      >
                        <option value="">Select category…</option>
                        {categories.map((c) => (
                          <option key={c.id} value={c.id}>{c.name}</option>
                        ))}
                      </select>
                      <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
                    </div>
                    {errors.categoryId && <p className="text-xs text-red-500 mt-1">{errors.categoryId}</p>}
                  </div>
                )}

                {/* Discount fields */}
                <div className="grid grid-cols-2 gap-3">
                  {needsPercent && (
                    <div className={form.type === 'CartDiscount' ? '' : 'col-span-2'}>
                      <label className={labelCls}>
                        Discount % {form.type !== 'CartDiscount' && '*'}
                      </label>
                      <div className="relative">
                        <input
                          type="number"
                          min="0"
                          max="100"
                          className={cn(inputCls, 'pr-8', errors.discountPercentage && 'border-red-400')}
                          placeholder="10"
                          value={form.discountPercentage}
                          onChange={(e) => set('discountPercentage', e.target.value)}
                        />
                        <span className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm">%</span>
                      </div>
                      {errors.discountPercentage && (
                        <p className="text-xs text-red-500 mt-1">{errors.discountPercentage}</p>
                      )}
                    </div>
                  )}

                  {needsAmount && (
                    <div className={form.type === 'CartDiscount' ? '' : 'col-span-2'}>
                      <label className={labelCls}>
                        Discount Amount {form.type !== 'CartDiscount' && '*'}
                      </label>
                      <div className="relative">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm">৳</span>
                        <input
                          type="number"
                          min="0"
                          className={cn(inputCls, 'pl-8', errors.discountAmount && 'border-red-400')}
                          placeholder="50"
                          value={form.discountAmount}
                          onChange={(e) => set('discountAmount', e.target.value)}
                        />
                      </div>
                      {errors.discountAmount && (
                        <p className="text-xs text-red-500 mt-1">{errors.discountAmount}</p>
                      )}
                    </div>
                  )}

                  {needsBXGY && (
                    <>
                      <div>
                        <label className={labelCls}>Buy Quantity *</label>
                        <input
                          type="number"
                          min="1"
                          className={cn(inputCls, errors.buyQuantity && 'border-red-400')}
                          placeholder="2"
                          value={form.buyQuantity}
                          onChange={(e) => set('buyQuantity', e.target.value)}
                        />
                        {errors.buyQuantity && <p className="text-xs text-red-500 mt-1">{errors.buyQuantity}</p>}
                      </div>
                      <div>
                        <label className={labelCls}>Get Quantity Free *</label>
                        <input
                          type="number"
                          min="1"
                          className={cn(inputCls, errors.getQuantity && 'border-red-400')}
                          placeholder="1"
                          value={form.getQuantity}
                          onChange={(e) => set('getQuantity', e.target.value)}
                        />
                        {errors.getQuantity && <p className="text-xs text-red-500 mt-1">{errors.getQuantity}</p>}
                      </div>
                    </>
                  )}
                </div>

                {/* Minimum cart value */}
                {(needsMinCart || form.type !== 'BuyXGetYFree') && (
                  <div className={cn(needsMinCart ? '' : '')}>
                    <label className={labelCls}>
                      Minimum Cart Value {needsMinCart && '*'}
                    </label>
                    <div className="relative">
                      <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm">৳</span>
                      <input
                        type="number"
                        min="0"
                        className={cn(inputCls, 'pl-8', errors.minimumCartValue && 'border-red-400')}
                        placeholder="0"
                        value={form.minimumCartValue}
                        onChange={(e) => set('minimumCartValue', e.target.value)}
                      />
                    </div>
                    {errors.minimumCartValue && (
                      <p className="text-xs text-red-500 mt-1">{errors.minimumCartValue}</p>
                    )}
                  </div>
                )}

                {/* Store restriction */}
                <div>
                  <label className={labelCls}>Store (optional — leave empty for all stores)</label>
                  <div className="relative">
                    <select
                      className={cn(inputCls, 'appearance-none pr-8')}
                      value={form.storeId}
                      onChange={(e) => set('storeId', e.target.value)}
                    >
                      <option value="">All Stores</option>
                      {stores.map((s) => (
                        <option key={s.id} value={s.id}>{s.name}</option>
                      ))}
                    </select>
                    <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
                  </div>
                </div>

                {/* Coupon code */}
                <div>
                  <div className="flex items-center justify-between mb-1">
                    <label className={cn(labelCls, 'mb-0')}>Coupon Code</label>
                    <label className="flex items-center gap-1.5 cursor-pointer">
                      <input
                        type="checkbox"
                        className="w-3.5 h-3.5 rounded accent-indigo-600"
                        checked={form.requiresCoupon}
                        onChange={(e) => set('requiresCoupon', e.target.checked)}
                      />
                      <span className="text-xs text-gray-500 dark:text-gray-400">Require coupon to apply</span>
                    </label>
                  </div>
                  <input
                    className={inputCls}
                    placeholder="SUMMER25 (optional)"
                    value={form.couponCode}
                    onChange={(e) => set('couponCode', e.target.value.toUpperCase())}
                  />
                </div>

                {/* Dates + Max Uses */}
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className={labelCls}>Starts At</label>
                    <input
                      type="datetime-local"
                      className={inputCls}
                      value={form.startsAt}
                      onChange={(e) => set('startsAt', e.target.value)}
                    />
                  </div>
                  <div>
                    <label className={labelCls}>Ends At</label>
                    <input
                      type="datetime-local"
                      className={inputCls}
                      value={form.endsAt}
                      onChange={(e) => set('endsAt', e.target.value)}
                    />
                  </div>
                </div>

                <div>
                  <label className={labelCls}>Max Total Uses (optional)</label>
                  <input
                    type="number"
                    min="1"
                    className={inputCls}
                    placeholder="Unlimited"
                    value={form.maxUses}
                    onChange={(e) => set('maxUses', e.target.value)}
                  />
                </div>
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="flex items-center justify-between px-6 py-4 border-t border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50">
            {step === 'details' ? (
              <button
                type="button"
                onClick={() => setStep('type')}
                className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100 font-medium transition-colors"
              >
                ← Back
              </button>
            ) : (
              <button
                type="button"
                onClick={onClose}
                className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100 font-medium transition-colors"
              >
                Cancel
              </button>
            )}

            {step === 'type' ? (
              <button
                type="button"
                onClick={() => setStep('details')}
                className="px-5 py-2 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium transition-colors"
              >
                Continue →
              </button>
            ) : (
              <button
                type="button"
                onClick={handleSubmit}
                disabled={isPending}
                className="flex items-center gap-2 px-5 py-2 rounded-lg bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-sm font-medium transition-colors"
              >
                {isPending ? 'Saving…' : 'Create Promotion'}
              </button>
            )}
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
