import { Suspense } from 'react';
import { Outlet } from 'react-router-dom';
import { inputTabs } from '../app/navigation';
import { TopTabs } from './TopTabs';

/** Layout for the Input section: the per-account top tabs plus the routed tab content. */
export function InputLayout() {
  return (
    <>
      <TopTabs items={inputTabs} />
      <Suspense fallback={<div style={{ padding: '0.5rem' }}>Loading…</div>}>
        <Outlet />
      </Suspense>
    </>
  );
}
