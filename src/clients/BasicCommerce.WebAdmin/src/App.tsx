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
import MenusPage from './pages/MenusPage';
import PermissionsPage from './pages/PermissionsPage';
import StoresPage from './pages/StoresPage';
import TerminalsPage from './pages/TerminalsPage';
import VatRatesPage from './pages/VatRatesPage';
import WarehousesPage from './pages/WarehousesPage';
import StockLevelsPage from './pages/StockLevelsPage';
import StockBatchesPage from './pages/StockBatchesPage';
import StockMovementsPage from './pages/StockMovementsPage';
import TillSessionsPage from './pages/TillSessionsPage';
import RewardPointsPage from './pages/RewardPointsPage';
import PromotionsPage from './pages/PromotionsPage';
import GiftCardsPage from './pages/GiftCardsPage';
import CustomersPage from './pages/CustomersPage';
import CreditAccountsPage from './pages/CreditAccountsPage';
import ManufacturersPage from './pages/ManufacturersPage';
import ProductsPage from './pages/ProductsPage';
import ProductTagsPage from './pages/ProductTagsPage';
import SuppliersPage from './pages/SuppliersPage';
import PurchaseOrdersPage from './pages/PurchaseOrdersPage';
import SupplierReturnsPage from './pages/SupplierReturnsPage';
import TransactionsListPage from './pages/TransactionsListPage';
import TransactionDetailPage from './pages/TransactionDetailPage';
import TransactionReturnPage from './pages/TransactionReturnPage';
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
            <Route path="products/tags" element={<ProductTagsPage />} />

            {/* Inventory */}
            <Route path="inventory/stock" element={<StockLevelsPage />} />
            <Route path="inventory/batches" element={<StockBatchesPage />} />
            <Route path="inventory/movements" element={<StockMovementsPage />} />
            <Route path="inventory/warehouses" element={<WarehousesPage />} />

            {/* Purchasing */}
            <Route path="purchasing/orders" element={<PurchaseOrdersPage />} />
            <Route path="purchasing/suppliers" element={<SuppliersPage />} />
            <Route path="purchasing/returns" element={<SupplierReturnsPage />} />

            {/* Sales */}
            <Route path="sales/transactions" element={<TransactionsListPage />} />
            <Route path="sales/transactions/:id" element={<TransactionDetailPage />} />
            <Route path="sales/transactions/:id/return" element={<TransactionReturnPage />} />
            <Route path="sales/till-sessions" element={<TillSessionsPage />} />

            {/* Customers */}
            <Route path="customers" element={<CustomersPage />} />
            <Route path="customers/credit" element={<CreditAccountsPage />} />
            <Route path="customers/rewards" element={<RewardPointsPage />} />

            {/* Promotions */}
            <Route path="promotions" element={<PromotionsPage />} />
            <Route path="promotions/gift-cards" element={<GiftCardsPage />} />

            {/* Reports */}
            <Route path="reports/daily-sales" element={<PlaceholderPage title="Daily Sales Report" />} />
            <Route path="reports/stock" element={<PlaceholderPage title="Stock Report" />} />
            <Route path="reports/categories" element={<PlaceholderPage title="Category Report (PDF)" />} />

            {/* Settings */}
            <Route path="settings/users" element={<UsersPage />} />
            <Route path="settings/user-types" element={<UserTypesPage />} />
            <Route path="settings/menus" element={<MenusPage />} />
            <Route path="settings/permissions" element={<PermissionsPage />} />
            <Route path="settings/stores" element={<StoresPage />} />
            <Route path="settings/terminals" element={<TerminalsPage />} />
            <Route path="settings/vat-rates" element={<VatRatesPage />} />

            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
