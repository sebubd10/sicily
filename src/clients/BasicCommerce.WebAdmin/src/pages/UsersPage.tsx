import { useState } from 'react';
import {
  Users, Search, Plus, Pencil, Power, PowerOff, LockOpen, Tag, Trash2,
  AlertCircle, Loader2, Download,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { User } from '../types/user';
import { exportUsersPdf } from '../api/usersApi';
import { CreateUserModal } from '../components/users/CreateUserModal';
import { EditUserModal } from '../components/users/EditUserModal';
import { AssignUserTypeModal } from '../components/users/AssignUserTypeModal';
import { ConfirmDialog } from '../components/ui/ConfirmDialog';
import { Pagination } from '../components/ui/Pagination';
import {
  useUsers,
  useCreateUser,
  useUserDetail,
  useUpdateUser,
  useActivateUser,
  useDeactivateUser,
  useUnlockUser,
  useAssignUserType,
  useDeleteUser,
} from '../hooks/useUsers';
import { useUserTypes } from '../hooks/useUserTypes';

const PAGE_SIZE_OPTIONS = [10, 20, 50];

type ConfirmState =
  | { type: 'activate';   user: User }
  | { type: 'deactivate'; user: User }
  | { type: 'unlock';     user: User }
  | { type: 'delete';     user: User }
  | null;

const ROLE_COLORS: Record<string, string> = {
  SystemAdmin:  'bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-300',
  ChainAdmin:   'bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300',
  StoreManager: 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300',
  Supervisor:   'bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300',
  Cashier:      'bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400',
};

export default function UsersPage() {
  const [search, setSearch]     = useState('');
  const [page, setPage]         = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [showInactive, setShowInactive] = useState(false);

  const [createOpen, setCreateOpen] = useState(false);
  const [editId, setEditId]         = useState<string | null>(null);
  const [assignUser, setAssignUser] = useState<User | null>(null);
  const [confirm, setConfirm]       = useState<ConfirmState>(null);
  const [isExporting, setIsExporting] = useState(false);

  const { data, isLoading, isError, isFetching } = useUsers({
    page,
    pageSize,
    search: search.trim() || undefined,
    includeInactive: showInactive,
  });

  const { data: userTypes = [] } = useUserTypes();

  // Fetch full detail when an edit ID is set; opens the modal once data arrives
  const { data: editUser, isFetching: isFetchingEdit } = useUserDetail(editId);

  const createMutation     = useCreateUser();
  const updateMutation     = useUpdateUser();
  const activateMutation   = useActivateUser();
  const deactivateMutation = useDeactivateUser();
  const unlockMutation     = useUnlockUser();
  const assignMutation     = useAssignUserType();
  const deleteMutation     = useDeleteUser();

  const [confirmError, setConfirmError] = useState<string | null>(null);

  const isMutating =
    createMutation.isPending ||
    updateMutation.isPending ||
    activateMutation.isPending ||
    deactivateMutation.isPending ||
    unlockMutation.isPending ||
    assignMutation.isPending ||
    deleteMutation.isPending;

  const rows     = data?.items ?? [];
  const total    = data?.totalCount ?? 0;
  const totPages = data?.totalPages ?? 1;

  async function handleExportPdf() {
    setIsExporting(true);
    try {
      await exportUsersPdf({ search: search.trim() || undefined });
    } finally {
      setIsExporting(false);
    }
  }

  async function handleCreate(form: Parameters<typeof createMutation.mutateAsync>[0]) {
    await createMutation.mutateAsync(form);
    setCreateOpen(false);
  }

  async function handleEdit(form: Parameters<typeof updateMutation.mutateAsync>[0]['form']) {
    if (!editId) return;
    await updateMutation.mutateAsync({ id: editId, form });
    setEditId(null);
  }

  async function handleAssign(userTypeId: string | null) {
    if (!assignUser) return;
    await assignMutation.mutateAsync({ id: assignUser.id, userTypeId });
    setAssignUser(null);
  }

  async function executeConfirm() {
    if (!confirm) return;
    try {
      setConfirmError(null);
      if (confirm.type === 'activate')   await activateMutation.mutateAsync(confirm.user.id);
      if (confirm.type === 'deactivate') await deactivateMutation.mutateAsync(confirm.user.id);
      if (confirm.type === 'unlock')     await unlockMutation.mutateAsync(confirm.user.id);
      if (confirm.type === 'delete')     await deleteMutation.mutateAsync(confirm.user.id);
      setConfirm(null);
    } catch (err) {
      setConfirmError(extractApiError(err));
    }
  }

  const confirmProps = (() => {
    if (!confirm) return null;
    const name = confirm.user.fullName;
    if (confirm.type === 'deactivate') return {
      title: 'Deactivate User',
      message: `"${name}" will no longer be able to log in.`,
      confirmLabel: 'Deactivate',
      variant: 'warning' as const,
    };
    if (confirm.type === 'activate') return {
      title: 'Activate User',
      message: `"${name}" will be able to log in again.`,
      confirmLabel: 'Activate',
      variant: 'warning' as const,
    };
    if (confirm.type === 'delete') return {
      title: 'Delete User',
      message: `"${name}" will be permanently removed. Their audit history is preserved, but they will not be able to log in. This cannot be undone.`,
      confirmLabel: 'Delete',
      variant: 'danger' as const,
    };
    return {
      title: 'Unlock Account',
      message: `Reset failed login attempts and remove the lock on "${name}".`,
      confirmLabel: 'Unlock',
      variant: 'warning' as const,
    };
  })();

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <Users className="w-6 h-6 text-primary-700" /> Users
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Manage staff accounts and role assignments
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={handleExportPdf}
            disabled={isExporting}
            title={search ? 'Export current search as PDF' : 'Export all users as PDF'}
            className="flex items-center gap-2 px-4 py-2 border border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 text-sm font-medium rounded-lg transition-colors disabled:opacity-50"
          >
            {isExporting
              ? <Loader2 className="w-4 h-4 animate-spin" />
              : <Download className="w-4 h-4" />}
            {isExporting ? 'Generating…' : 'Export PDF'}
          </button>
          <button
            onClick={() => setCreateOpen(true)}
            className="flex items-center gap-2 px-4 py-2 bg-primary-800 hover:bg-primary-900 text-white text-sm font-medium rounded-lg transition-colors shadow-sm"
          >
            <Plus className="w-4 h-4" /> Add User
          </button>
        </div>
      </div>

      {/* Summary chips */}
      <div className="flex flex-wrap gap-3">
        {[
          { label: 'Total',    value: total,                                                        color: 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300' },
          { label: 'Active',   value: rows.filter((u) => u.status === 'Active').length,             color: 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300' },
          { label: 'Inactive', value: rows.filter((u) => u.status === 'Inactive').length,           color: 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400' },
          { label: 'Locked',   value: rows.filter((u) => u.isLocked).length,                       color: 'bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300' },
        ].map((s) => (
          <span key={s.label} className={cn('px-3 py-1 rounded-full text-sm font-medium', s.color)}>
            {s.label}: <strong>{s.value}</strong>
          </span>
        ))}
      </div>

      {/* Toolbar */}
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            placeholder="Search by name, email, or code..."
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="w-full pl-9 pr-4 py-2 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-gray-900 dark:text-white placeholder-gray-400"
          />
        </div>
        <label className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 cursor-pointer select-none">
          <div
            onClick={() => { setShowInactive((v) => !v); setPage(1); }}
            className={cn(
              'w-9 h-5 rounded-full transition-colors relative',
              showInactive ? 'bg-primary-700' : 'bg-gray-300 dark:bg-gray-600',
            )}
          >
            <span className={cn(
              'absolute top-0.5 w-4 h-4 rounded-full bg-white shadow transition-all',
              showInactive ? 'left-4' : 'left-0.5',
            )} />
          </div>
          Show inactive
        </label>
        {isFetching && !isLoading && (
          <Loader2 className="w-4 h-4 animate-spin text-gray-400 self-center" />
        )}
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
        {isError ? (
          <div className="flex items-center gap-2 px-6 py-12 text-red-500">
            <AlertCircle className="w-5 h-5 flex-shrink-0" />
            <span className="text-sm">Failed to load users. Please refresh and try again.</span>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                    {['#', 'Code', 'Full Name', 'Email', 'Role', 'User Type', 'Store', 'Status', 'Last Login', 'Actions'].map((h) => (
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
                    Array.from({ length: 5 }).map((_, i) => (
                      <tr key={i} className="animate-pulse">
                        {Array.from({ length: 10 }).map((_, j) => (
                          <td key={j} className="px-4 py-3">
                            <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-full" />
                          </td>
                        ))}
                      </tr>
                    ))
                  ) : rows.length === 0 ? (
                    <tr>
                      <td colSpan={10} className="px-4 py-12 text-center text-sm text-gray-400">
                        No users found.
                      </td>
                    </tr>
                  ) : (
                    rows.map((user, idx) => (
                      <tr key={user.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                        <td className="px-4 py-3 text-gray-400">{(page - 1) * pageSize + idx + 1}</td>
                        <td className="px-4 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">{user.employeeCode}</td>
                        <td className="px-4 py-3 font-medium text-gray-900 dark:text-white whitespace-nowrap">
                          {user.fullName}
                          {user.isLocked && (
                            <span className="ml-2 text-xs px-1.5 py-0.5 bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300 rounded font-medium">Locked</span>
                          )}
                        </td>
                        <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{user.email}</td>
                        <td className="px-4 py-3">
                          <span className={cn(
                            'px-2 py-0.5 rounded-full text-xs font-medium',
                            ROLE_COLORS[user.role] ?? ROLE_COLORS.Cashier,
                          )}>
                            {user.role}
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          {user.userTypeName ? (
                            <span className="text-xs text-gray-700 dark:text-gray-300 font-medium">{user.userTypeName}</span>
                          ) : (
                            <span className="text-gray-300 dark:text-gray-600 text-xs">—</span>
                          )}
                        </td>
                        <td className="px-4 py-3 text-gray-500 dark:text-gray-400 text-xs whitespace-nowrap">
                          {user.storeName ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                        </td>
                        <td className="px-4 py-3">
                          <span className={cn(
                            'inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium',
                            user.status === 'Active'
                              ? 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300'
                              : 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400',
                          )}>
                            <span className={cn(
                              'w-1.5 h-1.5 rounded-full mr-1.5',
                              user.status === 'Active' ? 'bg-emerald-500' : 'bg-red-400',
                            )} />
                            {user.status}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">
                          {user.lastLoginAt
                            ? new Date(user.lastLoginAt).toLocaleDateString()
                            : <span className="text-gray-300 dark:text-gray-600">Never</span>}
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-1">
                            {/* Edit */}
                            <button
                              title="Edit"
                              onClick={() => setEditId(user.id)}
                              disabled={isMutating || isFetchingEdit}
                              className="p-1.5 rounded-lg text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors disabled:opacity-40"
                            >
                              {isFetchingEdit && editId === user.id
                                ? <Loader2 className="w-3.5 h-3.5 animate-spin" />
                                : <Pencil className="w-3.5 h-3.5" />}
                            </button>

                            {/* Assign user type */}
                            <button
                              title="Assign User Type"
                              onClick={() => setAssignUser(user)}
                              disabled={isMutating}
                              className="p-1.5 rounded-lg text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors disabled:opacity-40"
                            >
                              <Tag className="w-3.5 h-3.5" />
                            </button>

                            {/* Activate / Deactivate */}
                            <button
                              title={user.status === 'Active' ? 'Deactivate' : 'Activate'}
                              onClick={() =>
                                setConfirm(
                                  user.status === 'Active'
                                    ? { type: 'deactivate', user }
                                    : { type: 'activate', user },
                                )
                              }
                              disabled={isMutating}
                              className={cn(
                                'p-1.5 rounded-lg transition-colors disabled:opacity-40',
                                user.status === 'Active'
                                  ? 'text-gray-400 hover:text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-900/20'
                                  : 'text-gray-400 hover:text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/20',
                              )}
                            >
                              {user.status === 'Active'
                                ? <PowerOff className="w-3.5 h-3.5" />
                                : <Power    className="w-3.5 h-3.5" />}
                            </button>

                            {/* Unlock */}
                            {user.isLocked && (
                              <button
                                title="Unlock Account"
                                onClick={() => setConfirm({ type: 'unlock', user })}
                                disabled={isMutating}
                                className="p-1.5 rounded-lg text-gray-400 hover:text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-900/20 transition-colors disabled:opacity-40"
                              >
                                <LockOpen className="w-3.5 h-3.5" />
                              </button>
                            )}

                            {/* Delete */}
                            <button
                              title="Delete User"
                              onClick={() => setConfirm({ type: 'delete', user })}
                              disabled={isMutating}
                              className="p-1.5 rounded-lg text-gray-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-40"
                            >
                              <Trash2 className="w-3.5 h-3.5" />
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>

            <Pagination
              page={page}
              totalPages={totPages}
              totalItems={total}
              pageSize={pageSize}
              pageSizeOptions={PAGE_SIZE_OPTIONS}
              onPageChange={setPage}
              onPageSizeChange={(s) => { setPageSize(s); setPage(1); }}
            />
          </>
        )}
      </div>

      {/* Create user modal */}
      <CreateUserModal
        open={createOpen}
        onSave={handleCreate}
        onClose={() => setCreateOpen(false)}
        isSaving={createMutation.isPending}
      />

      {/* Edit user modal */}
      <EditUserModal
        open={!!editId && !!editUser}
        user={editUser ?? null}
        onSave={handleEdit}
        onClose={() => setEditId(null)}
        isSaving={updateMutation.isPending}
      />

      {/* Assign user type modal */}
      <AssignUserTypeModal
        open={!!assignUser}
        user={assignUser}
        userTypes={userTypes}
        onSave={handleAssign}
        onClose={() => setAssignUser(null)}
        isSaving={assignMutation.isPending}
      />

      {/* Confirm dialog */}
      {confirmProps && (
        <ConfirmDialog
          open={!!confirm}
          title={confirmProps.title}
          message={confirmProps.message}
          confirmLabel={confirmProps.confirmLabel}
          variant={confirmProps.variant}
          loading={isMutating}
          error={confirmError}
          onConfirm={executeConfirm}
          onClose={() => { setConfirm(null); setConfirmError(null); }}
        />
      )}
    </div>
  );
}
