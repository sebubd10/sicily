export type PromotionType =
  | 'PercentageOff'
  | 'FixedAmountOff'
  | 'BuyXGetYFree'
  | 'CategoryPercentageOff'
  | 'CartDiscount';

export type PromotionStatus = 'Draft' | 'Active' | 'Paused' | 'Expired' | 'Cancelled';

export interface Promotion {
  id: string;
  name: string;
  description: string | null;
  type: PromotionType;
  promotionStatus: PromotionStatus;
  productId: string | null;
  categoryId: string | null;
  storeId: string | null;
  discountPercentage: number | null;
  discountAmount: number | null;
  buyQuantity: number | null;
  getQuantity: number | null;
  minimumCartValue: number | null;
  couponCode: string | null;
  requiresCoupon: boolean;
  startsAt: string | null;
  endsAt: string | null;
  maxUses: number | null;
  usedCount: number;
  createdAt: string;
}

export interface PromotionSummary {
  id: string;
  name: string;
  type: string;
  promotionStatus: string;
  discountPercentage: number | null;
  discountAmount: number | null;
  startsAt: string | null;
  endsAt: string | null;
}

export interface PromotionListResponse {
  items: PromotionSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreatePromotionForm {
  name: string;
  description: string;
  type: PromotionType;
  productId: string;
  categoryId: string;
  storeId: string;
  discountPercentage: string;
  discountAmount: string;
  buyQuantity: string;
  getQuantity: string;
  minimumCartValue: string;
  couponCode: string;
  requiresCoupon: boolean;
  startsAt: string;
  endsAt: string;
  maxUses: string;
}
