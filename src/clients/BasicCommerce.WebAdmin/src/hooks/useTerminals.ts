import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import * as terminalsApi from '../api/terminalsApi';
import type { TerminalFormData } from '../types/terminal';

const KEY = ['terminals'] as const;

export function useTerminals() {
  return useQuery({
    queryKey: KEY,
    queryFn: terminalsApi.getTerminals,
    staleTime: 30_000,
  });
}

export function useTerminalDetail(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => terminalsApi.getTerminalById(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

export function useCreateTerminal() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (form: TerminalFormData) => terminalsApi.createTerminal(form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useUpdateTerminal() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, form }: { id: string; form: TerminalFormData }) =>
      terminalsApi.updateTerminal(id, form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useActivateTerminal() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => terminalsApi.activateTerminal(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeactivateTerminal() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => terminalsApi.deactivateTerminal(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeleteTerminal() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => terminalsApi.deleteTerminal(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}
