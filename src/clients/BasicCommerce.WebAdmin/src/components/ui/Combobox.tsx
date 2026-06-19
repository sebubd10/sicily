import { useState, useRef, useEffect } from 'react';
import { ChevronDown } from 'lucide-react';
import { cn } from '../../lib/utils';

export interface ComboboxOption {
  value: string;
  label: string;
}

interface ComboboxProps {
  value: string;
  options: ComboboxOption[];
  onChange: (value: string) => void;
  placeholder?: string;
  clearable?: boolean;
  clearLabel?: string;
  disabled?: boolean;
  className?: string;
  error?: boolean;
  warning?: boolean;
}

export function Combobox({
  value,
  options,
  onChange,
  placeholder = 'Select…',
  clearable = false,
  clearLabel = 'None',
  disabled = false,
  className,
  error = false,
  warning = false,
}: ComboboxProps) {
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState('');
  const containerRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const selectedOption = options.find((o) => o.value === value) ?? null;

  const filtered = query.trim()
    ? options.filter((o) => o.label.toLowerCase().includes(query.toLowerCase()))
    : options;

  useEffect(() => {
    if (!open) return;
    function handleOutside(e: MouseEvent) {
      if (!containerRef.current?.contains(e.target as Node)) {
        setOpen(false);
        setQuery('');
      }
    }
    document.addEventListener('mousedown', handleOutside);
    return () => document.removeEventListener('mousedown', handleOutside);
  }, [open]);

  function openDropdown() {
    if (disabled) return;
    setOpen(true);
    setQuery('');
    requestAnimationFrame(() => inputRef.current?.focus());
  }

  function selectOption(val: string) {
    onChange(val);
    setOpen(false);
    setQuery('');
  }

  function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    if (e.key === 'Escape') {
      setOpen(false);
      setQuery('');
    } else if (e.key === 'Enter') {
      e.preventDefault();
      if (filtered.length > 0) selectOption(filtered[0].value);
    } else if (e.key === 'Tab') {
      setOpen(false);
      setQuery('');
    }
  }

  const borderCls = error
    ? 'border-red-400 dark:border-red-500'
    : warning
      ? 'border-amber-400 dark:border-amber-500'
      : 'border-gray-200 dark:border-gray-700';

  const baseCls = cn(
    'w-full rounded-lg border px-3 py-2 text-sm bg-white dark:bg-gray-900',
    'focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 transition',
    borderCls,
    disabled && 'opacity-50 cursor-not-allowed',
  );

  return (
    <div ref={containerRef} className={cn('relative', className)}>
      {open ? (
        <input
          ref={inputRef}
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder={selectedOption?.label ?? placeholder}
          className={cn(baseCls, 'text-gray-900 dark:text-white placeholder-gray-400')}
        />
      ) : (
        <button
          type="button"
          onClick={openDropdown}
          disabled={disabled}
          className={cn(
            baseCls,
            'flex items-center justify-between text-left',
            !disabled && 'cursor-pointer hover:border-gray-300 dark:hover:border-gray-600',
          )}
        >
          <span className={cn('truncate', selectedOption ? 'text-gray-900 dark:text-white' : 'text-gray-400')}>
            {selectedOption?.label ?? placeholder}
          </span>
          <ChevronDown className="w-4 h-4 text-gray-400 flex-shrink-0 ml-2" />
        </button>
      )}

      {open && (
        <ul className="absolute z-50 mt-1 w-full max-h-52 overflow-y-auto rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 shadow-lg py-1">
          {clearable && (
            <li>
              <button
                type="button"
                onMouseDown={(e) => { e.preventDefault(); selectOption(''); }}
                className={cn(
                  'w-full text-left px-3 py-2 text-sm transition-colors',
                  !value
                    ? 'bg-primary-50 dark:bg-primary-900/30 text-primary-700 dark:text-primary-400 font-medium'
                    : 'text-gray-500 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-800',
                )}
              >
                {clearLabel}
              </button>
            </li>
          )}
          {filtered.length === 0 ? (
            <li className="px-3 py-2 text-sm text-gray-400 italic">No results.</li>
          ) : (
            filtered.map((o) => (
              <li key={o.value}>
                <button
                  type="button"
                  onMouseDown={(e) => { e.preventDefault(); selectOption(o.value); }}
                  className={cn(
                    'w-full text-left px-3 py-2 text-sm transition-colors',
                    o.value === value
                      ? 'bg-primary-50 dark:bg-primary-900/30 text-primary-700 dark:text-primary-400 font-medium'
                      : 'text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800',
                  )}
                >
                  {o.label}
                </button>
              </li>
            ))
          )}
        </ul>
      )}
    </div>
  );
}
