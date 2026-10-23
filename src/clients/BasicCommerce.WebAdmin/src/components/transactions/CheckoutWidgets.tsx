import { useState, useRef, useEffect } from 'react';
import { X, Loader2, Package, User, ScanLine, Star } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { ProductListItem } from '../../types/product';
import type { Customer } from '../../types/customer';
import type { CheckoutRewardPoints } from '../../types/rewardPoints';
import { useProductSearch } from '../../hooks/useProducts';
import { useCustomerSearch } from '../../hooks/useCustomers';

function fmtMoney(n: number) {
  return `৳${n.toLocaleString('en-BD', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

/* ── Product search (keyboard-first, scanner-friendly) ────────────────────── */

export function ProductSearch({ onSelect }: { onSelect: (p: ProductListItem) => void }) {
  const [term, setTerm]           = useState('');
  const [open, setOpen]           = useState(false);
  const [highlight, setHighlight] = useState(0);
  const wrapRef  = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const { data: rawResults = [], isFetching } = useProductSearch(term);
  const results = rawResults.filter((p) => p.status === 'Active');

  // Auto-focus on mount — cashier can start typing / scanning immediately
  useEffect(() => { inputRef.current?.focus(); }, []);

  useEffect(() => {
    function handler(e: MouseEvent) {
      if (wrapRef.current && !wrapRef.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  useEffect(() => { setHighlight(0); }, [term]);

  function pick(p: ProductListItem) {
    onSelect(p);
    setTerm('');
    setOpen(false);
    inputRef.current?.focus();
  }

  function handleKeyDown(e: React.KeyboardEvent) {
    if (!open || results.length === 0) {
      // Enter with an exact SKU (barcode scanner) — pick the exact match if present
      if (e.key === 'Enter' && term && results.length === 0) e.preventDefault();
      return;
    }
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      setHighlight((h) => Math.min(h + 1, results.length - 1));
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      setHighlight((h) => Math.max(h - 1, 0));
    } else if (e.key === 'Enter') {
      e.preventDefault();
      // Barcode scanners type the code and send Enter — prefer an exact SKU match
      const exact = results.find((p) => p.sku.toLowerCase() === term.trim().toLowerCase());
      pick(exact ?? results[highlight] ?? results[0]);
    } else if (e.key === 'Escape') {
      setOpen(false);
    }
  }

  return (
    <div ref={wrapRef} className="relative">
      <div className="relative">
        <ScanLine className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-primary-500 pointer-events-none" />
        <input
          ref={inputRef}
          value={term}
          onChange={(e) => { setTerm(e.target.value); setOpen(true); }}
          onFocus={() => term.length >= 2 && setOpen(true)}
          onKeyDown={handleKeyDown}
          placeholder="Scan barcode or search product name / SKU…"
          className="w-full pl-12 pr-12 py-3.5 text-base border-2 border-primary-200 dark:border-primary-900 bg-white dark:bg-gray-900 rounded-2xl text-gray-900 dark:text-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 placeholder-gray-400 shadow-sm transition"
        />
        {isFetching
          ? <Loader2 className="absolute right-4 top-1/2 -translate-y-1/2 w-5 h-5 animate-spin text-gray-400" />
          : term && (
            <button onClick={() => { setTerm(''); setOpen(false); inputRef.current?.focus(); }}
              className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 p-0.5">
              <X className="w-5 h-5" />
            </button>
          )
        }
      </div>

      {open && term.length >= 2 && (
        <div className="absolute top-full left-0 right-0 z-50 mt-2 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-2xl shadow-xl overflow-hidden">
          {results.length === 0 ? (
            <div className="px-4 py-8 text-center text-sm text-gray-400">
              {isFetching ? 'Searching…' : `No products match “${term}”`}
            </div>
          ) : (
            <>
              {results.map((p, idx) => (
                <button
                  key={p.id}
                  onClick={() => pick(p)}
                  onMouseEnter={() => setHighlight(idx)}
                  className={cn(
                    'w-full flex items-center gap-3 px-4 py-3 transition-colors text-left',
                    idx === highlight
                      ? 'bg-primary-50 dark:bg-primary-900/20'
                      : 'hover:bg-gray-50 dark:hover:bg-gray-700',
                  )}
                >
                  <div className="w-10 h-10 bg-gray-100 dark:bg-gray-700 rounded-xl flex items-center justify-center flex-shrink-0 overflow-hidden">
                    {p.imageUrl
                      ? <img src={p.imageUrl} alt="" className="w-full h-full object-cover" />
                      : <Package className="w-5 h-5 text-gray-400" />
                    }
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-gray-900 dark:text-white truncate">{p.name}</p>
                    <p className="text-xs text-gray-500 dark:text-gray-400 font-mono">{p.sku}</p>
                  </div>
                  <div className="text-right flex-shrink-0">
                    <p className="text-sm font-bold text-gray-900 dark:text-white tabular-nums">{fmtMoney(p.price)}</p>
                    {p.vatRate > 0 && <p className="text-xs text-gray-400">+{p.vatRate}% VAT</p>}
                  </div>
                </button>
              ))}
              <div className="px-4 py-2 bg-gray-50 dark:bg-gray-900/60 border-t border-gray-100 dark:border-gray-700 flex items-center gap-3 text-xs text-gray-400">
                <span><kbd className="px-1 py-0.5 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-600 rounded">↑↓</kbd> navigate</span>
                <span><kbd className="px-1 py-0.5 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-600 rounded">Enter</kbd> add to cart</span>
              </div>
            </>
          )}
        </div>
      )}
    </div>
  );
}

/* ── Customer picker ──────────────────────────────────────────────────────── */

export function CustomerPicker({
  value, onChange,
}: { value: Customer | null; onChange: (c: Customer | null) => void }) {
  const [term, setTerm] = useState('');
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  const { data: results = [], isFetching } = useCustomerSearch(term);

  useEffect(() => {
    function handler(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  if (value) {
    return (
      <div className="flex items-center gap-3 px-3 py-2.5 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-xl">
        <div className="w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900/40 flex items-center justify-center flex-shrink-0">
          <User className="w-4 h-4 text-blue-600 dark:text-blue-400" />
        </div>
        <div className="flex-1 min-w-0">
          <p className="text-sm font-semibold text-blue-900 dark:text-blue-300 truncate">{value.name}</p>
          <p className="text-xs text-blue-500 dark:text-blue-400">
            {value.phone ?? '—'}
          </p>
        </div>
        <button onClick={() => onChange(null)}
          className="p-1 text-blue-400 hover:text-blue-600 transition-colors" title="Remove customer">
          <X className="w-4 h-4" />
        </button>
      </div>
    );
  }

  return (
    <div ref={ref} className="relative">
      <div className="relative">
        <User className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
        <input
          value={term}
          onChange={(e) => { setTerm(e.target.value); setOpen(true); }}
          onFocus={() => term.length >= 2 && setOpen(true)}
          placeholder="Walk-in — search to attach customer"
          className="w-full pl-9 pr-4 py-2.5 text-sm border border-dashed border-gray-300 dark:border-gray-600 bg-transparent rounded-xl text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-solid placeholder-gray-400 transition"
        />
        {isFetching && (
          <Loader2 className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 animate-spin text-gray-400" />
        )}
      </div>

      {open && term.length >= 2 && results.length > 0 && (
        <div className="absolute top-full left-0 right-0 z-50 mt-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl shadow-xl overflow-hidden max-h-56 overflow-y-auto">
          {results.map((c: Customer) => (
            <button
              key={c.id}
              onClick={() => { onChange(c); setTerm(''); setOpen(false); }}
              className="w-full flex items-center justify-between gap-3 px-4 py-2.5 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors text-left"
            >
              <div className="min-w-0">
                <p className="text-sm font-medium text-gray-900 dark:text-white truncate">{c.name}</p>
                <p className="text-xs text-gray-500 dark:text-gray-400">{c.phone}</p>
              </div>
              {(c.loyaltyPoints ?? 0) > 0 && (
                <span className="text-xs text-amber-600 dark:text-amber-400 font-semibold whitespace-nowrap">
                  {c.loyaltyPoints} pts
                </span>
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

/* ── Reward points summary for the attached customer ─────────────────────── */

export function RewardPointsStrip({ points, hasItems }: {
  points: CheckoutRewardPoints | undefined;
  hasItems: boolean;
}) {
  if (!points) {
    return (
      <div className="mt-3 h-12 rounded-xl bg-gray-50 dark:bg-gray-800/60 animate-pulse" />
    );
  }
  if (!points.programEnabled) return null;

  return (
    <div className="mt-3 px-3 py-2.5 rounded-xl bg-amber-50 dark:bg-amber-900/15 border border-amber-200/70 dark:border-amber-800/60">
      <div className="flex items-center justify-between gap-3">
        <div className="flex items-center gap-2 min-w-0">
          <Star className="w-4 h-4 text-amber-500 fill-amber-400 flex-shrink-0" />
          <div className="min-w-0">
            <p className="text-sm font-bold text-amber-900 dark:text-amber-300 tabular-nums">
              {points.availablePoints.toLocaleString()} pts
            </p>
            <p className="text-[11px] text-amber-700/80 dark:text-amber-400/80">
              Reward balance
              {points.pendingPoints > 0 && ` · +${points.pendingPoints.toLocaleString()} pending`}
            </p>
          </div>
        </div>
        <div className="text-right flex-shrink-0">
          <p className="text-sm font-bold text-amber-900 dark:text-amber-300 tabular-nums">
            {fmtMoney(points.availableValue)}
          </p>
          <p className="text-[11px] text-amber-700/80 dark:text-amber-400/80">worth</p>
        </div>
      </div>
      {hasItems && (
        <div className="mt-2 pt-2 border-t border-amber-200/70 dark:border-amber-800/60 flex flex-wrap items-center justify-between gap-x-3 gap-y-1 text-[11px]">
          <span className="text-amber-800 dark:text-amber-300">
            {points.maxRedeemableAmount > 0
              ? <>Can use up to <b className="tabular-nums">{fmtMoney(points.maxRedeemableAmount)}</b> on this sale</>
              : points.availablePoints > 0 && points.availablePoints < points.minimumPointsToUse
                ? `Needs ${points.minimumPointsToUse} pts to redeem`
                : 'No points usable on this sale'}
          </span>
          {points.pointsToEarn > 0 && (
            <span className="font-semibold text-emerald-700 dark:text-emerald-400 tabular-nums">
              +{points.pointsToEarn.toLocaleString()} pts {points.pointsActivateImmediately ? 'earned' : 'pending'} on this sale
            </span>
          )}
        </div>
      )}
    </div>
  );
}
