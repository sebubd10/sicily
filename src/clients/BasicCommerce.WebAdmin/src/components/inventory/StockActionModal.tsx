import { useState, useEffect } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import {
  X, Plus, SlidersHorizontal, Trash2, Flag,
  Loader2, AlertCircle, Package,
} from 'lucide-react';
import { cn } from '../../lib/utils';
import type { StockLevel, StockActionType } from '../../types/inventory';

type Props = {
  open: boolean;
  type: StockActionType;
  storeId: string;
  item: StockLevel | null;
  isSaving: boolean;
  error: string | null;
  onSave: (payload: ActionPayload) => void;
  onClose: () => void;
};

export type ActionPayload =
  | { type: 'receive';    quantity: number; reference?: string; notes?: string }
  | { type: 'adjust';     newQuantity: number; notes: string }
  | { type: 'writeOff';   quantity: number; reason: string }
  | { type: 'threshold';  threshold: number };

const ACTION_META: Record<StockActionType, {
  title: string;
  icon: React.ReactNode;
  btnColor: string;
  btnLabel: string;
}> = {
  receive:   { title: 'Receive Stock',          icon: <Plus className="w-5 h-5 text-emerald-600" />,        btnColor: 'bg-emerald-600 hover:bg-emerald-700',        btnLabel: 'Receive' },
  adjust:    { title: 'Adjust Stock Count',     icon: <SlidersHorizontal className="w-5 h-5 text-blue-600" />, btnColor: 'bg-blue-600 hover:bg-blue-700',           btnLabel: 'Save Adjustment' },
  writeOff:  { title: 'Write Off Stock',        icon: <Trash2 className="w-5 h-5 text-red-600" />,           btnColor: 'bg-red-600 hover:bg-red-700',              btnLabel: 'Write Off' },
  threshold: { title: 'Set Low Stock Threshold',icon: <Flag className="w-5 h-5 text-amber-500" />,           btnColor: 'bg-amber-500 hover:bg-amber-600',          btnLabel: 'Set Threshold' },
};

const ICON_WRAP: Record<StockActionType, string> = {
  receive:   'bg-emerald-100 dark:bg-emerald-900/30',
  adjust:    'bg-blue-100 dark:bg-blue-900/30',
  writeOff:  'bg-red-100 dark:bg-red-900/30',
  threshold: 'bg-amber-100 dark:bg-amber-900/30',
};

const inputCls = (err?: string) =>
  cn(
    'w-full rounded-lg border px-3 py-2 text-sm bg-white dark:bg-gray-900',
    'text-gray-900 dark:text-white placeholder-gray-400',
    'focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 transition',
    err ? 'border-red-400 dark:border-red-500' : 'border-gray-200 dark:border-gray-700',
  );

