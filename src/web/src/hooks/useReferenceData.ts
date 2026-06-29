import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { referenceApi } from '../api/endpoints';
import type { ReferenceItem, ReferenceType } from '../api/types';

/**
 * Loads all (active) reference items once and caches them, exposing helpers to list the
 * items of a given type and to resolve a reference id to its display name.
 */
export function useReferenceData() {
  const { data, isLoading } = useQuery({
    queryKey: ['reference'],
    queryFn: () => referenceApi.getAll(false),
    staleTime: 5 * 60 * 1000,
  });

  const items: ReferenceItem[] = data ?? [];

  const nameById = useMemo(() => {
    const lookup = new Map<number, string>();
    for (const item of items) {
      lookup.set(item.id, item.name);
    }
    return lookup;
  }, [data]); // eslint-disable-line react-hooks/exhaustive-deps

  const getItems = (type: ReferenceType): ReferenceItem[] =>
    items.filter((item) => item.type === type);

  const getName = (id?: number | null): string | undefined =>
    id == null ? undefined : nameById.get(id);

  return { isLoading, items, getItems, getName };
}
