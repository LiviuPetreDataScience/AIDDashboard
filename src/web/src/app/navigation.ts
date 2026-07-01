/** A navigation entry (sidebar section or top tab). */
export interface NavItem {
  path: string;
  label: string;
  /** When true, only administrators see this entry. */
  adminOnly?: boolean;
}

/** Left sidebar: the top-level sections of the app. */
export const sidebarSections: NavItem[] = [
  { path: '/input', label: 'Input' },
  { path: '/admin', label: 'Admin', adminOnly: true },
  { path: '/faq', label: 'FAQ' },
  { path: '/aid-dashboard', label: 'AID Dashboard' },
];

/** Top tabs within the Input section: the per-account data tabs. */
export const inputTabs: NavItem[] = [
  { path: '/input/overall', label: 'Overall' },
  { path: '/input/staffing/initial', label: 'Initial Staffing Model' },
  { path: '/input/staffing/latest', label: 'Latest Approved Model' },
  { path: '/input/contractual-languages', label: 'Contractual Languages' },
  { path: '/input/countries-devices', label: 'Countries / Users / Devices' },
  { path: '/input/automations', label: 'Automations' },
  { path: '/input/services', label: 'Services' },
  { path: '/input/opportunities', label: 'Opportunities' },
  { path: '/input/support-hours', label: 'Support Hours' },
  { path: '/input/sla-kpis', label: 'SLAs & KPIs' },
];

/** Top tabs within the Admin section. */
export const adminTabs: NavItem[] = [
  { path: '/admin/users', label: 'Users' },
  { path: '/admin/reference', label: 'Reference Tables' },
];

/** Top tabs within the AID Dashboard section (analytics views). */
export const dashboardTabs: NavItem[] = [
  { path: '/aid-dashboard/automations', label: 'Automations' },
];
