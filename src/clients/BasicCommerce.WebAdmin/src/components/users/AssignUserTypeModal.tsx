import { useEffect, useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { Tag, X } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { User } from '../../types/user';
import type { UserType } from '../../types/userType';

type Props = {
  open: boolean;
  user: User | null;
  userTypes: UserType[];
  onSave: (userTypeId: string | null) => void;
  onClose: () => void;
  isSaving?: boolean;
};

export function AssignUserTypeModal({ open, user, userTypes, onSave, onClose, isSaving }: Props) {
  const [selected, setSelected] = useState<string>('');

  useEffect(() => {
    if (open && user) {
      setSelected(user.userTypeId ?? '');
    }
  }, [open, user]);

  function handleSubmit(ev: React.FormEvent) {
    ev.preventDefault();
    onSave(selected || null);
  }

  return (
    <Dialog.Root open={open} onOpenChange={(o) => !o && !isSaving && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <Dialog.Content
          className={cn(
            'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
            'w-full max-w-md rounded-xl bg-white dark:bg-gray-900',
            'border border-gray-200 dark:border-gray-700 shadow-xl',
            'focus:outline-none',
            'data-[state=open]:animate-in data-[state=closed]:animate-out',
            'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
            'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
            'data-[state=closed]:slide-out-to-left-1/2 data-[state=closed]:slide-out-to-top-48%',
            'data-[state=open]:slide-in-from-left-1/2 data-[state=open]:slide-in-from-top-48%',
          )}
        >
          <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 px-6 py-4">
            <Dialog.Title className="flex items-center gap-2 text-base font-semibold text-gray-900 dark:text-white">
              <Tag className="w-5 h-5 text-primary-700" />
              Assign User Type
            </Dialog.Title>
            <Dialog.Close
              disabled={isSaving}
              className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 disabled:opacity-40"
            >
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          <form onSubmit={handleSubmit}>
            <div className="px-6 py-5 space-y-4">
              {user && (
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  Assigning user type for <span className="font-medium text-gray-900 dark:text-white">{user.fullName}</span>
                </p>
              )}

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  User Type
                </label>
                <div className="space-y-2">
                  {/* None option */}
                  <label className={cn(
                    'flex items-center gap-3 p-3 rounded-lg border cursor-pointer transition-colors',
                    selected === ''
                      ? 'border-primary-500 bg-primary-50 dark:bg-primary-900/20'
                      : 'border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800',
                  )}>
                    <input
                      type="radio"
                      name="userType"
                      value=""
                      checked={selected === ''}
                      onChange={() => setSelected('')}
                      disabled={isSaving}
                      className="accent-primary-700"
                    />
                    <div>
                      <p className="text-sm font-medium text-gray-700 dark:text-gray-300">None</p>
                      <p className="text-xs text-gray-500 dark:text-gray-400">Remove user type assignment</p>
                    </div>
                  </label>

                  {userTypes.map((ut) => (
                    <label key={ut.id} className={cn(
                      'flex items-center gap-3 p-3 rounded-lg border cursor-pointer transition-colors',
                      selected === ut.id
                        ? 'border-primary-500 bg-primary-50 dark:bg-primary-900/20'
                        : 'border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800',
                    )}>
                      <input
                        type="radio"
                        name="userType"
                        value={ut.id}
                        checked={selected === ut.id}
                        onChange={() => setSelected(ut.id)}
                        disabled={isSaving}
                        className="accent-primary-700"
                      />
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2">
                          {ut.color && (
                            <span
                              className="w-3 h-3 rounded-full flex-shrink-0"
                              style={{ backgroundColor: ut.color }}
                            />
                          )}
                          <p className="text-sm font-medium text-gray-900 dark:text-white truncate">{ut.name}</p>
                          {ut.isSystem && (
                            <span className="text-xs px-1.5 py-0.5 bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300 rounded font-medium">System</span>
                          )}
                        </div>
                        {ut.description && (
                          <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5 truncate">{ut.description}</p>
                        )}
                        <p className="text-xs text-gray-400 dark:text-gray-500 mt-0.5">
                          {ut.permissionCodes?.length ?? 0} permissions
                        </p>
                      </div>
                    </label>
                  ))}
                </div>
              </div>
            </div>

            <div className="flex justify-end gap-3 px-6 py-4 border-t border-gray-200 dark:border-gray-700">
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
                {isSaving ? 'Saving…' : 'Save'}
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
