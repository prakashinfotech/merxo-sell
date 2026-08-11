import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { DialogService } from '../../../core/dialog/dialog.service';
import { ToastService } from '../../../core/toast/toast.service';
import {
  AdminCurrenciesService,
  AdminCurrencyDto,
  CreateCurrencyDto,
  UpdateCurrencyDto,
} from '../../../core/admin/admin-currencies.service';

interface CurrencyForm {
  currencyCode: string;
  currencyName: string;
  rateToCad: number;
  symbol: string;
  isActive: boolean;
}

@Component({
  selector: 'app-admin-currencies',
  standalone: false,
  templateUrl: './currencies.html',
  styleUrl: './currencies.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Currencies implements OnInit {
  private static readonly BASE = 'CAD';

  currencies: AdminCurrencyDto[] = [];
  loading = true;
  error = '';

  searchTerm = '';
  readonly pageSize = 8;
  page = 1;

  showForm = false;
  viewing: AdminCurrencyDto | null = null;
  editing: AdminCurrencyDto | null = null;
  saving = false;

  form: CurrencyForm = this.emptyForm();

  constructor(
    private service: AdminCurrenciesService,
    private dialog: DialogService,
    private cdr: ChangeDetectorRef,
    private toast: ToastService,
  ) {}

  ngOnInit(): void {
    this.load();
  }

  get filteredCurrencies(): AdminCurrencyDto[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.currencies;
    return this.currencies.filter(c =>
      c.currencyCode.toLowerCase().includes(term) ||
      c.currencyName.toLowerCase().includes(term));
  }

  get totalPages(): number { return Math.max(1, Math.ceil(this.filteredCurrencies.length / this.pageSize)); }
  get pagedCurrencies(): AdminCurrencyDto[] {
    const start = (this.page - 1) * this.pageSize;
    return this.filteredCurrencies.slice(start, start + this.pageSize);
  }
  goPage(page: number): void {
    this.page = Math.min(Math.max(page, 1), this.totalPages);
    this.cdr.markForCheck();
  }

  isBase(c: AdminCurrencyDto): boolean {
    return c.currencyCode === Currencies.BASE;
  }

  load(): void {
    this.loading = true;
    this.service.getAll().subscribe({
      next: list => {
        this.currencies = list;
        this.page = 1;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.toast.error('Failed to load currency rates.');
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  resetSearch(): void {
    this.searchTerm = '';
    this.page = 1;
    this.cdr.markForCheck();
  }

  onSearchChange(): void {
    this.page = 1;
    this.cdr.markForCheck();
  }

  // ── Modals
  openCreate(): void {
    this.editing = null;
    this.form = this.emptyForm();
    this.showForm = true;
    this.error = '';
    this.cdr.markForCheck();
  }

  openEdit(c: AdminCurrencyDto): void {
    this.editing = c;
    this.form = {
      currencyCode: c.currencyCode,
      currencyName: c.currencyName,
      rateToCad: c.rateToCad,
      symbol: c.symbol,
      isActive: c.isActive,
    };
    this.showForm = true;
    this.error = '';
    this.cdr.markForCheck();
  }

  openView(c: AdminCurrencyDto): void {
    this.viewing = c;
    this.cdr.markForCheck();
  }

  closeView(): void {
    this.viewing = null;
    this.cdr.markForCheck();
  }

  cancelForm(): void {
    this.showForm = false;
    this.editing = null;
    this.cdr.markForCheck();
  }

  save(): void {
    const code = this.form.currencyCode.trim().toUpperCase();
    const name = this.form.currencyName.trim();
    const symbol = this.form.symbol.trim();

    if (!code || code.length < 2 || code.length > 10) {
      this.error = 'Currency code must be 2–10 characters.';
      this.cdr.markForCheck();
      return;
    }
    if (!name) {
      this.error = 'Currency name is required.';
      this.cdr.markForCheck();
      return;
    }
    if (!symbol) {
      this.error = 'Symbol is required.';
      this.cdr.markForCheck();
      return;
    }
    if (!(this.form.rateToCad > 0)) {
      this.error = 'Rate must be greater than zero.';
      this.cdr.markForCheck();
      return;
    }

    this.saving = true;
    if (this.editing) {
      const dto: UpdateCurrencyDto = {
        currencyName: name,
        rateToCad: this.form.rateToCad,
        symbol,
        isActive: this.form.isActive,
      };
      this.service.update(this.editing.currencyCode, dto).subscribe({
        next: () => {
          this.toast.success(`${this.editing!.currencyCode} updated.`);
          this.saving = false;
          this.showForm = false;
          this.editing = null;
          this.load();
        },
        error: err => {
          this.error = err?.error?.error || 'Update failed. Please try again.';
          this.saving = false;
          this.cdr.markForCheck();
        },
      });
    } else {
      const dto: CreateCurrencyDto = {
        currencyCode: code,
        currencyName: name,
        rateToCad: this.form.rateToCad,
        symbol,
        isActive: this.form.isActive,
      };
      this.service.create(dto).subscribe({
        next: () => {
          this.toast.success(`${code} added.`);
          this.saving = false;
          this.showForm = false;
          this.load();
        },
        error: err => {
          this.error = err?.error?.error || 'Create failed. Please try again.';
          this.saving = false;
          this.cdr.markForCheck();
        },
      });
    }
  }

  toggleActive(c: AdminCurrencyDto): void {
    if (this.isBase(c)) {
      this.toast.warn('CAD is the base currency and cannot be deactivated.');
      return;
    }
    const next = !c.isActive;
    const dto: UpdateCurrencyDto = {
      currencyName: c.currencyName,
      rateToCad: c.rateToCad,
      symbol: c.symbol,
      isActive: next,
    };
    this.service.update(c.currencyCode, dto).subscribe({
      next: () => {
        this.toast.success(`${c.currencyCode} ${next ? 'activated' : 'deactivated'}.`);
        this.load();
      },
      error: () => {
        this.toast.error('Status change failed.');
        this.cdr.markForCheck();
      },
    });
  }

  delete(c: AdminCurrencyDto): void {
    if (this.isBase(c)) {
      this.toast.warn('CAD is the base currency and cannot be removed.');
      return;
    }
    this.dialog.ask('Deactivate Currency', `Deactivate currency "${c.currencyCode} - ${c.currencyName}"?`, true)
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.service.delete(c.currencyCode).subscribe({
          next: () => {
            this.toast.success(`${c.currencyCode} deactivated.`);
            this.load();
          },
          error: () => {
            this.toast.error('Delete operation failed.');
            this.cdr.markForCheck();
          },
        });
      });
  }

  trackByCode = (_: number, c: AdminCurrencyDto) => c.currencyCode;

  private emptyForm(): CurrencyForm {
    return { currencyCode: '', currencyName: '', rateToCad: 1, symbol: '', isActive: true };
  }
}
