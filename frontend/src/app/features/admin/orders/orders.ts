import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { DialogService } from '../../../core/dialog/dialog.service';
import { ToastService } from '../../../core/toast/toast.service';
import {
  AdminOrdersService,
  AdminOrderListDto,
  AdminOrderDetailDto,
  OrderStatus,
} from '../../../core/admin/admin-orders.service';
import { OrderStatusHistoryDto } from '../../../core/order/order.service';

@Component({
  selector: 'app-admin-orders',
  standalone: false,
  templateUrl: './orders.html',
  styleUrl: './orders.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Orders implements OnInit {
  readonly statuses: OrderStatus[] = [
    'Pending', 'Confirmed', 'Processing',
    'Shipped', 'Delivered', 'Cancelled', 'Refunded',
  ];

  orders: AdminOrderListDto[] = [];
  loading = true;
  error = '';

  searchTerm = '';
  statusFilter: '' | OrderStatus = '';
  readonly pageSize = 8;
  page = 1;

  viewing: AdminOrderDetailDto | null = null;
  viewingLoading = false;

  showStatusForm = false;
  editing: AdminOrderListDto | null = null;
  pendingStatus: OrderStatus = 'Pending';
  pendingNote = '';
  saving = false;

  // Timeline modal
  timelineOpen = false;
  timeline: OrderStatusHistoryDto[] = [];
  loadingTimeline = false;
  timelineForOrderId: number | null = null;

  constructor(
    private service: AdminOrdersService,
    private dialog: DialogService,
    private toast: ToastService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    const search = this.searchTerm.trim() || undefined;
    const status = this.statusFilter || undefined;
    this.service.getAll(search, status).subscribe({
      next: list => {
        this.orders = list;
        this.page = 1;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.error = 'Failed to load orders.';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  applyFilters(): void {
    this.load();
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.statusFilter = '';
    this.load();
  }

  get totalPages(): number { return Math.max(1, Math.ceil(this.orders.length / this.pageSize)); }
  get pagedOrders(): AdminOrderListDto[] {
    const start = (this.page - 1) * this.pageSize;
    return this.orders.slice(start, start + this.pageSize);
  }
  goPage(page: number): void {
    this.page = Math.min(Math.max(page, 1), this.totalPages);
    this.cdr.markForCheck();
  }

  // ── View
  openView(o: AdminOrderListDto): void {
    this.viewingLoading = true;
    this.viewing = null;
    this.cdr.markForCheck();
    this.service.getById(o.orderId).subscribe({
      next: detail => {
        this.viewing = detail;
        this.viewingLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.error = 'Failed to load order details.';
        this.viewingLoading = false;
        this.cdr.markForCheck();
      },
    });
  }

  closeView(): void {
    this.viewing = null;
    this.cdr.markForCheck();
  }

  // ── Edit status
  openEditStatus(o: AdminOrderListDto): void {
    this.editing = o;
    this.pendingStatus = o.status;
    this.showStatusForm = true;
    this.error = '';
    this.cdr.markForCheck();
  }

  cancelEditStatus(): void {
    this.showStatusForm = false;
    this.editing = null;
    this.cdr.markForCheck();
  }

  saveStatus(): void {
    if (!this.editing) return;
    if (this.pendingStatus === this.editing.status) {
      this.cancelEditStatus();
      return;
    }

    this.saving = true;
    this.service.updateStatus(
      this.editing.orderId,
      this.pendingStatus,
      this.pendingNote.trim() || undefined,
    ).subscribe({
      next: () => {
        this.toast.success('Status updated', `Order #${this.editing!.orderId} marked as ${this.pendingStatus}.`);
        this.saving = false;
        this.showStatusForm = false;
        this.editing = null;
        this.pendingNote = '';
        this.load();
      },
      error: err => {
        const msg = err?.error?.error ?? 'Status update failed. Please try again.';
        this.toast.error('Update failed', msg);
        this.error = msg;
        this.saving = false;
        this.cdr.markForCheck();
      },
    });
  }

  // ── Timeline modal
  openTimeline(o: AdminOrderListDto): void {
    this.timelineOpen = true;
    this.loadingTimeline = true;
    this.timeline = [];
    this.timelineForOrderId = o.orderId;
    this.cdr.markForCheck();

    this.service.history(o.orderId).subscribe({
      next: rows => {
        this.timeline = rows;
        this.loadingTimeline = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.loadingTimeline = false;
        this.cdr.markForCheck();
      },
    });
  }
  closeTimeline(): void {
    this.timelineOpen = false;
    this.timeline = [];
    this.timelineForOrderId = null;
    this.cdr.markForCheck();
  }

  cancelOrder(o: AdminOrderListDto): void {
    if (o.status === 'Delivered' || o.status === 'Refunded') {
      this.toast.warn('Action not allowed', `Order #${o.orderId} is ${o.status} and cannot be cancelled.`);
      return;
    }
    this.dialog.ask('Cancel Order', `Cancel order #${o.orderId} for ${o.buyerName}? This action sets status to Cancelled.`, true)
      .subscribe(confirmed => {
        if (!confirmed) return;

        this.service.cancel(o.orderId).subscribe({
          next: () => {
            this.toast.success('Order cancelled', `Order #${o.orderId} has been cancelled.`);
            this.load();
          },
          error: () => {
            this.toast.error('Cancel failed', 'Could not cancel the order. Please try again.');
            this.cdr.markForCheck();
          },
        });
      });
  }

  statusVariant(status: string): string {
    switch (status) {
      case 'Pending':    return 'pending';
      case 'Confirmed':
      case 'Processing': return 'progress';
      case 'Shipped':    return 'shipped';
      case 'Delivered':  return 'delivered';
      case 'Cancelled':  return 'cancelled';
      case 'Refunded':   return 'refunded';
      default:           return '';
    }
  }

  trackById     = (_: number, o: AdminOrderListDto) => o.orderId;
  trackHistory  = (_: number, h: OrderStatusHistoryDto) => h.orderStatusHistoryId;
}
