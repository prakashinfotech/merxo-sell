import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { DialogService } from '../../../core/dialog/dialog.service';
import { ToastService } from '../../../core/toast/toast.service';
import {
  AdminSellersService,
  AdminSellerDto,
  UpdateAdminSellerDto
} from '../../../core/admin/admin-sellers.service';

interface SellerForm {
  storeName: string;
  storeDescription: string;
  contactEmail: string;
  phone: string;
}

@Component({
  selector: 'app-admin-sellers',
  standalone: false,
  templateUrl: './sellers.html',
  styleUrl: './sellers.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Sellers implements OnInit {
  sellers: AdminSellerDto[] = [];
  loading = true;
  error = '';

  searchTerm = '';
  statusFilter: '' | 'true' | 'false' = '';
  readonly pageSize = 8;
  page = 1;

  showForm = false;
  viewing: AdminSellerDto | null = null;
  editing: AdminSellerDto | null = null;
  saving = false;

  form: SellerForm = this.emptyForm();

  constructor(
    private service: AdminSellersService,
    private dialog: DialogService,
    private cdr: ChangeDetectorRef,
    private toast: ToastService,
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    const isActive = this.statusFilter === '' ? undefined : this.statusFilter === 'true';
    const search = this.searchTerm.trim() || undefined;
    this.service.getAll(search, isActive).subscribe({
      next: list => {
        this.sellers = list;
        this.page = 1;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.toast.error('Failed to load sellers.');
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

  get totalPages(): number { return Math.max(1, Math.ceil(this.sellers.length / this.pageSize)); }
  get pagedSellers(): AdminSellerDto[] {
    const start = (this.page - 1) * this.pageSize;
    return this.sellers.slice(start, start + this.pageSize);
  }
  goPage(page: number): void {
    this.page = Math.min(Math.max(page, 1), this.totalPages);
    this.cdr.markForCheck();
  }

  openView(s: AdminSellerDto): void {
    this.viewing = s;
    this.cdr.markForCheck();
  }

  closeView(): void {
    this.viewing = null;
    this.cdr.markForCheck();
  }

  openEdit(s: AdminSellerDto): void {
    this.editing = s;
    this.form = {
      storeName: s.storeName,
      storeDescription: s.storeDescription ?? '',
      contactEmail: s.contactEmail ?? '',
      phone: s.phone ?? '',
    };
    this.showForm = true;
    this.error = '';
    this.cdr.markForCheck();
  }

  cancelForm(): void {
    this.showForm = false;
    this.editing = null;
    this.cdr.markForCheck();
  }

  save(): void {
    if (!this.editing) return;
    if (!this.form.storeName.trim()) {
      this.error = 'Store name is required.';
      this.cdr.markForCheck();
      return;
    }

    this.saving = true;
    const dto: UpdateAdminSellerDto = {
      storeName: this.form.storeName.trim(),
      storeDescription: this.form.storeDescription || undefined,
      contactEmail: this.form.contactEmail || undefined,
      phone: this.form.phone || undefined,
    };

    this.service.update(this.editing.sellerId, dto).subscribe({
      next: () => {
        this.toast.success('Seller profile updated.');
        this.saving = false;
        this.showForm = false;
        this.load();
      },
      error: () => {
        this.error = 'Update failed. Please try again.';
        this.saving = false;
        this.cdr.markForCheck();
      },
    });
  }

  toggleStatus(s: AdminSellerDto): void {
    const next = !s.isActive;
    this.service.setStatus(s.sellerId, next).subscribe({
      next: () => {
        this.toast.success(`"${s.storeName}" ${next ? 'activated' : 'deactivated'}.`);
        this.load();
      },
      error: () => {
        this.toast.error('Status update failed.');
        this.cdr.markForCheck();
      },
    });
  }

  toggleVerified(s: AdminSellerDto): void {
    const next = !s.isVerified;
    this.service.setVerified(s.sellerId, next).subscribe({
      next: () => {
        this.toast.success(`"${s.storeName}" ${next ? 'granted' : 'revoked'} verification.`);
        this.load();
      },
      error: () => {
        this.toast.error('Verification update failed.');
        this.cdr.markForCheck();
      },
    });
  }

  delete(s: AdminSellerDto): void {
    this.dialog.ask('Deactivate Seller', `Deactivate seller "${s.storeName}"? Their products will be hidden from buyers.`, true)
      .subscribe(confirmed => {
        if (!confirmed) return;
        
        this.service.delete(s.sellerId).subscribe({
          next: () => {
            this.toast.success(`"${s.storeName}" deactivated.`);
            this.load();
          },
          error: () => {
            this.toast.error('Delete operation failed.');
            this.cdr.markForCheck();
          },
        });
      });
  }

  trackById = (_: number, s: AdminSellerDto) => s.sellerId;

  private emptyForm(): SellerForm {
    return { storeName: '', storeDescription: '', contactEmail: '', phone: '' };
  }
}
