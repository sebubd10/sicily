import { useState, useMemo } from 'react';
import {
  Search, Plus, Pencil, PowerOff, Power, Loader2, AlertCircle,
  Warehouse as WarehouseIcon, Package, Activity, Star,
} from 'lucide-react';
import { cn } from '../lib/utils';
import type { Warehouse, WarehouseFormData } from '../types/warehouse';
import { WarehouseModal } from '../components/warehouses/WarehouseModal';
import { WarehouseStockModal } from '../components/warehouses/WarehouseStockModal';
import { WarehouseMovementsModal } from '../components/warehouses/WarehouseMovementsModal';
import { ConfirmDialog } from '../components/ui/ConfirmDialog';
import {
  useWarehouses,
  useStores,
  useCreateWarehouse,
  useUpdateWarehouse,
  useActivateWarehouse,
  useDeactivateWarehouse,
} from '../hooks/useWarehouses';

type ConfirmState =
  | { type: 'deactivate'; warehouse: Warehouse }
  | { type: 'activate';   warehouse: Warehouse }
  | null;

export default function WarehousesPage() {
  const [search, setSearch]         = useState('');
  const [showInactive, setShowInactive] = useState(false);

  const [modalOpen, setModalOpen]   = useState(false);
  const [editWarehouse, setEditWarehouse] = useState<Warehouse | null>(null);

  const [stockWarehouse, setStockWarehouse]           = useState<Warehouse | null>(null);
  const [movementsWarehouse, setMovementsWarehouse]   = useState<Warehouse | null>(null);

  const [confirm, setConfirm]       = useState<ConfirmState>(null);

  const { data: warehouses = [], isLoading, isError, isFetching } = useWarehouses();
  const { data: stores = [] } = useStores();

  const createMutation     = useCreateWarehouse();
  const updateMutation     = useUpdateWarehouse();
  const activateMutation   = useActivateWarehouse();
  const deactivateMutation = useDeactivateWarehouse();

  const isMutating =
    createMutation.isPending ||
    updateMutation.isPending ||
    activateMutation.isPending ||
    deactivateMutation.isPending;

  const filtered = useMemo(() => {
    let list = warehouses;
    if (!showInactive) list = list.filter((w) => w.status === 'Active');
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter(
        (w) =>
          w.name.toLowerCase().includes(q) ||
          w.code.toLowerCase().includes(q) ||
          w.city.toLowerCase().includes(q),
      );
    }
    return list;
  }, [warehouses, search, showInactive]);

  const stats = useMemo(() => ({
    total:   warehouses.length,
    active:  warehouses.filter((w) => w.status === 'Active').length,
    isDefault: warehouses.filter((w) => w.isDefault).length,
  }), [warehouses]);

  function openAdd() {
    setEditWarehouse(null);
    setModalOpen(true);
  }

  function openEdit(w: Warehouse) {
    setEditWarehouse(w);
    setModalOpen(true);
  }

  function handleToggleStatus(w: Warehouse) {
    setConfirm(
      w.status === 'Active'
        ? { type: 'deactivate', warehouse: w }
        : { type: 'activate',   warehouse: w },
    );
  }

  async function handleSave(form: WarehouseFormData) {
    if (editWarehouse) {
      await updateMutation.mutateAsync({ id: editWarehouse.id, form });
    } else {
      await createMutation.mutateAsync(form);
    }
    setModalOpen(false);
    setEditWarehouse(null);
  }

  async function executeConfirm() {
    if (!confirm) return;
    if (confirm.type === 'deactivate') await deactivateMutation.mutateAsync(confirm.warehouse.id);
    if (confirm.type === 'activate')   await activateMutation.mutateAsync(confirm.warehouse.id);
    setConfirm(null);
  }

  const confirmProps = (() => {
    if (!confirm) return null;
    const name = confirm.warehouse.name;
    if (confirm.type === 'deactivate') return {
      title: 'Deactivate Warehouse',
      message: `"${name}" will be marked inactive. Stock transfers to/from it will be blocked.`,
      confirmLabel: 'Deactivate',
      variant: 'warning' as const,
    };
    return {
      title: 'Activate Warehouse',
      message: `"${name}" will be made active again.`,
      confirmLabel: 'Activate',
      variant: 'warning' as const,
    };
  })();

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <WarehouseIcon className="w-6 h-6 text-primary-700" /> Warehouses
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Manage stock warehouses and inventory locations
          </p>
        </div>
        <button
          onClick={openAdd}
          className="flex items-center gap-2 px-4 py-2 bg-primary-800 hover:bg-primary-900 text-white text-sm font-medium rounded-lg transition-colors shadow-sm"
        >
          <Plus className="w-4 h-4" /> Add Warehouse
        </button>
      </div>

      {/* Stats */}
      <div className="flex flex-wrap gap-3">
        {[
          { label: 'Total',   value: stats.total,     color: 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300' },
          { label: 'Active',  value: stats.active,    color: 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300' },
          { label: 'Default', value: stats.isDefault, color: 'bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300' },
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
            placeholder="Search warehouses..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-9 pr-4 py-2 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-gray-900 dark:text-white placeholder-gray-400"
          />
        </div>
        <label className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 cursor-pointer select-none">
          <div
            onClick={() => setShowInactive((v) => !v)}
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
            <span className="text-sm">Failed to load warehouses. Please refresh and try again.</span>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                  {['Code', 'Name', 'City', 'Phone', 'Email', 'Default', 'Status', 'Actions'].map((h) => (
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
                      {Array.from({ length: 8 }).map((_, j) => (
                        <td key={j} className="px-4 py-3">
                          <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-full" />
                        </td>
                      ))}
                    </tr>
                  ))
                ) : filtered.length === 0 ? (
                  <tr>
                    <td colSpan={8} className="px-4 py-12 text-center text-sm text-gray-400">
                      No warehouses found.
                    </td>
                  </tr>
                ) : (
                  filtered.map((w) => (
                    <tr key={w.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                      <td className="px-4 py-3 font-mono text-xs font-medium text-gray-700 dark:text-gray-300 whitespace-nowrap">
                        {w.code}
                      </td>
                      <td className="px-4 py-3 font-medium text-gray-900 dark:text-white whitespace-nowrap">
                        {w.name}
                      </td>
                      <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                        {w.city}
                      </td>
                      <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                        {w.phone ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                      </td>
                      <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                        {w.email ?? <span className="text-gray-300 dark:text-gray-600">—</span>}
                      </td>
                      <td className="px-4 py-3">
                        {w.isDefault && (
                          <span className="inline-flex items-center gap-1 text-xs font-medium px-2 py-0.5 rounded-full bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300">
                            <Star className="w-3 h-3" /> Default
                          </span>
                        )}
                      </td>
                      <td className="px-4 py-3">
                        <span className={cn(
                          'inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium',
                          w.status === 'Active'
                            ? 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300'
                            : 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400',
                        )}>
                          <span className={cn(
                            'w-1.5 h-1.5 rounded-full mr-1.5',
                            w.status === 'Active' ? 'bg-emerald-500' : 'bg-red-400',
                          )} />
                          {w.status}
                        </span>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-1">
                          <button
                            title="View Stock"
                            onClick={() => setStockWarehouse(w)}
                            className="p-1.5 rounded-lg text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors"
                          >
                            <Package className="w-3.5 h-3.5" />
                          </button>
                          <button
                            title="View Movements"
                            onClick={() => setMovementsWarehouse(w)}
                            className="p-1.5 rounded-lg text-gray-400 hover:text-violet-600 hover:bg-violet-50 dark:hover:bg-violet-900/20 transition-colors"
                          >
                            <Activity className="w-3.5 h-3.5" />
                          </button>
                          <button
                            title="Edit"
                            onClick={() => openEdit(w)}
                            disabled={isMutating}
                            className="p-1.5 rounded-lg text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors disabled:opacity-40"
                          >
                            <Pencil className="w-3.5 h-3.5" />
                          </button>
                          <button
                            title={w.status === 'Active' ? 'Deactivate' : 'Activate'}
                            onClick={() => handleToggleStatus(w)}
                            disabled={isMutating}
                            className={cn(
                              'p-1.5 rounded-lg transition-colors disabled:opacity-40',
                              w.status === 'Active'
                                ? 'text-gray-400 hover:text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-900/20'
                                : 'text-gray-400 hover:text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/20',
                            )}
                          >
                            {w.status === 'Active'
                              ? <PowerOff className="w-3.5 h-3.5" />
                              : <Power    className="w-3.5 h-3.5" />}
                          </button>
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

      {/* Create / Edit Modal */}
      <WarehouseModal
        open={modalOpen}
        warehouse={editWarehouse}
        onSave={handleSave}
        onClose={() => { setModalOpen(false); setEditWarehouse(null); }}
        isSaving={createMutation.isPending || updateMutation.isPending}
      />

      {/* Stock Levels Modal */}
      <WarehouseStockModal
        open={!!stockWarehouse}
        warehouse={stockWarehouse}
        stores={stores}
        onClose={() => setStockWarehouse(null)}
      />

      {/* Movements Modal */}
      <WarehouseMovementsModal
        open={!!movementsWarehouse}
        warehouse={movementsWarehouse}
        onClose={() => setMovementsWarehouse(null)}
      />

      {/* Confirm Dialog */}
      {confirmProps && (
        <ConfirmDialog
          open={!!confirm}
          title={confirmProps.title}
          message={confirmProps.message}
          confirmLabel={confirmProps.confirmLabel}
          variant={confirmProps.variant}
          loading={isMutating}
          onConfirm={executeConfirm}
          onClose={() => setConfirm(null)}
        />
      )}
    </div>
  );
}
