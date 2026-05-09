import { useEffect, useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { LayoutDashboard, X } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { MenuResponse, UserType } from '../../types/userType';

type Props = {
  open: boolean;
  userType: UserType | null;
  allMenus: MenuResponse[];
  onSave: (subMenuIds: string[]) => void;
  onClose: () => void;
  isSaving?: boolean;
};

export function ManageMenusModal({
  open,
  userType,
  allMenus,
  onSave,
  onClose,
  isSaving,
}: Props) {
  const [selected, setSelected] = useState<Set<string>>(new Set());

  useEffect(() => {
    if (open && userType) {
      setSelected(new Set(userType.allowedSubMenuIds ?? []));
    }
  }, [open, userType]);

  function toggle(id: string) {
    setSelected((prev) => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  }

  function toggleMenu(menu: MenuResponse) {
    const allChecked = menu.items.every((i) => selected.has(i.id));
    setSelected((prev) => {
      const next = new Set(prev);
      menu.items.forEach((i) => (allChecked ? next.delete(i.id) : next.add(i.id)));
      return next;
    });
  }

  function selectAll() {
    setSelected(new Set(allMenus.flatMap((m) => m.items.map((i) => i.id))));
  }

  function clearAll() {
    setSelected(new Set());
  }

  const totalItems = allMenus.reduce((acc, m) => acc + m.items.length, 0);

  function handleSave() {
    onSave(Array.from(selected));
  }

  return (
    <Dialog.Root open={open} onOpenChange={(o) => !o && !isSaving && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <Dialog.Content
          className={cn(
            'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
            'w-full max-w-lg rounded-xl bg-white dark:bg-gray-900',
            'border border-gray-200 dark:border-gray-700 shadow-xl',
            'focus:outline-none flex flex-col',
            'max-h-[85vh]',
            'data-[state=open]:animate-in data-[state=closed]:animate-out',
            'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
            'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
            'data-[state=closed]:slide-out-to-left-1/2 data-[state=closed]:slide-out-to-top-48%',
            'data-[state=open]:slide-in-from-left-1/2 data-[state=open]:slide-in-from-top-48%',
          )}
        >
          {/* Header */}
          <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 px-6 py-4 flex-shrink-0">
            <Dialog.Title className="flex items-center gap-2 text-base font-semibold text-gray-900 dark:text-white">
              <LayoutDashboard className="w-5 h-5 text-primary-700" />
              Menu Access
              {userType && (
                <span className="text-sm font-normal text-gray-500 dark:text-gray-400">
                  — {userType.name}
                </span>
              )}
            </Dialog.Title>
            <Dialog.Close
              disabled={isSaving}
              className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 disabled:opacity-40"
            >
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          {/* Toolbar */}
          <div className="px-6 py-3 border-b border-gray-100 dark:border-gray-800 flex items-center justify-between flex-shrink-0">
            <span className="text-xs text-gray-500 dark:text-gray-400">
              {selected.size} / {totalItems} items selected
            </span>
            <div className="flex items-center gap-3">
              <button type="button" onClick={selectAll} className="text-xs text-primary-700 hover:underline">Select all</button>
              <button type="button" onClick={clearAll} className="text-xs text-gray-500 hover:underline">Clear all</button>
            </div>
          </div>

          {/* Scrollable menu list */}
          <div className="flex-1 overflow-y-auto px-6 py-4 space-y-4 min-h-0">
            {allMenus.length === 0 ? (
              <p className="text-sm text-gray-400 text-center py-8">No menus configured.</p>
            ) : (
              allMenus
                .slice()
                .sort((a, b) => a.sortOrder - b.sortOrder)
                .map((menu) => {
                  const allChecked = menu.items.every((i) => selected.has(i.id));
                  const someChecked = !allChecked && menu.items.some((i) => selected.has(i.id));

                  return (
                    <div key={menu.id}>
                      {/* Menu group header */}
                      <label className="flex items-center gap-2 cursor-pointer mb-2">
                        <input
                          type="checkbox"
                          checked={allChecked}
                          ref={(el) => { if (el) el.indeterminate = someChecked; }}
                          onChange={() => toggleMenu(menu)}
                          disabled={isSaving}
                          className="accent-primary-700 w-4 h-4 rounded"
                        />
                        <span className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                          {menu.name}
                        </span>
                        <span className="text-xs text-gray-400">
                          ({menu.items.filter((i) => selected.has(i.id)).length}/{menu.items.length})
                        </span>
                      </label>

                      {/* Sub-menu items */}
                      {menu.items.length === 0 ? (
                        <p className="ml-6 text-xs text-gray-400">No sub-menus.</p>
                      ) : (
                        <div className="ml-4 space-y-1">
                          {menu.items
                            .slice()
                            .sort((a, b) => a.sortOrder - b.sortOrder)
                            .map((item) => (
                              <label
                                key={item.id}
                                className={cn(
                                  'flex items-start gap-3 p-2 rounded-lg cursor-pointer transition-colors',
                                  selected.has(item.id)
                                    ? 'bg-primary-50 dark:bg-primary-900/20'
                                    : 'hover:bg-gray-50 dark:hover:bg-gray-800',
                                )}
                              >
                                <input
                                  type="checkbox"
                                  checked={selected.has(item.id)}
                                  onChange={() => toggle(item.id)}
                                  disabled={isSaving}
                                  className="accent-primary-700 w-4 h-4 mt-0.5 flex-shrink-0"
                                />
                                <div className="min-w-0">
                                  <p className="text-sm font-medium text-gray-900 dark:text-white">{item.name}</p>
                                  <p className="text-xs text-gray-400 font-mono">{item.route}</p>
                                  {item.permissionCode && (
                                    <p className="text-xs text-gray-400 mt-0.5">
                                      Requires: <span className="font-mono">{item.permissionCode}</span>
                                    </p>
                                  )}
                                </div>
                              </label>
                            ))}
                        </div>
                      )}
                    </div>
                  );
                })
            )}
          </div>

          {/* Footer */}
          <div className="flex justify-end gap-3 px-6 py-4 border-t border-gray-200 dark:border-gray-700 flex-shrink-0">
            <button
              type="button"
              onClick={onClose}
              disabled={isSaving}
              className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-40"
            >
              Cancel
            </button>
            <button
              type="button"
              onClick={handleSave}
              disabled={isSaving}
              className="px-4 py-2 text-sm font-medium text-white bg-primary-800 hover:bg-primary-900 rounded-lg transition-colors disabled:opacity-50 flex items-center gap-2"
            >
              {isSaving && (
                <svg className="w-4 h-4 animate-spin" viewBox="0 0 24 24" fill="none">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
                </svg>
              )}
              {isSaving ? 'Saving…' : 'Save Menu Access'}
            </button>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
