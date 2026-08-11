import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialog } from '../../../shared/components/confirm-dialog/confirm-dialog';
import { SellerService, SellerProductDto } from '../../../core/seller/seller.service';
import { ToastService } from '../../../core/toast/toast.service';

@Component({
  selector: 'app-seller-products',
  standalone: false,
  templateUrl: './seller-products.html',
  styleUrls: ['./seller-products.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SellerProducts implements OnInit {
  products: SellerProductDto[] = [];
  loading = true;
  error = '';

  searchTerm = '';
  readonly pageSize = 8;
  page = 1;

  constructor(
    private service: SellerService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private dialog: MatDialog,
    private toast: ToastService,
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    const search = this.searchTerm.trim() || undefined;

    this.service.getProducts(search).subscribe({
      next: list => {
        this.products = list;
        this.page = 1;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.error = 'Failed to load products.';
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
    this.load();
  }

  get totalPages(): number { return Math.max(1, Math.ceil(this.products.length / this.pageSize)); }
  get pagedProducts(): SellerProductDto[] {
    const start = (this.page - 1) * this.pageSize;
    return this.products.slice(start, start + this.pageSize);
  }
  goPage(page: number): void {
    this.page = Math.min(Math.max(page, 1), this.totalPages);
    this.cdr.markForCheck();
  }

  goNew(): void {
    this.router.navigate(['/seller/products/new']);
  }

  goEdit(p: SellerProductDto): void {
    this.router.navigate(['/seller/products', p.productId, 'edit']);
  }

  delete(p: SellerProductDto): void {
    const dialogRef = this.dialog.open(ConfirmDialog, {
      data: {
        title: 'Delete Product',
        message: `Are you sure you want to delete "${p.name}"? This will hide the product from buyers.`,
        isDanger: true
      }
    });
    dialogRef.afterClosed().subscribe(res => {
      if (res) {
        this.service.deleteProduct(p.productId).subscribe({
          next: () => {
            this.toast.success(`"${p.name}" has been deleted.`);
            this.load();
          },
          error: () => {
            this.toast.error('Delete failed. Please try again.');
            this.cdr.markForCheck();
          },
        });
      }
    });
  }

  statusVariant(status: string): string {
    const s = status?.toLowerCase() || '';
    if (s.includes('approve') || s.includes('active')) return 'active';
    if (s.includes('pending')) return 'pending';
    if (s.includes('reject') || s.includes('inactive')) return 'rejected';
    return '';
  }

  isLowStock(p: SellerProductDto): boolean {
    return p.stock > 0 && p.stock <= 10;
  }
}
