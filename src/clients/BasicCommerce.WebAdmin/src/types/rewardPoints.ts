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

/** Reward points snapshot for a customer at checkout. */
export interface CheckoutRewardPoints {
  programEnabled: boolean;
  /** Spendable now — expired points already excluded. */
  availablePoints: number;
  /** Currency value of availablePoints. */
  availableValue: number;
  /** Earned but not yet activated. */
  pendingPoints: number;
  exchangeRate: number;
  minimumPointsToUse: number;
  /** Most points usable on this order after programme limits. */
  maxRedeemablePoints: number;
  /** Currency value of maxRedeemablePoints, never above the order total. */
  maxRedeemableAmount: number;
  /** Points this order earns on the part not paid with points. */
  pointsToEarn: number;
  pointsActivateImmediately: boolean;
}
