import { useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import {
  ClipboardList, X, Send, XCircle, PackageCheck,
  Loader2, AlertCircle, Building2, Warehouse, CreditCard,
  CalendarDays, FileText, Pencil,
} from 'lucide-react';
import { cn, extractApiError } from '../../lib/utils';
import { PO_STATUS_CONFIG } from '../../types/purchaseOrder';
import {
  usePurchaseOrderDetail,
  useSubmitPurchaseOrder,
  useCancelPurchaseOrder,
} from '../../hooks/usePurchaseOrders';
import { ConfirmDialog } from '../ui/ConfirmDialog';

type Props = {
  open: boolean;
  poId: string | null;
  onClose: () => void;
  onReceive: (poId: string) => void;
  onEdit?: (poId: string) => void;
};

function fmtDate(d: string | null | undefined): string {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

function fmtMoney(amount: number, currency: string): string {
  return `${currency} ${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

function InfoCell({ icon, label, value }: { icon: React.ReactNode; label: string; value: string }) {
  return (
    <div>
      <p className="flex items-center gap-1 text-xs text-gray-400 mb-0.5">
        <span className="text-gray-300 dark:text-gray-600">{icon}</span>
        {label}
      </p>
      <p className="text-sm font-medium text-gray-800 dark:text-gray-200 break-words">{value}</p>
    </div>
  );
}

export function PurchaseOrderDetailModal({ open, poId, onClose, onReceive, onEdit }: Props) {
  const [showCancelConfirm, setShowCancelConfirm] = useState(false);
  const [cancelError, setCancelError] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const { data: po, isLoading, isError } = usePurchaseOrderDetail(poId);
  const submitMutation = useSubmitPurchaseOrder();
  const cancelMutation = useCancelPurchaseOrder();

  const isMutating = submitMutation.isPending || cancelMutation.isPending;

  const statusCfg = po ? PO_STATUS_CONFIG[po.status] : null;
  const canEdit = po?.status === 'Draft';
  const canSubmit = po?.status === 'Draft';
  const canReceive = po?.status === 'Submitted' || po?.status === 'PartiallyReceived';
  const canCancel = po?.status === 'Draft' || po?.status === 'Submitted' || po?.status === 'PartiallyReceived';
  const hasActions = canSubmit || canReceive || canCancel;

  async function handleSubmit() {
    if (!poId) return;
    try {
      setSubmitError(null);
      await submitMutation.mutateAsync(poId);
    } catch (err) {
      setSubmitError(extractApiError(err));
    }
  }

  async function handleCancel() {
    if (!poId) return;
    try {
      setCancelError(null);
      await cancelMutation.mutateAsync(poId);
      setShowCancelConfirm(false);
    } catch (err) {
      setCancelError(extractApiError(err));
    }
  }

  return (
    <>
      <Dialog.Root open={open} onOpenChange={(o) => !o && !isMutating && onClose()}>
        <Dialog.Portal>
          <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
          <Dialog.Content
            className={cn(
              'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
              'w-full max-w-4xl rounded-xl bg-white dark:bg-gray-900',
              'border border-gray-200 dark:border-gray-700 shadow-2xl focus:outline-none',
              'data-[state=open]:animate-in data-[state=closed]:animate-out',
              'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
              'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
            )}
          >
            {/* Header */}
            <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 px-6 py-4">
              <Dialog.Title className="flex items-center gap-2.5 text-base font-semibold text-gray-900 dark:text-white">
                <ClipboardList className="w-5 h-5 text-primary-700 flex-shrink-0" />
                <span>{po ? `Purchase Order · ${po.orderNumber}` : 'Purchase Order'}</span>
                {statusCfg && (
                  <span className={cn('inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium', statusCfg.color)}>
                    <span className={cn('w-1.5 h-1.5 rounded-full mr-1.5', statusCfg.dot)} />
                    {statusCfg.label}
                  </span>
                )}
              </Dialog.Title>
              <Dialog.Description className="sr-only">Purchase order details and actions</Dialog.Description>
              <Dialog.Close
                disabled={isMutating}
                className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none disabled:opacity-40"
              >
                <X className="w-4 h-4" />
              </Dialog.Close>
            </div>

            {/* Body */}
            <div className="max-h-[72vh] overflow-y-auto">
              {isLoading && (
                <div className="flex items-center justify-center py-20 text-gray-400">
                  <Loader2 className="w-6 h-6 animate-spin" />
                </div>
              )}

              {isError && (
                <div className="flex items-center gap-2 px-6 py-12 text-red-500">
                  <AlertCircle className="w-5 h-5 flex-shrink-0" />
                  <span className="text-sm">Failed to load purchase order details. Please try again.</span>
                </div>
              )}

              {po && (
                <>
                  {/* Info grid */}
                  <div className="px-6 py-4 border-b border-gray-100 dark:border-gray-800 bg-gray-50/50 dark:bg-gray-800/30">
                    <div className="grid grid-cols-2 sm:grid-cols-3 gap-x-6 gap-y-4">
                      <InfoCell icon={<Building2 className="w-3.5 h-3.5" />} label="Supplier" value={po.supplierName} />
                      <InfoCell icon={<Warehouse className="w-3.5 h-3.5" />} label="Warehouse" value={po.warehouseName} />
                      <InfoCell icon={<CreditCard className="w-3.5 h-3.5" />} label="Currency" value={po.currency} />
                      <InfoCell icon={<CalendarDays className="w-3.5 h-3.5" />} label="Order Date" value={fmtDate(po.orderDate)} />
                      <InfoCell icon={<CalendarDays className="w-3.5 h-3.5" />} label="Expected Delivery" value={fmtDate(po.expectedDate)} />
                      {po.receivedDate && (
                        <InfoCell icon={<CalendarDays className="w-3.5 h-3.5" />} label="Received Date" value={fmtDate(po.receivedDate)} />
                      )}
                      {po.notes && (
                        <div className="col-span-2 sm:col-span-3">
                          <InfoCell icon={<FileText className="w-3.5 h-3.5" />} label="Notes" value={po.notes} />
                        </div>
                      )}
                    </div>
                  </div>

                  {/* Submit error banner */}
                  {submitError && (
                    <div className="mx-6 mt-4 flex items-center gap-2 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 px-4 py-3 text-sm text-red-600 dark:text-red-400">
                      <AlertCircle className="w-4 h-4 flex-shrink-0" />
                      {submitError}
                    </div>
                  )}

                  {/* Items table */}
                  <div className="px-6 py-5">
                    <p className="text-xs font-semibold uppercase tracking-wider text-gray-400 mb-3">
                      Line Items ({po.items.length})
                    </p>
                    <div className="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden">
                      <table className="w-full text-sm">
                        <thead>
                          <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                            {['Product', 'SKU', 'Ordered', 'Received', 'Remaining', 'Unit Cost', 'Total', 'Progress'].map((h) => (
                              <th key={h} className="px-3 py-2.5 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider whitespace-nowrap">
                                {h}
                              </th>
                            ))}
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                          {po.items.map((item) => {
                            const pct = item.orderedQuantity > 0
                              ? Math.round((item.receivedQuantity / item.orderedQuantity) * 100)
                              : 0;
                            return (
                              <tr key={item.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
                                <td className="px-3 py-2.5 font-medium text-gray-900 dark:text-white">{item.productName}</td>
                                <td className="px-3 py-2.5 font-mono text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">{item.sku}</td>
                                <td className="px-3 py-2.5 text-center text-gray-600 dark:text-gray-400">{item.orderedQuantity}</td>
                                <td className="px-3 py-2.5 text-center font-medium text-emerald-600 dark:text-emerald-400">{item.receivedQuantity}</td>
                                <td className="px-3 py-2.5 text-center text-amber-600 dark:text-amber-400">{item.remainingQuantity}</td>
                                <td className="px-3 py-2.5 text-right text-gray-600 dark:text-gray-400 whitespace-nowrap">
                                  {fmtMoney(item.unitCost, po.currency)}
                                </td>
                                <td className="px-3 py-2.5 text-right font-semibold text-gray-800 dark:text-gray-200 whitespace-nowrap">
                                  {fmtMoney(item.totalCost, po.currency)}
                                </td>
                                <td className="px-3 py-2.5 w-32">
                                  <div className="flex items-center gap-2">
                                    <div className="flex-1 h-1.5 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
                                      <div
                                        className={cn(
                                          'h-full rounded-full transition-all',
                                          item.isFullyReceived
                                            ? 'bg-emerald-500'
                                            : pct > 0
                                              ? 'bg-amber-500'
                                              : 'bg-gray-300 dark:bg-gray-600',
                                        )}
                                        style={{ width: `${pct}%` }}
                                      />
                                    </div>
                                    <span className="text-xs text-gray-400 w-8 text-right tabular-nums">{pct}%</span>
                                  </div>
                                </td>
                              </tr>
                            );
                          })}
                        </tbody>
                        <tfoot>
                          <tr className="bg-gray-50 dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700">
                            <td colSpan={6} className="px-3 py-2.5 text-sm font-semibold text-gray-700 dark:text-gray-300 text-right">
                              Order Total
                            </td>
                            <td className="px-3 py-2.5 text-right font-bold text-gray-900 dark:text-white whitespace-nowrap">
                              {fmtMoney(po.totalAmount, po.currency)}
                            </td>
                            <td />
                          </tr>
                        </tfoot>
                      </table>
                    </div>
                  </div>
                </>
              )}
            </div>

            {/* Footer */}
            <div className="flex items-center justify-between px-6 py-4 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50 rounded-b-xl">
              <div>
                {po && canCancel && (
                  <button
                    onClick={() => setShowCancelConfirm(true)}
                    disabled={isMutating}
                    className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-red-600 dark:text-red-400 border border-red-200 dark:border-red-800 rounded-lg hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-40"
                  >
                    <XCircle className="w-4 h-4" />
                    Cancel Order
                  </button>
                )}
              </div>
              <div className="flex items-center gap-3">
                <button
                  onClick={onClose}
                  disabled={isMutating}
                  className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors disabled:opacity-40"
                >
                  Close
                </button>
                {po && canEdit && onEdit && poId && (
                  <button
                    onClick={() => { onClose(); onEdit(poId); }}
                    disabled={isMutating}
                    className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors disabled:opacity-40"
                  >
                    <Pencil className="w-4 h-4" />
                    Edit
                  </button>
                )}
                {po && canSubmit && (
                  <button
                    onClick={handleSubmit}
                    disabled={isMutating}
                    className="flex items-center gap-2 px-5 py-2 text-sm font-medium text-white bg-primary-800 hover:bg-primary-900 rounded-lg transition-colors disabled:opacity-50"
                  >
                    {submitMutation.isPending
                      ? <Loader2 className="w-4 h-4 animate-spin" />
                      : <Send className="w-4 h-4" />}
                    Submit Order
                  </button>
                )}
                {po && canReceive && poId && (
                  <button
                    onClick={() => onReceive(poId)}
                    disabled={isMutating}
                    className="flex items-center gap-2 px-5 py-2 text-sm font-medium text-white bg-emerald-700 hover:bg-emerald-800 rounded-lg transition-colors disabled:opacity-50"
                  >
                    <PackageCheck className="w-4 h-4" />
                    Receive Items
                  </button>
                )}
              </div>
            </div>
          </Dialog.Content>
        </Dialog.Portal>
      </Dialog.Root>

      <ConfirmDialog
        open={showCancelConfirm}
        title="Cancel Purchase Order"
        message={po ? `Cancel order ${po.orderNumber}? This action cannot be undone.` : 'Cancel this purchase order?'}
        confirmLabel="Cancel Order"
        variant="danger"
        loading={cancelMutation.isPending}
        error={cancelError}
        onConfirm={handleCancel}
        onClose={() => { setShowCancelConfirm(false); setCancelError(null); }}
      />
    </>
  );
}
