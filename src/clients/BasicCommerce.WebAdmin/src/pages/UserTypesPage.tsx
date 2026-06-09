import { useState } from 'react';
import {
  ShieldCheck, Plus, Pencil, Trash2, Key, LayoutDashboard,
  AlertCircle, Loader2,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { UserType, UserTypeFormData } from '../types/userType';
import { UserTypeModal } from '../components/userTypes/UserTypeModal';
import { ManagePermissionsModal } from '../components/userTypes/ManagePermissionsModal';
import { ManageMenusModal } from '../components/userTypes/ManageMenusModal';
import { ConfirmDialog } from '../components/ui/ConfirmDialog';
import {
  useUserTypes,
  useCreateUserType,
  useUpdateUserType,
  useDeleteUserType,
  useSetUserTypePermissions,
  useSetUserTypeMenus,
  useApiPermissions,
  useAllMenus,
} from '../hooks/useUserTypes';

type PermissionTarget = { userType: UserType };
type MenuTarget       = { userType: UserType };

export default function UserTypesPage() {
  const [modalOpen, setModalOpen]       = useState(false);
  const [editType, setEditType]         = useState<UserType | null>(null);
  const [permTarget, setPermTarget]     = useState<PermissionTarget | null>(null);
  const [menuTarget, setMenuTarget]     = useState<MenuTarget | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<UserType | null>(null);
  const [deleteError, setDeleteError]   = useState<string | null>(null);

  const { data: userTypes = [], isLoading, isError } = useUserTypes();
  const { data: allPermissions = [] } = useApiPermissions();
  const { data: allMenus = [] } = useAllMenus();

  const createMutation  = useCreateUserType();
  const updateMutation  = useUpdateUserType();
  const deleteMutation  = useDeleteUserType();
  const permsMutation   = useSetUserTypePermissions();
  const menusMutation   = useSetUserTypeMenus();

  const isMutating =
    createMutation.isPending ||
    updateMutation.isPending ||
    deleteMutation.isPending ||
    permsMutation.isPending ||
    menusMutation.isPending;

  function openAdd() {
    setEditType(null);
    setModalOpen(true);
  }

  function openEdit(ut: UserType) {
    setEditType(ut);
    setModalOpen(true);
  }

  async function handleSave(form: UserTypeFormData) {
    if (editType) {
      await updateMutation.mutateAsync({ id: editType.id, form });
    } else {
      await createMutation.mutateAsync(form);
    }
    setModalOpen(false);
    setEditType(null);
  }

  async function handleDeleteConfirm() {
    if (!deleteTarget) return;
    try {
      setDeleteError(null);
      await deleteMutation.mutateAsync(deleteTarget.id);
      setDeleteTarget(null);
    } catch (err) {
      setDeleteError(extractApiError(err));
    }
  }

  async function handleSavePermissions(codes: string[]) {
    if (!permTarget) return;
    await permsMutation.mutateAsync({ id: permTarget.userType.id, codes });
    setPermTarget(null);
  }

  async function handleSaveMenus(subMenuIds: string[]) {
    if (!menuTarget) return;
    await menusMutation.mutateAsync({ id: menuTarget.userType.id, subMenuIds });
    setMenuTarget(null);
  }

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <ShieldCheck className="w-6 h-6 text-primary-700" /> User Types
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Configure operational role profiles, permissions, and menu access
          </p>
        </div>
        <button
          onClick={openAdd}
          className="flex items-center gap-2 px-4 py-2 bg-primary-800 hover:bg-primary-900 text-white text-sm font-medium rounded-lg transition-colors shadow-sm"
        >
          <Plus className="w-4 h-4" /> Add User Type
        </button>
      </div>

      {/* Summary chips */}
      <div className="flex flex-wrap gap-3">
        {[
          { label: 'Total',  value: userTypes.length,                                  color: 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300' },
          { label: 'System', value: userTypes.filter((u) => u.isSystem).length,        color: 'bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300' },
          { label: 'Custom', value: userTypes.filter((u) => !u.isSystem).length,       color: 'bg-primary-100 dark:bg-primary-900/30 text-primary-700 dark:text-primary-300' },
        ].map((s) => (
          <span key={s.label} className={cn('px-3 py-1 rounded-full text-sm font-medium', s.color)}>
            {s.label}: <strong>{s.value}</strong>
          </span>
        ))}
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
        {isError ? (
          <div className="flex items-center gap-2 px-6 py-12 text-red-500">
            <AlertCircle className="w-5 h-5 flex-shrink-0" />
            <span className="text-sm">Failed to load user types. Please refresh and try again.</span>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                  {['#', 'Name', 'Description', 'Permissions', 'Menu Items', 'Sort', 'Actions'].map((h) => (
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
                  Array.from({ length: 4 }).map((_, i) => (
                    <tr key={i} className="animate-pulse">
                      {Array.from({ length: 7 }).map((_, j) => (
                        <td key={j} className="px-4 py-3">
                          <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-full" />
                        </td>
                      ))}
                    </tr>
                  ))
                ) : userTypes.length === 0 ? (
                  <tr>
                    <td colSpan={7} className="px-4 py-12 text-center text-sm text-gray-400">
                      No user types found.
                    </td>
                  </tr>
                ) : (
                  userTypes
                    .slice()
                    .sort((a, b) => a.sortOrder - b.sortOrder)
                    .map((ut, idx) => (
                      <tr key={ut.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                        <td className="px-4 py-3 text-gray-400">{idx + 1}</td>
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-2">
                            {ut.color && (
                              <span
                                className="w-3 h-3 rounded-full flex-shrink-0"
                                style={{ backgroundColor: ut.color }}
                              />
                            )}
                            <span className="font-medium text-gray-900 dark:text-white whitespace-nowrap">
                              {ut.name}
                            </span>
                            {ut.isSystem && (
                              <span className="text-xs px-1.5 py-0.5 bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300 rounded font-medium">
                                System
                              </span>
                            )}
                          </div>
                        </td>
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400 max-w-xs truncate">
                          {ut.description || <span className="text-gray-300 dark:text-gray-600">—</span>}
                        </td>
                        <td className="px-4 py-3">
                          <span className={cn(
                            'inline-flex items-center gap-1.5 text-xs font-medium px-2 py-0.5 rounded-full',
                            (ut.permissionCodes?.length ?? 0) > 0
                              ? 'bg-primary-100 dark:bg-primary-900/30 text-primary-700 dark:text-primary-300'
                              : 'bg-gray-100 dark:bg-gray-800 text-gray-500 dark:text-gray-400',
                          )}>
                            <Key className="w-3 h-3" />
                            {ut.permissionCodes?.length ?? 0}
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          <span className={cn(
                            'inline-flex items-center gap-1.5 text-xs font-medium px-2 py-0.5 rounded-full',
                            (ut.allowedSubMenuIds?.length ?? 0) > 0
                              ? 'bg-violet-100 dark:bg-violet-900/30 text-violet-700 dark:text-violet-300'
                              : 'bg-gray-100 dark:bg-gray-800 text-gray-500 dark:text-gray-400',
                          )}>
                            <LayoutDashboard className="w-3 h-3" />
                            {ut.allowedSubMenuIds?.length ?? 0}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-center text-gray-600 dark:text-gray-400">{ut.sortOrder}</td>
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-1">
                            {/* Manage permissions */}
                            <button
                              title="Manage Permissions"
                              onClick={() => setPermTarget({ userType: ut })}
                              disabled={isMutating}
                              className="p-1.5 rounded-lg text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors disabled:opacity-40"
                            >
                              <Key className="w-3.5 h-3.5" />
                            </button>

                            {/* Manage menus */}
                            <button
                              title="Manage Menu Access"
                              onClick={() => setMenuTarget({ userType: ut })}
                              disabled={isMutating}
                              className="p-1.5 rounded-lg text-gray-400 hover:text-violet-700 hover:bg-violet-50 dark:hover:bg-violet-900/20 transition-colors disabled:opacity-40"
                            >
                              <LayoutDashboard className="w-3.5 h-3.5" />
                            </button>

                            {/* Edit — disabled for system types */}
                            {!ut.isSystem && (
                              <button
                                title="Edit"
                                onClick={() => openEdit(ut)}
                                disabled={isMutating}
                                className="p-1.5 rounded-lg text-gray-400 hover:text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-900/20 transition-colors disabled:opacity-40"
                              >
                                <Pencil className="w-3.5 h-3.5" />
                              </button>
                            )}

                            {/* Delete — disabled for system types */}
                            {!ut.isSystem && (
                              <button
                                title="Delete"
                                onClick={() => setDeleteTarget(ut)}
                                disabled={isMutating}
                                className="p-1.5 rounded-lg text-gray-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-40"
                              >
                                <Trash2 className="w-3.5 h-3.5" />
                              </button>
                            )}
                          </div>
                        </td>
                      </tr>
                    ))
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Create / Edit modal */}
      <UserTypeModal
        open={modalOpen}
        userType={editType}
        onSave={handleSave}
        onClose={() => { setModalOpen(false); setEditType(null); }}
        isSaving={createMutation.isPending || updateMutation.isPending}
      />

      {/* Permissions modal */}
      <ManagePermissionsModal
        open={!!permTarget}
        userType={permTarget?.userType ?? null}
        allPermissions={allPermissions}
        onSave={handleSavePermissions}
        onClose={() => setPermTarget(null)}
        isSaving={permsMutation.isPending}
      />

      {/* Menus modal */}
      <ManageMenusModal
        open={!!menuTarget}
        userType={menuTarget?.userType ?? null}
        allMenus={allMenus}
        onSave={handleSaveMenus}
        onClose={() => setMenuTarget(null)}
        isSaving={menusMutation.isPending}
      />

      {/* Delete confirm */}
      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete User Type"
        message={`"${deleteTarget?.name}" will be permanently removed. Users assigned this type will lose their role profile.`}
        confirmLabel="Delete"
        variant="danger"
        loading={deleteMutation.isPending}
        error={deleteError}
        onConfirm={handleDeleteConfirm}
        onClose={() => { setDeleteTarget(null); setDeleteError(null); }}
      />
    </div>
  );
}
