import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import Topbar from './Topbar';
import { useSidebarStore } from '../../store/sidebarStore';
import { cn } from '../../lib/utils';

export default function AppLayout() {
  const { collapsed } = useSidebarStore();

  return (
    <div className="flex min-h-screen bg-gray-50 dark:bg-gray-950">
      <Sidebar />
      <div className={cn('flex flex-col flex-1 transition-all duration-300', collapsed ? 'ml-16' : 'ml-60')}>
        <Topbar />
        <main className="flex-1 p-6 overflow-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
