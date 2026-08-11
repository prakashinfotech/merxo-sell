import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import {
  OrderService, OrderDto, OrderItemDto, OrderStatusHistoryDto,
} from '../../core/order/order.service';
import { CurrencyService } from '../../core/currency/currency.service';
import { ReviewService } from '../../core/review/review.service';
import { ToastService } from '../../core/toast/toast.service';

@Component({
  selector: 'app-orders',
  templateUrl: './orders.html',
  styleUrls: ['./orders.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Orders implements OnInit {
  orders: OrderDto[] = [];
  loading = true;
  error = false;

  // Timeline modal
  timelineOpen = false;
  timeline: OrderStatusHistoryDto[] = [];
  loadingTimeline = false;
  timelineForOrderId: number | null = null;

  // Cancel modal
  cancelOpen = false;
  cancelOrderId: number | null = null;
  cancelReason = '';
  cancelling = false;
  cancelError = '';

  // Review modal — buyer rates a product from a delivered order
  reviewOpen = false;
  reviewItem: OrderItemDto | null = null;
  reviewRating = 5;
  reviewComment = '';
  reviewSubmitting = false;
  reviewError = '';

  constructor(
    private orderService:    OrderService,
    public  currencyService: CurrencyService,
    private reviewService:   ReviewService,
    private cdr: ChangeDetectorRef,
    private toast: ToastService,
  ) {}

  ngOnInit(): void { this.loadOrders(); }

  loadOrders(): void {
    this.loading = true;
    this.error = false;
    this.orderService.getUserOrders().subscribe({
      next: orders => {
        this.orders = orders.sort((a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.error = true;
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  getStatusClass(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'delivered': return 'status--success';
      case 'shipped':   return 'status--info';
      case 'cancelled': return 'status--danger';
      case 'refunded':  return 'status--danger';
      case 'pending':   return 'status--warning';
      case 'processing':
      case 'confirmed': return 'status--info';
      default:          return '';
    }
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

  canCancel(o: OrderDto): boolean {
    return o.status === 'Pending';
  }

  // ── Timeline modal
  openTimeline(o: OrderDto): void {
    this.timelineOpen = true;
    this.loadingTimeline = true;
    this.timeline = [];
    this.timelineForOrderId = o.orderId;
    this.cdr.markForCheck();

    this.orderService.getOrderHistory(o.orderId).subscribe({
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

  // ── Cancel modal
  openCancelModal(o: OrderDto): void {
    if (!this.canCancel(o)) return;
    this.cancelOrderId = o.orderId;
    this.cancelReason = '';
    this.cancelError = '';
    this.cancelOpen = true;
    this.cdr.markForCheck();
  }
  closeCancelModal(): void {
    this.cancelOpen = false;
    this.cancelOrderId = null;
    this.cancelReason = '';
    this.cdr.markForCheck();
  }
  confirmCancel(): void {
    if (!this.cancelOrderId || !this.cancelReason.trim()) {
      this.cancelError = 'Please provide a reason.';
      this.cdr.markForCheck();
      return;
    }
    this.cancelling = true;
    this.orderService.cancelOrder(this.cancelOrderId, this.cancelReason.trim()).subscribe({
      next: () => {
        this.toast.success(`Order #${this.cancelOrderId} cancelled.`);
        this.cancelling = false;
        this.cancelOpen = false;
        this.cancelOrderId = null;
        this.cancelReason = '';
        this.loadOrders();
      },
      error: err => {
        this.cancelError = err?.error?.error ?? 'Cancel failed.';
        this.cancelling = false;
        this.cdr.markForCheck();
      },
    });
  }

  // ── Review modal ──────────────────────────────────────────────────────
  /**
   * Buyers can only review a product they actually received. Surface the
   * "Write Review" button only on items inside a Delivered order — the
   * server enforces the same rule (`HasUserPurchasedProductAsync`).
   */
  canReview(order: OrderDto): boolean {
    return (order.status ?? '').toLowerCase() === 'delivered';
  }

  openReviewModal(item: OrderItemDto): void {
    this.reviewItem      = item;
    this.reviewRating    = 5;
    this.reviewComment   = '';
    this.reviewError     = '';
    this.reviewOpen      = true;
    this.cdr.markForCheck();
  }

  closeReviewModal(): void {
    this.reviewOpen      = false;
    this.reviewItem      = null;
    this.reviewError     = '';
    this.cdr.markForCheck();
  }

  setReviewRating(rating: number): void {
    this.reviewRating = rating;
    this.cdr.markForCheck();
  }

  submitReview(): void {
    if (!this.reviewItem) return;
    if (this.reviewRating < 1 || this.reviewRating > 5) {
      this.reviewError = 'Please choose a rating between 1 and 5 stars.';
      this.cdr.markForCheck();
      return;
    }

    this.reviewSubmitting = true;
    this.reviewError = '';

    this.reviewService.addReview({
      productId: this.reviewItem.productId,
      rating:    this.reviewRating,
      comment:   this.reviewComment.trim() || undefined,
    }).subscribe({
      next: () => {
        this.toast.success(`Thanks! Your review for "${this.reviewItem!.productName}" has been posted.`);
        this.reviewSubmitting = false;
        this.reviewOpen       = false;
        this.reviewItem       = null;
        this.cdr.markForCheck();
      },
      error: err => {
        this.reviewSubmitting = false;
        this.reviewError = err?.error?.error
          ?? 'We couldn\'t post your review. You may have already reviewed this product.';
        this.cdr.markForCheck();
      },
    });
  }

  starsTrack = (i: number) => i;

  trackByOrderId = (_: number, o: OrderDto) => o.orderId;
  trackHistory   = (_: number, h: OrderStatusHistoryDto) => h.orderStatusHistoryId;
}
