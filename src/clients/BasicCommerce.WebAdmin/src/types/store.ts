export interface Store {
  id: string;
  name: string;
  code: string;
  address: string;
  phone: string | null;
  email: string | null;
  status: string;
  terminalCount: number;
  addressLine1: string;
  addressLine2: string | null;
  city: string;
  district: string;
  postalCode: string;
  country: string;
  openingTime: string;
  closingTime: string;
}

export interface StoreFormData {
  name: string;
  code: string;
  addressLine1: string;
  addressLine2: string;
  city: string;
  district: string;
  postalCode: string;
  phone: string;
  email: string;
  openingTime: string;
  closingTime: string;
}
