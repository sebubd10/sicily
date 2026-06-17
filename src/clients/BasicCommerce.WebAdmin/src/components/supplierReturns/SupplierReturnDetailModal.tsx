import { useState, useRef, useEffect } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import {
  RotateCcw, X, Send, Truck, BadgeDollarSign, XCircle,
  Loader2, AlertCircle, Building2, Store, FileText,
  Plus, Trash2, Package, ChevronDown, Check, CalendarDays,
  CreditCard, Hash,
} from 'lucide-react';
import { cn, extractApiError } from '../../lib/utils';
import {
  RETURN_STATUS_CONFIG, RETURN_REASONS, reasonLabel,
  type SupplierReturnDetail, type SupplierReturnReason,
} from '../../types/supplierReturn';
import {
  useSupplierReturnDetail,
  useAddSupplierReturnItem,
  useRemoveSupplierReturnItem,
  useSubmitSupplierReturn,
  useShipSupplierReturn,
  useCancelSupplierReturn,
  useSetExpectedCredit,
} from '../../hooks/useSupplierReturns';
import { ConfirmDialog } from '../ui/ConfirmDialog';
import { ReceiveCreditModal } from './ReceiveCreditModal';
import { useReceiveCredit } from '../../hooks/useSupplierReturns';

type Product = { id: string; name: string; sku: string };

type Props = {
  open: boolean;
  srId: string | null;
  products: Product[];
  onClose: () => void;
};

const EMPTY_ITEM = { productId: '', quantity: '', unitCost: '', reason: 'Damaged' as SupplierReturnReason, notes: '' };

