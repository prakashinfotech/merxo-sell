import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ModerationService, PendingProductDto } from '../../../core/admin/moderation.service';
import { ProductService } from '../../../features/products/product.service';
import { ProductDetail } from '../../../shared/models/product.models';

@Component({
  selector: 'app-moderation-products',
  templateUrl: './moderation-products.html',
  styleUrls: ['./moderation.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ModerationProducts implements OnInit {
  loading = true;
  saving  = false;
  error   = '';
  products: PendingProductDto[] = [];
  rejectingId: number | null = null;
  rejectReason = '';

  // Preview modal state
  previewLoading = false;
  previewError   = '';
  previewProduct: ProductDetail | null = null;
  previewSummary: PendingProductDto | null = null;
  activeImageIndex = 0;

  constructor(
    private svc: ModerationService,
    private productService: ProductService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.cdr.markForCheck();
    this.svc.getPendingProducts().subscribe({
      next: rows => { this.products = rows; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.error = 'Failed to load pending products.'; this.loading = false; this.cdr.markForCheck(); },
    });
  }

  approve(p: PendingProductDto): void {
    this.saving = true;
    this.cdr.markForCheck();
    this.svc.approveProduct(p.productId).subscribe({
      next: () => {
        this.products = this.products.filter(x => x.productId !== p.productId);
        // Close the preview modal if we approved the same product.
        if (this.previewSummary?.productId === p.productId) this.closePreview();
        this.saving = false; this.cdr.markForCheck();
      },
      error: () => { this.saving = false; this.error = 'Approve failed.'; this.cdr.markForCheck(); },
    });
  }

  beginReject(id: number): void {
    this.rejectingId = id;
    this.rejectReason = '';
    this.cdr.markForCheck();
  }

  cancelReject(): void {
    this.rejectingId = null;
    this.rejectReason = '';
    this.cdr.markForCheck();
  }

  confirmReject(): void {
    if (!this.rejectingId || !this.rejectReason.trim()) return;
    const id = this.rejectingId;
    this.saving = true;
    this.cdr.markForCheck();
    this.svc.rejectProduct(id, this.rejectReason.trim()).subscribe({
      next: () => {
        this.products = this.products.filter(x => x.productId !== id);
        if (this.previewSummary?.productId === id) this.closePreview();
        this.cancelReject();
        this.saving = false;
        this.cdr.markForCheck();
      },
      error: () => { this.saving = false; this.error = 'Reject failed.'; this.cdr.markForCheck(); },
    });
  }

  // ── Preview modal ────────────────────────────────────────────────
  openPreview(p: PendingProductDto): void {
    this.previewSummary = p;
    this.previewProduct = null;
    this.previewError   = '';
    this.previewLoading = true;
    this.activeImageIndex = 0;
    document.body.classList.add('modal-open');
    this.cdr.markForCheck();

    this.productService.getById(p.productId).subscribe({
      next: detail => {
        this.previewProduct = detail;
        this.previewLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        // Buyer-facing detail endpoint hides Pending products. Show the
        // summary card we already have instead of pretending nothing loaded.
        this.previewError = 'Full preview unavailable for products awaiting approval. Showing summary view only.';
        this.previewLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  closePreview(): void {
    this.previewSummary = null;
    this.previewProduct = null;
    this.previewLoading = false;
    this.previewError = '';
    document.body.classList.remove('modal-open');
    this.cdr.markForCheck();
  }

  setActiveImage(i: number): void {
    this.activeImageIndex = i;
    this.cdr.markForCheck();
  }
}
