import { PageHeader } from '../components/PageHeader';

/** Placeholder for tabs that are intentionally not built yet (Services, FAQ, AID Dashboard). */
export function PlaceholderPage({ title }: { title: string }) {
  return (
    <>
      <PageHeader title={title} />
      <div className="no-account-banner">This section is not available yet.</div>
    </>
  );
}
