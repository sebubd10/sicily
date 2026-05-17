import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import * as userTypesApi from '../api/userTypesApi';
import type { UserTypeFormData } from '../types/userType';

const KEY = ['user-types'] as const;
const PERMS_KEY = ['api-permissions'] as const;
const MENUS_KEY = ['menus-all'] as const;

export function useUserTypes() {
  return useQuery({
    queryKey: KEY,
    queryFn: userTypesApi.getUserTypes,
    staleTime: 30_000,
  });
}

export function useUserTypeDetail(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => userTypesApi.getUserTypeById(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

export function useApiPermissions() {
  return useQuery({
    queryKey: PERMS_KEY,
    queryFn: () => userTypesApi.getApiPermissions(),
    staleTime: 5 * 60_000,
  });
}

export function useAllMenus() {
  return useQuery({
    queryKey: MENUS_KEY,
    queryFn: userTypesApi.getAllMenus,
    staleTime: 5 * 60_000,
  });
}

export function useCreateUserType() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (form: UserTypeFormData) => userTypesApi.createUserType(form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useUpdateUserType() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, form }: { id: string; form: UserTypeFormData }) =>
      userTypesApi.updateUserType(id, form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeleteUserType() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => userTypesApi.deleteUserType(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useSetUserTypePermissions() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, codes }: { id: string; codes: string[] }) =>
      userTypesApi.setUserTypePermissions(id, codes),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useSetUserTypeMenus() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, subMenuIds }: { id: string; subMenuIds: string[] }) =>
      userTypesApi.setUserTypeMenus(id, subMenuIds),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}
