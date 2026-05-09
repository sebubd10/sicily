import {
  LayoutDashboard, Package,
  Warehouse,
  ShoppingCart,
  Receipt,
  Users,
  Ticket,
  BarChart2,
  Settings,
  type LucideIcon,
} from 'lucide-react';

export interface SubNavItem {
  label: string;
  path: string;
  permission?: string;
}

export interface NavItem {
  label: string;
  icon: LucideIcon;
  path?: string;
  children?: SubNavItem[];
}

export const navItems: NavItem[] = [
  {
    label: 'Dashboard',
    icon: LayoutDashboard,
    path: '/',
  },
  {
    label: 'Products',
    icon: Package,
    children: [
      { label: 'All Products',    path: '/products' },
      { label: 'Categories',      path: '/products/categories' },
      { label: 'Manufacturers',   path: '/products/manufacturers' },
      { label: 'Tags',            path: '/products/tags' },
    ],
  },
  {
    label: 'Inventory',
    icon: Warehouse,
    children: [
      { label: 'Stock Levels',    path: '/inventory/stock' },
      { label: 'Movements',       path: '/inventory/movements' },
      { label: 'Warehouses',      path: '/inventory/warehouses' },
    ],
  },
  {
    label: 'Purchasing',
    icon: ShoppingCart,
    children: [
      { label: 'Purchase Orders', path: '/purchasing/orders' },
      { label: 'Suppliers',       path: '/purchasing/suppliers' },
      { label: 'Supplier Returns',path: '/purchasing/returns' },
    ],
  },
  {
    label: 'Sales',
    icon: Receipt,
    children: [
      { label: 'Transactions',    path: '/sales/transactions' },
      { label: 'Till Sessions',   path: '/sales/till-sessions' },
    ],
  },
  {
    label: 'Customers',
    icon: Users,
    children: [
      { label: 'All Customers',   path: '/customers' },
      { label: 'Credit Accounts', path: '/customers/credit' },
      { label: 'Reward Points',   path: '/customers/rewards' },
    ],
  },
  {
    label: 'Promotions',
    icon: Ticket,
    children: [
      { label: 'Promotions',      path: '/promotions' },
      { label: 'Gift Cards',      path: '/promotions/gift-cards' },
    ],
  },
  {
    label: 'Reports',
    icon: BarChart2,
    children: [
      { label: 'Daily Sales',     path: '/reports/daily-sales' },
      { label: 'Stock Report',    path: '/reports/stock' },
      { label: 'Category Report', path: '/reports/categories' },
    ],
  },
  {
    label: 'Settings',
    icon: Settings,
    children: [
      { label: 'Users',           path: '/settings/users' },
      { label: 'User Types',      path: '/settings/user-types' },
      { label: 'Menus',           path: '/settings/menus' },
      { label: 'Permissions',     path: '/settings/permissions' },
    ],
  },
];
