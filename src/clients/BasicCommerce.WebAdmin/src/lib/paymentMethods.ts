export interface PaymentMethodOption {
  value: string;
  label: string;
  hint?: string;
}

export const PAYMENT_METHODS: PaymentMethodOption[] = [
  { value: 'Cash',         label: 'Cash' },
  { value: 'Card',         label: 'Card' },
  { value: 'BKash',        label: 'bKash' },
  { value: 'Nagad',        label: 'Nagad' },
  { value: 'Rocket',       label: 'Rocket' },
  { value: 'Credit',       label: 'Credit',  hint: 'Customer account' },
  { value: 'GiftCard',     label: 'Gift Card' },
  { value: 'RewardPoints', label: 'Points',  hint: 'Loyalty points' },
];

export const MOBILE_WALLET_METHODS = ['BKash', 'Nagad', 'Rocket'];

export function paymentMethodLabel(value: string): string {
  return PAYMENT_METHODS.find((m) => m.value === value)?.label ?? value;
}
