/** The main tab definitions, used both for the top navigation and the routes. */
export interface NavTab {
  path: string;
  label: string;
}

export const mainTabs: NavTab[] = [
  { path: '/overall', label: 'Overall' },
  { path: '/staffing/initial', label: 'Initial Staffing Model' },
  { path: '/staffing/latest', label: 'Latest Approved Model' },
  { path: '/contractual-languages', label: 'Contractual Languages' },
  { path: '/countries-devices', label: 'Countries / Users / Devices Supported' },
  { path: '/automations', label: 'Automations' },
  { path: '/services', label: 'Services' },
  { path: '/opportunities', label: 'Opportunities' },
  { path: '/support-hours', label: 'Support Hours' },
  { path: '/sla-kpis', label: 'SLAs & KPIs' },
  { path: '/faq', label: 'FAQ' },
  { path: '/aid-dashboard', label: 'AID Dashboard' },
];

export const adminTabs: NavTab[] = [
  { path: '/admin/users', label: 'Users' },
  { path: '/admin/reference', label: 'Reference Tables' },
];
