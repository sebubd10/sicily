import { Banknote, CreditCard, Wallet, Gift, Star, Rocket } from 'lucide-react';

const BRAND_COLORS: Record<string, string> = {
  BKash: '#E2136E',
  Nagad: '#EE3A24',
  Rocket: '#8C3494',
};

function BrandBadge({ letter, bg, size }: { letter: string; bg: string; size: number }) {
  return (
    <span
      className="inline-flex items-center justify-center rounded-md text-white font-black leading-none flex-shrink-0 select-none"
      style={{ width: size, height: size, backgroundColor: bg, fontSize: Math.max(9, Math.round(size * 0.52)) }}
    >
      {letter}
    </span>
  );
}

export function PaymentMethodIcon({ method, size = 20 }: { method: string; size?: number }) {
  const style = { width: size, height: size };

  switch (method) {
    case 'Cash':
      return <Banknote style={style} />;
    case 'Card':
      return <CreditCard style={style} />;
    case 'BKash':
      return <BrandBadge letter="b" bg={BRAND_COLORS.BKash} size={size} />;
    case 'Nagad':
      return <BrandBadge letter="N" bg={BRAND_COLORS.Nagad} size={size} />;
    case 'Rocket':
      return <Rocket style={style} color={BRAND_COLORS.Rocket} />;
    case 'Credit':
      return <Wallet style={style} />;
    case 'GiftCard':
      return <Gift style={style} />;
    case 'RewardPoints':
      return <Star style={style} />;
    default:
      return <CreditCard style={style} />;
  }
}
