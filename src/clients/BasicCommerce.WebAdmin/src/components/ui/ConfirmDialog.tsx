import * as Dialog from '@radix-ui/react-dialog';
import { AlertTriangle, Trash2, X } from 'lucide-react';
import { cn } from '../../lib/utils';

type Variant = 'danger' | 'warning';

type Props = {
  open: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  variant?: Variant;
  loading?: boolean;
  onConfirm: () => void;
  onClose: () => void;
};

const variantStyles: Record<Variant, { icon: React.ReactNode; btn: string; iconWrap: string }> = {
  danger: {
    icon: <Trash2 className="w-6 h-6 text-red-600 dark:text-red-400" />,
    btn: 'bg-red-600 hover:bg-red-700 focus-visible:ring-red-500',
    iconWrap: 'bg-red-100 dark:bg-red-900/30',
  },
  warning: {
    icon: <AlertTriangle className="w-6 h-6 text-amber-500 dark:text-amber-400" />,
    btn: 'bg-amber-500 hover:bg-amber-600 focus-visible:ring-amber-400',
    iconWrap: 'bg-amber-100 dark:bg-amber-900/30',
  },
};

export function ConfirmDialog({
  open,
  title,
  message,
  confirmLabel = 'Confirm',
  variant = 'danger',
  loading = false,
  onConfirm,
  onClose,
}: Props) {
  const v = variantStyles[variant];

  return (
    <Dialog.Root open={open} onOpenChange={(o) => !o && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <Dialog.Content
          className={cn(
            'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
            'w-full max-w-md rounded-xl bg-white dark:bg-gray-900',
            'border border-gray-200 dark:border-gray-700 shadow-xl',
            'p-6 focus:outline-none',
            'data-[state=open]:animate-in data-[state=closed]:animate-out',
            'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
            'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
            'data-[state=closed]:slide-out-to-left-1/2 data-[state=closed]:slide-out-to-top-48%',
            'data-[state=open]:slide-in-from-left-1/2 data-[state=open]:slide-in-from-top-48%',
          )}
        >
          {/* Close button */}
          <Dialog.Close
            className="absolute right-4 top-4 rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500"
            disabled={loading}
          >
            <X className="w-4 h-4" />
          </Dialog.Close>

          {/* Icon + title */}
          <div className="flex items-start gap-4">
            <div className={cn('flex-shrink-0 flex items-center justify-center w-11 h-11 rounded-full', v.iconWrap)}>
              {v.icon}
            </div>
            <div>
              <Dialog.Title className="text-base font-semibold text-gray-900 dark:text-white">
                {title}
              </Dialog.Title>
              <Dialog.Description className="mt-1 text-sm text-gray-500 dark:text-gray-400">
                {message}
              </Dialog.Description>
            </div>
          </div>

          {/* Actions */}
          <div className="mt-6 flex justify-end gap-3">
            <button
              type="button"
              onClick={onClose}
              disabled={loading}
              className="px-4 py-2 text-sm font-medium rounded-lg border border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-50"
            >
              Cancel
            </button>
            <button
              type="button"
              onClick={onConfirm}
              disabled={loading}
              className={cn(
                'px-4 py-2 text-sm font-medium rounded-lg text-white transition-colors',
                'focus:outline-none focus-visible:ring-2 focus-visible:ring-offset-2',
                'disabled:opacity-60 disabled:cursor-not-allowed',
                v.btn,
              )}
            >
              {loading ? 'Please wait…' : confirmLabel}
            </button>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