export function StockActionModal({ open, type, item, isSaving, error, onSave, onClose }: Props) {
  const meta = ACTION_META[type];

  const [quantity,     setQuantity]     = useState('');
  const [newQuantity,  setNewQuantity]  = useState('');
  const [reason,       setReason]       = useState('');
  const [reference,    setReference]    = useState('');
  const [notes,        setNotes]        = useState('');
  const [threshold,    setThreshold]    = useState('');
  const [errs,         setErrs]         = useState<Record<string, string>>({});

  useEffect(() => {
    if (open) {
      setQuantity(''); setNewQuantity(''); setReason('');
      setReference(''); setNotes(''); setErrs('');
      if (item && type === 'threshold') setThreshold(String(item.lowStockThreshold));
      else setThreshold('');
    }
  }, [open, type, item]);

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (type === 'receive') {
      const q = parseFloat(quantity);
      if (!quantity || isNaN(q) || q <= 0) e.quantity = 'Enter quantity > 0';
    }
    if (type === 'adjust') {
      const q = parseFloat(newQuantity);
      if (!newQuantity || isNaN(q) || q < 0) e.newQuantity = 'Enter valid quantity (≥ 0)';
      if (!notes.trim()) e.notes = 'Notes are required for adjustments';
    }
    if (type === 'writeOff') {
      const q = parseFloat(quantity);
      if (!quantity || isNaN(q) || q <= 0) e.quantity = 'Enter quantity > 0';
      if (item && q > item.quantity) e.quantity = `Cannot exceed stock on hand (${item.quantity})`;
      if (!reason.trim()) e.reason = 'Reason is required';
    }
    if (type === 'threshold') {
      const t = parseFloat(threshold);
      if (!threshold || isNaN(t) || t < 0) e.threshold = 'Enter a valid threshold (≥ 0)';
    }
    setErrs(e);
    return Object.keys(e).length === 0;
  }

  function handleSubmit(ev: React.FormEvent) {
    ev.preventDefault();
    if (!validate()) return;
    if (type === 'receive')   onSave({ type, quantity: parseFloat(quantity), reference: reference || undefined, notes: notes || undefined });
    if (type === 'adjust')    onSave({ type, newQuantity: parseFloat(newQuantity), notes });
    if (type === 'writeOff')  onSave({ type, quantity: parseFloat(quantity), reason });
    if (type === 'threshold') onSave({ type, threshold: parseFloat(threshold) });
  }

  return (
    <Dialog.Root open={open} onOpenChange={(o) => !o && !isSaving && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <Dialog.Content className={cn(
          'fixed left-1/2 top-1/2 z-50 -translate-x-1/2 -translate-y-1/2',
          'w-full max-w-md rounded-xl bg-white dark:bg-gray-900',
          'border border-gray-200 dark:border-gray-700 shadow-xl focus:outline-none',
          'data-[state=open]:animate-in data-[state=closed]:animate-out',
          'data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0',
          'data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95',
        )}>
          {/* Header */}
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200 dark:border-gray-700">
            <Dialog.Title className="flex items-center gap-3 text-base font-semibold text-gray-900 dark:text-white">
              <div className={cn('flex items-center justify-center w-9 h-9 rounded-full flex-shrink-0', ICON_WRAP[type])}>
                {meta.icon}
              </div>
              {meta.title}
            </Dialog.Title>
            <Dialog.Close disabled={isSaving} className="rounded-lg p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none disabled:opacity-40">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          <form onSubmit={handleSubmit} noValidate>
            <div className="px-6 py-5 space-y-4">
              {/* Product info */}
              {item && (
                <div className="flex items-center gap-3 rounded-lg bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 px-4 py-3">
                  <Package className="w-4 h-4 text-gray-400 flex-shrink-0" />
                  <div className="min-w-0">
                    <p className="text-sm font-medium text-gray-900 dark:text-white truncate">{item.productName}</p>
                    <p className="text-xs text-gray-400 font-mono">{item.sku}</p>
                  </div>
                  <div className="ml-auto text-right flex-shrink-0">
                    <p className="text-xs text-gray-400">In Stock</p>
                    <p className="text-sm font-bold text-gray-900 dark:text-white">{item.quantity.toLocaleString()}</p>
                  </div>
                </div>
              )}

              <Dialog.Description className="sr-only">
                {meta.title} for {item?.productName}
              </Dialog.Description>

              {/* Receive fields */}
              {type === 'receive' && (
                <>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">
                      Quantity to Receive <span className="text-red-500">*</span>
                    </label>
                    <input type="number" min={0.01} step="0.01" value={quantity}
                      onChange={(e) => { setQuantity(e.target.value); setErrs((x) => ({ ...x, quantity: '' })); }}
                      placeholder="0.00" className={inputCls(errs.quantity)} />
                    {errs.quantity && <p className="mt-1 text-xs text-red-500">{errs.quantity}</p>}
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Reference (optional)</label>
                    <input type="text" value={reference} onChange={(e) => setReference(e.target.value)}
                      placeholder="PO number, delivery note…" className={inputCls()} />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Notes (optional)</label>
                    <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2}
                      placeholder="Any additional notes…" className={cn(inputCls(), 'resize-none')} />
                  </div>
                </>
              )}

              {/* Adjust fields */}
              {type === 'adjust' && (
                <>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">
                      New Quantity (actual count) <span className="text-red-500">*</span>
                    </label>
                    <input type="number" min={0} step="0.01" value={newQuantity}
                      onChange={(e) => { setNewQuantity(e.target.value); setErrs((x) => ({ ...x, newQuantity: '' })); }}
                      placeholder="0.00" className={inputCls(errs.newQuantity)} />
                    {errs.newQuantity && <p className="mt-1 text-xs text-red-500">{errs.newQuantity}</p>}
                    {item && newQuantity && !isNaN(parseFloat(newQuantity)) && (
                      <p className={cn('mt-1 text-xs font-medium', parseFloat(newQuantity) >= item.quantity ? 'text-emerald-600 dark:text-emerald-400' : 'text-red-500')}>
                        {parseFloat(newQuantity) >= item.quantity
                          ? `+${(parseFloat(newQuantity) - item.quantity).toFixed(2)} increase`
                          : `${(parseFloat(newQuantity) - item.quantity).toFixed(2)} decrease`}
                      </p>
                    )}
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">
                      Reason / Notes <span className="text-red-500">*</span>
                    </label>
                    <textarea value={notes} onChange={(e) => { setNotes(e.target.value); setErrs((x) => ({ ...x, notes: '' })); }}
                      rows={2} placeholder="Reason for adjustment…" className={cn(inputCls(errs.notes), 'resize-none')} />
                    {errs.notes && <p className="mt-1 text-xs text-red-500">{errs.notes}</p>}
                  </div>
                </>
              )}

              {/* Write-off fields */}
              {type === 'writeOff' && (
                <>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">
                      Quantity to Write Off <span className="text-red-500">*</span>
                    </label>
                    <input type="number" min={0.01} step="0.01" value={quantity}
                      onChange={(e) => { setQuantity(e.target.value); setErrs((x) => ({ ...x, quantity: '' })); }}
                      placeholder="0.00" className={inputCls(errs.quantity)} />
                    {errs.quantity && <p className="mt-1 text-xs text-red-500">{errs.quantity}</p>}
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">
                      Reason <span className="text-red-500">*</span>
                    </label>
                    <select value={reason} onChange={(e) => { setReason(e.target.value); setErrs((x) => ({ ...x, reason: '' })); }}
                      className={inputCls(errs.reason)}>
                      <option value="">Select reason…</option>
                      <option value="Damaged">Damaged</option>
                      <option value="Expired">Expired</option>
                      <option value="Theft">Theft / Shrinkage</option>
                      <option value="Lost">Lost</option>
                      <option value="Quality Issue">Quality Issue</option>
                      <option value="Other">Other</option>
                    </select>
                    {errs.reason && <p className="mt-1 text-xs text-red-500">{errs.reason}</p>}
                  </div>
                </>
              )}

              {/* Threshold fields */}
              {type === 'threshold' && (
                <div>
                  <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">
                    Low Stock Threshold <span className="text-red-500">*</span>
                  </label>
                  <input type="number" min={0} step="1" value={threshold}
                    onChange={(e) => { setThreshold(e.target.value); setErrs((x) => ({ ...x, threshold: '' })); }}
                    placeholder="e.g. 10" className={inputCls(errs.threshold)} />
                  {errs.threshold && <p className="mt-1 text-xs text-red-500">{errs.threshold}</p>}
                  <p className="mt-1.5 text-xs text-gray-400">
                    An alert will be shown when stock falls at or below this number.
                  </p>
                </div>
              )}

              {/* API Error */}
              {error && (
                <div className="flex items-center gap-2 rounded-lg border border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-900/20 px-3 py-2.5">
                  <AlertCircle className="w-4 h-4 text-red-600 dark:text-red-400 flex-shrink-0" />
                  <p className="text-xs text-red-700 dark:text-red-300">{error}</p>
                </div>
              )}
            </div>

            {/* Footer */}
            <div className="flex justify-end gap-3 px-6 py-4 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50 rounded-b-xl">
              <button type="button" onClick={onClose} disabled={isSaving}
                className="px-4 py-2 text-sm font-medium rounded-lg border border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-50">
                Cancel
              </button>
              <button type="submit" disabled={isSaving}
                className={cn('px-4 py-2 text-sm font-medium rounded-lg text-white transition-colors disabled:opacity-60 flex items-center gap-2', meta.btnColor)}>
                {isSaving && <Loader2 className="w-3.5 h-3.5 animate-spin" />}
                {isSaving ? 'Saving…' : meta.btnLabel}
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
