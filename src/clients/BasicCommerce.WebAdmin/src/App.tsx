import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useEffect } from 'react';
import AppLayout from './components/layout/AppLayout';
import ProtectedRoute from './components/auth/ProtectedRoute';
import LoginPage from './pages/auth/LoginPage';
import OAuthCallbackPage from './pages/auth/OAuthCallbackPage';
import DashboardPage from './pages/DashboardPage';
import CategoriesPage from './pages/CategoriesPage';
import UsersPage from './pages/UsersPage';
import UserTypesPage from './pages/UserTypesPage';
import PermissionsPage from './pages/PermissionsPage';
import WarehousesPage from './pages/WarehousesPage';
import ManufacturersPage from './pages/ManufacturersPage';
import ProductsPage from './pages/ProductsPage';
import PlaceholderPage from './pages/PlaceholderPage';
import { useThemeStore } from './store/themeStore';

export default function App() {
  const { dark } = useThemeStore();

  useEffect(() => {
    document.documentElement.classList.toggle('dark', dark);
  }, [dark]);

  return (
    <BrowserRouter>
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/auth/callback" element={<OAuthCallbackPage />} />

        {/* Protected routes */}
        <Route element={<ProtectedRoute />}>
          <Route element={<AppLayout />}>
            <Route index element={<DashboardPage />} />

            {/* Products */}
            <Route path="products" element={<ProductsPage />} />
            <Route path="products/categories" element={<CategoriesPage />} />
            <Route path="products/manufacturers" element={<ManufacturersPage />} />
            <Route path="products/tags" element={<PlaceholderPage title="Product Tags" />} />

            {/* Inventory */}
            <Route path="inventory/stock" element={<PlaceholderPage title="Stock Levels" />} />
            <Route path="inventory/movements" element={<PlaceholderPage title="Stock Movements" />} />
            <Route path="inventory/warehouses" element={<WarehousesPage />} />

            {/* Purchasing */}
            <Route path="purchasing/orders" element={<PlaceholderPage title="Purchase Orders" />} />
            <Route path="purchasing/suppliers" element={<PlaceholderPage title="Suppliers" />} />
            <Route path="purchasing/returns" element={<PlaceholderPage title="Supplier Returns" />} />

            {/* Sales */}
            <Route path="sales/transactions" element={<PlaceholderPage title="Transactions" />} />
            <Route path="sales/till-sessions" element={<PlaceholderPage title="Till Sessions" />} />

            {/* Customers */}
            <Route path="customers" element={<PlaceholderPage title="All Customers" />} />
            <Route path="customers/credit" element={<PlaceholderPage title="Credit Accounts" />} />
            <Route path="customers/rewards" element={<PlaceholderPage title="Reward Points" />} />

            {/* Promotions */}
            <Route path="promotions" element={<PlaceholderPage title="Promotions" />} />
            <Route path="promotions/gift-cards" element={<PlaceholderPage title="Gift Cards" />} />

            {/* Reports */}
            <Route path="reports/daily-sales" element={<PlaceholderPage title="Daily Sales Report" />} />
            <Route path="reports/stock" element={<PlaceholderPage title="Stock Report" />} />
            <Route path="reports/categories" element={<PlaceholderPage title="Category Report (PDF)" />} />

            {/* Settings */}
            <Route path="settings/users" element={<UsersPage />} />
            <Route path="settings/user-types" element={<UserTypesPage />} />
            <Route path="settings/menus" element={<PlaceholderPage title="Menu Management" />} />
            <Route path="settings/permissions" element={<PermissionsPage />} />

            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
