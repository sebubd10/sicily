export interface VatRate {
  id: string;
  name: string;
  code: string;
  rate: number;
  isDefault: boolean;
  status: string;
}

export interface VatRateFormData {
  name: string;
  code: string;
  rate: string;
  isDefault: boolean;
}
