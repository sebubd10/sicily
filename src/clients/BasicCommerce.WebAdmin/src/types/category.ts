export type Category = {
  id: string;
  name: string;
  nameBn: string;
  parent: string | null;
  parentName: string | null;
  children: number;
  sort: number;
  status: 'Active' | 'Inactive';
};

export type CategoryFormData = {
  name: string;
  nameBn: string;
  parentId: string;
  sort: number;
};