function fmtDate(d: string | null | undefined) {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}
function fmtMoney(n: number | null | undefined) {
  if (n == null) return '—';
  return n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function InfoCell({ icon, label, value }: { icon: React.ReactNode; label: string; value: string }) {
  return (
    <div>
      <p className="flex items-center gap-1 text-xs text-gray-400 mb-0.5">
        <span className="text-gray-300 dark:text-gray-600">{icon}</span>{label}
      </p>
      <p className="text-sm font-medium text-gray-800 dark:text-gray-200 break-words">{value}</p>
    </div>
  );
}

const WORKFLOW_STEPS = ['Draft', 'Submitted', 'Shipped', 'Credit Received'];

export function SupplierReturnDetailModal({ open, srId, products, onClose }: Props) {
  const { data: sr, isLoading, isError } = useSupplierReturnDetail(srId);

  const addItemMutation       = useAddSupplierReturnItem();
  const removeItemMutation    = useRemoveSupplierReturnItem();
  const submitMutation        = useSubmitSupplierReturn();
  const shipMutation          = useShipSupplierReturn();
  const receiveCreditMutation = useReceiveCredit();
  const cancelMutation        = useCancelSupplierReturn();
  const setExpectedMutation   = useSetExpectedCredit();

  const [itemForm, setItemForm]         = useState(EMPTY_ITEM);
  const [itemErrors, setItemErrors]     = useState<Record<string, string>>({});
  const [addError, setAddError]         = useState<string | null>(null);
  const [actionError, setActionError]   = useState<string | null>(null);
  const [showCancel, setShowCancel]     = useState(false);
  const [cancelError, setCancelError]   = useState<string | null>(null);
  const [showShipConfirm, setShowShipConfirm] = useState(false);
  const [shipError, setShipError]       = useState<string | null>(null);
  const [showReceiveCredit, setShowReceiveCredit] = useState(false);
  const [creditError, setCreditError]   = useState<string | null>(null);
  const [expectedCredit, setExpectedCredit] = useState('');
  const [expectedCreditError, setExpectedCreditError] = useState('');

  // Product search combobox
  const [productQuery, setProductQuery] = useState('');
  const [productOpen, setProductOpen]   = useState(false);
  const productRef = useRef<HTMLDivElement>(null);

  const filteredProducts = products.filter((p) =>
    p.name.toLowerCase().includes(productQuery.toLowerCase()) ||
    p.sku.toLowerCase().includes(productQuery.toLowerCase()),
  );

  useEffect(() => {
    function onClick(e: MouseEvent) {
      if (productRef.current && !productRef.current.contains(e.target as Node))
        setProductOpen(false);
    }
    document.addEventListener('mousedown', onClick);
    return () => document.removeEventListener('mousedown', onClick);
  }, []);

  useEffect(() => {
    if (open) {
      setItemForm(EMPTY_ITEM); setItemErrors({}); setAddError(null);
      setActionError(null); setProductQuery('');
      setExpectedCredit(''); setExpectedCreditError('');
    }
  }, [open, srId]);

  const statusCfg = sr ? RETURN_STATUS_CONFIG[sr.status] : null;
  const isDraft     = sr?.status === 'Draft';
  const isSubmitted = sr?.status === 'Submitted';
  const isShipped   = sr?.status === 'Shipped';
  const isCancelled = sr?.status === 'Cancelled';
  const isCreditReceived = sr?.status === 'CreditReceived';

  const canCancel = isDraft || isSubmitted;
  const isAnyMutating =
    addItemMutation.isPending || removeItemMutation.isPending ||
    submitMutation.isPending || shipMutation.isPending ||
    receiveCreditMutation.isPending || cancelMutation.isPending ||
    setExpectedMutation.isPending;

  const selectedProduct = products.find((p) => p.id === itemForm.productId);
  const currentStep = sr ? RETURN_STATUS_CONFIG[sr.status].step : 0;

  function setItem(k: keyof typeof EMPTY_ITEM, v: string) {
    setItemForm((f) => ({ ...f, [k]: v }));
    setItemErrors((e) => ({ ...e, [k]: '' }));
  }

  function validateItem(): boolean {
    const e: Record<string, string> = {};
    if (!itemForm.productId) e.productId = 'Product is required';
    const qty = parseFloat(itemForm.quantity);
    if (!itemForm.quantity || isNaN(qty) || qty <= 0) e.quantity = 'Enter quantity > 0';
    const cost = parseFloat(itemForm.unitCost);
    if (!itemForm.unitCost || isNaN(cost) || cost < 0) e.unitCost = 'Enter valid unit cost';
    setItemErrors(e);
    return Object.keys(e).length === 0;
  }

  async function handleAddItem(ev: React.FormEvent) {
    ev.preventDefault();
    if (!srId || !validateItem()) return;
    try {
      setAddError(null);
      await addItemMutation.mutateAsync({
        id: srId,
        item: {
          productId: itemForm.productId,
          quantity: parseFloat(itemForm.quantity),
          unitCost: parseFloat(itemForm.unitCost),
          reason: itemForm.reason,
          notes: itemForm.notes || undefined,
        },
      });
      setItemForm(EMPTY_ITEM); setProductQuery('');
    } catch (err) { setAddError(extractApiError(err)); }
  }

  async function handleRemoveItem(productId: string) {
    if (!srId) return;
    try { await removeItemMutation.mutateAsync({ id: srId, productId }); }
    catch (err) { setAddError(extractApiError(err)); }
  }

  async function handleSubmit() {
    if (!srId) return;
    try { setActionError(null); await submitMutation.mutateAsync(srId); }
    catch (err) { setActionError(extractApiError(err)); }
  }

  async function handleShip() {
    if (!srId) return;
    try {
      setShipError(null);
      await shipMutation.mutateAsync(srId);
      setShowShipConfirm(false);
    } catch (err) { setShipError(extractApiError(err)); }
  }

  async function handleCancel() {
    if (!srId) return;
    try {
      setCancelError(null);
      await cancelMutation.mutateAsync(srId);
      setShowCancel(false);
    } catch (err) { setCancelError(extractApiError(err)); }
  }

  async function handleReceiveCredit(creditAmount: number, ref?: string) {
    if (!srId) return;
    try {
      setCreditError(null);
      await receiveCreditMutation.mutateAsync({ id: srId, creditAmount, creditNoteReference: ref });
      setShowReceiveCredit(false);
    } catch (err) { setCreditError(extractApiError(err)); }
  }

  async function handleSetExpectedCredit(ev: React.FormEvent) {
    ev.preventDefault();
    if (!srId) return;
    const n = parseFloat(expectedCredit);
    if (!expectedCredit || isNaN(n) || n < 0) { setExpectedCreditError('Enter a valid amount'); return; }
    try {
      setExpectedCreditError('');
      await setExpectedMutation.mutateAsync({ id: srId, amount: n });
      setExpectedCredit('');
    } catch (err) { setExpectedCreditError(extractApiError(err)); }
  }

  const fieldCls = (err?: string) =>
    cn(
      'w-full rounded-lg border px-3 py-2 text-sm bg-white dark:bg-gray-900',
      'text-gray-900 dark:text-white placeholder-gray-400',
      'focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 transition',
      err ? 'border-red-400 dark:border-red-500' : 'border-gray-200 dark:border-gray-700',
    );

  return (
    <>
      <Dialog.Root open={open} onOpenChange={(o) => !o && !isAnyMutating && onClose()}>
        <Dialog.Portal>
          <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
          <Dialog.Content className={cn(
            'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
            'w-full max-w-5xl rounded-xl bg-white dark:bg-gray-900',
            'border border-gray-200 dark:border-gray-700 shadow-2xl focus:outline-none',
            'data-[state=open]:animate-in data-[state=closed]:animate-out',
            'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
            'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
          )}>
            {/* Header */}
            <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 px-6 py-4">
              <Dialog.Title className="flex items-center gap-2.5 text-base font-semibold text-gray-900 dark:text-white">
                <RotateCcw className="w-5 h-5 text-primary-700 flex-shrink-0" />
                <span>{sr ? `Return · ${sr.returnNumber}` : 'Supplier Return'}</span>
                {statusCfg && (
                  <span className={cn('inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium', statusCfg.color)}>
                    <span className={cn('w-1.5 h-1.5 rounded-full mr-1.5', statusCfg.dot)} />
                    {statusCfg.label}
                  </span>
                )}
              </Dialog.Title>
              <Dialog.Description className="sr-only">Supplier return details and actions</Dialog.Description>
              <Dialog.Close disabled={isAnyMutating} className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none disabled:opacity-40">
                <X className="w-4 h-4" />
              </Dialog.Close>
            </div>

            {/* Body */}
            <div className="max-h-[76vh] overflow-y-auto">
              {isLoading && (
                <div className="flex items-center justify-center py-20 text-gray-400">
                  <Loader2 className="w-6 h-6 animate-spin" />
                </div>
              )}
              {isError && (
                <div className="flex items-center gap-2 px-6 py-12 text-red-500">
                  <AlertCircle className="w-5 h-5 flex-shrink-0" />
                  <span className="text-sm">Failed to load return details.</span>
                </div>
              )}

              {sr && (
                <>
                  {/* Workflow stepper */}
                  {!isCancelled && (
                    <div className="px-6 py-4 border-b border-gray-100 dark:border-gray-800 bg-gray-50/50 dark:bg-gray-800/20">
                      <div className="flex items-center gap-0">
                        {WORKFLOW_STEPS.map((step, i) => {
                          const done = currentStep > i + 1;
                          const active = currentStep === i + 1;
                          return (
                            <div key={step} className="flex items-center flex-1 last:flex-none">
                              <div className="flex flex-col items-center gap-1">
                                <div className={cn(
                                  'w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold border-2 transition-all',
                                  done  ? 'bg-emerald-500 border-emerald-500 text-white'
                                        : active ? 'bg-primary-800 border-primary-800 text-white'
                                               : 'bg-white dark:bg-gray-900 border-gray-300 dark:border-gray-600 text-gray-400',
                                )}>
                                  {done ? <Check className="w-3.5 h-3.5" /> : i + 1}
                                </div>
                                <span className={cn('text-[10px] whitespace-nowrap font-medium',
                                  active ? 'text-primary-700 dark:text-primary-400' : done ? 'text-emerald-600 dark:text-emerald-400' : 'text-gray-400')}>
                                  {step}
                                </span>
                              </div>
                              {i < WORKFLOW_STEPS.length - 1 && (
                                <div className={cn('flex-1 h-0.5 mb-4 mx-1', done ? 'bg-emerald-400' : 'bg-gray-200 dark:bg-gray-700')} />
                              )}
                            </div>
                          );
                        })}
                      </div>
                    </div>
                  )}

                  {isCancelled && (
                    <div className="mx-6 mt-4 flex items-center gap-2 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 px-4 py-3 text-sm text-red-600 dark:text-red-400">
                      <XCircle className="w-4 h-4 flex-shrink-0" />
                      This return has been cancelled.
                    </div>
                  )}

                  {/* Info grid */}
                  <div className="px-6 py-4 border-b border-gray-100 dark:border-gray-800 bg-gray-50/30 dark:bg-gray-800/20">
                    <div className="grid grid-cols-2 sm:grid-cols-3 gap-x-6 gap-y-4">
                      <InfoCell icon={<Building2 className="w-3.5 h-3.5" />} label="Supplier" value={sr.supplierName} />
                      <InfoCell icon={<Store className="w-3.5 h-3.5" />} label="Store" value={String(sr.storeId).slice(0, 8) + '…'} />
                      <InfoCell icon={<CalendarDays className="w-3.5 h-3.5" />} label="Created" value={fmtDate(sr.createdAt)} />
                      {sr.shippedAt && <InfoCell icon={<Truck className="w-3.5 h-3.5" />} label="Shipped" value={fmtDate(sr.shippedAt)} />}
                      {sr.creditReceivedAt && <InfoCell icon={<BadgeDollarSign className="w-3.5 h-3.5" />} label="Credit Received" value={fmtDate(sr.creditReceivedAt)} />}
                      {sr.purchaseOrderId && <InfoCell icon={<Hash className="w-3.5 h-3.5" />} label="PO Reference" value={String(sr.purchaseOrderId)} />}
                      {sr.notes && (
                        <div className="col-span-2 sm:col-span-3">
                          <InfoCell icon={<FileText className="w-3.5 h-3.5" />} label="Notes" value={sr.notes} />
                        </div>
                      )}
                    </div>
                  </div>

                  {/* Credit info (Shipped / CreditReceived) */}
                  {(isShipped || isCreditReceived) && (
                    <div className="px-6 py-4 border-b border-gray-100 dark:border-gray-800">
                      <div className="grid grid-cols-3 gap-4">
                        <div className="rounded-lg bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 p-4">
                          <p className="text-xs text-gray-500 dark:text-gray-400 mb-1">Return Value</p>
                          <p className="text-lg font-bold text-gray-900 dark:text-white">{fmtMoney(sr.totalReturnValue)}</p>
                        </div>
                        <div className="rounded-lg bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 p-4">
                          <p className="text-xs text-blue-600 dark:text-blue-400 mb-1">Expected Credit</p>
                          <p className="text-lg font-bold text-blue-700 dark:text-blue-300">{fmtMoney(sr.expectedCreditAmount)}</p>
                        </div>
                        <div className={cn('rounded-lg border p-4', isCreditReceived
                          ? 'bg-emerald-50 dark:bg-emerald-900/20 border-emerald-200 dark:border-emerald-800'
                          : 'bg-gray-50 dark:bg-gray-800 border-gray-200 dark:border-gray-700')}>
                          <p className={cn('text-xs mb-1', isCreditReceived ? 'text-emerald-600 dark:text-emerald-400' : 'text-gray-500 dark:text-gray-400')}>
                            Actual Credit
                          </p>
                          <p className={cn('text-lg font-bold', isCreditReceived ? 'text-emerald-700 dark:text-emerald-300' : 'text-gray-400')}>
                            {fmtMoney(sr.actualCreditAmount)}
                          </p>
                        </div>
                      </div>
                      {sr.creditNoteReference && (
                        <p className="mt-2 text-xs text-gray-500 dark:text-gray-400">
                          Credit Note: <span className="font-mono font-semibold text-gray-700 dark:text-gray-300">{sr.creditNoteReference}</span>
                        </p>
                      )}
                    </div>
                  )}

                  {/* Expected credit setter (Submitted) */}
                  {isSubmitted && (
                    <div className="px-6 py-4 border-b border-gray-100 dark:border-gray-800">
                      <p className="text-xs font-semibold uppercase tracking-wider text-gray-400 mb-3">Expected Credit</p>
                      <form onSubmit={handleSetExpectedCredit} className="flex items-start gap-3">
                        <div className="flex-1">
                          <input
                            type="number"
                            min={0}
                            step="0.01"
                            value={expectedCredit}
                            onChange={(e) => { setExpectedCredit(e.target.value); setExpectedCreditError(''); }}
                            placeholder={sr.expectedCreditAmount ? fmtMoney(sr.expectedCreditAmount) : 'Enter expected amount…'}
                            disabled={setExpectedMutation.isPending}
                            className={fieldCls(expectedCreditError)}
                          />
                          {expectedCreditError && <p className="mt-1 text-xs text-red-500">{expectedCreditError}</p>}
                        </div>
                        <button
                          type="submit"
                          disabled={setExpectedMutation.isPending || !expectedCredit}
                          className="flex items-center gap-1.5 px-4 py-2 text-sm font-medium text-white bg-blue-700 hover:bg-blue-800 rounded-lg transition-colors disabled:opacity-50"
                        >
                          {setExpectedMutation.isPending ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <CreditCard className="w-3.5 h-3.5" />}
                          Set
                        </button>
                      </form>
                    </div>
                  )}

                  {/* Action error */}
                  {actionError && (
                    <div className="mx-6 mt-4 flex items-center gap-2 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 px-4 py-3 text-sm text-red-600 dark:text-red-400">
                      <AlertCircle className="w-4 h-4 flex-shrink-0" />
                      {actionError}
                    </div>
                  )}

                  {/* Items */}
                  <div className="px-6 py-5">
                    <p className="text-xs font-semibold uppercase tracking-wider text-gray-400 mb-3">
                      Return Items ({sr.items.length})
                    </p>

                    {sr.items.length === 0 ? (
                      <div className="rounded-lg border border-dashed border-gray-200 dark:border-gray-700 py-10 text-center">
                        <Package className="w-8 h-8 text-gray-300 dark:text-gray-600 mx-auto mb-2" />
                        <p className="text-sm text-gray-400">No items added yet.</p>
                        {isDraft && <p className="text-xs text-gray-400 mt-1">Use the form below to add products.</p>}
                      </div>
                    ) : (
                      <div className="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden">
                        <table className="w-full text-sm">
                          <thead>
                            <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                              {['Product', 'SKU', 'Reason', 'Qty', 'Unit Cost', 'Total', 'Notes', ...(isDraft ? [''] : [])].map((h) => (
                                <th key={h} className="px-3 py-2.5 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider whitespace-nowrap">
                                  {h}
                                </th>
                              ))}
                            </tr>
                          </thead>
                          <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                            {sr.items.map((item) => (
                              <tr key={item.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
                                <td className="px-3 py-2.5 font-medium text-gray-900 dark:text-white">{item.productName}</td>
                                <td className="px-3 py-2.5 font-mono text-xs text-gray-500 dark:text-gray-400">{item.productSku}</td>
                                <td className="px-3 py-2.5">
                                  <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-orange-50 text-orange-700 dark:bg-orange-900/20 dark:text-orange-400">
                                    {reasonLabel(item.reason)}
                                  </span>
                                </td>
                                <td className="px-3 py-2.5 text-center text-gray-600 dark:text-gray-400">{item.quantity}</td>
                                <td className="px-3 py-2.5 text-right text-gray-600 dark:text-gray-400">{fmtMoney(item.unitCost)}</td>
                                <td className="px-3 py-2.5 text-right font-semibold text-gray-800 dark:text-gray-200">{fmtMoney(item.totalCost)}</td>
                                <td className="px-3 py-2.5 text-xs text-gray-400 max-w-[120px] truncate">{item.notes ?? '—'}</td>
                                {isDraft && (
                                  <td className="px-3 py-2.5">
                                    <button
                                      onClick={() => handleRemoveItem(item.productId)}
                                      disabled={removeItemMutation.isPending}
                                      className="p-1 rounded text-gray-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-40"
                                      title="Remove item"
                                    >
                                      <Trash2 className="w-3.5 h-3.5" />
                                    </button>
                                  </td>
                                )}
                              </tr>
                            ))}
                          </tbody>
                          <tfoot>
                            <tr className="bg-gray-50 dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700">
                              <td colSpan={isDraft ? 5 : 4} className="px-3 py-2.5 text-sm font-semibold text-gray-700 dark:text-gray-300 text-right">
                                Total Return Value
                              </td>
                              <td className="px-3 py-2.5 text-right font-bold text-gray-900 dark:text-white">
                                {fmtMoney(sr.totalReturnValue)}
                              </td>
                              <td colSpan={isDraft ? 2 : 1} />
                            </tr>
                          </tfoot>
                        </table>
                      </div>
                    )}

                    {/* Add item form (Draft only) */}
                    {isDraft && (
                      <form onSubmit={handleAddItem} noValidate className="mt-4 border border-gray-200 dark:border-gray-700 rounded-lg p-4 bg-gray-50/50 dark:bg-gray-800/30">
                        <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 mb-3 flex items-center gap-1.5">
                          <Plus className="w-3.5 h-3.5" /> Add Item
                        </p>
                        {addError && (
                          <div className="mb-3 flex items-center gap-2 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 px-3 py-2 text-xs text-red-600 dark:text-red-400">
                            <AlertCircle className="w-3.5 h-3.5 flex-shrink-0" />{addError}
                          </div>
                        )}
                        <div className="grid grid-cols-12 gap-3 items-start">
                          {/* Product combobox */}
                          <div className="col-span-4" ref={productRef}>
                            <div className="relative">
                              <button
                                type="button"
                                onClick={() => setProductOpen((o) => !o)}
                                disabled={addItemMutation.isPending}
                                className={cn(
                                  fieldCls(itemErrors.productId),
                                  'flex items-center justify-between text-left',
                                  !selectedProduct && 'text-gray-400',
                                )}
                              >
                                <span className="truncate">{selectedProduct ? selectedProduct.name : 'Search product…'}</span>
                                <ChevronDown className={cn('w-4 h-4 text-gray-400 flex-shrink-0 transition-transform', productOpen && 'rotate-180')} />
                              </button>
                              {productOpen && (
                                <div className="absolute z-10 mt-1 w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 shadow-lg">
                                  <div className="p-2 border-b border-gray-100 dark:border-gray-800">
                                    <input
                                      autoFocus
                                      type="text"
                                      value={productQuery}
                                      onChange={(e) => setProductQuery(e.target.value)}
                                      placeholder="Search…"
                                      className="w-full rounded-md border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 px-3 py-1.5 text-xs text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:ring-1 focus:ring-primary-500"
                                    />
                                  </div>
                                  <ul className="max-h-40 overflow-y-auto py-1">
                                    {filteredProducts.length === 0
                                      ? <li className="px-3 py-2 text-xs text-gray-400">No products found</li>
                                      : filteredProducts.slice(0, 40).map((p) => (
                                          <li
                                            key={p.id}
                                            onClick={() => { setItem('productId', p.id); setProductOpen(false); setProductQuery(''); }}
                                            className={cn(
                                              'flex items-center gap-2 px-3 py-1.5 text-xs cursor-pointer transition-colors',
                                              p.id === itemForm.productId
                                                ? 'bg-primary-50 dark:bg-primary-900/20 text-primary-700 dark:text-primary-400'
                                                : 'text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800',
                                            )}
                                          >
                                            {p.id === itemForm.productId && <Check className="w-3 h-3 flex-shrink-0" />}
                                            <span className={p.id === itemForm.productId ? '' : 'ml-4'}>{p.name}</span>
                                            <span className="ml-auto font-mono text-gray-400">{p.sku}</span>
                                          </li>
                                        ))}
                                  </ul>
                                </div>
                              )}
                            </div>
                            {itemErrors.productId && <p className="mt-1 text-xs text-red-500">{itemErrors.productId}</p>}
                          </div>

                          {/* Reason */}
                          <div className="col-span-2">
                            <select
                              value={itemForm.reason}
                              onChange={(e) => setItem('reason', e.target.value)}
                              disabled={addItemMutation.isPending}
                              className={fieldCls()}
                            >
                              {RETURN_REASONS.map((r) => (
                                <option key={r.value} value={r.value}>{r.label}</option>
                              ))}
                            </select>
                          </div>

                          {/* Quantity */}
                          <div className="col-span-2">
                            <input
                              type="number"
                              min={0.01}
                              step="0.01"
                              value={itemForm.quantity}
                              onChange={(e) => setItem('quantity', e.target.value)}
                              placeholder="Qty"
                              disabled={addItemMutation.isPending}
                              className={fieldCls(itemErrors.quantity)}
                            />
                            {itemErrors.quantity && <p className="mt-1 text-xs text-red-500">{itemErrors.quantity}</p>}
                          </div>

                          {/* Unit cost */}
                          <div className="col-span-2">
                            <input
                              type="number"
                              min={0}
                              step="0.01"
                              value={itemForm.unitCost}
                              onChange={(e) => setItem('unitCost', e.target.value)}
                              placeholder="Unit cost"
                              disabled={addItemMutation.isPending}
                              className={fieldCls(itemErrors.unitCost)}
                            />
                            {itemErrors.unitCost && <p className="mt-1 text-xs text-red-500">{itemErrors.unitCost}</p>}
                          </div>

                          {/* Notes */}
                          <div className="col-span-1">
                            <input
                              type="text"
                              value={itemForm.notes}
                              onChange={(e) => setItem('notes', e.target.value)}
                              placeholder="Notes"
                              disabled={addItemMutation.isPending}
                              className={fieldCls()}
                            />
                          </div>

                          {/* Add button */}
                          <div className="col-span-1">
                            <button
                              type="submit"
                              disabled={addItemMutation.isPending}
                              className="w-full flex items-center justify-center gap-1 px-3 py-2 text-sm font-medium text-white bg-primary-800 hover:bg-primary-900 rounded-lg transition-colors disabled:opacity-50"
                            >
                              {addItemMutation.isPending ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <Plus className="w-3.5 h-3.5" />}
                            </button>
                          </div>
                        </div>
                      </form>
                    )}
                  </div>
                </>
              )}
            </div>

            {/* Footer */}
            {sr && (
              <div className="flex items-center justify-between px-6 py-4 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50 rounded-b-xl">
                <div>
                  {canCancel && (
                    <button
                      onClick={() => setShowCancel(true)}
                      disabled={isAnyMutating}
                      className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-red-600 dark:text-red-400 border border-red-200 dark:border-red-800 rounded-lg hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-40"
                    >
                      <XCircle className="w-4 h-4" />
                      Cancel Return
                    </button>
                  )}
                </div>
                <div className="flex items-center gap-3">
                  <button
                    onClick={onClose}
                    disabled={isAnyMutating}
                    className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors disabled:opacity-40"
                  >
                    Close
                  </button>
                  {isDraft && (
                    <button
                      onClick={handleSubmit}
                      disabled={isAnyMutating || sr.items.length === 0}
                      className="flex items-center gap-2 px-5 py-2 text-sm font-medium text-white bg-blue-700 hover:bg-blue-800 rounded-lg transition-colors disabled:opacity-50"
                      title={sr.items.length === 0 ? 'Add items before submitting' : undefined}
                    >
                      {submitMutation.isPending ? <Loader2 className="w-4 h-4 animate-spin" /> : <Send className="w-4 h-4" />}
                      Submit Return
                    </button>
                  )}
                  {isSubmitted && (
                    <button
                      onClick={() => setShowShipConfirm(true)}
                      disabled={isAnyMutating}
                      className="flex items-center gap-2 px-5 py-2 text-sm font-medium text-white bg-purple-700 hover:bg-purple-800 rounded-lg transition-colors disabled:opacity-50"
                    >
                      <Truck className="w-4 h-4" />
                      Mark as Shipped
                    </button>
                  )}
                  {isShipped && (
                    <button
                      onClick={() => setShowReceiveCredit(true)}
                      disabled={isAnyMutating}
                      className="flex items-center gap-2 px-5 py-2 text-sm font-medium text-white bg-emerald-700 hover:bg-emerald-800 rounded-lg transition-colors disabled:opacity-50"
                    >
                      <BadgeDollarSign className="w-4 h-4" />
                      Receive Credit
                    </button>
                  )}
                </div>
              </div>
            )}
          </Dialog.Content>
        </Dialog.Portal>
      </Dialog.Root>

      {/* Confirm ship */}
      <ConfirmDialog
        open={showShipConfirm}
        title="Mark as Shipped"
        message="Confirm goods have been shipped back to the supplier. This will decrement stock from the store and cannot be undone."
        confirmLabel="Mark Shipped"
        variant="primary"
        loading={shipMutation.isPending}
        error={shipError}
        onConfirm={handleShip}
        onClose={() => { setShowShipConfirm(false); setShipError(null); }}
      />

      {/* Confirm cancel */}
      <ConfirmDialog
        open={showCancel}
        title="Cancel Supplier Return"
        message={sr ? `Cancel return ${sr.returnNumber}? This action cannot be undone.` : 'Cancel this return?'}
        confirmLabel="Cancel Return"
        variant="danger"
        loading={cancelMutation.isPending}
        error={cancelError}
        onConfirm={handleCancel}
        onClose={() => { setShowCancel(false); setCancelError(null); }}
      />

      {/* Receive credit */}
      <ReceiveCreditModal
        open={showReceiveCredit}
        sr={sr ?? null}
        isSaving={receiveCreditMutation.isPending}
        error={creditError}
        onSave={handleReceiveCredit}
        onClose={() => { setShowReceiveCredit(false); setCreditError(null); }}
      />
    </>
  );
}
