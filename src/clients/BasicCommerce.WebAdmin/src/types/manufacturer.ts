export interface Manufacturer {
  id: string;
  name: string;
  code: string | null;
  country: string | null;
  website: string | null;
  contactEmail: string | null;
  notes: string | null;
  status: string;
}

export interface ManufacturerFormData {
  name: string;
  code: string;
  country: string;
  website: string;
  contactEmail: string;
  notes: string;
}
