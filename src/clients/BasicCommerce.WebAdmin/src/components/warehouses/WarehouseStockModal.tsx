import { useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { Package, X, ArrowRightLeft, AlertTriangle, Loader2, Search } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { Warehouse, WarehouseStockLevel, Store } from '../../types/warehouse';
import { TransferToStoreModal } from './TransferToStoreModal';
import { useWarehouseStock, useTransferToStore } from '../../hooks/useWarehouses';

type Props = {
  open: boolean;
  warehouse: Warehouse | null;
  stores: Store[];
  onClose: () => void;
};

export function WarehouseStockModal({ open, warehouse, stores, onClose }: Props) {
  const [search, setSearch]         = useState('');
  const [transferItem, setTransferItem] = useState<WarehouseStockLevel | null>(null);

  const { data: stock = [], isLoading, isError } = useWarehouseStock(
    open && warehouse ? warehouse.id : null,
  );

  const transferMutation = useTransferToStore();

  const filtered = search.trim()
    ? stock.filter(
        (s) =>
          s.productName.toLowerCase().includes(search.toLowerCase()) ||
          s.sku.toLowerCase().includes(search.toLowerCase()),
      )
    : stock;

  const lowStockCount = stock.filter((s) => s.isLowStock).length;

  async function handleTransfer(storeId: string, quantity: number, notes: string) {
    if (!warehouse || !transferItem) return;
    await transferMutation.mutateAsync({
      warehouseId: warehouse.id,
      storeId,
      productId: transferItem.productId,
      quantity,
      notes,
    });
    setTransferItem(null);
  }

  return (
    <>
      <Dialog.Root open={open} onOpenChange={(o) => !o && onClose()}>
        <Dialog.Portal>
          <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
          <Dialog.Content
            className={cn(
              'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
              'w-full max-w-4xl rounded-xl bg-white dark:bg-gray-900',
              'border border-gray-200 dark:border-gray-700 shadow-xl focus:outline-none flex flex-col',
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
                <Package className="w-5 h-5 text-primary-700" />
                Stock Levels
                {warehouse && (
                  <span className="text-sm font-normal text-gray-500 dark:text-gray-400">
                    — {warehouse.name}
                  </span>
                )}
              </Dialog.Title>
              <div className="flex items-center gap-3">
                {lowStockCount > 0 && (
                  <span className="flex items-center gap-1.5 text-xs font-medium px-2 py-1 rounded-full bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300">
                    <AlertTriangle className="w-3.5 h-3.5" />
                    {lowStockCount} low stock
                  </span>
                )}
                <Dialog.Close className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none">
                  <X className="w-4 h-4" />
                </Dialog.Close>
              </div>
            </div>

            {/* Toolbar */}
            <div className="px-6 py-3 border-b border-gray-100 dark:border-gray-800 flex-shrink-0">
              <div className="relative max-w-xs">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-gray-400" />
                <input
                  type="text"
                  placeholder="Search products…"
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  className="w-full pl-8 pr-3 py-1.5 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-gray-900 dark:text-white placeholder-gray-400"
                />
              </div>
            </div>

            {/* Table */}
            <div className="flex-1 overflow-y-auto min-h-0">
              {isLoading ? (
                <div className="flex items-center justify-center py-16">
                  <Loader2 className="w-6 h-6 animate-spin text-gray-400" />
                </div>
              ) : isError ? (
                <div className="px-6 py-12 text-center text-sm text-red-500">Failed to load stock levels.</div>
              ) : filtered.length === 0 ? (
                <div className="px-6 py-12 text-center text-sm text-gray-400">
                  {stock.length === 0 ? 'No stock recorded for this warehouse yet.' : 'No products match your search.'}
                </div>
              ) : (
                <table className="w-full text-sm">
                  <thead className="sticky top-0 bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                    <tr>
                      {['SKU', 'Product', 'In Stock', 'Reserved', 'Available', 'Low Stock At', 'Status', 'Actions'].map((h) => (
                        <th key={h} className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider whitespace-nowrap">{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                    {filtered.map((item) => (
                      <tr key={item.productId} className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                        <td className="px-4 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">{item.sku}</td>
                        <td className="px-4 py-3 font-medium text-gray-900 dark:text-white">{item.productName}</td>
                        <td className="px-4 py-3 text-right text-gray-700 dark:text-gray-300">{item.quantity}</td>
                        <td className="px-4 py-3 text-right text-gray-500 dark:text-gray-400">{item.reservedQuantity}</td>
                        <td className="px-4 py-3 text-right font-medium text-emerald-700 dark:text-emerald-300">{item.availableQuantity}</td>
                        <td className="px-4 py-3 text-right text-gray-500 dark:text-gray-400">{item.lowStockThreshold}</td>
                        <td className="px-4 py-3">
                          {item.isLowStock ? (
                            <span className="inline-flex items-center gap-1 text-xs font-medium px-2 py-0.5 rounded-full bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300">
                              <AlertTriangle className="w-3 h-3" /> Low
                            </span>
                          ) : (
                            <span className="text-xs text-emerald-600 dark:text-emerald-400 font-medium">OK</span>
                          )}
                        </td>
                        <td className="px-4 py-3">
                          <button
                            title="Transfer to Store"
                            onClick={() => setTransferItem(item)}
                            disabled={item.availableQuantity <= 0}
                            className="flex items-center gap-1.5 px-2 py-1 text-xs font-medium rounded-lg text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
                          >
                            <ArrowRightLeft className="w-3.5 h-3.5" />
                            Transfer
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>

            {/* Footer */}
            <div className="px-6 py-3 border-t border-gray-100 dark:border-gray-800 flex-shrink-0 text-xs text-gray-400">
              {filtered.length} of {stock.length} products
            </div>
          </Dialog.Content>
        </Dialog.Portal>
      </Dialog.Root>

      {/* Transfer modal (z-[60] so it layers above this modal) */}
      <TransferToStoreModal
        open={!!transferItem}
        warehouseId={warehouse?.id ?? ''}
        stockItem={transferItem}
        stores={stores}
        onSave={handleTransfer}
        onClose={() => setTransferItem(null)}
        isSaving={transferMutation.isPending}
      />
    </>
  );
}
