import { useState } from 'react';
import * as Dialog from '@radix-ui/react-dialog';
import {
  X, User, Phone, Mail, MapPin, Star, CreditCard, Clock,
  TrendingUp, TrendingDown, Wallet, AlertCircle, Loader2,
  ArrowDownCircle, ArrowUpCircle, BadgeCheck, Ban, Pencil,
} from 'lucide-react';
import { cn, extractApiError } from '../../lib/utils';
import type { Customer } from '../../types/customer';
import {
  useCustomerCreditAccounts,
  useUpdateCreditLimit,
  useRecordCreditPayment,
  useAddLoyaltyPoints,
  useRedeemLoyaltyPoints,
} from '../../hooks/useCustomers';

type Tab = 'overview' | 'credit' | 'loyalty';

type Props = {
  customer: Customer | null;
  open: boolean;
  onClose: () => void;
  onEdit: () => void;
};

const inputCls =
  'w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 ' +
  'px-3 py-2 text-sm text-gray-900 dark:text-gray-100 placeholder-gray-400 ' +
  'focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent';

const labelCls = 'block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1';

function fmtDate(d: string | null | undefined) {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-GB', {
    day: '2-digit', month: 'short', year: 'numeric',
  });
}

function fmtMoney(n: number) {
  return `৳${n.toLocaleString('en-BD', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

function StatCard({
  label, value, sub, color = 'default',
}: {
  label: string;
  value: string;
  sub?: string;
  color?: 'default' | 'green' | 'red' | 'amber' | 'indigo';
}) {
  const colors = {
    default: 'bg-gray-50 dark:bg-gray-800',
    green: 'bg-emerald-50 dark:bg-emerald-900/20',
    red: 'bg-red-50 dark:bg-red-900/20',
    amber: 'bg-amber-50 dark:bg-amber-900/20',
    indigo: 'bg-indigo-50 dark:bg-indigo-900/20',
  };
  const valueColors = {
    default: 'text-gray-900 dark:text-gray-100',
    green: 'text-emerald-700 dark:text-emerald-300',
    red: 'text-red-700 dark:text-red-300',
    amber: 'text-amber-700 dark:text-amber-300',
    indigo: 'text-indigo-700 dark:text-indigo-300',
  };
  return (
    <div className={cn('rounded-xl p-3', colors[color])}>
      <p className="text-xs text-gray-500 dark:text-gray-400 mb-1">{label}</p>
      <p className={cn('text-lg font-bold', valueColors[color])}>{value}</p>
      {sub && <p className="text-xs text-gray-400 dark:text-gray-500 mt-0.5">{sub}</p>}
    </div>
  );
}

export function CustomerDetailModal({ customer, open, onClose, onEdit }: Props) {
  const [tab, setTab] = useState<Tab>('overview');

  // Credit state
  const [showCreditLimitForm, setShowCreditLimitForm] = useState(false);
  const [newCreditLimit, setNewCreditLimit] = useState('');
  const [paymentAmount, setPaymentAmount] = useState('');
  const [paymentRef, setPaymentRef] = useState('');
  const [selectedCreditAccountId, setSelectedCreditAccountId] = useState<string | null>(null);

  // Loyalty state
  const [loyaltyMode, setLoyaltyMode] = useState<'add' | 'redeem'>('add');
  const [loyaltyPoints, setLoyaltyPoints] = useState('');
  const [loyaltyReason, setLoyaltyReason] = useState('');
  const [loyaltySuccess, setLoyaltySuccess] = useState('');

  const { data: creditAccounts = [], isLoading: loadingCredit } = useCustomerCreditAccounts(
    customer?.id ?? null,
  );
  const { mutate: updateCreditLimit, isPending: updatingLimit, error: limitError } = useUpdateCreditLimit();
  const { mutate: recordPayment, isPending: recordingPayment, error: paymentError } = useRecordCreditPayment();
  const { mutate: addPoints, isPending: addingPoints, error: addError } = useAddLoyaltyPoints();
  const { mutate: redeemPoints, isPending: redeemingPoints, error: redeemError } = useRedeemLoyaltyPoints();

  if (!customer) return null;

  const activeAccount = creditAccounts.find((a) => a.status === 'Active') ?? creditAccounts[0];

  function handleCreditLimitSave() {
    const val = Number(newCreditLimit);
    if (isNaN(val) || val < 0) return;
    updateCreditLimit(
      { id: customer.id, creditLimit: val },
      {
        onSuccess: () => {
          setShowCreditLimitForm(false);
          setNewCreditLimit('');
        },
      },
    );
  }

  function handlePayment() {
    if (!selectedCreditAccountId && !activeAccount) return;
    const accountId = selectedCreditAccountId ?? activeAccount?.id;
    if (!accountId) return;
    const amt = Number(paymentAmount);
    if (isNaN(amt) || amt <= 0) return;
    recordPayment(
      { creditAccountId: accountId, amount: amt, reference: paymentRef || null },
      {
        onSuccess: () => {
          setPaymentAmount('');
          setPaymentRef('');
        },
      },
    );
  }

  function handleLoyaltyAction() {
    const pts = Number(loyaltyPoints);
    if (isNaN(pts) || pts <= 0 || !loyaltyReason.trim()) return;

    const vars = { id: customer.id, points: pts, reason: loyaltyReason };
    const successMsg =
      loyaltyMode === 'add'
        ? `Added ${pts} points successfully`
        : `Redeemed ${pts} points successfully`;

    if (loyaltyMode === 'add') {
      addPoints(vars, {
        onSuccess: () => {
          setLoyaltyPoints('');
          setLoyaltyReason('');
          setLoyaltySuccess(successMsg);
          setTimeout(() => setLoyaltySuccess(''), 3000);
        },
      });
    } else {
      redeemPoints(vars, {
        onSuccess: () => {
          setLoyaltyPoints('');
          setLoyaltyReason('');
          setLoyaltySuccess(successMsg);
          setTimeout(() => setLoyaltySuccess(''), 3000);
        },
      });
    }
  }

  const TABS: { id: Tab; label: string }[] = [
    { id: 'overview', label: 'Overview' },
    { id: 'credit', label: 'Credit' },
    { id: 'loyalty', label: 'Loyalty' },
  ];

  return (
    <Dialog.Root open={open} onOpenChange={(v) => !v && onClose()}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 bg-black/50 backdrop-blur-sm z-40 animate-in fade-in-0 duration-200" />
        <Dialog.Content className="fixed left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 z-50 w-full max-w-2xl bg-white dark:bg-gray-900 rounded-2xl shadow-2xl flex flex-col max-h-[90vh]">
          {/* Header */}
          <div className="flex items-start justify-between px-6 py-4 border-b border-gray-100 dark:border-gray-800">
            <div className="flex items-center gap-3">
              <div className="w-11 h-11 rounded-full bg-indigo-100 dark:bg-indigo-900/30 flex items-center justify-center flex-shrink-0">
                <span className="text-lg font-bold text-indigo-600 dark:text-indigo-400">
                  {customer.name[0].toUpperCase()}
                </span>
              </div>
              <div>
                <div className="flex items-center gap-2">
                  <Dialog.Title className="text-base font-semibold text-gray-900 dark:text-gray-100">
                    {customer.name}
                  </Dialog.Title>
                  <span className={cn(
                    'text-xs px-2 py-0.5 rounded-full font-medium',
                    customer.status === 'Active'
                      ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400'
                      : 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400',
                  )}>
                    {customer.status}
                  </span>
                </div>
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                  {customer.code} · Customer since {fmtDate(customer.createdAt)}
                </p>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={onEdit}
                className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
              >
                <Pencil className="w-3.5 h-3.5" />
                Edit
              </button>
              <Dialog.Close className="p-1.5 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors">
                <X className="w-4 h-4" />
              </Dialog.Close>
            </div>
          </div>

          {/* Tabs */}
          <div className="flex gap-1 px-6 pt-3">
            {TABS.map((t) => (
              <button
                key={t.id}
                onClick={() => setTab(t.id)}
                className={cn(
                  'px-4 py-2 rounded-lg text-sm font-medium transition-all',
                  tab === t.id
                    ? 'bg-indigo-100 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300'
                    : 'text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800',
                )}
              >
                {t.label}
              </button>
            ))}
          </div>

          {/* Body */}
          <div className="flex-1 overflow-y-auto px-6 py-4">
            {/* Overview Tab */}
            {tab === 'overview' && (
              <div className="space-y-5">
                {/* Contact info */}
                <div className="grid grid-cols-2 gap-3">
                  <div className="flex items-center gap-2 p-3 rounded-xl bg-gray-50 dark:bg-gray-800">
                    <Phone className="w-4 h-4 text-gray-400 flex-shrink-0" />
                    <div>
                      <p className="text-xs text-gray-400">Phone</p>
                      <p className="text-sm font-medium text-gray-900 dark:text-gray-100">
                        {customer.phone ?? '—'}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-2 p-3 rounded-xl bg-gray-50 dark:bg-gray-800">
                    <Mail className="w-4 h-4 text-gray-400 flex-shrink-0" />
                    <div>
                      <p className="text-xs text-gray-400">Email</p>
                      <p className="text-sm font-medium text-gray-900 dark:text-gray-100 truncate">
                        {customer.email ?? '—'}
                      </p>
                    </div>
                  </div>
                </div>

                {/* Address */}
                {(customer.addressLine1 || customer.city) && (
                  <div className="flex items-start gap-2 p-3 rounded-xl bg-gray-50 dark:bg-gray-800">
                    <MapPin className="w-4 h-4 text-gray-400 flex-shrink-0 mt-0.5" />
                    <div>
                      <p className="text-xs text-gray-400">Address</p>
                      <p className="text-sm font-medium text-gray-900 dark:text-gray-100">
                        {[customer.addressLine1, customer.city].filter(Boolean).join(', ')}
                      </p>
                    </div>
                  </div>
                )}

                {/* Summary stats */}
                <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
                  <StatCard
                    label="Loyalty Points"
                    value={customer.loyaltyPoints.toLocaleString()}
                    color="indigo"
                  />
                  <StatCard
                    label="Credit Limit"
                    value={fmtMoney(customer.creditLimit)}
                    color={customer.creditLimit > 0 ? 'green' : 'default'}
                  />
                  <StatCard
                    label="Outstanding"
                    value={fmtMoney(customer.currentBalance)}
                    color={customer.currentBalance > 0 ? 'red' : 'default'}
                  />
                  <StatCard
                    label="Available Credit"
                    value={fmtMoney(customer.availableCredit)}
                    color={customer.availableCredit > 0 ? 'green' : 'default'}
                  />
                </div>
              </div>
            )}

            {/* Credit Tab */}
            {tab === 'credit' && (
              <div className="space-y-5">
                {loadingCredit ? (
                  <div className="flex items-center justify-center py-12">
                    <Loader2 className="w-6 h-6 text-indigo-500 animate-spin" />
                  </div>
                ) : creditAccounts.length === 0 ? (
                  <div className="text-center py-12">
                    <CreditCard className="w-8 h-8 text-gray-300 dark:text-gray-600 mx-auto mb-3" />
                    <p className="text-gray-500 dark:text-gray-400 text-sm">No credit accounts</p>
                    <p className="text-gray-400 dark:text-gray-500 text-xs mt-1">
                      Update credit limit to open a credit account
                    </p>
                  </div>
                ) : (
                  creditAccounts.map((acc) => (
                    <div key={acc.id} className="space-y-4">
                      {/* Account header */}
                      <div className="flex items-center justify-between">
                        <div>
                          <p className="text-sm font-semibold text-gray-900 dark:text-gray-100">
                            {acc.storeName}
                          </p>
                          <p className="text-xs text-gray-400">
                            Last payment: {fmtDate(acc.lastPaymentAt)}
                          </p>
                        </div>
                        <span className={cn(
                          'text-xs px-2 py-0.5 rounded-full font-medium',
                          acc.status === 'Active'
                            ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400'
                            : 'bg-gray-100 text-gray-600',
                        )}>
                          {acc.status}
                        </span>
                      </div>

                      <div className="grid grid-cols-3 gap-3">
                        <StatCard label="Limit" value={fmtMoney(acc.creditLimit)} color="default" />
                        <StatCard
                          label="Outstanding"
                          value={fmtMoney(acc.outstandingBalance)}
                          color={acc.outstandingBalance > 0 ? 'red' : 'default'}
                        />
                        <StatCard
                          label="Available"
                          value={fmtMoney(acc.availableCredit)}
                          color={acc.availableCredit > 0 ? 'green' : 'default'}
                        />
                      </div>

                      {/* Record payment */}
                      {acc.status === 'Active' && acc.outstandingBalance > 0 && (
                        <div className="p-4 rounded-xl bg-gray-50 dark:bg-gray-800 space-y-3">
                          <p className="text-xs font-semibold text-gray-600 dark:text-gray-400 uppercase tracking-wider">
                            Record Payment
                          </p>
                          {paymentError && (
                            <div className="flex items-center gap-2 text-xs text-red-600 dark:text-red-400">
                              <AlertCircle className="w-3.5 h-3.5" />
                              {extractApiError(paymentError)}
                            </div>
                          )}
                          <div className="grid grid-cols-2 gap-2">
                            <div>
                              <label className={labelCls}>Amount *</label>
                              <div className="relative">
                                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-xs">৳</span>
                                <input
                                  type="number"
                                  min="0.01"
                                  className={cn(inputCls, 'pl-8 py-1.5 text-xs')}
                                  placeholder="0.00"
                                  value={paymentAmount}
                                  onChange={(e) => setPaymentAmount(e.target.value)}
                                />
                              </div>
                            </div>
                            <div>
                              <label className={labelCls}>Reference</label>
                              <input
                                className={cn(inputCls, 'py-1.5 text-xs')}
                                placeholder="Cheque / TXN no."
                                value={paymentRef}
                                onChange={(e) => setPaymentRef(e.target.value)}
                              />
                            </div>
                          </div>
                          <button
                            onClick={() => {
                              setSelectedCreditAccountId(acc.id);
                              handlePayment();
                            }}
                            disabled={recordingPayment || !paymentAmount}
                            className="flex items-center gap-2 px-4 py-1.5 rounded-lg bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white text-xs font-medium transition-colors"
                          >
                            <ArrowDownCircle className="w-3.5 h-3.5" />
                            {recordingPayment ? 'Recording…' : 'Record Payment'}
                          </button>
                        </div>
                      )}

                      {/* Transaction history */}
                      {acc.recentHistory.length > 0 && (
                        <div>
                          <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-2">
                            Recent Transactions
                          </p>
                          <div className="space-y-1.5">
                            {acc.recentHistory.map((tx) => (
                              <div
                                key={tx.id}
                                className="flex items-center justify-between p-2.5 rounded-lg bg-gray-50 dark:bg-gray-800"
                              >
                                <div className="flex items-center gap-2">
                                  {tx.type === 'Payment' ? (
                                    <ArrowDownCircle className="w-4 h-4 text-emerald-500 flex-shrink-0" />
                                  ) : (
                                    <ArrowUpCircle className="w-4 h-4 text-red-500 flex-shrink-0" />
                                  )}
                                  <div>
                                    <p className="text-xs font-medium text-gray-800 dark:text-gray-200">
                                      {tx.description}
                                    </p>
                                    {tx.reference && (
                                      <p className="text-xs text-gray-400">{tx.reference}</p>
                                    )}
                                  </div>
                                </div>
                                <div className="text-right">
                                  <p className={cn(
                                    'text-sm font-semibold',
                                    tx.type === 'Payment'
                                      ? 'text-emerald-600 dark:text-emerald-400'
                                      : 'text-red-600 dark:text-red-400',
                                  )}>
                                    {tx.type === 'Payment' ? '-' : '+'}{fmtMoney(tx.amount)}
                                  </p>
                                  <p className="text-xs text-gray-400">{fmtDate(tx.createdAt)}</p>
                                </div>
                              </div>
                            ))}
                          </div>
                        </div>
                      )}
                    </div>
                  ))
                )}

                {/* Update credit limit */}
                <div className="pt-3 border-t border-gray-100 dark:border-gray-800">
                  {!showCreditLimitForm ? (
                    <button
                      onClick={() => {
                        setNewCreditLimit(String(customer.creditLimit));
                        setShowCreditLimitForm(true);
                      }}
                      className="text-xs text-indigo-600 dark:text-indigo-400 hover:underline"
                    >
                      Update credit limit
                    </button>
                  ) : (
                    <div className="p-3 rounded-xl bg-gray-50 dark:bg-gray-800 space-y-2">
                      <p className="text-xs font-semibold text-gray-600 dark:text-gray-400">
                        New Credit Limit
                      </p>
                      {limitError && (
                        <p className="text-xs text-red-500">{extractApiError(limitError)}</p>
                      )}
                      <div className="flex gap-2">
                        <div className="relative flex-1">
                          <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-xs">৳</span>
                          <input
                            type="number"
                            min="0"
                            className={cn(inputCls, 'pl-8 py-1.5 text-xs')}
                            value={newCreditLimit}
                            onChange={(e) => setNewCreditLimit(e.target.value)}
                          />
                        </div>
                        <button
                          onClick={handleCreditLimitSave}
                          disabled={updatingLimit}
                          className="px-3 py-1.5 rounded-lg bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white text-xs font-medium"
                        >
                          {updatingLimit ? '…' : 'Save'}
                        </button>
                        <button
                          onClick={() => setShowCreditLimitForm(false)}
                          className="px-3 py-1.5 rounded-lg text-gray-500 hover:bg-gray-200 dark:hover:bg-gray-700 text-xs"
                        >
                          Cancel
                        </button>
                      </div>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Loyalty Tab */}
            {tab === 'loyalty' && (
              <div className="space-y-5">
                <div className="grid grid-cols-2 gap-3">
                  <StatCard
                    label="Total Points"
                    value={customer.loyaltyPoints.toLocaleString()}
                    color="indigo"
                  />
                  <StatCard
                    label="Taka Value"
                    value={fmtMoney(customer.loyaltyPoints * 0.75)}
                    sub="at ৳0.75/point"
                    color="indigo"
                  />
                </div>

                {/* Adjust form */}
                <div className="p-4 rounded-xl bg-gray-50 dark:bg-gray-800 space-y-3">
                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => setLoyaltyMode('add')}
                      className={cn(
                        'flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition-all',
                        loyaltyMode === 'add'
                          ? 'bg-emerald-600 text-white'
                          : 'bg-gray-200 dark:bg-gray-700 text-gray-600 dark:text-gray-400',
                      )}
                    >
                      <TrendingUp className="w-3.5 h-3.5" />
                      Add Points
                    </button>
                    <button
                      onClick={() => setLoyaltyMode('redeem')}
                      className={cn(
                        'flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition-all',
                        loyaltyMode === 'redeem'
                          ? 'bg-amber-600 text-white'
                          : 'bg-gray-200 dark:bg-gray-700 text-gray-600 dark:text-gray-400',
                      )}
                    >
                      <TrendingDown className="w-3.5 h-3.5" />
                      Redeem
                    </button>
                  </div>

                  {(addError || redeemError) && (
                    <div className="flex items-center gap-2 text-xs text-red-600 dark:text-red-400">
                      <AlertCircle className="w-3.5 h-3.5" />
                      {extractApiError(addError ?? redeemError)}
                    </div>
                  )}

                  {loyaltySuccess && (
                    <div className="flex items-center gap-2 text-xs text-emerald-600 dark:text-emerald-400">
                      <BadgeCheck className="w-3.5 h-3.5" />
                      {loyaltySuccess}
                    </div>
                  )}

                  <div className="grid grid-cols-2 gap-2">
                    <div>
                      <label className={labelCls}>Points *</label>
                      <input
                        type="number"
                        min="1"
                        className={cn(inputCls, 'py-1.5 text-xs')}
                        placeholder="100"
                        value={loyaltyPoints}
                        onChange={(e) => setLoyaltyPoints(e.target.value)}
                      />
                    </div>
                    <div>
                      <label className={labelCls}>Reason *</label>
                      <input
                        className={cn(inputCls, 'py-1.5 text-xs')}
                        placeholder="Manual adjustment"
                        value={loyaltyReason}
                        onChange={(e) => setLoyaltyReason(e.target.value)}
                      />
                    </div>
                  </div>

                  {loyaltyPoints && Number(loyaltyPoints) > 0 && (
                    <p className="text-xs text-gray-500 dark:text-gray-400">
                      ≈ {fmtMoney(Number(loyaltyPoints) * 0.75)} value
                    </p>
                  )}

                  <button
                    onClick={handleLoyaltyAction}
                    disabled={addingPoints || redeemingPoints || !loyaltyPoints || !loyaltyReason}
                    className={cn(
                      'flex items-center gap-2 px-4 py-1.5 rounded-lg text-white text-xs font-medium transition-colors disabled:opacity-50',
                      loyaltyMode === 'add'
                        ? 'bg-emerald-600 hover:bg-emerald-700'
                        : 'bg-amber-600 hover:bg-amber-700',
                    )}
                  >
                    {addingPoints || redeemingPoints ? (
                      <Loader2 className="w-3.5 h-3.5 animate-spin" />
                    ) : loyaltyMode === 'add' ? (
                      <TrendingUp className="w-3.5 h-3.5" />
                    ) : (
                      <TrendingDown className="w-3.5 h-3.5" />
                    )}
                    {addingPoints || redeemingPoints
                      ? 'Processing…'
                      : loyaltyMode === 'add'
                        ? 'Add Points'
                        : 'Redeem Points'}
                  </button>
                </div>
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="px-6 py-3 border-t border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50 flex justify-end">
            <Dialog.Close className="px-4 py-2 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100 font-medium transition-colors">
              Close
            </Dialog.Close>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
