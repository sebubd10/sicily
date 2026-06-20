import { useQuery } from '@tanstack/react-query';
import { searchCustomers } from '../api/customersApi';

const KEY = ['customers'] as const;

export function useCustomerSearch(term: string, enabled = true) {
  return useQuery({
    queryKey: [...KEY, 'search', term],
    queryFn: () => searchCustomers(term, 1, 10),
    enabled: enabled && term.length >= 2,
    staleTime: 30_000,
  });
}
