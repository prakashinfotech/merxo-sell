import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { AdminApiService, AdminProduct, UpdateProductPayload } from '../../../core/admin/admin-api.service';
import { MediaService } from '../../../core/media/media.service';
import { DialogService } from '../../../core/dialog/dialog.service';
import { ToastService } from '../../../core/toast/toast.service';

@Component({
  selector: 'app-admin-products',
  templateUrl: './admin-products.html',
  styleUrls: ['./admin-products.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminProducts implements OnInit {
  products: AdminProduct[] = [];
  loading = true;
  error = '';
  search = '';
  readonly pageSize = 8;
  page = 1;

  editingProduct: AdminProduct | null = null;
  viewingProduct: AdminProduct | null = null;
  editForm: UpdateProductPayload & { name: string } = this.emptyEdit();
  saving = false;

  imageSource: 'url' | 'upload' = 'url';
  uploadingFile = false;

  constructor(
    private adminApi: AdminApiService,
    private mediaService: MediaService,
    private cdr: ChangeDetectorRef,
    private dialog: DialogService,
    private toast: ToastService,
  ) {}

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      this.uploadingFile = true;
      this.cdr.markForCheck();
      this.mediaService.uploadFile(file).subscribe({
        next: (res) => {
          this.editForm.primaryImageUrl = res.url;
          this.uploadingFile = false;
          this.cdr.markForCheck();
        },
        error: () => {
          this.dialog.notify('Upload Failed', 'There was an error uploading the image. Please try again.');
          this.uploadingFile = false;
          this.cdr.markForCheck();
        }
      });
    }
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.adminApi.getProducts(this.search || undefined).subscribe({
      next: p => { this.products = p; this.page = 1; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.toast.error('Failed to load products.'); this.loading = false; this.cdr.markForCheck(); }
    });
  }

  onSearch(): void { this.load(); }

  get totalPages(): number { return Math.max(1, Math.ceil(this.products.length / this.pageSize)); }
  get pagedProducts(): AdminProduct[] {
    const start = (this.page - 1) * this.pageSize;
    return this.products.slice(start, start + this.pageSize);
  }
  goPage(page: number): void {
    this.page = Math.min(Math.max(page, 1), this.totalPages);
    this.cdr.markForCheck();
  }

  openEdit(p: AdminProduct): void {
    this.editingProduct = p;
    this.editForm = {
      name: p.name,
      basePrice: p.basePrice,
      salePrice: p.salePrice,
      stock: p.stock,
      isActive: p.isActive,
      primaryImageUrl: p.primaryImageUrl
    };
    this.error = '';
    this.cdr.markForCheck();
  }

  openView(p: AdminProduct): void {
    this.viewingProduct = p;
    this.cdr.markForCheck();
  }

  closeView(): void {
    this.viewingProduct = null;
    this.cdr.markForCheck();
  }

  cancelEdit(): void { this.editingProduct = null; this.cdr.markForCheck(); }

  saveEdit(): void {
    if (!this.editingProduct) return;
    this.saving = true;
    this.adminApi.updateProduct(this.editingProduct.productId, this.editForm).subscribe({
      next: () => {
        this.toast.success(`"${this.editForm.name}" updated.`);
        this.saving = false;
        this.editingProduct = null;
        this.load();
      },
      error: () => { this.error = 'Update failed.'; this.saving = false; this.cdr.markForCheck(); }
    });
  }

  viewProduct(p: AdminProduct): void {
    this.openView(p);
  }

  deactivate(p: AdminProduct): void {
    this.dialog.ask('Delete Product', `Are you sure you want to delete "${p.name}"? This will hide it from the store.`, true)
      .subscribe(res => {
        if (res) {
          this.adminApi.deleteProduct(p.productId).subscribe({
            next: () => { this.toast.success(`"${p.name}" has been deleted.`); this.load(); },
            error: () => { this.toast.error('Delete operation failed.'); this.cdr.markForCheck(); }
          });
        }
      });
  }



  private emptyEdit(): UpdateProductPayload & { name: string } {
    return { name: '', basePrice: 0, stock: 0, isActive: true };
  }
}
