import { useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { X, Gift, Search, Loader2, AlertCircle, Calendar } from 'lucide-react';
import { cn, extractApiError } from '../../lib/utils';
import { useIssueGiftCard } from '../../hooks/useGiftCards';
import { useStoreList } from '../../hooks/useStoreAdmin';
import { useCustomerSearch } from '../../hooks/useCustomers';

type Props = { open: boolean; onClose: () => void };

export function IssueGiftCardModal({ open, onClose }: Props) {
  const [storeId, setStoreId] = useState('');
  const [amount, setAmount] = useState('');
  const [expiryDate, setExpiryDate] = useState('');
  const [customerSearch, setCustomerSearch] = useState('');
  const [selectedCustomerId, setSelectedCustomerId] = useState('');
  const [selectedCustomerName, setSelectedCustomerName] = useState('');
  const [showCustomerDrop, setShowCustomerDrop] = useState(false);
  const [notes, setNotes] = useState('');

  const { data: stores = [] } = useStoreList();
  const { data: customers = [], isFetching: searchingCustomers } = useCustomerSearch(customerSearch);
  const { mutate: issue, isPending, error } = useIssueGiftCard();

  const activeStores = stores.filter((s) => s.status === 'Active');
  const apiError = error ? extractApiError(error) : null;

  // min date for expiry picker = tomorrow
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  const minDate = tomorrow.toISOString().split('T')[0];

  function reset() {
    setStoreId(''); setAmount(''); setExpiryDate('');
    setCustomerSearch(''); setSelectedCustomerId(''); setSelectedCustomerName('');
    setShowCustomerDrop(false); setNotes('');
  }

  function handleClose() { reset(); onClose(); }

  function selectCustomer(id: string, name: string) {
    setSelectedCustomerId(id);
    setSelectedCustomerName(name);
    setCustomerSearch(name);
    setShowCustomerDrop(false);
  }

  function clearCustomer() {
    setSelectedCustomerId('');
    setSelectedCustomerName('');
    setCustomerSearch('');
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const amt = parseFloat(amount);
    if (!storeId || isNaN(amt) || amt <= 0) return;
    issue(
      {
        storeId,
        amount: amt,
        expiryDate: expiryDate || null,
        issuedToCustomerId: selectedCustomerId || null,
        notes: notes.trim() || null,
      },
      { onSuccess: handleClose },
    );
  }

  return (
    <Dialog.Root open={open} onOpenChange={(v) => !v && handleClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 bg-black/50 backdrop-blur-sm z-40 animate-in fade-in-0 duration-200" />
        <Dialog.Content className="fixed left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 z-50 w-full max-w-lg bg-white dark:bg-gray-900 rounded-2xl shadow-2xl flex flex-col max-h-[90vh]">

          {/* Header */}
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-gray-800">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-xl bg-violet-100 dark:bg-violet-900/30">
                <Gift className="w-5 h-5 text-violet-600 dark:text-violet-400" />
              </div>
              <div>
                <Dialog.Title className="text-base font-semibold text-gray-900 dark:text-gray-100">
                  Issue Gift Card
                </Dialog.Title>
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                  Generate a new prepaid gift card with a unique code
                </p>
              </div>
            </div>
            <Dialog.Close onClick={handleClose}
              className="p-1.5 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors">
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          {/* Body */}
          <form onSubmit={handleSubmit} className="flex-1 overflow-y-auto px-6 py-5 space-y-4">
            {apiError && (
              <div className="flex items-center gap-2 p-3 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-sm text-red-700 dark:text-red-300">
                <AlertCircle className="w-4 h-4 flex-shrink-0" />{apiError}
              </div>
            )}

            {/* Store */}
            <div>
              <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1.5">
                Store <span className="text-red-400">*</span>
              </label>
              <select
                required
                value={storeId}
                onChange={(e) => setStoreId(e.target.value)}
                className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-violet-500"
              >
                <option value="">Select a store…</option>
                {activeStores.map((s) => (
                  <option key={s.id} value={s.id}>{s.name} ({s.code})</option>
                ))}
              </select>
            </div>

            {/* Amount */}
            <div>
              <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1.5">
                Card Value <span className="text-red-400">*</span>
              </label>
              <div className="relative">
                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm font-medium">৳</span>
                <input
                  type="number"
                  min="1"
                  step="1"
                  required
                  placeholder="500"
                  value={amount}
                  onChange={(e) => setAmount(e.target.value)}
                  className="w-full pl-8 pr-3 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-violet-500"
                />
              </div>
              {/* Quick amounts */}
              <div className="flex gap-1.5 mt-2">
                {[200, 500, 1000, 2000, 5000].map((v) => (
                  <button
                    key={v}
                    type="button"
                    onClick={() => setAmount(String(v))}
                    className={cn(
                      'px-2.5 py-1 rounded-lg text-xs font-medium border transition-colors',
                      amount === String(v)
                        ? 'bg-violet-100 border-violet-300 text-violet-700 dark:bg-violet-900/30 dark:border-violet-600 dark:text-violet-300'
                        : 'border-gray-200 dark:border-gray-700 text-gray-500 dark:text-gray-400 hover:border-gray-300',
                    )}
                  >
                    ৳{v.toLocaleString()}
                  </button>
                ))}
              </div>
            </div>

            {/* Expiry + Customer (2-col) */}
            <div className="grid grid-cols-2 gap-3">
              {/* Expiry Date */}
              <div>
                <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1.5">
                  <span className="flex items-center gap-1"><Calendar className="w-3 h-3" />Expiry Date</span>
                </label>
                <input
                  type="date"
                  min={minDate}
                  value={expiryDate}
                  onChange={(e) => setExpiryDate(e.target.value)}
                  className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-violet-500"
                />
                {!expiryDate && (
                  <p className="text-xs text-gray-400 mt-1">No expiry (never expires)</p>
                )}
              </div>

              {/* Issued to (optional) */}
              <div className="relative">
                <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1.5">
                  Issue To (optional)
                </label>
                <div className="relative">
                  <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-gray-400" />
                  <input
                    type="text"
                    placeholder="Search customer…"
                    value={customerSearch}
                    onChange={(e) => {
                      setCustomerSearch(e.target.value);
                      setSelectedCustomerId('');
                      setSelectedCustomerName('');
                      setShowCustomerDrop(true);
                    }}
                    onFocus={() => setShowCustomerDrop(true)}
                    onBlur={() => setTimeout(() => setShowCustomerDrop(false), 150)}
                    className="w-full pl-8 pr-3 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-violet-500"
                  />
                  {searchingCustomers && (
                    <Loader2 className="absolute right-2.5 top-1/2 -translate-y-1/2 w-3 h-3 animate-spin text-gray-400" />
                  )}
                </div>
                {selectedCustomerId && (
                  <button type="button" onClick={clearCustomer}
                    className="text-xs text-violet-600 hover:underline mt-1 block">
                    Clear selection
                  </button>
                )}
                {showCustomerDrop && customerSearch.length >= 2 && customers.length > 0 && (
                  <div className="absolute z-20 top-full mt-1 w-full bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 shadow-lg overflow-hidden max-h-40 overflow-y-auto">
                    {customers.map((c) => (
                      <button
                        key={c.id}
                        type="button"
                        onMouseDown={(e) => { e.preventDefault(); selectCustomer(c.id, c.name); }}
                        className={cn(
                          'w-full text-left px-3 py-2 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors',
                          selectedCustomerId === c.id && 'bg-violet-50 dark:bg-violet-900/20',
                        )}
                      >
                        <p className="text-xs font-medium text-gray-900 dark:text-gray-100">{c.name}</p>
                        <p className="text-xs text-gray-400">{c.code}</p>
                      </button>
                    ))}
                  </div>
                )}
              </div>
            </div>

            {/* Notes */}
            <div>
              <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1.5">
                Notes (optional)
              </label>
              <textarea
                rows={2}
                placeholder="e.g. Birthday gift for Rahim"
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-violet-500 resize-none"
              />
            </div>

            {/* Preview */}
            {amount && parseFloat(amount) > 0 && storeId && (
              <div className="bg-violet-50 dark:bg-violet-900/20 rounded-xl p-3 border border-violet-100 dark:border-violet-800">
                <p className="text-xs font-semibold text-violet-700 dark:text-violet-300 uppercase tracking-wider mb-1.5">
                  Card Preview
                </p>
                <div className="flex items-center justify-between text-sm">
                  <span className="text-gray-600 dark:text-gray-400">Value</span>
                  <span className="font-bold text-violet-700 dark:text-violet-300">
                    ৳{parseFloat(amount).toLocaleString('en-BD', { minimumFractionDigits: 0 })}
                  </span>
                </div>
                {expiryDate && (
                  <div className="flex items-center justify-between text-sm mt-1">
                    <span className="text-gray-600 dark:text-gray-400">Expires</span>
                    <span className="text-gray-700 dark:text-gray-300">
                      {new Date(expiryDate).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })}
                    </span>
                  </div>
                )}
                {selectedCustomerName && (
                  <div className="flex items-center justify-between text-sm mt-1">
                    <span className="text-gray-600 dark:text-gray-400">Issued to</span>
                    <span className="text-gray-700 dark:text-gray-300">{selectedCustomerName}</span>
                  </div>
                )}
                <p className="text-xs text-violet-500 dark:text-violet-400 mt-2">
                  A unique code will be generated automatically
                </p>
              </div>
            )}
          </form>

          {/* Footer */}
          <div className="px-6 py-3 border-t border-gray-100 dark:border-gray-800 flex items-center justify-end gap-3">
            <button type="button" onClick={handleClose}
              className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100 font-medium transition-colors">
              Cancel
            </button>
            <button
              form="issue-form"
              type="submit"
              onClick={handleSubmit}
              disabled={isPending || !storeId || !amount}
              className="flex items-center gap-2 px-5 py-2 rounded-xl bg-violet-600 hover:bg-violet-700 disabled:opacity-50 text-white text-sm font-medium transition-colors shadow-sm"
            >
              {isPending ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <Gift className="w-3.5 h-3.5" />}
              {isPending ? 'Issuing…' : 'Issue Card'}
            </button>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
