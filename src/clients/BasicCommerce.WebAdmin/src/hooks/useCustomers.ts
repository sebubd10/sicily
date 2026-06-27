import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';
import * as customersApi from '../api/customersApi';
import type { RegisterCustomerPayload, UpdateCustomerPayload } from '../types/customer';

const KEY = ['customers'] as const;
const CREDIT_KEY = ['credit-accounts'] as const;

export function useCustomers(params: { term?: string; page?: number; pageSize?: number }) {
  return useQuery({
    queryKey: [...KEY, params],
    queryFn: () =>
      customersApi.searchCustomers(params.term ?? '', params.page ?? 1, params.pageSize ?? 20),
    placeholderData: keepPreviousData,
    staleTime: 30_000,
  });
}

export function useCustomerSearch(term: string, enabled = true) {
  return useQuery({
    queryKey: [...KEY, 'search', term],
    queryFn: () => customersApi.searchCustomers(term, 1, 10),
    enabled: enabled && term.length >= 2,
    staleTime: 30_000,
    select: (data) => data.items,
  });
}

export function useCustomerDetail(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => customersApi.getCustomerById(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

export function useRegisterCustomer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: RegisterCustomerPayload) => customersApi.registerCustomer(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useUpdateCustomer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateCustomerPayload }) =>
      customersApi.updateCustomer(id, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeactivateCustomer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => customersApi.deactivateCustomer(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useActivateCustomer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => customersApi.activateCustomer(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeleteCustomer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => customersApi.deleteCustomer(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useUpdateCreditLimit() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, creditLimit }: { id: string; creditLimit: number }) =>
      customersApi.updateCreditLimit(id, creditLimit),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEY });
      qc.invalidateQueries({ queryKey: CREDIT_KEY });
    },
  });
}

export function useCreateCreditAccount() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({
      customerId,
      storeId,
      creditLimit,
    }: {
      customerId: string;
      storeId: string;
      creditLimit: number;
    }) => customersApi.createCreditAccount(customerId, storeId, creditLimit),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: CREDIT_KEY });
      qc.invalidateQueries({ queryKey: KEY });
    },
  });
}

export function useAllCreditAccounts(params: {
  term?: string;
  hasBalance?: boolean;
  page?: number;
  pageSize?: number;
}) {
  return useQuery({
    queryKey: [...CREDIT_KEY, params],
    queryFn: () => customersApi.getAllCreditAccounts(params),
    placeholderData: keepPreviousData,
    staleTime: 30_000,
  });
}

export function useCreditAccountDetail(creditAccountId: string | null) {
  return useQuery({
    queryKey: [...CREDIT_KEY, 'detail', creditAccountId],
    queryFn: () => customersApi.getCreditAccount(creditAccountId!),
    enabled: !!creditAccountId,
    staleTime: 0,
  });
}

export function useCustomerCreditAccounts(customerId: string | null) {
  return useQuery({
    queryKey: [...KEY, 'credit-accounts', customerId],
    queryFn: () => customersApi.getCustomerCreditAccounts(customerId!),
    enabled: !!customerId,
    staleTime: 0,
  });
}

export function useRecordCreditPayment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({
      creditAccountId,
      amount,
      reference,
    }: {
      creditAccountId: string;
      amount: number;
      reference?: string | null;
    }) => customersApi.recordCreditPayment(creditAccountId, amount, reference),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: KEY });
      qc.invalidateQueries({ queryKey: CREDIT_KEY });
    },
  });
}

export function useAddLoyaltyPoints() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, points, reason }: { id: string; points: number; reason: string }) =>
      customersApi.addLoyaltyPoints(id, points, reason),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useRedeemLoyaltyPoints() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, points, reason }: { id: string; points: number; reason: string }) =>
      customersApi.redeemLoyaltyPoints(id, points, reason),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}
