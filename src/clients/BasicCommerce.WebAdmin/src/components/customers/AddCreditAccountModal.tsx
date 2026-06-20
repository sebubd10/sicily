import { useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import { X, CreditCard, Search, AlertCircle, Loader2 } from 'lucide-react';
import { cn, extractApiError } from '../../lib/utils';
import { useCustomerSearch } from '../../hooks/useCustomers';
import { useCreateCreditAccount } from '../../hooks/useCustomers';
import { useStoreList } from '../../hooks/useStoreAdmin';

type Props = {
  open: boolean;
  onClose: () => void;
};

export function AddCreditAccountModal({ open, onClose }: Props) {
  const [customerSearch, setCustomerSearch] = useState('');
  const [selectedCustomerId, setSelectedCustomerId] = useState('');
  const [selectedCustomerName, setSelectedCustomerName] = useState('');
  const [storeId, setStoreId] = useState('');
  const [creditLimit, setCreditLimit] = useState('');
  const [showDropdown, setShowDropdown] = useState(false);

  const { data: customers = [], isFetching: searchingCustomers } = useCustomerSearch(customerSearch);
  const { data: stores = [] } = useStoreList();
  const { mutate: create, isPending, error } = useCreateCreditAccount();

  const apiError = error ? extractApiError(error) : null;
  const activeStores = stores.filter((s) => s.status === 'Active');

  function handleClose() {
    setCustomerSearch('');
    setSelectedCustomerId('');
    setSelectedCustomerName('');
    setStoreId('');
    setCreditLimit('');
    setShowDropdown(false);
    onClose();
  }

  function selectCustomer(id: string, name: string) {
    setSelectedCustomerId(id);
    setSelectedCustomerName(name);
    setCustomerSearch(name);
    setShowDropdown(false);
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const limit = parseFloat(creditLimit);
    if (!selectedCustomerId || !storeId || isNaN(limit) || limit <= 0) return;
    create(
      { customerId: selectedCustomerId, storeId, creditLimit: limit },
      { onSuccess: handleClose },
    );
  }

  return (
    <Dialog.Root open={open} onOpenChange={(v) => !v && handleClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 bg-black/50 backdrop-blur-sm z-40 animate-in fade-in-0 duration-200" />
        <Dialog.Content className="fixed left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 z-50 w-full max-w-md bg-white dark:bg-gray-900 rounded-2xl shadow-2xl flex flex-col">
          {/* Header */}
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-gray-800">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-xl bg-blue-100 dark:bg-blue-900/30">
                <CreditCard className="w-5 h-5 text-blue-600 dark:text-blue-400" />
              </div>
              <div>
                <Dialog.Title className="text-base font-semibold text-gray-900 dark:text-gray-100">
                  New Credit Account
                </Dialog.Title>
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                  Open a credit account for a customer at a specific store
                </p>
              </div>
            </div>
            <Dialog.Close
              onClick={handleClose}
              className="p-1.5 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
            >
              <X className="w-4 h-4" />
            </Dialog.Close>
          </div>

          {/* Body */}
          <form onSubmit={handleSubmit} className="px-6 py-5 space-y-4">
            {apiError && (
              <div className="flex items-center gap-2 p-3 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-sm text-red-700 dark:text-red-300">
                <AlertCircle className="w-4 h-4 flex-shrink-0" />
                {apiError}
              </div>
            )}

            {/* Customer picker */}
            <div className="relative">
              <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1.5">
                Customer *
              </label>
              <div className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                <input
                  type="text"
                  placeholder="Search by name, phone, or code…"
                  value={customerSearch}
                  onChange={(e) => {
                    setCustomerSearch(e.target.value);
                    setSelectedCustomerId('');
                    setSelectedCustomerName('');
                    setShowDropdown(true);
                  }}
                  onFocus={() => setShowDropdown(true)}
                  className="w-full pl-9 pr-3 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                {searchingCustomers && (
                  <Loader2 className="absolute right-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-gray-400 animate-spin" />
                )}
              </div>

              {showDropdown && customerSearch.length >= 2 && customers.length > 0 && (
                <div className="absolute z-10 top-full mt-1 w-full bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 shadow-lg overflow-hidden max-h-48 overflow-y-auto">
                  {customers.map((c) => (
                    <button
                      key={c.id}
                      type="button"
                      onMouseDown={(e) => { e.preventDefault(); selectCustomer(c.id, c.name); }}
                      className={cn(
                        'w-full text-left px-4 py-2.5 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors',
                        selectedCustomerId === c.id && 'bg-blue-50 dark:bg-blue-900/20',
                      )}
                    >
                      <p className="text-sm font-medium text-gray-900 dark:text-gray-100">{c.name}</p>
                      <p className="text-xs text-gray-400">{c.code} {c.phone ? `· ${c.phone}` : ''}</p>
                    </button>
                  ))}
                </div>
              )}
            </div>

            {/* Store picker */}
            <div>
              <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1.5">
                Store *
              </label>
              <select
                value={storeId}
                onChange={(e) => setStoreId(e.target.value)}
                required
                className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Select a store…</option>
                {activeStores.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.name} ({s.code})
                  </option>
                ))}
              </select>
            </div>

            {/* Credit limit */}
            <div>
              <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1.5">
                Credit Limit *
              </label>
              <div className="relative">
                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm">৳</span>
                <input
                  type="number"
                  min="1"
                  step="1"
                  required
                  placeholder="0"
                  value={creditLimit}
                  onChange={(e) => setCreditLimit(e.target.value)}
                  className="w-full pl-8 pr-3 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>

            {/* Actions */}
            <div className="flex justify-end gap-3 pt-2">
              <button
                type="button"
                onClick={handleClose}
                className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100 font-medium transition-colors"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isPending || !selectedCustomerId || !storeId || !creditLimit}
                className="flex items-center gap-2 px-4 py-2 rounded-lg bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white text-sm font-medium transition-colors"
              >
                {isPending ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <CreditCard className="w-3.5 h-3.5" />}
                {isPending ? 'Creating…' : 'Create Account'}
              </button>
            </div>
          </form>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
