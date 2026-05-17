import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';
import * as usersApi from '../api/usersApi';
import type { UserFormData, UserListParams } from '../types/user';

const KEY = ['users'] as const;

export function useUsers(params: UserListParams) {
  return useQuery({
    queryKey: [...KEY, params],
    queryFn: () => usersApi.getUsers(params),
    placeholderData: keepPreviousData,
  });
}

export function useCreateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (form: UserFormData) => usersApi.createUser(form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useActivateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => usersApi.activateUser(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeactivateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => usersApi.deactivateUser(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useUnlockUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => usersApi.unlockUser(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useAssignUserType() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, userTypeId }: { id: string; userTypeId: string | null }) =>
      usersApi.assignUserType(id, userTypeId),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}
