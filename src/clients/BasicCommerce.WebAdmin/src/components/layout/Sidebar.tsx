import { useState } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { ChevronDown, ChevronRight, Store } from 'lucide-react';
import { navItems } from '../../lib/navItems';
import { useSidebarStore } from '../../store/sidebarStore';
import { cn } from '../../lib/utils';

export default function Sidebar() {
  const { collapsed } = useSidebarStore();
  const location = useLocation();
  const [openGroups, setOpenGroups] = useState<string[]>(['Products']);

  const toggleGroup = (label: string) => {
    setOpenGroups((prev) =>
      prev.includes(label) ? prev.filter((g) => g !== label) : [...prev, label]
    );
  };

  const isGroupActive = (children?: { path: string }[]) =>
    children?.some((c) => location.pathname.startsWith(c.path)) ?? false;

  return (
    <aside
      className={cn(
        'fixed inset-y-0 left-0 z-40 flex flex-col bg-primary-900 dark:bg-gray-900 text-white transition-all duration-300',
        collapsed ? 'w-16' : 'w-60'
      )}
    >
      {/* Logo */}
      <div className={cn('flex items-center gap-3 px-4 py-4 border-b border-primary-800 dark:border-gray-700 min-h-[64px]', collapsed && 'justify-center px-2')}>
        <div className="flex-shrink-0 w-8 h-8 bg-white rounded-lg flex items-center justify-center">
          <Store className="w-5 h-5 text-primary-800" />
        </div>
        {!collapsed && (
          <div className="overflow-hidden">
            <p className="text-sm font-bold leading-tight truncate">BasicCommerce</p>
            <p className="text-xs text-primary-300 truncate">Agora Supermarkets</p>
          </div>
        )}
      </div>

      {/* Nav */}
      <nav className="flex-1 overflow-y-auto scrollbar-thin py-3 space-y-0.5 px-2">
        {navItems.map((item) => {
          if (!item.children) {
            return (
              <NavLink
                key={item.label}
                to={item.path!}
                end
                className={({ isActive }) =>
                  cn(
                    'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                    isActive
                      ? 'bg-primary-700 text-white'
                      : 'text-primary-200 hover:bg-primary-800 hover:text-white',
                    collapsed && 'justify-center px-2'
                  )
                }
                title={collapsed ? item.label : undefined}
              >
                <item.icon className="w-4 h-4 flex-shrink-0" />
                {!collapsed && <span>{item.label}</span>}
              </NavLink>
            );
          }

          const open = openGroups.includes(item.label) || isGroupActive(item.children);

          return (
            <div key={item.label}>
              <button
                onClick={() => !collapsed && toggleGroup(item.label)}
                title={collapsed ? item.label : undefined}
                className={cn(
                  'w-full flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                  isGroupActive(item.children)
                    ? 'bg-primary-800 text-white'
                    : 'text-primary-200 hover:bg-primary-800 hover:text-white',
                  collapsed && 'justify-center px-2'
                )}
              >
                <item.icon className="w-4 h-4 flex-shrink-0" />
                {!collapsed && (
                  <>
                    <span className="flex-1 text-left">{item.label}</span>
                    {open ? <ChevronDown className="w-3.5 h-3.5" /> : <ChevronRight className="w-3.5 h-3.5" />}
                  </>
                )}
              </button>

              {!collapsed && open && (
                <div className="ml-4 mt-0.5 space-y-0.5 border-l border-primary-700 pl-3">
                  {item.children.map((child) => (
                    <NavLink
                      key={child.path}
                      to={child.path}
                      className={({ isActive }) =>
                        cn(
                          'block rounded-md px-3 py-1.5 text-xs font-medium transition-colors',
                          isActive
                            ? 'bg-primary-700 text-white'
                            : 'text-primary-300 hover:bg-primary-800 hover:text-white'
                        )
                      }
                    >
                      {child.label}
                    </NavLink>
                  ))}
                </div>
              )}
            </div>
          );
        })}
      </nav>

      {/* Version */}
      {!collapsed && (
        <div className="px-4 py-3 border-t border-primary-800 dark:border-gray-700">
          <p className="text-xs text-primary-400">v1.0.0 · BasicCommerce POS</p>
        </div>
      )}
    </aside>
  );
}
