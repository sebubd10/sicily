import { Menu, Moon, Sun, Bell, ChevronDown, LogOut, User } from 'lucide-react';
import { useState } from 'react';
import { useSidebarStore } from '../../store/sidebarStore';
import { useThemeStore } from '../../store/themeStore';
import { cn } from '../../lib/utils';

export default function Topbar() {
  const { toggle: toggleSidebar } = useSidebarStore();
  const { dark, toggle: toggleTheme } = useThemeStore();
  const [userMenuOpen, setUserMenuOpen] = useState(false);

  return (
    <header className="h-16 bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-700 flex items-center px-4 gap-3 z-30 sticky top-0">
      {/* Hamburger */}
      <button
        onClick={toggleSidebar}
        className="p-2 rounded-lg text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
      >
        <Menu className="w-5 h-5" />
      </button>

      {/* Breadcrumb placeholder */}
      <div className="flex-1" />

      {/* Actions */}
      <div className="flex items-center gap-2">
        {/* Notifications */}
        <button className="relative p-2 rounded-lg text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors">
          <Bell className="w-5 h-5" />
          <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-red-500 rounded-full" />
        </button>

        {/* Dark mode */}
        <button
          onClick={toggleTheme}
          className="p-2 rounded-lg text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
          title={dark ? 'Light mode' : 'Dark mode'}
        >
          {dark ? <Sun className="w-5 h-5" /> : <Moon className="w-5 h-5" />}
        </button>

        {/* Divider */}
        <div className="w-px h-6 bg-gray-200 dark:bg-gray-700" />

        {/* User menu */}
        <div className="relative">
          <button
            onClick={() => setUserMenuOpen((v) => !v)}
            className="flex items-center gap-2 px-3 py-1.5 rounded-lg text-sm hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
          >
            <div className="w-7 h-7 rounded-full bg-primary-800 text-white flex items-center justify-center text-xs font-bold">
              A
            </div>
            <div className="text-left hidden sm:block">
              <p className="text-sm font-medium text-gray-800 dark:text-gray-200 leading-tight">Admin</p>
              <p className="text-xs text-gray-500 leading-tight">Chain Admin</p>
            </div>
            <ChevronDown className="w-4 h-4 text-gray-400" />
          </button>

          {userMenuOpen && (
            <>
              <div className="fixed inset-0 z-10" onClick={() => setUserMenuOpen(false)} />
              <div className="absolute right-0 top-full mt-1 w-48 bg-white dark:bg-gray-800 rounded-xl shadow-lg border border-gray-200 dark:border-gray-700 z-20 py-1">
                <button className="w-full flex items-center gap-2 px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700">
                  <User className="w-4 h-4" /> Profile
                </button>
                <div className="border-t border-gray-100 dark:border-gray-700 my-1" />
                <button className="w-full flex items-center gap-2 px-4 py-2 text-sm text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20">
                  <LogOut className="w-4 h-4" /> Sign out
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </header>
  );
}
