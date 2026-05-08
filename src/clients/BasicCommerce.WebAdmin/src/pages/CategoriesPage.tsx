import { useState } from 'react';
import { Search, Plus, Pencil, PowerOff, ChevronDown, Layers } from 'lucide-react';
import { cn } from '../lib/utils';

// ── Mock data ──────────────────────────────────────────────────────────────────
const mockCategories = [
  { id: '1', name: 'Dairy & Eggs',          nameBn: 'দুগ্ধজাত পণ্য ও ডিম',   parent: null,           parentName: null,               children: 3, sort: 1, status: 'Active' },
  { id: '2', name: 'Fresh Milk',            nameBn: 'তাজা দুধ',               parent: '1',            parentName: 'Dairy & Eggs',     children: 0, sort: 1, status: 'Active' },
  { id: '3', name: 'Cheese & Butter',       nameBn: 'পনির ও মাখন',            parent: '1',            parentName: 'Dairy & Eggs',     children: 0, sort: 2, status: 'Active' },
  { id: '4', name: 'Eggs',                  nameBn: 'ডিম',                    parent: '1',            parentName: 'Dairy & Eggs',     children: 0, sort: 3, status: 'Active' },
  { id: '5', name: 'Grains & Rice',         nameBn: 'শস্য ও চাল',             parent: null,           parentName: null,               children: 2, sort: 2, status: 'Active' },
  { id: '6', name: 'Basmati Rice',          nameBn: 'বাসমতি চাল',             parent: '5',            parentName: 'Grains & Rice',    children: 0, sort: 1, status: 'Active' },
  { id: '7', name: 'Flour & Semolina',      nameBn: 'ময়দা ও সুজি',            parent: '5',            parentName: 'Grains & Rice',    children: 0, sort: 2, status: 'Active' },
  { id: '8', name: 'Cooking Oil',           nameBn: 'রান্নার তেল',            parent: null,           parentName: null,               children: 0, sort: 3, status: 'Active' },
  { id: '9', name: 'Bakery',               nameBn: 'বেকারি পণ্য',             parent: null,           parentName: null,               children: 1, sort: 4, status: 'Active' },
  { id: '10', name: 'Bread & Buns',         nameBn: 'রুটি ও বান',             parent: '9',            parentName: 'Bakery',           children: 0, sort: 1, status: 'Active' },
  { id: '11', name: 'Beverages',            nameBn: 'পানীয়',                 parent: null,           parentName: null,               children: 2, sort: 5, status: 'Active' },
  { id: '12', name: 'Soft Drinks',          nameBn: 'কোমল পানীয়',            parent: '11',           parentName: 'Beverages',        children: 0, sort: 1, status: 'Inactive' },
  { id: '13', name: 'Snacks',              nameBn: 'স্ন্যাকস',               parent: null,           parentName: null,               children: 0, sort: 6, status: 'Active' },
];

type Category = typeof mockCategories[number];

