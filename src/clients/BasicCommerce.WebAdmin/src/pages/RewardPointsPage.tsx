import { useState, useEffect, useRef } from 'react';
import {
  Gift, Settings, Search, User, Plus, Minus,
  CheckCircle, Clock, TrendingUp, TrendingDown, AlertCircle,
  Loader2, ChevronRight, Save, Star,
} from 'lucide-react';
import { cn, extractApiError } from '../lib/utils';
import type { RewardPointsSettings } from '../types/rewardPoints';
import {
  useRewardPointsSettings,
  useUpdateRewardPointsSettings,
  useCustomerRewardPoints,
  useManualAdjust,
} from '../hooks/useRewardPoints';
import { useCustomerSearch } from '../hooks/useCustomers';
import type { Customer } from '../types/customer';

// ── Helpers ───────────────────────────────────────────────────────────────────

function fmt(n: number, decimals = 2) {
  return n.toLocaleString(undefined, { minimumFractionDigits: decimals, maximumFractionDigits: decimals });
}

function fmtDate(d: string | null | undefined) {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

function fmtDateTime(d: string) {
  return new Date(d).toLocaleString('en-GB', {
    day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit',
  });
}

const ENTRY_TYPE_CONFIG: Record<string, { label: string; color: string; icon: React.ReactNode }> = {
  PurchaseEarned:    { label: 'Purchase',       color: 'text-emerald-600 dark:text-emerald-400', icon: <TrendingUp className="w-3.5 h-3.5" /> },
  RegistrationEarned:{ label: 'Sign-up Bonus', color: 'text-blue-600 dark:text-blue-400',    icon: <Star className="w-3.5 h-3.5" /> },
  Redeemed:          { label: 'Redeemed',       color: 'text-red-600 dark:text-red-400',      icon: <TrendingDown className="w-3.5 h-3.5" /> },
  Expired:           { label: 'Expired',        color: 'text-gray-400',                       icon: <Clock className="w-3.5 h-3.5" /> },
  ManualAdjustment:  { label: 'Manual Adjust',  color: 'text-purple-600 dark:text-purple-400',icon: <Settings className="w-3.5 h-3.5" /> },
};

// ── Settings Form ─────────────────────────────────────────────────────────────

type SettingsForm = {
  isEnabled: boolean;
  purchaseSpendPerPoint: string;
  pointsEarnedPerSpend: string;
  exchangeRate: string;
  minimumPointsToUse: string;
  maximumPointsPerOrder: string;
  maximumRedeemedRate: string;
  purchasePointsValidityDays: string;
  minimumOrderTotalForPoints: string;
  pointsForRegistration: string;
  registrationPointsValidityDays: string;
  activatePointsImmediately: boolean;
  displayHowMuchWillBeEarned: boolean;
  pointsAccumulatedForAllStores: boolean;
};

function toForm(s: RewardPointsSettings): SettingsForm {
  return {
    isEnabled: s.isEnabled,
    purchaseSpendPerPoint: String(s.purchaseSpendPerPoint),
    pointsEarnedPerSpend: String(s.pointsEarnedPerSpend),
    exchangeRate: String(s.exchangeRate),
    minimumPointsToUse: String(s.minimumPointsToUse),
    maximumPointsPerOrder: String(s.maximumPointsPerOrder),
    maximumRedeemedRate: String(Math.round(s.maximumRedeemedRate * 100)),
    purchasePointsValidityDays: String(s.purchasePointsValidityDays),
    minimumOrderTotalForPoints: String(s.minimumOrderTotalForPoints),
    pointsForRegistration: String(s.pointsForRegistration),
    registrationPointsValidityDays: String(s.registrationPointsValidityDays),
    activatePointsImmediately: s.activatePointsImmediately,
    displayHowMuchWillBeEarned: s.displayHowMuchWillBeEarned,
    pointsAccumulatedForAllStores: s.pointsAccumulatedForAllStores,
  };
}

function Toggle({ value, onChange, disabled }: { value: boolean; onChange: (v: boolean) => void; disabled?: boolean }) {
  return (
    <button
      type="button"
      onClick={() => onChange(!value)}
      disabled={disabled}
      className={cn(
        'relative w-10 h-5.5 rounded-full transition-colors flex-shrink-0 focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500',
        value ? 'bg-primary-700' : 'bg-gray-300 dark:bg-gray-600',
        disabled && 'opacity-50 cursor-not-allowed',
      )}
      style={{ height: '1.375rem' }}
    >
      <span className={cn(
        'absolute top-0.5 w-4 h-4 rounded-full bg-white shadow transition-all',
        value ? 'left-5' : 'left-0.5',
      )} />
    </button>
  );
}

function FieldLabel({ children }: { children: React.ReactNode }) {
  return <p className="text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">{children}</p>;
}

function SectionTitle({ children }: { children: React.ReactNode }) {
  return (
    <p className="text-xs font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500 mb-3 flex items-center gap-1.5">
      {children}
    </p>
  );
}

const inputCls = 'w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 px-3 py-2 text-sm text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 transition';

// ── Main Page ─────────────────────────────────────────────────────────────────

export default function RewardPointsPage() {
  const { data: settings, isLoading: settingsLoading } = useRewardPointsSettings();
  const updateMutation = useUpdateRewardPointsSettings();
  const adjustMutation = useManualAdjust();

  const [form, setForm]               = useState<SettingsForm | null>(null);
  const [saveError, setSaveError]     = useState<string | null>(null);
  const [savedOk, setSavedOk]         = useState(false);
  const [isDirty, setIsDirty]         = useState(false);

  const [customerSearch, setCustomerSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [searchOpen, setSearchOpen]   = useState(false);
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);
  const searchRef = useRef<HTMLDivElement>(null);

  const [adjustPoints, setAdjustPoints] = useState('');
  const [adjustNotes, setAdjustNotes]   = useState('');
  const [adjustDir, setAdjustDir]       = useState<'add' | 'subtract'>('add');
  const [adjustError, setAdjustError]   = useState<string | null>(null);
  const [adjustOk, setAdjustOk]         = useState(false);
  const [showAdjust, setShowAdjust]     = useState(false);

  useEffect(() => {
    if (settings && !form) setForm(toForm(settings));
  }, [settings, form]);

  useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(customerSearch), 300);
    return () => clearTimeout(t);
  }, [customerSearch]);

  useEffect(() => {
    if (!searchOpen) return;
    function handler(e: MouseEvent) {
      if (!searchRef.current?.contains(e.target as Node)) setSearchOpen(false);
    }
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, [searchOpen]);

  const { data: searchResults = [], isFetching: searchFetching } = useCustomerSearch(debouncedSearch, searchOpen);
  const { data: account, isFetching: accountFetching } = useCustomerRewardPoints(
    selectedCustomer?.id ?? null,
  );

  function setField<K extends keyof SettingsForm>(k: K, v: SettingsForm[K]) {
    setForm((f) => f ? { ...f, [k]: v } : f);
    setIsDirty(true);
    setSavedOk(false);
  }

  async function handleSave(e: React.FormEvent) {
    e.preventDefault();
    if (!form) return;
    try {
      setSaveError(null);
      await updateMutation.mutateAsync({
        isEnabled: form.isEnabled,
        purchaseSpendPerPoint: parseFloat(form.purchaseSpendPerPoint) || 100,
        pointsEarnedPerSpend: parseInt(form.pointsEarnedPerSpend) || 1,
        exchangeRate: parseFloat(form.exchangeRate) || 0.75,
        minimumPointsToUse: parseInt(form.minimumPointsToUse) || 1000,
        maximumPointsPerOrder: parseInt(form.maximumPointsPerOrder) || 0,
        maximumRedeemedRate: (parseFloat(form.maximumRedeemedRate) || 100) / 100,
        purchasePointsValidityDays: parseInt(form.purchasePointsValidityDays) || 365,
        minimumOrderTotalForPoints: parseFloat(form.minimumOrderTotalForPoints) || 0,
        pointsForRegistration: parseInt(form.pointsForRegistration) || 0,
        registrationPointsValidityDays: parseInt(form.registrationPointsValidityDays) || 365,
        activatePointsImmediately: form.activatePointsImmediately,
        displayHowMuchWillBeEarned: form.displayHowMuchWillBeEarned,
        pointsAccumulatedForAllStores: form.pointsAccumulatedForAllStores,
      });
      setIsDirty(false);
      setSavedOk(true);
      setTimeout(() => setSavedOk(false), 3000);
    } catch (err) {
      setSaveError(extractApiError(err));
    }
  }

  async function handleAdjust(e: React.FormEvent) {
    e.preventDefault();
    if (!selectedCustomer) return;
    const pts = parseInt(adjustPoints);
    if (!pts || pts <= 0) return;
    try {
      setAdjustError(null);
      await adjustMutation.mutateAsync({
        customerId: selectedCustomer.id,
        points: adjustDir === 'add' ? pts : -pts,
        notes: adjustNotes.trim() || `Manual ${adjustDir === 'add' ? 'addition' : 'deduction'}`,
      });
      setAdjustPoints(''); setAdjustNotes('');
      setAdjustOk(true); setTimeout(() => setAdjustOk(false), 3000);
      setShowAdjust(false);
    } catch (err) {
      setAdjustError(extractApiError(err));
    }
  }

  // Live preview calculation
  const spendPerPt = parseFloat(form?.purchaseSpendPerPoint ?? '100') || 100;
  const ptsPerSpend = parseInt(form?.pointsEarnedPerSpend ?? '1') || 1;
  const exRate = parseFloat(form?.exchangeRate ?? '0.75') || 0.75;
  const minPts = parseInt(form?.minimumPointsToUse ?? '1000') || 1000;
  const exampleSpend = 500;
  const examplePts = Math.floor(exampleSpend / spendPerPt) * ptsPerSpend;
  const exampleValue = examplePts * exRate;

  return (
    <div className="space-y-5">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
          <Gift className="w-6 h-6 text-primary-700" />
          Reward Points
        </h1>
        <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
          Configure earn & redemption rules, and manage customer point balances
        </p>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-5 gap-5">

        {/* ── Settings Panel (left, 3 cols) ── */}
        <div className="xl:col-span-3 space-y-4">
          <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm">
            {/* Settings header */}
            <div className="flex items-center justify-between px-5 py-4 border-b border-gray-100 dark:border-gray-800">
              <div className="flex items-center gap-2">
                <Settings className="w-4 h-4 text-primary-700" />
                <h2 className="text-sm font-semibold text-gray-900 dark:text-white">Configuration</h2>
              </div>
              {form && (
                <div className="flex items-center gap-2">
                  <span className="text-xs text-gray-500 dark:text-gray-400">Program</span>
                  <Toggle value={form.isEnabled} onChange={(v) => setField('isEnabled', v)} />
                  <span className={cn('text-xs font-medium', form.isEnabled ? 'text-emerald-600 dark:text-emerald-400' : 'text-gray-400')}>
                    {form.isEnabled ? 'Active' : 'Inactive'}
                  </span>
                </div>
              )}
            </div>

            {settingsLoading || !form ? (
              <div className="flex items-center justify-center py-16 text-gray-400">
                <Loader2 className="w-5 h-5 animate-spin" />
              </div>
            ) : (
              <form onSubmit={handleSave} noValidate>
                <div className="px-5 py-5 space-y-6">

                  {/* ── Earning Rules ── */}
                  <div>
                    <SectionTitle><TrendingUp className="w-3.5 h-3.5" /> Earning Rules</SectionTitle>
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <FieldLabel>Spend per point (৳)</FieldLabel>
                        <input type="number" min={1} step={1} value={form.purchaseSpendPerPoint}
                          onChange={(e) => setField('purchaseSpendPerPoint', e.target.value)}
                          className={inputCls} placeholder="100" />
                        <p className="mt-1 text-xs text-gray-400">Customer spends this amount to earn points</p>
                      </div>
                      <div>
                        <FieldLabel>Points earned per spend</FieldLabel>
                        <input type="number" min={1} step={1} value={form.pointsEarnedPerSpend}
                          onChange={(e) => setField('pointsEarnedPerSpend', e.target.value)}
                          className={inputCls} placeholder="1" />
                        <p className="mt-1 text-xs text-gray-400">Points awarded for each spend unit</p>
                      </div>
                      <div>
                        <FieldLabel>Min. order total to earn (৳)</FieldLabel>
                        <input type="number" min={0} step={1} value={form.minimumOrderTotalForPoints}
                          onChange={(e) => setField('minimumOrderTotalForPoints', e.target.value)}
                          className={inputCls} placeholder="0" />
                        <p className="mt-1 text-xs text-gray-400">0 = no minimum</p>
                      </div>
                      <div>
                        <FieldLabel>Points validity (days)</FieldLabel>
                        <input type="number" min={1} step={1} value={form.purchasePointsValidityDays}
                          onChange={(e) => setField('purchasePointsValidityDays', e.target.value)}
                          className={inputCls} placeholder="365" />
                        <p className="mt-1 text-xs text-gray-400">Days before earned points expire</p>
                      </div>
                    </div>

                    <div className="mt-4 space-y-3">
                      <div className="flex items-center justify-between py-2.5 px-3 rounded-lg bg-gray-50 dark:bg-gray-800">
                        <div>
                          <p className="text-sm text-gray-700 dark:text-gray-300">Activate points immediately</p>
                          <p className="text-xs text-gray-400 mt-0.5">If off, points stay pending until confirmed</p>
                        </div>
                        <Toggle value={form.activatePointsImmediately}
                          onChange={(v) => setField('activatePointsImmediately', v)} />
                      </div>
                      <div className="flex items-center justify-between py-2.5 px-3 rounded-lg bg-gray-50 dark:bg-gray-800">
                        <div>
                          <p className="text-sm text-gray-700 dark:text-gray-300">Show earnings preview at checkout</p>
                          <p className="text-xs text-gray-400 mt-0.5">Display "You'll earn X pts" on POS screen</p>
                        </div>
                        <Toggle value={form.displayHowMuchWillBeEarned}
                          onChange={(v) => setField('displayHowMuchWillBeEarned', v)} />
                      </div>
                      <div className="flex items-center justify-between py-2.5 px-3 rounded-lg bg-gray-50 dark:bg-gray-800">
                        <div>
                          <p className="text-sm text-gray-700 dark:text-gray-300">Shared wallet across all stores</p>
                          <p className="text-xs text-gray-400 mt-0.5">If off, points are tracked per store separately</p>
                        </div>
                        <Toggle value={form.pointsAccumulatedForAllStores}
                          onChange={(v) => setField('pointsAccumulatedForAllStores', v)} />
                      </div>
                    </div>
                  </div>

                  {/* ── Redemption Rules ── */}
                  <div>
                    <SectionTitle><TrendingDown className="w-3.5 h-3.5" /> Redemption Rules</SectionTitle>
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <FieldLabel>Point value (৳ per point)</FieldLabel>
                        <input type="number" min={0.01} step={0.01} value={form.exchangeRate}
                          onChange={(e) => setField('exchangeRate', e.target.value)}
                          className={inputCls} placeholder="0.75" />
                        <p className="mt-1 text-xs text-gray-400">Shwapno standard: ৳0.75 per point</p>
                      </div>
                      <div>
                        <FieldLabel>Minimum points to redeem</FieldLabel>
                        <input type="number" min={1} step={1} value={form.minimumPointsToUse}
                          onChange={(e) => setField('minimumPointsToUse', e.target.value)}
                          className={inputCls} placeholder="1000" />
                        <p className="mt-1 text-xs text-gray-400">Shwapno standard: 1,000 pts</p>
                      </div>
                      <div>
                        <FieldLabel>Max points per order (0 = unlimited)</FieldLabel>
                        <input type="number" min={0} step={1} value={form.maximumPointsPerOrder}
                          onChange={(e) => setField('maximumPointsPerOrder', e.target.value)}
                          className={inputCls} placeholder="0" />
                      </div>
                      <div>
                        <FieldLabel>Max % of order payable by points</FieldLabel>
                        <div className="relative">
                          <input type="number" min={1} max={100} step={1} value={form.maximumRedeemedRate}
                            onChange={(e) => setField('maximumRedeemedRate', e.target.value)}
                            className={cn(inputCls, 'pr-8')} placeholder="100" />
                          <span className="absolute right-3 top-1/2 -translate-y-1/2 text-sm text-gray-400">%</span>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* ── Sign-up Bonus ── */}
                  <div>
                    <SectionTitle><Star className="w-3.5 h-3.5" /> Sign-up Bonus</SectionTitle>
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <FieldLabel>Points on registration</FieldLabel>
                        <input type="number" min={0} step={1} value={form.pointsForRegistration}
                          onChange={(e) => setField('pointsForRegistration', e.target.value)}
                          className={inputCls} placeholder="0" />
                        <p className="mt-1 text-xs text-gray-400">0 = no sign-up bonus</p>
                      </div>
                      <div>
                        <FieldLabel>Sign-up bonus validity (days)</FieldLabel>
                        <input type="number" min={1} step={1} value={form.registrationPointsValidityDays}
                          onChange={(e) => setField('registrationPointsValidityDays', e.target.value)}
                          className={inputCls} placeholder="365" />
                      </div>
                    </div>
                  </div>

                  {/* ── Live Preview ── */}
                  <div className="rounded-xl border border-primary-200 dark:border-primary-800 bg-primary-50 dark:bg-primary-900/20 p-4">
                    <p className="text-xs font-semibold text-primary-700 dark:text-primary-400 mb-3 uppercase tracking-wider">
                      Live Preview
                    </p>
                    <div className="flex items-center gap-2 flex-wrap text-sm">
                      <span className="text-gray-600 dark:text-gray-300">Spend</span>
                      <span className="font-semibold text-gray-900 dark:text-white">৳{exampleSpend.toLocaleString()}</span>
                      <ChevronRight className="w-3.5 h-3.5 text-gray-400" />
                      <span className="text-gray-600 dark:text-gray-300">earn</span>
                      <span className="font-semibold text-primary-700 dark:text-primary-400">{examplePts} pts</span>
                      <ChevronRight className="w-3.5 h-3.5 text-gray-400" />
                      <span className="text-gray-600 dark:text-gray-300">worth</span>
                      <span className="font-semibold text-emerald-600 dark:text-emerald-400">৳{fmt(exampleValue)}</span>
                    </div>
                    <p className="mt-2 text-xs text-gray-500 dark:text-gray-400">
                      Effective cashback: {fmt((examplePts * exRate / exampleSpend) * 100, 2)}% · Min. to redeem: {minPts.toLocaleString()} pts (= ৳{fmt(minPts * exRate)})
                    </p>
                  </div>

                  {/* Error / success */}
                  {saveError && (
                    <div className="flex items-center gap-2 rounded-lg bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 px-3 py-2.5 text-sm text-red-600 dark:text-red-400">
                      <AlertCircle className="w-4 h-4 flex-shrink-0" />{saveError}
                    </div>
                  )}
                  {savedOk && (
                    <div className="flex items-center gap-2 rounded-lg bg-emerald-50 dark:bg-emerald-900/20 border border-emerald-200 dark:border-emerald-800 px-3 py-2.5 text-sm text-emerald-600 dark:text-emerald-400">
                      <CheckCircle className="w-4 h-4 flex-shrink-0" />Settings saved successfully.
                    </div>
                  )}
                </div>

                <div className="flex justify-end px-5 py-4 border-t border-gray-100 dark:border-gray-800">
                  <button
                    type="submit"
                    disabled={updateMutation.isPending || !isDirty}
                    className="flex items-center gap-2 px-5 py-2 text-sm font-medium text-white bg-primary-800 hover:bg-primary-900 rounded-lg transition-colors disabled:opacity-50"
                  >
                    {updateMutation.isPending
                      ? <Loader2 className="w-4 h-4 animate-spin" />
                      : <Save className="w-4 h-4" />}
                    Save Settings
                  </button>
                </div>
              </form>
            )}
          </div>
        </div>

        {/* ── Customer Lookup (right, 2 cols) ── */}
        <div className="xl:col-span-2 space-y-4">
          <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm">
            <div className="px-5 py-4 border-b border-gray-100 dark:border-gray-800 flex items-center gap-2">
              <User className="w-4 h-4 text-primary-700" />
              <h2 className="text-sm font-semibold text-gray-900 dark:text-white">Customer Lookup</h2>
            </div>

            <div className="px-5 py-4 space-y-4">
              {/* Search */}
              <div ref={searchRef} className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-gray-400" />
                <input
                  type="text"
                  value={customerSearch}
                  onChange={(e) => { setCustomerSearch(e.target.value); setSearchOpen(true); }}
                  onFocus={() => setSearchOpen(true)}
                  placeholder="Search by name, phone, email…"
                  className={cn(inputCls, 'pl-8')}
                />
                {searchOpen && customerSearch.length >= 2 && (
                  <div className="absolute z-20 mt-1 w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 shadow-lg overflow-hidden">
                    {searchFetching && (
                      <div className="px-3 py-2.5 text-sm text-gray-400 flex items-center gap-2">
                        <Loader2 className="w-3.5 h-3.5 animate-spin" />Searching…
                      </div>
                    )}
                    {!searchFetching && searchResults.length === 0 && (
                      <div className="px-3 py-2.5 text-sm text-gray-400 italic">No customers found.</div>
                    )}
                    {!searchFetching && searchResults.map((c) => (
                      <button
                        key={c.id}
                        type="button"
                        onMouseDown={(e) => {
                          e.preventDefault();
                          setSelectedCustomer(c);
                          setCustomerSearch('');
                          setSearchOpen(false);
                          setShowAdjust(false);
                        }}
                        className="w-full text-left px-3 py-2.5 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors border-b border-gray-100 dark:border-gray-800 last:border-0"
                      >
                        <p className="text-sm font-medium text-gray-900 dark:text-white">{c.name}</p>
                        <p className="text-xs text-gray-400">{c.phone ?? c.email ?? c.code}</p>
                      </button>
                    ))}
                  </div>
                )}
              </div>

              {/* No customer selected */}
              {!selectedCustomer && (
                <div className="flex flex-col items-center py-10 text-center">
                  <User className="w-8 h-8 text-gray-300 dark:text-gray-600 mb-2" />
                  <p className="text-sm text-gray-400">Search for a customer to view their points</p>
                </div>
              )}

              {/* Customer detail */}
              {selectedCustomer && (
                <div className="space-y-4">
                  {/* Customer header */}
                  <div className="flex items-start justify-between">
                    <div>
                      <p className="font-semibold text-gray-900 dark:text-white">{selectedCustomer.name}</p>
                      <p className="text-xs text-gray-400">{selectedCustomer.phone ?? selectedCustomer.email ?? selectedCustomer.code}</p>
                    </div>
                    <button
                      onClick={() => { setSelectedCustomer(null); setShowAdjust(false); }}
                      className="text-xs text-gray-400 hover:text-gray-600 dark:hover:text-gray-200"
                    >
                      Clear
                    </button>
                  </div>

                  {accountFetching && (
                    <div className="flex items-center justify-center py-6 text-gray-400">
                      <Loader2 className="w-4 h-4 animate-spin" />
                    </div>
                  )}

                  {account && (
                    <>
                      {/* Stats */}
                      <div className="grid grid-cols-2 gap-2">
                        {[
                          { label: 'Available', value: account.availablePoints, color: 'text-primary-700 dark:text-primary-400', bg: 'bg-primary-50 dark:bg-primary-900/20 border-primary-200 dark:border-primary-800' },
                          { label: 'Pending',   value: account.pendingPoints,   color: 'text-amber-600 dark:text-amber-400',   bg: 'bg-amber-50 dark:bg-amber-900/20 border-amber-200 dark:border-amber-800' },
                          { label: 'Total Earned', value: account.totalEarnedPoints, color: 'text-emerald-600 dark:text-emerald-400', bg: 'bg-gray-50 dark:bg-gray-800 border-gray-200 dark:border-gray-700' },
                          { label: 'Used',      value: account.usedPoints,      color: 'text-gray-500 dark:text-gray-400',    bg: 'bg-gray-50 dark:bg-gray-800 border-gray-200 dark:border-gray-700' },
                        ].map(({ label, value, color, bg }) => (
                          <div key={label} className={cn('rounded-lg border p-3', bg)}>
                            <p className="text-xs text-gray-500 dark:text-gray-400 mb-1">{label}</p>
                            <p className={cn('text-xl font-bold tabular-nums', color)}>{value.toLocaleString()}</p>
                            <p className="text-xs text-gray-400 mt-0.5">
                              ≈ ৳{fmt(value * exRate)}
                            </p>
                          </div>
                        ))}
                      </div>

                      {/* Manual adjust button */}
                      {!showAdjust && (
                        <button
                          onClick={() => setShowAdjust(true)}
                          className="w-full flex items-center justify-center gap-2 py-2 text-sm font-medium text-primary-700 dark:text-primary-400 border border-primary-200 dark:border-primary-700 rounded-lg hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors"
                        >
                          <Settings className="w-3.5 h-3.5" />
                          Manual Adjustment
                        </button>
                      )}

                      {/* Manual adjust form */}
                      {showAdjust && (
                        <form onSubmit={handleAdjust} className="space-y-3 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50 p-4">
                          <p className="text-xs font-semibold text-gray-600 dark:text-gray-400 uppercase tracking-wider">Manual Adjustment</p>
                          <div className="flex rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
                            {(['add', 'subtract'] as const).map((d) => (
                              <button
                                key={d} type="button"
                                onClick={() => setAdjustDir(d)}
                                className={cn(
                                  'flex-1 flex items-center justify-center gap-1.5 py-2 text-sm font-medium transition-colors',
                                  adjustDir === d
                                    ? d === 'add'
                                      ? 'bg-emerald-600 text-white'
                                      : 'bg-red-600 text-white'
                                    : 'bg-white dark:bg-gray-900 text-gray-600 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-800',
                                )}
                              >
                                {d === 'add' ? <Plus className="w-3.5 h-3.5" /> : <Minus className="w-3.5 h-3.5" />}
                                {d === 'add' ? 'Add' : 'Subtract'}
                              </button>
                            ))}
                          </div>
                          <input type="number" min={1} step={1} value={adjustPoints}
                            onChange={(e) => setAdjustPoints(e.target.value)}
                            placeholder="Points" className={inputCls} />
                          <input type="text" value={adjustNotes}
                            onChange={(e) => setAdjustNotes(e.target.value)}
                            placeholder="Reason / notes…" className={inputCls} />
                          {adjustError && (
                            <p className="text-xs text-red-500 flex items-center gap-1">
                              <AlertCircle className="w-3.5 h-3.5" />{adjustError}
                            </p>
                          )}
                          {adjustOk && (
                            <p className="text-xs text-emerald-600 dark:text-emerald-400 flex items-center gap-1">
                              <CheckCircle className="w-3.5 h-3.5" />Adjustment applied.
                            </p>
                          )}
                          <div className="flex gap-2">
                            <button type="button" onClick={() => setShowAdjust(false)}
                              className="flex-1 py-2 text-sm text-gray-600 dark:text-gray-400 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors">
                              Cancel
                            </button>
                            <button type="submit" disabled={adjustMutation.isPending || !adjustPoints}
                              className="flex-1 flex items-center justify-center gap-1.5 py-2 text-sm font-medium text-white bg-primary-800 hover:bg-primary-900 rounded-lg transition-colors disabled:opacity-50">
                              {adjustMutation.isPending ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : null}
                              Apply
                            </button>
                          </div>
                        </form>
                      )}

                      {/* History */}
                      {account.recentEntries.length > 0 && (
                        <div>
                          <p className="text-xs font-semibold uppercase tracking-wider text-gray-400 mb-2">Recent History</p>
                          <div className="space-y-1 max-h-60 overflow-y-auto">
                            {account.recentEntries.slice(0, 20).map((entry) => {
                              const cfg = ENTRY_TYPE_CONFIG[entry.entryType] ?? {
                                label: entry.entryType, color: 'text-gray-500', icon: null,
                              };
                              return (
                                <div key={entry.id} className="flex items-center gap-2.5 px-2 py-1.5 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors">
                                  <span className={cn('flex-shrink-0', cfg.color)}>{cfg.icon}</span>
                                  <div className="flex-1 min-w-0">
                                    <p className="text-xs font-medium text-gray-700 dark:text-gray-300">{cfg.label}</p>
                                    <p className="text-xs text-gray-400 truncate">{entry.notes ?? fmtDateTime(entry.createdAt)}</p>
                                  </div>
                                  <span className={cn(
                                    'text-sm font-semibold tabular-nums flex-shrink-0',
                                    entry.points > 0 ? 'text-emerald-600 dark:text-emerald-400' : 'text-red-600 dark:text-red-400',
                                  )}>
                                    {entry.points > 0 ? '+' : ''}{entry.points.toLocaleString()}
                                  </span>
                                </div>
                              );
                            })}
                          </div>
                        </div>
                      )}
                    </>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
