import { useState, useMemo } from 'react';
import { Menu as MenuIcon, Search, AlertCircle, Loader2 } from 'lucide-react';
import { cn } from '../lib/utils';
import { useAllMenus } from '../hooks/useUserTypes';

export default function MenusPage() {
  const [search, setSearch]           = useState('');
  const [expandedMenus, setExpanded]  = useState<Set<string>>(new Set());

  const { data: menus = [], isLoading, isError, isFetching } = useAllMenus();

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return menus;

    return menus
      .map((menu) => ({
        ...menu,
        items: menu.items.filter(
          (item) =>
            item.name.toLowerCase().includes(q) ||
            item.route.toLowerCase().includes(q) ||
            (item.permissionCode ?? '').toLowerCase().includes(q) ||
            menu.name.toLowerCase().includes(q),
        ),
      }))
      .filter((menu) => menu.name.toLowerCase().includes(q) || menu.items.length > 0);
  }, [menus, search]);

  const totalSubMenus = useMemo(
    () => menus.reduce((sum, m) => sum + m.items.length, 0),
    [menus],
  );

  function toggleMenu(id: string) {
    setExpanded((prev) => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  }

  function expandAll() {
    setExpanded(new Set(menus.map((m) => m.id)));
  }

  function collapseAll() {
    setExpanded(new Set());
  }

  return (
    <div className="space-y-5">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
            <MenuIcon className="w-6 h-6 text-primary-700" /> Menu Management
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            All navigation menus and sub-menus — assign these to User Types to control sidebar access
          </p>
        </div>
      </div>

      {/* Summary */}
      <div className="flex flex-wrap gap-3">
        <span className="px-3 py-1 rounded-full text-sm font-medium bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300">
          Menus: <strong>{menus.length}</strong>
        </span>
        <span className="px-3 py-1 rounded-full text-sm font-medium bg-primary-100 dark:bg-primary-900/30 text-primary-700 dark:text-primary-300">
          Sub-menus: <strong>{totalSubMenus}</strong>
        </span>
      </div>

      {/* Toolbar */}
      <div className="flex flex-col sm:flex-row gap-3 items-start sm:items-center">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            placeholder="Search menus..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-9 pr-4 py-2 text-sm border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-gray-900 dark:text-white placeholder-gray-400"
          />
        </div>
        <div className="flex items-center gap-3">
          <button onClick={expandAll} className="text-xs text-primary-700 hover:underline">Expand all</button>
          <button onClick={collapseAll} className="text-xs text-gray-500 hover:underline">Collapse all</button>
          {isFetching && !isLoading && <Loader2 className="w-4 h-4 animate-spin text-gray-400" />}
        </div>
      </div>

      {/* Content */}
      {isError ? (
        <div className="flex items-center gap-2 px-6 py-12 text-red-500 bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700">
          <AlertCircle className="w-5 h-5 flex-shrink-0" />
          <span className="text-sm">Failed to load menus. Please refresh and try again.</span>
        </div>
      ) : isLoading ? (
        <div className="space-y-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="animate-pulse bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 p-4">
              <div className="h-4 bg-gray-100 dark:bg-gray-800 rounded w-32 mb-3" />
              <div className="space-y-2">
                {Array.from({ length: 3 }).map((_, j) => (
                  <div key={j} className="h-3 bg-gray-100 dark:bg-gray-800 rounded w-full" />
                ))}
              </div>
            </div>
          ))}
        </div>
      ) : filtered.length === 0 ? (
        <div className="text-center py-12 text-sm text-gray-400">
          No menus match your search.
        </div>
      ) : (
        <div className="space-y-3">
          {filtered
            .slice()
            .sort((a, b) => a.sortOrder - b.sortOrder)
            .map((menu) => {
              const isOpen = expandedMenus.has(menu.id) || !!search.trim();

              return (
                <div
                  key={menu.id}
                  className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm overflow-hidden"
                >
                  {/* Menu header */}
                  <button
                    type="button"
                    onClick={() => toggleMenu(menu.id)}
                    className="w-full flex items-center justify-between px-5 py-3.5 text-left hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
                  >
                    <div className="flex items-center gap-3">
                      <MenuIcon className="w-4 h-4 text-primary-700" />
                      <span className="text-sm font-semibold text-gray-800 dark:text-white">{menu.name}</span>
                      {menu.icon && (
                        <code className="text-xs font-mono bg-gray-100 dark:bg-gray-800 text-gray-500 dark:text-gray-400 px-1.5 py-0.5 rounded">
                          {menu.icon}
                        </code>
                      )}
                      <span className="text-xs px-2 py-0.5 rounded-full font-medium bg-primary-100 dark:bg-primary-900/30 text-primary-700 dark:text-primary-300">
                        {menu.items.length} {menu.items.length === 1 ? 'item' : 'items'}
                      </span>
                    </div>
                    <svg
                      className={cn('w-4 h-4 text-gray-400 transition-transform', isOpen && 'rotate-180')}
                      fill="none" viewBox="0 0 24 24" stroke="currentColor"
                    >
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                    </svg>
                  </button>

                  {/* Sub-menu rows */}
                  {isOpen && (
                    <div className="border-t border-gray-100 dark:border-gray-800">
                      {menu.items.length === 0 ? (
                        <div className="px-5 py-4 text-sm text-gray-400">No sub-menus.</div>
                      ) : (
                        <table className="w-full text-sm">
                          <thead>
                            <tr className="bg-gray-50/70 dark:bg-gray-800/50">
                              <th className="px-5 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-400">Name</th>
                              <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-400">Route</th>
                              <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-400">Permission Code</th>
                              <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-400 w-24">Sort</th>
                            </tr>
                          </thead>
                          <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                            {menu.items
                              .slice()
                              .sort((a, b) => a.sortOrder - b.sortOrder)
                              .map((item) => (
                                <tr key={item.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                                  <td className="px-5 py-2.5 font-medium text-gray-800 dark:text-gray-200">{item.name}</td>
                                  <td className="px-4 py-2.5">
                                    <code className="text-xs font-mono bg-gray-100 dark:bg-gray-800 text-primary-700 dark:text-primary-300 px-1.5 py-0.5 rounded">
                                      {item.route}
                                    </code>
                                  </td>
                                  <td className="px-4 py-2.5 text-gray-500 dark:text-gray-400">
                                    {item.permissionCode || <span className="text-gray-300 dark:text-gray-600">—</span>}
                                  </td>
                                  <td className="px-4 py-2.5 text-gray-500 dark:text-gray-400">{item.sortOrder}</td>
                                </tr>
                              ))}
                          </tbody>
                        </table>
                      )}
                    </div>
                  )}
                </div>
              );
            })}
        </div>
      )}
    </div>
  );
}
