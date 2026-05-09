import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight } from 'lucide-react';
import { cn } from '../../lib/utils';

type Props = {
  page: number;
  totalPages: number;
  totalItems: number;
  pageSize: number;
  pageSizeOptions?: number[];
  onPageChange: (page: number) => void;
  onPageSizeChange?: (size: number) => void;
};

function buildPageList(current: number, total: number): (number | '…')[] {
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);

  const pages: (number | '…')[] = [];

  if (current <= 4) {
    for (let i = 1; i <= 5; i++) pages.push(i);
    pages.push('…');
    pages.push(total);
  } else if (current >= total - 3) {
    pages.push(1);
    pages.push('…');
    for (let i = total - 4; i <= total; i++) pages.push(i);
  } else {
    pages.push(1);
    pages.push('…');
    pages.push(current - 1);
    pages.push(current);
    pages.push(current + 1);
    pages.push('…');
    pages.push(total);
  }

  return pages;
}

export function Pagination({
  page,
  totalPages,
  totalItems,
  pageSize,
  pageSizeOptions = [10, 20, 50],
  onPageChange,
  onPageSizeChange,
}: Props) {
  const from = totalItems === 0 ? 0 : (page - 1) * pageSize + 1;
  const to   = Math.min(page * pageSize, totalItems);
  const pages = buildPageList(page, totalPages);

  const btnBase =
    'inline-flex items-center justify-center h-8 min-w-[2rem] px-1 rounded-md text-sm font-medium transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 disabled:opacity-40 disabled:cursor-not-allowed';

  return (
    <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 px-4 py-3 bg-gray-50 dark:bg-gray-800/50 border-t border-gray-200 dark:border-gray-700">
      {/* Left: count + page size */}
      <div className="flex items-center gap-3 text-xs text-gray-500 dark:text-gray-400">
        <span>
          {totalItems === 0
            ? 'No items'
            : `Showing ${from}–${to} of ${totalItems}`}
        </span>

        {onPageSizeChange && (
          <div className="flex items-center gap-1.5">
            <span>Rows:</span>
            <select
              value={pageSize}
              onChange={(e) => {
                onPageSizeChange(Number(e.target.value));
                onPageChange(1);
              }}
              className="h-7 rounded-md border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-gray-700 dark:text-gray-300 text-xs px-1.5 focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500"
            >
              {pageSizeOptions.map((s) => (
                <option key={s} value={s}>{s}</option>
              ))}
            </select>
          </div>
        )}
      </div>

      {/* Right: page controls */}
      {totalPages > 1 && (
        <div className="flex items-center gap-1">
          {/* First */}
          <button
            className={cn(btnBase, 'text-gray-500 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700')}
            onClick={() => onPageChange(1)}
            disabled={page === 1}
            title="First page"
          >
            <ChevronsLeft className="w-4 h-4" />
          </button>

          {/* Prev */}
          <button
            className={cn(btnBase, 'text-gray-500 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700')}
            onClick={() => onPageChange(page - 1)}
            disabled={page === 1}
            title="Previous page"
          >
            <ChevronLeft className="w-4 h-4" />
          </button>

          {/* Page numbers */}
          {pages.map((p, i) =>
            p === '…' ? (
              <span
                key={`ellipsis-${i}`}
                className="h-8 min-w-[2rem] flex items-center justify-center text-sm text-gray-400"
              >
                …
              </span>
            ) : (
              <button
                key={p}
                onClick={() => onPageChange(p as number)}
                className={cn(
                  btnBase,
                  p === page
                    ? 'bg-primary-700 text-white shadow-sm'
                    : 'text-gray-600 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700',
                )}
              >
                {p}
              </button>
            )
          )}

          {/* Next */}
          <button
            className={cn(btnBase, 'text-gray-500 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700')}
            onClick={() => onPageChange(page + 1)}
            disabled={page === totalPages}
            title="Next page"
          >
            <ChevronRight className="w-4 h-4" />
          </button>

          {/* Last */}
          <button
            className={cn(btnBase, 'text-gray-500 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700')}
            onClick={() => onPageChange(totalPages)}
            disabled={page === totalPages}
            title="Last page"
          >
            <ChevronsRight className="w-4 h-4" />
          </button>
        </div>
      )}
    </div>
  );
}
