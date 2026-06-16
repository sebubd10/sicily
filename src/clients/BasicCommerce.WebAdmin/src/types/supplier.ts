export interface Supplier {
  id: string;
  name: string;
  code: string;
  contactName: string | null;
  email: string | null;
  phone: string | null;
  addressLine1: string | null;
  addressLine2: string | null;
  city: string | null;
  district: string | null;
  postalCode: string | null;
  leadTimeDays: number;
  notes: string | null;
  manufacturerId: string | null;
  manufacturerName: string | null;
  status: string;
}

export interface SupplierFormData {
  name: string;
  code: string;
  contactName: string;
  email: string;
  phone: string;
  addressLine1: string;
  addressLine2: string;
  city: string;
  district: string;
  postalCode: string;
  leadTimeDays: number;
  notes: string;
}
