import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import {
  SellerService, SellerOrderListDto, SellerOrderDetailDto,
} from '../../../core/seller/seller.service';
import { SellerOrdersService } from '../../../core/seller/seller-orders.service';
import { OrderStatusHistoryDto } from '../../../core/order/order.service';
import { ToastService } from '../../../core/toast/toast.service';

/** Seller order workflow statuses — single source of truth for the dropdown. */
type SellerStatus = 'Pending' | 'Processing' | 'Ready for Pickup' | 'Shipped' | 'Out for Delivery' | 'Delivered' | 'Cancelled' | 'Returned';

@Component({
  selector: 'app-seller-orders',
  standalone: false,
  templateUrl: './seller-orders.html',
  styleUrls: ['./seller-orders.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SellerOrders implements OnInit {
  readonly statuses: SellerStatus[] = [
    'Pending', 'Processing', 'Ready for Pickup', 'Shipped', 'Out for Delivery', 'Delivered', 'Cancelled', 'Returned',
  ];

  orders: SellerOrderListDto[] = [];
  selectedOrder: SellerOrderDetailDto | null = null;
  loading = true;
  loadingDetail = false;
  error = '';

  searchTerm = '';
  statusFilter = '';
  readonly pageSize = 8;
  page = 1;

  // ── Status update modal state
  statusModalOpen = false;
  statusModalOrder: SellerOrderListDto | null = null;
  pendingStatus: SellerStatus = 'Pending';
  pendingNote = '';
  savingStatus = false;

  // ── Cancel modal state
  cancelModalOpen = false;
  cancelModalOrder: SellerOrderListDto | null = null;
  cancelReason = '';
  cancellingOrder = false;

  // ── Timeline modal state
  timelineOpen = false;
  timeline: OrderStatusHistoryDto[] = [];
  loadingTimeline = false;
  timelineForOrderId: number | null = null;

  constructor(
    private service:     SellerService,
    private ordersAdmin: SellerOrdersService,
    private cdr:         ChangeDetectorRef,
    private toast:       ToastService,
  ) {}

  ngOnInit(): void { this.load(); }

  // ── List ────────────────────────────────────────────────────────────
  load(): void {
    this.loading = true;
    this.error = '';
    this.cdr.markForCheck();

    this.service.getOrders(this.searchTerm || undefined, this.statusFilter || undefined).subscribe({
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

  applyFilters(): void { this.load(); }
  resetFilters(): void {
    this.searchTerm = '';
    this.statusFilter = '';
    this.load();
  }

  get totalPages(): number { return Math.max(1, Math.ceil(this.orders.length / this.pageSize)); }
  get processingCount(): number {
    return this.orders.filter(o => o.status === 'Processing').length;
  }
  get pagedOrders(): SellerOrderListDto[] {
    const start = (this.page - 1) * this.pageSize;
    return this.orders.slice(start, start + this.pageSize);
  }
  goPage(page: number): void {
    this.page = Math.min(Math.max(page, 1), this.totalPages);
    this.cdr.markForCheck();
  }

  // ── Detail
  viewDetail(id: number): void {
    this.loadingDetail = true;
    this.selectedOrder = null;
    this.cdr.markForCheck();

    this.service.getOrder(id).subscribe({
      next: detail => {
        this.selectedOrder = detail;
        this.loadingDetail = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.error = 'Failed to load order details.';
        this.loadingDetail = false;
        this.cdr.markForCheck();
      },
    });
  }
  closeDetail(): void {
    this.selectedOrder = null;
    this.cdr.markForCheck();
  }

  // ── Status update modal
  openStatusModal(o: SellerOrderListDto): void {
    this.statusModalOrder = o;
    this.pendingStatus = (this.statuses.includes(o.status as SellerStatus)
      ? o.status as SellerStatus
      : 'Pending');
    this.pendingNote = '';
    this.statusModalOpen = true;
    this.error = '';
    this.cdr.markForCheck();
  }
  closeStatusModal(): void {
    this.statusModalOpen = false;
    this.statusModalOrder = null;
    this.cdr.markForCheck();
  }

  saveStatus(): void {
    if (!this.statusModalOrder) return;
    if (this.pendingStatus === this.statusModalOrder.status) {
      this.closeStatusModal();
      return;
    }
    this.savingStatus = true;

    this.ordersAdmin.updateStatus(
      this.statusModalOrder.orderId,
      this.pendingStatus,
      this.pendingNote.trim() || undefined,
    ).subscribe({
      next: () => {
        this.toast.success(`Order #${this.statusModalOrder!.orderId} updated to ${this.pendingStatus}.`);
        this.savingStatus = false;
        this.statusModalOpen = false;
        this.statusModalOrder = null;
        this.load();
      },
      error: err => {
        this.error = err?.error?.error ?? 'Status update failed.';
        this.savingStatus = false;
        this.cdr.markForCheck();
      },
    });
  }

  // ── Cancel modal
  openCancelModal(o: SellerOrderListDto): void {
    if (o.status === 'Delivered' || o.status === 'Refunded') {
      this.error = `Order #${o.orderId} is ${o.status} and cannot be cancelled.`;
      this.cdr.markForCheck();
      return;
    }
    this.cancelModalOrder = o;
    this.cancelReason = '';
    this.cancelModalOpen = true;
    this.error = '';
    this.cdr.markForCheck();
  }
  closeCancelModal(): void {
    this.cancelModalOpen = false;
    this.cancelModalOrder = null;
    this.cdr.markForCheck();
  }
  confirmCancel(): void {
    if (!this.cancelModalOrder) return;
    if (!this.cancelReason.trim()) {
      this.error = 'A cancellation reason is required.';
      this.cdr.markForCheck();
      return;
    }
    this.cancellingOrder = true;

    this.ordersAdmin.cancel(this.cancelModalOrder.orderId, this.cancelReason.trim()).subscribe({
      next: () => {
        this.toast.success(`Order #${this.cancelModalOrder!.orderId} cancelled.`);
        this.cancellingOrder = false;
        this.cancelModalOpen = false;
        this.cancelModalOrder = null;
        this.load();
      },
      error: err => {
        this.error = err?.error?.error ?? 'Cancel failed.';
        this.cancellingOrder = false;
        this.cdr.markForCheck();
      },
    });
  }

  // ── Timeline modal
  openTimeline(o: SellerOrderListDto): void {
    this.timelineOpen = true;
    this.loadingTimeline = true;
    this.timeline = [];
    this.timelineForOrderId = o.orderId;
    this.cdr.markForCheck();

    this.ordersAdmin.history(o.orderId).subscribe({
      next: rows => {
        this.timeline = rows;
        this.loadingTimeline = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.loadingTimeline = false;
        this.error = 'Failed to load timeline.';
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

  statusVariant(status: string): string {
    const s = (status || '').toLowerCase();
    if (s.includes('pending'))                              return 'pending';
    if (s.includes('confirm') || s.includes('process'))     return 'progress';
    if (s.includes('ship'))                                 return 'shipped';
    if (s.includes('deliver'))                              return 'delivered';
    if (s.includes('cancel'))                               return 'cancelled';
    if (s.includes('refund'))                               return 'refunded';
    return '';
  }

  trackById = (_: number, o: SellerOrderListDto) => o.orderId;
  trackHistory = (_: number, h: OrderStatusHistoryDto) => h.orderStatusHistoryId;
}
