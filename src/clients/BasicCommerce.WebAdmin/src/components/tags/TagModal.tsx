import { useEffect, useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { Tag, X } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { TagDetail, TagFormData } from '../../types/tag';

type Props = {
  open: boolean;
  tag: TagDetail | null;
  onSave: (data: TagFormData) => void;
  onClose: () => void;
  isSaving?: boolean;
};

const EMPTY: TagFormData = { name: '' };

export function TagModal({ open, tag, onSave, onClose, isSaving }: Props) {
  const isEdit = tag !== null;
  const [form, setForm] = useState<TagFormData>(EMPTY);
  const [error, setError] = useState<string | undefined>();

  useEffect(() => {
    if (open) {
      setError(undefined);
      setForm(isEdit ? { name: tag.name } : EMPTY);
    }
  }, [open, tag]);

  function handleSubmit(ev: React.FormEvent) {
    ev.preventDefault();
    if (!form.name.trim()) {
      setError('Tag name is required.');
      return;
    }
    onSave({ name: form.name.trim() });
  }

  const inputCls = cn(
    'w-full rounded-lg border px-3 py-2 text-sm bg-white dark:bg-gray-900',
    'text-gray-900 dark:text-white placeholder-gray-400',
    'focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 transition',
    error ? 'border-red-400 dark:border-red-500' : 'border-gray-200 dark:border-gray-700',
  );

  return (
    <Dialog.Root open={open} onOpenChange={(o) => !o && !isSaving && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <Dialog.Content
          className={cn(
            'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
            'w-full max-w-sm rounded-xl bg-white dark:bg-gray-900',
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
              <Tag className="w-5 h-5 text-primary-700" />
              {isEdit ? 'Edit Tag' : 'Add Tag'}
            </Dialog.Title>
            <Dialog.Close
              disabled={isSaving}
              className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 disabled:opacity-40"
            >
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} noValidate>
            <div className="px-6 py-5">
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Name <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={form.name}
                onChange={(e) => { setForm({ name: e.target.value }); setError(undefined); }}
                placeholder="e.g. Gluten Free"
                className={inputCls}
                autoFocus
              />
              {error && <p className="mt-1 text-xs text-red-500">{error}</p>}
              <p className="mt-1.5 text-xs text-gray-400">
                Tag names must be unique. Used to label and filter products.
              </p>
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
                {isSaving ? 'Saving…' : isEdit ? 'Save Changes' : 'Add Tag'}
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
