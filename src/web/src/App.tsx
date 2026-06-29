import { lazy } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { Layout } from './components/Layout';
import { ProtectedRoute } from './components/ProtectedRoute';
import { LoginPage } from './pages/LoginPage';

// Tab pages are lazily loaded so each becomes its own chunk. The heavy data-grid tabs
// (and AG Grid itself) are only downloaded when the user actually opens them.
const OverallPage = lazy(() => import('./pages/OverallPage').then((m) => ({ default: m.OverallPage })));
const StaffingPage = lazy(() => import('./pages/StaffingPage').then((m) => ({ default: m.StaffingPage })));
const ContractualLanguagesPage = lazy(() =>
  import('./pages/ContractualLanguagesPage').then((m) => ({ default: m.ContractualLanguagesPage })));
const CountriesDevicesPage = lazy(() =>
  import('./pages/CountriesDevicesPage').then((m) => ({ default: m.CountriesDevicesPage })));
const AutomationsPage = lazy(() => import('./pages/AutomationsPage').then((m) => ({ default: m.AutomationsPage })));
const OpportunitiesPage = lazy(() => import('./pages/OpportunitiesPage').then((m) => ({ default: m.OpportunitiesPage })));
const SupportHoursPage = lazy(() => import('./pages/SupportHoursPage').then((m) => ({ default: m.SupportHoursPage })));
const SlaKpisPage = lazy(() => import('./pages/SlaKpisPage').then((m) => ({ default: m.SlaKpisPage })));
const PlaceholderPage = lazy(() => import('./pages/PlaceholderPage').then((m) => ({ default: m.PlaceholderPage })));
const UsersAdminPage = lazy(() => import('./pages/admin/UsersAdminPage').then((m) => ({ default: m.UsersAdminPage })));
const ReferenceAdminPage = lazy(() =>
  import('./pages/admin/ReferenceAdminPage').then((m) => ({ default: m.ReferenceAdminPage })));

/** Application routes. All app pages live inside the authenticated Layout shell. */
export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<Layout />}>
          <Route index element={<Navigate to="/overall" replace />} />
          <Route path="/overall" element={<OverallPage />} />
          <Route path="/staffing/initial" element={<StaffingPage modelType="Initial" title="Initial Staffing Model" />} />
          <Route path="/staffing/latest" element={<StaffingPage modelType="LatestApproved" title="Latest Approved Model" />} />
          <Route path="/contractual-languages" element={<ContractualLanguagesPage />} />
          <Route path="/countries-devices" element={<CountriesDevicesPage />} />
          <Route path="/automations" element={<AutomationsPage />} />
          <Route path="/services" element={<PlaceholderPage title="Services" />} />
          <Route path="/opportunities" element={<OpportunitiesPage />} />
          <Route path="/support-hours" element={<SupportHoursPage />} />
          <Route path="/sla-kpis" element={<SlaKpisPage />} />
          <Route path="/faq" element={<PlaceholderPage title="FAQ" />} />
          <Route path="/aid-dashboard" element={<PlaceholderPage title="AID Dashboard" />} />

          <Route element={<ProtectedRoute adminOnly />}>
            <Route path="/admin/users" element={<UsersAdminPage />} />
            <Route path="/admin/reference" element={<ReferenceAdminPage />} />
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