export default function CategoriesPage() {
  const [search, setSearch] = useState('');
  const [showInactive, setShowInactive] = useState(false);

  const filtered = mockCategories.filter((c) => {
    if (!showInactive && c.status === 'Inactive') return false;
    if (!search) return true;
    return (
      c.name.toLowerCase().includes(search.toLowerCase()) ||
      c.nameBn.includes(search)
    );
  });

  const activeCount   = mockCategories.filter((c) => c.status === 'Active').length;
  const inactiveCount = mockCategories.filter((c) => c.status === 'Inactive').length;
  const rootCount     = mockCategories.filter((c) => !c.parent).length;
  const subCount      = mockCategories.filter((c) => !!c.parent).length;

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <Layers className="w-6 h-6 text-primary-700" /> Categories
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Manage product category hierarchy
          </p>
        </div>
        <button className="flex items-center gap-2 px-4 py-2 bg-primary-800 hover:bg-primary-900 text-white text-sm font-medium rounded-lg transition-colors shadow-sm">
          <Plus className="w-4 h-4" /> Add Category
        </button>
      </div>

      {/* Summary chips */}
      <div className="flex flex-wrap gap-3">
        {[
          { label: 'Total',          value: mockCategories.length, color: 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300' },
          { label: 'Active',         value: activeCount,           color: 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300' },
          { label: 'Inactive',       value: inactiveCount,         color: 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400' },
          { label: 'Root',           value: rootCount,             color: 'bg-primary-100 dark:bg-primary-900/30 text-primary-700 dark:text-primary-300' },
          { label: 'Sub-categories', value: subCount,              color: 'bg-violet-100 dark:bg-violet-900/30 text-violet-700 dark:text-violet-300' },
        ].map((s) => (
          <span key={s.label} className={cn('px-3 py-1 rounded-full text-sm font-medium', s.color)}>
            {s.label}: <strong>{s.value}</strong>
          </span>
        ))}
      </div>

      {/* Toolbar */}
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            placeholder="Search categories..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-9 pr-4 py-2 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-gray-900 dark:text-white placeholder-gray-400"
          />
        </div>
        <label className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 cursor-pointer select-none">
          <div
            onClick={() => setShowInactive((v) => !v)}
            className={cn(
              'w-9 h-5 rounded-full transition-colors relative',
              showInactive ? 'bg-primary-700' : 'bg-gray-300 dark:bg-gray-600'
            )}
          >
            <span className={cn('absolute top-0.5 w-4 h-4 rounded-full bg-white shadow transition-all', showInactive ? 'left-4' : 'left-0.5')} />
          </div>
          Show inactive
        </label>
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
                {['#', 'Name (English)', 'Name (Bengali)', 'Parent Category', 'Sub-categories', 'Sort', 'Status', 'Actions'].map((h) => (
                  <th
                    key={h}
                    className="px-4 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider whitespace-nowrap"
                  >
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
              {filtered.length === 0 ? (
                <tr>
                  <td colSpan={8} className="px-4 py-12 text-center text-sm text-gray-400">
                    No categories found.
                  </td>
                </tr>
              ) : (
                filtered.map((cat, idx) => (
                  <tr key={cat.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                    <td className="px-4 py-3 text-gray-400">{idx + 1}</td>
                    <td className="px-4 py-3 font-medium text-gray-900 dark:text-white whitespace-nowrap">
                      {cat.parent && <span className="text-gray-300 dark:text-gray-600 mr-1">└</span>}
                      {cat.name}
                    </td>
                    <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{cat.nameBn}</td>
                    <td className="px-4 py-3 text-gray-500 dark:text-gray-400 whitespace-nowrap">
                      {cat.parentName
                        ? <span className="px-2 py-0.5 bg-gray-100 dark:bg-gray-800 rounded text-xs">{cat.parentName}</span>
                        : <span className="text-gray-300 dark:text-gray-600">—</span>
                      }
                    </td>
                    <td className="px-4 py-3 text-center text-gray-600 dark:text-gray-400">{cat.children}</td>
                    <td className="px-4 py-3 text-center text-gray-600 dark:text-gray-400">{cat.sort}</td>
                    <td className="px-4 py-3">
                      <span className={cn(
                        'inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium',
                        cat.status === 'Active'
                          ? 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-300'
                          : 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400'
                      )}>
                        <span className={cn('w-1.5 h-1.5 rounded-full mr-1.5', cat.status === 'Active' ? 'bg-emerald-500' : 'bg-red-400')} />
                        {cat.status}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-1">
                        <button
                          title="Edit"
                          className="p-1.5 rounded-lg text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors"
                        >
                          <Pencil className="w-3.5 h-3.5" />
                        </button>
                        <button
                          title={cat.status === 'Active' ? 'Deactivate' : 'Activate'}
                          className="p-1.5 rounded-lg text-gray-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
                        >
                          <PowerOff className="w-3.5 h-3.5" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Footer */}
        <div className="px-4 py-3 bg-gray-50 dark:bg-gray-800/50 border-t border-gray-200 dark:border-gray-700 flex items-center justify-between text-xs text-gray-500 dark:text-gray-400">
          <span>Showing {filtered.length} of {mockCategories.length} categories</span>
          <span>Page 1 of 1</span>
        </div>
      </div>
    </div>
  );
}
