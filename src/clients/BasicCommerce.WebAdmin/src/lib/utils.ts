import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatCurrency(amount: number, currency = 'BDT') {
  return new Intl.NumberFormat('en-BD', { style: 'currency', currency, minimumFractionDigits: 2 }).format(amount);
}

export function formatDate(date: string | Date) {
  return new Intl.DateTimeFormat('en-BD', { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(date));
}
