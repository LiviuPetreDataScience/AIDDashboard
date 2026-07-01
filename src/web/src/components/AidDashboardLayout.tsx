import { Suspense } from 'react';
import { Outlet } from 'react-router-dom';
import { dashboardTabs } from '../app/navigation';
import { TopTabs } from './TopTabs';

/** Layout for the AID Dashboard section: the analytics top tabs plus the routed view. */
export function AidDashboardLayout() {
  return (
    <>
      <TopTabs items={dashboardTabs} />
      <Suspense fallback={<div style={{ padding: '0.5rem' }}>Loading…</div>}>
        <Outlet />
      </Suspense>
    </>
  );
}
