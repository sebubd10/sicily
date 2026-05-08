import { Construction } from 'lucide-react';

interface Props { title: string; }

export default function PlaceholderPage({ title }: Props) {
  return (
    <div className="flex flex-col items-center justify-center h-64 gap-3 text-gray-400 dark:text-gray-600">
      <Construction className="w-10 h-10" />
      <p className="text-lg font-medium">{title}</p>
      <p className="text-sm">Coming soon</p>
    </div>
  );
}
