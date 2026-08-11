import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import {
  AdminCustomersService,
  CustomerDto,
  CustomerDetailDto,
  UpdateCustomerDto
} from '../../../core/admin/admin-customers.service';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialog } from '../../../shared/components/confirm-dialog/confirm-dialog';
import { ToastService } from '../../../core/toast/toast.service';

interface CustomerForm {
  fullName: string;
  phone: string;
  preferredCurrency: string;
  isActive: boolean;
}

@Component({
  selector: 'app-admin-customers',
  standalone: false,
  templateUrl: './admin-customers.html',
  styleUrl: './admin-customers.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminCustomers implements OnInit {
  customers: CustomerDto[] = [];
  loading = true;
  error = '';

  searchTerm = '';
  statusFilter: '' | 'true' | 'false' = '';
  readonly pageSize = 8;
  page = 1;

  viewing: CustomerDetailDto | null = null;
  viewingLoading = false;

  showForm = false;
  editing: CustomerDto | null = null;
  saving = false;

  readonly currencyOptions = ['CAD', 'USD', 'EUR', 'GBP', 'INR', 'AUD'];

  form: CustomerForm = this.emptyForm();

  constructor(
    private service: AdminCustomersService,
    private cdr: ChangeDetectorRef,
    private dialog: MatDialog,
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
        this.customers = list;
        this.page = 1;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.toast.error('Failed to load customers.');
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

  get totalPages(): number { return Math.max(1, Math.ceil(this.customers.length / this.pageSize)); }
  get pagedCustomers(): CustomerDto[] {
    const start = (this.page - 1) * this.pageSize;
    return this.customers.slice(start, start + this.pageSize);
  }
  goPage(page: number): void {
    this.page = Math.min(Math.max(page, 1), this.totalPages);
    this.cdr.markForCheck();
  }

  openView(c: CustomerDto): void {
    this.viewingLoading = true;
    this.viewing = null;
    this.cdr.markForCheck();
    this.service.getById(c.userId).subscribe({
      next: detail => {
        this.viewing = detail;
        this.viewingLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.toast.error('Failed to load customer details.');
        this.viewingLoading = false;
        this.cdr.markForCheck();
      },
    });
  }

  closeView(): void {
    this.viewing = null;
    this.cdr.markForCheck();
  }

  openEdit(c: CustomerDto): void {
    this.editing = c;
    this.form = {
      fullName: c.fullName,
      phone: c.phone ?? '',
      preferredCurrency: c.preferredCurrency,
      isActive: c.isActive,
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
    if (!this.form.fullName.trim()) {
      this.error = 'Full name is required.';
      this.cdr.markForCheck();
      return;
    }

    this.saving = true;
    const dto: UpdateCustomerDto = {
      fullName: this.form.fullName.trim(),
      phone: this.form.phone || undefined,
      preferredCurrency: this.form.preferredCurrency || 'CAD',
      isActive: this.form.isActive,
    };

    this.service.update(this.editing.userId, dto).subscribe({
      next: () => {
        this.toast.success(`${this.editing!.fullName}'s profile updated.`);
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

  toggleStatus(c: CustomerDto): void {
    const next = !c.isActive;
    this.service.setStatus(c.userId, next).subscribe({
      next: () => {
        this.toast.success(`${c.fullName} ${next ? 'activated' : 'deactivated'}.`);
        this.load();
      },
      error: () => {
        this.toast.error('Status update failed.');
        this.cdr.markForCheck();
      },
    });
  }

  delete(c: CustomerDto): void {
    const dialogRef = this.dialog.open(ConfirmDialog, {
      data: {
        title: 'Delete Customer',
        message: `Are you sure you want to delete "${c.fullName}"? This will prevent them from logging in.`,
        isDanger: true
      }
    });
    dialogRef.afterClosed().subscribe(res => {
      if (res) {
        this.service.delete(c.userId).subscribe({
          next: () => {
            this.toast.success(`${c.fullName} has been deleted.`);
            this.load();
          },
          error: () => {
            this.toast.error('Delete operation failed.');
            this.cdr.markForCheck();
          },
        });
      }
    });
  }

  trackById = (_: number, c: CustomerDto) => c.userId;

  /**
   * Deterministic cartoon avatar for a customer using DiceBear's `avataaars`
   * style — recognisable cartoon people with hair, accessories, and outfits.
   * Seed is the lowercased email (or userId fallback) so a given customer
   * always renders the same character.
   */
  avatarFor(c: { email?: string; userId: number }): string {
    const seed = encodeURIComponent((c.email || `user-${c.userId}`).toLowerCase().trim());
    return `https://api.dicebear.com/7.x/avataaars/svg?seed=${seed}&radius=50&backgroundColor=ffe4cc,ffdab3,ffd1a3,ffe9d6,fef3c7,e0f2fe,fce7f3`;
  }

  private emptyForm(): CustomerForm {
    return { fullName: '', phone: '', preferredCurrency: 'CAD', isActive: true };
  }
}
