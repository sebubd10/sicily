import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';
import * as tagsApi from '../api/tagsApi';
import type { TagFormData, TagListParams } from '../types/tag';

const KEY = ['product-tags'] as const;

export function useTags(params: TagListParams) {
  return useQuery({
    queryKey: [...KEY, params],
    queryFn: () => tagsApi.getTags(params),
    placeholderData: keepPreviousData,
  });
}

export function useTagDetail(id: string | null) {
  return useQuery({
    queryKey: [...KEY, 'detail', id],
    queryFn: () => tagsApi.getTagById(id!),
    enabled: !!id,
    staleTime: 0,
  });
}

export function useCreateTag() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (form: TagFormData) => tagsApi.createTag(form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useUpdateTag() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, form }: { id: string; form: TagFormData }) =>
      tagsApi.updateTag(id, form),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeleteTag() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => tagsApi.deleteTag(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useActivateTag() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => tagsApi.activateTag(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useDeactivateTag() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => tagsApi.deactivateTag(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}

export function useBulkDeleteTags() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (ids: string[]) => tagsApi.bulkDeleteTags(ids),
    onSuccess: () => qc.invalidateQueries({ queryKey: KEY }),
  });
}
