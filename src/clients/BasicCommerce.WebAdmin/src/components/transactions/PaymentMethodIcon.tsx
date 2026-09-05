import { Banknote, CreditCard, Wallet, Gift, Star } from 'lucide-react';
import bkashLogo from '../../assets/payment-icons/bkash.png';
import nagadLogo from '../../assets/payment-icons/nagad.png';
import rocketLogo from '../../assets/payment-icons/rocket.png';

const BRAND_LOGOS: Record<string, string> = {
  BKash: bkashLogo,
  Nagad: nagadLogo,
  Rocket: rocketLogo,
};

function BrandLogo({ src, alt, size }: { src: string; alt: string; size: number }) {
  return (
    <img
      src={src}
      alt={alt}
      width={size}
      height={size}
      className="rounded-md object-cover flex-shrink-0"
      style={{ width: size, height: size }}
    />
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
      return <BrandLogo src={BRAND_LOGOS.BKash} alt="bKash" size={size} />;
    case 'Nagad':
      return <BrandLogo src={BRAND_LOGOS.Nagad} alt="Nagad" size={size} />;
    case 'Rocket':
      return <BrandLogo src={BRAND_LOGOS.Rocket} alt="Rocket" size={size} />;
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
