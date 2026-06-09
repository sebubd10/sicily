import { useEffect, useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { Layers, X, AlertTriangle } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { Category, CategoryFormData } from '../../types/category';

type Props = {
  open: boolean;
  category: Category | null;
  allCategories: Category[];
  onSave: (data: CategoryFormData) => void;
  onClose: () => void;
  isSaving?: boolean;
};

const EMPTY: CategoryFormData = {
  name: '',
  nameBn: '',
  description: '',
  parentCategoryId: '',
  sortOrder: 1,
};

export function CategoryModal({ open, category, allCategories, onSave, onClose, isSaving }: Props) {
  const isEdit = category !== null;
  const [form, setForm] = useState<CategoryFormData>(EMPTY);
  const [errors, setErrors] = useState<Partial<Record<keyof CategoryFormData, string>>>({});

  useEffect(() => {
    if (open) {
      setErrors({});
      setForm(
        isEdit
          ? {
              name: category.name,
              nameBn: category.nameBn,
              description: category.description ?? '',
              parentCategoryId: category.parentCategoryId ?? '',
              sortOrder: category.sortOrder,
            }
          : {
              ...EMPTY,
              sortOrder: Math.max(0, ...allCategories.map((c) => c.sortOrder)) + 1,
            },
      );
    }
  }, [open, category]);

  // A category with sub-categories cannot be nested under another parent —
  // that would create a 3-level hierarchy.
  const hasChildren = isEdit && (category.childCount ?? 0) > 0;

  // Root categories only; exclude self when editing
  const parentOptions = allCategories.filter(
    (c) => !c.parentCategoryId && c.id !== category?.id,
  );

  function set<K extends keyof CategoryFormData>(key: K, value: CategoryFormData[K]) {
    setForm((f) => ({ ...f, [key]: value }));
    setErrors((e) => ({ ...e, [key]: undefined }));
  }

  function validate(): boolean {
    const e: Partial<Record<keyof CategoryFormData, string>> = {};
    if (!form.name.trim()) e.name = 'Name (English) is required.';
    if (form.sortOrder < 1) e.sortOrder = 'Sort order must be at least 1.';
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  function handleSubmit(ev: React.FormEvent) {
    ev.preventDefault();
    if (validate()) onSave(form);
  }

  const inputCls = (err?: string) =>
    cn(
      'w-full rounded-lg border px-3 py-2 text-sm bg-white dark:bg-gray-900',
      'text-gray-900 dark:text-white placeholder-gray-400',
      'focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 transition',
      err
        ? 'border-red-400 dark:border-red-500'
        : 'border-gray-200 dark:border-gray-700',
    );

  return (
    <Dialog.Root open={open} onOpenChange={(o) => !o && !isSaving && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <Dialog.Content
          className={cn(
            'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
            'w-full max-w-lg rounded-xl bg-white dark:bg-gray-900',
            'border border-gray-200 dark:border-gray-700 shadow-xl',
            'focus:outline-none',
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
              <Layers className="w-5 h-5 text-primary-700" />
              {isEdit ? 'Edit Category' : 'Add Category'}
            </Dialog.Title>
            <Dialog.Description className="sr-only">
              {isEdit ? `Edit category ${category?.name}` : 'Add a new product category'}
            </Dialog.Description>
            <Dialog.Close
              disabled={isSaving}
              className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 disabled:opacity-40"
            >
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} noValidate>
            <div className="px-6 py-5 space-y-4">

              {/* Name (English) */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Name <span className="text-xs text-gray-400">(English)</span>
                  <span className="text-red-500 ml-0.5">*</span>
                </label>
                <input
                  type="text"
                  value={form.name}
                  onChange={(e) => set('name', e.target.value)}
                  placeholder="e.g. Dairy & Eggs"
                  className={inputCls(errors.name)}
                />
                {errors.name && <p className="mt-1 text-xs text-red-500">{errors.name}</p>}
              </div>

              {/* Name (Bengali) */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Name <span className="text-xs text-gray-400">(Bengali)</span>
                </label>
                <input
                  type="text"
                  value={form.nameBn}
                  onChange={(e) => set('nameBn', e.target.value)}
                  placeholder="e.g. দুগ্ধজাত পণ্য"
                  className={inputCls()}
                  dir="auto"
                />
              </div>

              {/* Description */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Description
                </label>
                <input
                  type="text"
                  value={form.description}
                  onChange={(e) => set('description', e.target.value)}
                  placeholder="Optional short description"
                  className={inputCls()}
                />
              </div>

              {/* Parent Category */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Parent Category
                  <span className="text-xs text-gray-400 ml-1">(leave empty for root)</span>
                </label>
                {hasChildren ? (
                  <div className="flex items-start gap-2 rounded-lg border border-amber-200 dark:border-amber-700 bg-amber-50 dark:bg-amber-900/20 px-3 py-2.5">
                    <AlertTriangle className="w-4 h-4 text-amber-600 dark:text-amber-400 flex-shrink-0 mt-0.5" />
                    <p className="text-xs text-amber-700 dark:text-amber-300">
                      This category has {category.childCount} sub-categor{category.childCount === 1 ? 'y' : 'ies'} and cannot be nested under another category. Remove its sub-categories first.
                    </p>
                  </div>
                ) : (
                  <select
                    value={form.parentCategoryId}
                    onChange={(e) => set('parentCategoryId', e.target.value)}
                    disabled={isSaving}
                    className={cn(inputCls(), 'cursor-pointer')}
                  >
                    <option value="">— None (root category) —</option>
                    {parentOptions.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.name}
                      </option>
                    ))}
                  </select>
                )}
              </div>

              {/* Sort Order */}
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Sort Order <span className="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  min={1}
                  value={form.sortOrder}
                  onChange={(e) => set('sortOrder', Math.max(1, Number(e.target.value)))}
                  className={cn(inputCls(errors.sortOrder), 'w-28')}
                />
                {errors.sortOrder && <p className="mt-1 text-xs text-red-500">{errors.sortOrder}</p>}
                <p className="mt-1 text-xs text-gray-400">Lower numbers appear first in listings.</p>
              </div>
            </div>

            {/* Footer */}
            <div className="flex justify-end gap-3 border-t border-gray-200 dark:border-gray-700 px-6 py-4">
              <Dialog.Close
                type="button"
                disabled={isSaving}
                className="px-4 py-2 text-sm font-medium rounded-lg border border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-50"
              >
                Cancel
              </Dialog.Close>
              <button
                type="submit"
                disabled={isSaving}
                className="px-5 py-2 text-sm font-medium rounded-lg bg-primary-800 hover:bg-primary-900 text-white shadow-sm transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 focus-visible:ring-offset-2 disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {isSaving ? 'Saving…' : isEdit ? 'Save Changes' : 'Add Category'}
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
