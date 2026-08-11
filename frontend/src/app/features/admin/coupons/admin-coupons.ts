import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CouponDto, CouponService, CouponUsageStatDto, DiscountType } from '../../../core/coupon/coupon.service';

type CouponForm = {
  couponCode: string;
  title: string;
  description: string;
  discountType: DiscountType;
  discountValue: number;
  minimumPurchaseAmount?: number | null;
  maximumDiscountAmount?: number | null;
  usageLimit?: number | null;
  startDate: string;
  expiryDate?: string | null;
  isActive: boolean;
  currencyCode: string;
};

@Component({
  selector: 'app-admin-coupons',
  templateUrl: './admin-coupons.html',
  styleUrls: ['./admin-coupons.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminCoupons implements OnInit {
  coupons: CouponDto[] = [];
  stats: CouponUsageStatDto[] = [];
  loading = true;
  saving = false;
  error = '';
  editing: CouponDto | null = null;
  showForm = false;
  searchTerm = '';
  /** Visibility filter: all / active / paused / expired. */
  statusFilter: 'all' | 'active' | 'paused' | 'expired' = 'all';

  form: CouponForm = this.emptyForm();

  /** Actual Date objects for mat-datepicker — avoids getter-based infinite change detection loop. */
  startDateVal: Date | null = new Date();
  expiryDateVal: Date | null = null;

  /** Live-filtered list driven by the toolbar search input + status filter. */
  get filteredCoupons(): CouponDto[] {
    const q = this.searchTerm.trim().toLowerCase();
    const now = new Date();
    return this.coupons.filter(c => {
      if (q && !(c.couponCode.toLowerCase().includes(q) || (c.title || '').toLowerCase().includes(q))) return false;
      switch (this.statusFilter) {
        case 'active':  return c.isActive && (!c.expiryDate || new Date(c.expiryDate) > now);
        case 'paused':  return !c.isActive;
        case 'expired': return !!c.expiryDate && new Date(c.expiryDate) <= now;
        default: return true;
      }
    });
  }

  /** Header summary cards. */
  get activeCount(): number {
    const now = new Date();
    return this.coupons.filter(c => c.isActive && (!c.expiryDate || new Date(c.expiryDate) > now)).length;
  }
  get expiredCount(): number {
    const now = new Date();
    return this.coupons.filter(c => !!c.expiryDate && new Date(c.expiryDate) <= now).length;
  }
  get totalRedemptions(): number {
    return this.coupons.reduce((sum, c) => sum + (c.usedCount || 0), 0);
  }
  get totalSavings(): number {
    return this.stats.reduce((sum, s) => sum + (s.totalDiscountGiven || 0), 0);
  }

  /** Days remaining until expiry, or null when indefinite/already expired. */
  daysToExpiry(c: CouponDto): number | null {
    if (!c.expiryDate) return null;
    const diffMs = new Date(c.expiryDate).getTime() - Date.now();
    if (diffMs < 0) return -1;
    return Math.ceil(diffMs / 86_400_000);
  }

  /** Percentage of the usage limit consumed, capped at 100. */
  usagePct(c: CouponDto): number {
    if (!c.usageLimit) return 0;
    return Math.min(100, Math.round((c.usedCount / c.usageLimit) * 100));
  }

  constructor(private couponsApi: CouponService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.couponsApi.getAllAdmin().subscribe({
      next: rows => {
        this.coupons = rows;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.error = 'Could not load coupons.';
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
    this.couponsApi.stats().subscribe(stats => { this.stats = stats; this.cdr.markForCheck(); });
  }

  edit(coupon: CouponDto): void {
    this.editing = coupon;
    this.form = {
      couponCode: coupon.couponCode,
      title: coupon.title,
      description: coupon.description || '',
      discountType: coupon.discountType,
      discountValue: coupon.discountValue,
      minimumPurchaseAmount: coupon.minimumPurchaseAmount ?? null,
      maximumDiscountAmount: coupon.maximumDiscountAmount ?? null,
      usageLimit: coupon.usageLimit ?? null,
      startDate: this.toInputDate(coupon.startDate),
      expiryDate: coupon.expiryDate ? this.toInputDate(coupon.expiryDate) : null,
      isActive: coupon.isActive,
      currencyCode: coupon.currencyCode || 'CAD',
    };
    this.startDateVal = new Date(coupon.startDate);
    this.expiryDateVal = coupon.expiryDate ? new Date(coupon.expiryDate) : null;
    this.error = '';
    this.showForm = true;
  }

  openCreate(): void {
    this.editing = null;
    this.form = this.emptyForm();
    this.startDateVal = new Date();
    this.expiryDateVal = null;
    this.error = '';
    this.showForm = true;
  }

  closeForm(): void {
    this.showForm = false;
    this.editing = null;
    this.error = '';
  }

  reset(): void {
    this.editing = null;
    this.form = this.emptyForm();
    this.showForm = false;
  }

  save(): void {
    this.error = this.validate();
    if (this.error) return;

    this.saving = true;
    const dto = this.toPayload();
    const request = this.editing
      ? this.couponsApi.update(this.editing.couponId, dto)
      : this.couponsApi.create(dto);

    request.subscribe({
      next: () => {
        this.saving = false;
        this.showForm = false;
        this.editing = null;
        this.form = this.emptyForm();
        this.load();
      },
      error: err => {
        this.error = err?.error?.error || 'Coupon could not be saved.';
        this.saving = false;
        this.cdr.markForCheck();
      }
    });
  }

  toggle(coupon: CouponDto): void {
    this.couponsApi.setActive(coupon.couponId, !coupon.isActive).subscribe(() => this.load());
  }

  remove(coupon: CouponDto): void {
    if (!confirm(`Delete coupon ${coupon.couponCode}?`)) return;
    this.couponsApi.delete(coupon.couponId).subscribe(() => this.load());
  }

  statFor(couponId: number): CouponUsageStatDto | undefined {
    return this.stats.find(s => s.couponId === couponId);
  }

  onStartDateChange(d: Date | null): void {
    this.startDateVal = d;
    this.form.startDate = d ? d.toISOString().slice(0, 10) : '';
  }

  onExpiryDateChange(d: Date | null): void {
    this.expiryDateVal = d;
    this.form.expiryDate = d ? d.toISOString().slice(0, 10) : null;
  }

  private emptyForm(): CouponForm {
    const now = new Date();
    return {
      couponCode: '',
      title: '',
      description: '',
      discountType: 'FixedAmount',
      discountValue: 0,
      minimumPurchaseAmount: null,
      maximumDiscountAmount: null,
      usageLimit: null,
      startDate: this.toInputDate(now.toISOString()),
      expiryDate: null,
      isActive: true,
      currencyCode: 'CAD',
    };
  }

  private validate(): string {
    if (!this.form.title.trim()) return 'Title is required.';
    if (!this.editing && !this.form.couponCode.trim()) return 'Coupon code is required.';
    if (this.form.discountValue <= 0) return 'Discount value must be greater than zero.';
    if (this.form.discountType === 'Percentage' && this.form.discountValue > 100) return 'Percentage cannot exceed 100.';
    if (this.form.expiryDate && new Date(this.form.expiryDate) <= new Date(this.form.startDate)) return 'Expiry must be after start date.';
    return '';
  }

  private toPayload(): Partial<CouponDto> {
    return {
      couponCode: this.form.couponCode.trim().toUpperCase(),
      title: this.form.title.trim(),
      description: this.form.description.trim(),
      discountType: this.form.discountType,
      discountValue: Number(this.form.discountValue),
      minimumPurchaseAmount: this.form.minimumPurchaseAmount || undefined,
      maximumDiscountAmount: this.form.maximumDiscountAmount || undefined,
      usageLimit: this.form.usageLimit || undefined,
      startDate: new Date(this.form.startDate).toISOString(),
      expiryDate: this.form.expiryDate ? new Date(this.form.expiryDate).toISOString() : undefined,
      isActive: this.form.isActive,
      currencyCode: this.form.discountType === 'FixedAmount' ? this.form.currencyCode : 'CAD',
    };
  }

  private toInputDate(value: string): string {
    return new Date(value).toISOString().slice(0, 10);
  }
}
