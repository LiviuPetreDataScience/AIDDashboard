import { Suspense } from 'react';
import { Outlet } from 'react-router-dom';
import { adminTabs } from '../app/navigation';
import { TopTabs } from './TopTabs';

/** Layout for the Admin section: the Users / Reference Tables top tabs plus the routed content. */
export function AdminLayout() {
  return (
    <>
      <TopTabs items={adminTabs} />
      <Suspense fallback={<div style={{ padding: '0.5rem' }}>Loading…</div>}>
        <Outlet />
      </Suspense>
    </>
  );
}
