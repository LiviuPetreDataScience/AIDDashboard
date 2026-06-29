import { themeQuartz } from 'ag-grid-community';

/** Reads a CSS custom property from :root so the grid honours the central theme.css palette. */
function readCssVariable(name: string, fallback: string): string {
  if (typeof window === 'undefined') {
    return fallback;
  }
  const value = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  return value || fallback;
}

/**
 * AG Grid theme derived from the application's CSS variables, so changing a colour in
 * theme.css also restyles every data grid.
 */
export const aidGridTheme = themeQuartz.withParams({
  accentColor: readCssVariable('--color-accent', '#e4002b'),
  headerBackgroundColor: readCssVariable('--color-primary', '#111d41'),
  headerTextColor: readCssVariable('--color-primary-contrast', '#ffffff'),
  borderColor: readCssVariable('--color-border', '#d8dae0'),
  foregroundColor: readCssVariable('--color-text', '#1f2430'),
  fontFamily: 'inherit',
  headerFontWeight: 600,
  rowHoverColor: readCssVariable('--color-surface-alt', '#f0f1f4'),
});
