import { useState } from 'react';
import { Plus, X, AlertCircle, CheckCircle2 } from 'lucide-react';
import { cn } from '../../lib/utils';
import { PaymentMethodIcon } from './PaymentMethodIcon';
import { PAYMENT_METHODS, MOBILE_WALLET_METHODS, paymentMethodLabel } from '../../lib/paymentMethods';

export interface DraftPayment {
  tempId: string;
  method: string;
  amount: number;
  reference?: string;
  mobileNumber?: string;
  giftCardCode?: string;
}

function fmtMoney(n: number) {
  return `৳${n.toLocaleString('en-BD', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

interface Props {
  total: number;
  payments: DraftPayment[];
  onAdd: (payment: DraftPayment) => void;
  onRemove: (tempId: string) => void;
}

export function SplitPaymentPanel({ total, payments, onAdd, onRemove }: Props) {
  const paidSoFar   = payments.reduce((s, p) => s + p.amount, 0);
  const balanceDue  = Math.round((total - paidSoFar) * 100) / 100;
  const isPaidInFull = balanceDue <= 0.005;

  const [method,   setMethod]   = useState('Cash');
  const [amount,   setAmount]   = useState(total > 0 ? total.toFixed(2) : '');
  const [mobile,   setMobile]   = useState('');
  const [giftCode, setGiftCode] = useState('');
  const [error,    setError]    = useState('');

  const isCash        = method === 'Cash';
  const needsMobile   = MOBILE_WALLET_METHODS.includes(method);
  const needsGiftCard = method === 'GiftCard';
  const amountNum     = parseFloat(amount) || 0;

  function pickMethod(m: string) {
    setMethod(m);
    setError('');
    setAmount(Math.max(balanceDue, 0).toFixed(2));
    setMobile('');
    setGiftCode('');
  }

  function handleAdd() {
    if (amountNum <= 0) {
      setError('Enter an amount greater than zero.');
      return;
    }
    if (!isCash && amountNum > balanceDue + 0.01) {
      setError(`Amount exceeds the balance due (${fmtMoney(Math.max(balanceDue, 0))}). Only cash can exceed the balance to give change.`);
      return;
    }
    if (needsMobile && !mobile.trim()) {
      setError('Mobile number is required.');
      return;
    }
    if (needsGiftCard && !giftCode.trim()) {
      setError('Gift card code is required.');
      return;
    }

    onAdd({
      tempId:       crypto.randomUUID(),
      method,
      amount:       amountNum,
      mobileNumber: needsMobile ? mobile.trim() : undefined,
      giftCardCode: needsGiftCard ? giftCode.trim() : undefined,
    });

    const nextBalance = Math.max(balanceDue - amountNum, 0);
    setAmount(nextBalance > 0 ? nextBalance.toFixed(2) : '');
    setMobile('');
    setGiftCode('');
    setError('');
  }

  return (
    <div className="space-y-3">
      {/* Added payment lines */}
      {payments.length > 0 && (
        <div className="space-y-1.5">
          {payments.map((p) => (
            <div key={p.tempId} className="flex items-center gap-2.5 px-3 py-2 bg-gray-50 dark:bg-gray-800 rounded-xl">
              <PaymentMethodIcon method={p.method} size={18} />
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300 flex-1 truncate">
                {paymentMethodLabel(p.method)}
                {p.mobileNumber && <span className="text-xs text-gray-400 font-normal"> · {p.mobileNumber}</span>}
                {p.giftCardCode && <span className="text-xs text-gray-400 font-mono font-normal"> · {p.giftCardCode}</span>}
              </span>
              <span className="text-sm font-bold tabular-nums text-gray-900 dark:text-white">{fmtMoney(p.amount)}</span>
              <button
                onClick={() => onRemove(p.tempId)}
                className="text-gray-400 hover:text-red-600 transition-colors p-0.5 flex-shrink-0"
                title="Remove payment"
              >
                <X className="w-3.5 h-3.5" />
              </button>
            </div>
          ))}
        </div>
      )}

      {/* Balance banner */}
      {payments.length > 0 && (
        <div className={cn(
          'flex items-center justify-between px-3.5 py-2.5 rounded-xl border',
          isPaidInFull
            ? 'bg-emerald-50 dark:bg-emerald-900/20 border-emerald-200 dark:border-emerald-800'
            : 'bg-amber-50 dark:bg-amber-900/20 border-amber-200 dark:border-amber-800',
        )}>
          <span className={cn(
            'text-sm font-semibold',
            isPaidInFull ? 'text-emerald-700 dark:text-emerald-400' : 'text-amber-700 dark:text-amber-400',
          )}>
            {balanceDue < -0.005 ? 'Change due' : isPaidInFull ? 'Fully paid' : 'Balance due'}
          </span>
          <span className={cn(
            'text-lg font-extrabold tabular-nums',
            isPaidInFull ? 'text-emerald-700 dark:text-emerald-400' : 'text-amber-700 dark:text-amber-400',
          )}>
            {fmtMoney(Math.abs(balanceDue))}
          </span>
        </div>
      )}

      {/* Add-payment mini form — hidden once fully paid */}
      {!isPaidInFull && (
        <div className="p-3 border border-dashed border-gray-300 dark:border-gray-700 rounded-xl space-y-2.5">
          <div className="flex flex-wrap gap-1.5">
            {PAYMENT_METHODS.map((m) => (
              <button
                key={m.value}
                onClick={() => pickMethod(m.value)}
                title={m.hint}
                className={cn(
                  'flex items-center gap-1.5 pl-1.5 pr-2.5 py-1 rounded-full border text-xs font-semibold transition-all',
                  method === m.value
                    ? 'border-primary-600 bg-primary-50 dark:bg-primary-900/20 text-primary-700 dark:text-primary-400'
                    : 'border-gray-200 dark:border-gray-700 text-gray-500 dark:text-gray-400 hover:border-gray-300 dark:hover:border-gray-600',
                )}
              >
                <PaymentMethodIcon method={m.value} size={16} />
                {m.label}
              </button>
            ))}
          </div>

          <div className="flex gap-2">
            <div className="relative flex-1">
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm font-semibold pointer-events-none">৳</span>
              <input
                type="number"
                inputMode="decimal"
                min={0}
                value={amount}
                onChange={(e) => { setAmount(e.target.value); setError(''); }}
                placeholder="0.00"
                className="w-full pl-7 pr-3 py-2 text-sm font-bold tabular-nums border border-gray-300 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
              />
            </div>
            <button
              onClick={handleAdd}
              className="flex items-center gap-1.5 px-4 py-2 text-sm font-semibold text-white bg-primary-700 hover:bg-primary-800 rounded-lg transition flex-shrink-0"
            >
              <Plus className="w-4 h-4" />
              Add
            </button>
          </div>

          {needsMobile && (
            <input
              type="tel"
              value={mobile}
              onChange={(e) => setMobile(e.target.value)}
              placeholder="Mobile number *"
              className="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
          )}
          {needsGiftCard && (
            <input
              type="text"
              value={giftCode}
              onChange={(e) => setGiftCode(e.target.value.toUpperCase())}
              placeholder="Gift card code *"
              className="w-full px-3 py-2 text-sm font-mono border border-gray-300 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500 uppercase"
            />
          )}

          {error && (
            <p className="text-xs text-red-600 dark:text-red-400 flex items-center gap-1.5">
              <AlertCircle className="w-3.5 h-3.5 flex-shrink-0" /> {error}
            </p>
          )}
        </div>
      )}

      {isPaidInFull && payments.length > 0 && (
        <p className="text-xs text-emerald-600 dark:text-emerald-400 flex items-center gap-1.5">
          <CheckCircle2 className="w-3.5 h-3.5 flex-shrink-0" /> All set — ready to complete the sale.
        </p>
      )}
    </div>
  );
}
