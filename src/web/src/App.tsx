import { lazy } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { AppShell } from './components/AppShell';
import { InputLayout } from './components/InputLayout';
import { AdminLayout } from './components/AdminLayout';
import { AidDashboardLayout } from './components/AidDashboardLayout';
import { ProtectedRoute } from './components/ProtectedRoute';
import { LoginPage } from './pages/LoginPage';

// Pages are lazily loaded so each becomes its own chunk (and AG Grid only loads with the tabs that use it).
const HomePage = lazy(() => import('./pages/HomePage').then((m) => ({ default: m.HomePage })));
const OverallPage = lazy(() => import('./pages/OverallPage').then((m) => ({ default: m.OverallPage })));
const StaffingPage = lazy(() => import('./pages/StaffingPage').then((m) => ({ default: m.StaffingPage })));
const ContractualLanguagesPage = lazy(() =>
  import('./pages/ContractualLanguagesPage').then((m) => ({ default: m.ContractualLanguagesPage })));
const CountriesDevicesPage = lazy(() =>
  import('./pages/CountriesDevicesPage').then((m) => ({ default: m.CountriesDevicesPage })));
const AutomationsPage = lazy(() => import('./pages/AutomationsPage').then((m) => ({ default: m.AutomationsPage })));
const OpportunitiesPage = lazy(() => import('./pages/OpportunitiesPage').then((m) => ({ default: m.OpportunitiesPage })));
const SupportHoursPage = lazy(() => import('./pages/SupportHoursPage').then((m) => ({ default: m.SupportHoursPage })));
const ServicesPage = lazy(() => import('./pages/ServicesPage').then((m) => ({ default: m.ServicesPage })));
const SlaKpisPage = lazy(() => import('./pages/SlaKpisPage').then((m) => ({ default: m.SlaKpisPage })));
const FaqPage = lazy(() => import('./pages/FaqPage').then((m) => ({ default: m.FaqPage })));
const AutomationsDashboardPage = lazy(() =>
  import('./pages/dashboard/AutomationsDashboardPage').then((m) => ({ default: m.AutomationsDashboardPage })));
const UsersAdminPage = lazy(() => import('./pages/admin/UsersAdminPage').then((m) => ({ default: m.UsersAdminPage })));
const ReferenceAdminPage = lazy(() =>
  import('./pages/admin/ReferenceAdminPage').then((m) => ({ default: m.ReferenceAdminPage })));

/**
 * Routes. After login the AppShell (header + left sidebar) hosts the sections:
 * Home (accounts list), Input (per-account tabs), Admin (admin-only), and the FAQ / AID
 * Dashboard placeholders.
 */
export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<AppShell />}>
          <Route index element={<HomePage />} />
          <Route path="faq" element={<FaqPage />} />

          {/* AID Dashboard section: analytics views (currently the Automations dashboard). */}
          <Route path="aid-dashboard" element={<AidDashboardLayout />}>
            <Route index element={<Navigate to="/aid-dashboard/automations" replace />} />
            <Route path="automations" element={<AutomationsDashboardPage />} />
          </Route>

          {/* Input section: per-account data tabs. */}
          <Route path="input" element={<InputLayout />}>
            <Route index element={<Navigate to="/input/overall" replace />} />
            <Route path="overall" element={<OverallPage />} />
            <Route path="staffing/initial" element={<StaffingPage modelType="Initial" title="Initial Staffing Model" />} />
            <Route path="staffing/latest" element={<StaffingPage modelType="LatestApproved" title="Latest Approved Model" />} />
            <Route path="contractual-languages" element={<ContractualLanguagesPage />} />
            <Route path="countries-devices" element={<CountriesDevicesPage />} />
            <Route path="automations" element={<AutomationsPage />} />
            <Route path="services" element={<ServicesPage />} />
            <Route path="opportunities" element={<OpportunitiesPage />} />
            <Route path="support-hours" element={<SupportHoursPage />} />
            <Route path="sla-kpis" element={<SlaKpisPage />} />
          </Route>

          {/* Admin section: restricted to administrators. */}
          <Route element={<ProtectedRoute adminOnly />}>
            <Route path="admin" element={<AdminLayout />}>
              <Route index element={<Navigate to="/admin/users" replace />} />
              <Route path="users" element={<UsersAdminPage />} />
              <Route path="reference" element={<ReferenceAdminPage />} />
            </Route>
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
