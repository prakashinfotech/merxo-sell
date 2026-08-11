import {
  Component, Input, Output, EventEmitter, ChangeDetectionStrategy,
} from '@angular/core';

/**
 * Shared pagination control used in the admin + seller portals.
 *
 * Inputs:
 *  - page       (1-based current page)
 *  - pageSize   (page size; informational, drives the "showing X–Y of N" line)
 *  - totalCount (full result-set size — needed to compute totalPages locally)
 *
 * The host owns paging state; the component is purely presentational and
 * emits `pageChange` events. It does NOT navigate, mutate, or filter — just
 * reports the page the user clicked.
 */
@Component({
  selector: 'app-pagination',
  standalone: false,
  templateUrl: './pagination.html',
  styleUrls: ['./pagination.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Pagination {
  @Input() page = 1;
  @Input() pageSize = 10;
  @Input() totalCount = 0;
  @Input() showRange = true;
  @Input() maxButtons = 5;

  @Output() pageChange = new EventEmitter<number>();

  get totalPages(): number {
    if (!this.totalCount || this.totalCount <= 0) return 1;
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  get rangeStart(): number {
    if (this.totalCount === 0) return 0;
    return (this.page - 1) * this.pageSize + 1;
  }

  get rangeEnd(): number {
    return Math.min(this.totalCount, this.page * this.pageSize);
  }

  /**
   * Window of page buttons around the current page, capped at maxButtons.
   * Always includes the first + last page if they aren't in the window.
   */
  get pageButtons(): (number | 'ellipsis-left' | 'ellipsis-right')[] {
    const total = this.totalPages;
    if (total <= this.maxButtons) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }

    const half  = Math.floor(this.maxButtons / 2);
    let   start = Math.max(2, this.page - half);
    let   end   = Math.min(total - 1, this.page + half);

    if (this.page <= half + 1)         { start = 2;             end = this.maxButtons; }
    if (this.page >= total - half)     { start = total - this.maxButtons + 1; end = total - 1; }

    const buttons: (number | 'ellipsis-left' | 'ellipsis-right')[] = [1];
    if (start > 2) buttons.push('ellipsis-left');
    for (let i = start; i <= end; i++) buttons.push(i);
    if (end < total - 1) buttons.push('ellipsis-right');
    buttons.push(total);
    return buttons;
  }

  goTo(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.page) return;
    this.pageChange.emit(page);
  }

  prev(): void { this.goTo(this.page - 1); }
  next(): void { this.goTo(this.page + 1); }
  first(): void { this.goTo(1); }
  last(): void { this.goTo(this.totalPages); }
}
