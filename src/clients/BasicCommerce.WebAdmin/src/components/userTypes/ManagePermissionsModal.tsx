import { useEffect, useMemo, useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { Key, Search, X } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { ApiPermission, UserType } from '../../types/userType';

type Props = {
  open: boolean;
  userType: UserType | null;
  allPermissions: ApiPermission[];
  onSave: (codes: string[]) => void;
  onClose: () => void;
  isSaving?: boolean;
};

export function ManagePermissionsModal({
  open,
  userType,
  allPermissions,
  onSave,
  onClose,
  isSaving,
}: Props) {
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [search, setSearch] = useState('');

  useEffect(() => {
    if (open && userType) {
      setSelected(new Set(userType.permissionCodes ?? []));
      setSearch('');
    }
  }, [open, userType]);

  // Group permissions by their Group field
  const grouped = useMemo(() => {
    const q = search.trim().toLowerCase();
    const filtered = q
      ? allPermissions.filter(
          (p) =>
            p.name.toLowerCase().includes(q) ||
            p.code.toLowerCase().includes(q) ||
            p.group.toLowerCase().includes(q),
        )
      : allPermissions;

    return filtered.reduce<Record<string, ApiPermission[]>>((acc, p) => {
      (acc[p.group] ??= []).push(p);
      return acc;
    }, {});
  }, [allPermissions, search]);

  const groups = Object.keys(grouped).sort();

  function toggle(code: string) {
    setSelected((prev) => {
      const next = new Set(prev);
      next.has(code) ? next.delete(code) : next.add(code);
      return next;
    });
  }

  function toggleGroup(group: string, perms: ApiPermission[]) {
    const allChecked = perms.every((p) => selected.has(p.code));
    setSelected((prev) => {
      const next = new Set(prev);
      perms.forEach((p) => (allChecked ? next.delete(p.code) : next.add(p.code)));
      return next;
    });
  }

  function selectAll() {
    setSelected(new Set(allPermissions.map((p) => p.code)));
  }

  function clearAll() {
    setSelected(new Set());
  }

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
            'w-full max-w-2xl rounded-xl bg-white dark:bg-gray-900',
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
              <Key className="w-5 h-5 text-primary-700" />
              Manage Permissions
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
          <div className="px-6 py-3 border-b border-gray-100 dark:border-gray-800 flex items-center gap-3 flex-shrink-0">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-gray-400" />
              <input
                type="text"
                placeholder="Search permissions..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="w-full pl-8 pr-3 py-1.5 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-gray-900 dark:text-white placeholder-gray-400"
              />
            </div>
            <span className="text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">
              {selected.size} / {allPermissions.length} selected
            </span>
            <button type="button" onClick={selectAll} className="text-xs text-primary-700 hover:underline whitespace-nowrap">All</button>
            <button type="button" onClick={clearAll} className="text-xs text-gray-500 hover:underline whitespace-nowrap">None</button>
          </div>

          {/* Scrollable permission list */}
          <div className="flex-1 overflow-y-auto px-6 py-4 space-y-4 min-h-0">
            {groups.length === 0 ? (
              <p className="text-sm text-gray-400 text-center py-8">No permissions match your search.</p>
            ) : (
              groups.map((group) => {
                const perms = grouped[group];
                const allChecked = perms.every((p) => selected.has(p.code));
                const someChecked = !allChecked && perms.some((p) => selected.has(p.code));

                return (
                  <div key={group}>
                    {/* Group header */}
                    <label className="flex items-center gap-2 cursor-pointer mb-2">
                      <input
                        type="checkbox"
                        checked={allChecked}
                        ref={(el) => { if (el) el.indeterminate = someChecked; }}
                        onChange={() => toggleGroup(group, perms)}
                        disabled={isSaving}
                        className="accent-primary-700 w-4 h-4 rounded"
                      />
                      <span className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        {group}
                      </span>
                      <span className="text-xs text-gray-400">
                        ({perms.filter((p) => selected.has(p.code)).length}/{perms.length})
                      </span>
                    </label>

                    {/* Permission items */}
                    <div className="ml-4 space-y-1">
                      {perms.map((p) => (
                        <label
                          key={p.code}
                          className={cn(
                            'flex items-start gap-3 p-2 rounded-lg cursor-pointer transition-colors',
                            selected.has(p.code)
                              ? 'bg-primary-50 dark:bg-primary-900/20'
                              : 'hover:bg-gray-50 dark:hover:bg-gray-800',
                          )}
                        >
                          <input
                            type="checkbox"
                            checked={selected.has(p.code)}
                            onChange={() => toggle(p.code)}
                            disabled={isSaving}
                            className="accent-primary-700 w-4 h-4 mt-0.5 rounded flex-shrink-0"
                          />
                          <div className="min-w-0">
                            <p className="text-sm font-medium text-gray-900 dark:text-white">{p.name}</p>
                            <p className="text-xs text-gray-400 font-mono">{p.code}</p>
                            {p.description && (
                              <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">{p.description}</p>
                            )}
                          </div>
                        </label>
                      ))}
                    </div>
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
              {isSaving ? 'Saving…' : 'Save Permissions'}
            </button>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
