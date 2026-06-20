export interface RewardPointsSettings {
  id: string;
  isEnabled: boolean;
  exchangeRate: number;
  minimumPointsToUse: number;
  maximumPointsPerOrder: number;
  maximumRedeemedRate: number;
  purchaseSpendPerPoint: number;
  pointsEarnedPerSpend: number;
  purchasePointsValidityDays: number;
  minimumOrderTotalForPoints: number;
  pointsForRegistration: number;
  registrationPointsValidityDays: number;
  activatePointsImmediately: boolean;
  displayHowMuchWillBeEarned: boolean;
  pointsAccumulatedForAllStores: boolean;
}

export interface RewardPointsEntry {
  id: string;
  points: number;
  entryType: string;
  isActivated: boolean;
  expiresAt: string | null;
  transactionId: string | null;
  notes: string | null;
  createdAt: string;
}

export interface RewardPointsAccount {
  id: string;
  customerId: string;
  storeId: string | null;
  totalEarnedPoints: number;
  usedPoints: number;
  expiredPoints: number;
  pendingPoints: number;
  availablePoints: number;
  recentEntries: RewardPointsEntry[];
}
