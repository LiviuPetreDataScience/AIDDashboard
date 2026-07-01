import { NavLink } from 'react-router-dom';
import type { NavItem } from '../app/navigation';
import './TopTabs.css';

/** Horizontal tab navigation used within a section (Input tabs, Admin tabs). */
export function TopTabs({ items }: { items: NavItem[] }) {
  return (
    <nav className="top-tabs">
      {items.map((item) => (
        <NavLink key={item.path} to={item.path} end>
          {item.label}
        </NavLink>
      ))}
    </nav>
  );
}
